using Microsoft.Extensions.Configuration;
using TravelExpenseClient.Models;
using TravelExpenseClient.Services;

namespace TravelExpenseClient
{
    public partial class Form1 : Form
    {
        private readonly TravelExpenseApiService _apiService;
        private readonly AuthenticationService _authService;
        private List<TravelExpenseResponse> _expenses = new();
        private bool _isEditMode = false;
        private string? _editingPartitionKey;
        private string? _editingRowKey;

        // 認証ありのコンストラクタ
        public Form1(AuthenticationService authService)
        {
            InitializeComponent();
            _authService = authService;
            _apiService = new TravelExpenseApiService(authService);
            
            // タイトルバーに接続先を表示
            UpdateFormTitle();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            await LoadDataAsync();
        }

        /// <summary>
        /// フォームタイトルに接続先を表示
        /// </summary>
        private void UpdateFormTitle()
        {
            var currentUrl = GetCurrentApiUrl();
            var environment = GetEnvironmentName(currentUrl);
            this.Text = $"旅費精算管理システム - [{environment}]";
        }

        /// <summary>
        /// 現在のAPIエンドポイントを取得
        /// </summary>
        private string GetCurrentApiUrl()
        {
            try
            {
                var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .Build();

                return configuration["ApiSettings:BaseUrl"] ?? "不明";
            }
            catch
            {
                return "不明";
            }
        }

        /// <summary>
        /// URLから環境名を判定
        /// </summary>
        private string GetEnvironmentName(string url)
        {
            if (url.Contains("localhost"))
            {
                return "ローカル環境";
            }
            else if (url.Contains("azurewebsites.net"))
            {
                return "Azure環境";
            }
            else if (url == "不明")
            {
                return "接続先不明";
            }
            else
            {
                return "カスタム環境";
            }
        }

        private async Task LoadDataAsync()
        {
            try
            {
                // 一覧を読み込み
                _expenses = await _apiService.GetAllExpensesAsync();
                
                // DataGridViewにバインド
                dataGridViewExpenses.DataSource = null;
                dataGridViewExpenses.DataSource = _expenses;
                
                // 列の表示設定
                ConfigureDataGridView();
                
                // サマリーを更新
                await LoadSummaryAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"データの読み込みに失敗しました: {ex.Message}", "エラー", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureDataGridView()
        {
            if (dataGridViewExpenses.Columns.Count > 0)
            {
                dataGridViewExpenses.Columns["Id"].HeaderText = "ID";
                dataGridViewExpenses.Columns["ApplicationDate"].HeaderText = "申請日";
                dataGridViewExpenses.Columns["ApplicantName"].HeaderText = "申請者";
                dataGridViewExpenses.Columns["TravelDate"].HeaderText = "出張日";
                dataGridViewExpenses.Columns["Destination"].HeaderText = "出張先";
                dataGridViewExpenses.Columns["Purpose"].HeaderText = "目的";
                dataGridViewExpenses.Columns["Transportation"].HeaderText = "交通手段";
                dataGridViewExpenses.Columns["TransportationCost"].HeaderText = "交通費";
                dataGridViewExpenses.Columns["AccommodationCost"].HeaderText = "宿泊費";
                dataGridViewExpenses.Columns["MealCost"].HeaderText = "食事代";
                dataGridViewExpenses.Columns["OtherCost"].HeaderText = "その他";
                dataGridViewExpenses.Columns["TotalAmount"].HeaderText = "合計";
                dataGridViewExpenses.Columns["Status"].HeaderText = "ステータス";
                dataGridViewExpenses.Columns["Remarks"].HeaderText = "備考";

                // 日付のフォーマット
                dataGridViewExpenses.Columns["ApplicationDate"].DefaultCellStyle.Format = "yyyy/MM/dd";
                dataGridViewExpenses.Columns["TravelDate"].DefaultCellStyle.Format = "yyyy/MM/dd";
                
                // 金額のフォーマット
                dataGridViewExpenses.Columns["TransportationCost"].DefaultCellStyle.Format = "N0";
                dataGridViewExpenses.Columns["AccommodationCost"].DefaultCellStyle.Format = "N0";
                dataGridViewExpenses.Columns["MealCost"].DefaultCellStyle.Format = "N0";
                dataGridViewExpenses.Columns["OtherCost"].DefaultCellStyle.Format = "N0";
                dataGridViewExpenses.Columns["TotalAmount"].DefaultCellStyle.Format = "N0";
            }
        }

        private async Task LoadSummaryAsync()
        {
            try
            {
                var summary = await _apiService.GetSummaryAsync();
                labelTotal.Text = $"総申請数: {summary.TotalCount} 件";
                labelPending.Text = $"承認待ち: {summary.PendingCount} 件";
                labelApproved.Text = $"承認済み: {summary.ApprovedCount} 件";
                labelRejected.Text = $"却下: {summary.RejectedCount} 件";
                labelTotalAmount.Text = $"総費用: {summary.TotalAmount:N0} 円";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"サマリーの読み込みに失敗しました: {ex.Message}", "エラー",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void ButtonRefresh_Click(object sender, EventArgs e)
        {
            await LoadDataAsync();
        }

        private void ButtonAdd_Click(object sender, EventArgs e)
        {
            _isEditMode = false;
            _editingPartitionKey = null;
            _editingRowKey = null;
            ClearDetailFields();
            groupBoxDetails.Visible = true;
        }

        private void ButtonEdit_Click(object sender, EventArgs e)
        {
            if (dataGridViewExpenses.SelectedRows.Count == 0)
            {
                MessageBox.Show("編集する行を選択してください。", "確認",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedExpense = (TravelExpenseResponse)dataGridViewExpenses.SelectedRows[0].DataBoundItem;
            _isEditMode = true;
            _editingPartitionKey = selectedExpense.ApplicationDate.ToString("yyyy-MM");
            _editingRowKey = selectedExpense.Id;
            
            LoadDetailFields(selectedExpense);
            groupBoxDetails.Visible = true;
        }

        private async void ButtonDelete_Click(object sender, EventArgs e)
        {
            if (dataGridViewExpenses.SelectedRows.Count == 0)
            {
                MessageBox.Show("削除する行を選択してください。", "確認",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedExpense = (TravelExpenseResponse)dataGridViewExpenses.SelectedRows[0].DataBoundItem;
            
            var result = MessageBox.Show(
                $"申請者: {selectedExpense.ApplicantName}\n出張先: {selectedExpense.Destination}\n\nこの申請を削除しますか?",
                "削除確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    var partitionKey = selectedExpense.ApplicationDate.ToString("yyyy-MM");
                    await _apiService.DeleteExpenseAsync(partitionKey, selectedExpense.Id);
                    MessageBox.Show("削除しました。", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadDataAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"削除に失敗しました: {ex.Message}", "エラー",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void ButtonSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs())
            {
                return;
            }

            try
            {
                var request = new TravelExpenseRequest
                {
                    ApplicantName = textBoxApplicant.Text.Trim(),
                    TravelDate = dateTimePickerTravel.Value,
                    Destination = textBoxDestination.Text.Trim(),
                    Purpose = textBoxPurpose.Text.Trim(),
                    Transportation = comboBoxTransportation.Text,
                    TransportationCost = (int)numericUpDownTransportation.Value,
                    AccommodationCost = (int)numericUpDownAccommodation.Value,
                    MealCost = (int)numericUpDownMeal.Value,
                    OtherCost = (int)numericUpDownOther.Value,
                    Remarks = textBoxRemarks.Text.Trim()
                };

                if (_isEditMode && _editingPartitionKey != null && _editingRowKey != null)
                {
                    await _apiService.UpdateExpenseAsync(_editingPartitionKey, _editingRowKey, request);
                    MessageBox.Show("更新しました。", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    await _apiService.CreateExpenseAsync(request);
                    MessageBox.Show("登録しました。", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                groupBoxDetails.Visible = false;
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存に失敗しました: {ex.Message}", "エラー",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            groupBoxDetails.Visible = false;
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(textBoxApplicant.Text))
            {
                MessageBox.Show("申請者名を入力してください。", "入力エラー",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxApplicant.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(textBoxDestination.Text))
            {
                MessageBox.Show("出張先を入力してください。", "入力エラー",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxDestination.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(textBoxPurpose.Text))
            {
                MessageBox.Show("目的を入力してください。", "入力エラー",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxPurpose.Focus();
                return false;
            }

            if (comboBoxTransportation.SelectedIndex == -1)
            {
                MessageBox.Show("交通手段を選択してください。", "入力エラー",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                comboBoxTransportation.Focus();
                return false;
            }

            return true;
        }

        private void ClearDetailFields()
        {
            textBoxApplicant.Clear();
            dateTimePickerTravel.Value = DateTime.Now;
            textBoxDestination.Clear();
            textBoxPurpose.Clear();
            comboBoxTransportation.SelectedIndex = -1;
            numericUpDownTransportation.Value = 0;
            numericUpDownAccommodation.Value = 0;
            numericUpDownMeal.Value = 0;
            numericUpDownOther.Value = 0;
            textBoxRemarks.Clear();
            UpdateTotalCost();
        }

        private void LoadDetailFields(TravelExpenseResponse expense)
        {
            textBoxApplicant.Text = expense.ApplicantName;
            dateTimePickerTravel.Value = expense.TravelDate;
            textBoxDestination.Text = expense.Destination;
            textBoxPurpose.Text = expense.Purpose;
            comboBoxTransportation.Text = expense.Transportation;
            numericUpDownTransportation.Value = expense.TransportationCost;
            numericUpDownAccommodation.Value = expense.AccommodationCost;
            numericUpDownMeal.Value = expense.MealCost;
            numericUpDownOther.Value = expense.OtherCost;
            textBoxRemarks.Text = expense.Remarks ?? string.Empty;
            UpdateTotalCost();
        }

        private void NumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            UpdateTotalCost();
        }

        private void UpdateTotalCost()
        {
            var total = (int)(numericUpDownTransportation.Value + 
                             numericUpDownAccommodation.Value + 
                             numericUpDownMeal.Value + 
                             numericUpDownOther.Value);
            labelTotalCost.Text = $"{total:N0} 円";
        }
    }
}
