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
            headerPanel.Padding = new Padding(30, 25, 30, 25);
            headerPanel.Size = new Size(1600, 100);
            headerPanel.TabIndex = 0;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(17, 24, 39);
            lblTitle.Location = new Point(30, 25);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(190, 54);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Hóa Đơn";
            // 
            // contentPanel
            // 
            contentPanel.BackColor = Color.FromArgb(243, 244, 246);
            contentPanel.Controls.Add(whitePanel);
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.Location = new Point(0, 100);
            contentPanel.Margin = new Padding(3, 4, 3, 4);
            contentPanel.Name = "contentPanel";
            contentPanel.Padding = new Padding(30, 15, 30, 15);
            contentPanel.Size = new Size(1600, 955);
            contentPanel.TabIndex = 1;
            // 
            // whitePanel
            // 
            whitePanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            whitePanel.BackColor = Color.White;
            whitePanel.Controls.Add(invoiceGrid);
            whitePanel.Controls.Add(filterContainerPanel);
            whitePanel.Controls.Add(searchPanel);
            whitePanel.Location = new Point(30, 15);
            whitePanel.Margin = new Padding(3, 4, 3, 4);
            whitePanel.Name = "whitePanel";
            whitePanel.Padding = new Padding(30, 15, 30, 15);
            whitePanel.Size = new Size(1540, 925);
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
            invoiceGrid.ColumnHeadersHeight = 45;
            invoiceGrid.Dock = DockStyle.Fill;
            invoiceGrid.GridColor = Color.FromArgb(229, 231, 235);
            invoiceGrid.Location = new Point(30, 195);
            invoiceGrid.Margin = new Padding(3, 4, 3, 4);
            invoiceGrid.MultiSelect = false;
            invoiceGrid.Name = "invoiceGrid";
            invoiceGrid.ReadOnly = true;
            invoiceGrid.RowHeadersVisible = false;
            invoiceGrid.RowHeadersWidth = 51;
            invoiceGrid.RowTemplate.Height = 55;
            invoiceGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            invoiceGrid.Size = new Size(1480, 715);
            invoiceGrid.TabIndex = 1;
            // 
            // filterContainerPanel
            // 
            filterContainerPanel.BackColor = Color.White;
            filterContainerPanel.Controls.Add(filterPanel);
            filterContainerPanel.Dock = DockStyle.Top;
            filterContainerPanel.Location = new Point(30, 110);
            filterContainerPanel.Margin = new Padding(3, 4, 3, 4);
            filterContainerPanel.Name = "filterContainerPanel";
            filterContainerPanel.Padding = new Padding(0, 10, 0, 10);
            filterContainerPanel.Size = new Size(1480, 85);
            filterContainerPanel.TabIndex = 2;
            filterContainerPanel.Visible = false;
            // 
            // filterPanel
            // 
            filterPanel.BackColor = Color.FromArgb(249, 250, 251);
            filterPanel.Controls.Add(btnStatusFilter);
            filterPanel.Controls.Add(btnServiceFilter);
            filterPanel.Controls.Add(btnCustomerFilter);
            filterPanel.Dock = DockStyle.Fill;
            filterPanel.Location = new Point(0, 10);
            filterPanel.Margin = new Padding(3, 4, 3, 4);
            filterPanel.Name = "filterPanel";
            filterPanel.Padding = new Padding(10, 10, 10, 10);
            filterPanel.Size = new Size(1480, 65);
            filterPanel.TabIndex = 0;
            // 
            // btnStatusFilter
            // 
            btnStatusFilter.BackColor = Color.White;
            btnStatusFilter.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            btnStatusFilter.FlatAppearance.BorderSize = 1;
            btnStatusFilter.FlatStyle = FlatStyle.Flat;
            btnStatusFilter.Font = new Font("Segoe UI", 9.75F);
            btnStatusFilter.ForeColor = Color.FromArgb(55, 65, 81);
            btnStatusFilter.Location = new Point(350, 8);
            btnStatusFilter.Margin = new Padding(3, 4, 3, 4);
            btnStatusFilter.Name = "btnStatusFilter";
            btnStatusFilter.Padding = new Padding(10, 0, 8, 0);
            btnStatusFilter.Size = new Size(150, 48);
            btnStatusFilter.TabIndex = 3;
            btnStatusFilter.Text = "⚪ Trạng thái";
            btnStatusFilter.TextAlign = ContentAlignment.MiddleLeft;
            btnStatusFilter.UseVisualStyleBackColor = false;
            // 
            // btnServiceFilter
            // 
            btnServiceFilter.BackColor = Color.White;
            btnServiceFilter.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            btnServiceFilter.FlatAppearance.BorderSize = 1;
            btnServiceFilter.FlatStyle = FlatStyle.Flat;
            btnServiceFilter.Font = new Font("Segoe UI", 9.75F);
            btnServiceFilter.ForeColor = Color.FromArgb(55, 65, 81);
            btnServiceFilter.Location = new Point(175, 8);
            btnServiceFilter.Margin = new Padding(3, 4, 3, 4);
            btnServiceFilter.Name = "btnServiceFilter";
            btnServiceFilter.Padding = new Padding(10, 0, 8, 0);
            btnServiceFilter.Size = new Size(150, 48);
            btnServiceFilter.TabIndex = 2;
            btnServiceFilter.Text = "💼 Dịch vụ";
            btnServiceFilter.TextAlign = ContentAlignment.MiddleLeft;
            btnServiceFilter.UseVisualStyleBackColor = false;
            // 
            // btnCustomerFilter
            // 
            btnCustomerFilter.BackColor = Color.White;
            btnCustomerFilter.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            btnCustomerFilter.FlatAppearance.BorderSize = 1;
            btnCustomerFilter.FlatStyle = FlatStyle.Flat;
            btnCustomerFilter.Font = new Font("Segoe UI", 9.75F);
            btnCustomerFilter.ForeColor = Color.FromArgb(55, 65, 81);
            btnCustomerFilter.Location = new Point(0, 8);
            btnCustomerFilter.Margin = new Padding(3, 4, 3, 4);
            btnCustomerFilter.Name = "btnCustomerFilter";
            btnCustomerFilter.Padding = new Padding(10, 0, 8, 0);
            btnCustomerFilter.Size = new Size(150, 48);
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
            searchPanel.Location = new Point(30, 15);
            searchPanel.Margin = new Padding(3, 4, 3, 4);
            searchPanel.Name = "searchPanel";
            searchPanel.Padding = new Padding(10, 10, 10, 10);
            searchPanel.Size = new Size(1480, 75);
            searchPanel.TabIndex = 0;
            // 
            // btnFilter
            // 
            btnFilter.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnFilter.BackColor = Color.FromArgb(243, 244, 246);
            btnFilter.FlatAppearance.BorderSize = 0;
            btnFilter.FlatStyle = FlatStyle.Flat;
            btnFilter.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            btnFilter.ForeColor = Color.FromArgb(55, 65, 81);
            btnFilter.Location = new Point(1330, 17);
            btnFilter.Margin = new Padding(6, 4, 6, 4);
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
            txtSearch.Font = new Font("Segoe UI", 10.5F);
            txtSearch.Location = new Point(12, 19);
            txtSearch.Margin = new Padding(3, 4, 3, 4);
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = "🔍 Tìm kiếm hóa đơn...";
            txtSearch.Size = new Size(1210, 36);
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
        private Button btnServiceFilter;
        private Button btnStatusFilter;
    }
}