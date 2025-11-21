# Travel Expense Management System

旅費精算管理システム - Azure Table Storage + .NET 8 + Microsoft Entra ID 認証

## 構成

- **TravelExpenseApi** - ASP.NET Core Web API（認証保護済み）
- **TravelExpenseWebApp** - Blazor Server Web アプリケーション（認証必須）
- **TravelExpenseClient** - Windows Forms デスクトップアプリ（MSAL認証）

## 機能

- ✅ 旅費申請の作成・一覧表示・削除
- ✅ サマリー表示（総件数・合計金額・ステータス別集計）
- ✅ **AI Agent統合** - Azure AI Foundry Agent Serviceによる対話型フォーム入力
  - Copilot風のチャットインターフェース
  - 自然言語での旅費情報入力
  - 画像アップロード・クリップボード画像ペースト対応
  - フォーム自動入力とハイライト表示
- ✅ Microsoft Entra ID による認証・認可
- ✅ Azure Table Storage によるデータ永続化
- ✅ マルチクライアント対応（Web/Desktop）

## セットアップ

### 1. 必須：開発環境用設定ファイルの作成

#### TravelExpenseApi/appsettings.Development.json
```json
{
  "AzureTableStorage": {
    "ConnectionString": "YOUR_AZURE_STORAGE_CONNECTION_STRING"
  },
  "AzureAd": {
    "TenantId": "YOUR_TENANT_ID",
    "ClientId": "YOUR_API_CLIENT_ID"
  }
}
```

#### TravelExpenseWebApp/appsettings.Development.json
```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "YOUR_TENANT_ID",
    "ClientId": "YOUR_WEBAPP_CLIENT_ID",
    "ClientSecret": "YOUR_CLIENT_SECRET",
    "CallbackPath": "/signin-oidc",
    "SignedOutCallbackPath": "/signout-callback-oidc",
    "ApiClientId": "YOUR_API_CLIENT_ID"
  },
  "ApiSettings": {
    "BaseUrl": "https://localhost:7115/api/TravelExpenses"
  },
  "AzureAIAgent": {
    "ProjectEndpoint": "https://yourproject.services.ai.azure.com/api/projects/yourproject",
    "AgentId": "asst_xxxxxxxxxxxxx"
  }
}
```

**AI Agent機能を使用する場合は、`AI_AGENT_SETUP.md` を参照して設定してください。**

### 2. Microsoft Entra ID アプリ登録

詳細は `AUTHENTICATION_SETUP.md` を参照してください。

**必要なアプリ登録:**
1. **API用** - TravelExpenseApi の保護
2. **WebApp用** - TravelExpenseWebApp のユーザー認証
3. **WinForm用** (既に設定済み) - TravelExpenseClient のユーザー認証

### 3. 実行

#### API
```bash
cd TravelExpenseApi
dotnet run
```

#### WebApp
```bash
cd TravelExpenseWebApp
dotnet run
```

#### WinForm Client

デスクトップクライアントは本番環境用の設定ファイルが必要です：

**TravelExpenseClient/appsettings.Production.json**
```json
{
  "ApiSettings": {
    "BaseUrl": "https://YOUR_AZURE_API_URL/api/TravelExpenses"
  }
}
```

実行：
```bash
cd TravelExpenseClient
dotnet run
```

**注意**: `appsettings.Production.json` は `.gitignore` で除外されており、各開発者が個別に作成する必要があります。

## Azure デプロイ設定

本番環境（Azure App Service）では、以下の環境変数を設定してください：

**注意**: Azure App Serviceの環境変数では、階層構造を表現するために `:` ではなく `__`（ダブルアンダースコア）を使用します。

### TravelExpenseApi
- `AzureTableStorage__ConnectionString`
- `AzureAd__TenantId`
- `AzureAd__ClientId`

### TravelExpenseWebApp
- `AzureAd__TenantId`
- `AzureAd__ClientId`
- `AzureAd__ClientSecret`
- `AzureAd__ApiClientId`
- `ApiSettings__BaseUrl`

## セキュリティ

- ✅ すべての API エンドポイントは認証が必須
- ✅ WebApp はログインしないとアクセス不可
- ✅ 機密情報（接続文字列・シークレット）は `.gitignore` で除外
- ✅ 本番環境では Azure App Service の環境変数を使用
- ✅ トークンベースの認証・認可（JWT Bearer Token）

## アーキテクチャ

```
[Web Browser] → TravelExpenseWebApp (Blazor Server)
                        ↓ (OAuth 2.0 / OpenID Connect)
                    Entra ID
                        ↓ (Access Token)
                    TravelExpenseApi
                        ↓
                Azure Table Storage

[Desktop App] → TravelExpenseClient (WinForms)
                        ↓ (MSAL - Interactive Auth)
                    Entra ID
                        ↓ (Access Token)
                    TravelExpenseApi
                        ↓
                Azure Table Storage
```

## 技術スタック

- .NET 8
- ASP.NET Core Web API
- Blazor Server
- Windows Forms
- Microsoft Entra ID (Azure AD)
- Azure Table Storage
- Microsoft Identity Web
- MSAL.NET

## ライセンス

MIT
