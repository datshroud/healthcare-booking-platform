namespace BookingCareManagement.WinForms
{
    partial class Customer
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private Panel headerPanel;
        private Panel contentPanel;
        private DataGridView customersDataGridView;
        private RoundedButton exportBtn;
        private RoundedButton importBtn;
        private RoundedButton addBtn;
        private Label title;
        private Panel whitePanel;
        private Panel searchPanel;
        private Label searchIcon;
        private TextBox searchBox;
        private Panel searchUnderline;

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
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            headerPanel = new Panel();
            title = new Label();
            exportBtn = new RoundedButton();
            importBtn = new RoundedButton();
            addBtn = new RoundedButton();
            contentPanel = new Panel();
            whitePanel = new Panel();
            customersDataGridView = new DataGridView();
            searchPanel = new Panel();
            searchBox = new TextBox();
            searchUnderline = new Panel();
            searchIcon = new Label();
            headerPanel.SuspendLayout();
            contentPanel.SuspendLayout();
            whitePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)customersDataGridView).BeginInit();
            searchPanel.SuspendLayout();
            SuspendLayout();
            // 
            // headerPanel
            // 
            headerPanel.BackColor = Color.FromArgb(243, 244, 246);
            headerPanel.Controls.Add(title);
            headerPanel.Controls.Add(exportBtn);
            headerPanel.Controls.Add(importBtn);
            headerPanel.Controls.Add(addBtn);
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Location = new Point(0, 0);
            headerPanel.Margin = new Padding(3, 4, 3, 4);
            headerPanel.Name = "headerPanel";
            headerPanel.Padding = new Padding(30, 20, 30, 20);
            headerPanel.Size = new Size(1382, 80);
            headerPanel.TabIndex = 0;
            // 
            // title
            // 
            title.AutoSize = true;
            title.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
            title.ForeColor = Color.FromArgb(17, 24, 39);
            title.Location = new Point(30, 20);
            title.Name = "title";
            title.Size = new Size(287, 54);
            title.TabIndex = 0;
            title.Text = "Khách hàng (1)";
            // 
            // exportBtn
            // 
            exportBtn.BackColor = Color.White;
            exportBtn.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            exportBtn.FlatStyle = FlatStyle.Flat;
            exportBtn.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            exportBtn.ForeColor = Color.FromArgb(55, 65, 81);
            exportBtn.Location = new Point(900, 18);
            exportBtn.Margin = new Padding(3, 4, 3, 4);
            exportBtn.Name = "exportBtn";
            exportBtn.Size = new Size(140, 44);
            exportBtn.TabIndex = 1;
            exportBtn.Text = "⬇  Xuất dữ liệu";
            exportBtn.UseVisualStyleBackColor = false;
            // 
            // importBtn
            // 
            importBtn.BackColor = Color.White;
            importBtn.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            importBtn.FlatStyle = FlatStyle.Flat;
            importBtn.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            importBtn.ForeColor = Color.FromArgb(55, 65, 81);
            importBtn.Location = new Point(1050, 18);
            importBtn.Margin = new Padding(3, 4, 3, 4);
            importBtn.Name = "importBtn";
            importBtn.Size = new Size(140, 44);
            importBtn.TabIndex = 2;
            importBtn.Text = "📄  Thên dữ liệu";
            importBtn.UseVisualStyleBackColor = false;
            importBtn.Click += importBtn_Click;
            // 
            // addBtn
            // 
            addBtn.BackColor = Color.FromArgb(37, 99, 235);
            addBtn.FlatAppearance.BorderSize = 0;
            addBtn.FlatStyle = FlatStyle.Flat;
            addBtn.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            addBtn.ForeColor = Color.White;
            addBtn.Location = new Point(1200, 18);
            addBtn.Margin = new Padding(3, 4, 3, 4);
            addBtn.Name = "addBtn";
            addBtn.Size = new Size(180, 44);
            addBtn.TabIndex = 3;
            addBtn.Text = "+  Thêm khách hàng";
            addBtn.UseVisualStyleBackColor = false;
            addBtn.Click += addBtn_Click;
            // 
            // contentPanel
            // 
            contentPanel.BackColor = Color.FromArgb(243, 244, 246);
            contentPanel.Controls.Add(whitePanel);
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.Location = new Point(0, 80);
            contentPanel.Margin = new Padding(3, 4, 3, 4);
            contentPanel.Name = "contentPanel";
            contentPanel.Padding = new Padding(30, 10, 30, 50);
            contentPanel.Size = new Size(1382, 673);
            contentPanel.TabIndex = 1;
            // 
            // whitePanel
            // 
            whitePanel.BackColor = Color.FromArgb(243, 244, 246);
            whitePanel.Controls.Add(customersDataGridView);
            whitePanel.Controls.Add(searchPanel);
            whitePanel.Dock = DockStyle.Fill;
            whitePanel.Location = new Point(30, 10);
            whitePanel.Margin = new Padding(3, 4, 3, 4);
            whitePanel.Name = "whitePanel";
            whitePanel.Padding = new Padding(30, 10, 30, 50);
            whitePanel.Size = new Size(1322, 613);
            whitePanel.TabIndex = 0;
            // 
            // customersDataGridView
            // 
            customersDataGridView.AllowUserToAddRows = false;
            customersDataGridView.AllowUserToDeleteRows = false;
            customersDataGridView.AllowUserToResizeRows = false;
            customersDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            customersDataGridView.BackgroundColor = Color.White;
            customersDataGridView.BorderStyle = BorderStyle.None;
            customersDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            customersDataGridView.ColumnHeadersHeight = 50;
            customersDataGridView.Dock = DockStyle.Fill;
            customersDataGridView.Font = new Font("Segoe UI", 12F);
            customersDataGridView.GridColor = Color.FromArgb(243, 244, 246);
            customersDataGridView.Location = new Point(30, 70);
            customersDataGridView.Margin = new Padding(3, 4, 3, 4);
            customersDataGridView.MultiSelect = false;
            customersDataGridView.Name = "customersDataGridView";
            customersDataGridView.RowHeadersVisible = false;
            customersDataGridView.RowHeadersWidth = 51;
            customersDataGridView.RowTemplate.Height = 70;
            customersDataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            customersDataGridView.Size = new Size(1262, 493);
            customersDataGridView.TabIndex = 1;
            // 
            // searchPanel
            // 
            searchPanel.BackColor = Color.White;
            searchPanel.Controls.Add(searchBox);
            searchPanel.Controls.Add(searchUnderline);
            searchPanel.Controls.Add(searchIcon);
            searchPanel.Dock = DockStyle.Top;
            searchPanel.Location = new Point(30, 10);
            searchPanel.Margin = new Padding(3, 4, 3, 4);
            searchPanel.Name = "searchPanel";
            searchPanel.Size = new Size(1262, 60);
            searchPanel.TabIndex = 0;
            // 
            // searchBox
            // 
            searchBox.BorderStyle = BorderStyle.None;
            searchBox.Font = new Font("Segoe UI", 11F);
            searchBox.ForeColor = Color.Gray;
            searchBox.Location = new Point(55, 20);
            searchBox.Margin = new Padding(3, 4, 3, 4);
            searchBox.Name = "searchBox";
            searchBox.Size = new Size(300, 25);
            searchBox.TabIndex = 1;
            searchBox.Text = "Search";
            // 
            // searchUnderline
            // 
            searchUnderline.BackColor = Color.FromArgb(229, 231, 235);
            searchUnderline.Location = new Point(50, 55);
            searchUnderline.Margin = new Padding(3, 4, 3, 4);
            searchUnderline.Name = "searchUnderline";
            searchUnderline.Size = new Size(340, 1);
            searchUnderline.TabIndex = 2;
            // 
            // searchIcon
            // 
            searchIcon.AutoSize = true;
            searchIcon.Font = new Font("Segoe UI", 12F);
            searchIcon.Location = new Point(20, 25);
            searchIcon.Name = "searchIcon";
            searchIcon.Size = new Size(39, 28);
            searchIcon.TabIndex = 0;
            searchIcon.Text = "🔍";
            // 
            // Customer
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(243, 244, 246);
            ClientSize = new Size(1382, 753);
            Controls.Add(contentPanel);
            Controls.Add(headerPanel);
            Name = "Customer";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Customer";
            headerPanel.ResumeLayout(false);
            headerPanel.PerformLayout();
            contentPanel.ResumeLayout(false);
            whitePanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)customersDataGridView).EndInit();
            searchPanel.ResumeLayout(false);
            searchPanel.PerformLayout();
            ResumeLayout(false);

        }

        #endregion
    }
}