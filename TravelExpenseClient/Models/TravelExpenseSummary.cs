namespace TravelExpenseClient.Models;

/// <summary>
/// サマリー情報
/// </summary>
public class TravelExpenseSummary
{
    /// <summary>総申請数</summary>
    public int TotalCount { get; set; }
    
    /// <summary>承認待ち件数</summary>
    public int PendingCount { get; set; }
    
    /// <summary>承認済み件数</summary>
    public int ApprovedCount { get; set; }
    
    /// <summary>却下件数</summary>
    public int RejectedCount { get; set; }
    
    /// <summary>総費用 (円)</summary>
    public int TotalAmount { get; set; }
}
