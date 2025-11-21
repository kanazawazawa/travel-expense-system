using TravelExpenseClient.Services;

namespace TravelExpenseClient
{
    public partial class LoginForm : Form
    {
        private readonly AuthenticationService _authService;

        public LoginForm(AuthenticationService authService)
        {
            InitializeComponent();
            _authService = authService;
        }

        private async void ButtonSignIn_Click(object sender, EventArgs e)
        {
            // ボタンを無効化
            buttonSignIn.Enabled = false;
            buttonSignIn.Text = "サインイン中...";
            buttonCancel.Enabled = false;

            try
            {
                // ブラウザを開いてログイン
                var token = await _authService.GetAccessTokenAsync();

                if (!string.IsNullOrEmpty(token))
                {
                    // 認証成功
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("サインインに失敗しました。", "エラー",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    
                    // ボタンを再有効化
                    buttonSignIn.Enabled = true;
                    buttonSignIn.Text = "サインイン";
                    buttonCancel.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"サインインエラー:\n{ex.Message}", "エラー",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                // ボタンを再有効化
                buttonSignIn.Enabled = true;
                buttonSignIn.Text = "サインイン";
                buttonCancel.Enabled = true;
            }
        }

        private void ButtonSettings_Click(object sender, EventArgs e)
        {
            var settingsForm = new SettingsForm();
            settingsForm.ShowDialog(this);
        }
    }
}
