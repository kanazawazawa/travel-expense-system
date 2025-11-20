# Travel Expense Management System

旅費精算管理システム - Azure Table Storage + .NET 8

## 構成

- **TravelExpenseApi** - ASP.NET Core Web API
- **TravelExpenseClient** - Windows Forms デスクトップアプリ

## セットアップ

### 1. appsettings.Development.json の作成

TravelExpenseApi/appsettings.Development.json を作成:

```json
{
  "AzureTableStorage": {
    "ConnectionString": "YOUR_CONNECTION_STRING"
  }
}
```

### 2. 実行

```bash
cd TravelExpenseApi
dotnet run

cd TravelExpenseClient
dotnet run
```

## セキュリティ

- appsettings.Development.json は .gitignore で除外済み
- 本番環境では Azure App Service の Application Settings を使用

## ライセンス

MIT
