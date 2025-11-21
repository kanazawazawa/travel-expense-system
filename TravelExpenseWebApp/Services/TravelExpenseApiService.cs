using System.Net.Http.Headers;
using System.Net.Http.Json;
using TravelExpenseWebApp.Models;

namespace TravelExpenseWebApp.Services;

public class TravelExpenseApiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _baseUrl;

    public TravelExpenseApiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _baseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7115/api/TravelExpenses";
    }

    /// <summary>
    /// 認証トークンを設定（Web版では不要だが、将来的に必要になる可能性あり）
    /// </summary>
    public void SetAuthorizationHeader(string? token)
    {
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<List<TravelExpenseResponse>> GetAllExpensesAsync()
    {
        var response = await _httpClient.GetAsync(_baseUrl);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<TravelExpenseResponse>>() ?? new List<TravelExpenseResponse>();
    }

    public async Task<TravelExpenseResponse?> GetExpenseByIdAsync(string partitionKey, string rowKey)
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/{partitionKey}/{rowKey}");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TravelExpenseResponse>();
    }

    public async Task<TravelExpenseResponse> CreateExpenseAsync(TravelExpenseRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync(_baseUrl, request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TravelExpenseResponse>() ?? throw new Exception("Failed to create expense");
    }

    public async Task<TravelExpenseResponse> UpdateExpenseAsync(string partitionKey, string rowKey, TravelExpenseRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}/{partitionKey}/{rowKey}", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TravelExpenseResponse>() ?? throw new Exception("Failed to update expense");
    }

    public async Task<bool> DeleteExpenseAsync(string partitionKey, string rowKey)
    {
        var response = await _httpClient.DeleteAsync($"{_baseUrl}/{partitionKey}/{rowKey}");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }

        response.EnsureSuccessStatusCode();
        return true;
    }

    public async Task<TravelExpenseSummary> GetSummaryAsync()
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/summary");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TravelExpenseSummary>() ?? new TravelExpenseSummary();
    }
}
