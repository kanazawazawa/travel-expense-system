using System;

namespace TravelExpenseWebApp.Services
{
    /// <summary>
    /// AI Agentから旅費精算UIへの更新指示を伝達するサービス
    /// </summary>
    public class TravelExpenseUIUpdateService
    {
        /// <summary>
        /// UI更新イベント
        /// </summary>
        public event Action<TravelExpenseUIUpdateInstruction>? TravelExpenseUIUpdateRequested;
        
        /// <summary>
        /// UI更新を要求
        /// </summary>
        public void RequestTravelExpenseUIUpdate(TravelExpenseUIUpdateInstruction instruction)
        {
            TravelExpenseUIUpdateRequested?.Invoke(instruction);
        }
    }
    
    /// <summary>
    /// 旅費精算UI更新指示
    /// </summary>
    public class TravelExpenseUIUpdateInstruction
    {
        public string? ApplicantName { get; set; }
        public DateTime? TravelDate { get; set; }
        public string? Destination { get; set; }
        public string? Purpose { get; set; }
        public decimal? TransportationCost { get; set; }
        public decimal? AccommodationCost { get; set; }
        public decimal? MealCost { get; set; }
        public decimal? OtherCost { get; set; }
        public string? Notes { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
