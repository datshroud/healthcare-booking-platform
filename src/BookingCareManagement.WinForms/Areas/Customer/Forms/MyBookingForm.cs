using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BookingCareManagement.WinForms.Areas.Customer.Forms
{
    public partial class MyBookingForm : Form
    {
        public MyBookingForm()
        {
            InitializeComponent();
            LoadDummyData();

            // CÀI ĐẶT VỊ TRÍ NÚT TỰ ĐỘNG
            this.Load += MyBookingForm_Load;
            this.panelActions.Resize += PanelActions_Resize;

            // Cài đặt vẽ nút bo tròn
            btnCancel.Paint += (s, e) => DrawRoundedButton(s as Button, e.Graphics, 10, Color.FromArgb(220, 53, 69));
            btnCancel.Click += BtnCancel_Click;

            // Thêm sự kiện định dạng màu sắc cho cột Trạng thái
            dgvBookings.CellFormatting += DgvBookings_CellFormatting;
        }

        // Sự kiện tô màu chữ Trạng thái
        private void DgvBookings_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Nếu đang vẽ ở cột TRẠNG THÁI (cột thứ 3 - index tính từ 0)
            if (dgvBookings.Columns[e.ColumnIndex].Name == "colStatus" && e.Value != null)
            {
                string status = e.Value.ToString();
                if (status == "Đã Duyệt")
                {
                    e.CellStyle.ForeColor = Color.SeaGreen; // Màu xanh lá
                    e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                }
                else if (status == "Chờ Duyệt")
                {
                    e.CellStyle.ForeColor = Color.DarkOrange; // Màu cam
                    e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Italic);
                }
            }
        }

        private void UpdateButtonPosition()
        {
            // Đặt nút cách lề phải 30px
            btnCancel.Location = new Point(panelActions.Width - btnCancel.Width - 30, 10);
        }

        private void MyBookingForm_Load(object sender, EventArgs e)
        {
            UpdateButtonPosition();
        }

        private void PanelActions_Resize(object sender, EventArgs e)
        {
            UpdateButtonPosition();
        }

        private void LoadDummyData()
        {
            // Xóa dữ liệu cũ (nếu có)
            dgvBookings.Rows.Clear();

            // THÊM DỮ LIỆU MỚI (ĐÚNG THỨ TỰ CỘT TRONG DESIGNER)
            // Thứ tự cột: Ngày | Giờ | Bác sĩ | Trạng thái | Thanh toán

            dgvBookings.Rows.Add("Thứ Hai, 20/11/2023", "09:00 - 10:00", "Nha Khoa - BS. Nguyễn Văn A", "Đã Duyệt", 200000);
            dgvBookings.Rows.Add("Thứ Tư, 22/11/2023", "14:30 - 16:30", "Phẫu thuật - BS. Le Wilson", "Chờ Duyệt", 1000000);
            dgvBookings.Rows.Add("Chủ Nhật, 26/11/2023", "08:00 - 09:00", "Tư vấn Niềng răng - BS. Trần B", "Đã Duyệt", 500000);
            dgvBookings.Rows.Add("Thứ Ba, 28/11/2023", "10:00 - 11:00", "Nhổ răng khôn - BS. Sarah", "Đã Duyệt", 400000);

            if (dgvBookings.Rows.Count > 0) dgvBookings.Rows[0].Selected = true;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            if (dgvBookings.CurrentRow != null)
            {
                string date = dgvBookings.CurrentRow.Cells["colDate"].Value.ToString();
                DialogResult result = MessageBox.Show($"Bạn muốn hủy lịch ngày {date}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    dgvBookings.Rows.Remove(dgvBookings.CurrentRow);
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn lịch cần hủy!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DrawRoundedButton(Button btn, Graphics g, int radius, Color backColor)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle r = new Rectangle(0, 0, btn.Width, btn.Height);
            g.Clear(this.panelActions.BackColor);

            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(r.X, r.Y, radius, radius, 180, 90);
                path.AddArc(r.Right - radius, r.Y, radius, radius, 270, 90);
                path.AddArc(r.Right - radius, r.Bottom - radius, radius, radius, 0, 90);
                path.AddArc(r.X, r.Bottom - radius, radius, radius, 90, 90);
                path.CloseFigure();

                using (SolidBrush brush = new SolidBrush(backColor))
                {
                    g.FillPath(brush, path);
                }
                TextRenderer.DrawText(g, btn.Text, btn.Font, r, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }
    }
}