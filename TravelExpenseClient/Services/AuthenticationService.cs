using Microsoft.Identity.Client;

namespace TravelExpenseClient.Services;

/// <summary>
/// Azure AD認証サービス
/// </summary>
public class AuthenticationService
{
    private readonly IPublicClientApplication _app;
    private readonly string[] _scopes;

    // Azure ADアプリ登録情報
    private const string ClientId = "d613203e-6233-42a5-a02a-b02ee73b70f2";
    private const string TenantId = "f5581409-bf53-4ac0-8cf8-2ba3fe7b5684";
    private const string ApiClientId = "152a1a3e-27e0-4600-8b77-aa02c1e64b5a";

    public AuthenticationService()
    {
        // APIへのアクセススコープ
        _scopes = new[]
        {
            $"api://{ApiClientId}/Expenses.Read",
            $"api://{ApiClientId}/Expenses.Write"
        };

        // MSALクライアントアプリケーションの構築
        _app = PublicClientApplicationBuilder
            .Create(ClientId)
            .WithAuthority($"https://login.microsoftonline.com/{TenantId}")
            .WithRedirectUri("http://localhost")
            .Build();
    }

    /// <summary>
    /// アクセストークンを取得
    /// </summary>
    public async Task<string?> GetAccessTokenAsync()
    {
        try
        {
            // まずキャッシュから取得を試みる（サイレント認証）
            var accounts = await _app.GetAccountsAsync();
            var firstAccount = accounts.FirstOrDefault();

            if (firstAccount != null)
            {
                try
                {
                    var result = await _app.AcquireTokenSilent(_scopes, firstAccount)
                        .ExecuteAsync();
                    return result.AccessToken;
                }
                catch (MsalUiRequiredException)
                {
                    // サイレント認証失敗 - 対話的認証が必要
                }
            }

            // 対話的認証（ブラウザを開いてログイン）
            var interactiveResult = await _app.AcquireTokenInteractive(_scopes)
                .WithPrompt(Prompt.SelectAccount)
                .ExecuteAsync();

            return interactiveResult.AccessToken;
        }
        catch (MsalException ex)
        {
            throw new Exception($"認証エラー: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 現在のユーザーアカウントを取得
    /// </summary>
    public async Task<IAccount?> GetCurrentAccountAsync()
    {
        var accounts = await _app.GetAccountsAsync();
        return accounts.FirstOrDefault();
    }

    /// <summary>
    /// ログアウト
    /// </summary>
    public async Task SignOutAsync()
    {
        var accounts = await _app.GetAccountsAsync();
        foreach (var account in accounts)
        {
            await _app.RemoveAsync(account);
        }
    }

    /// <summary>
    /// 認証済みかどうかを確認
    /// </summary>
    public async Task<bool> IsAuthenticatedAsync()
    {
        var accounts = await _app.GetAccountsAsync();
        return accounts.Any();
    }
}
