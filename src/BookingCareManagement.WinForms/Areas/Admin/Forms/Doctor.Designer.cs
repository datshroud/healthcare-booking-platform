namespace BookingCareManagement.WinForms.Areas.Admin.Forms
{
    public partial class Doctor
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            panelTop = new Panel();
            labelTitle = new Label();
            labelCount = new Label();
            buttonAdd = new Button();
            panelSearchFilter = new Panel();
            textBoxSearch = new TextBox();
            buttonFilter = new Button();
            panelMain = new Panel();
            dataGridViewDoctors = new DataGridView();
            ColumnImage = new DataGridViewImageColumn();
            ColumnName = new DataGridViewTextBoxColumn();
            ColumnEmail = new DataGridViewTextBoxColumn();
            ColumnPhone = new DataGridViewTextBoxColumn();
            ColumnSpecialty = new DataGridViewTextBoxColumn();
            ColumnStatus = new DataGridViewTextBoxColumn();
            ColumnActions = new DataGridViewButtonColumn();
            panelTop.SuspendLayout();
            panelSearchFilter.SuspendLayout();
            panelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridViewDoctors).BeginInit();
            SuspendLayout();
            // 
            // panelTop
            // 
            panelTop.BackColor = Color.White;
            panelTop.Controls.Add(labelTitle);
            panelTop.Controls.Add(labelCount);
            panelTop.Controls.Add(buttonAdd);
            panelTop.Dock = DockStyle.Top;
            panelTop.Location = new Point(0, 0);
            panelTop.Margin = new Padding(0);
            panelTop.Name = "panelTop";
            panelTop.Padding = new Padding(30, 25, 30, 25);
            panelTop.Size = new Size(1400, 90);
            panelTop.TabIndex = 0;
            // 
            // labelTitle
            // 
            labelTitle.AutoSize = true;
            labelTitle.Font = new Font("Segoe UI", 28F, FontStyle.Bold);
            labelTitle.ForeColor = Color.FromArgb(29, 29, 31);
            labelTitle.Location = new Point(30, 20);
            labelTitle.Name = "labelTitle";
            labelTitle.Size = new Size(152, 62);
            labelTitle.TabIndex = 0;
            labelTitle.Text = "Bác sĩ";
            // 
            // labelCount
            // 
            labelCount.AutoSize = true;
            labelCount.Font = new Font("Segoe UI", 19.8000011F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelCount.ForeColor = Color.Black;
            labelCount.Location = new Point(188, 25);
            labelCount.Name = "labelCount";
            labelCount.Size = new Size(58, 46);
            labelCount.TabIndex = 3;
            labelCount.Text = "(0)";
            // 
            // buttonAdd
            // 
            buttonAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonAdd.BackColor = Color.FromArgb(0, 102, 204);
            buttonAdd.Cursor = Cursors.Hand;
            buttonAdd.FlatAppearance.BorderSize = 0;
            buttonAdd.FlatStyle = FlatStyle.Flat;
            buttonAdd.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            buttonAdd.ForeColor = Color.White;
            buttonAdd.Location = new Point(1220, 25);
            buttonAdd.Margin = new Padding(0);
            buttonAdd.Name = "buttonAdd";
            buttonAdd.Size = new Size(150, 40);
            buttonAdd.TabIndex = 0;
            buttonAdd.Text = "➕ Thêm bác sĩ";
            buttonAdd.UseVisualStyleBackColor = false;
            // 
            // panelSearchFilter
            // 
            panelSearchFilter.BackColor = Color.White;
            panelSearchFilter.Controls.Add(textBoxSearch);
            panelSearchFilter.Controls.Add(buttonFilter);
            panelSearchFilter.Dock = DockStyle.Top;
            panelSearchFilter.Location = new Point(0, 90);
            panelSearchFilter.Margin = new Padding(0);
            panelSearchFilter.Name = "panelSearchFilter";
            panelSearchFilter.Padding = new Padding(30, 15, 30, 15);
            panelSearchFilter.Size = new Size(1400, 60);
            panelSearchFilter.TabIndex = 1;
            // 
            // textBoxSearch
            // 
            textBoxSearch.BackColor = Color.FromArgb(245, 245, 247);
            textBoxSearch.BorderStyle = BorderStyle.None;
            textBoxSearch.Font = new Font("Segoe UI", 12F);
            textBoxSearch.ForeColor = Color.FromArgb(100, 100, 100);
            textBoxSearch.Location = new Point(30, 16);
            textBoxSearch.Name = "textBoxSearch";
            textBoxSearch.Size = new Size(420, 27);
            textBoxSearch.TabIndex = 1;
            textBoxSearch.Text = "🔍 Tìm kiếm...";
            // 
            // buttonFilter
            // 
            buttonFilter.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonFilter.BackColor = Color.White;
            buttonFilter.Cursor = Cursors.Hand;
            buttonFilter.FlatAppearance.BorderColor = Color.FromArgb(210, 210, 215);
            buttonFilter.FlatStyle = FlatStyle.Flat;
            buttonFilter.Font = new Font("Segoe UI", 11F);
            buttonFilter.ForeColor = Color.FromArgb(29, 29, 31);
            buttonFilter.Location = new Point(1220, 12);
            buttonFilter.Name = "buttonFilter";
            buttonFilter.Size = new Size(150, 36);
            buttonFilter.TabIndex = 2;
            buttonFilter.Text = "🔽 Bộ lọc";
            buttonFilter.UseVisualStyleBackColor = false;
            // 
            // panelMain
            // 
            panelMain.BackColor = Color.White;
            panelMain.Controls.Add(dataGridViewDoctors);
            panelMain.Dock = DockStyle.Fill;
            panelMain.Location = new Point(0, 150);
            panelMain.Margin = new Padding(0);
            panelMain.Name = "panelMain";
            panelMain.Padding = new Padding(25, 20, 25, 100);
            panelMain.Size = new Size(1400, 775);
            panelMain.TabIndex = 2;
            // 
            // dataGridViewDoctors
            // 
            dataGridViewDoctors.AllowUserToAddRows = false;
            dataGridViewDoctors.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(250, 250, 250);
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 10F);
            dataGridViewCellStyle1.ForeColor = Color.FromArgb(29, 29, 31);
            dataGridViewDoctors.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dataGridViewDoctors.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewDoctors.BackgroundColor = Color.White;
            dataGridViewDoctors.BorderStyle = BorderStyle.None;
            dataGridViewDoctors.CellBorderStyle = DataGridViewCellBorderStyle.None;
            dataGridViewDoctors.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(250, 250, 250);
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            dataGridViewCellStyle2.ForeColor = Color.FromArgb(134, 134, 139);
            dataGridViewCellStyle2.Padding = new Padding(0, 12, 0, 12);
            dataGridViewCellStyle2.SelectionBackColor = Color.FromArgb(250, 250, 250);
            dataGridViewCellStyle2.SelectionForeColor = Color.FromArgb(134, 134, 139);
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dataGridViewDoctors.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dataGridViewDoctors.ColumnHeadersHeight = 62;
            dataGridViewDoctors.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridViewDoctors.Columns.AddRange(new DataGridViewColumn[] { ColumnImage, ColumnName, ColumnEmail, ColumnPhone, ColumnSpecialty, ColumnStatus, ColumnActions });
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = Color.White;
            dataGridViewCellStyle3.Font = new Font("Segoe UI", 10F);
            dataGridViewCellStyle3.ForeColor = Color.FromArgb(29, 29, 31);
            dataGridViewCellStyle3.Padding = new Padding(0, 12, 0, 12);
            dataGridViewCellStyle3.SelectionBackColor = Color.FromArgb(230, 245, 255);
            dataGridViewCellStyle3.SelectionForeColor = Color.FromArgb(29, 29, 31);
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            dataGridViewDoctors.DefaultCellStyle = dataGridViewCellStyle3;
            dataGridViewDoctors.Dock = DockStyle.Fill;
            dataGridViewDoctors.EnableHeadersVisualStyles = false;
            dataGridViewDoctors.GridColor = Color.White;
            dataGridViewDoctors.Location = new Point(25, 20);
            dataGridViewDoctors.Margin = new Padding(0);
            dataGridViewDoctors.MultiSelect = false;
            dataGridViewDoctors.Name = "dataGridViewDoctors";
            dataGridViewDoctors.ReadOnly = true;
            dataGridViewDoctors.RowHeadersVisible = false;
            dataGridViewDoctors.RowHeadersWidth = 51;
            dataGridViewDoctors.RowTemplate.Height = 74;
            dataGridViewDoctors.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewDoctors.Size = new Size(1350, 655);
            dataGridViewDoctors.TabIndex = 0;
            // 
            // ColumnImage
            // 
            ColumnImage.FillWeight = 8F;
            ColumnImage.HeaderText = "";
            ColumnImage.ImageLayout = DataGridViewImageCellLayout.Zoom;
            ColumnImage.MinimumWidth = 50;
            ColumnImage.Name = "ColumnImage";
            ColumnImage.ReadOnly = true;
            // 
            // ColumnName
            // 
            ColumnName.FillWeight = 25F;
            ColumnName.HeaderText = "Bác sĩ";
            ColumnName.MinimumWidth = 150;
            ColumnName.Name = "ColumnName";
            ColumnName.ReadOnly = true;
            // 
            // ColumnEmail
            // 
            ColumnEmail.FillWeight = 25F;
            ColumnEmail.HeaderText = "Email";
            ColumnEmail.MinimumWidth = 150;
            ColumnEmail.Name = "ColumnEmail";
            ColumnEmail.ReadOnly = true;
            // 
            // ColumnPhone
            // 
            ColumnPhone.FillWeight = 15F;
            ColumnPhone.HeaderText = "Điện thoại";
            ColumnPhone.MinimumWidth = 120;
            ColumnPhone.Name = "ColumnPhone";
            ColumnPhone.ReadOnly = true;
            // 
            // ColumnSpecialty
            // 
            ColumnSpecialty.FillWeight = 18F;
            ColumnSpecialty.HeaderText = "Chuyên khoa";
            ColumnSpecialty.MinimumWidth = 130;
            ColumnSpecialty.Name = "ColumnSpecialty";
            ColumnSpecialty.ReadOnly = true;
            // 
            // ColumnStatus
            // 
            ColumnStatus.FillWeight = 12F;
            ColumnStatus.HeaderText = "Trạng Thái";
            ColumnStatus.MinimumWidth = 100;
            ColumnStatus.Name = "ColumnStatus";
            ColumnStatus.ReadOnly = true;
            // 
            // ColumnActions
            // 
            ColumnActions.FillWeight = 5F;
            ColumnActions.HeaderText = "";
            ColumnActions.MinimumWidth = 40;
            ColumnActions.Name = "ColumnActions";
            ColumnActions.ReadOnly = true;
            ColumnActions.Text = "···";
            ColumnActions.UseColumnTextForButtonValue = true;
            // 
            // Doctor
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(245, 245, 247);
            ClientSize = new Size(1400, 925);
            Controls.Add(panelMain);
            Controls.Add(panelSearchFilter);
            Controls.Add(panelTop);
            Margin = new Padding(3, 4, 3, 4);
            Name = "Doctor";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Quản lý Bác sĩ";
            panelTop.ResumeLayout(false);
            panelTop.PerformLayout();
            panelSearchFilter.ResumeLayout(false);
            panelSearchFilter.PerformLayout();
            panelMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridViewDoctors).EndInit();
            ResumeLayout(false);
        }

        #endregion

        // =============== CONTROL DECLARATIONS ===============
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Label labelCount;
        private System.Windows.Forms.Button buttonAdd;
        
        private System.Windows.Forms.Panel panelSearchFilter;
        private System.Windows.Forms.TextBox textBoxSearch;
        
        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.DataGridView dataGridViewDoctors;
        private System.Windows.Forms.DataGridViewImageColumn ColumnImage;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnName;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnEmail;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnPhone;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnSpecialty;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnStatus;
        private System.Windows.Forms.DataGridViewButtonColumn ColumnActions;
        private Button buttonFilter;
    }
}