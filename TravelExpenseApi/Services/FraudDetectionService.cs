using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using TravelExpenseApi.Models;

namespace TravelExpenseApi.Services;

/// <summary>
/// Azure AI Foundry Agentを使用した不正検知サービス
/// </summary>
public class FraudDetectionService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<FraudDetectionService> _logger;
    private readonly TravelExpenseService _travelExpenseService;
    private AIProjectClient? _aiProjectClient;
    private AIAgent? _agent;
    private readonly string? _projectEndpoint;
    private readonly string? _deploymentName;
    private readonly string? _agentName;
    private bool _isInitialized = false;
    private bool _initializationFailed = false;
    private string? _initializationError = null;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    public FraudDetectionService(
        IConfiguration configuration, 
        ILogger<FraudDetectionService> logger,
        TravelExpenseService travelExpenseService)
    {
        _configuration = configuration;
        _logger = logger;
        _travelExpenseService = travelExpenseService;
        
        _projectEndpoint = configuration["AzureAIAgent:ProjectEndpoint"];
        _deploymentName = configuration["AzureAIAgent:ModelDeploymentName"];
        _agentName = configuration["AzureAIAgent:AgentName"];
    }

    private async Task EnsureInitializedAsync()
    {
        if (_isInitialized) return;
        if (_initializationFailed)
        {
            _logger.LogWarning("Skipping initialization - previous attempt failed: {Error}", _initializationError);
            return;
        }

        await _initLock.WaitAsync();
        try
        {
            if (_isInitialized) return;
            if (_initializationFailed) return;

            if (string.IsNullOrEmpty(_projectEndpoint) || string.IsNullOrEmpty(_agentName))
            {
                _logger.LogWarning("Azure AI Agent configuration is missing (ProjectEndpoint or AgentName)");
                return;
            }

            _logger.LogInformation("Initializing Fraud Detection Agent: {AgentName}", _agentName);
            
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

            _aiProjectClient = new AIProjectClient(new Uri(_projectEndpoint), credential);

            _logger.LogInformation("Attempting to retrieve Agent: {AgentName} from endpoint: {Endpoint}", _agentName, _projectEndpoint);

            // Foundryで作成済みのAgentを名前で取得
            try
            {
                _agent = await _aiProjectClient.GetAIAgentAsync(_agentName!);
                _isInitialized = true;
                _logger.LogInformation("✅ Fraud Detection Agent retrieved successfully: {AgentName}", _agentName);
            }
            catch (Exception agentEx)
            {
                _logger.LogError(agentEx, "❌ Failed to retrieve Agent '{AgentName}'. Make sure the agent exists in Foundry with this exact name.", _agentName);
                _initializationFailed = true;
                _initializationError = $"Failed to retrieve Agent: {agentEx.Message}";
                throw;
            }
        }
        catch (Azure.RequestFailedException requestEx)
        {
            _initializationFailed = true;
            _initializationError = $"Azure Request Failed: {requestEx.Status} - {requestEx.ErrorCode}";
            _logger.LogError(requestEx, "❌ Failed to initialize Fraud Detection Agent - Azure Request Failed. Status: {Status}, ErrorCode: {ErrorCode}", 
                requestEx.Status, requestEx.ErrorCode);
            _logger.LogError("Agent Name: {AgentName}, Endpoint: {Endpoint}", _agentName, _projectEndpoint);
        }
        catch (Exception ex)
        {
            _initializationFailed = true;
            _initializationError = $"Unexpected Error: {ex.Message}";
            _logger.LogError(ex, "❌ Failed to initialize Fraud Detection Agent - Unexpected Error");
            _logger.LogError("Agent Name: {AgentName}, Endpoint: {Endpoint}", _agentName, _projectEndpoint);
        }
        finally
        {
            _initLock.Release();
        }
    }

    /// <summary>
    /// 単一の旅費申請に対して不正検知を実行
    /// </summary>
    public async Task<FraudCheckResult> CheckExpenseAsync(string partitionKey, string rowKey)
    {
        await EnsureInitializedAsync();

        if (_agent == null)
        {
            var errorMessage = _initializationFailed && !string.IsNullOrEmpty(_initializationError)
                ? $"AI Agentが初期化されていません: {_initializationError}"
                : "AI Agentが初期化されていません";
            
            return new FraudCheckResult
            {
                Result = "ERROR",
                Details = errorMessage
            };
        }

        try
        {
            // 対象の申請を取得
            var targetExpense = await _travelExpenseService.GetExpenseByIdAsync(partitionKey, rowKey);
            if (targetExpense == null)
            {
                return new FraudCheckResult
                {
                    Result = "ERROR",
                    Details = "対象の申請が見つかりません"
                };
            }

            // 前後1ヶ月の申請を取得して分析用コンテキストを作成
            var allExpenses = await _travelExpenseService.GetAllExpensesAsync();
            var relatedExpenses = allExpenses
                .Where(e => e.ApplicantName == targetExpense.ApplicantName &&
                           Math.Abs((e.TravelDate - targetExpense.TravelDate).TotalDays) <= 30 &&
                           e.Id != targetExpense.Id)
                .OrderBy(e => e.TravelDate)
                .ToList();

            // AI Agentに送信するプロンプトを作成
            var prompt = BuildFraudCheckPrompt(targetExpense, relatedExpenses);

            _logger.LogInformation("Sending fraud check request to AI Agent for expense ID: {Id}", targetExpense.Id);

            // AI Agentを実行
            var response = await _agent.RunAsync(prompt);
            var resultText = response.ToString();

            _logger.LogInformation("Fraud check completed for expense ID: {Id}", targetExpense.Id);

            // レスポンスをパース
            return ParseAgentResponse(resultText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during fraud check");
            return new FraudCheckResult
            {
                Result = "ERROR",
                Details = $"不正検知中にエラーが発生しました: {ex.Message}"
            };
        }
    }

    private string BuildFraudCheckPrompt(TravelExpenseResponse target, List<TravelExpenseResponse> related)
    {
        var prompt = $@"以下の旅費申請について不正検知を行ってください：

【チェック対象の申請】
- 申請者: {target.ApplicantName}
- 出張日: {target.TravelDate:yyyy/MM/dd}
- 出張先: {target.Destination}
- 目的: {target.Purpose}
- 交通手段: {target.Transportation}
- 交通費: {target.TransportationCost:N0}円
- 宿泊費: {target.AccommodationCost:N0}円
- 食事代: {target.MealCost:N0}円
- その他: {target.OtherCost:N0}円
- 合計金額: {target.TotalAmount:N0}円
- 備考: {target.Remarks}

【前後1ヶ月の関連申請】
";

        if (related.Count > 0)
        {
            foreach (var expense in related)
            {
                prompt += $@"
- {expense.TravelDate:yyyy/MM/dd} {expense.Destination} ({expense.Transportation}) {expense.TotalAmount:N0}円";
            }
        }
        else
        {
            prompt += "なし（近い日付の他の申請はありません）";
        }

        prompt += @"

上記の情報を元に、不正や異常がないかチェックしてください。";

        return prompt;
    }

    private FraudCheckResult ParseAgentResponse(string response)
    {
        // レスポンスから結果とdetailsを抽出
        var result = "OK";
        var details = response;

        // 簡易的なパース（"ERROR"、"WARNING"、"OK"のキーワードを検索）
        if (response.Contains("ERROR", StringComparison.OrdinalIgnoreCase) ||
            response.Contains("エラー", StringComparison.OrdinalIgnoreCase) ||
            response.Contains("不正", StringComparison.OrdinalIgnoreCase) ||
            response.Contains("重複", StringComparison.OrdinalIgnoreCase))
        {
            result = "ERROR";
        }
        else if (response.Contains("WARNING", StringComparison.OrdinalIgnoreCase) ||
                 response.Contains("警告", StringComparison.OrdinalIgnoreCase) ||
                 response.Contains("注意", StringComparison.OrdinalIgnoreCase) ||
                 response.Contains("懸念", StringComparison.OrdinalIgnoreCase))
        {
            result = "WARNING";
        }

        return new FraudCheckResult
        {
            Result = result,
            Details = details
        };
    }
}

/// <summary>
/// 不正検知結果
/// </summary>
public class FraudCheckResult
{
    public string Result { get; set; } = "OK";
    public string Details { get; set; } = string.Empty;
}
