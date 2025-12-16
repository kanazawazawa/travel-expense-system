using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Identity.Web;
using TravelExpenseWebApp.Models;

namespace TravelExpenseWebApp.Services;

public class TravelExpenseApiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ITokenAcquisition _tokenAcquisition;
    private readonly string _baseUrl;
    private readonly string[] _scopes;

    public TravelExpenseApiService(
        HttpClient httpClient, 
        IConfiguration configuration,
        ITokenAcquisition tokenAcquisition)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _tokenAcquisition = tokenAcquisition;
        _baseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7115/api/TravelExpenses";
        
        // APIのスコープを設定
        var apiClientId = configuration["AzureAd:ApiClientId"] ?? "152a1a3e-27e0-4600-8b77-aa02c1e64b5a";
        _scopes = new[] 
        { 
            $"api://{apiClientId}/Expenses.Read",
            $"api://{apiClientId}/Expenses.Write"
        };
    }

    /// <summary>
    /// 認証トークンをHttpClientに設定
    /// </summary>
    private async Task SetAuthorizationHeaderAsync()
    {
        try
        {
            var token = await _tokenAcquisition.GetAccessTokenForUserAsync(_scopes);
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
        catch (Exception)
        {
            // トークン取得失敗時は認証ヘッダーをクリア
            _httpClient.DefaultRequestHeaders.Authorization = null;
            throw;
        }
    }

    public async Task<List<TravelExpenseResponse>> GetAllExpensesAsync()
    {
        await SetAuthorizationHeaderAsync();
        var response = await _httpClient.GetAsync(_baseUrl);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<TravelExpenseResponse>>() ?? new List<TravelExpenseResponse>();
    }

    public async Task<TravelExpenseResponse?> GetExpenseByIdAsync(string partitionKey, string rowKey)
    {
        await SetAuthorizationHeaderAsync();
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
        await SetAuthorizationHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync(_baseUrl, request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TravelExpenseResponse>() ?? throw new Exception("Failed to create expense");
    }

    public async Task<TravelExpenseResponse> UpdateExpenseAsync(string partitionKey, string rowKey, TravelExpenseRequest request)
    {
        await SetAuthorizationHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}/{partitionKey}/{rowKey}", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TravelExpenseResponse>() ?? throw new Exception("Failed to update expense");
    }

    public async Task<bool> DeleteExpenseAsync(string partitionKey, string rowKey)
    {
        await SetAuthorizationHeaderAsync();
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
        await SetAuthorizationHeaderAsync();
        var response = await _httpClient.GetAsync($"{_baseUrl}/summary");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TravelExpenseSummary>() ?? new TravelExpenseSummary();
    }

    public async Task<TravelExpenseResponse> RunFraudCheckAsync(string partitionKey, string rowKey)
    {
        await SetAuthorizationHeaderAsync();
        var response = await _httpClient.PostAsync($"{_baseUrl}/{partitionKey}/{rowKey}/fraud-check", null);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TravelExpenseResponse>() ?? throw new Exception("Failed to run fraud check");
    }
}
