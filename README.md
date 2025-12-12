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
- ✅ **音声チャット機能** - GPT Realtime API による音声対話
  - リアルタイム音声認識と応答
  - テキストチャットと音声チャットの切り替え可能
  - サーバー側VAD（音声アクティビティ検出）
  - 自然な会話による旅費情報入力
- ✅ Microsoft Entra ID による認証・認可
- ✅ Azure Table Storage によるデータ永続化
- ✅ マルチクライアント対応（Web/Desktop）

## セットアップ

### 1. 必須：開発環境用設定ファイルの作成

**⚠️ 重要**: すべての機密情報は `appsettings.Development.json` に保存してください。このファイルは `.gitignore` で除外されており、GitHubにコミットされません。

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

**テンプレートファイルをコピー:**
```bash
cd TravelExpenseWebApp
cp appsettings.Development.json.template appsettings.Development.json
```

**または、以下の内容で新規作成:**
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
    "ProjectEndpoint": "YOUR_AI_PROJECT_ENDPOINT",
    "AgentName": "YOUR_AGENT_NAME",
    "ModelDeploymentName": "gpt-4o"
  },
  "AzureOpenAI": {
    "Endpoint": "YOUR_AZURE_OPENAI_ENDPOINT",
    "RealtimeDeploymentName": "gpt-realtime",
    "ApiKey": "YOUR_AZURE_OPENAI_API_KEY"
  }
}
```

**AI Agent機能を使用する場合は、`AI_AGENT_SETUP.md` を参照して設定してください。**

**🔐 セキュリティのベストプラクティス:**

機密情報はUser Secretsで管理することを強く推奨します：

```bash
cd TravelExpenseWebApp
dotnet user-secrets init
dotnet user-secrets set "AzureAd:ClientSecret" "YOUR_CLIENT_SECRET"
dotnet user-secrets set "AzureOpenAI:ApiKey" "YOUR_API_KEY"
```

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
- `AzureAIAgent__ProjectEndpoint`
- `AzureAIAgent__AgentName`
- `AzureAIAgent__ModelDeploymentName`
- `AzureOpenAI__Endpoint`
- `AzureOpenAI__RealtimeDeploymentName`
- `AzureOpenAI__ApiKey`

## 🎤 音声チャット機能の使い方

### 前提条件
- Azure OpenAI Service で `gpt-4o-realtime-preview` モデルをデプロイ
- サポートされているリージョン: 米国東部2 (EastUS2)、スウェーデン中部 (SwedenCentral)
- HTTPS 接続（必須）
- マイクアクセス許可

### 主な機能
✨ **スマート出張アシスタント**
- 🎯 過去の出張パターンを学習して自動提案
- 📅 日本時間に対応した日付計算（「明後日」「来週火曜」など）
- ⚡ リアルタイムでフォームに反映
- 🗣️ 自然な会話で情報収集
- 🔄 「適当に入れておいて」で過去パターンから自動入力

### 使い方
1. `/expenses/create` ページにアクセス
2. 右側の **「音声チャット」** タブをクリック
3. **「音声会話を開始」** ボタンを押す
4. ブラウザのマイクアクセス許可を承認
5. 出張情報を話す

**会話例:**
```
User: 「来週火曜から大阪に出張で、適当に入れておいて」
AI  : 「入力しました。他にありますか？」
→ 過去パターンから交通費・宿泊費・日当を自動入力
→ フォームに即座に反映（緑色ハイライト）
```

### デモ用ファイル
以下のファイルはデモ・開発用です（`.gitignore` で除外済み）：
- `USER_PROFILE_DEMO.json` - ユーザープロファイル（役職・旅費規程）
- `TRAVEL_HISTORY_DEMO.json` - 過去の出張履歴（学習データ）
- `VOICE_AGENT_INSTRUCTIONS.md` - AI エージェントの指示書

**本番環境では**:
- ユーザープロファイル → Microsoft Entra ID から取得
- 出張履歴 → Azure Table Storage から取得

### トラブルシューティング
- **音声が認識されない**: マイクアクセス許可を確認
- **接続エラー**: `appsettings.Development.json` の `AzureOpenAI` セクションを確認
- **エラー詳細**: F12でブラウザの開発者コンソールを開いて確認

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
