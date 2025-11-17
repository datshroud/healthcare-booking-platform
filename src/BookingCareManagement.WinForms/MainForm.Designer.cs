namespace BookingCareManagement.WinForms
{
    partial class MainForm
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
            sidebarPanel = new Panel();
            btnLich = new Button();
            btnCuocHen = new Button();
            btnBangDieuKhien = new Button();
            btnBacSi = new Button();
            btnKhachHang = new Button();
            btnChuyenKhoa = new Button();
            btnDiaDiem = new Button();
            btnTaiChinh = new Button();
            btnCaiDat = new Button();
            navbarPanel = new Panel();
            contentPanel = new Panel();
            sidebarPanel.SuspendLayout();
            SuspendLayout();
            // 
            // sidebarPanel
            // 
            sidebarPanel.AllowDrop = true;
            sidebarPanel.AutoScroll = true;
            sidebarPanel.BackColor = Color.FromArgb(15, 23, 42);
            sidebarPanel.Controls.Add(btnLich);
            sidebarPanel.Controls.Add(btnCuocHen);
            sidebarPanel.Controls.Add(btnBangDieuKhien);
            sidebarPanel.Controls.Add(btnBacSi);
            sidebarPanel.Controls.Add(btnKhachHang);
            sidebarPanel.Controls.Add(btnChuyenKhoa);
            sidebarPanel.Controls.Add(btnDiaDiem);
            sidebarPanel.Controls.Add(btnTaiChinh);
            sidebarPanel.Controls.Add(btnCaiDat);
            sidebarPanel.Dock = DockStyle.Left;
            sidebarPanel.Location = new Point(0, 0);
            sidebarPanel.Margin = new Padding(3, 4, 3, 4);
            sidebarPanel.Name = "sidebarPanel";
            sidebarPanel.Padding = new Padding(0, 12, 0, 0);
            sidebarPanel.Size = new Size(250, 1000);
            sidebarPanel.TabIndex = 0;
            // 
            // btnLich
            // 
            btnLich.BackColor = Color.Transparent;
            btnLich.Cursor = Cursors.Hand;
            btnLich.FlatAppearance.BorderSize = 0;
            btnLich.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 41, 59);
            btnLich.FlatStyle = FlatStyle.Flat;
            btnLich.Font = new Font("Segoe UI", 10F);
            btnLich.ForeColor = Color.White;
            btnLich.Location = new Point(10, 25);
            btnLich.Margin = new Padding(3, 4, 3, 4);
            btnLich.Name = "btnLich";
            btnLich.Padding = new Padding(15, 0, 0, 0);
            btnLich.Size = new Size(230, 56);
            btnLich.TabIndex = 3;
            btnLich.Text = "📅  Lịch";
            btnLich.TextAlign = ContentAlignment.MiddleLeft;
            btnLich.UseVisualStyleBackColor = false;
            btnLich.Click += btnLich_Click;
            // 
            // btnCuocHen
            // 
            btnCuocHen.BackColor = Color.Transparent;
            btnCuocHen.Cursor = Cursors.Hand;
            btnCuocHen.FlatAppearance.BorderSize = 0;
            btnCuocHen.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 41, 59);
            btnCuocHen.FlatStyle = FlatStyle.Flat;
            btnCuocHen.Font = new Font("Segoe UI", 10F);
            btnCuocHen.ForeColor = Color.White;
            btnCuocHen.Location = new Point(10, 150);
            btnCuocHen.Margin = new Padding(3, 4, 3, 4);
            btnCuocHen.Name = "btnCuocHen";
            btnCuocHen.Padding = new Padding(15, 0, 0, 0);
            btnCuocHen.Size = new Size(230, 56);
            btnCuocHen.TabIndex = 5;
            btnCuocHen.Text = "✅  Cuộc hẹn";
            btnCuocHen.TextAlign = ContentAlignment.MiddleLeft;
            btnCuocHen.UseVisualStyleBackColor = false;
            // 
            // btnBangDieuKhien
            // 
            btnBangDieuKhien.BackColor = Color.Transparent;
            btnBangDieuKhien.Cursor = Cursors.Hand;
            btnBangDieuKhien.FlatAppearance.BorderSize = 0;
            btnBangDieuKhien.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 41, 59);
            btnBangDieuKhien.FlatStyle = FlatStyle.Flat;
            btnBangDieuKhien.Font = new Font("Segoe UI", 10F);
            btnBangDieuKhien.ForeColor = Color.White;
            btnBangDieuKhien.Location = new Point(10, 88);
            btnBangDieuKhien.Margin = new Padding(3, 4, 3, 4);
            btnBangDieuKhien.Name = "btnBangDieuKhien";
            btnBangDieuKhien.Padding = new Padding(15, 0, 0, 0);
            btnBangDieuKhien.Size = new Size(230, 56);
            btnBangDieuKhien.TabIndex = 4;
            btnBangDieuKhien.Text = "📊  Bảng điều khiển";
            btnBangDieuKhien.TextAlign = ContentAlignment.MiddleLeft;
            btnBangDieuKhien.UseVisualStyleBackColor = false;
            btnBangDieuKhien.Click += btnBangDieuKhien_Click;
            // 
            // btnBacSi
            // 
            btnBacSi.BackColor = Color.Transparent;
            btnBacSi.Cursor = Cursors.Hand;
            btnBacSi.FlatAppearance.BorderSize = 0;
            btnBacSi.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 41, 59);
            btnBacSi.FlatStyle = FlatStyle.Flat;
            btnBacSi.Font = new Font("Segoe UI", 10F);
            btnBacSi.ForeColor = Color.White;
            btnBacSi.Location = new Point(10, 212);
            btnBacSi.Margin = new Padding(3, 4, 3, 4);
            btnBacSi.Name = "btnBacSi";
            btnBacSi.Padding = new Padding(15, 0, 0, 0);
            btnBacSi.Size = new Size(230, 56);
            btnBacSi.TabIndex = 6;
            btnBacSi.Text = "👥  Bác sĩ";
            btnBacSi.TextAlign = ContentAlignment.MiddleLeft;
            btnBacSi.UseVisualStyleBackColor = false;
            // 
            // btnKhachHang
            // 
            btnKhachHang.BackColor = Color.Transparent;
            btnKhachHang.Cursor = Cursors.Hand;
            btnKhachHang.FlatAppearance.BorderSize = 0;
            btnKhachHang.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 41, 59);
            btnKhachHang.FlatStyle = FlatStyle.Flat;
            btnKhachHang.Font = new Font("Segoe UI", 10F);
            btnKhachHang.ForeColor = Color.White;
            btnKhachHang.Location = new Point(10, 275);
            btnKhachHang.Margin = new Padding(3, 4, 3, 4);
            btnKhachHang.Name = "btnKhachHang";
            btnKhachHang.Padding = new Padding(15, 0, 0, 0);
            btnKhachHang.Size = new Size(230, 56);
            btnKhachHang.TabIndex = 7;
            btnKhachHang.Text = "👤  Khách hàng";
            btnKhachHang.TextAlign = ContentAlignment.MiddleLeft;
            btnKhachHang.UseVisualStyleBackColor = false;
            btnKhachHang.Click += btnKhachHang_Click;
            // 
            // btnChuyenKhoa
            // 
            btnChuyenKhoa.BackColor = Color.Transparent;
            btnChuyenKhoa.Cursor = Cursors.Hand;
            btnChuyenKhoa.FlatAppearance.BorderSize = 0;
            btnChuyenKhoa.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 41, 59);
            btnChuyenKhoa.FlatStyle = FlatStyle.Flat;
            btnChuyenKhoa.Font = new Font("Segoe UI", 10F);
            btnChuyenKhoa.ForeColor = Color.White;
            btnChuyenKhoa.Location = new Point(10, 338);
            btnChuyenKhoa.Margin = new Padding(3, 4, 3, 4);
            btnChuyenKhoa.Name = "btnChuyenKhoa";
            btnChuyenKhoa.Padding = new Padding(15, 0, 0, 0);
            btnChuyenKhoa.Size = new Size(230, 56);
            btnChuyenKhoa.TabIndex = 8;
            btnChuyenKhoa.Text = "🎯  Chuyên khoa";
            btnChuyenKhoa.TextAlign = ContentAlignment.MiddleLeft;
            btnChuyenKhoa.UseVisualStyleBackColor = false;
            // 
            // btnDiaDiem
            // 
            btnDiaDiem.BackColor = Color.Transparent;
            btnDiaDiem.Cursor = Cursors.Hand;
            btnDiaDiem.FlatAppearance.BorderSize = 0;
            btnDiaDiem.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 41, 59);
            btnDiaDiem.FlatStyle = FlatStyle.Flat;
            btnDiaDiem.Font = new Font("Segoe UI", 10F);
            btnDiaDiem.ForeColor = Color.White;
            btnDiaDiem.Location = new Point(10, 400);
            btnDiaDiem.Margin = new Padding(3, 4, 3, 4);
            btnDiaDiem.Name = "btnDiaDiem";
            btnDiaDiem.Padding = new Padding(15, 0, 0, 0);
            btnDiaDiem.Size = new Size(230, 56);
            btnDiaDiem.TabIndex = 9;
            btnDiaDiem.Text = "📍  Địa điểm";
            btnDiaDiem.TextAlign = ContentAlignment.MiddleLeft;
            btnDiaDiem.UseVisualStyleBackColor = false;
            // 
            // btnTaiChinh
            // 
            btnTaiChinh.BackColor = Color.Transparent;
            btnTaiChinh.Cursor = Cursors.Hand;
            btnTaiChinh.FlatAppearance.BorderSize = 0;
            btnTaiChinh.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 41, 59);
            btnTaiChinh.FlatStyle = FlatStyle.Flat;
            btnTaiChinh.Font = new Font("Segoe UI", 10F);
            btnTaiChinh.ForeColor = Color.White;
            btnTaiChinh.Location = new Point(10, 462);
            btnTaiChinh.Margin = new Padding(3, 4, 3, 4);
            btnTaiChinh.Name = "btnTaiChinh";
            btnTaiChinh.Padding = new Padding(15, 0, 0, 0);
            btnTaiChinh.Size = new Size(230, 56);
            btnTaiChinh.TabIndex = 10;
            btnTaiChinh.Text = "💰  Hóa đơn";
            btnTaiChinh.TextAlign = ContentAlignment.MiddleLeft;
            btnTaiChinh.UseVisualStyleBackColor = false;
            btnTaiChinh.Click += btnTaiChinh_Click;
            // 
            // btnCaiDat
            // 
            btnCaiDat.BackColor = Color.Transparent;
            btnCaiDat.Cursor = Cursors.Hand;
            btnCaiDat.FlatAppearance.BorderSize = 0;
            btnCaiDat.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 41, 59);
            btnCaiDat.FlatStyle = FlatStyle.Flat;
            btnCaiDat.Font = new Font("Segoe UI", 10F);
            btnCaiDat.ForeColor = Color.White;
            btnCaiDat.Location = new Point(10, 525);
            btnCaiDat.Margin = new Padding(3, 4, 3, 4);
            btnCaiDat.Name = "btnCaiDat";
            btnCaiDat.Padding = new Padding(15, 0, 0, 0);
            btnCaiDat.Size = new Size(230, 56);
            btnCaiDat.TabIndex = 11;
            btnCaiDat.Text = "⚙️  Cài đặt";
            btnCaiDat.TextAlign = ContentAlignment.MiddleLeft;
            btnCaiDat.UseVisualStyleBackColor = false;
            // 
            // navbarPanel
            // 
            navbarPanel.BackColor = Color.White;
            navbarPanel.Dock = DockStyle.Top;
            navbarPanel.Location = new Point(250, 0);
            navbarPanel.Margin = new Padding(3, 4, 3, 4);
            navbarPanel.Name = "navbarPanel";
            navbarPanel.Size = new Size(1150, 75);
            navbarPanel.TabIndex = 1;
            // 
            // contentPanel
            // 
            contentPanel.AutoScroll = true;
            contentPanel.BackColor = Color.FromArgb(240, 242, 245);
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.Location = new Point(250, 75);
            contentPanel.Margin = new Padding(3, 4, 3, 4);
            contentPanel.Name = "contentPanel";
            contentPanel.Padding = new Padding(20, 25, 20, 25);
            contentPanel.Size = new Size(1150, 925);
            contentPanel.TabIndex = 2;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(240, 242, 245);
            ClientSize = new Size(1400, 1000);
            Controls.Add(contentPanel);
            Controls.Add(navbarPanel);
            Controls.Add(sidebarPanel);
            Margin = new Padding(3, 4, 3, 4);
            MinimumSize = new Size(1000, 738);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Booking Website";
            WindowState = FormWindowState.Maximized;
            sidebarPanel.ResumeLayout(false);
            ResumeLayout(false);
        }
        private System.Windows.Forms.Button btnLich;
        private System.Windows.Forms.Button btnBangDieuKhien;
        private System.Windows.Forms.Button btnCuocHen;
        private System.Windows.Forms.Button btnBacSi;
        private System.Windows.Forms.Button btnKhachHang;
        private System.Windows.Forms.Button btnChuyenKhoa;
        private System.Windows.Forms.Button btnDiaDiem;
        private System.Windows.Forms.Button btnTaiChinh;
        private System.Windows.Forms.Button btnCaiDat;


        #endregion

        private CircularPictureBox circularPictureBox1;
        private RoundedButton roundedButton1;
    }
}