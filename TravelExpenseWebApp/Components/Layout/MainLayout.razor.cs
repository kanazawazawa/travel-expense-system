using Microsoft.AspNetCore.Components;
using TravelExpenseWebApp.Services;

namespace TravelExpenseWebApp.Components.Layout
{
    public partial class MainLayout : IDisposable
    {
        [Inject]
        private AzureAIAgentService? AzureAIAgentService { get; set; }
        
        [Inject]
        private AgentModeService? AgentModeService { get; set; }
        
        private bool _threadInitialized = false;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            
            // Start thread pre-creation as early as possible
            if (!_threadInitialized && AzureAIAgentService != null && AgentModeService != null)
            {
                _threadInitialized = true;
                
                // Don't await - let it run in background
                _ = Task.Run(async () =>
                {
                    try
                    {
                        Console.WriteLine("🚀 [MainLayout] Starting early thread pre-creation...");
                        var threadId = await AzureAIAgentService.CreateThreadAsync();
                        
                        if (!string.IsNullOrEmpty(threadId) && threadId != "agent-not-configured" && threadId != "thread-creation-failed")
                        {
                            AgentModeService.CurrentThreadId = threadId;
                            AgentModeService.IsAgentReady = true;
                            AgentModeService.NotifyAgentReadyChanged();
                            Console.WriteLine($"✅ [MainLayout] Thread pre-created early: {threadId}");
                        }
                        else
                        {
                            Console.WriteLine($"⚠️ [MainLayout] Thread creation returned error: {threadId}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ [MainLayout] Failed to pre-create thread: {ex.Message}");
                    }
                });
            }
        }

        public void Dispose()
        {
            // Cleanup if needed
        }
    }
}
