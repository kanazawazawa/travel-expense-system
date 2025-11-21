# Azure デプロイ - アプリケーション設定ガイド

## 📋 TravelExpenseApi (Web API)

Azure App Service → 構成 → アプリケーション設定

| 設定名 | 値の例 | 説明 |
|--------|--------|------|
| `AzureTableStorage__ConnectionString` | `DefaultEndpointsProtocol=https;AccountName=...` | Azure Storage接続文字列 |
| `AzureAd__TenantId` | `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx` | Microsoft Entra ID テナントID |
| `AzureAd__ClientId` | `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx` | API用アプリ登録のクライアントID |
| `AzureAd__Instance` | `https://login.microsoftonline.com/` | （オプション）認証エンドポイント |
| `AzureAd__Audience` | `api://xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx` | （オプション）API識別子URI |

### 重要な注意事項
- ✅ **階層構造は `__`（ダブルアンダースコア）で表現**
- ✅ **ConnectionStringは必ず「接続文字列」セクションではなく「アプリケーション設定」に追加**
- ✅ **CORS設定でWebAppのURLを許可**（例：`https://your-webapp.azurewebsites.net`）

---

## 📋 TravelExpenseWebApp (Blazor Server)

Azure App Service → 構成 → アプリケーション設定

| 設定名 | 値の例 | 説明 |
|--------|--------|------|
| `AzureAd__Instance` | `https://login.microsoftonline.com/` | Microsoft Entra ID 認証エンドポイント |
| `AzureAd__TenantId` | `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx` | Microsoft Entra ID テナントID |
| `AzureAd__ClientId` | `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx` | WebApp用アプリ登録のクライアントID |
| `AzureAd__ClientSecret` | `xxxxxxxxxxxxxxxxxxxxxxxxxxxxx` | WebApp用アプリ登録のクライアントシークレット |
| `AzureAd__CallbackPath` | `/signin-oidc` | サインインコールバックパス |
| `AzureAd__SignedOutCallbackPath` | `/signout-callback-oidc` | サインアウトコールバックパス |
| `AzureAd__ApiClientId` | `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx` | API用アプリ登録のクライアントID |
| `ApiSettings__BaseUrl` | `https://your-api.azurewebsites.net/api/TravelExpenses` | TravelExpenseApi のURL |
| `AzureAIAgent__ProjectEndpoint` | `https://yourproject.services.ai.azure.com/api/projects/yourproject` | （オプション）AI Foundryプロジェクトエンドポイント |
| `AzureAIAgent__AgentId` | `asst_xxxxxxxxxxxxx` | （オプション）AI Foundry Agent ID |

### 重要な注意事項
- ✅ **ClientSecretは機密情報として扱う**
- ✅ **ApiSettings__BaseUrlは必ずデプロイ後のAPI URLに変更**
- ✅ **AI Agent機能を使わない場合、AzureAIAgent設定は不要**
- ✅ **Entra IDのリダイレクトURIに `https://your-webapp.azurewebsites.net/signin-oidc` を登録**

---

## 🔐 Microsoft Entra ID アプリ登録の設定

### 1. API用アプリ登録（TravelExpenseApi）

**認証 > リダイレクトURI:**
- 不要（APIは認証を検証するのみ）

**公開 > アプリケーションIDのURI:**
- `api://xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`

**公開 > スコープの追加:**
- `user_impersonation` または `access_as_user`

### 2. WebApp用アプリ登録（TravelExpenseWebApp）

**認証 > リダイレクトURI:**
- プラットフォーム: `Web`
- URI: `https://your-webapp.azurewebsites.net/signin-oidc`

**認証 > ログアウトURL:**
- `https://your-webapp.azurewebsites.net/signout-callback-oidc`

**証明書とシークレット:**
- 新しいクライアントシークレットを作成
- **値をコピーして `AzureAd__ClientSecret` に設定**

**APIのアクセス許可:**
- `TravelExpenseApi` の `user_impersonation` スコープを追加
- 管理者の同意を付与

---

## 🚀 デプロイ手順

### 1. Azure App Service の作成

#### API用
```bash
# リソースグループ作成（既にある場合はスキップ）
az group create --name rg-travel-expense --location japaneast

# App Service プラン作成
az appservice plan create \
  --name plan-travel-expense \
  --resource-group rg-travel-expense \
  --sku B1 \
  --is-linux

# Web App 作成（API）
az webapp create \
  --name your-api-name \
  --resource-group rg-travel-expense \
  --plan plan-travel-expense \
  --runtime "DOTNETCORE:8.0"
```

#### WebApp用
```bash
# Web App 作成（WebApp）
az webapp create \
  --name your-webapp-name \
  --resource-group rg-travel-expense \
  --plan plan-travel-expense \
  --runtime "DOTNETCORE:8.0"
```

### 2. アプリケーション設定の追加

#### API
```bash
az webapp config appsettings set \
  --name your-api-name \
  --resource-group rg-travel-expense \
  --settings \
    AzureTableStorage__ConnectionString="DefaultEndpointsProtocol=https;AccountName=..." \
    AzureAd__TenantId="your-tenant-id" \
    AzureAd__ClientId="your-api-client-id"
```

#### WebApp
```bash
az webapp config appsettings set \
  --name your-webapp-name \
  --resource-group rg-travel-expense \
  --settings \
    AzureAd__Instance="https://login.microsoftonline.com/" \
    AzureAd__TenantId="your-tenant-id" \
    AzureAd__ClientId="your-webapp-client-id" \
    AzureAd__ClientSecret="your-webapp-secret" \
    AzureAd__ApiClientId="your-api-client-id" \
    ApiSettings__BaseUrl="https://your-api-name.azurewebsites.net/api/TravelExpenses"
```

### 3. デプロイ

#### Visual Studio から
1. プロジェクトを右クリック → 「発行」
2. ターゲット: Azure → Azure App Service (Windows)
3. 作成したApp Serviceを選択
4. 発行

#### Azure CLI から
```bash
# API
cd TravelExpenseApi
dotnet publish -c Release
cd bin/Release/net8.0/publish
zip -r api.zip *
az webapp deployment source config-zip \
  --name your-api-name \
  --resource-group rg-travel-expense \
  --src api.zip

# WebApp
cd TravelExpenseWebApp
dotnet publish -c Release
cd bin/Release/net8.0/publish
zip -r webapp.zip *
az webapp deployment source config-zip \
  --name your-webapp-name \
  --resource-group rg-travel-expense \
  --src webapp.zip
```

---

## ✅ デプロイ後の確認

### 1. API動作確認
```bash
curl https://your-api-name.azurewebsites.net/health
```

### 2. WebApp動作確認
- ブラウザで `https://your-webapp-name.azurewebsites.net` にアクセス
- ログインが求められることを確認
- Entra ID でログイン
- 新規登録画面で旅費申請を作成できることを確認

### 3. AI Agent動作確認（設定した場合）
- 新規登録画面のチャットパネルが表示されることを確認
- 「こんにちは」と入力してAIが応答することを確認

---

## 🔧 トラブルシューティング

### ログの確認
```bash
# API
az webapp log tail \
  --name your-api-name \
  --resource-group rg-travel-expense

# WebApp
az webapp log tail \
  --name your-webapp-name \
  --resource-group rg-travel-expense
```

### よくあるエラー

#### 1. "401 Unauthorized" エラー
- ✅ Entra IDのアプリ登録が正しいか確認
- ✅ APIのアクセス許可が付与されているか確認
- ✅ ClientId/TenantIdが正しいか確認

#### 2. "CORS policy" エラー
- ✅ APIのCORS設定にWebAppのURLを追加
- ✅ `https://your-webapp.azurewebsites.net`（末尾スラッシュなし）

#### 3. "Agent not configured" エラー
- ✅ `AzureAIAgent__ProjectEndpoint` と `AzureAIAgent__AgentId` が設定されているか確認
- ✅ Azure AI Foundryでエージェントがデプロイされているか確認
- ✅ Managed Identityに適切なアクセス許可があるか確認

#### 4. AI Agentの初期化が遅い
- ✅ 正常動作（初回は25秒程度かかります）
- ✅ ホーム画面で数秒待ってから新規登録画面に移動すると高速化

---

## 📝 セキュリティチェックリスト

- [ ] すべてのシークレットが環境変数として設定されている
- [ ] `.gitignore` にローカル設定ファイルが含まれている
- [ ] APIのCORSが適切に設定されている
- [ ] HTTPSのみが許可されている
- [ ] Entra IDで多要素認証（MFA）が有効
- [ ] Storage AccountのファイアウォールルールでApp Serviceからのアクセスのみ許可
- [ ] App ServiceでManaged Identityが有効（Storage/AI Foundryアクセス用）

---

## 📞 サポート

デプロイに問題がある場合：
1. Azure Portal → App Service → ログストリーム でエラーを確認
2. Application Insights を有効化してテレメトリを収集
3. `AZURE_DEPLOYMENT_SETTINGS.md`（このファイル）を参照

---

**最終更新日**: 2025年1月19日
