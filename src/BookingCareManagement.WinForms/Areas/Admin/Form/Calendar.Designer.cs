namespace BookingCareManagement.WinForms
{
    partial class Calendar
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
            newAppointmentBtn = new Button();
            headerTitleLabel = new Label();
            userPanel = new Panel();
            navigationPanel = new Panel();
            btnToday = new Button();
            comboBox1 = new ComboBox();
            filtersBtn = new Button();
            dayBtn = new Button();
            weekBtn = new Button();
            monthBtn = new Button();
            nextBtn = new Button();
            monthLabel = new Label();
            prevBtn = new Button();
            calendarPanel = new Panel();
            contentPanel = new Panel();
            headerPanel.SuspendLayout();
            navigationPanel.SuspendLayout();
            SuspendLayout();
            // 
            // headerPanel
            // 
            headerPanel.BackColor = Color.FromArgb(243, 244, 246);
            headerPanel.Controls.Add(newAppointmentBtn);
            headerPanel.Controls.Add(headerTitleLabel);
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Location = new Point(0, 0);
            headerPanel.Margin = new Padding(3, 4, 3, 4);
            headerPanel.Name = "headerPanel";
            headerPanel.Padding = new Padding(30, 25, 30, 0);
            headerPanel.Size = new Size(1600, 100);
            headerPanel.TabIndex = 0;
            // 
            // newAppointmentBtn
            // 
            newAppointmentBtn.BackColor = Color.FromArgb(37, 99, 235);
            newAppointmentBtn.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            newAppointmentBtn.ForeColor = Color.White;
            newAppointmentBtn.Location = new Point(1317, 28);
            newAppointmentBtn.Name = "newAppointmentBtn";
            newAppointmentBtn.Size = new Size(250, 50);
            newAppointmentBtn.TabIndex = 1;
            newAppointmentBtn.Text = "+  Thêm cuôc hẹn";
            newAppointmentBtn.UseVisualStyleBackColor = false;
            newAppointmentBtn.Click += newAppointmentBtn_Click;
            // 
            // headerTitleLabel
            // 
            headerTitleLabel.AutoSize = true;
            headerTitleLabel.Font = new Font("Segoe UI", 24F, FontStyle.Bold, GraphicsUnit.Point, 0);
            headerTitleLabel.ForeColor = Color.FromArgb(17, 24, 39);
            headerTitleLabel.Location = new Point(30, 25);
            headerTitleLabel.Name = "headerTitleLabel";
            headerTitleLabel.Size = new Size(319, 54);
            headerTitleLabel.TabIndex = 0;
            headerTitleLabel.Text = "Lịch khám bệnh";
            // 
            // userPanel
            // 
            userPanel.BackColor = Color.White;
            userPanel.Dock = DockStyle.Top;
            userPanel.Location = new Point(0, 100);
            userPanel.Margin = new Padding(3, 4, 3, 4);
            userPanel.Name = "userPanel";
            userPanel.Padding = new Padding(30, 25, 30, 25);
            userPanel.Size = new Size(1600, 125);
            userPanel.TabIndex = 1;
            // 
            // navigationPanel
            // 
            navigationPanel.BackColor = Color.White;
            navigationPanel.Controls.Add(btnToday);
            navigationPanel.Controls.Add(comboBox1);
            navigationPanel.Controls.Add(filtersBtn);
            navigationPanel.Controls.Add(dayBtn);
            navigationPanel.Controls.Add(weekBtn);
            navigationPanel.Controls.Add(monthBtn);
            navigationPanel.Controls.Add(nextBtn);
            navigationPanel.Controls.Add(monthLabel);
            navigationPanel.Controls.Add(prevBtn);
            navigationPanel.Dock = DockStyle.Top;
            navigationPanel.Location = new Point(0, 225);
            navigationPanel.Margin = new Padding(3, 4, 3, 4);
            navigationPanel.Name = "navigationPanel";
            navigationPanel.Padding = new Padding(30, 19, 30, 19);
            navigationPanel.Size = new Size(1600, 88);
            navigationPanel.TabIndex = 2;
            // 
            // btnToday
            // 
            btnToday.Font = new Font("Segoe UI", 10F);
            btnToday.ForeColor = Color.Black;
            btnToday.Location = new Point(12, 26);
            btnToday.Name = "btnToday";
            btnToday.Size = new Size(100, 40);
            btnToday.TabIndex = 9;
            btnToday.Text = "Hôm nay";
            btnToday.UseVisualStyleBackColor = true;
            btnToday.Click += btnToday_Click;
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(1034, 35);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(80, 28);
            comboBox1.TabIndex = 8;
            // 
            // filtersBtn
            // 
            filtersBtn.Font = new Font("Segoe UI", 10F);
            filtersBtn.ForeColor = Color.Black;
            filtersBtn.Location = new Point(1400, 26);
            filtersBtn.Name = "filtersBtn";
            filtersBtn.Size = new Size(100, 40);
            filtersBtn.TabIndex = 7;
            filtersBtn.Text = "⚙ Bộ lọc";
            filtersBtn.UseVisualStyleBackColor = true;
            // 
            // dayBtn
            // 
            dayBtn.Font = new Font("Segoe UI", 10F);
            dayBtn.ForeColor = Color.Black;
            dayBtn.Location = new Point(1310, 26);
            dayBtn.Name = "dayBtn";
            dayBtn.Size = new Size(80, 40);
            dayBtn.TabIndex = 6;
            dayBtn.Text = "Ngày";
            dayBtn.UseVisualStyleBackColor = true;
            dayBtn.Click += dayBtn_Click;
            // 
            // weekBtn
            // 
            weekBtn.Font = new Font("Segoe UI", 10F);
            weekBtn.ForeColor = Color.Black;
            weekBtn.Location = new Point(1220, 26);
            weekBtn.Name = "weekBtn";
            weekBtn.Size = new Size(80, 40);
            weekBtn.TabIndex = 5;
            weekBtn.Text = "Tuần";
            weekBtn.UseVisualStyleBackColor = true;
            weekBtn.Click += weekBtn_Click;
            // 
            // monthBtn
            // 
            monthBtn.Font = new Font("Segoe UI", 10F);
            monthBtn.ForeColor = Color.Black;
            monthBtn.Location = new Point(1130, 26);
            monthBtn.Name = "monthBtn";
            monthBtn.Size = new Size(80, 40);
            monthBtn.TabIndex = 4;
            monthBtn.Text = "Tháng";
            monthBtn.UseVisualStyleBackColor = true;
            monthBtn.Click += monthBtn_Click;
            // 
            // nextBtn
            // 
            nextBtn.FlatAppearance.BorderSize = 0;
            nextBtn.FlatStyle = FlatStyle.Flat;
            nextBtn.Font = new Font("Segoe UI", 12F);
            nextBtn.Location = new Point(520, 19);
            nextBtn.Margin = new Padding(3, 4, 3, 4);
            nextBtn.Name = "nextBtn";
            nextBtn.Size = new Size(40, 50);
            nextBtn.TabIndex = 3;
            nextBtn.Text = "▶";
            nextBtn.UseVisualStyleBackColor = true;
            // 
            // monthLabel
            // 
            monthLabel.AutoSize = true;
            monthLabel.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            monthLabel.ForeColor = Color.FromArgb(17, 24, 39);
            monthLabel.Location = new Point(177, 28);
            monthLabel.Name = "monthLabel";
            monthLabel.Size = new Size(340, 32);
            monthLabel.TabIndex = 2;
            monthLabel.Text = "Monday November 17, 2025";
            monthLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // prevBtn
            // 
            prevBtn.FlatAppearance.BorderSize = 0;
            prevBtn.FlatStyle = FlatStyle.Flat;
            prevBtn.Font = new Font("Segoe UI", 12F);
            prevBtn.Location = new Point(120, 19);
            prevBtn.Margin = new Padding(3, 4, 3, 4);
            prevBtn.Name = "prevBtn";
            prevBtn.Size = new Size(40, 50);
            prevBtn.TabIndex = 1;
            prevBtn.Text = "◀";
            prevBtn.UseVisualStyleBackColor = true;
            prevBtn.Click += prevBtn_Click;
            // 
            // calendarPanel
            // 
            calendarPanel.AutoScroll = true;
            calendarPanel.BackColor = Color.White;
            calendarPanel.Dock = DockStyle.Fill;
            calendarPanel.Location = new Point(0, 313);
            calendarPanel.Margin = new Padding(3, 4, 3, 4);
            calendarPanel.Name = "calendarPanel";
            calendarPanel.Padding = new Padding(15, 19, 15, 19);
            calendarPanel.Size = new Size(1600, 742);
            calendarPanel.TabIndex = 3;
            // 
            // contentPanel
            // 
            contentPanel.BackColor = Color.White;
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.Location = new Point(0, 313);
            contentPanel.Margin = new Padding(3, 4, 3, 4);
            contentPanel.Name = "contentPanel";
            contentPanel.Size = new Size(1600, 742);
            contentPanel.TabIndex = 4;
            contentPanel.Visible = false;
            // 
            // Calendar
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(243, 244, 246);
            ClientSize = new Size(1600, 1055);
            Controls.Add(calendarPanel);
            Controls.Add(contentPanel);
            Controls.Add(navigationPanel);
            Controls.Add(userPanel);
            Controls.Add(headerPanel);
            Margin = new Padding(3, 4, 3, 4);
            Name = "Calendar";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Calendar";
            headerPanel.ResumeLayout(false);
            headerPanel.PerformLayout();
            navigationPanel.ResumeLayout(false);
            navigationPanel.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel headerPanel;
        private System.Windows.Forms.Label headerTitleLabel;
        private System.Windows.Forms.Panel userPanel;
        private System.Windows.Forms.Panel navigationPanel;
        private System.Windows.Forms.Button nextBtn;
        private System.Windows.Forms.Label monthLabel;
        private System.Windows.Forms.Button prevBtn;
        private RoundedButton1 todayBtn;
        private System.Windows.Forms.Panel calendarPanel;
        private System.Windows.Forms.Panel contentPanel;
        private Button newAppointmentBtn;
        private Button monthBtn;
        private Button filtersBtn;
        private Button dayBtn;
        private Button weekBtn;
        private ComboBox comboBox1;
        private Button btnToday;
    }
}