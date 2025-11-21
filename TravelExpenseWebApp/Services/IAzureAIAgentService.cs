using TravelExpenseWebApp.Models;

namespace TravelExpenseWebApp.Services
{
    /// <summary>
    /// Azure AI Agent Service interface for dependency injection
    /// </summary>
    public interface IAzureAIAgentService
    {
        Task<string> CreateThreadAsync();
        Task<string> SendMessageAsync(string threadId, string userMessage);
        IAsyncEnumerable<string> SendMessageStreamAsync(string threadId, string userMessage);
        Task<List<ChatMessage>> GetThreadHistoryAsync(string threadId);
        void SetAgentId(string newAgentId);
        string? GetCurrentAgentId();
        string? GetOriginalAgentId();
        bool IsAgentIdModified();
        bool IsConfigured();
    }
}
