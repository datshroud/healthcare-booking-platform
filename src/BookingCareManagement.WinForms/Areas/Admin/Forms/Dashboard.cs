using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection; // Cần thêm thư viện này để can thiệp sâu vào Panel
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace BookingCareManagement.WinForms.Areas.Admin.Forms
{
    public partial class Dashboard : Form
    {
        public Dashboard()
        {
            InitializeComponent();

            // Thiết lập để Form chính không bị nháy
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.ResizeRedraw, true);

            this.Load += Dashboard_Load;

            // --- QUAN TRỌNG: Gán sự kiện vẽ và xử lý lỗi resize ---
            SetupPanelDrawing(panelKhachHang, Color.FromArgb(79, 172, 254), Color.FromArgb(0, 242, 254));
            SetupPanelDrawing(panelDoanhThu, Color.FromArgb(67, 233, 123), Color.FromArgb(56, 249, 215));
            SetupPanelDrawing(panelBacSi, Color.FromArgb(255, 153, 102), Color.FromArgb(255, 94, 98));

            // Setup cho panel biểu đồ (nền trắng)
            SetupPanelBorder(panelLineChart);
            SetupPanelBorder(panelPieChart);
        }

        private void Dashboard_Load(object sender, EventArgs e)
        {
            this.BackColor = Color.FromArgb(240, 242, 245);
            SetupCharts();
            LoadDashboardData();
        }

        // ================= HÀM XỬ LÝ LỖI RESIZE & VẼ (CORE FIX) =================

        private void SetupPanelDrawing(Panel panel, Color startColor, Color endColor)
        {
            // 1. Bật DoubleBuffered cho Panel (Fix lỗi nháy)
            EnableDoubleBuffering(panel);

            // 2. Buộc vẽ lại khi thay đổi kích thước (Fix lỗi vệt bóng ma)
            panel.Resize += (s, e) => panel.Invalidate();

            // 3. Gán sự kiện Paint
            panel.Paint += (s, e) =>
            {
                // Vẽ đè lên nền cũ để xóa vết
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.Clear(this.BackColor); // Xóa bằng màu nền của Form cha

                Rectangle r = new Rectangle(0, 0, panel.Width, panel.Height);
                int radius = 20;

                using (GraphicsPath path = GetRoundedPath(r, radius))
                using (LinearGradientBrush brush = new LinearGradientBrush(r, startColor, endColor, 45F))
                {
                    e.Graphics.FillPath(brush, path);
                }
            };
        }

        private void SetupPanelBorder(Panel panel)
        {
            EnableDoubleBuffering(panel);
            panel.Resize += (s, e) => panel.Invalidate();

            panel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.Clear(this.BackColor); // Xóa vết cũ

                Rectangle r = new Rectangle(0, 0, panel.Width, panel.Height);
                int radius = 20;

                using (GraphicsPath path = GetRoundedPath(r, radius))
                using (SolidBrush brush = new SolidBrush(Color.White))
                {
                    e.Graphics.FillPath(brush, path);
                }
            };
        }

        // Hàm hack để bật DoubleBuffered cho Panel (vì thuộc tính này bị ẩn)
        public static void EnableDoubleBuffering(Control control)
        {
            PropertyInfo property = typeof(Control).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            property.SetValue(control, true, null);
        }

        // Hàm tạo hình bo tròn
        private GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            float r = radius;
            // Trừ đi 1px để tránh bị viền răng cưa khi vẽ sát mép
            path.AddArc(rect.X, rect.Y, r, r, 180, 90);
            path.AddArc(rect.Right - r - 1, rect.Y, r, r, 270, 90);
            path.AddArc(rect.Right - r - 1, rect.Bottom - r - 1, r, r, 0, 90);
            path.AddArc(rect.X, rect.Bottom - r - 1, r, r, 90, 90);
            path.CloseFigure();
            return path;
        }

        // ================= CẤU HÌNH DỮ LIỆU & BIỂU ĐỒ (GIỮ NGUYÊN) =================

        private void SetupCharts()
        {
            // --- CẤU HÌNH CHART 1: CUỘC HẸN ---
            var chartArea = chartCuocHen.ChartAreas[0];
            chartArea.BackColor = Color.White;

            // 1. TẮT LƯỚI
            chartArea.AxisX.MajorGrid.Enabled = false;
            chartArea.AxisY.MajorGrid.LineColor = Color.FromArgb(240, 240, 240);
            chartArea.AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            // 2. XỬ LÝ TRỤC X (KHẮC PHỤC LỖI MẤT CHỮ)
            // Tắt tính năng tự động tính toán nhãn (Quan trọng nhất)
            chartArea.AxisX.IsLabelAutoFit = false;

            // Ép buộc hiện bước nhảy là 1 (Hiện tất cả)
            chartArea.AxisX.Interval = 1;

            // Đảm bảo bắt đầu từ phần tử đầu tiên
            chartArea.AxisX.IntervalOffset = 0;

            // Nếu chữ bị chồng chéo, cho phép xoay nhẹ hoặc chỉnh font nhỏ lại
            chartArea.AxisX.LabelStyle.Font = new Font("Segoe UI", 9F);
            chartArea.AxisX.LabelStyle.Angle = 0; // 0 là nằm ngang, để -45 nếu chữ quá dài

            // 3. CẤU HÌNH SERIES
            chartCuocHen.Titles.Clear();
            chartCuocHen.Titles.Add(new Title("Thống kê cuộc hẹn", Docking.Top, new Font("Segoe UI", 12, FontStyle.Bold), Color.Gray));

            var s1 = chartCuocHen.Series["CuocHen"];
            s1.ChartType = SeriesChartType.Spline;
            s1.IsXValueIndexed = true;
            s1.Color = Color.FromArgb(108, 92, 231);
            s1.BorderWidth = 3;
            s1.MarkerStyle = MarkerStyle.Circle;
            s1.MarkerSize = 8;
            s1.IsVisibleInLegend = false;

            // --- CẤU HÌNH CHART 2: CHUYÊN KHOA (GIỮ NGUYÊN) ---
            chartChuyenKhoa.Titles.Clear();
            chartChuyenKhoa.Titles.Add(new Title("Tỷ lệ chuyên khoa", Docking.Top, new Font("Segoe UI", 12, FontStyle.Bold), Color.Gray));

            var s2 = chartChuyenKhoa.Series["ChuyenKhoa"];
            s2.ChartType = SeriesChartType.Doughnut;
            s2["DoughnutRadius"] = "50";
            s2.LabelForeColor = Color.White;

            chartChuyenKhoa.Legends[0].Docking = Docking.Bottom;
            chartChuyenKhoa.Legends[0].Alignment = StringAlignment.Center;
            chartCuocHen.Update();
        }
        public void LoadDashboardData()
        {
            lblKhachHangValue.Text = "1,284";
            lblDoanhThuValue.Text = "87.5M";
            lblBacSiValue.Text = "42";

            // Đẩy dữ liệu ảo
            chartCuocHen.Series["CuocHen"].Points.Clear();
            int[] data = { 45, 68, 52, 79, 91, 73, 58 };
            string[] days = { "T2", "T3", "T4", "T5", "T6", "T7", "CN" };
            for (int i = 0; i < data.Length; i++) chartCuocHen.Series["CuocHen"].Points.AddXY(days[i], data[i]);

            chartChuyenKhoa.Series["ChuyenKhoa"].Points.Clear();
            AddPiePoint("Nội khoa", 320, "#6c5ce7");
            AddPiePoint("Ngoại khoa", 210, "#00b894");
            AddPiePoint("Nhi khoa", 280, "#0984e3");
            AddPiePoint("Sản khoa", 150, "#e17055");
        }

        private void AddPiePoint(string name, double val, string hex)
        {
            int i = chartChuyenKhoa.Series["ChuyenKhoa"].Points.AddXY(name, val);
            chartChuyenKhoa.Series["ChuyenKhoa"].Points[i].Color = ColorTranslator.FromHtml(hex);
        }
    }
}