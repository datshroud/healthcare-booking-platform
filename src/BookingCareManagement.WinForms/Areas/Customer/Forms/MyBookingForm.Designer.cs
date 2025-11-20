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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.panelHeader = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.panelActions = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.panelBody = new System.Windows.Forms.Panel();
            this.dgvBookings = new System.Windows.Forms.DataGridView();

            // Khai báo các cột
            this.colDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDoctor = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSpecialty = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStatus = new System.Windows.Forms.DataGridViewTextBoxColumn(); // Đổi vị trí lên trên
            this.colPrice = new System.Windows.Forms.DataGridViewTextBoxColumn(); // Đổi vị trí xuống dưới

            this.panelHeader.SuspendLayout();
            this.panelActions.SuspendLayout();
            this.panelBody.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBookings)).BeginInit();
            this.SuspendLayout();
            // 
            // panelHeader
            // 
            this.panelHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(162)))), ((int)(((byte)(184)))));
            this.panelHeader.Controls.Add(this.lblTitle);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Location = new System.Drawing.Point(0, 0);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Padding = new System.Windows.Forms.Padding(30, 0, 30, 0);
            this.panelHeader.Size = new System.Drawing.Size(1000, 80);
            this.panelHeader.TabIndex = 2;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(25, 20);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(230, 46);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "My Bookings";
            // 
            // panelActions
            // 
            this.panelActions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(250)))));
            this.panelActions.Controls.Add(this.btnCancel);
            this.panelActions.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelActions.Location = new System.Drawing.Point(0, 80);
            this.panelActions.Name = "panelActions";
            this.panelActions.Size = new System.Drawing.Size(1000, 60);
            this.panelActions.TabIndex = 1;
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(53)))), ((int)(((byte)(69)))));
            this.btnCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCancel.FlatAppearance.BorderSize = 0;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnCancel.ForeColor = System.Drawing.Color.White;
            this.btnCancel.Location = new System.Drawing.Point(30, 10);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(150, 40);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "Hủy Lịch Hẹn";
            this.btnCancel.UseVisualStyleBackColor = false;
            // 
            // panelBody
            // 
            this.panelBody.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(250)))));
            this.panelBody.Controls.Add(this.dgvBookings);
            this.panelBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBody.Location = new System.Drawing.Point(0, 140);
            this.panelBody.Name = "panelBody";
            this.panelBody.Padding = new System.Windows.Forms.Padding(30, 0, 30, 20);
            this.panelBody.Size = new System.Drawing.Size(1000, 510);
            this.panelBody.TabIndex = 0;
            // 
            // dgvBookings
            // 
            this.dgvBookings.AllowUserToAddRows = false;
            this.dgvBookings.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvBookings.BackgroundColor = System.Drawing.Color.White;
            this.dgvBookings.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvBookings.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.dgvBookings.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;

            // Style Header
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))));
            dataGridViewCellStyle1.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.dgvBookings.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvBookings.ColumnHeadersHeight = 50;

            // Add columns: Date, Time, Doctor, Specialty, Status, Price
            this.dgvBookings.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colDate,
            this.colTime,
            this.colDoctor,
            this.colSpecialty,
            this.colStatus,
            this.colPrice});

            // Style Default Cell
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 11F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            dataGridViewCellStyle2.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(245)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvBookings.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvBookings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvBookings.EnableHeadersVisualStyles = false;
            this.dgvBookings.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.dgvBookings.Location = new System.Drawing.Point(30, 0);
            this.dgvBookings.Name = "dgvBookings";
            this.dgvBookings.RowHeadersVisible = false;
            this.dgvBookings.RowHeadersWidth = 51;
            this.dgvBookings.RowTemplate.Height = 60;
            this.dgvBookings.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvBookings.Size = new System.Drawing.Size(940, 490);
            this.dgvBookings.TabIndex = 0;
            // 
            // colDate
            // 
            this.colDate.FillWeight = 20F;
            this.colDate.HeaderText = "NGÀY KHÁM";
            this.colDate.MinimumWidth = 6;
            this.colDate.Name = "colDate";
            // 
            // colTime
            // 
            this.colTime.FillWeight = 15F;
            this.colTime.HeaderText = "GIỜ";
            this.colTime.MinimumWidth = 6;
            this.colTime.Name = "colTime";
            // 
            // colDoctor
            // 
            this.colDoctor.FillWeight = 25F;
            this.colDoctor.HeaderText = "BÁC SĨ";
            this.colDoctor.MinimumWidth = 6;
            this.colDoctor.Name = "colDoctor";
            // 
            // colSpecialty
            // 
            this.colSpecialty.FillWeight = 20F;
            this.colSpecialty.HeaderText = "CHUYÊN KHOA";
            this.colSpecialty.MinimumWidth = 6;
            this.colSpecialty.Name = "colSpecialty";
            // 
            // colStatus
            // 
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(193)))), ((int)(((byte)(7)))));
            this.colStatus.DefaultCellStyle = dataGridViewCellStyle3;
            this.colStatus.FillWeight = 15F;
            this.colStatus.HeaderText = "TRẠNG THÁI";
            this.colStatus.MinimumWidth = 6;
            this.colStatus.Name = "colStatus";
            // 
            // colPrice
            // 
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle4.ForeColor = System.Drawing.Color.SeaGreen;
            dataGridViewCellStyle4.Format = "C0";
            this.colPrice.DefaultCellStyle = dataGridViewCellStyle4;
            this.colPrice.FillWeight = 15F;
            this.colPrice.HeaderText = "THANH TOÁN";
            this.colPrice.MinimumWidth = 6;
            this.colPrice.Name = "colPrice";
            // 
            // MyBookingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 650);
            this.Controls.Add(this.panelBody);
            this.Controls.Add(this.panelActions);
            this.Controls.Add(this.panelHeader);
            this.Name = "MyBookingForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Lịch Hẹn Của Tôi";
            this.panelHeader.ResumeLayout(false);
            this.panelHeader.PerformLayout();
            this.panelActions.ResumeLayout(false);
            this.panelBody.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvBookings)).EndInit();
            this.ResumeLayout(false);
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