using Azure;
using Azure.Data.Tables;

namespace TravelExpenseApi.Models;

/// <summary>
/// 旅費申請エンティティ
/// </summary>
public class TravelExpenseEntity : ITableEntity
{
    // Table Storage required properties
    public string PartitionKey { get; set; } = default!;
    public string RowKey { get; set; } = default!;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    // Business properties
    /// <summary>申請日 (RowKey as ISO 8601 format)</summary>
    public DateTime ApplicationDate { get; set; }
    
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
    
    /// <summary>合計金額 (円)</summary>
    public int TotalAmount { get; set; }
    
    /// <summary>備考</summary>
    public string? Remarks { get; set; }
    
    /// <summary>ステータス (承認待ち, 承認済み, 申請件数)</summary>
    public string Status { get; set; } = "承認待ち";
    
    /// <summary>不正検知チェック完了フラグ</summary>
    public bool? FraudCheckCompleted { get; set; }
    
    /// <summary>不正検知チェック日時</summary>
    public DateTime? FraudCheckDate { get; set; }
    
    /// <summary>不正検知結果 (OK, WARNING, ERROR)</summary>
    public string? FraudCheckResult { get; set; }
    
    /// <summary>不正検知詳細メッセージ</summary>
    public string? FraudCheckDetails { get; set; }

    public TravelExpenseEntity()
    {
        // PartitionKey: 年月でパーティション分割 (例: "2025-11")
        // RowKey: 申請日時のユニークID (例: "20251120-123456-guid")
    }
}
