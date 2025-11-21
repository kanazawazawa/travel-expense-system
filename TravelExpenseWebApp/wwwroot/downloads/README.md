# TravelExpenseClient ClickOnce Downloads

このフォルダには、ClickOnce で発行された TravelExpenseClient のインストーラーファイルを配置します。

## ファイル構成

発行後、以下のファイルをこのフォルダに配置してください：

```
downloads/
  ├── TravelExpenseClient.application  (マニフェストファイル)
  ├── Application Files/
  │   └── TravelExpenseClient_1_0_0_0/
  │       ├── TravelExpenseClient.exe.manifest
  │       ├── TravelExpenseClient.exe.deploy
  │       └── (その他の依存ファイル)
  └── setup.exe  (オプション: ブートストラッパー)
```

## 発行手順

### Visual Studio から発行

1. TravelExpenseClient プロジェクトを右クリック → **発行**
2. **フォルダー** を選択 → **参照** → このフォルダを選択
3. **発行** をクリック

### コマンドラインから発行

```powershell
cd TravelExpenseClient
dotnet publish -c Release -r win-x64 --self-contained false
```

その後、発行されたファイルをこのフォルダにコピーしてください。

## 注意事項

- ClickOnce ファイルは `.gitignore` で除外されています
- Azure にデプロイする際は、これらのファイルも含めてください
- または、Azure Blob Storage を使用して配信することを推奨します
