namespace TravelExpenseWebApp.Models
{
    /// <summary>
    /// ユーザープロファイル（デモ用）
    /// 本番環境では Microsoft Entra ID から取得
    /// </summary>
    public class UserProfile
    {
        public string UserId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public DateTime JoinDate { get; set; }
        public TravelExpenseSettings TravelExpenseSettings { get; set; } = new();
        public string Note { get; set; } = string.Empty;
    }

    /// <summary>
    /// 旅費規程設定
    /// </summary>
    public class TravelExpenseSettings
    {
        public int DailyAllowance { get; set; }
        public int AccommodationLimit { get; set; }
        public bool CanUseGreenCar { get; set; }
        public bool CanUseBusinessClass { get; set; }
        public bool ApprovalRequired { get; set; }
    }
}
