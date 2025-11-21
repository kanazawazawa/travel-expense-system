using Azure;
using Azure.AI.Projects;
using Azure.Identity;
using Azure.AI.Agents.Persistent;
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
    /// Azure AI Projects 1.1.0 �����h�L�������g������
    /// GetPersistentAgentsClient() �g�p
    /// </summary>
    public class AzureAIAgentService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AzureAIAgentService> _logger;
        private AIProjectClient? _projectClient;
        private PersistentAgentsClient? _agentsClient;
        private string? _agentId;
        private readonly string? _originalAgentId;
        private string? _projectEndpoint;
        private bool _isConfigured = false;
        private bool _isInitialized = false;
        private readonly object _initLock = new object();
        private readonly int _maxCacheEntries = 100;
        
        private readonly ConcurrentDictionary<string, List<ChatMessage>> _threadHistoryCache = new();
        private readonly ConcurrentDictionary<string, DateTime> _lastCacheUpdateTime = new();

        public AzureAIAgentService(IConfiguration configuration, ILogger<AzureAIAgentService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            _projectEndpoint = configuration["AzureAIAgent:ProjectEndpoint"]
                ?? Environment.GetEnvironmentVariable("AzureAIAgent__ProjectEndpoint");

            _agentId = configuration["AzureAIAgent:AgentId"]
                ?? Environment.GetEnvironmentVariable("AzureAIAgent__AgentId");

            _originalAgentId = _agentId;

            _isConfigured = !string.IsNullOrEmpty(_projectEndpoint) && !string.IsNullOrEmpty(_agentId);

            if (!_isConfigured)
            {
                _logger.LogWarning("Azure AI Agent configuration missing");
            }
            else
            {
                _logger.LogInformation("Azure AI Agent (Projects 1.1.0) initialized");
                EnsureInitialized();
            }
        }

        private void EnsureInitialized()
        {
            if (_isInitialized || !_isConfigured)
                return;

            lock (_initLock)
            {
                if (_isInitialized)
                    return;

                try
                {
                    var credential = new DefaultAzureCredential();
                    _projectClient = new AIProjectClient(new Uri(_projectEndpoint!), credential);
                    _agentsClient = _projectClient.GetPersistentAgentsClient();
                    _isInitialized = true;
                    _logger.LogInformation("? Azure AI Projects client initialized (GetPersistentAgentsClient)");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error initializing Azure AI Projects client");
                }
            }
        }

        public async Task<string> CreateThreadAsync()
        {
            if (!_isConfigured || _agentsClient == null)
            {
                _logger.LogWarning("Attempted to create thread with unconfigured service");
                return "agent-not-configured";
            }

            EnsureInitialized();

            try
            {
                var thread = _agentsClient.Threads.CreateThread();
                var threadId = thread.Value.Id;
                _logger.LogInformation("? Thread created: {ThreadId}", threadId);
                return threadId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating thread");
                return "thread-creation-failed";
            }
        }

        public async Task<string> SendMessageWithImageAsync(string threadId, string userMessage, Stream imageStream, string imageFileName)
        {
            if (!_isConfigured || threadId == "agent-not-configured")
                return "�G�[�W�F���g���������ݒ肳��Ă��܂���B";

            EnsureInitialized();

            if (_agentsClient == null || _agentId == null)
                return "�N���C�A���g�̏������Ɏ��s���܂����B";

            var totalStopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                // 1. Upload file with purpose=Agents (Vision enum��beta SDK�Ŗ��T�|�[�g)
                _logger.LogInformation("?? Uploading image: {FileName}", imageFileName);
                var uploadedFile = await _agentsClient.Files.UploadFileAsync(
                    imageStream,
                    PersistentAgentFilePurpose.Agents,  // ? Agents purpose for multimodal
                    imageFileName);

                var fileId = uploadedFile.Value.Id;
                _logger.LogInformation("? Image uploaded (purpose=Agents): {FileId}", fileId);

                // 2. Create message with multimodal content (text + image)
                var messageText = !string.IsNullOrWhiteSpace(userMessage) 
                    ? userMessage
                    : "���̉摜�𕪐͂��āA�o�����i���t�A�ꏊ�A���z�A�ړI�Ȃǁj�𒊏o���Ă��������B";

                // ? SDK 1.2.0-beta.7: Use MessageInputContentBlock for multimodal
                var contentBlocks = new List<MessageInputContentBlock>
                {
                    new MessageInputTextBlock(messageText),
                    new MessageInputImageFileBlock(new MessageImageFileParam(fileId))
                };

                var message = await _agentsClient.Messages.CreateMessageAsync(
                    threadId,
                    MessageRole.User,
                    contentBlocks);

                _logger.LogInformation("? Message created with image: {MessageId}, FileId: {FileId}", message.Value.Id, fileId);

                // 3. Run agent
                var run = _agentsClient.Runs.CreateRun(threadId, _agentId);

                // 4. Poll for completion
                int pollCount = 0;
                do
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(pollCount < 5 ? 100 : 300));
                    run = _agentsClient.Runs.GetRun(threadId, run.Value.Id);
                    pollCount++;
                }
                while (run.Value.Status == RunStatus.Queued || run.Value.Status == RunStatus.InProgress);

                _logger.LogInformation("?? Polling ({PollCount} polls): {Status}", pollCount, run.Value.Status);

                if (run.Value.Status != RunStatus.Completed)
                {
                    _logger.LogWarning("Run failed: {Error}", run.Value.LastError?.Message);
                    return $"�G�[�W�F���g�̎��s�Ɏ��s���܂���: {run.Value.LastError?.Message}";
                }

                // 5. Get messages
                var messages = _agentsClient.Messages.GetMessages(threadId, order: ListSortOrder.Ascending);
                var latestAssistantMessage = messages
                    .Where(m => m.Role.ToString().Equals("Assistant", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(m => m.CreatedAt)
                    .FirstOrDefault();

                if (latestAssistantMessage != null)
                {
                    string responseText = "";
                    foreach (var contentItem in latestAssistantMessage.ContentItems)
                    {
                        if (contentItem is MessageTextContent textContent)
                        {
                            responseText += textContent.Text;
                        }
                    }

                    _logger.LogInformation("? TOTAL TIME (with image): {TotalDuration}ms", totalStopwatch.ElapsedMilliseconds);
                    return responseText;
                }

                return "����������܂���ł����B";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message with image");
                return $"�G���[���������܂���: {ex.Message}";
            }
        }

        public async Task<string> SendMessageAsync(string threadId, string userMessage)
        {
            if (!_isConfigured || threadId == "agent-not-configured")
                return "�G�[�W�F���g���������ݒ肳��Ă��܂���B";

            EnsureInitialized();

            if (_agentsClient == null || _agentId == null)
                return "�N���C�A���g�̏������Ɏ��s���܂����B";

            var totalStopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                // 1. Create message
                var message = _agentsClient.Messages.CreateMessage(
                    threadId,
                    MessageRole.User,
                    userMessage);

                // 2. Run agent
                var run = _agentsClient.Runs.CreateRun(threadId, _agentId);

                // 3. Poll for completion
                int pollCount = 0;
                do
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(pollCount < 5 ? 100 : 300));
                    run = _agentsClient.Runs.GetRun(threadId, run.Value.Id);
                    pollCount++;
                }
                while (run.Value.Status == RunStatus.Queued || run.Value.Status == RunStatus.InProgress);

                _logger.LogInformation("?? Polling ({PollCount} polls)", pollCount);

                if (run.Value.Status != RunStatus.Completed)
                {
                    _logger.LogWarning("Run failed: {Error}", run.Value.LastError?.Message);
                    return $"�G�[�W�F���g�̎��s�Ɏ��s���܂���: {run.Value.LastError?.Message}";
                }

                // 4. Get messages
                var messages = _agentsClient.Messages.GetMessages(threadId, order: ListSortOrder.Ascending);
                var latestAssistantMessage = messages
                    .Where(m => m.Role.ToString().Equals("Assistant", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(m => m.CreatedAt)
                    .FirstOrDefault();

                if (latestAssistantMessage != null)
                {
                    string responseText = "";
                    foreach (var contentItem in latestAssistantMessage.ContentItems)
                    {
                        if (contentItem is MessageTextContent textContent)
                        {
                            responseText += textContent.Text;
                        }
                    }

                    _logger.LogInformation("? TOTAL TIME: {TotalDuration}ms", totalStopwatch.ElapsedMilliseconds);
                    return responseText;
                }

                return "����������܂���ł����B";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                return $"�G���[���������܂���: {ex.Message}";
            }
        }

        public async Task<List<ChatMessage>> GetThreadHistoryAsync(string threadId)
        {
            if (!_isConfigured || threadId == "agent-not-configured" || _agentsClient == null)
            {
                return new List<ChatMessage>
                {
                    new ChatMessage
                    {
                        Content = "�G�[�W�F���g���������ݒ肳��Ă��܂���B",
                        IsUser = false,
                        Timestamp = DateTime.Now
                    }
                };
            }

            if (_threadHistoryCache.TryGetValue(threadId, out var cachedHistory) &&
                _lastCacheUpdateTime.TryGetValue(threadId, out var lastUpdate) &&
                (DateTime.UtcNow - lastUpdate).TotalSeconds < 5)
            {
                return new List<ChatMessage>(cachedHistory);
            }

            var chatHistory = new List<ChatMessage>();

            try
            {
                var messages = _agentsClient.Messages.GetMessages(threadId);

                foreach (var message in messages.OrderBy(m => m.CreatedAt))
                {
                    string messageContent = "";
                    foreach (var contentItem in message.ContentItems)
                    {
                        if (contentItem is MessageTextContent textContent)
                        {
                            messageContent += textContent.Text ?? string.Empty;
                        }
                    }

                    string formattedContent = !message.Role.ToString().Equals("User", StringComparison.OrdinalIgnoreCase) ?
                        FormatMessageContent(messageContent) : "";

                    chatHistory.Add(new ChatMessage
                    {
                        Content = messageContent,
                        FormattedContent = formattedContent,
                        IsUser = message.Role.ToString().Equals("User", StringComparison.OrdinalIgnoreCase),
                        Timestamp = message.CreatedAt.DateTime
                    });
                }

                if (_threadHistoryCache.Count >= _maxCacheEntries)
                {
                    var oldestEntries = _lastCacheUpdateTime
                        .OrderBy(x => x.Value)
                        .Take(_threadHistoryCache.Count - _maxCacheEntries + 1)
                        .ToList();

                    foreach (var entry in oldestEntries)
                    {
                        _threadHistoryCache.TryRemove(entry.Key, out _);
                        _lastCacheUpdateTime.TryRemove(entry.Key, out _);
                    }
                }

                _threadHistoryCache.AddOrUpdate(threadId, new List<ChatMessage>(chatHistory),
                    (key, oldValue) => new List<ChatMessage>(chatHistory));
                _lastCacheUpdateTime.AddOrUpdate(threadId, DateTime.UtcNow,
                    (key, oldValue) => DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving thread history");
                return cachedHistory ?? new List<ChatMessage>();
            }

            return chatHistory;
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

        public void SetAgentId(string newAgentId)
        {
            if (string.IsNullOrWhiteSpace(newAgentId))
            {
                _logger.LogWarning("Attempted to set empty Agent ID");
                return;
            }

            _agentId = newAgentId;
            _isConfigured = !string.IsNullOrEmpty(_projectEndpoint) && !string.IsNullOrEmpty(_agentId);
            _isInitialized = false;

            _logger.LogInformation("Agent ID updated");
        }

        public string? GetCurrentAgentId() => _agentId;
        public string? GetOriginalAgentId() => _originalAgentId;
        public bool IsAgentIdModified() => _agentId != _originalAgentId;
        public bool IsConfigured() => _isConfigured;

        public async IAsyncEnumerable<string> SendMessageStreamAsync(string threadId, string userMessage)
        {
            var result = await SendMessageAsync(threadId, userMessage);
            yield return result;
        }
    }
}

