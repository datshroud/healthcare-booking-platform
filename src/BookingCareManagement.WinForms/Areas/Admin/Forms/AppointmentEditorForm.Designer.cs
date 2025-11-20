namespace BookingCareManagement.WinForms.Areas.Admin.Forms
{
    partial class AppointmentEditorForm
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
            btnNew = new Button();
            btnExport = new Button();
            lblTitle = new Label();
            contentPanel = new Panel();
            whitePanel = new Panel();
            listContainer = new Panel();
            emptyStatePanel = new Panel();
            lblEmptyMessage = new Label();
            lblEmptySubtitle = new Label();
            appointmentGrid = new DataGridView();
            filterContainerPanel = new Panel();
            filterPanel = new Panel();
            btnStatusFilter = new Button();
            btnEmployeeFilter = new Button();
            btnCustomerFilter = new Button();
            btnServiceFilter = new Button();
            searchPanel = new Panel();
            searchBoxPanel = new Panel();
            txtSearch = new TextBox();
            lblSearchIcon = new Label();
            btnFilter = new Button();
            dtTo = new DateTimePicker();
            dtFrom = new DateTimePicker();
            headerPanel.SuspendLayout();
            contentPanel.SuspendLayout();
            whitePanel.SuspendLayout();
            listContainer.SuspendLayout();
            emptyStatePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)appointmentGrid).BeginInit();
            filterContainerPanel.SuspendLayout();
            filterPanel.SuspendLayout();
            searchPanel.SuspendLayout();
            SuspendLayout();
            // 
            // headerPanel
            // 
            headerPanel.BackColor = Color.FromArgb(243, 244, 246);
            headerPanel.Controls.Add(btnNew);
            headerPanel.Controls.Add(btnExport);
            headerPanel.Controls.Add(lblTitle);
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Location = new Point(0, 0);
            headerPanel.Margin = new Padding(3, 4, 3, 4);
            headerPanel.Name = "headerPanel";
            headerPanel.Padding = new Padding(34, 27, 34, 27);
            headerPanel.Size = new Size(1600, 107);
            headerPanel.TabIndex = 0;
            // 
            // btnNew
            // 
            btnNew.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnNew.BackColor = Color.FromArgb(37, 99, 235);
            btnNew.FlatAppearance.BorderSize = 0;
            btnNew.FlatStyle = FlatStyle.Flat;
            btnNew.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnNew.ForeColor = Color.White;
            btnNew.Location = new Point(1383, 24);
            btnNew.Margin = new Padding(3, 4, 3, 4);
            btnNew.Name = "btnNew";
            btnNew.Size = new Size(183, 59);
            btnNew.TabIndex = 2;
            btnNew.Text = "+  Tạo Lịch Hẹn";
            btnNew.UseVisualStyleBackColor = false;
            // 
            // btnExport
            // 
            btnExport.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnExport.BackColor = Color.White;
            btnExport.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            btnExport.FlatStyle = FlatStyle.Flat;
            btnExport.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnExport.ForeColor = Color.FromArgb(55, 65, 81);
            btnExport.Location = new Point(1211, 24);
            btnExport.Margin = new Padding(3, 4, 3, 4);
            btnExport.Name = "btnExport";
            btnExport.Size = new Size(160, 59);
            btnExport.TabIndex = 1;
            btnExport.Text = "↓  Xuất Dữ Liệu";
            btnExport.UseVisualStyleBackColor = false;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(17, 24, 39);
            lblTitle.Location = new Point(34, 27);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(249, 54);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Lịch Hẹn (2)";
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
            whitePanel.Controls.Add(listContainer);
            whitePanel.Controls.Add(filterContainerPanel);
            whitePanel.Controls.Add(searchPanel);
            whitePanel.Location = new Point(34, 13);
            whitePanel.Margin = new Padding(3, 4, 3, 4);
            whitePanel.Name = "whitePanel";
            whitePanel.Padding = new Padding(34, 13, 34, 67);
            whitePanel.Size = new Size(1531, 921);
            whitePanel.TabIndex = 0;
            //
            // listContainer
            //
            listContainer.Controls.Add(emptyStatePanel);
            listContainer.Controls.Add(appointmentGrid);
            listContainer.Dock = DockStyle.Fill;
            listContainer.Location = new Point(34, 199);
            listContainer.Margin = new Padding(3, 4, 3, 4);
            listContainer.Name = "listContainer";
            listContainer.Size = new Size(1463, 655);
            listContainer.TabIndex = 3;
            //
            // emptyStatePanel
            //
            emptyStatePanel.BackColor = Color.FromArgb(249, 250, 251);
            emptyStatePanel.Controls.Add(lblEmptyMessage);
            emptyStatePanel.Controls.Add(lblEmptySubtitle);
            emptyStatePanel.Dock = DockStyle.Fill;
            emptyStatePanel.Location = new Point(0, 0);
            emptyStatePanel.Margin = new Padding(3, 4, 3, 4);
            emptyStatePanel.Name = "emptyStatePanel";
            emptyStatePanel.Size = new Size(1463, 655);
            emptyStatePanel.TabIndex = 2;
            emptyStatePanel.Visible = false;
            //
            // lblEmptyMessage
            //
            lblEmptyMessage.Anchor = AnchorStyles.None;
            lblEmptyMessage.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblEmptyMessage.ForeColor = Color.FromArgb(17, 24, 39);
            lblEmptyMessage.Location = new Point(0, 285);
            lblEmptyMessage.Name = "lblEmptyMessage";
            lblEmptyMessage.Size = new Size(1463, 32);
            lblEmptyMessage.TabIndex = 1;
            lblEmptyMessage.Text = "Không có cuộc hẹn nào";
            lblEmptyMessage.TextAlign = ContentAlignment.MiddleCenter;
            //
            // lblEmptySubtitle
            //
            lblEmptySubtitle.Anchor = AnchorStyles.None;
            lblEmptySubtitle.Font = new Font("Segoe UI", 10F);
            lblEmptySubtitle.ForeColor = Color.FromArgb(107, 114, 128);
            lblEmptySubtitle.Location = new Point(0, 318);
            lblEmptySubtitle.Name = "lblEmptySubtitle";
            lblEmptySubtitle.Size = new Size(1463, 32);
            lblEmptySubtitle.TabIndex = 0;
            lblEmptySubtitle.Text = "Vui lòng điều chỉnh bộ lọc hoặc phạm vi ngày để tìm kiếm cuộc hẹn phù hợp.";
            lblEmptySubtitle.TextAlign = ContentAlignment.MiddleCenter;
            //
            // appointmentGrid
            //
            appointmentGrid.AllowUserToAddRows = false;
            appointmentGrid.AllowUserToDeleteRows = false;
            appointmentGrid.AllowUserToResizeRows = false;
            appointmentGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            appointmentGrid.BackgroundColor = Color.White;
            appointmentGrid.BorderStyle = BorderStyle.None;
            appointmentGrid.ColumnHeadersHeight = 50;
            appointmentGrid.Dock = DockStyle.Fill;
            appointmentGrid.GridColor = Color.FromArgb(243, 244, 246);
            appointmentGrid.Location = new Point(0, 0);
            appointmentGrid.Margin = new Padding(3, 4, 3, 4);
            appointmentGrid.MultiSelect = false;
            appointmentGrid.Name = "appointmentGrid";
            appointmentGrid.ReadOnly = true;
            appointmentGrid.RowHeadersVisible = false;
            appointmentGrid.RowHeadersWidth = 51;
            appointmentGrid.RowTemplate.Height = 60;
            appointmentGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            appointmentGrid.Size = new Size(1463, 655);
            appointmentGrid.TabIndex = 1;
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
            filterPanel.Controls.Add(btnEmployeeFilter);
            filterPanel.Controls.Add(btnCustomerFilter);
            filterPanel.Controls.Add(btnServiceFilter);
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
            btnStatusFilter.Text = "⚪ Trạng Thái";
            btnStatusFilter.TextAlign = ContentAlignment.MiddleLeft;
            btnStatusFilter.UseVisualStyleBackColor = false;
            // 
            // btnEmployeeFilter
            // 
            btnEmployeeFilter.BackColor = Color.White;
            btnEmployeeFilter.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            btnEmployeeFilter.FlatStyle = FlatStyle.Flat;
            btnEmployeeFilter.Font = new Font("Segoe UI", 10F);
            btnEmployeeFilter.Location = new Point(400, 20);
            btnEmployeeFilter.Margin = new Padding(3, 4, 3, 4);
            btnEmployeeFilter.Name = "btnEmployeeFilter";
            btnEmployeeFilter.Padding = new Padding(11, 0, 6, 0);
            btnEmployeeFilter.Size = new Size(171, 53);
            btnEmployeeFilter.TabIndex = 2;
            btnEmployeeFilter.Text = "👤  Nhân viên";
            btnEmployeeFilter.TextAlign = ContentAlignment.MiddleLeft;
            btnEmployeeFilter.UseVisualStyleBackColor = false;
            // 
            // btnCustomerFilter
            // 
            btnCustomerFilter.BackColor = Color.White;
            btnCustomerFilter.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            btnCustomerFilter.FlatStyle = FlatStyle.Flat;
            btnCustomerFilter.Font = new Font("Segoe UI", 10F);
            btnCustomerFilter.Location = new Point(206, 20);
            btnCustomerFilter.Margin = new Padding(3, 4, 3, 4);
            btnCustomerFilter.Name = "btnCustomerFilter";
            btnCustomerFilter.Padding = new Padding(11, 0, 6, 0);
            btnCustomerFilter.Size = new Size(171, 53);
            btnCustomerFilter.TabIndex = 1;
            btnCustomerFilter.Text = "👥  Khách hàng";
            btnCustomerFilter.TextAlign = ContentAlignment.MiddleLeft;
            btnCustomerFilter.UseVisualStyleBackColor = false;
            // 
            // btnServiceFilter
            // 
            btnServiceFilter.BackColor = Color.White;
            btnServiceFilter.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            btnServiceFilter.FlatStyle = FlatStyle.Flat;
            btnServiceFilter.Font = new Font("Segoe UI", 10F);
            btnServiceFilter.ForeColor = Color.Black;
            btnServiceFilter.Location = new Point(11, 20);
            btnServiceFilter.Margin = new Padding(3, 4, 3, 4);
            btnServiceFilter.Name = "btnServiceFilter";
            btnServiceFilter.Padding = new Padding(11, 0, 6, 0);
            btnServiceFilter.Size = new Size(171, 53);
            btnServiceFilter.TabIndex = 0;
            btnServiceFilter.Text = "🏥  Dịch vụ";
            btnServiceFilter.TextAlign = ContentAlignment.MiddleLeft;
            btnServiceFilter.UseVisualStyleBackColor = false;
            // 
            // searchPanel
            // 
            searchPanel.BackColor = Color.White;
            searchPanel.Controls.Add(searchBoxPanel);
            searchPanel.Controls.Add(btnFilter);
            searchPanel.Controls.Add(dtTo);
            searchPanel.Controls.Add(dtFrom);
            searchPanel.Dock = DockStyle.Top;
            searchPanel.Location = new Point(34, 13);
            searchPanel.Margin = new Padding(3, 4, 3, 4);
            searchPanel.Name = "searchPanel";
            searchPanel.Size = new Size(1463, 93);
            searchPanel.TabIndex = 0;
            //
            // searchBoxPanel
            //
            searchBoxPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            searchBoxPanel.BackColor = Color.FromArgb(249, 250, 251);
            searchBoxPanel.BorderStyle = BorderStyle.FixedSingle;
            searchBoxPanel.Controls.Add(txtSearch);
            searchBoxPanel.Controls.Add(lblSearchIcon);
            searchBoxPanel.Location = new Point(23, 23);
            searchBoxPanel.Margin = new Padding(3, 4, 3, 4);
            searchBoxPanel.Name = "searchBoxPanel";
            searchBoxPanel.Padding = new Padding(15, 8, 15, 8);
            searchBoxPanel.Size = new Size(551, 47);
            searchBoxPanel.TabIndex = 4;
            //
            // txtSearch
            //
            txtSearch.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtSearch.BackColor = Color.FromArgb(249, 250, 251);
            txtSearch.BorderStyle = BorderStyle.None;
            txtSearch.Font = new Font("Segoe UI", 11F);
            txtSearch.Location = new Point(42, 10);
            txtSearch.Margin = new Padding(3, 4, 3, 4);
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = "Tìm kiếm cuộc hẹn (chuyên khoa, bệnh nhân, bác sĩ...)";
            txtSearch.Size = new Size(496, 25);
            txtSearch.TabIndex = 0;
            //
            // lblSearchIcon
            //
            lblSearchIcon.AutoSize = true;
            lblSearchIcon.Font = new Font("Segoe UI", 11F);
            lblSearchIcon.ForeColor = Color.FromArgb(107, 114, 128);
            lblSearchIcon.Location = new Point(5, 9);
            lblSearchIcon.Name = "lblSearchIcon";
            lblSearchIcon.Size = new Size(23, 25);
            lblSearchIcon.TabIndex = 4;
            lblSearchIcon.Text = "🔍";
            //
            // btnFilter
            //
            btnFilter.BackColor = Color.FromArgb(229, 231, 235);
            btnFilter.FlatStyle = FlatStyle.Flat;
            btnFilter.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnFilter.Location = new Point(1299, 26);
            btnFilter.Margin = new Padding(3, 4, 3, 4);
            btnFilter.Name = "btnFilter";
            btnFilter.Size = new Size(141, 40);
            btnFilter.TabIndex = 3;
            btnFilter.Text = "Bộ lọc";
            btnFilter.UseVisualStyleBackColor = false;
            btnFilter.Click += btnFilter_Click;
            //
            // dtTo
            //
            dtTo.Format = DateTimePickerFormat.Short;
            dtTo.Location = new Point(1134, 28);
            dtTo.Margin = new Padding(3, 4, 3, 4);
            dtTo.Name = "dtTo";
            dtTo.Size = new Size(145, 27);
            dtTo.TabIndex = 2;
            //
            // dtFrom
            //
            dtFrom.Format = DateTimePickerFormat.Short;
            dtFrom.Location = new Point(985, 28);
            dtFrom.Margin = new Padding(3, 4, 3, 4);
            dtFrom.Name = "dtFrom";
            dtFrom.Size = new Size(145, 27);
            dtFrom.TabIndex = 1;
            //
            // AppointmentEditorForm
            //
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(243, 244, 246);
            ClientSize = new Size(1600, 1055);
            Controls.Add(contentPanel);
            Controls.Add(headerPanel);
            Font = new Font("Segoe UI", 9F);
            Margin = new Padding(3, 4, 3, 4);
            Name = "AppointmentEditorForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Lịch Hẹn";
            headerPanel.ResumeLayout(false);
            headerPanel.PerformLayout();
            contentPanel.ResumeLayout(false);
            whitePanel.ResumeLayout(false);
            listContainer.ResumeLayout(false);
            emptyStatePanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)appointmentGrid).EndInit();
            filterContainerPanel.ResumeLayout(false);
            filterPanel.ResumeLayout(false);
            searchPanel.ResumeLayout(false);
            searchBoxPanel.ResumeLayout(false);
            searchBoxPanel.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel headerPanel;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Button btnNew;
        private System.Windows.Forms.Panel contentPanel;
        private System.Windows.Forms.Panel whitePanel;
        private System.Windows.Forms.Panel searchPanel;
        private System.Windows.Forms.DateTimePicker dtFrom;
        private System.Windows.Forms.DateTimePicker dtTo;
        private System.Windows.Forms.Button btnFilter;
        private System.Windows.Forms.DataGridView appointmentGrid;
        private System.Windows.Forms.Panel filterContainerPanel;
        private System.Windows.Forms.Panel filterPanel;
        private System.Windows.Forms.Button btnServiceFilter;
        private System.Windows.Forms.Button btnCustomerFilter;
        private System.Windows.Forms.Button btnEmployeeFilter;
        private System.Windows.Forms.Button btnStatusFilter;
        private System.Windows.Forms.Panel listContainer;
        private System.Windows.Forms.Panel emptyStatePanel;
        private System.Windows.Forms.Label lblEmptyMessage;
        private System.Windows.Forms.Label lblEmptySubtitle;
        private System.Windows.Forms.Panel searchBoxPanel;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblSearchIcon;
    }
}
