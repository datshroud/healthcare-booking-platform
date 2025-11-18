namespace BookingCareManagement.WinForms.Areas.Admin.Forms
{
    public partial class Specialty
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            panelTop = new Panel();
            labelCount = new Label();
            textBoxSearch = new TextBox();
            labelTitle = new Label();
            panelControls = new Panel();
            buttonDelete = new Button();
            buttonEdit = new Button();
            buttonAdd = new Button();
            panelMain = new Panel();
            dataGridViewSpecialties = new DataGridView();
            ColumnImage = new DataGridViewImageColumn();
            ColumnName = new DataGridViewTextBoxColumn();
            ColumnDoctors = new DataGridViewTextBoxColumn();
            ColumnPrice = new DataGridViewTextBoxColumn();
            panelTop.SuspendLayout();
            panelControls.SuspendLayout();
            panelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridViewSpecialties).BeginInit();
            SuspendLayout();
            // 
            // panelTop
            // 
            panelTop.BackColor = Color.FromArgb(23, 162, 184);
            panelTop.Controls.Add(labelCount);
            panelTop.Controls.Add(textBoxSearch);
            panelTop.Controls.Add(labelTitle);
            panelTop.Dock = DockStyle.Top;
            panelTop.Location = new Point(0, 0);
            panelTop.Margin = new Padding(3, 4, 3, 4);
            panelTop.Name = "panelTop";
            panelTop.Size = new Size(1600, 100);
            panelTop.TabIndex = 0;
            // 
            // labelCount
            // 
            labelCount.AutoSize = true;
            labelCount.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelCount.ForeColor = Color.White;
            labelCount.Location = new Point(235, 30);
            labelCount.Name = "labelCount";
            labelCount.Size = new Size(57, 41);
            labelCount.TabIndex = 3;
            labelCount.Text = "(0)";
            // 
            // textBoxSearch
            // 
            textBoxSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            textBoxSearch.BorderStyle = BorderStyle.FixedSingle;
            textBoxSearch.Font = new Font("Segoe UI", 12F);
            textBoxSearch.ForeColor = Color.Gray;
            textBoxSearch.Location = new Point(1150, 31);
            textBoxSearch.Margin = new Padding(3, 4, 3, 4);
            textBoxSearch.Name = "textBoxSearch";
            textBoxSearch.Size = new Size(400, 34);
            textBoxSearch.TabIndex = 1;
            textBoxSearch.Text = "Tìm kiếm chuyên khoa...";
            // 
            // labelTitle
            // 
            labelTitle.AutoSize = true;
            labelTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            labelTitle.ForeColor = Color.White;
            labelTitle.Location = new Point(27, 30);
            labelTitle.Name = "labelTitle";
            labelTitle.Size = new Size(202, 41);
            labelTitle.TabIndex = 0;
            labelTitle.Text = "Chuyên Khoa";
            // 
            // panelControls
            // 
            panelControls.BackColor = Color.White;
            panelControls.Controls.Add(buttonDelete);
            panelControls.Controls.Add(buttonEdit);
            panelControls.Controls.Add(buttonAdd);
            panelControls.Dock = DockStyle.Top;
            panelControls.Location = new Point(0, 100);
            panelControls.Margin = new Padding(3, 4, 3, 4);
            panelControls.Name = "panelControls";
            panelControls.Padding = new Padding(27, 22, 27, 22);
            panelControls.Size = new Size(1600, 100);
            panelControls.TabIndex = 1;
            // 
            // buttonDelete
            // 
            buttonDelete.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonDelete.BackColor = Color.Teal;
            buttonDelete.FlatAppearance.BorderSize = 0;
            buttonDelete.FlatStyle = FlatStyle.Flat;
            buttonDelete.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            buttonDelete.ForeColor = Color.White;
            buttonDelete.Location = new Point(1423, 20);
            buttonDelete.Margin = new Padding(3, 4, 3, 4);
            buttonDelete.Name = "buttonDelete";
            buttonDelete.Size = new Size(150, 56);
            buttonDelete.TabIndex = 2;
            buttonDelete.Text = "🗑️ Xóa";
            buttonDelete.UseVisualStyleBackColor = false;
            // 
            // buttonEdit
            // 
            buttonEdit.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonEdit.BackColor = Color.Teal;
            buttonEdit.FlatAppearance.BorderSize = 0;
            buttonEdit.FlatStyle = FlatStyle.Flat;
            buttonEdit.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            buttonEdit.ForeColor = Color.White;
            buttonEdit.Location = new Point(1258, 20);
            buttonEdit.Margin = new Padding(3, 4, 3, 4);
            buttonEdit.Name = "buttonEdit";
            buttonEdit.Size = new Size(150, 56);
            buttonEdit.TabIndex = 1;
            buttonEdit.Text = "✏️ Sửa";
            buttonEdit.UseVisualStyleBackColor = false;
            // 
            // buttonAdd
            // 
            buttonAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonAdd.BackColor = Color.Teal;
            buttonAdd.FlatAppearance.BorderSize = 0;
            buttonAdd.FlatStyle = FlatStyle.Flat;
            buttonAdd.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            buttonAdd.ForeColor = Color.White;
            buttonAdd.Location = new Point(1093, 20);
            buttonAdd.Margin = new Padding(3, 4, 3, 4);
            buttonAdd.Name = "buttonAdd";
            buttonAdd.Size = new Size(150, 56);
            buttonAdd.TabIndex = 0;
            buttonAdd.Text = "➕ Thêm";
            buttonAdd.UseVisualStyleBackColor = false;
            // 
            // panelMain
            // 
            panelMain.BackColor = Color.White;
            panelMain.Controls.Add(dataGridViewSpecialties);
            panelMain.Dock = DockStyle.Fill;
            panelMain.Location = new Point(0, 200);
            panelMain.Margin = new Padding(3, 4, 3, 4);
            panelMain.Name = "panelMain";
            panelMain.Padding = new Padding(27, 31, 27, 31);
            panelMain.Size = new Size(1600, 855);
            panelMain.TabIndex = 2;
            // 
            // dataGridViewSpecialties
            // 
            dataGridViewSpecialties.AllowUserToAddRows = false;
            dataGridViewSpecialties.AllowUserToDeleteRows = false;
            dataGridViewSpecialties.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewSpecialties.BackgroundColor = Color.White;
            dataGridViewSpecialties.BorderStyle = BorderStyle.None;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(23, 162, 184);
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            dataGridViewCellStyle1.ForeColor = Color.White;
            dataGridViewCellStyle1.SelectionBackColor = Color.FromArgb(23, 162, 184);
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            dataGridViewSpecialties.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dataGridViewSpecialties.ColumnHeadersHeight = 45;
            dataGridViewSpecialties.Columns.AddRange(new DataGridViewColumn[] { ColumnImage, ColumnName, ColumnDoctors, ColumnPrice });
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = SystemColors.Window;
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 10F);
            dataGridViewCellStyle2.ForeColor = Color.FromArgb(64, 64, 64);
            dataGridViewCellStyle2.SelectionBackColor = Color.FromArgb(230, 247, 255);
            dataGridViewCellStyle2.SelectionForeColor = Color.FromArgb(23, 162, 184);
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
            dataGridViewSpecialties.DefaultCellStyle = dataGridViewCellStyle2;
            dataGridViewSpecialties.Dock = DockStyle.Fill;
            dataGridViewSpecialties.EnableHeadersVisualStyles = false;
            dataGridViewSpecialties.GridColor = Color.FromArgb(220, 220, 220);
            dataGridViewSpecialties.Location = new Point(27, 31);
            dataGridViewSpecialties.Margin = new Padding(3, 4, 3, 4);
            dataGridViewSpecialties.MultiSelect = false;
            dataGridViewSpecialties.Name = "dataGridViewSpecialties";
            dataGridViewSpecialties.ReadOnly = true;
            dataGridViewSpecialties.RowHeadersVisible = false;
            dataGridViewSpecialties.RowHeadersWidth = 51;
            dataGridViewSpecialties.RowTemplate.Height = 80;
            dataGridViewSpecialties.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewSpecialties.Size = new Size(1546, 793);
            dataGridViewSpecialties.TabIndex = 0;
            // 
            // ColumnImage
            // 
            ColumnImage.FillWeight = 20F;
            ColumnImage.HeaderText = "Ảnh";
            ColumnImage.ImageLayout = DataGridViewImageCellLayout.Zoom;
            ColumnImage.MinimumWidth = 6;
            ColumnImage.Name = "ColumnImage";
            ColumnImage.ReadOnly = true;
            ColumnImage.Resizable = DataGridViewTriState.True;
            ColumnImage.SortMode = DataGridViewColumnSortMode.Automatic;
            // 
            // ColumnName
            // 
            ColumnName.FillWeight = 30F;
            ColumnName.HeaderText = "Tên Chuyên Khoa";
            ColumnName.MinimumWidth = 6;
            ColumnName.Name = "ColumnName";
            ColumnName.ReadOnly = true;
            // 
            // ColumnDoctors
            // 
            ColumnDoctors.FillWeight = 35F;
            ColumnDoctors.HeaderText = "Bác Sĩ";
            ColumnDoctors.MinimumWidth = 6;
            ColumnDoctors.Name = "ColumnDoctors";
            ColumnDoctors.ReadOnly = true;
            // 
            // ColumnPrice
            // 
            ColumnPrice.FillWeight = 15F;
            ColumnPrice.HeaderText = "Giá Tiền";
            ColumnPrice.MinimumWidth = 6;
            ColumnPrice.Name = "ColumnPrice";
            ColumnPrice.ReadOnly = true;
            // 
            // Specialty
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1600, 1055);
            Controls.Add(panelMain);
            Controls.Add(panelControls);
            Controls.Add(panelTop);
            Margin = new Padding(3, 4, 3, 4);
            Name = "Specialty";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Quản lý chuyên khoa";
            panelTop.ResumeLayout(false);
            panelTop.PerformLayout();
            panelControls.ResumeLayout(false);
            panelMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridViewSpecialties).EndInit();
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.TextBox textBoxSearch;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Panel panelControls;
        private System.Windows.Forms.Button buttonDelete;
        private System.Windows.Forms.Button buttonEdit;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.DataGridView dataGridViewSpecialties;
        private System.Windows.Forms.DataGridViewImageColumn ColumnImage;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnName;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnDoctors;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnPrice;
        private System.Windows.Forms.Label labelCount;
    }
}