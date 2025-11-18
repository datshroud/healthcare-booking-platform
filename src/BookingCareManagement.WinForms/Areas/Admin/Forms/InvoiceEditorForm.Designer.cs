namespace BookingCareManagement.WinForms.Areas.Admin.Forms
{
    partial class InvoiceEditorForm
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
            headerPanel = new Panel();
            lblTitle = new Label();
            contentPanel = new Panel();
            whitePanel = new Panel();
            invoiceGrid = new DataGridView();
            filterContainerPanel = new Panel();
            filterPanel = new Panel();
            btnStatusFilter = new Button();
            btnServiceFilter = new Button();
            btnEmployeeFilter = new Button();
            btnCustomerFilter = new Button();
            searchPanel = new Panel();
            btnFilter = new Button();
            txtSearch = new TextBox();
            headerPanel.SuspendLayout();
            contentPanel.SuspendLayout();
            whitePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)invoiceGrid).BeginInit();
            filterContainerPanel.SuspendLayout();
            filterPanel.SuspendLayout();
            searchPanel.SuspendLayout();
            SuspendLayout();
            // 
            // headerPanel
            // 
            headerPanel.BackColor = Color.FromArgb(243, 244, 246);
            headerPanel.Controls.Add(lblTitle);
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Location = new Point(0, 0);
            headerPanel.Margin = new Padding(3, 4, 3, 4);
            headerPanel.Name = "headerPanel";
            headerPanel.Padding = new Padding(34, 27, 34, 27);
            headerPanel.Size = new Size(1600, 107);
            headerPanel.TabIndex = 0;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(17, 24, 39);
            lblTitle.Location = new Point(34, 27);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(172, 54);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Hóa Đơn";
            // 
            // contentPanel
            // 
            contentPanel.BackColor = Color.FromArgb(243, 244, 246);
            contentPanel.Controls.Add(whitePanel);
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.Location = new Point(0, 107);
            contentPanel.Margin = new Padding(3, 4, 3, 4);
            contentPanel.Name = "contentPanel";
            contentPanel.Padding = new Padding(34, 13, 34, 1333);
            contentPanel.Size = new Size(1600, 948);
            contentPanel.TabIndex = 1;
            // 
            // whitePanel
            // 
            whitePanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            whitePanel.BackColor = Color.White;
            whitePanel.Controls.Add(invoiceGrid);
            whitePanel.Controls.Add(filterContainerPanel);
            whitePanel.Controls.Add(searchPanel);
            whitePanel.Location = new Point(34, 13);
            whitePanel.Margin = new Padding(3, 4, 3, 4);
            whitePanel.Name = "whitePanel";
            whitePanel.Padding = new Padding(34, 13, 34, 67);
            whitePanel.Size = new Size(1531, 921);
            whitePanel.TabIndex = 0;
            // 
            // invoiceGrid
            // 
            invoiceGrid.AllowUserToAddRows = false;
            invoiceGrid.AllowUserToDeleteRows = false;
            invoiceGrid.AllowUserToResizeRows = false;
            invoiceGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            invoiceGrid.BackgroundColor = Color.White;
            invoiceGrid.BorderStyle = BorderStyle.None;
            invoiceGrid.ColumnHeadersHeight = 50;
            invoiceGrid.Dock = DockStyle.Fill;
            invoiceGrid.GridColor = Color.FromArgb(243, 244, 246);
            invoiceGrid.Location = new Point(34, 199);
            invoiceGrid.Margin = new Padding(3, 4, 3, 4);
            invoiceGrid.MultiSelect = false;
            invoiceGrid.Name = "invoiceGrid";
            invoiceGrid.ReadOnly = true;
            invoiceGrid.RowHeadersVisible = false;
            invoiceGrid.RowHeadersWidth = 51;
            invoiceGrid.RowTemplate.Height = 60;
            invoiceGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            invoiceGrid.Size = new Size(1463, 655);
            invoiceGrid.TabIndex = 1;
            // 
            // filterContainerPanel
            // 
            filterContainerPanel.BackColor = Color.White;
            filterContainerPanel.Controls.Add(filterPanel);
            filterContainerPanel.Dock = DockStyle.Top;
            filterContainerPanel.Location = new Point(34, 106);
            filterContainerPanel.Margin = new Padding(3, 4, 3, 4);
            filterContainerPanel.Name = "filterContainerPanel";
            filterContainerPanel.Size = new Size(1463, 93);
            filterContainerPanel.TabIndex = 2;
            filterContainerPanel.Visible = false;
            // 
            // filterPanel
            // 
            filterPanel.BackColor = Color.FromArgb(249, 250, 251);
            filterPanel.Controls.Add(btnStatusFilter);
            filterPanel.Controls.Add(btnServiceFilter);
            filterPanel.Controls.Add(btnEmployeeFilter);
            filterPanel.Controls.Add(btnCustomerFilter);
            filterPanel.Dock = DockStyle.Fill;
            filterPanel.Location = new Point(0, 0);
            filterPanel.Margin = new Padding(3, 4, 3, 4);
            filterPanel.Name = "filterPanel";
            filterPanel.Padding = new Padding(11, 13, 11, 13);
            filterPanel.Size = new Size(1463, 93);
            filterPanel.TabIndex = 0;
            // 
            // btnStatusFilter
            // 
            btnStatusFilter.BackColor = Color.White;
            btnStatusFilter.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            btnStatusFilter.FlatStyle = FlatStyle.Flat;
            btnStatusFilter.Font = new Font("Segoe UI", 10F);
            btnStatusFilter.Location = new Point(594, 20);
            btnStatusFilter.Margin = new Padding(3, 4, 3, 4);
            btnStatusFilter.Name = "btnStatusFilter";
            btnStatusFilter.Padding = new Padding(11, 0, 6, 0);
            btnStatusFilter.Size = new Size(171, 53);
            btnStatusFilter.TabIndex = 3;
            btnStatusFilter.Text = "⚪ Trạng thái";
            btnStatusFilter.TextAlign = ContentAlignment.MiddleLeft;
            btnStatusFilter.UseVisualStyleBackColor = false;
            // 
            // btnServiceFilter
            // 
            btnServiceFilter.BackColor = Color.White;
            btnServiceFilter.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            btnServiceFilter.FlatStyle = FlatStyle.Flat;
            btnServiceFilter.Font = new Font("Segoe UI", 10F);
            btnServiceFilter.Location = new Point(400, 20);
            btnServiceFilter.Margin = new Padding(3, 4, 3, 4);
            btnServiceFilter.Name = "btnServiceFilter";
            btnServiceFilter.Padding = new Padding(11, 0, 6, 0);
            btnServiceFilter.Size = new Size(171, 53);
            btnServiceFilter.TabIndex = 2;
            btnServiceFilter.Text = "💼 Dịch vụ";
            btnServiceFilter.TextAlign = ContentAlignment.MiddleLeft;
            btnServiceFilter.UseVisualStyleBackColor = false;
            // 
            // btnEmployeeFilter
            // 
            btnEmployeeFilter.BackColor = Color.White;
            btnEmployeeFilter.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            btnEmployeeFilter.FlatStyle = FlatStyle.Flat;
            btnEmployeeFilter.Font = new Font("Segoe UI", 10F);
            btnEmployeeFilter.Location = new Point(206, 20);
            btnEmployeeFilter.Margin = new Padding(3, 4, 3, 4);
            btnEmployeeFilter.Name = "btnEmployeeFilter";
            btnEmployeeFilter.Padding = new Padding(11, 0, 6, 0);
            btnEmployeeFilter.Size = new Size(171, 53);
            btnEmployeeFilter.TabIndex = 1;
            btnEmployeeFilter.Text = "✏️ Nhân viên";
            btnEmployeeFilter.TextAlign = ContentAlignment.MiddleLeft;
            btnEmployeeFilter.UseVisualStyleBackColor = false;
            // 
            // btnCustomerFilter
            // 
            btnCustomerFilter.BackColor = Color.White;
            btnCustomerFilter.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            btnCustomerFilter.FlatStyle = FlatStyle.Flat;
            btnCustomerFilter.Font = new Font("Segoe UI", 10F);
            btnCustomerFilter.ForeColor = Color.Black;
            btnCustomerFilter.Location = new Point(11, 20);
            btnCustomerFilter.Margin = new Padding(3, 4, 3, 4);
            btnCustomerFilter.Name = "btnCustomerFilter";
            btnCustomerFilter.Padding = new Padding(11, 0, 6, 0);
            btnCustomerFilter.Size = new Size(171, 53);
            btnCustomerFilter.TabIndex = 0;
            btnCustomerFilter.Text = "👥 Khách hàng";
            btnCustomerFilter.TextAlign = ContentAlignment.MiddleLeft;
            btnCustomerFilter.UseVisualStyleBackColor = false;
            // 
            // searchPanel
            // 
            searchPanel.BackColor = Color.White;
            searchPanel.Controls.Add(btnFilter);
            searchPanel.Controls.Add(txtSearch);
            searchPanel.Dock = DockStyle.Top;
            searchPanel.Location = new Point(34, 13);
            searchPanel.Margin = new Padding(3, 4, 3, 4);
            searchPanel.Name = "searchPanel";
            searchPanel.Size = new Size(1463, 93);
            searchPanel.TabIndex = 0;
            // 
            // btnFilter
            // 
            btnFilter.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnFilter.BackColor = Color.FromArgb(229, 231, 235);
            btnFilter.FlatAppearance.BorderSize = 0;
            btnFilter.FlatStyle = FlatStyle.Flat;
            btnFilter.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnFilter.ForeColor = Color.FromArgb(55, 65, 81);
            btnFilter.Location = new Point(1300, 27);
            btnFilter.Margin = new Padding(3, 4, 3, 4);
            btnFilter.Name = "btnFilter";
            btnFilter.Size = new Size(140, 40);
            btnFilter.TabIndex = 1;
            btnFilter.Text = "🔧 Bộ lọc";
            btnFilter.UseVisualStyleBackColor = false;
            btnFilter.Click += btnFilter_Click;
            // 
            // txtSearch
            // 
            txtSearch.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtSearch.BorderStyle = BorderStyle.FixedSingle;
            txtSearch.Font = new Font("Segoe UI", 11F);
            txtSearch.Location = new Point(23, 27);
            txtSearch.Margin = new Padding(3, 4, 3, 4);
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = "🔍 Tìm kiếm hóa đơn...";
            txtSearch.Size = new Size(1260, 32);
            txtSearch.TabIndex = 0;
            // 
            // InvoiceEditorForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(243, 244, 246);
            ClientSize = new Size(1600, 1055);
            Controls.Add(contentPanel);
            Controls.Add(headerPanel);
            Font = new Font("Segoe UI", 9F);
            Margin = new Padding(3, 4, 3, 4);
            Name = "InvoiceEditorForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Quản lý Hóa Đơn";
            Load += InvoiceEditorForm_Load;
            headerPanel.ResumeLayout(false);
            headerPanel.PerformLayout();
            contentPanel.ResumeLayout(false);
            whitePanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)invoiceGrid).EndInit();
            filterContainerPanel.ResumeLayout(false);
            filterPanel.ResumeLayout(false);
            searchPanel.ResumeLayout(false);
            searchPanel.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel headerPanel;
        private Label lblTitle;
        private Panel contentPanel;
        private Panel whitePanel;
        private Panel searchPanel;
        private TextBox txtSearch;
        private Button btnFilter;
        private DataGridView invoiceGrid;
        private Panel filterContainerPanel;
        private Panel filterPanel;
        private Button btnCustomerFilter;
        private Button btnEmployeeFilter;
        private Button btnServiceFilter;
        private Button btnStatusFilter;
    }
}