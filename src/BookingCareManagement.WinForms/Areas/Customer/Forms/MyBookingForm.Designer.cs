namespace BookingCareManagement.WinForms.Areas.Customer.Forms
{
    partial class MyBookingForm
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
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            panelHeader = new Panel();
            lblTitle = new Label();
            panelActions = new Panel();
            btnCancel = new Button();
            panelBody = new Panel();
            dgvBookings = new DataGridView();
            colDate = new DataGridViewTextBoxColumn();
            colTime = new DataGridViewTextBoxColumn();
            colDoctor = new DataGridViewTextBoxColumn();
            colSpecialty = new DataGridViewTextBoxColumn();
            colStatus = new DataGridViewTextBoxColumn();
            colPrice = new DataGridViewTextBoxColumn();
            panelHeader.SuspendLayout();
            panelActions.SuspendLayout();
            panelBody.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvBookings).BeginInit();
            SuspendLayout();
            // 
            // panelHeader
            // 
            panelHeader.BackColor = Color.FromArgb(23, 162, 184);
            panelHeader.Controls.Add(lblTitle);
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Location = new Point(0, 0);
            panelHeader.Margin = new Padding(3, 4, 3, 4);
            panelHeader.Name = "panelHeader";
            panelHeader.Padding = new Padding(30, 0, 30, 0);
            panelHeader.Size = new Size(1000, 100);
            panelHeader.TabIndex = 2;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(25, 25);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(200, 46);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Lịch của tôi";
            // 
            // panelActions
            // 
            panelActions.BackColor = Color.FromArgb(248, 249, 250);
            panelActions.Controls.Add(btnCancel);
            panelActions.Dock = DockStyle.Top;
            panelActions.Location = new Point(0, 100);
            panelActions.Margin = new Padding(3, 4, 3, 4);
            panelActions.Name = "panelActions";
            panelActions.Size = new Size(1000, 75);
            panelActions.TabIndex = 1;
            // 
            // btnCancel
            // 
            btnCancel.BackColor = Color.FromArgb(220, 53, 69);
            btnCancel.Cursor = Cursors.Hand;
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnCancel.ForeColor = Color.White;
            btnCancel.Location = new Point(30, 12);
            btnCancel.Margin = new Padding(3, 4, 3, 4);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(150, 50);
            btnCancel.TabIndex = 0;
            btnCancel.Text = "Hủy Lịch Hẹn";
            btnCancel.UseVisualStyleBackColor = false;
            // 
            // panelBody
            // 
            panelBody.BackColor = Color.FromArgb(248, 249, 250);
            panelBody.Controls.Add(dgvBookings);
            panelBody.Dock = DockStyle.Fill;
            panelBody.Location = new Point(0, 175);
            panelBody.Margin = new Padding(3, 4, 3, 4);
            panelBody.Name = "panelBody";
            panelBody.Padding = new Padding(30, 0, 30, 25);
            panelBody.Size = new Size(1000, 637);
            panelBody.TabIndex = 0;
            // 
            // dgvBookings
            // 
            dgvBookings.AllowUserToAddRows = false;
            dgvBookings.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvBookings.BackgroundColor = Color.White;
            dgvBookings.BorderStyle = BorderStyle.None;
            dgvBookings.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvBookings.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle1.BackColor = Color.White;
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dataGridViewCellStyle1.ForeColor = Color.FromArgb(160, 160, 160);
            dataGridViewCellStyle1.Padding = new Padding(10, 0, 0, 0);
            dgvBookings.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dgvBookings.ColumnHeadersHeight = 50;
            dgvBookings.Columns.AddRange(new DataGridViewColumn[] { colDate, colTime, colDoctor, colSpecialty, colStatus, colPrice });
            dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = Color.White;
            dataGridViewCellStyle4.Font = new Font("Segoe UI", 11F);
            dataGridViewCellStyle4.ForeColor = Color.FromArgb(50, 50, 50);
            dataGridViewCellStyle4.Padding = new Padding(10, 0, 0, 0);
            dataGridViewCellStyle4.SelectionBackColor = Color.FromArgb(235, 245, 255);
            dataGridViewCellStyle4.SelectionForeColor = Color.Black;
            dataGridViewCellStyle4.WrapMode = DataGridViewTriState.False;
            dgvBookings.DefaultCellStyle = dataGridViewCellStyle4;
            dgvBookings.Dock = DockStyle.Fill;
            dgvBookings.EnableHeadersVisualStyles = false;
            dgvBookings.GridColor = Color.FromArgb(240, 240, 240);
            dgvBookings.Location = new Point(30, 0);
            dgvBookings.Margin = new Padding(3, 4, 3, 4);
            dgvBookings.Name = "dgvBookings";
            dgvBookings.RowHeadersVisible = false;
            dgvBookings.RowHeadersWidth = 51;
            dgvBookings.RowTemplate.Height = 60;
            dgvBookings.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvBookings.Size = new Size(940, 612);
            dgvBookings.TabIndex = 0;
            // 
            // colDate
            // 
            colDate.FillWeight = 20F;
            colDate.HeaderText = "NGÀY KHÁM";
            colDate.MinimumWidth = 6;
            colDate.Name = "colDate";
            // 
            // colTime
            // 
            colTime.FillWeight = 15F;
            colTime.HeaderText = "GIỜ";
            colTime.MinimumWidth = 6;
            colTime.Name = "colTime";
            // 
            // colDoctor
            // 
            colDoctor.FillWeight = 25F;
            colDoctor.HeaderText = "BÁC SĨ";
            colDoctor.MinimumWidth = 6;
            colDoctor.Name = "colDoctor";
            // 
            // colSpecialty
            // 
            colSpecialty.FillWeight = 20F;
            colSpecialty.HeaderText = "CHUYÊN KHOA";
            colSpecialty.MinimumWidth = 6;
            colSpecialty.Name = "colSpecialty";
            // 
            // colStatus
            // 
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dataGridViewCellStyle2.ForeColor = Color.FromArgb(255, 193, 7);
            colStatus.DefaultCellStyle = dataGridViewCellStyle2;
            colStatus.FillWeight = 15F;
            colStatus.HeaderText = "TRẠNG THÁI";
            colStatus.MinimumWidth = 6;
            colStatus.Name = "colStatus";
            // 
            // colPrice
            // 
            dataGridViewCellStyle3.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            dataGridViewCellStyle3.ForeColor = Color.SeaGreen;
            dataGridViewCellStyle3.Format = "C0";
            colPrice.DefaultCellStyle = dataGridViewCellStyle3;
            colPrice.FillWeight = 15F;
            colPrice.HeaderText = "THANH TOÁN";
            colPrice.MinimumWidth = 6;
            colPrice.Name = "colPrice";
            // 
            // MyBookingForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1000, 812);
            Controls.Add(panelBody);
            Controls.Add(panelActions);
            Controls.Add(panelHeader);
            Margin = new Padding(3, 4, 3, 4);
            Name = "MyBookingForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Lịch Hẹn Của Tôi";
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            panelActions.ResumeLayout(false);
            panelBody.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvBookings).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Panel panelActions;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Panel panelBody;
        private System.Windows.Forms.DataGridView dgvBookings;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDate;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDoctor;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSpecialty;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStatus; // Status
        private System.Windows.Forms.DataGridViewTextBoxColumn colPrice; // Price
    }
}