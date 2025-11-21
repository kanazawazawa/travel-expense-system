using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace TravelExpenseClient
{
    public partial class SettingsForm : Form
    {
        private const string LocalUrl = "https://localhost:7115/api/TravelExpenses";
        private const string AzureUrl = "https://app-20251120-api.azurewebsites.net/api/TravelExpenses";

        public SettingsForm()
        {
            InitializeComponent();
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            try
            {
                var currentUrl = GetCurrentUrl();
                labelCurrentUrl.Text = $"現在の接続先: {currentUrl}";

                // 現在の設定に応じてラジオボタンを選択
                if (currentUrl == LocalUrl)
                {
                    radioButtonLocal.Checked = true;
                }
                else if (currentUrl == AzureUrl)
                {
                    radioButtonAzure.Checked = true;
                }
                else
                {
                    radioButtonCustom.Checked = true;
                    textBoxCustomUrl.Text = currentUrl;
                }
            }
            catch
            {
                radioButtonLocal.Checked = true;
            }
        }

        private string GetCurrentUrl()
        {
            try
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .Build();

                return configuration["ApiSettings:BaseUrl"] ?? LocalUrl;
            }
            catch
            {
                return LocalUrl;
            }
        }

        private void RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            // カスタムURLが選択された場合のみテキストボックスを有効化
            textBoxCustomUrl.Enabled = radioButtonCustom.Checked;
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            try
            {
                string newUrl;

                if (radioButtonLocal.Checked)
                {
                    newUrl = LocalUrl;
                }
                else if (radioButtonAzure.Checked)
                {
                    newUrl = AzureUrl;
                }
                else if (radioButtonCustom.Checked)
                {
                    newUrl = textBoxCustomUrl.Text.Trim();
                    
                    // URL検証
                    if (string.IsNullOrWhiteSpace(newUrl) || !Uri.IsWellFormedUriString(newUrl, UriKind.Absolute))
                    {
                        MessageBox.Show("有効なURLを入力してください。", "入力エラー",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        textBoxCustomUrl.Focus();
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("接続先を選択してください。", "選択エラー",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // appsettings.jsonを更新
                SaveSettings(newUrl);

                MessageBox.Show("設定を保存しました。\nアプリケーションを再起動してください。", "保存成功",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"設定の保存に失敗しました:\n{ex.Message}", "エラー",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveSettings(string baseUrl)
        {
            var settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

            // 既存の設定を読み込む
            var settings = new Dictionary<string, object>();
            
            if (File.Exists(settingsPath))
            {
                var json = File.ReadAllText(settingsPath);
                settings = JsonSerializer.Deserialize<Dictionary<string, object>>(json) 
                    ?? new Dictionary<string, object>();
            }

            // ApiSettingsを更新
            settings["ApiSettings"] = new Dictionary<string, string>
            {
                { "BaseUrl", baseUrl }
            };

            // ファイルに書き込み
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            var updatedJson = JsonSerializer.Serialize(settings, options);
            File.WriteAllText(settingsPath, updatedJson);
        }
    }
}
