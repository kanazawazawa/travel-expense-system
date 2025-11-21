namespace TravelExpenseClient
{
    partial class SettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelTitle = new System.Windows.Forms.Label();
            this.groupBoxEndpoint = new System.Windows.Forms.GroupBox();
            this.radioButtonLocal = new System.Windows.Forms.RadioButton();
            this.radioButtonAzure = new System.Windows.Forms.RadioButton();
            this.radioButtonCustom = new System.Windows.Forms.RadioButton();
            this.textBoxCustomUrl = new System.Windows.Forms.TextBox();
            this.labelLocalUrl = new System.Windows.Forms.Label();
            this.labelAzureUrl = new System.Windows.Forms.Label();
            this.buttonSave = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.labelCurrentUrl = new System.Windows.Forms.Label();
            this.groupBoxEndpoint.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelTitle
            // 
            this.labelTitle.AutoSize = true;
            this.labelTitle.Font = new System.Drawing.Font("Yu Gothic UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.labelTitle.Location = new System.Drawing.Point(20, 20);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(158, 21);
            this.labelTitle.TabIndex = 0;
            this.labelTitle.Text = "API エンドポイント設定";
            // 
            // groupBoxEndpoint
            // 
            this.groupBoxEndpoint.Controls.Add(this.labelAzureUrl);
            this.groupBoxEndpoint.Controls.Add(this.labelLocalUrl);
            this.groupBoxEndpoint.Controls.Add(this.textBoxCustomUrl);
            this.groupBoxEndpoint.Controls.Add(this.radioButtonCustom);
            this.groupBoxEndpoint.Controls.Add(this.radioButtonAzure);
            this.groupBoxEndpoint.Controls.Add(this.radioButtonLocal);
            this.groupBoxEndpoint.Font = new System.Drawing.Font("Yu Gothic UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.groupBoxEndpoint.Location = new System.Drawing.Point(20, 60);
            this.groupBoxEndpoint.Name = "groupBoxEndpoint";
            this.groupBoxEndpoint.Size = new System.Drawing.Size(560, 240);
            this.groupBoxEndpoint.TabIndex = 1;
            this.groupBoxEndpoint.TabStop = false;
            this.groupBoxEndpoint.Text = "接続先";
            // 
            // radioButtonLocal
            // 
            this.radioButtonLocal.AutoSize = true;
            this.radioButtonLocal.Location = new System.Drawing.Point(20, 30);
            this.radioButtonLocal.Name = "radioButtonLocal";
            this.radioButtonLocal.Size = new System.Drawing.Size(162, 19);
            this.radioButtonLocal.TabIndex = 0;
            this.radioButtonLocal.TabStop = true;
            this.radioButtonLocal.Text = "ローカル環境 (開発用)";
            this.radioButtonLocal.UseVisualStyleBackColor = true;
            this.radioButtonLocal.CheckedChanged += new System.EventHandler(this.RadioButton_CheckedChanged);
            // 
            // radioButtonAzure
            // 
            this.radioButtonAzure.AutoSize = true;
            this.radioButtonAzure.Location = new System.Drawing.Point(20, 90);
            this.radioButtonAzure.Name = "radioButtonAzure";
            this.radioButtonAzure.Size = new System.Drawing.Size(150, 19);
            this.radioButtonAzure.TabIndex = 1;
            this.radioButtonAzure.TabStop = true;
            this.radioButtonAzure.Text = "Azure 環境 (本番用)";
            this.radioButtonAzure.UseVisualStyleBackColor = true;
            this.radioButtonAzure.CheckedChanged += new System.EventHandler(this.RadioButton_CheckedChanged);
            // 
            // radioButtonCustom
            // 
            this.radioButtonCustom.AutoSize = true;
            this.radioButtonCustom.Location = new System.Drawing.Point(20, 150);
            this.radioButtonCustom.Name = "radioButtonCustom";
            this.radioButtonCustom.Size = new System.Drawing.Size(99, 19);
            this.radioButtonCustom.TabIndex = 2;
            this.radioButtonCustom.TabStop = true;
            this.radioButtonCustom.Text = "カスタムURL";
            this.radioButtonCustom.UseVisualStyleBackColor = true;
            this.radioButtonCustom.CheckedChanged += new System.EventHandler(this.RadioButton_CheckedChanged);
            // 
            // textBoxCustomUrl
            // 
            this.textBoxCustomUrl.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.textBoxCustomUrl.Location = new System.Drawing.Point(40, 180);
            this.textBoxCustomUrl.Name = "textBoxCustomUrl";
            this.textBoxCustomUrl.Size = new System.Drawing.Size(500, 22);
            this.textBoxCustomUrl.TabIndex = 3;
            this.textBoxCustomUrl.Text = "https://";
            // 
            // labelLocalUrl
            // 
            this.labelLocalUrl.AutoSize = true;
            this.labelLocalUrl.Font = new System.Drawing.Font("Consolas", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.labelLocalUrl.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.labelLocalUrl.Location = new System.Drawing.Point(40, 55);
            this.labelLocalUrl.Name = "labelLocalUrl";
            this.labelLocalUrl.Size = new System.Drawing.Size(355, 13);
            this.labelLocalUrl.TabIndex = 4;
            this.labelLocalUrl.Text = "https://localhost:7115/api/TravelExpenses";
            // 
            // labelAzureUrl
            // 
            this.labelAzureUrl.AutoSize = true;
            this.labelAzureUrl.Font = new System.Drawing.Font("Consolas", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.labelAzureUrl.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.labelAzureUrl.Location = new System.Drawing.Point(40, 115);
            this.labelAzureUrl.Name = "labelAzureUrl";
            this.labelAzureUrl.Size = new System.Drawing.Size(493, 13);
            this.labelAzureUrl.TabIndex = 5;
            this.labelAzureUrl.Text = "https://app-20251120-api.azurewebsites.net/api/TravelExpenses";
            // 
            // buttonSave
            // 
            this.buttonSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(212)))));
            this.buttonSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonSave.Font = new System.Drawing.Font("Yu Gothic UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.buttonSave.ForeColor = System.Drawing.Color.White;
            this.buttonSave.Location = new System.Drawing.Point(380, 370);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(90, 35);
            this.buttonSave.TabIndex = 2;
            this.buttonSave.Text = "保存";
            this.buttonSave.UseVisualStyleBackColor = false;
            this.buttonSave.Click += new System.EventHandler(this.ButtonSave_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonCancel.Location = new System.Drawing.Point(490, 370);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(90, 35);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "キャンセル";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // labelCurrentUrl
            // 
            this.labelCurrentUrl.AutoSize = true;
            this.labelCurrentUrl.Font = new System.Drawing.Font("Yu Gothic UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.labelCurrentUrl.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.labelCurrentUrl.Location = new System.Drawing.Point(20, 320);
            this.labelCurrentUrl.Name = "labelCurrentUrl";
            this.labelCurrentUrl.Size = new System.Drawing.Size(100, 13);
            this.labelCurrentUrl.TabIndex = 4;
            this.labelCurrentUrl.Text = "現在の接続先: なし";
            // 
            // SettingsForm
            // 
            this.AcceptButton = this.buttonSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(600, 420);
            this.Controls.Add(this.labelCurrentUrl);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.groupBoxEndpoint);
            this.Controls.Add(this.labelTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "設定";
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            this.groupBoxEndpoint.ResumeLayout(false);
            this.groupBoxEndpoint.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.GroupBox groupBoxEndpoint;
        private System.Windows.Forms.RadioButton radioButtonLocal;
        private System.Windows.Forms.RadioButton radioButtonAzure;
        private System.Windows.Forms.RadioButton radioButtonCustom;
        private System.Windows.Forms.TextBox textBoxCustomUrl;
        private System.Windows.Forms.Label labelLocalUrl;
        private System.Windows.Forms.Label labelAzureUrl;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label labelCurrentUrl;
    }
}
