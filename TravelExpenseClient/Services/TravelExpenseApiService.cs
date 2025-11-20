using System.Net.Http.Json;
using TravelExpenseClient.Models;

namespace TravelExpenseClient.Services;

/// <summary>
/// 旅費精算APIサービス
/// </summary>
public class TravelExpenseApiService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://app-20251120-api.azurewebsites.net/api/TravelExpenses";

    public TravelExpenseApiService()
    {
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// すべての旅費精算を取得
    /// </summary>
    public async Task<List<TravelExpenseResponse>> GetAllExpensesAsync()
    {
        var response = await _httpClient.GetAsync(BaseUrl);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<TravelExpenseResponse>>() ?? new List<TravelExpenseResponse>();
    }

    /// <summary>
    /// IDで旅費精算を取得
    /// </summary>
    public async Task<TravelExpenseResponse?> GetExpenseByIdAsync(string partitionKey, string rowKey)
    {
        var response = await _httpClient.GetAsync($"{BaseUrl}/{partitionKey}/{rowKey}");
        
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TravelExpenseResponse>();
    }

    /// <summary>
    /// 新規旅費精算を作成
    /// </summary>
    public async Task<TravelExpenseResponse> CreateExpenseAsync(TravelExpenseRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync(BaseUrl, request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TravelExpenseResponse>() ?? throw new Exception("Failed to create expense");
    }

    /// <summary>
    /// 旅費精算を更新
    /// </summary>
    public async Task<TravelExpenseResponse> UpdateExpenseAsync(string partitionKey, string rowKey, TravelExpenseRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{partitionKey}/{rowKey}", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TravelExpenseResponse>() ?? throw new Exception("Failed to update expense");
    }

    /// <summary>
    /// ステータスを更新
    /// </summary>
    public async Task<TravelExpenseResponse> UpdateStatusAsync(string partitionKey, string rowKey, string status)
    {
        var response = await _httpClient.PatchAsJsonAsync($"{BaseUrl}/{partitionKey}/{rowKey}/status", status);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TravelExpenseResponse>() ?? throw new Exception("Failed to update status");
    }

    /// <summary>
    /// 旅費精算を削除
    /// </summary>
    public async Task<bool> DeleteExpenseAsync(string partitionKey, string rowKey)
    {
        var response = await _httpClient.DeleteAsync($"{BaseUrl}/{partitionKey}/{rowKey}");
        
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
        
        response.EnsureSuccessStatusCode();
        return true;
    }

    /// <summary>
    /// サマリーを取得
    /// </summary>
    public async Task<TravelExpenseSummary> GetSummaryAsync()
    {
        var response = await _httpClient.GetAsync($"{BaseUrl}/summary");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TravelExpenseSummary>() ?? new TravelExpenseSummary();
    }
}
