namespace TravelExpenseClient
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            groupBoxSummary = new GroupBox();
            labelTotalAmount = new Label();
            labelRejected = new Label();
            labelApproved = new Label();
            labelPending = new Label();
            labelTotal = new Label();
            groupBoxList = new GroupBox();
            dataGridViewExpenses = new DataGridView();
            buttonRefresh = new Button();
            buttonDelete = new Button();
            buttonEdit = new Button();
            buttonAdd = new Button();
            groupBoxDetails = new GroupBox();
            textBoxRemarks = new TextBox();
            label11 = new Label();
            numericUpDownOther = new NumericUpDown();
            label10 = new Label();
            numericUpDownMeal = new NumericUpDown();
            label9 = new Label();
            numericUpDownAccommodation = new NumericUpDown();
            label8 = new Label();
            numericUpDownTransportation = new NumericUpDown();
            label7 = new Label();
            comboBoxTransportation = new ComboBox();
            label6 = new Label();
            textBoxPurpose = new TextBox();
            label5 = new Label();
            textBoxDestination = new TextBox();
            label4 = new Label();
            dateTimePickerTravel = new DateTimePicker();
            label3 = new Label();
            textBoxApplicant = new TextBox();
            label2 = new Label();
            buttonSave = new Button();
            buttonCancel = new Button();
            labelTotalCost = new Label();
            label1 = new Label();
            groupBoxSummary.SuspendLayout();
            groupBoxList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridViewExpenses).BeginInit();
            groupBoxDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDownOther).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownMeal).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownAccommodation).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownTransportation).BeginInit();
            SuspendLayout();
            // 
            // groupBoxSummary
            // 
            groupBoxSummary.Controls.Add(labelTotalAmount);
            groupBoxSummary.Controls.Add(labelRejected);
            groupBoxSummary.Controls.Add(labelApproved);
            groupBoxSummary.Controls.Add(labelPending);
            groupBoxSummary.Controls.Add(labelTotal);
            groupBoxSummary.Location = new Point(12, 12);
            groupBoxSummary.Name = "groupBoxSummary";
            groupBoxSummary.Size = new Size(1160, 80);
            groupBoxSummary.TabIndex = 0;
            groupBoxSummary.TabStop = false;
            groupBoxSummary.Text = "サマリー";
            // 
            // labelTotalAmount
            // 
            labelTotalAmount.AutoSize = true;
            labelTotalAmount.Font = new Font("Yu Gothic UI", 10F);
            labelTotalAmount.Location = new Point(820, 30);
            labelTotalAmount.Name = "labelTotalAmount";
            labelTotalAmount.Size = new Size(126, 28);
            labelTotalAmount.TabIndex = 4;
            labelTotalAmount.Text = "総費用: 0 円";
            // 
            // labelRejected
            // 
            labelRejected.AutoSize = true;
            labelRejected.Font = new Font("Yu Gothic UI", 10F);
            labelRejected.Location = new Point(620, 30);
            labelRejected.Name = "labelRejected";
            labelRejected.Size = new Size(103, 28);
            labelRejected.TabIndex = 3;
            labelRejected.Text = "却下: 0 件";
            // 
            // labelApproved
            // 
            labelApproved.AutoSize = true;
            labelApproved.Font = new Font("Yu Gothic UI", 10F);
            labelApproved.Location = new Point(420, 30);
            labelApproved.Name = "labelApproved";
            labelApproved.Size = new Size(141, 28);
            labelApproved.TabIndex = 2;
            labelApproved.Text = "承認済み: 0 件";
            // 
            // labelPending
            // 
            labelPending.AutoSize = true;
            labelPending.Font = new Font("Yu Gothic UI", 10F);
            labelPending.Location = new Point(220, 30);
            labelPending.Name = "labelPending";
            labelPending.Size = new Size(141, 28);
            labelPending.TabIndex = 1;
            labelPending.Text = "承認待ち: 0 件";
            // 
            // labelTotal
            // 
            labelTotal.AutoSize = true;
            labelTotal.Font = new Font("Yu Gothic UI", 10F);
            labelTotal.Location = new Point(20, 30);
            labelTotal.Name = "labelTotal";
            labelTotal.Size = new Size(141, 28);
            labelTotal.TabIndex = 0;
            labelTotal.Text = "総申請数: 0 件";
            // 
            // groupBoxList
            // 
            groupBoxList.Controls.Add(dataGridViewExpenses);
            groupBoxList.Controls.Add(buttonRefresh);
            groupBoxList.Controls.Add(buttonDelete);
            groupBoxList.Controls.Add(buttonEdit);
            groupBoxList.Controls.Add(buttonAdd);
            groupBoxList.Location = new Point(12, 98);
            groupBoxList.Name = "groupBoxList";
            groupBoxList.Size = new Size(1160, 350);
            groupBoxList.TabIndex = 1;
            groupBoxList.TabStop = false;
            groupBoxList.Text = "旅費精算一覧";
            // 
            // dataGridViewExpenses
            // 
            dataGridViewExpenses.AllowUserToAddRows = false;
            dataGridViewExpenses.AllowUserToDeleteRows = false;
            dataGridViewExpenses.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewExpenses.Location = new Point(20, 30);
            dataGridViewExpenses.MultiSelect = false;
            dataGridViewExpenses.Name = "dataGridViewExpenses";
            dataGridViewExpenses.ReadOnly = true;
            dataGridViewExpenses.RowHeadersWidth = 62;
            dataGridViewExpenses.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewExpenses.Size = new Size(1120, 250);
            dataGridViewExpenses.TabIndex = 0;
            // 
            // buttonRefresh
            // 
            buttonRefresh.Location = new Point(950, 295);
            buttonRefresh.Name = "buttonRefresh";
            buttonRefresh.Size = new Size(190, 35);
            buttonRefresh.TabIndex = 4;
            buttonRefresh.Text = "更新";
            buttonRefresh.UseVisualStyleBackColor = true;
            buttonRefresh.Click += ButtonRefresh_Click;
            // 
            // buttonDelete
            // 
            buttonDelete.Location = new Point(420, 295);
            buttonDelete.Name = "buttonDelete";
            buttonDelete.Size = new Size(190, 35);
            buttonDelete.TabIndex = 3;
            buttonDelete.Text = "削除";
            buttonDelete.UseVisualStyleBackColor = true;
            buttonDelete.Click += ButtonDelete_Click;
            // 
            // buttonEdit
            // 
            buttonEdit.Location = new Point(220, 295);
            buttonEdit.Name = "buttonEdit";
            buttonEdit.Size = new Size(190, 35);
            buttonEdit.TabIndex = 2;
            buttonEdit.Text = "編集";
            buttonEdit.UseVisualStyleBackColor = true;
            buttonEdit.Click += ButtonEdit_Click;
            // 
            // buttonAdd
            // 
            buttonAdd.Location = new Point(20, 295);
            buttonAdd.Name = "buttonAdd";
            buttonAdd.Size = new Size(190, 35);
            buttonAdd.TabIndex = 1;
            buttonAdd.Text = "新規追加";
            buttonAdd.UseVisualStyleBackColor = true;
            buttonAdd.Click += ButtonAdd_Click;
            // 
            // groupBoxDetails
            // 
            groupBoxDetails.Controls.Add(labelTotalCost);
            groupBoxDetails.Controls.Add(label1);
            groupBoxDetails.Controls.Add(buttonCancel);
            groupBoxDetails.Controls.Add(buttonSave);
            groupBoxDetails.Controls.Add(textBoxRemarks);
            groupBoxDetails.Controls.Add(label11);
            groupBoxDetails.Controls.Add(numericUpDownOther);
            groupBoxDetails.Controls.Add(label10);
            groupBoxDetails.Controls.Add(numericUpDownMeal);
            groupBoxDetails.Controls.Add(label9);
            groupBoxDetails.Controls.Add(numericUpDownAccommodation);
            groupBoxDetails.Controls.Add(label8);
            groupBoxDetails.Controls.Add(numericUpDownTransportation);
            groupBoxDetails.Controls.Add(label7);
            groupBoxDetails.Controls.Add(comboBoxTransportation);
            groupBoxDetails.Controls.Add(label6);
            groupBoxDetails.Controls.Add(textBoxPurpose);
            groupBoxDetails.Controls.Add(label5);
            groupBoxDetails.Controls.Add(textBoxDestination);
            groupBoxDetails.Controls.Add(label4);
            groupBoxDetails.Controls.Add(dateTimePickerTravel);
            groupBoxDetails.Controls.Add(label3);
            groupBoxDetails.Controls.Add(textBoxApplicant);
            groupBoxDetails.Controls.Add(label2);
            groupBoxDetails.Location = new Point(12, 454);
            groupBoxDetails.Name = "groupBoxDetails";
            groupBoxDetails.Size = new Size(1160, 380);
            groupBoxDetails.TabIndex = 2;
            groupBoxDetails.TabStop = false;
            groupBoxDetails.Text = "詳細情報";
            groupBoxDetails.Visible = false;
            // 
            // textBoxRemarks
            // 
            textBoxRemarks.Location = new Point(800, 80);
            textBoxRemarks.Multiline = true;
            textBoxRemarks.Name = "textBoxRemarks";
            textBoxRemarks.Size = new Size(340, 180);
            textBoxRemarks.TabIndex = 21;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(800, 50);
            label11.Name = "label11";
            label11.Size = new Size(51, 25);
            label11.TabIndex = 20;
            label11.Text = "備考";
            // 
            // numericUpDownOther
            // 
            numericUpDownOther.Location = new Point(530, 235);
            numericUpDownOther.Maximum = new decimal(new int[] { 10000000, 0, 0, 0 });
            numericUpDownOther.Name = "numericUpDownOther";
            numericUpDownOther.Size = new Size(240, 31);
            numericUpDownOther.TabIndex = 19;
            numericUpDownOther.ValueChanged += NumericUpDown_ValueChanged;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(420, 240);
            label10.Name = "label10";
            label10.Size = new Size(93, 25);
            label10.TabIndex = 18;
            label10.Text = "その他 (円)";
            // 
            // numericUpDownMeal
            // 
            numericUpDownMeal.Location = new Point(530, 190);
            numericUpDownMeal.Maximum = new decimal(new int[] { 10000000, 0, 0, 0 });
            numericUpDownMeal.Name = "numericUpDownMeal";
            numericUpDownMeal.Size = new Size(240, 31);
            numericUpDownMeal.TabIndex = 17;
            numericUpDownMeal.ValueChanged += NumericUpDown_ValueChanged;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(420, 195);
            label9.Name = "label9";
            label9.Size = new Size(93, 25);
            label9.TabIndex = 16;
            label9.Text = "食事代 (円)";
            // 
            // numericUpDownAccommodation
            // 
            numericUpDownAccommodation.Location = new Point(530, 145);
            numericUpDownAccommodation.Maximum = new decimal(new int[] { 10000000, 0, 0, 0 });
            numericUpDownAccommodation.Name = "numericUpDownAccommodation";
            numericUpDownAccommodation.Size = new Size(240, 31);
            numericUpDownAccommodation.TabIndex = 15;
            numericUpDownAccommodation.ValueChanged += NumericUpDown_ValueChanged;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(420, 150);
            label8.Name = "label8";
            label8.Size = new Size(93, 25);
            label8.TabIndex = 14;
            label8.Text = "宿泊費 (円)";
            // 
            // numericUpDownTransportation
            // 
            numericUpDownTransportation.Location = new Point(530, 100);
            numericUpDownTransportation.Maximum = new decimal(new int[] { 10000000, 0, 0, 0 });
            numericUpDownTransportation.Name = "numericUpDownTransportation";
            numericUpDownTransportation.Size = new Size(240, 31);
            numericUpDownTransportation.TabIndex = 13;
            numericUpDownTransportation.ValueChanged += NumericUpDown_ValueChanged;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(420, 105);
            label7.Name = "label7";
            label7.Size = new Size(93, 25);
            label7.TabIndex = 12;
            label7.Text = "交通費 (円)";
            // 
            // comboBoxTransportation
            // 
            comboBoxTransportation.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxTransportation.FormattingEnabled = true;
            comboBoxTransportation.Items.AddRange(new object[] { "電車", "新幹線", "飛行機", "バス", "タクシー", "自家用車", "その他" });
            comboBoxTransportation.Location = new Point(530, 55);
            comboBoxTransportation.Name = "comboBoxTransportation";
            comboBoxTransportation.Size = new Size(240, 33);
            comboBoxTransportation.TabIndex = 11;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(420, 60);
            label6.Name = "label6";
            label6.Size = new Size(84, 25);
            label6.TabIndex = 10;
            label6.Text = "交通手段";
            // 
            // textBoxPurpose
            // 
            textBoxPurpose.Location = new Point(150, 235);
            textBoxPurpose.Name = "textBoxPurpose";
            textBoxPurpose.Size = new Size(240, 31);
            textBoxPurpose.TabIndex = 9;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(20, 240);
            label5.Name = "label5";
            label5.Size = new Size(51, 25);
            label5.TabIndex = 8;
            label5.Text = "目的";
            // 
            // textBoxDestination
            // 
            textBoxDestination.Location = new Point(150, 190);
            textBoxDestination.Name = "textBoxDestination";
            textBoxDestination.Size = new Size(240, 31);
            textBoxDestination.TabIndex = 7;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(20, 195);
            label4.Name = "label4";
            label4.Size = new Size(69, 25);
            label4.TabIndex = 6;
            label4.Text = "出張先";
            // 
            // dateTimePickerTravel
            // 
            dateTimePickerTravel.Format = DateTimePickerFormat.Short;
            dateTimePickerTravel.Location = new Point(150, 145);
            dateTimePickerTravel.Name = "dateTimePickerTravel";
            dateTimePickerTravel.Size = new Size(240, 31);
            dateTimePickerTravel.TabIndex = 5;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(20, 150);
            label3.Name = "label3";
            label3.Size = new Size(69, 25);
            label3.TabIndex = 4;
            label3.Text = "出張日";
            // 
            // textBoxApplicant
            // 
            textBoxApplicant.Location = new Point(150, 55);
            textBoxApplicant.Name = "textBoxApplicant";
            textBoxApplicant.Size = new Size(240, 31);
            textBoxApplicant.TabIndex = 1;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(20, 60);
            label2.Name = "label2";
            label2.Size = new Size(84, 25);
            label2.TabIndex = 0;
            label2.Text = "申請者名";
            // 
            // buttonSave
            // 
            buttonSave.Location = new Point(820, 320);
            buttonSave.Name = "buttonSave";
            buttonSave.Size = new Size(150, 40);
            buttonSave.TabIndex = 22;
            buttonSave.Text = "保存";
            buttonSave.UseVisualStyleBackColor = true;
            buttonSave.Click += ButtonSave_Click;
            // 
            // buttonCancel
            // 
            buttonCancel.Location = new Point(990, 320);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(150, 40);
            buttonCancel.TabIndex = 23;
            buttonCancel.Text = "キャンセル";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += ButtonCancel_Click;
            // 
            // labelTotalCost
            // 
            labelTotalCost.AutoSize = true;
            labelTotalCost.Font = new Font("Yu Gothic UI", 12F, FontStyle.Bold);
            labelTotalCost.Location = new Point(150, 330);
            labelTotalCost.Name = "labelTotalCost";
            labelTotalCost.Size = new Size(80, 32);
            labelTotalCost.TabIndex = 25;
            labelTotalCost.Text = "0 円";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Yu Gothic UI", 12F, FontStyle.Bold);
            label1.Location = new Point(20, 330);
            label1.Name = "label1";
            label1.Size = new Size(110, 32);
            label1.TabIndex = 24;
            label1.Text = "合計金額";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1184, 851);
            Controls.Add(groupBoxDetails);
            Controls.Add(groupBoxList);
            Controls.Add(groupBoxSummary);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "Form1";
            Text = "旅費精算管理システム";
            Load += Form1_Load;
            groupBoxSummary.ResumeLayout(false);
            groupBoxSummary.PerformLayout();
            groupBoxList.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridViewExpenses).EndInit();
            groupBoxDetails.ResumeLayout(false);
            groupBoxDetails.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDownOther).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownMeal).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownAccommodation).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownTransportation).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox groupBoxSummary;
        private Label labelTotal;
        private Label labelPending;
        private Label labelApproved;
        private Label labelRejected;
        private Label labelTotalAmount;
        private GroupBox groupBoxList;
        private DataGridView dataGridViewExpenses;
        private Button buttonAdd;
        private Button buttonEdit;
        private Button buttonDelete;
        private Button buttonRefresh;
        private GroupBox groupBoxDetails;
        private Label label2;
        private TextBox textBoxApplicant;
        private Label label3;
        private DateTimePicker dateTimePickerTravel;
        private Label label4;
        private TextBox textBoxDestination;
        private Label label5;
        private TextBox textBoxPurpose;
        private Label label6;
        private ComboBox comboBoxTransportation;
        private Label label7;
        private NumericUpDown numericUpDownTransportation;
        private Label label8;
        private NumericUpDown numericUpDownAccommodation;
        private Label label9;
        private NumericUpDown numericUpDownMeal;
        private Label label10;
        private NumericUpDown numericUpDownOther;
        private Label label11;
        private TextBox textBoxRemarks;
        private Button buttonCancel;
        private Button buttonSave;
        private Label labelTotalCost;
        private Label label1;
    }
}
