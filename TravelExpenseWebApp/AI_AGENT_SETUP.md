# AI Agent Integration - 設定ガイド

TravelExpenseWebAppに、AI Foundry Agent Serviceを統合しました。新規登録画面の右側にCopilot風のチャットインターフェースが表示されます。

## 統合内容

### 追加されたファイル

1. **Models**
   - `ChatMessage.cs` - チャットメッセージモデル

2. **Services**
   - `IAzureAIAgentService.cs` - AI Agentサービスインターフェース
   - `AzureAIAgentService.cs` - AI Foundry Agent Serviceとの接続実装
   - `AgentModeService.cs` - エージェントの状態管理
   - `TravelExpenseUIUpdateService.cs` - エージェントからUIへの更新通知サービス

3. **Components**
   - `ChatComponent.razor` - Copilot風のチャットUI

4. **更新されたファイル**
   - `ExpenseCreate.razor` - 右側にChatComponentを配置、エージェントからの入力を受け取る
   - `Program.cs` - 新しいサービスの依存性注入
   - `appsettings.json` / `appsettings.Development.json` - AI Agent設定追加

### NuGetパッケージ

以下のパッケージが追加されました：
- `Azure.AI.Agents.Persistent` (1.2.0-beta.7)
- `Azure.AI.Projects` (1.1.0)
- `Azure.Identity` (1.17.0)

## AI Foundry Agent Serviceの設定方法

### 1. AI Foundryでエージェントを作成

1. [Azure AI Foundry](https://ai.azure.com/)にアクセス
2. プロジェクトを作成または既存のプロジェクトを選択
3. エージェントを作成してデプロイ
4. 以下の情報を取得：
   - **Project Endpoint**: プロジェクトのエンドポイントURL（例: `https://yourproject.services.ai.azure.com/api/projects/yourproject`）
   - **Agent ID**: デプロイされたエージェントのID（例: `asst_xxxxxxxxxxxxx`）

### 2. アプリケーション設定を更新

`TravelExpenseWebApp/appsettings.Development.json`を開いて、以下の値を更新：

```json
{
  "AzureAIAgent": {
    "ProjectEndpoint": "https://yourproject.services.ai.azure.com/api/projects/yourproject",
    "AgentId": "asst_xxxxxxxxxxxxx"
  }
}
```

### 3. Azure認証の設定

AI Foundry Agent Serviceへのアクセスには`DefaultAzureCredential`を使用します。以下のいずれかの方法で認証：

#### 開発環境（推奨）
Azure CLIでログイン：
```bash
az login
```

#### Visual Studioを使用
1. Visual Studioの設定でAzureアカウントにサインイン
2. Tools > Options > Azure Service Authentication

#### 本番環境
- Managed Identity（推奨）
- Service Principal（環境変数: `AZURE_CLIENT_ID`, `AZURE_CLIENT_SECRET`, `AZURE_TENANT_ID`）

### 4. アクセス権限の設定

AI Foundryプロジェクトで、使用するAzureアカウントまたはManaged Identityに適切なロールを付与：
- **Cognitive Services OpenAI User** または
- **Cognitive Services Contributor**

## 使用方法

1. アプリケーションを起動
2. `/expenses/create`（新規登録画面）にアクセス
3. 右側にチャットパネルが表示されます
4. チャットで旅費情報を入力すると、左側のフォームに自動的に反映されます

### チャット例

```
ユーザー: 明日、大阪に出張します。新幹線で往復28,000円、宿泊費15,000円、夕食代3,000円です。
エージェント: [フォームに自動入力]
- 出張日: 明日の日付
- 出張先: 大阪
- 交通費: 28,000円
- 宿泊費: 15,000円
- 食事代: 3,000円
```

## トラブルシューティング

### "Agent not configured"と表示される
- `appsettings.Development.json`の`AzureAIAgent`設定を確認
- ProjectEndpointとAgentIdが正しく設定されているか確認

### 認証エラー
- `az login`でAzure CLIにログインしているか確認
- AI Foundryプロジェクトへのアクセス権限を確認

### チャットからフォームに反映されない
- ブラウザのコンソールでエラーを確認
- エージェントが正しいフォーマットで応答しているか確認（EXPENSE_UPDATE形式）

## 参考アプリ

この実装は以下の参考アプリをベースにしています：
`C:\Users\tkanazawa\source\20251118-keihiseisan\ai-agent-openai-web-app-main`

## サポート

問題が発生した場合は、以下を確認してください：
1. ビルドログのエラーメッセージ
2. ブラウザの開発者ツールのコンソール
3. アプリケーションログ（`ILogger`の出力）
