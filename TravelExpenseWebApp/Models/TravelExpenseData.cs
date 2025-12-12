namespace TravelExpenseWebApp.Models
{
    /// <summary>
    /// 音声から抽出した旅費データ
    /// </summary>
    public class TravelExpenseData
    {
        public string? Destination { get; set; }
        public DateTime? TravelDate { get; set; }
        public string? Purpose { get; set; }
        public string? TransportationType { get; set; }
        public int? TransportationCost { get; set; }
        public bool? HasAccommodation { get; set; }
        public int? AccommodationNights { get; set; }
        public int? AccommodationCost { get; set; }
        public int? DailyAllowance { get; set; }
        public List<OtherExpenseItem>? OtherExpenses { get; set; }
        public string? Notes { get; set; }
        public bool IsAutoFilled { get; set; } // 過去パターンから自動入力されたか
    }

    /// <summary>
    /// その他経費の項目
    /// </summary>
    public class OtherExpenseItem
    {
        public string Type { get; set; } = string.Empty;
        public int Cost { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
