using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TravelExpenseWebApp.Models;

namespace TravelExpenseWebApp.Services
{
    /// <summary>
    /// Azure OpenAI Realtime API (WebSocket) を使用した音声対話サービス
    /// </summary>
    public class RealtimeAudioService : IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<RealtimeAudioService> _logger;
        private ClientWebSocket? _webSocket;
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _isConnected = false;
        private string? _deploymentName;
        private string? _endpoint;
        private string? _apiKey;

        public event Action<string>? OnTranscriptReceived;
        public event Action<byte[]>? OnAudioReceived;
        public event Action<string>? OnError;
        public event Action? OnConnected;
        public event Action? OnDisconnected;
        public event Action<TravelExpenseData>? OnExpenseDataExtracted; // 新規: 構造化データ抽出

        public bool IsConnected => _isConnected;

        public RealtimeAudioService(IConfiguration configuration, ILogger<RealtimeAudioService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            _endpoint = configuration["AzureOpenAI:Endpoint"] 
                ?? Environment.GetEnvironmentVariable("AzureOpenAI__Endpoint");
            _deploymentName = configuration["AzureOpenAI:RealtimeDeploymentName"] 
                ?? Environment.GetEnvironmentVariable("AzureOpenAI__RealtimeDeploymentName");
            _apiKey = configuration["AzureOpenAI:ApiKey"] 
                ?? Environment.GetEnvironmentVariable("AzureOpenAI__ApiKey");

            _logger.LogInformation("RealtimeAudioService initialized");
            _logger.LogInformation("Endpoint: {Endpoint}", _endpoint ?? "(not configured)");
            _logger.LogInformation("DeploymentName: {DeploymentName}", _deploymentName ?? "(not configured)");
        }

        /// <summary>
        /// Realtime API WebSocketセッションに接続
        /// </summary>
        public async Task<bool> ConnectAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(_endpoint) || string.IsNullOrEmpty(_deploymentName))
                {
                    _logger.LogError("Azure OpenAI Realtime API configuration is missing");
                    OnError?.Invoke("Realtime API の設定が不足しています");
                    return false;
                }

                _webSocket = new ClientWebSocket();
                _cancellationTokenSource = new CancellationTokenSource();

                // Azure OpenAI Realtime API WebSocket URL
                // Format: wss://{endpoint}/openai/realtime?api-version=2025-04-01-preview&deployment={deployment}
                var uriBuilder = new UriBuilder(_endpoint);
                uriBuilder.Scheme = "wss";
                uriBuilder.Path = $"/openai/realtime";
                uriBuilder.Query = $"api-version=2025-04-01-preview&deployment={_deploymentName}";

                var wsUri = uriBuilder.Uri;
                _logger.LogInformation("Connecting to Realtime API: {Uri}", wsUri);

                // Add API key header if available
                if (!string.IsNullOrEmpty(_apiKey))
                {
                    _webSocket.Options.SetRequestHeader("api-key", _apiKey);
                }
                else
                {
                    // Use DefaultAzureCredential for authentication
                    var credential = new DefaultAzureCredential();
                    var token = await credential.GetTokenAsync(
                        new Azure.Core.TokenRequestContext(new[] { "https://cognitiveservices.azure.com/.default" }));
                    _webSocket.Options.SetRequestHeader("Authorization", $"Bearer {token.Token}");
                }

                await _webSocket.ConnectAsync(wsUri, _cancellationTokenSource.Token);
                _isConnected = true;
                _logger.LogInformation("✅ Connected to Realtime API");
                OnConnected?.Invoke();

                // セッション設定を送信
                await ConfigureSessionAsync();

                // メッセージ受信ループを開始
                _ = Task.Run(() => ReceiveMessagesAsync(_cancellationTokenSource.Token));

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to Realtime API");
                OnError?.Invoke($"接続エラー: {ex.Message}");
                _isConnected = false;
                return false;
            }
        }

        /// <summary>
        /// セッション設定を送信（音声アクティビティ検出、トランスクリプションなど）
        /// </summary>
        private async Task ConfigureSessionAsync()
        {
            try
            {
                // Load user profile
                var userProfile = await LoadUserProfileAsync();
                
                // Load travel history
                var travelHistory = await LoadTravelHistoryAsync(userProfile.UserId);
                
                // Load instructions from external file
                string instructions = await LoadVoiceAgentInstructionsAsync();
                
                // Build context-aware instructions with user profile and travel history
                string contextualInstructions = BuildContextualInstructions(instructions, userProfile, travelHistory);

                var sessionConfig = new
                {
                    type = "session.update",
                    session = new
                    {
                        voice = "alloy",
                        instructions = contextualInstructions,
                        input_audio_format = "pcm16",
                        output_audio_format = "pcm16",
                        input_audio_transcription = new
                        {
                            model = "whisper-1"
                        },
                        turn_detection = new
                        {
                            type = "server_vad",
                            threshold = 0.5,
                            prefix_padding_ms = 300,
                            silence_duration_ms = 700,  // 適度な長さに調整
                            create_response = true
                        },
                        // 音声応答の設定
                        modalities = new[] { "text", "audio" },
                        temperature = 0.6, // 低めに設定してレスポンスを速く、一貫性を高める
                        max_response_output_tokens = 150, // 応答を短く制限（約30-40単語、日本語で15-20文）
                        // Function Calling の設定
                        tools = new[]
                        {
                            new
                            {
                                type = "function",
                                name = "update_expense_form",
                                description = "旅費申請フォームに情報を反映します。情報が確定したら即座に呼び出してください。",
                                parameters = new
                                {
                                    type = "object",
                                    properties = new
                                    {
                                        destination = new { type = "string", description = "出張先（都市名）" },
                                        travelDate = new { type = "string", description = "出張日（ISO 8601形式: YYYY-MM-DD）" },
                                        purpose = new { type = "string", description = "出張目的" },
                                        transportationType = new { type = "string", description = "交通手段（例: 新幹線、飛行機）" },
                                        transportationCost = new { type = "number", description = "交通費（円）" },
                                        hasAccommodation = new { type = "boolean", description = "宿泊の有無" },
                                        accommodationNights = new { type = "number", description = "宿泊泊数" },
                                        accommodationCost = new { type = "number", description = "宿泊費（1泊あたり、円）" },
                                        dailyAllowance = new { type = "number", description = "日当（円）" },
                                        notes = new { type = "string", description = "備考" },
                                        isAutoFilled = new { type = "boolean", description = "過去パターンから自動入力されたか" }
                                    }
                                }
                            }
                        }
                    }
                };

                var json = JsonSerializer.Serialize(sessionConfig);
                var bytes = Encoding.UTF8.GetBytes(json);
                await _webSocket!.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                
                _logger.LogInformation("Session configuration sent with user context: {UserName} ({Position}), {HistoryCount} past trips", 
                    userProfile.DisplayName, userProfile.Position, travelHistory.TravelRecords.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to configure session");
            }
        }

        /// <summary>
        /// 音声エージェントのインストラクションをファイルから読み込む
        /// </summary>
        private async Task<string> LoadVoiceAgentInstructionsAsync()
        {
            try
            {
                string instructionsPath = Path.Combine(AppContext.BaseDirectory, "VOICE_AGENT_INSTRUCTIONS.md");
                if (File.Exists(instructionsPath))
                {
                    _logger.LogInformation("Loading voice agent instructions from: {Path}", instructionsPath);
                    var content = await File.ReadAllTextAsync(instructionsPath);
                    
                    // Remove markdown headers and formatting for cleaner prompt
                    content = System.Text.RegularExpressions.Regex.Replace(content, @"^#.*$", "", System.Text.RegularExpressions.RegexOptions.Multiline);
                    content = content.Trim();
                    
                    return content;
                }
                else
                {
                    _logger.LogWarning("VOICE_AGENT_INSTRUCTIONS.md not found at: {Path}, using default instructions", instructionsPath);
                    return GetDefaultInstructions();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading VOICE_AGENT_INSTRUCTIONS.md");
                return GetDefaultInstructions();
            }
        }

        /// <summary>
        /// デフォルトのインストラクション（ファイルが見つからない場合のフォールバック）
        /// </summary>
        private string GetDefaultInstructions()
        {
            return @"あなたは旅費申請をサポートするアシスタントです。
ユーザーの出張に関する情報を聞き取り、出張先、日付、交通費、宿泊費などの情報を抽出してください。
日本語で会話し、丁寧で親しみやすい対応を心がけてください。
簡潔に答えてください。";
        }

        /// <summary>
        /// ユーザープロファイルを読み込む（デモ用）
        /// 本番環境では Microsoft Entra ID から取得
        /// </summary>
        private async Task<UserProfile> LoadUserProfileAsync()
        {
            try
            {
                string profilePath = Path.Combine(AppContext.BaseDirectory, "USER_PROFILE_DEMO.json");
                if (File.Exists(profilePath))
                {
                    _logger.LogInformation("Loading user profile from: {Path}", profilePath);
                    var json = await File.ReadAllTextAsync(profilePath);
                    var profile = JsonSerializer.Deserialize<UserProfile>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    if (profile != null)
                    {
                        _logger.LogInformation("User profile loaded: {UserName} ({Position})", 
                            profile.DisplayName, profile.Position);
                        return profile;
                    }
                }
                else
                {
                    _logger.LogWarning("USER_PROFILE_DEMO.json not found at: {Path}, using default profile", profilePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading USER_PROFILE_DEMO.json");
            }

            // Return default profile
            return new UserProfile
            {
                DisplayName = "ゲストユーザー",
                Position = "一般社員",
                Department = "未設定",
                TravelExpenseSettings = new TravelExpenseSettings
                {
                    DailyAllowance = 2000,
                    AccommodationLimit = 9000,
                    CanUseGreenCar = false,
                    CanUseBusinessClass = false,
                    ApprovalRequired = true
                }
            };
        }

        /// <summary>
        /// ユーザープロファイルに基づいて文脈を追加したインストラクションを構築
        /// </summary>
        private string BuildContextualInstructions(string baseInstructions, UserProfile userProfile, TravelHistory travelHistory)
        {
            var contextBuilder = new StringBuilder();
            
            // 現在の日本時間を追加
            var japanTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
            var nowInJapan = TimeZoneInfo.ConvertTime(DateTime.UtcNow, japanTimeZone);
            
            contextBuilder.AppendLine("## 現在の日時情報");
            contextBuilder.AppendLine();
            contextBuilder.AppendLine($"**現在の日本時間**: {nowInJapan:yyyy年M月d日(ddd) HH:mm} JST");
            contextBuilder.AppendLine($"**今日**: {nowInJapan:yyyy-MM-dd} ({nowInJapan:dddd})");
            contextBuilder.AppendLine($"**明日**: {nowInJapan.AddDays(1):yyyy-MM-dd} ({nowInJapan.AddDays(1):dddd})");
            contextBuilder.AppendLine($"**明後日**: {nowInJapan.AddDays(2):yyyy-MM-dd} ({nowInJapan.AddDays(2):dddd})");
            contextBuilder.AppendLine($"**来週の月曜日**: {GetNextMonday(nowInJapan):yyyy-MM-dd}");
            contextBuilder.AppendLine();
            contextBuilder.AppendLine("### 日付の計算ルール");
            contextBuilder.AppendLine("- 「今日」「本日」→ 上記の「今日」の日付を使用");
            contextBuilder.AppendLine("- 「明日」→ 上記の「明日」の日付を使用");
            contextBuilder.AppendLine("- 「明後日」→ 上記の「明後日」の日付を使用");
            contextBuilder.AppendLine("- 「来週」「来週の〇曜日」→ 次の該当曜日を計算");
            contextBuilder.AppendLine("- 「3日後」「5日後」→ 今日から指定日数後を計算");
            contextBuilder.AppendLine("- 「〇月〇日」→ 年が省略されている場合は今年を使用");
            contextBuilder.AppendLine("- **重要**: すべての日付は ISO 8601 形式 (YYYY-MM-DD) で `update_expense_form` に渡してください");
            contextBuilder.AppendLine();
            contextBuilder.AppendLine("---");
            contextBuilder.AppendLine();
            
            // ユーザー情報のコンテキスト追加
            contextBuilder.AppendLine("## 現在のユーザー情報");
            contextBuilder.AppendLine();
            contextBuilder.AppendLine($"**氏名**: {userProfile.DisplayName}");
            contextBuilder.AppendLine($"**部署**: {userProfile.Department}");
            contextBuilder.AppendLine($"**役職**: {userProfile.Position}");
            contextBuilder.AppendLine();
            contextBuilder.AppendLine("### このユーザーに適用される旅費規程");
            contextBuilder.AppendLine();
            contextBuilder.AppendLine($"- **日当**: {userProfile.TravelExpenseSettings.DailyAllowance:N0}円/日");
            contextBuilder.AppendLine($"- **宿泊費上限**: {userProfile.TravelExpenseSettings.AccommodationLimit:N0}円/泊");
            contextBuilder.AppendLine($"- **新幹線グリーン車**: {(userProfile.TravelExpenseSettings.CanUseGreenCar ? "利用可" : "利用不可")}");
            contextBuilder.AppendLine($"- **航空機ビジネスクラス**: {(userProfile.TravelExpenseSettings.CanUseBusinessClass ? "利用可" : "利用不可")}");
            contextBuilder.AppendLine($"- **事前承認**: {(userProfile.TravelExpenseSettings.ApprovalRequired ? "必要" : "不要")}");
            contextBuilder.AppendLine();
            
            // 過去の出張履歴のコンテキスト追加
            if (travelHistory.FrequentDestinations.Any())
            {
                contextBuilder.AppendLine("### 過去の出張パターン（よく訪れる出張先）");
                contextBuilder.AppendLine();
                contextBuilder.AppendLine("このユーザーは以下の出張先によく訪れています。類似の出張先が入力された場合、過去のパターンを参考に提案してください。");
                contextBuilder.AppendLine();
                
                foreach (var dest in travelHistory.FrequentDestinations.Take(5))
                {
                    contextBuilder.AppendLine($"**{dest.Destination}** (過去{dest.Frequency}回)");
                    contextBuilder.AppendLine($"  - 主な目的: {dest.CommonPurpose}");
                    contextBuilder.AppendLine($"  - よく使う交通手段: {dest.CommonTransportation}（平均: {dest.AverageTransportationCost:N0}円）");
                    if (dest.CommonAccommodationCost.HasValue)
                    {
                        contextBuilder.AppendLine($"  - 宿泊: あり（平均: {dest.CommonAccommodationCost.Value:N0}円/泊）");
                    }
                    else
                    {
                        contextBuilder.AppendLine($"  - 宿泊: 通常なし（日帰り）");
                    }
                    contextBuilder.AppendLine();
                }
            }
            
            // 最近の出張履歴
            if (travelHistory.TravelRecords.Any())
            {
                contextBuilder.AppendLine("### 最近の出張履歴（参考）");
                contextBuilder.AppendLine();
                contextBuilder.AppendLine("最近の出張から類似パターンを見つけて提案してください。");
                contextBuilder.AppendLine();
                
                foreach (var record in travelHistory.TravelRecords.Take(3))
                {
                    contextBuilder.AppendLine($"- **{record.Date:yyyy年M月d日}**: {record.Destination} ({record.Purpose})");
                    contextBuilder.AppendLine($"  - 交通: {record.Transportation.Type} {record.Transportation.Cost:N0}円");
                    if (record.Accommodation != null)
                    {
                        contextBuilder.AppendLine($"  - 宿泊: {record.Accommodation.Nights}泊 {record.Accommodation.TotalCost:N0}円");
                    }
                    contextBuilder.AppendLine();
                }
            }
            
            contextBuilder.AppendLine("---");
            contextBuilder.AppendLine();
            
            // 基本インストラクションを追加
            contextBuilder.AppendLine(baseInstructions);
            contextBuilder.AppendLine();
            
            // 重要な注意事項
            contextBuilder.AppendLine("---");
            contextBuilder.AppendLine();
            contextBuilder.AppendLine("## スマートアシスタント機能");
            contextBuilder.AppendLine();
            contextBuilder.AppendLine("### 過去パターンに基づく自動提案");
            contextBuilder.AppendLine();
            contextBuilder.AppendLine("ユーザーが出張先を入力したら、以下のように対応してください：");
            contextBuilder.AppendLine();
            contextBuilder.AppendLine("1. **頻繁に訪れる出張先の場合**:");
            contextBuilder.AppendLine("   - 過去のパターン（交通手段、宿泊の有無、費用）を参考に提案");
            contextBuilder.AppendLine("   - 例: 「大阪への出張ですね。いつもと同じ新幹線指定席でよろしいですか？前回は13,620円でした。」");
            contextBuilder.AppendLine();
            contextBuilder.AppendLine("2. **類似の出張先の場合**:");
            contextBuilder.AppendLine("   - 距離や都市の規模から類似パターンを提案");
            contextBuilder.AppendLine("   - 例: 「名古屋への出張ですね。同じくらいの距離の大阪では通常新幹線を使われていますが、今回もそうされますか？」");
            contextBuilder.AppendLine();
            contextBuilder.AppendLine("3. **新しい出張先の場合**:");
            contextBuilder.AppendLine("   - 基本的な質問から始める");
            contextBuilder.AppendLine("   - ただし、過去の傾向（日帰りが多い、宿泊が多いなど）を考慮");
            contextBuilder.AppendLine();
            contextBuilder.AppendLine("### 重要事項");
            contextBuilder.AppendLine();
            contextBuilder.AppendLine($"- ユーザーは既に認証されており、氏名は「{userProfile.DisplayName}」、役職は「{userProfile.Position}」です。");
            contextBuilder.AppendLine("- 役職の確認は不要です。上記の情報を基に適切な規程を適用してください。");
            contextBuilder.AppendLine("- ユーザーに役職を尋ねないでください。既に把握しています。");
            contextBuilder.AppendLine("- 過去のパターンを活用して、効率的に情報収集してください。");
            contextBuilder.AppendLine("- ただし、過去のパターンを押し付けず、ユーザーの意向を最優先してください。");
            contextBuilder.AppendLine("- **日付計算は上記の「現在の日時情報」を必ず参照してください**。");
            
            return contextBuilder.ToString();
        }

        /// <summary>
        /// 次の月曜日の日付を取得
        /// </summary>
        private static DateTime GetNextMonday(DateTime fromDate)
        {
            int daysUntilMonday = ((int)DayOfWeek.Monday - (int)fromDate.DayOfWeek + 7) % 7;
            if (daysUntilMonday == 0) daysUntilMonday = 7; // 今日が月曜日の場合は来週の月曜日
            return fromDate.AddDays(daysUntilMonday);
        }

        /// <summary>
        /// 出張履歴を読み込む（デモ用）
        /// 本番環境ではデータベースから取得
        /// </summary>
        private async Task<TravelHistory> LoadTravelHistoryAsync(string userId)
        {
            try
            {
                string historyPath = Path.Combine(AppContext.BaseDirectory, "TRAVEL_HISTORY_DEMO.json");
                if (File.Exists(historyPath))
                {
                    _logger.LogInformation("Loading travel history from: {Path}", historyPath);
                    var json = await File.ReadAllTextAsync(historyPath);
                    
                    var jsonOptions = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    
                    var rawData = JsonSerializer.Deserialize<JsonElement>(json, jsonOptions);
                    
                    var history = new TravelHistory
                    {
                        UserId = rawData.GetProperty("userId").GetString() ?? string.Empty,
                        TravelRecords = new List<TravelRecord>(),
                        FrequentDestinations = new List<FrequentDestination>()
                    };
                    
                    // Parse travel history
                    if (rawData.TryGetProperty("travelHistory", out var historyArray))
                    {
                        foreach (var item in historyArray.EnumerateArray())
                        {
                            var record = new TravelRecord
                            {
                                Id = item.GetProperty("id").GetString() ?? string.Empty,
                                Date = DateTime.Parse(item.GetProperty("date").GetString() ?? DateTime.Now.ToString()),
                                Destination = item.GetProperty("destination").GetString() ?? string.Empty,
                                Purpose = item.GetProperty("purpose").GetString() ?? string.Empty,
                                DailyAllowance = item.GetProperty("dailyAllowance").GetInt32(),
                                TotalAmount = item.GetProperty("totalAmount").GetInt32(),
                                Status = item.GetProperty("status").GetString() ?? string.Empty
                            };
                            
                            // Parse transportation
                            if (item.TryGetProperty("transportation", out var trans))
                            {
                                record.Transportation = new TransportationInfo
                                {
                                    Type = trans.GetProperty("type").GetString() ?? string.Empty,
                                    Cost = trans.GetProperty("cost").GetInt32(),
                                    Distance = trans.GetProperty("distance").GetInt32()
                                };
                            }
                            
                            // Parse accommodation
                            if (item.TryGetProperty("accommodation", out var accom) && accom.ValueKind != JsonValueKind.Null)
                            {
                                record.Accommodation = new AccommodationInfo
                                {
                                    Nights = accom.GetProperty("nights").GetInt32(),
                                    CostPerNight = accom.GetProperty("costPerNight").GetInt32(),
                                    TotalCost = accom.GetProperty("totalCost").GetInt32()
                                };
                            }
                            
                            history.TravelRecords.Add(record);
                        }
                    }
                    
                    // Parse frequent destinations
                    if (rawData.TryGetProperty("frequentDestinations", out var freqArray))
                    {
                        foreach (var item in freqArray.EnumerateArray())
                        {
                            var dest = new FrequentDestination
                            {
                                Destination = item.GetProperty("destination").GetString() ?? string.Empty,
                                Frequency = item.GetProperty("frequency").GetInt32(),
                                AverageTransportationCost = item.GetProperty("averageTransportationCost").GetInt32(),
                                CommonTransportation = item.GetProperty("commonTransportation").GetString() ?? string.Empty,
                                CommonPurpose = item.GetProperty("commonPurpose").GetString() ?? string.Empty
                            };
                            
                            if (item.TryGetProperty("commonAccommodationCost", out var cost) && cost.ValueKind != JsonValueKind.Null)
                            {
                                dest.CommonAccommodationCost = cost.GetInt32();
                            }
                            
                            history.FrequentDestinations.Add(dest);
                        }
                    }
                    
                    _logger.LogInformation("Travel history loaded: {RecordCount} records, {DestCount} frequent destinations", 
                        history.TravelRecords.Count, history.FrequentDestinations.Count);
                    return history;
                }
                else
                {
                    _logger.LogWarning("TRAVEL_HISTORY_DEMO.json not found at: {Path}, using empty history", historyPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading TRAVEL_HISTORY_DEMO.json");
            }

            // Return empty history
            return new TravelHistory
            {
                UserId = userId,
                TravelRecords = new List<TravelRecord>(),
                FrequentDestinations = new List<FrequentDestination>()
            };
        }

        /// <summary>
        /// Function Call の結果をサーバーに送信（AIに応答を続けさせる）
        /// </summary>
        private async Task SendFunctionCallResultAsync(string callId)
        {
            if (!_isConnected || _webSocket == null)
            {
                return;
            }

            try
            {
                var functionResult = new
                {
                    type = "conversation.item.create",
                    item = new
                    {
                        type = "function_call_output",
                        call_id = callId,
                        output = "{\"status\": \"success\", \"message\": \"フォームに反映しました\"}"
                    }
                };

                var json = JsonSerializer.Serialize(functionResult);
                var bytes = Encoding.UTF8.GetBytes(json);
                await _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                
                // 応答生成をリクエスト
                await CreateResponseAsync();
                
                _logger.LogDebug("Function call result sent and response requested");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send function call result");
            }
        }

        /// <summary>
        /// オーディオデータを送信
        /// </summary>
        public async Task SendAudioAsync(byte[] audioData)
        {
            if (!_isConnected || _webSocket == null)
            {
                _logger.LogWarning("Cannot send audio: not connected");
                return;
            }

            if (audioData == null || audioData.Length == 0)
            {
                _logger.LogWarning("Empty audio data, skipping");
                return;
            }

            try
            {
                _logger.LogDebug($"📤 Sending {audioData.Length} bytes of audio");

                // Base64エンコード
                var base64Audio = Convert.ToBase64String(audioData);

                var audioMessage = new
                {
                    type = "input_audio_buffer.append",
                    audio = base64Audio
                };

                var json = JsonSerializer.Serialize(audioMessage);
                var bytes = Encoding.UTF8.GetBytes(json);
                await _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                
                _logger.LogDebug("✅ Audio sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send audio");
                OnError?.Invoke($"音声送信エラー: {ex.Message}");
            }
        }

        /// <summary>
        /// 応答生成をリクエスト（手動モード用）
        /// </summary>
        public async Task CreateResponseAsync()
        {
            if (!_isConnected || _webSocket == null)
            {
                _logger.LogWarning("Cannot create response: not connected");
                return;
            }

            try
            {
                var responseRequest = new
                {
                    type = "response.create"
                };

                var json = JsonSerializer.Serialize(responseRequest);
                var bytes = Encoding.UTF8.GetBytes(json);
                await _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                
                _logger.LogInformation("Response creation requested");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create response");
            }
        }

        /// <summary>
        /// 進行中の応答をキャンセル（会話の割り込み用）
        /// </summary>
        public async Task CancelResponseAsync()
        {
            if (!_isConnected || _webSocket == null)
            {
                return; // エラーログを出さずに静かに終了
            }

            try
            {
                var cancelRequest = new
                {
                    type = "response.cancel"
                };

                var json = JsonSerializer.Serialize(cancelRequest);
                var bytes = Encoding.UTF8.GetBytes(json);
                await _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                
                _logger.LogDebug("Response cancellation requested"); // Debug レベルに変更
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Error cancelling response (可能性: 既にキャンセル済み)"); // Debug レベルに変更
            }
        }

        /// <summary>
        /// WebSocketからのメッセージ受信ループ
        /// </summary>
        private async Task ReceiveMessagesAsync(CancellationToken cancellationToken)
        {
            var buffer = new byte[1024 * 64]; // 64KB buffer
            var messageBuilder = new StringBuilder();

            try
            {
                while (_isConnected && !cancellationToken.IsCancellationRequested)
                {
                    var result = await _webSocket!.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _logger.LogInformation("WebSocket connection closed by server");
                        await DisconnectAsync();
                        break;
                    }

                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    messageBuilder.Append(message);

                    if (result.EndOfMessage)
                    {
                        var completeMessage = messageBuilder.ToString();
                        messageBuilder.Clear();
                        
                        await ProcessServerEventAsync(completeMessage);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Receive loop cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in receive loop");
                OnError?.Invoke($"受信エラー: {ex.Message}");
                await DisconnectAsync();
            }
        }

        /// <summary>
        /// サーバーから受信したイベントを処理
        /// </summary>
        private async Task ProcessServerEventAsync(string eventJson)
        {
            try
            {
                using var doc = JsonDocument.Parse(eventJson);
                var root = doc.RootElement;

                if (!root.TryGetProperty("type", out var typeElement))
                    return;

                var eventType = typeElement.GetString();
                _logger.LogDebug("Received event: {EventType}", eventType);

                switch (eventType)
                {
                    case "session.created":
                    case "session.updated":
                        _logger.LogInformation("Session event: {EventType}", eventType);
                        break;

                    case "conversation.item.audio_transcription.completed":
                        if (root.TryGetProperty("transcript", out var transcript))
                        {
                            var text = transcript.GetString();
                            _logger.LogInformation("User transcript: {Text}", text);
                            OnTranscriptReceived?.Invoke(text ?? string.Empty);
                        }
                        break;

                    case "response.audio.delta":
                        if (root.TryGetProperty("delta", out var delta))
                        {
                            var base64Audio = delta.GetString();
                            if (!string.IsNullOrEmpty(base64Audio))
                            {
                                var audioBytes = Convert.FromBase64String(base64Audio);
                                OnAudioReceived?.Invoke(audioBytes);
                            }
                        }
                        break;

                    case "response.audio_transcript.delta":
                        if (root.TryGetProperty("delta", out var textDelta))
                        {
                            var text = textDelta.GetString();
                            _logger.LogDebug("AI transcript delta: {Text}", text);
                        }
                        break;

                    case "response.function_call_arguments.done":
                        // Function Call の引数が完了
                        if (root.TryGetProperty("name", out var funcName) && funcName.GetString() == "update_expense_form")
                        {
                            if (root.TryGetProperty("arguments", out var argsJson))
                            {
                                var argsString = argsJson.GetString();
                                if (!string.IsNullOrEmpty(argsString))
                                {
                                    _logger.LogInformation("Function call received: update_expense_form with args: {Args}", argsString);
                                    var expenseData = JsonSerializer.Deserialize<TravelExpenseData>(argsString, new JsonSerializerOptions
                                    {
                                        PropertyNameCaseInsensitive = true
                                    });
                                    
                                    if (expenseData != null)
                                    {
                                        OnExpenseDataExtracted?.Invoke(expenseData);
                                    }
                                    
                                    // Function Call 完了を通知して、AI に応答を生成させる
                                    await SendFunctionCallResultAsync(root.GetProperty("call_id").GetString() ?? "unknown");
                                }
                            }
                        }
                        break;

                    case "error":
                        if (root.TryGetProperty("error", out var errorObj))
                        {
                            var errorMessage = errorObj.GetProperty("message").GetString();
                            
                            // "Cancellation failed" エラーは無視（正常な動作）
                            if (errorMessage?.Contains("Cancellation failed", StringComparison.OrdinalIgnoreCase) == true)
                            {
                                _logger.LogDebug("Server info: {Error} (これは正常です)", errorMessage);
                                return; // エラーイベントを発火しない
                            }
                            
                            _logger.LogError("Server error: {Error}", errorMessage);
                            OnError?.Invoke(errorMessage ?? "Unknown error");
                        }
                        break;

                    case "response.done":
                        _logger.LogInformation("Response completed");
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process server event: {Json}", eventJson);
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// WebSocket接続を切断
        /// </summary>
        public async Task DisconnectAsync()
        {
            if (!_isConnected)
                return;

            try
            {
                _isConnected = false;
                _cancellationTokenSource?.Cancel();

                if (_webSocket?.State == WebSocketState.Open)
                {
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                }

                _logger.LogInformation("Disconnected from Realtime API");
                OnDisconnected?.Invoke();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during disconnect");
            }
            finally
            {
                _webSocket?.Dispose();
                _webSocket = null;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        public void Dispose()
        {
            _ = DisconnectAsync();
        }
    }
}
