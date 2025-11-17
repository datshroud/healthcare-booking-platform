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
            this.headerPanel = new System.Windows.Forms.Panel();
            this.btnNew = new System.Windows.Forms.Button();
            this.btnExport = new System.Windows.Forms.Button();
            this.lblTitle = new System.Windows.Forms.Label();
            this.contentPanel = new System.Windows.Forms.Panel();
            this.whitePanel = new System.Windows.Forms.Panel();
            this.appointmentGrid = new System.Windows.Forms.DataGridView();
            this.filterContainerPanel = new System.Windows.Forms.Panel();
            this.filterPanel = new System.Windows.Forms.Panel();
            this.btnStatusFilter = new System.Windows.Forms.Button();
            this.btnEmployeeFilter = new System.Windows.Forms.Button();
            this.btnCustomerFilter = new System.Windows.Forms.Button();
            this.btnServiceFilter = new System.Windows.Forms.Button();
            this.searchPanel = new System.Windows.Forms.Panel();
            this.btnFilter = new System.Windows.Forms.Button();
            this.dtTo = new System.Windows.Forms.DateTimePicker();
            this.dtFrom = new System.Windows.Forms.DateTimePicker();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.headerPanel.SuspendLayout();
            this.contentPanel.SuspendLayout();
            this.whitePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.appointmentGrid)).BeginInit();
            this.filterContainerPanel.SuspendLayout();
            this.filterPanel.SuspendLayout();
            this.searchPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // headerPanel
            // 
            this.headerPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(244)))), ((int)(((byte)(246)))));
            this.headerPanel.Controls.Add(this.btnNew);
            this.headerPanel.Controls.Add(this.btnExport);
            this.headerPanel.Controls.Add(this.lblTitle);
            this.headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.headerPanel.Location = new System.Drawing.Point(0, 0);
            this.headerPanel.Name = "headerPanel";
            this.headerPanel.Padding = new System.Windows.Forms.Padding(30, 20, 30, 20);
            this.headerPanel.Size = new System.Drawing.Size(1400, 80);
            this.headerPanel.TabIndex = 0;
            // 
            // btnNew
            // 
            this.btnNew.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNew.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.btnNew.FlatAppearance.BorderSize = 0;
            this.btnNew.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNew.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnNew.ForeColor = System.Drawing.Color.White;
            this.btnNew.Location = new System.Drawing.Point(1210, 18);
            this.btnNew.Name = "btnNew";
            this.btnNew.Size = new System.Drawing.Size(160, 44);
            this.btnNew.TabIndex = 2;
            this.btnNew.Text = "+  T?o L?ch H?n";
            this.btnNew.UseVisualStyleBackColor = false;
            // 
            // btnExport
            // 
            this.btnExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExport.BackColor = System.Drawing.Color.White;
            this.btnExport.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.btnExport.FlatAppearance.BorderSize = 1;
            this.btnExport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExport.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnExport.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnExport.Location = new System.Drawing.Point(1060, 18);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(140, 44);
            this.btnExport.TabIndex = 1;
            this.btnExport.Text = "?  Xu?t D? Li?u";
            this.btnExport.UseVisualStyleBackColor = false;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(24)))), ((int)(((byte)(39)))));
            this.lblTitle.Location = new System.Drawing.Point(30, 20);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(221, 45);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "L?ch H?n (2)";
            // 
            // contentPanel
            // 
            this.contentPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(244)))), ((int)(((byte)(246)))));
            this.contentPanel.Controls.Add(this.whitePanel);
            this.contentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.contentPanel.Location = new System.Drawing.Point(0, 80);
            this.contentPanel.Name = "contentPanel";
            this.contentPanel.Padding = new System.Windows.Forms.Padding(30, 10, 30, 1000);
            this.contentPanel.Size = new System.Drawing.Size(1400, 720);
            this.contentPanel.TabIndex = 1;
            // 
            // whitePanel
            // 
            this.whitePanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.whitePanel.BackColor = System.Drawing.Color.White;
            this.whitePanel.Controls.Add(this.appointmentGrid);
            this.whitePanel.Controls.Add(this.filterContainerPanel);
            this.whitePanel.Controls.Add(this.searchPanel);
            this.whitePanel.Location = new System.Drawing.Point(30, 10);
            this.whitePanel.Name = "whitePanel";
            this.whitePanel.Padding = new System.Windows.Forms.Padding(30, 10, 30, 50);
            this.whitePanel.Size = new System.Drawing.Size(1340, 700);
            this.whitePanel.TabIndex = 0;
            // 
            // appointmentGrid
            // 
            this.appointmentGrid.AllowUserToAddRows = false;
            this.appointmentGrid.AllowUserToDeleteRows = false;
            this.appointmentGrid.AllowUserToResizeRows = false;
            this.appointmentGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.appointmentGrid.BackgroundColor = System.Drawing.Color.White;
            this.appointmentGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.appointmentGrid.ColumnHeadersHeight = 50;
            this.appointmentGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.appointmentGrid.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(244)))), ((int)(((byte)(246)))));
            this.appointmentGrid.Location = new System.Drawing.Point(30, 150);
            this.appointmentGrid.MultiSelect = false;
            this.appointmentGrid.Name = "appointmentGrid";
            this.appointmentGrid.ReadOnly = true;
            this.appointmentGrid.RowHeadersVisible = false;
            this.appointmentGrid.RowTemplate.Height = 60;
            this.appointmentGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.appointmentGrid.Size = new System.Drawing.Size(1280, 500);
            this.appointmentGrid.TabIndex = 1;
            // 
            // filterContainerPanel
            // 
            this.filterContainerPanel.BackColor = System.Drawing.Color.White;
            this.filterContainerPanel.Controls.Add(this.filterPanel);
            this.filterContainerPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.filterContainerPanel.Location = new System.Drawing.Point(30, 80);
            this.filterContainerPanel.Name = "filterContainerPanel";
            this.filterContainerPanel.Size = new System.Drawing.Size(1280, 70);
            this.filterContainerPanel.TabIndex = 2;
            this.filterContainerPanel.Visible = false;
            // 
            // filterPanel
            // 
            this.filterPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(250)))), ((int)(((byte)(251)))));

            this.filterPanel.Controls.Add(this.btnStatusFilter);
            this.filterPanel.Controls.Add(this.btnEmployeeFilter);
            this.filterPanel.Controls.Add(this.btnCustomerFilter);
            this.filterPanel.Controls.Add(this.btnServiceFilter);
            this.filterPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.filterPanel.Location = new System.Drawing.Point(0, 0);
            this.filterPanel.Name = "filterPanel";
            this.filterPanel.Padding = new System.Windows.Forms.Padding(10);
            this.filterPanel.Size = new System.Drawing.Size(1280, 70);
            this.filterPanel.TabIndex = 0;
            // 
            // btnStatusFilter
            // 
            this.btnStatusFilter.BackColor = System.Drawing.Color.White;
            this.btnStatusFilter.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.btnStatusFilter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStatusFilter.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnStatusFilter.Location = new System.Drawing.Point(520, 15);
            this.btnStatusFilter.Name = "btnStatusFilter";
            this.btnStatusFilter.Padding = new System.Windows.Forms.Padding(10, 0, 5, 0);
            this.btnStatusFilter.Size = new System.Drawing.Size(150, 40);
            this.btnStatusFilter.TabIndex = 3;
            this.btnStatusFilter.Text = "?  Tr?ng Thái";
            this.btnStatusFilter.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnStatusFilter.UseVisualStyleBackColor = false;
            // 
            // btnEmployeeFilter
            // 
            this.btnEmployeeFilter.BackColor = System.Drawing.Color.White;
            this.btnEmployeeFilter.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.btnEmployeeFilter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEmployeeFilter.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnEmployeeFilter.Location = new System.Drawing.Point(350, 15);
            this.btnEmployeeFilter.Name = "btnEmployeeFilter";
            this.btnEmployeeFilter.Padding = new System.Windows.Forms.Padding(10, 0, 5, 0);
            this.btnEmployeeFilter.Size = new System.Drawing.Size(150, 40);
            this.btnEmployeeFilter.TabIndex = 2;
            this.btnEmployeeFilter.Text = "??  Nhân Viên";
            this.btnEmployeeFilter.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnEmployeeFilter.UseVisualStyleBackColor = false;
            // 
            // btnCustomerFilter
            // 
            this.btnCustomerFilter.BackColor = System.Drawing.Color.White;
            this.btnCustomerFilter.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.btnCustomerFilter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCustomerFilter.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnCustomerFilter.Location = new System.Drawing.Point(180, 15);
            this.btnCustomerFilter.Name = "btnCustomerFilter";
            this.btnCustomerFilter.Padding = new System.Windows.Forms.Padding(10, 0, 5, 0);
            this.btnCustomerFilter.Size = new System.Drawing.Size(150, 40);
            this.btnCustomerFilter.TabIndex = 1;
            this.btnCustomerFilter.Text = "??  Khách Hàng";
            this.btnCustomerFilter.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCustomerFilter.UseVisualStyleBackColor = false;
            // 
            // btnServiceFilter
            // 
            this.btnServiceFilter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(234)))), ((int)(((byte)(254)))));
            this.btnServiceFilter.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.btnServiceFilter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnServiceFilter.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnServiceFilter.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.btnServiceFilter.Location = new System.Drawing.Point(10, 15);
            this.btnServiceFilter.Name = "btnServiceFilter";
            this.btnServiceFilter.Padding = new System.Windows.Forms.Padding(10, 0, 5, 0);
            this.btnServiceFilter.Size = new System.Drawing.Size(150, 40);
            this.btnServiceFilter.TabIndex = 0;
            this.btnServiceFilter.Text = "??  D?ch V?";
            this.btnServiceFilter.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnServiceFilter.UseVisualStyleBackColor = false;
            // 
            // searchPanel
            // 
            this.searchPanel.BackColor = System.Drawing.Color.White;
            this.searchPanel.Controls.Add(this.btnFilter);
            this.searchPanel.Controls.Add(this.dtTo);
            this.searchPanel.Controls.Add(this.dtFrom);
            this.searchPanel.Controls.Add(this.txtSearch);
            this.searchPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.searchPanel.Location = new System.Drawing.Point(30, 10);
            this.searchPanel.Name = "searchPanel";
            this.searchPanel.Size = new System.Drawing.Size(1280, 70);
            this.searchPanel.TabIndex = 0;
            // 
            // btnFilter
            // 
            this.btnFilter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(229)))), ((int)(((byte)(231)))), ((int)(((byte)(235)))));
            this.btnFilter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFilter.Location = new System.Drawing.Point(560, 20);
            this.btnFilter.Name = "btnFilter";
            this.btnFilter.Size = new System.Drawing.Size(80, 30);
            this.btnFilter.TabIndex = 3;
            this.btnFilter.Text = "L?c";
            this.btnFilter.UseVisualStyleBackColor = false;
            this.btnFilter.Click += new System.EventHandler(this.btnFilter_Click);
            // 
            // dtTo
            // 
            this.dtTo.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtTo.Location = new System.Drawing.Point(420, 20);
            this.dtTo.Name = "dtTo";
            this.dtTo.Size = new System.Drawing.Size(120, 23);
            this.dtTo.TabIndex = 2;
            // 
            // dtFrom
            // 
            this.dtFrom.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtFrom.Location = new System.Drawing.Point(290, 20);
            this.dtFrom.Name = "dtFrom";
            this.dtFrom.Size = new System.Drawing.Size(120, 23);
            this.dtFrom.TabIndex = 1;
            // 
            // txtSearch
            // 
            this.txtSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSearch.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtSearch.Location = new System.Drawing.Point(20, 20);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.PlaceholderText = "Tìm ki?m";
            this.txtSearch.Size = new System.Drawing.Size(250, 27);
            this.txtSearch.TabIndex = 0;
            // 
            // AppointmentEditorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(244)))), ((int)(((byte)(246)))));
            this.ClientSize = new System.Drawing.Size(1400, 800);
            this.Controls.Add(this.contentPanel);
            this.Controls.Add(this.headerPanel);
            this.Name = "AppointmentEditorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "L?ch H?n";
            this.headerPanel.ResumeLayout(false);
            this.headerPanel.PerformLayout();
            this.contentPanel.ResumeLayout(false);
            this.whitePanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.appointmentGrid)).EndInit();
            this.filterContainerPanel.ResumeLayout(false);
            this.filterPanel.ResumeLayout(false);
            this.searchPanel.ResumeLayout(false);
            this.searchPanel.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel headerPanel;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Button btnNew;
        private System.Windows.Forms.Panel contentPanel;
        private System.Windows.Forms.Panel whitePanel;
        private System.Windows.Forms.Panel searchPanel;
        private System.Windows.Forms.TextBox txtSearch;
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
    }
}
