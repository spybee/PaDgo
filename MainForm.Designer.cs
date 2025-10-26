namespace PaDgo
{
    partial class MainForm
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置受控資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.SolutionsList = new System.Windows.Forms.ListBox();
            this.GridPanel = new System.Windows.Forms.Panel();
            this.controlsPanel = new System.Windows.Forms.Panel();
            this.ExecutePathButton = new System.Windows.Forms.Button();
            this.statusLabel = new System.Windows.Forms.Label();
            this.ClearButton = new System.Windows.Forms.Button();
            this.GetGameBoardButton = new System.Windows.Forms.Button();
            this.SolveButton = new System.Windows.Forms.Button();
            this.maxLengthInput = new System.Windows.Forms.NumericUpDown();
            this.lengthLabel = new System.Windows.Forms.Label();
            this.allow8DirCheck = new System.Windows.Forms.CheckBox();
            this.profileCombo = new System.Windows.Forms.ComboBox();
            this.profileLabel = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.controlsPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.maxLengthInput)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 370F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.SolutionsList, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.GridPanel, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.controlsPanel, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 310F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(618, 459);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // SolutionsList
            // 
            this.SolutionsList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SolutionsList.Font = new System.Drawing.Font("新細明體", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.SolutionsList.FormattingEnabled = true;
            this.SolutionsList.ItemHeight = 21;
            this.SolutionsList.Location = new System.Drawing.Point(373, 3);
            this.SolutionsList.Name = "SolutionsList";
            this.tableLayoutPanel1.SetRowSpan(this.SolutionsList, 2);
            this.SolutionsList.Size = new System.Drawing.Size(242, 453);
            this.SolutionsList.TabIndex = 5;
            this.SolutionsList.SelectedIndexChanged += new System.EventHandler(this.SolutionsList_SelectedIndexChanged);
            // 
            // GridPanel
            // 
            this.GridPanel.BackColor = System.Drawing.Color.Black;
            this.GridPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GridPanel.Location = new System.Drawing.Point(3, 3);
            this.GridPanel.Name = "GridPanel";
            this.GridPanel.Size = new System.Drawing.Size(364, 304);
            this.GridPanel.TabIndex = 4;
            this.GridPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.GridPanel_Paint);
            this.GridPanel.MouseClick += new System.Windows.Forms.MouseEventHandler(this.GridPanel_MouseClick);
            // 
            // controlsPanel
            // 
            this.controlsPanel.Controls.Add(this.ExecutePathButton);
            this.controlsPanel.Controls.Add(this.statusLabel);
            this.controlsPanel.Controls.Add(this.ClearButton);
            this.controlsPanel.Controls.Add(this.GetGameBoardButton);
            this.controlsPanel.Controls.Add(this.SolveButton);
            this.controlsPanel.Controls.Add(this.maxLengthInput);
            this.controlsPanel.Controls.Add(this.lengthLabel);
            this.controlsPanel.Controls.Add(this.allow8DirCheck);
            this.controlsPanel.Controls.Add(this.profileCombo);
            this.controlsPanel.Controls.Add(this.profileLabel);
            this.controlsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.controlsPanel.Location = new System.Drawing.Point(3, 313);
            this.controlsPanel.Name = "controlsPanel";
            this.controlsPanel.Size = new System.Drawing.Size(364, 143);
            this.controlsPanel.TabIndex = 1;
            // 
            // ExecutePathButton
            // 
            this.ExecutePathButton.Location = new System.Drawing.Point(270, 85);
            this.ExecutePathButton.Name = "ExecutePathButton";
            this.ExecutePathButton.Size = new System.Drawing.Size(85, 30);
            this.ExecutePathButton.TabIndex = 10;
            this.ExecutePathButton.Text = "執行路徑";
            this.ExecutePathButton.UseVisualStyleBackColor = true;
            this.ExecutePathButton.Click += new System.EventHandler(this.ExecutePathButton_Click);
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = true;
            this.statusLabel.ForeColor = System.Drawing.Color.Blue;
            this.statusLabel.Location = new System.Drawing.Point(10, 120);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(29, 12);
            this.statusLabel.TabIndex = 8;
            this.statusLabel.Text = "就绪";
            // 
            // ClearButton
            // 
            this.ClearButton.Location = new System.Drawing.Point(183, 85);
            this.ClearButton.Name = "ClearButton";
            this.ClearButton.Size = new System.Drawing.Size(80, 30);
            this.ClearButton.TabIndex = 7;
            this.ClearButton.Text = "清空";
            this.ClearButton.UseVisualStyleBackColor = true;
            this.ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
            // 
            // GetGameBoardButton
            // 
            this.GetGameBoardButton.Location = new System.Drawing.Point(96, 85);
            this.GetGameBoardButton.Name = "GetGameBoardButton";
            this.GetGameBoardButton.Size = new System.Drawing.Size(80, 30);
            this.GetGameBoardButton.TabIndex = 6;
            this.GetGameBoardButton.Text = "取得現況";
            this.GetGameBoardButton.UseVisualStyleBackColor = true;
            this.GetGameBoardButton.Click += new System.EventHandler(this.GetGameBoardButton_Click);
            // 
            // SolveButton
            // 
            this.SolveButton.BackColor = System.Drawing.Color.LightGreen;
            this.SolveButton.Location = new System.Drawing.Point(10, 85);
            this.SolveButton.Name = "SolveButton";
            this.SolveButton.Size = new System.Drawing.Size(80, 30);
            this.SolveButton.TabIndex = 5;
            this.SolveButton.Text = "開始求解";
            this.SolveButton.UseVisualStyleBackColor = false;
            this.SolveButton.Click += new System.EventHandler(this.SolveButton_Click);
            // 
            // maxLengthInput
            // 
            this.maxLengthInput.Location = new System.Drawing.Point(80, 58);
            this.maxLengthInput.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.maxLengthInput.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.maxLengthInput.Name = "maxLengthInput";
            this.maxLengthInput.Size = new System.Drawing.Size(50, 22);
            this.maxLengthInput.TabIndex = 4;
            this.maxLengthInput.Value = new decimal(new int[] {
            16,
            0,
            0,
            0});
            // 
            // lengthLabel
            // 
            this.lengthLabel.AutoSize = true;
            this.lengthLabel.Location = new System.Drawing.Point(10, 60);
            this.lengthLabel.Name = "lengthLabel";
            this.lengthLabel.Size = new System.Drawing.Size(56, 12);
            this.lengthLabel.TabIndex = 3;
            this.lengthLabel.Text = "最大長度:";
            // 
            // allow8DirCheck
            // 
            this.allow8DirCheck.AutoSize = true;
            this.allow8DirCheck.Location = new System.Drawing.Point(10, 35);
            this.allow8DirCheck.Name = "allow8DirCheck";
            this.allow8DirCheck.Size = new System.Drawing.Size(102, 16);
            this.allow8DirCheck.TabIndex = 2;
            this.allow8DirCheck.Text = "允許8方向移動";
            this.allow8DirCheck.UseVisualStyleBackColor = true;
            // 
            // profileCombo
            // 
            this.profileCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.profileCombo.FormattingEnabled = true;
            this.profileCombo.Location = new System.Drawing.Point(50, 8);
            this.profileCombo.Name = "profileCombo";
            this.profileCombo.Size = new System.Drawing.Size(304, 20);
            this.profileCombo.TabIndex = 1;
            // 
            // profileLabel
            // 
            this.profileLabel.AutoSize = true;
            this.profileLabel.Location = new System.Drawing.Point(10, 10);
            this.profileLabel.Name = "profileLabel";
            this.profileLabel.Size = new System.Drawing.Size(32, 12);
            this.profileLabel.TabIndex = 0;
            this.profileLabel.Text = "配置:";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(618, 459);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "MainForm";
            this.Text = "P & D 0 自動化";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.controlsPanel.ResumeLayout(false);
            this.controlsPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.maxLengthInput)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel controlsPanel;
        private System.Windows.Forms.Label profileLabel;
        private System.Windows.Forms.ComboBox profileCombo;
        private System.Windows.Forms.CheckBox allow8DirCheck;
        private System.Windows.Forms.Label lengthLabel;
        private System.Windows.Forms.NumericUpDown maxLengthInput;
        private System.Windows.Forms.Button SolveButton;
        private System.Windows.Forms.Button GetGameBoardButton;
        private System.Windows.Forms.Button ClearButton;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.ListBox SolutionsList;
        private System.Windows.Forms.Panel GridPanel;
        private System.Windows.Forms.Button ExecutePathButton;
    }
}

