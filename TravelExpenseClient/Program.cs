using TravelExpenseClient.Services;

namespace TravelExpenseClient
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            // 認証サービスの初期化
            var authService = new AuthenticationService();

            // サインイン画面を表示
            var loginForm = new LoginForm(authService);
            
            if (loginForm.ShowDialog() == DialogResult.OK)
            {
                // サインイン成功 - メインフォームを表示
                Application.Run(new Form1(authService));
            }
            else
            {
                // サインインをキャンセル - アプリケーション終了
                // （特に何もせずに終了）
            }
        }
    }
}
