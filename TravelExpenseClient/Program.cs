using TravelExpenseClient.Services;

namespace TravelExpenseClient
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static async Task Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            // 認証サービスの初期化
            var authService = new AuthenticationService();

            try
            {
                // 起動時に認証を実行（ブラウザが開いてログイン）
                var token = await authService.GetAccessTokenAsync();

                if (!string.IsNullOrEmpty(token))
                {
                    // 認証成功 - メインフォームを表示
                    Application.Run(new Form1(authService));
                }
                else
                {
                    MessageBox.Show("認証に失敗しました。アプリケーションを終了します。", "認証エラー",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"認証エラー: {ex.Message}\n\nアプリケーションを終了します。", "エラー",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
