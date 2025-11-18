namespace BookingCareManagement.WinForms.Areas.Customer.Forms
{
    partial class Bookings
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
            this.panelTop = new System.Windows.Forms.Panel();
            this.labelTitle = new System.Windows.Forms.Label();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.labelSummary = new System.Windows.Forms.Label();
            this.buttonBookAppointment = new System.Windows.Forms.Button();
            this.panelMain = new System.Windows.Forms.Panel();
            this.groupBoxStep4 = new System.Windows.Forms.GroupBox();
            this.textBoxEmail = new System.Windows.Forms.TextBox();
            this.labelEmail = new System.Windows.Forms.Label();
            this.textBoxPhone = new System.Windows.Forms.TextBox();
            this.labelPhone = new System.Windows.Forms.Label();
            this.textBoxFullName = new System.Windows.Forms.TextBox();
            this.labelFullName = new System.Windows.Forms.Label();
            this.labelStep4 = new System.Windows.Forms.Label();
            this.groupBoxStep3 = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanelTimeSlots = new System.Windows.Forms.FlowLayoutPanel();
            this.dateTimePickerAppointment = new System.Windows.Forms.DateTimePicker();
            this.labelSelectTime = new System.Windows.Forms.Label();
            this.labelStep3 = new System.Windows.Forms.Label();
            this.groupBoxStep2 = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanelDoctors = new System.Windows.Forms.FlowLayoutPanel();
            this.labelStep2 = new System.Windows.Forms.Label();
            this.groupBoxStep1 = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanelSpecialties = new System.Windows.Forms.FlowLayoutPanel();
            this.labelStep1 = new System.Windows.Forms.Label();
            this.panelTop.SuspendLayout();
            this.panelBottom.SuspendLayout();
            this.panelMain.SuspendLayout();
            this.groupBoxStep4.SuspendLayout();
            this.groupBoxStep3.SuspendLayout();
            this.groupBoxStep2.SuspendLayout();
            this.groupBoxStep1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(162)))), ((int)(((byte)(184)))));
            this.panelTop.Controls.Add(this.labelTitle);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1000, 70);
            this.panelTop.TabIndex = 0;
            // 
            // labelTitle
            // 
            this.labelTitle.AutoSize = true;
            this.labelTitle.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.labelTitle.ForeColor = System.Drawing.Color.White;
            this.labelTitle.Location = new System.Drawing.Point(20, 18);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(186, 32);
            this.labelTitle.TabIndex = 0;
            this.labelTitle.Text = "ĐẶT LỊCH HẸN";
            // 
            // panelBottom
            // 
            this.panelBottom.BackColor = System.Drawing.Color.WhiteSmoke;
            this.panelBottom.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelBottom.Controls.Add(this.labelSummary);
            this.panelBottom.Controls.Add(this.buttonBookAppointment);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 620);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(1000, 80);
            this.panelBottom.TabIndex = 2;
            // 
            // labelSummary
            // 
            this.labelSummary.AutoSize = true;
            this.labelSummary.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Italic);
            this.labelSummary.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.labelSummary.Location = new System.Drawing.Point(20, 30);
            this.labelSummary.MaximumSize = new System.Drawing.Size(700, 0);
            this.labelSummary.Name = "labelSummary";
            this.labelSummary.Size = new System.Drawing.Size(278, 20);
            this.labelSummary.TabIndex = 1;
            this.labelSummary.Text = "Vui lòng hoàn thành các bước để đặt lịch";
            // 
            // buttonBookAppointment
            // 
            this.buttonBookAppointment.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBookAppointment.BackColor = System.Drawing.Color.Gray;
            this.buttonBookAppointment.Enabled = false;
            this.buttonBookAppointment.FlatAppearance.BorderSize = 0;
            this.buttonBookAppointment.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonBookAppointment.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.buttonBookAppointment.ForeColor = System.Drawing.Color.White;
            this.buttonBookAppointment.Location = new System.Drawing.Point(780, 15);
            this.buttonBookAppointment.Name = "buttonBookAppointment";
            this.buttonBookAppointment.Size = new System.Drawing.Size(200, 50);
            this.buttonBookAppointment.TabIndex = 0;
            this.buttonBookAppointment.Text = "XÁC NHẬN";
            this.buttonBookAppointment.UseVisualStyleBackColor = false;
            // 
            // panelMain
            // 
            this.panelMain.AutoScroll = true;
            this.panelMain.BackColor = System.Drawing.Color.White;
            this.panelMain.Controls.Add(this.groupBoxStep4);
            this.panelMain.Controls.Add(this.groupBoxStep3);
            this.panelMain.Controls.Add(this.groupBoxStep2);
            this.panelMain.Controls.Add(this.groupBoxStep1);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(0, 70);
            this.panelMain.Name = "panelMain";
            this.panelMain.Padding = new System.Windows.Forms.Padding(20);
            this.panelMain.Size = new System.Drawing.Size(1000, 550);
            this.panelMain.TabIndex = 1;
            // 
            // groupBoxStep4
            // 
            this.groupBoxStep4.Controls.Add(this.textBoxEmail);
            this.groupBoxStep4.Controls.Add(this.labelEmail);
            this.groupBoxStep4.Controls.Add(this.textBoxPhone);
            this.groupBoxStep4.Controls.Add(this.labelPhone);
            this.groupBoxStep4.Controls.Add(this.textBoxFullName);
            this.groupBoxStep4.Controls.Add(this.labelFullName);
            this.groupBoxStep4.Controls.Add(this.labelStep4);
            this.groupBoxStep4.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxStep4.Enabled = false;
            this.groupBoxStep4.Location = new System.Drawing.Point(20, 650);
            this.groupBoxStep4.Name = "groupBoxStep4";
            this.groupBoxStep4.Size = new System.Drawing.Size(943, 220);
            this.groupBoxStep4.TabIndex = 3;
            this.groupBoxStep4.TabStop = false;
            // 
            // textBoxEmail
            // 
            this.textBoxEmail.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxEmail.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.textBoxEmail.Location = new System.Drawing.Point(160, 158);
            this.textBoxEmail.Name = "textBoxEmail";
            this.textBoxEmail.Size = new System.Drawing.Size(350, 27);
            this.textBoxEmail.TabIndex = 6;
            // 
            // labelEmail
            // 
            this.labelEmail.AutoSize = true;
            this.labelEmail.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.labelEmail.Location = new System.Drawing.Point(40, 160);
            this.labelEmail.Name = "labelEmail";
            this.labelEmail.Size = new System.Drawing.Size(49, 20);
            this.labelEmail.TabIndex = 5;
            this.labelEmail.Text = "Email:";
            // 
            // textBoxPhone
            // 
            this.textBoxPhone.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxPhone.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.textBoxPhone.Location = new System.Drawing.Point(160, 108);
            this.textBoxPhone.Name = "textBoxPhone";
            this.textBoxPhone.Size = new System.Drawing.Size(350, 27);
            this.textBoxPhone.TabIndex = 4;
            // 
            // labelPhone
            // 
            this.labelPhone.AutoSize = true;
            this.labelPhone.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.labelPhone.Location = new System.Drawing.Point(40, 110);
            this.labelPhone.Name = "labelPhone";
            this.labelPhone.Size = new System.Drawing.Size(100, 20);
            this.labelPhone.TabIndex = 3;
            this.labelPhone.Text = "Số điện thoại:";
            // 
            // textBoxFullName
            // 
            this.textBoxFullName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxFullName.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.textBoxFullName.Location = new System.Drawing.Point(160, 58);
            this.textBoxFullName.Name = "textBoxFullName";
            this.textBoxFullName.Size = new System.Drawing.Size(350, 27);
            this.textBoxFullName.TabIndex = 2;
            // 
            // labelFullName
            // 
            this.labelFullName.AutoSize = true;
            this.labelFullName.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.labelFullName.Location = new System.Drawing.Point(40, 60);
            this.labelFullName.Name = "labelFullName";
            this.labelFullName.Size = new System.Drawing.Size(76, 20);
            this.labelFullName.TabIndex = 1;
            this.labelFullName.Text = "Họ và tên:";
            // 
            // labelStep4
            // 
            this.labelStep4.AutoSize = true;
            this.labelStep4.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.labelStep4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(162)))), ((int)(((byte)(184)))));
            this.labelStep4.Location = new System.Drawing.Point(15, 15);
            this.labelStep4.Name = "labelStep4";
            this.labelStep4.Size = new System.Drawing.Size(250, 25);
            this.labelStep4.TabIndex = 0;
            this.labelStep4.Text = "Thông tin cá nhân";
            // 
            // groupBoxStep3
            // 
            this.groupBoxStep3.Controls.Add(this.flowLayoutPanelTimeSlots);
            this.groupBoxStep3.Controls.Add(this.dateTimePickerAppointment);
            this.groupBoxStep3.Controls.Add(this.labelSelectTime);
            this.groupBoxStep3.Controls.Add(this.labelStep3);
            this.groupBoxStep3.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxStep3.Enabled = false;
            this.groupBoxStep3.Location = new System.Drawing.Point(20, 410);
            this.groupBoxStep3.Name = "groupBoxStep3";
            this.groupBoxStep3.Padding = new System.Windows.Forms.Padding(3, 80, 3, 3);
            this.groupBoxStep3.Size = new System.Drawing.Size(943, 240);
            this.groupBoxStep3.TabIndex = 2;
            this.groupBoxStep3.TabStop = false;
            // 
            // flowLayoutPanelTimeSlots
            // 
            this.flowLayoutPanelTimeSlots.AutoScroll = true;
            this.flowLayoutPanelTimeSlots.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanelTimeSlots.Location = new System.Drawing.Point(3, 93);
            this.flowLayoutPanelTimeSlots.Name = "flowLayoutPanelTimeSlots";
            this.flowLayoutPanelTimeSlots.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.flowLayoutPanelTimeSlots.Size = new System.Drawing.Size(937, 144);
            this.flowLayoutPanelTimeSlots.TabIndex = 3;
            // 
            // dateTimePickerAppointment
            // 
            this.dateTimePickerAppointment.CustomFormat = "dd/MM/yyyy";
            this.dateTimePickerAppointment.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.dateTimePickerAppointment.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePickerAppointment.Location = new System.Drawing.Point(160, 48);
            this.dateTimePickerAppointment.Name = "dateTimePickerAppointment";
            this.dateTimePickerAppointment.Size = new System.Drawing.Size(200, 27);
            this.dateTimePickerAppointment.TabIndex = 2;
            // 
            // labelSelectTime
            // 
            this.labelSelectTime.AutoSize = true;
            this.labelSelectTime.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.labelSelectTime.Location = new System.Drawing.Point(40, 50);
            this.labelSelectTime.Name = "labelSelectTime";
            this.labelSelectTime.Size = new System.Drawing.Size(84, 20);
            this.labelSelectTime.TabIndex = 1;
            this.labelSelectTime.Text = "Chọn ngày:";
            // 
            // labelStep3
            // 
            this.labelStep3.AutoSize = true;
            this.labelStep3.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.labelStep3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(162)))), ((int)(((byte)(184)))));
            this.labelStep3.Location = new System.Drawing.Point(15, 15);
            this.labelStep3.Name = "labelStep3";
            this.labelStep3.Size = new System.Drawing.Size(238, 25);
            this.labelStep3.TabIndex = 0;
            this.labelStep3.Text = "Chọn ngày và giờ";
            // 
            // groupBoxStep2
            // 
            this.groupBoxStep2.Controls.Add(this.flowLayoutPanelDoctors);
            this.groupBoxStep2.Controls.Add(this.labelStep2);
            this.groupBoxStep2.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxStep2.Enabled = false;
            this.groupBoxStep2.Location = new System.Drawing.Point(20, 200);
            this.groupBoxStep2.Name = "groupBoxStep2";
            this.groupBoxStep2.Padding = new System.Windows.Forms.Padding(3, 50, 3, 3);
            this.groupBoxStep2.Size = new System.Drawing.Size(943, 210);
            this.groupBoxStep2.TabIndex = 1;
            this.groupBoxStep2.TabStop = false;
            // 
            // flowLayoutPanelDoctors
            // 
            this.flowLayoutPanelDoctors.AutoScroll = true;
            this.flowLayoutPanelDoctors.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanelDoctors.Location = new System.Drawing.Point(3, 63);
            this.flowLayoutPanelDoctors.Name = "flowLayoutPanelDoctors";
            this.flowLayoutPanelDoctors.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.flowLayoutPanelDoctors.Size = new System.Drawing.Size(937, 144);
            this.flowLayoutPanelDoctors.TabIndex = 1;
            // 
            // labelStep2
            // 
            this.labelStep2.AutoSize = true;
            this.labelStep2.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.labelStep2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(162)))), ((int)(((byte)(184)))));
            this.labelStep2.Location = new System.Drawing.Point(15, 15);
            this.labelStep2.Name = "labelStep2";
            this.labelStep2.Size = new System.Drawing.Size(174, 25);
            this.labelStep2.TabIndex = 0;
            this.labelStep2.Text = "Chọn bác sĩ";
            // 
            // groupBoxStep1
            // 
            this.groupBoxStep1.Controls.Add(this.flowLayoutPanelSpecialties);
            this.groupBoxStep1.Controls.Add(this.labelStep1);
            this.groupBoxStep1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxStep1.Location = new System.Drawing.Point(20, 20);
            this.groupBoxStep1.Name = "groupBoxStep1";
            this.groupBoxStep1.Padding = new System.Windows.Forms.Padding(3, 50, 3, 3);
            this.groupBoxStep1.Size = new System.Drawing.Size(943, 180);
            this.groupBoxStep1.TabIndex = 0;
            this.groupBoxStep1.TabStop = false;
            // 
            // flowLayoutPanelSpecialties
            // 
            this.flowLayoutPanelSpecialties.AutoScroll = true;
            this.flowLayoutPanelSpecialties.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanelSpecialties.Location = new System.Drawing.Point(3, 63);
            this.flowLayoutPanelSpecialties.Name = "flowLayoutPanelSpecialties";
            this.flowLayoutPanelSpecialties.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.flowLayoutPanelSpecialties.Size = new System.Drawing.Size(937, 114);
            this.flowLayoutPanelSpecialties.TabIndex = 1;
            // 
            // labelStep1
            // 
            this.labelStep1.AutoSize = true;
            this.labelStep1.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.labelStep1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(162)))), ((int)(((byte)(184)))));
            this.labelStep1.Location = new System.Drawing.Point(15, 15);
            this.labelStep1.Name = "labelStep1";
            this.labelStep1.Size = new System.Drawing.Size(231, 25);
            this.labelStep1.TabIndex = 0;
            this.labelStep1.Text = "Chọn chuyên khoa";
            // 
            // Bookings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1000, 700);
            this.Controls.Add(this.panelMain);
            this.Controls.Add(this.panelBottom);
            this.Controls.Add(this.panelTop);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Bookings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Đặt lịch hẹn";
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.panelBottom.ResumeLayout(false);
            this.panelBottom.PerformLayout();
            this.panelMain.ResumeLayout(false);
            this.groupBoxStep4.ResumeLayout(false);
            this.groupBoxStep4.PerformLayout();
            this.groupBoxStep3.ResumeLayout(false);
            this.groupBoxStep3.PerformLayout();
            this.groupBoxStep2.ResumeLayout(false);
            this.groupBoxStep2.PerformLayout();
            this.groupBoxStep1.ResumeLayout(false);
            this.groupBoxStep1.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Button buttonBookAppointment;
        private System.Windows.Forms.Label labelSummary;
        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.GroupBox groupBoxStep4;
        private System.Windows.Forms.TextBox textBoxEmail;
        private System.Windows.Forms.Label labelEmail;
        private System.Windows.Forms.TextBox textBoxPhone;
        private System.Windows.Forms.Label labelPhone;
        private System.Windows.Forms.TextBox textBoxFullName;
        private System.Windows.Forms.Label labelFullName;
        private System.Windows.Forms.Label labelStep4;
        private System.Windows.Forms.GroupBox groupBoxStep3;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelTimeSlots;
        private System.Windows.Forms.DateTimePicker dateTimePickerAppointment;
        private System.Windows.Forms.Label labelSelectTime;
        private System.Windows.Forms.Label labelStep3;
        private System.Windows.Forms.GroupBox groupBoxStep2;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelDoctors;
        private System.Windows.Forms.Label labelStep2;
        private System.Windows.Forms.GroupBox groupBoxStep1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelSpecialties;
        private System.Windows.Forms.Label labelStep1;
    }
}