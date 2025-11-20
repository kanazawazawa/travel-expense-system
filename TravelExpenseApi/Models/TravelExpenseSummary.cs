namespace TravelExpenseApi.Models;

/// <summary>
/// 旅費申請サマリー
/// </summary>
public class TravelExpenseSummary
{
    /// <summary>承認待ちの合計金額</summary>
    public int PendingTotal { get; set; }
    
    /// <summary>承認待ちの件数</summary>
    public int PendingCount { get; set; }
    
    /// <summary>承認済みの合計金額</summary>
    public int ApprovedTotal { get; set; }
    
    /// <summary>承認済みの件数</summary>
    public int ApprovedCount { get; set; }
    
    /// <summary>申請件数の合計</summary>
    public int TotalCount { get; set; }
}
