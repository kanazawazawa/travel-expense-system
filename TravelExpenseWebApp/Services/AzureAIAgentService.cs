using Azure;
using Azure.AI.Projects;
using Azure.AI.Projects.OpenAI;
using Azure.Identity;
using OpenAI;
using OpenAI.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.IO;
using TravelExpenseWebApp.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TravelExpenseWebApp.Services
{
    /// <summary>
    /// Azure AI Projects 1.2.* 以降の新しいAPI（OpenAI Response Client）を使用
    /// </summary>
    public class AzureAIAgentService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AzureAIAgentService> _logger;
        private AIProjectClient? _projectClient;
        private OpenAIResponseClient? _responseClient;
        private AgentVersion? _agentVersion;
        private string? _agentName;
        private string? _modelDeploymentName;
        private readonly string? _originalAgentName;
        private string? _projectEndpoint;
        private bool _isConfigured = false;
        private bool _isInitialized = false;
        private bool _initializationFailed = false;
        private string? _initializationError = null;
        private readonly SemaphoreSlim _initLock = new SemaphoreSlim(1, 1);
        private readonly int _maxCacheEntries = 100;
        
        private readonly ConcurrentDictionary<string, List<ChatMessage>> _threadHistoryCache = new();
        private readonly ConcurrentDictionary<string, DateTime> _lastCacheUpdateTime = new();

        public AzureAIAgentService(IConfiguration configuration, ILogger<AzureAIAgentService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            _logger.LogInformation("🔍 AzureAIAgentService constructor started");

            _projectEndpoint = configuration["AzureAIAgent:ProjectEndpoint"]
                ?? Environment.GetEnvironmentVariable("AzureAIAgent__ProjectEndpoint");

            _agentName = configuration["AzureAIAgent:AgentName"]
                ?? Environment.GetEnvironmentVariable("AzureAIAgent__AgentName");

            _modelDeploymentName = configuration["AzureAIAgent:ModelDeploymentName"]
                ?? Environment.GetEnvironmentVariable("AzureAIAgent__ModelDeploymentName");

            _logger.LogInformation("🔍 Configuration values loaded:");
            _logger.LogInformation("  - ProjectEndpoint: {Endpoint}", _projectEndpoint ?? "(null)");
            _logger.LogInformation("  - AgentName: {AgentName}", _agentName ?? "(null)");
            _logger.LogInformation("  - ModelDeploymentName: {ModelDeploymentName}", _modelDeploymentName ?? "(null)");

            _originalAgentName = _agentName;

            _isConfigured = !string.IsNullOrEmpty(_projectEndpoint) 
                && !string.IsNullOrEmpty(_agentName) 
                && !string.IsNullOrEmpty(_modelDeploymentName);

            _logger.LogInformation("🔍 IsConfigured: {IsConfigured}", _isConfigured);

            if (!_isConfigured)
            {
                _logger.LogWarning("Azure AI Agent configuration missing. Required: ProjectEndpoint, AgentName, ModelDeploymentName");
            }
            else
            {
                _logger.LogInformation("Azure AI Agent (Projects 1.2.*) configured");
                // バックグラウンドで初期化を開始（ブロックしない）
                _ = Task.Run(async () => await EnsureInitializedAsync());
            }
        }

        private async Task EnsureInitializedAsync()
        {
            if (_isInitialized || !_isConfigured || _initializationFailed)
                return;

            await _initLock.WaitAsync();
            try
            {
                if (_isInitialized || _initializationFailed)
                    return;

                _logger.LogInformation("Initializing Azure AI Projects client...");
                _logger.LogInformation("Project Endpoint: {Endpoint}", _projectEndpoint);
                _logger.LogInformation("Agent Name: {AgentName}", _agentName);
                _logger.LogInformation("Model Deployment: {ModelDeploymentName}", _modelDeploymentName);
                
                // Azure App Service環境では ManagedIdentity を優先、ローカルでは VisualStudio/AzureCli を使用
                var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
                {
                    ExcludeEnvironmentCredential = false,
                    ExcludeManagedIdentityCredential = false,
                    ExcludeSharedTokenCacheCredential = true, // Azure App Serviceでは動作しないため除外
                    ExcludeVisualStudioCredential = false,
                    ExcludeVisualStudioCodeCredential = false,
                    ExcludeAzureCliCredential = false,
                    ExcludeAzurePowerShellCredential = true,
                    ExcludeInteractiveBrowserCredential = true
                });
                
                // Initialize AIProjectClient
                _projectClient = new AIProjectClient(endpoint: new Uri(_projectEndpoint!), tokenProvider: credential);

                // Read AI_AGENT_INSTRUCTIONS.md file
                string instructions = await ReadAgentInstructionsAsync();

                // Create agent with instructions from file
                PromptAgentDefinition agentDefinition = new(model: _modelDeploymentName!)
                {
                    Instructions = instructions
                };

                // Create or update agent version
                _agentVersion = _projectClient.Agents.CreateAgentVersion(
                    agentName: _agentName!,
                    options: new(agentDefinition));

                _logger.LogInformation("✅ Agent created/updated (id: {AgentId}, name: {AgentName}, version: {Version})",
                    _agentVersion.Id, _agentVersion.Name, _agentVersion.Version);

                // Get OpenAI Response Client for the agent
                _responseClient = _projectClient.OpenAI.GetProjectResponsesClientForAgent(_agentVersion);
                
                _isInitialized = true;
                _logger.LogInformation("✅ Azure AI Projects client initialized successfully (OpenAI Response Client)");
            }
            catch (Azure.Identity.AuthenticationFailedException authEx)
            {
                _initializationFailed = true;
                _initializationError = $"認証エラー: {authEx.Message}";
                _logger.LogError(authEx, "❌ Authentication failed. Please ensure proper Azure credentials are configured.");
                _logger.LogError("Authentication error details: {Message}", authEx.Message);
                _logger.LogError("Please check: 1) Azure CLI login (az login), 2) Visual Studio account, 3) Managed Identity permissions");
            }
            catch (Exception ex)
            {
                _initializationFailed = true;
                _initializationError = $"初期化エラー: {ex.Message}";
                _logger.LogError(ex, "❌ Error initializing Azure AI Projects client");
                _logger.LogError("Error type: {Type}", ex.GetType().Name);
                _logger.LogError("Error message: {Message}", ex.Message);
                if (ex.InnerException != null)
                {
                    _logger.LogError("Inner exception: {InnerMessage}", ex.InnerException.Message);
                }
            }
            finally
            {
                _initLock.Release();
            }
        }

        private async Task<string> ReadAgentInstructionsAsync()
        {
            try
            {
                string instructionsPath = Path.Combine(AppContext.BaseDirectory, "AI_AGENT_INSTRUCTIONS.md");
                if (File.Exists(instructionsPath))
                {
                    _logger.LogInformation("Loading agent instructions from: {Path}", instructionsPath);
                    return await File.ReadAllTextAsync(instructionsPath);
                }
                else
                {
                    _logger.LogWarning("AI_AGENT_INSTRUCTIONS.md not found at: {Path}, using default instructions", instructionsPath);
                    return "You are a travel expense assistant. Extract travel information from natural language and output it in EXPENSE_UPDATE format.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading AI_AGENT_INSTRUCTIONS.md");
                return "You are a travel expense assistant.";
            }
        }

        public async Task<string> CreateThreadAsync()
        {
            // 新しいAPIではスレッドが不要なため、ダミーIDを返す
            // セッションごとにユニークなIDを生成
            await EnsureInitializedAsync();
            
            if (_initializationFailed)
            {
                _logger.LogError("Cannot create session: Initialization failed - {Error}", _initializationError);
                return $"initialization-failed: {_initializationError}";
            }

            if (!_isConfigured)
            {
                _logger.LogWarning("Agent not configured");
                return "agent-not-configured";
            }

            var sessionId = $"session-{Guid.NewGuid()}";
            _logger.LogInformation("✅ Session created: {SessionId}", sessionId);
            return sessionId;
        }

        public async Task<string> SendMessageWithImageAsync(string sessionId, string userMessage, Stream imageStream, string imageFileName)
        {
            if (!_isConfigured || sessionId == "agent-not-configured" || sessionId.StartsWith("initialization-failed"))
                return "エージェントが正しく設定されていません。";

            await EnsureInitializedAsync();
            
            if (_initializationFailed)
                return $"クライアントの初期化に失敗しました: {_initializationError}";

            if (_responseClient == null)
                return "クライアントの初期化に失敗しました。";

            var totalStopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("📤 Uploading image: {FileName}", imageFileName);

                // Note: Image upload functionality may need to be adapted based on the new API's capabilities
                // For now, we'll use text-only processing
                var messageText = !string.IsNullOrWhiteSpace(userMessage) 
                    ? $"{userMessage}\n[画像がアップロードされました: {imageFileName}]"
                    : $"この画像を分析して、出張情報（日付、場所、金額、目的など）を抽出してください。\n[画像: {imageFileName}]";

                _logger.LogWarning("⚠️ Image analysis with new API - using text fallback. Full multimodal support may require additional implementation.");

                // Use OpenAI Response Client
                OpenAIResponse response = _responseClient.CreateResponse(messageText);

                string responseText = response.GetOutputText();

                _logger.LogInformation("✅ TOTAL TIME (with image): {TotalDuration}ms", totalStopwatch.ElapsedMilliseconds);

                // Cache the message for history
                AddMessageToCache(sessionId, new ChatMessage
                {
                    Content = messageText,
                    IsUser = true,
                    Timestamp = DateTime.Now
                });

                AddMessageToCache(sessionId, new ChatMessage
                {
                    Content = responseText,
                    FormattedContent = FormatMessageContent(responseText),
                    IsUser = false,
                    Timestamp = DateTime.Now
                });

                return responseText;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message with image");
                return $"エラーが発生しました: {ex.Message}";
            }
        }

        public async Task<string> SendMessageAsync(string sessionId, string userMessage)
        {
            if (!_isConfigured || sessionId == "agent-not-configured" || sessionId.StartsWith("initialization-failed"))
                return "エージェントが正しく設定されていません。";

            await EnsureInitializedAsync();
            
            if (_initializationFailed)
                return $"クライアントの初期化に失敗しました: {_initializationError}";

            if (_responseClient == null)
                return "クライアントの初期化に失敗しました。";

            var totalStopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Sending message to agent...");

                // Use OpenAI Response Client to generate response
                OpenAIResponse response = _responseClient.CreateResponse(userMessage);

                string responseText = response.GetOutputText();

                _logger.LogInformation("✅ TOTAL TIME: {TotalDuration}ms", totalStopwatch.ElapsedMilliseconds);
                
                // Cache the message for history
                AddMessageToCache(sessionId, new ChatMessage
                {
                    Content = userMessage,
                    IsUser = true,
                    Timestamp = DateTime.Now
                });

                AddMessageToCache(sessionId, new ChatMessage
                {
                    Content = responseText,
                    FormattedContent = FormatMessageContent(responseText),
                    IsUser = false,
                    Timestamp = DateTime.Now
                });

                return responseText;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                return $"エラーが発生しました: {ex.Message}";
            }
        }

        private void AddMessageToCache(string sessionId, ChatMessage message)
        {
            if (!_threadHistoryCache.ContainsKey(sessionId))
            {
                _threadHistoryCache[sessionId] = new List<ChatMessage>();
            }

            _threadHistoryCache[sessionId].Add(message);
            _lastCacheUpdateTime[sessionId] = DateTime.UtcNow;

            // Limit cache size
            if (_threadHistoryCache.Count > _maxCacheEntries)
            {
                var oldestEntry = _lastCacheUpdateTime.OrderBy(x => x.Value).First();
                _threadHistoryCache.TryRemove(oldestEntry.Key, out _);
                _lastCacheUpdateTime.TryRemove(oldestEntry.Key, out _);
            }
        }

        public async Task<List<ChatMessage>> GetThreadHistoryAsync(string sessionId)
        {
            await Task.CompletedTask; // For async signature compatibility

            if (!_isConfigured || sessionId == "agent-not-configured")
            {
                return new List<ChatMessage>
                {
                    new ChatMessage
                    {
                        Content = "エージェントが正しく設定されていません。",
                        IsUser = false,
                        Timestamp = DateTime.Now
                    }
                };
            }

            // Return cached history for this session
            if (_threadHistoryCache.TryGetValue(sessionId, out var cachedHistory))
            {
                return new List<ChatMessage>(cachedHistory);
            }

            return new List<ChatMessage>();
        }

        private string FormatMessageContent(string content)
        {
            if (string.IsNullOrEmpty(content))
                return string.Empty;

            content = System.Text.RegularExpressions.Regex.Replace(
                content,
                @"\*\*([^*]+)\*\*",
                "<strong>$1</strong>");

            return content
                .Replace("\n\n", "<br><br>")
                .Replace("\n", "<br>")
                .Replace("?", "<br>?");
        }

        public void SetAgentName(string newAgentName)
        {
            if (string.IsNullOrWhiteSpace(newAgentName))
            {
                _logger.LogWarning("Attempted to set empty Agent Name");
                return;
            }

            _agentName = newAgentName;
            _isConfigured = !string.IsNullOrEmpty(_projectEndpoint) 
                && !string.IsNullOrEmpty(_agentName) 
                && !string.IsNullOrEmpty(_modelDeploymentName);
            _isInitialized = false;
            _initializationFailed = false;
            _initializationError = null;

            _logger.LogInformation("Agent Name updated to: {AgentName}", _agentName);
        }

        public string? GetCurrentAgentName() => _agentName;
        public string? GetOriginalAgentName() => _originalAgentName;
        public bool IsAgentNameModified() => _agentName != _originalAgentName;
        public bool IsConfigured() => _isConfigured;
        public bool IsInitializationFailed() => _initializationFailed;
        public string? GetInitializationError() => _initializationError;
        public string GetConfigurationStatus()
        {
            if (!_isConfigured)
            {
                return $"設定不足:\n" +
                       $"- ProjectEndpoint: {(_projectEndpoint != null ? "✅" : "❌")}\n" +
                       $"- AgentName: {(_agentName != null ? "✅" : "❌")}\n" +
                       $"- ModelDeploymentName: {(_modelDeploymentName != null ? "✅" : "❌")}";
            }
            if (_initializationFailed)
            {
                return $"初期化失敗:\n{_initializationError}";
            }
            if (!_isInitialized)
            {
                return "初期化中...";
            }
            return "✅ 正常に初期化済み";
        }

        public async IAsyncEnumerable<string> SendMessageStreamAsync(string sessionId, string userMessage)
        {
            var result = await SendMessageAsync(sessionId, userMessage);
            yield return result;
        }
    }
}

