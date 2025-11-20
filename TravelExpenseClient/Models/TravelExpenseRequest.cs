namespace TravelExpenseClient.Models;

/// <summary>
/// 旅費精算リクエストDTO
/// </summary>
public class TravelExpenseRequest
{
    /// <summary>申請者名</summary>
    public string ApplicantName { get; set; } = default!;
    
    /// <summary>出張日</summary>
    public DateTime TravelDate { get; set; }
    
    /// <summary>出張先</summary>
    public string Destination { get; set; } = default!;
    
    /// <summary>目的</summary>
    public string Purpose { get; set; } = default!;
    
    /// <summary>交通手段</summary>
    public string Transportation { get; set; } = default!;
    
    /// <summary>交通費 (円)</summary>
    public int TransportationCost { get; set; }
    
    /// <summary>宿泊費 (円)</summary>
    public int AccommodationCost { get; set; }
    
    /// <summary>食事代 (円)</summary>
    public int MealCost { get; set; }
    
    /// <summary>その他 (円)</summary>
    public int OtherCost { get; set; }
    
    /// <summary>備考</summary>
    public string? Remarks { get; set; }
}
