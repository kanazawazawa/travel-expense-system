# 認証実装計画

## フェーズ1: Azure AD アプリケーション登録

### 1. API用アプリの登録
```
- 名前: TravelExpenseApi
- リダイレクトURI: なし
- スコープの公開:
  - api://travel-expense-api/Expenses.Read
  - api://travel-expense-api/Expenses.Write
```

### 2. クライアント用アプリの登録
```
- 名前: TravelExpenseClient
- プラットフォーム: モバイルとデスクトップ
- リダイレクトURI: http://localhost
- APIアクセス許可:
  - TravelExpenseApi (上記のスコープ)
```

## フェーズ2: API側の実装

### 必要なパッケージ
```bash
cd TravelExpenseApi
dotnet add package Microsoft.Identity.Web
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

### Program.cs の変更
```csharp
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthorization();

// ...

app.UseAuthentication();
app.UseAuthorization();
```

### appsettings.json の追加
```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "YOUR_TENANT_ID",
    "ClientId": "YOUR_API_CLIENT_ID",
    "Audience": "api://YOUR_API_CLIENT_ID"
  }
}
```

### Controller に [Authorize] を追加
```csharp
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TravelExpensesController : ControllerBase
{
    // ユーザー情報の取得
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var userName = User.Identity?.Name;
}
```

## フェーズ3: クライアント側の実装

### 必要なパッケージ
```bash
cd TravelExpenseClient
dotnet add package Microsoft.Identity.Client
dotnet add package Microsoft.Identity.Client.Desktop
```

### 認証サービスの作成
```csharp
// Services/AuthenticationService.cs
public class AuthenticationService
{
    private readonly IPublicClientApplication _app;
    private readonly string[] _scopes = { "api://YOUR_API_CLIENT_ID/Expenses.Read", 
                                          "api://YOUR_API_CLIENT_ID/Expenses.Write" };

    public AuthenticationService()
    {
        _app = PublicClientApplicationBuilder
            .Create("YOUR_CLIENT_APP_ID")
            .WithAuthority("https://login.microsoftonline.com/YOUR_TENANT_ID")
            .WithRedirectUri("http://localhost")
            .Build();
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        try
        {
            // キャッシュから取得を試みる
            var accounts = await _app.GetAccountsAsync();
            var result = await _app.AcquireTokenSilent(_scopes, accounts.FirstOrDefault())
                .ExecuteAsync();
            return result.AccessToken;
        }
        catch (MsalUiRequiredException)
        {
            // ブラウザを開いてログイン
            var result = await _app.AcquireTokenInteractive(_scopes)
                .WithPrompt(Prompt.SelectAccount)
                .ExecuteAsync();
            return result.AccessToken;
        }
    }
}
```

### HttpClient にトークンを追加
```csharp
// Services/TravelExpenseApiService.cs
private readonly AuthenticationService _authService;
private readonly HttpClient _httpClient;

public TravelExpenseApiService(AuthenticationService authService)
{
    _authService = authService;
    _httpClient = new HttpClient();
}

public async Task<List<TravelExpenseResponse>> GetAllExpensesAsync()
{
    var token = await _authService.GetAccessTokenAsync();
    _httpClient.DefaultRequestHeaders.Authorization = 
        new AuthenticationHeaderValue("Bearer", token);
    
    var response = await _httpClient.GetAsync(BaseUrl);
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadFromJsonAsync<List<TravelExpenseResponse>>() 
        ?? new List<TravelExpenseResponse>();
}
```

### ログインフォームの追加
```csharp
// LoginForm.cs
public partial class LoginForm : Form
{
    private readonly AuthenticationService _authService;

    public LoginForm(AuthenticationService authService)
    {
        InitializeComponent();
        _authService = authService;
    }

    private async void ButtonLogin_Click(object sender, EventArgs e)
    {
        try
        {
            var token = await _authService.GetAccessTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                MessageBox.Show("ログイン成功！", "成功", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"ログインに失敗しました: {ex.Message}", "エラー",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
```

### Program.cs でログインチェック
```csharp
// Program.cs
Application.EnableVisualStyles();
Application.SetCompatibleTextRenderingDefault(false);

var authService = new AuthenticationService();

// ログインフォームを表示
var loginForm = new LoginForm(authService);
if (loginForm.ShowDialog() == DialogResult.OK)
{
    // 認証成功 - メインフォームを表示
    Application.Run(new Form1(authService));
}
else
{
    // 認証失敗 - アプリケーション終了
    MessageBox.Show("認証が必要です。", "エラー", 
        MessageBoxButtons.OK, MessageBoxIcon.Warning);
}
```

## フェーズ4: セキュリティ強化

### CORS の設定を厳格化
```csharp
// Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAuthenticatedClients", policy =>
    {
        policy.WithOrigins("http://localhost") // デスクトップアプリ用
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

app.UseCors("AllowAuthenticatedClients");
```

### マルチテナント対応（オプション）
```csharp
// ユーザーごとにデータを分離
public async Task<List<TravelExpenseResponse>> GetAllExpensesAsync()
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    
    // PartitionKeyにユーザーIDを使用
    var expenses = new List<TravelExpenseResponse>();
    await foreach (var entity in _tableClient.QueryAsync<TravelExpenseEntity>(
        filter: $"PartitionKey eq '{userId}'"))
    {
        expenses.Add(MapToResponse(entity));
    }
    return expenses;
}
```

## フェーズ5: テスト

### テストシナリオ
1. ✅ 未認証でAPI呼び出し → 401 Unauthorized
2. ✅ 認証後にAPI呼び出し → 200 OK
3. ✅ トークン有効期限切れ → 自動更新
4. ✅ ログアウト機能
5. ✅ 複数ユーザーのデータ分離

## 概算工数

- フェーズ1: Azure AD設定 - **1時間**
- フェーズ2: API認証実装 - **2-3時間**
- フェーズ3: クライアント認証実装 - **4-5時間**
- フェーズ4: セキュリティ強化 - **2時間**
- フェーズ5: テスト - **2-3時間**

**合計: 約11-14時間**

## 代替案: 簡易認証（開発用）

開発段階では、以下のような簡易認証も検討できます:

### API Key認証（推奨しません：本番環境では使用禁止）
```csharp
// 開発用のみ - デモ目的
builder.Services.AddAuthentication("ApiKey")
    .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>("ApiKey", null);
```

しかし、**本番環境では必ずOAuth 2.0/OpenID Connect を使用してください**。
```
