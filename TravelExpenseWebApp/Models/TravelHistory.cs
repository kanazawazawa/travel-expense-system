namespace TravelExpenseWebApp.Models
{
    /// <summary>
    /// ユーザーの出張履歴（デモ用）
    /// 本番環境ではデータベースから取得
    /// </summary>
    public class TravelHistory
    {
        public string UserId { get; set; } = string.Empty;
        public List<TravelRecord> TravelRecords { get; set; } = new();
        public List<FrequentDestination> FrequentDestinations { get; set; } = new();
        public string Note { get; set; } = string.Empty;
    }

    /// <summary>
    /// 出張記録
    /// </summary>
    public class TravelRecord
    {
        public string Id { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Destination { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public TransportationInfo Transportation { get; set; } = new();
        public AccommodationInfo? Accommodation { get; set; }
        public int DailyAllowance { get; set; }
        public List<OtherExpense> OtherExpenses { get; set; } = new();
        public int TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// 交通情報
    /// </summary>
    public class TransportationInfo
    {
        public string Type { get; set; } = string.Empty;
        public int Cost { get; set; }
        public int Distance { get; set; }
    }

    /// <summary>
    /// 宿泊情報
    /// </summary>
    public class AccommodationInfo
    {
        public int Nights { get; set; }
        public int CostPerNight { get; set; }
        public int TotalCost { get; set; }
    }

    /// <summary>
    /// その他経費
    /// </summary>
    public class OtherExpense
    {
        public string Type { get; set; } = string.Empty;
        public int Cost { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// 頻繁に訪問する出張先
    /// </summary>
    public class FrequentDestination
    {
        public string Destination { get; set; } = string.Empty;
        public int Frequency { get; set; }
        public int AverageTransportationCost { get; set; }
        public string CommonTransportation { get; set; } = string.Empty;
        public int? CommonAccommodationCost { get; set; }
        public string CommonPurpose { get; set; } = string.Empty;
    }
}
