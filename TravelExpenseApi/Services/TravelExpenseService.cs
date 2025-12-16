using Azure;
using Azure.Data.Tables;
using TravelExpenseApi.Models;

namespace TravelExpenseApi.Services;

/// <summary>
/// 旅費申請データアクセスサービス
/// Azure Table Storageとの通信を担当
/// </summary>
public class TravelExpenseService
{
    private readonly TableClient _tableClient;
    private const string TableName = "TravelExpenses";

    public TravelExpenseService(IConfiguration configuration)
    {
        var connectionString = configuration["AzureTableStorage:ConnectionString"];
        
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Azure Table Storage connection string is not configured.");
        }

        var serviceClient = new TableServiceClient(connectionString);
        _tableClient = serviceClient.GetTableClient(TableName);
        
        // テーブルが存在しない場合は作成
        _tableClient.CreateIfNotExists();
    }

    /// <summary>
    /// 新規旅費申請を作成
    /// </summary>
    public async Task<TravelExpenseResponse> CreateExpenseAsync(TravelExpenseRequest request)
    {
        var now = DateTime.UtcNow;
        var entity = new TravelExpenseEntity
        {
            // PartitionKey: 年月でパーティション (例: "2025-11")
            PartitionKey = now.ToString("yyyy-MM"),
            // RowKey: タイムスタンプ + GUID でユニーク性を保証
            RowKey = $"{now:yyyyMMdd-HHmmss}-{Guid.NewGuid():N}",
            ApplicationDate = now,
            ApplicantName = request.ApplicantName,
            TravelDate = DateTime.SpecifyKind(request.TravelDate, DateTimeKind.Utc),
            Destination = request.Destination,
            Purpose = request.Purpose,
            Transportation = request.Transportation,
            TransportationCost = request.TransportationCost,
            AccommodationCost = request.AccommodationCost,
            MealCost = request.MealCost,
            OtherCost = request.OtherCost,
            TotalAmount = request.TransportationCost + request.AccommodationCost + 
                         request.MealCost + request.OtherCost,
            Remarks = request.Remarks,
            Status = "承認待ち"
        };

        await _tableClient.AddEntityAsync(entity);

        return MapToResponse(entity);
    }

    /// <summary>
    /// すべての旅費申請を取得
    /// </summary>
    public async Task<List<TravelExpenseResponse>> GetAllExpensesAsync()
    {
        var expenses = new List<TravelExpenseResponse>();

        await foreach (var entity in _tableClient.QueryAsync<TravelExpenseEntity>())
        {
            expenses.Add(MapToResponse(entity));
        }

        // 申請日で降順ソート
        return expenses.OrderByDescending(e => e.ApplicationDate).ToList();
    }

    /// <summary>
    /// IDで旅費申請を取得
    /// </summary>
    public async Task<TravelExpenseResponse?> GetExpenseByIdAsync(string partitionKey, string rowKey)
    {
        try
        {
            var response = await _tableClient.GetEntityAsync<TravelExpenseEntity>(partitionKey, rowKey);
            return MapToResponse(response.Value);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    /// <summary>
    /// 旅費申請を更新
    /// </summary>
    public async Task<TravelExpenseResponse?> UpdateExpenseAsync(string partitionKey, string rowKey, TravelExpenseRequest request)
    {
        try
        {
            var response = await _tableClient.GetEntityAsync<TravelExpenseEntity>(partitionKey, rowKey);
            var entity = response.Value;

            // 更新
            entity.ApplicantName = request.ApplicantName;
            entity.TravelDate = DateTime.SpecifyKind(request.TravelDate, DateTimeKind.Utc);
            entity.Destination = request.Destination;
            entity.Purpose = request.Purpose;
            entity.Transportation = request.Transportation;
            entity.TransportationCost = request.TransportationCost;
            entity.AccommodationCost = request.AccommodationCost;
            entity.MealCost = request.MealCost;
            entity.OtherCost = request.OtherCost;
            entity.TotalAmount = request.TransportationCost + request.AccommodationCost + 
                                request.MealCost + request.OtherCost;
            entity.Remarks = request.Remarks;

            await _tableClient.UpdateEntityAsync(entity, entity.ETag, TableUpdateMode.Replace);

            return MapToResponse(entity);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    /// <summary>
    /// 旅費申請のステータスを更新
    /// </summary>
    public async Task<TravelExpenseResponse?> UpdateStatusAsync(string partitionKey, string rowKey, string status)
    {
        try
        {
            var response = await _tableClient.GetEntityAsync<TravelExpenseEntity>(partitionKey, rowKey);
            var entity = response.Value;

            entity.Status = status;

            await _tableClient.UpdateEntityAsync(entity, entity.ETag, TableUpdateMode.Replace);

            return MapToResponse(entity);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    /// <summary>
    /// 不正検知結果を更新
    /// </summary>
    public async Task<TravelExpenseResponse?> UpdateFraudCheckResultAsync(
        string partitionKey, 
        string rowKey, 
        string result, 
        string details)
    {
        try
        {
            var response = await _tableClient.GetEntityAsync<TravelExpenseEntity>(partitionKey, rowKey);
            var entity = response.Value;

            entity.FraudCheckCompleted = true;
            entity.FraudCheckDate = DateTime.UtcNow;
            entity.FraudCheckResult = result;
            entity.FraudCheckDetails = details;

            await _tableClient.UpdateEntityAsync(entity, entity.ETag, TableUpdateMode.Replace);

            return MapToResponse(entity);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    /// <summary>
    /// 旅費申請を削除
    /// </summary>
    public async Task<bool> DeleteExpenseAsync(string partitionKey, string rowKey)
    {
        try
        {
            await _tableClient.DeleteEntityAsync(partitionKey, rowKey);
            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return false;
        }
    }

    /// <summary>
    /// サマリー情報を取得
    /// </summary>
    public async Task<TravelExpenseSummary> GetSummaryAsync()
    {
        var summary = new TravelExpenseSummary();

        await foreach (var entity in _tableClient.QueryAsync<TravelExpenseEntity>())
        {
            if (entity.Status == "承認待ち")
            {
                summary.PendingTotal += entity.TotalAmount;
                summary.PendingCount++;
            }
            else if (entity.Status == "承認済み")
            {
                summary.ApprovedTotal += entity.TotalAmount;
                summary.ApprovedCount++;
            }
        }

        summary.TotalCount = summary.PendingCount + summary.ApprovedCount;

        return summary;
    }

    private static TravelExpenseResponse MapToResponse(TravelExpenseEntity entity)
    {
        return new TravelExpenseResponse
        {
            Id = entity.RowKey,
            ApplicationDate = entity.ApplicationDate,
            ApplicantName = entity.ApplicantName,
            TravelDate = entity.TravelDate,
            Destination = entity.Destination,
            Purpose = entity.Purpose,
            Transportation = entity.Transportation,
            TransportationCost = entity.TransportationCost,
            AccommodationCost = entity.AccommodationCost,
            MealCost = entity.MealCost,
            OtherCost = entity.OtherCost,
            TotalAmount = entity.TotalAmount,
            Remarks = entity.Remarks,
            Status = entity.Status,
            FraudCheckCompleted = entity.FraudCheckCompleted,
            FraudCheckDate = entity.FraudCheckDate,
            FraudCheckResult = entity.FraudCheckResult,
            FraudCheckDetails = entity.FraudCheckDetails
        };
    }
}
