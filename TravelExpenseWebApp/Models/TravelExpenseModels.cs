namespace TravelExpenseWebApp.Models;

public class TravelExpenseRequest
{
    public string ApplicantName { get; set; } = string.Empty;
    public DateTime TravelDate { get; set; }
    public string Destination { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public string Transportation { get; set; } = string.Empty;
    public int TransportationCost { get; set; }
    public int AccommodationCost { get; set; }
    public int MealCost { get; set; }
    public int OtherCost { get; set; }
    public string? Remarks { get; set; }
}

public class TravelExpenseResponse
{
    public string Id { get; set; } = string.Empty;
    public DateTime ApplicationDate { get; set; }
    public string ApplicantName { get; set; } = string.Empty;
    public DateTime TravelDate { get; set; }
    public string Destination { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public string Transportation { get; set; } = string.Empty;
    public int TransportationCost { get; set; }
    public int AccommodationCost { get; set; }
    public int MealCost { get; set; }
    public int OtherCost { get; set; }
    public int TotalAmount { get; set; }
    public string Status { get; set; } = "申請中";
    public string? Remarks { get; set; }
    
    // 不正検知関連プロパティ
    public bool? FraudCheckCompleted { get; set; }
    public DateTime? FraudCheckDate { get; set; }
    public string? FraudCheckResult { get; set; }
    public string? FraudCheckDetails { get; set; }
}

public class TravelExpenseSummary
{
    public int TotalCount { get; set; }
    public int PendingCount { get; set; }
    public int ApprovedCount { get; set; }
    public int RejectedCount { get; set; }
    public int TotalAmount { get; set; }
}
