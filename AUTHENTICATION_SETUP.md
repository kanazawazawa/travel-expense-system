# 認証機能実装ガイド

このブランチ (`feature/authentication`) には、Azure AD認証の基礎実装が含まれています。

## 実装内容

### ✅ 完了した実装

#### API側 (TravelExpenseApi)
- ✅ `Microsoft.Identity.Web` パッケージの追加
- ✅ `Program.cs` に認証ミドルウェアの追加
- ✅ `appsettings.json` にAzure AD設定のプレースホルダー追加
- ✅ `TravelExpensesController` に `Microsoft.AspNetCore.Authorization` の using 追加

#### クライアント側 (TravelExpenseClient)
- ✅ `Microsoft.Identity.Client` パッケージの追加
- ✅ `AuthenticationService.cs` の作成（Azure AD認証ロジック）
- ✅ `TravelExpenseApiService.cs` の更新（トークンをHTTPヘッダーに追加）

## 次のステップ: Azure AD設定

### 1. Azure Portal でアプリを登録

#### 1-1. API用アプリの登録

```
1. Azure Portal → Microsoft Entra ID → アプリの登録 → 新規登録
2. 名前: TravelExpenseApi
3. サポートされているアカウントの種類: 
   - 単一テナント（あなたの組織のみ）を選択
4. リダイレクトURI: 空白のまま
5. 「登録」をクリック

6. 登録後、「概要」ページで以下をメモ:
   - アプリケーション (クライアント) ID
   - ディレクトリ (テナント) ID

7. 「公開」 → 「スコープの追加」:
   - スコープ名: Expenses.Read
   - 同意できるのは: 管理者とユーザー
   - 表示名と説明を入力
   - 「スコープの追加」

8. もう一つスコープを追加:
   - スコープ名: Expenses.Write
   - 同意できるのは: 管理者とユーザー
   - 表示名と説明を入力
   - 「スコープの追加」

9. 「公開」ページで「アプリケーション ID URI」をメモ
   - 形式: api://xxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
```

#### 1-2. クライアント用アプリの登録

```
1. Azure Portal → Microsoft Entra ID → アプリの登録 → 新規登録
2. 名前: TravelExpenseClient
3. サポートされているアカウントの種類: 
   - 単一テナント（あなたの組織のみ）を選択
4. リダイレクトURI: 
   - プラットフォーム: モバイルとデスクトップ
   - URI: http://localhost
5. 「登録」をクリック

6. 登録後、「概要」ページで以下をメモ:
   - アプリケーション (クライアント) ID

7. 「API のアクセス許可」:
   - 「アクセス許可の追加」
   - 「所属する組織で使用している API」
   - 「TravelExpenseApi」を検索して選択
   - 「委任されたアクセス許可」
   - Expenses.Read と Expenses.Write にチェック
   - 「アクセス許可の追加」
   - （オプション）「[テナント名] に管理者の同意を与えます」をクリック
```

### 2. 設定ファイルの更新

#### 2-1. API側の設定

`TravelExpenseApi/appsettings.Development.json` を編集:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AzureTableStorage": {
    "ConnectionString": "YOUR_CONNECTION_STRING"
  },
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "YOUR_TENANT_ID",
    "ClientId": "YOUR_API_CLIENT_ID",
    "Audience": "api://YOUR_API_CLIENT_ID"
  }
}
```

#### 2-2. クライアント側の設定

`TravelExpenseClient/Services/AuthenticationService.cs` を編集:

```csharp
private const string ClientId = "YOUR_CLIENT_APP_ID"; // クライアントアプリのID
private const string TenantId = "YOUR_TENANT_ID";     // テナントID
private const string ApiClientId = "YOUR_API_CLIENT_ID"; // APIアプリのID
```

### 3. 認証を有効化

#### 3-1. API側

`TravelExpenseApi/Controllers/TravelExpensesController.cs` のコメントを外す:

```csharp
// TODO: Azure AD設定後、以下のコメントを外して認証を有効化
[Authorize]  // ← このコメントを外す
[ApiController]
[Route("api/[controller]")]
public class TravelExpensesController : ControllerBase
```

#### 3-2. クライアント側

`TravelExpenseClient/Form1.cs` を更新して、AuthenticationServiceを使用:

```csharp
// コンストラクタを変更
private readonly AuthenticationService _authService;

public Form1(AuthenticationService authService)
{
    InitializeComponent();
    _authService = authService;
    _apiService = new TravelExpenseApiService(authService); // 認証サービスを渡す
}
```

`TravelExpenseClient/Program.cs` を更新して、起動時にログイン:

```csharp
Application.SetHighDpiMode(HighDpiMode.SystemAware);
Application.EnableVisualStyles();
Application.SetCompatibleTextRenderingDefault(false);

var authService = new AuthenticationService();

try
{
    // 起動時にログイン
    var token = await authService.GetAccessTokenAsync();
    if (!string.IsNullOrEmpty(token))
    {
        Application.Run(new Form1(authService));
    }
    else
    {
        MessageBox.Show("認証に失敗しました。", "エラー", 
            MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
catch (Exception ex)
{
    MessageBox.Show($"認証エラー: {ex.Message}", "エラー", 
        MessageBoxButtons.OK, MessageBoxIcon.Error);
}
```

### 4. テスト

1. APIを起動: `cd TravelExpenseApi && dotnet run`
2. クライアントを起動: `cd TravelExpenseClient && dotnet run`
3. ブラウザが開いてMicrosoftログイン画面が表示される
4. ログイン後、自動的にアプリに戻る
5. APIへのアクセスが成功することを確認

### 5. トラブルシューティング

#### 「AADSTS700016」エラー
- クライアントアプリのリダイレクトURIが正しく設定されているか確認
- `http://localhost` が「モバイルとデスクトップ」プラットフォームで設定されているか確認

#### 「AADSTS65001」エラー
- APIのアクセス許可が正しく設定されているか確認
- 管理者の同意が必要な場合があります

#### 401 Unauthorized エラー
- APIの `appsettings.json` の TenantId と ClientId が正しいか確認
- トークンのスコープが正しいか確認

## オプション機能

### ログアウト機能の追加

Form1にログアウトボタンを追加:

```csharp
private async void ButtonLogout_Click(object sender, EventArgs e)
{
    await _authService.SignOutAsync();
    MessageBox.Show("ログアウトしました。アプリケーションを再起動してください。", 
        "ログアウト", MessageBoxButtons.OK, MessageBoxIcon.Information);
    Application.Exit();
}
```

### ユーザー情報の表示

API側でユーザー情報を取得:

```csharp
[HttpGet("me")]
public IActionResult GetCurrentUser()
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var userName = User.Identity?.Name;
    var email = User.FindFirst(ClaimTypes.Email)?.Value;
    
    return Ok(new { UserId = userId, UserName = userName, Email = email });
}
```

## セキュリティ上の注意

- ✅ `appsettings.Development.json` は `.gitignore` で除外されています
- ✅ クライアントIDとテナントIDは、コード内にハードコードしても問題ありません（公開情報）
- ⚠️ 本番環境では、Azure App Service の Application Settings を使用してください
- ⚠️ トークンをログに出力しないでください

## 参考リンク

- [Microsoft Identity Platform](https://learn.microsoft.com/ja-jp/entra/identity-platform/)
- [MSAL.NET](https://learn.microsoft.com/ja-jp/entra/msal/dotnet/)
- [Microsoft.Identity.Web](https://learn.microsoft.com/ja-jp/entra/msal/dotnet/microsoft-identity-web/)
