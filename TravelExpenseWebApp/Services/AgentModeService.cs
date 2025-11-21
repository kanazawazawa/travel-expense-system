using System;

namespace TravelExpenseWebApp.Services
{
    /// <summary>
    /// Service to maintain the state of AI Agent across the application.
    /// </summary>
    public class AgentModeService
    {
        /// <summary>
        /// Gets or sets the current AI Agent thread ID for maintaining conversation state.
        /// </summary>
        public string? CurrentThreadId { get; set; }
        
        // Always using Azure AI Agent Mode, so this is now always true
        public bool UseAzureAIAgent { get; } = true;
        
        /// <summary>
        /// Indicates whether the agent is ready to accept messages
        /// </summary>
        public bool IsAgentReady { get; set; } = false;
        
        /// <summary>
        /// Event raised when agent readiness changes
        /// </summary>
        public event Action? OnAgentReadyChanged;
        
        /// <summary>
        /// Notify listeners that agent readiness has changed
        /// </summary>
        public void NotifyAgentReadyChanged()
        {
            OnAgentReadyChanged?.Invoke();
        }
    }
}
