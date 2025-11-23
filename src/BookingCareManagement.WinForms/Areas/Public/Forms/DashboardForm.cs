using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BookingCareManagement.WinForms.Areas.Admin.Services;
using System.Data;
using System.Windows.Forms.DataVisualization.Charting;
namespace BookingCareManagement.WinForms;

public sealed class DashboardForm : Form
{
    public DashboardForm()
    {
        // Initialize designer controls
        InitializeComponent();

        // Populate runtime-only data (guarded to avoid Designer errors)
        if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
        {
            PopulateTimeRangeComboBoxes();
            PopulateDataGrids();
            PopulateChartsWithMockData();
        }

        Text = "Dashboard";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(800, 600);
        Font = new Font("Segoe UI", 10);
        BackColor = Color.FromArgb(243, 244, 246);
    }

    // Preserve the API surface if other code constructs this form with the ApiClient
    public DashboardForm(AdminDashboardApiClient apiClient) : this()
    {
        // intentionally empty - placeholder for compatibility
    }

    // Populate combo boxes at runtime only
    private void PopulateTimeRangeComboBoxes()
    {
        var timeRanges = new[] { "Tuần này", "Tuần trước", "Tuần sau", "Tháng này", "Tháng trước", "Tháng sau" };

        cobXuHuong.Items.AddRange(timeRanges);
        if (cobXuHuong.Items.Count > 0) cobXuHuong.SelectedIndex = 0;

        cobDoanhThu.Items.AddRange(timeRanges);
        if (cobDoanhThu.Items.Count > 0) cobDoanhThu.SelectedIndex = 0;

        cobKhachHang.Items.AddRange(timeRanges);
        if (cobKhachHang.Items.Count > 0) cobKhachHang.SelectedIndex = 0;

        cobLichHen.Items.AddRange(timeRanges);
        if (cobLichHen.Items.Count > 0) cobLichHen.SelectedIndex = 0;

        cobCongXuat.Items.AddRange(timeRanges);
        if (cobCongXuat.Items.Count > 0) cobCongXuat.SelectedIndex = 0;

        cobCuocHen.Items.AddRange(timeRanges);
        if (cobCuocHen.Items.Count > 0) cobCuocHen.SelectedIndex = 0;

        cobHieuXuat.Items.AddRange(timeRanges);
        if (cobHieuXuat.Items.Count > 0) cobHieuXuat.SelectedIndex = 0;

        // Populate status combobox for appointments
        var statuses = new[] { "Tất cả các trạng thái", "Đã xác nhận", "Chờ xác nhận", "Đã hủy", "Đã từ chối", "Vắng mặt" };
        cobTrangThai.Items.AddRange(statuses);
        if (cobTrangThai.Items.Count > 0) cobTrangThai.SelectedIndex = 0;
    }

    private Panel HeaderPanel;
    private Label label2;
    private FlowLayoutPanel flowLayoutPanel1;
    private Panel panelDoanhThu;
    private Label lbDoanhThu;
    private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
    private ComboBox cobDoanhThu;
    private Label label3;
    private Panel panelKhachHang;
    private System.Windows.Forms.DataVisualization.Charting.Chart chartKhachHang;
    private Label lbKhachHang;
    private ComboBox cobKhachHang;
    private Label label1;
    private Panel panelLichHen;
    private Label lbLichHen;
    private System.Windows.Forms.DataVisualization.Charting.Chart chart2;
    private ComboBox cobLichHen;
    private Label label4;
    private Panel panelXuHuong;
    private ComboBox cobXuHuong;
    private Label label5;
    private Panel panelCongXuat;
    private ComboBox cobCongXuat;
    private Label label6;
    private Panel panel1;
    private ComboBox cobCuocHen;
    private ComboBox cobTrangThai;
    private Label label7;
    private Panel panelHieuXuat;
    private ComboBox cobHieuXuat;
    private Label label8;
    private TabControl tabControlHieuXuat;
    private TabPage tabPageChuyenKhoa;
    private TabPage tabPageBacSi;
    private DataGridView dgvCuocHen;
    private DataGridView dgvChuyenKhoa;
    private DataGridView dgvBacSi;
    private Label lbTitle;

    private void InitializeComponent()
    {
        ChartArea chartArea1 = new ChartArea();
        Legend legend1 = new Legend();
        Series series1 = new Series();
        ChartArea chartArea2 = new ChartArea();
        Legend legend2 = new Legend();
        Series series2 = new Series();
        ChartArea chartArea3 = new ChartArea();
        Legend legend3 = new Legend();
        Series series3 = new Series();
        HeaderPanel = new Panel();
        label2 = new Label();
        lbTitle = new Label();
        flowLayoutPanel1 = new FlowLayoutPanel();
        panelKhachHang = new Panel();
        chartKhachHang = new Chart();
        lbKhachHang = new Label();
        cobKhachHang = new ComboBox();
        label1 = new Label();
        panelDoanhThu = new Panel();
        lbDoanhThu = new Label();
        chart1 = new Chart();
        cobDoanhThu = new ComboBox();
        label3 = new Label();
        panelLichHen = new Panel();
        lbLichHen = new Label();
        chart2 = new Chart();
        cobLichHen = new ComboBox();
        label4 = new Label();
        panelXuHuong = new Panel();
        cobXuHuong = new ComboBox();
        label5 = new Label();
        panelCongXuat = new Panel();
        cobCongXuat = new ComboBox();
        label6 = new Label();
        panel1 = new Panel();
        dgvCuocHen = new DataGridView();
        cobCuocHen = new ComboBox();
        cobTrangThai = new ComboBox();
        label7 = new Label();
        panelHieuXuat = new Panel();
        cobHieuXuat = new ComboBox();
        label8 = new Label();
        tabControlHieuXuat = new TabControl();
        tabPageChuyenKhoa = new TabPage();
        dgvChuyenKhoa = new DataGridView();
        tabPageBacSi = new TabPage();
        dgvBacSi = new DataGridView();
        HeaderPanel.SuspendLayout();
        flowLayoutPanel1.SuspendLayout();
        panelKhachHang.SuspendLayout();
        ((ISupportInitialize)chartKhachHang).BeginInit();
        panelDoanhThu.SuspendLayout();
        ((ISupportInitialize)chart1).BeginInit();
        panelLichHen.SuspendLayout();
        ((ISupportInitialize)chart2).BeginInit();
        panelXuHuong.SuspendLayout();
        panelCongXuat.SuspendLayout();
        panel1.SuspendLayout();
        ((ISupportInitialize)dgvCuocHen).BeginInit();
        panelHieuXuat.SuspendLayout();
        tabControlHieuXuat.SuspendLayout();
        tabPageChuyenKhoa.SuspendLayout();
        ((ISupportInitialize)dgvChuyenKhoa).BeginInit();
        tabPageBacSi.SuspendLayout();
        ((ISupportInitialize)dgvBacSi).BeginInit();
        SuspendLayout();
        // 
        // HeaderPanel
        // 
        HeaderPanel.Controls.Add(label2);
        HeaderPanel.Controls.Add(lbTitle);
        HeaderPanel.Dock = DockStyle.Top;
        HeaderPanel.Location = new Point(0, 0);
        HeaderPanel.Name = "HeaderPanel";
        HeaderPanel.Size = new Size(1614, 125);
        HeaderPanel.TabIndex = 0;
        // 
        // label2
        // 
        label2.AutoSize = true;
        label2.Font = new Font("Segoe UI", 12F);
        label2.Location = new Point(80, 78);
        label2.Name = "label2";
        label2.Size = new Size(371, 28);
        label2.TabIndex = 1;
        label2.Text = "Chào mừng bạn đến với bảng điều khiển.";
        // 
        // lbTitle
        // 
        lbTitle.AutoSize = true;
        lbTitle.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
        lbTitle.Location = new Point(80, 20);
        lbTitle.Name = "lbTitle";
        lbTitle.Size = new Size(166, 46);
        lbTitle.TabIndex = 0;
        lbTitle.Text = "Xin chào,";
        // 
        // flowLayoutPanel1
        // 
        flowLayoutPanel1.Controls.Add(panelKhachHang);
        flowLayoutPanel1.Controls.Add(panelDoanhThu);
        flowLayoutPanel1.Controls.Add(panelLichHen);
        flowLayoutPanel1.Controls.Add(panelXuHuong);
        flowLayoutPanel1.Controls.Add(panelCongXuat);
        flowLayoutPanel1.Controls.Add(panel1);
        flowLayoutPanel1.Controls.Add(panelHieuXuat);
        flowLayoutPanel1.Location = new Point(12, 164);
        flowLayoutPanel1.Name = "flowLayoutPanel1";
        flowLayoutPanel1.Size = new Size(1590, 743);
        flowLayoutPanel1.TabIndex = 1;
        // 
        // panelKhachHang
        // 
        panelKhachHang.AutoSize = true;
        panelKhachHang.BorderStyle = BorderStyle.FixedSingle;
        panelKhachHang.Controls.Add(chartKhachHang);
        panelKhachHang.Controls.Add(lbKhachHang);
        panelKhachHang.Controls.Add(cobKhachHang);
        panelKhachHang.Controls.Add(label1);
        panelKhachHang.Location = new Point(80, 3);
        panelKhachHang.Margin = new Padding(80, 3, 20, 20);
        panelKhachHang.Name = "panelKhachHang";
        panelKhachHang.Size = new Size(460, 250);
        panelKhachHang.TabIndex = 8;
        // 
        // chartKhachHang
        // 
        chartArea1.AxisX.LabelStyle.Enabled = false;
        chartArea1.AxisX.LineColor = Color.Transparent;
        chartArea1.AxisX.LineWidth = 0;
        chartArea1.AxisX.MajorGrid.Enabled = false;
        chartArea1.AxisX.MajorTickMark.Enabled = false;
        chartArea1.AxisY.LabelStyle.Enabled = false;
        chartArea1.AxisY.LabelStyle.ForeColor = Color.Transparent;
        chartArea1.AxisY.LineWidth = 0;
        chartArea1.AxisY.MajorGrid.Enabled = false;
        chartArea1.AxisY.MajorTickMark.Enabled = false;
        chartArea1.Name = "ChartArea1";
        chartKhachHang.ChartAreas.Add(chartArea1);
        legend1.Enabled = false;
        legend1.Name = "Legend1";
        chartKhachHang.Legends.Add(legend1);
        chartKhachHang.Location = new Point(3, 125);
        chartKhachHang.Name = "chartKhachHang";
        series1.BorderWidth = 3;
        series1.ChartArea = "ChartArea1";
        series1.ChartType = SeriesChartType.Spline;
        series1.Color = Color.Lime;
        series1.Legend = "Legend1";
        series1.Name = "Series1";
        chartKhachHang.Series.Add(series1);
        chartKhachHang.Size = new Size(452, 120);
        chartKhachHang.TabIndex = 3;
        chartKhachHang.Text = "chart1";
        // 
        // lbKhachHang
        // 
        lbKhachHang.AutoSize = true;
        lbKhachHang.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
        lbKhachHang.Location = new Point(35, 83);
        lbKhachHang.Name = "lbKhachHang";
        lbKhachHang.Size = new Size(43, 35);
        lbKhachHang.TabIndex = 2;
        lbKhachHang.Text = "10";
        // 
        // cobKhachHang
        // 
        cobKhachHang.DropDownStyle = ComboBoxStyle.DropDownList;
        cobKhachHang.FormattingEnabled = true;
        cobKhachHang.Location = new Point(323, 36);
        cobKhachHang.Name = "cobKhachHang";
        cobKhachHang.Size = new Size(132, 28);
        cobKhachHang.TabIndex = 1;
        // 
        // label1
        // 
        label1.AutoSize = true;
        label1.Font = new Font("Segoe UI", 12F);
        label1.Location = new Point(35, 36);
        label1.Name = "label1";
        label1.Size = new Size(153, 28);
        label1.TabIndex = 0;
        label1.Text = "Khách hàng mới";
        // 
        // panelDoanhThu
        // 
        panelDoanhThu.AutoSize = true;
        panelDoanhThu.BorderStyle = BorderStyle.FixedSingle;
        panelDoanhThu.Controls.Add(lbDoanhThu);
        panelDoanhThu.Controls.Add(chart1);
        panelDoanhThu.Controls.Add(cobDoanhThu);
        panelDoanhThu.Controls.Add(label3);
        panelDoanhThu.Location = new Point(580, 3);
        panelDoanhThu.Margin = new Padding(20, 3, 20, 20);
        panelDoanhThu.Name = "panelDoanhThu";
        panelDoanhThu.Size = new Size(460, 250);
        panelDoanhThu.TabIndex = 9;
        // 
        // lbDoanhThu
        // 
        lbDoanhThu.AutoSize = true;
        lbDoanhThu.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
        lbDoanhThu.Location = new Point(35, 79);
        lbDoanhThu.Name = "lbDoanhThu";
        lbDoanhThu.Size = new Size(129, 35);
        lbDoanhThu.TabIndex = 4;
        lbDoanhThu.Text = "610.000 ₫";
        // 
        // chart1
        // 
        chartArea2.AxisX.LabelStyle.Enabled = false;
        chartArea2.AxisX.LineColor = Color.Transparent;
        chartArea2.AxisX.LineWidth = 0;
        chartArea2.AxisX.MajorGrid.Enabled = false;
        chartArea2.AxisX.MajorTickMark.Enabled = false;
        chartArea2.AxisY.LabelStyle.Enabled = false;
        chartArea2.AxisY.LabelStyle.ForeColor = Color.Transparent;
        chartArea2.AxisY.LineWidth = 0;
        chartArea2.AxisY.MajorGrid.Enabled = false;
        chartArea2.AxisY.MajorTickMark.Enabled = false;
        chartArea2.Name = "ChartArea1";
        chart1.ChartAreas.Add(chartArea2);
        legend2.Enabled = false;
        legend2.Name = "Legend1";
        chart1.Legends.Add(legend2);
        chart1.Location = new Point(-1, 117);
        chart1.Name = "chart1";
        series2.BorderWidth = 3;
        series2.ChartArea = "ChartArea1";
        series2.ChartType = SeriesChartType.Spline;
        series2.Color = Color.Gray;
        series2.Legend = "Legend1";
        series2.Name = "Series1";
        chart1.Series.Add(series2);
        chart1.Size = new Size(456, 128);
        chart1.TabIndex = 4;
        chart1.Text = "chart1";
        // 
        // cobDoanhThu
        // 
        cobDoanhThu.DropDownStyle = ComboBoxStyle.DropDownList;
        cobDoanhThu.FormattingEnabled = true;
        cobDoanhThu.Location = new Point(323, 36);
        cobDoanhThu.Name = "cobDoanhThu";
        cobDoanhThu.Size = new Size(132, 28);
        cobDoanhThu.TabIndex = 1;
        // 
        // label3
        // 
        label3.AutoSize = true;
        label3.Font = new Font("Segoe UI", 12F);
        label3.Location = new Point(35, 36);
        label3.Name = "label3";
        label3.Size = new Size(104, 28);
        label3.TabIndex = 0;
        label3.Text = "Doanh thu";
        // 
        // panelLichHen
        // 
        panelLichHen.AutoSize = true;
        panelLichHen.BorderStyle = BorderStyle.FixedSingle;
        panelLichHen.Controls.Add(lbLichHen);
        panelLichHen.Controls.Add(chart2);
        panelLichHen.Controls.Add(cobLichHen);
        panelLichHen.Controls.Add(label4);
        panelLichHen.Location = new Point(1080, 3);
        panelLichHen.Margin = new Padding(20, 3, 3, 20);
        panelLichHen.Name = "panelLichHen";
        panelLichHen.Size = new Size(461, 254);
        panelLichHen.TabIndex = 10;
        // 
        // lbLichHen
        // 
        lbLichHen.AutoSize = true;
        lbLichHen.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
        lbLichHen.Location = new Point(31, 79);
        lbLichHen.Name = "lbLichHen";
        lbLichHen.Size = new Size(29, 35);
        lbLichHen.TabIndex = 4;
        lbLichHen.Text = "0";
        // 
        // chart2
        // 
        chartArea3.AxisX.LabelStyle.Enabled = false;
        chartArea3.AxisX.LineColor = Color.Transparent;
        chartArea3.AxisX.LineWidth = 0;
        chartArea3.AxisX.MajorGrid.Enabled = false;
        chartArea3.AxisX.MajorTickMark.Enabled = false;
        chartArea3.AxisY.LabelStyle.Enabled = false;
        chartArea3.AxisY.LabelStyle.ForeColor = Color.Transparent;
        chartArea3.AxisY.LineWidth = 0;
        chartArea3.AxisY.MajorGrid.Enabled = false;
        chartArea3.AxisY.MajorTickMark.Enabled = false;
        chartArea3.Name = "ChartArea1";
        chart2.ChartAreas.Add(chartArea3);
        legend3.Enabled = false;
        legend3.Name = "Legend1";
        chart2.Legends.Add(legend3);
        chart2.Location = new Point(-1, 121);
        chart2.Name = "chart2";
        series3.BorderWidth = 3;
        series3.ChartArea = "ChartArea1";
        series3.ChartType = SeriesChartType.Spline;
        series3.Color = Color.Gold;
        series3.Legend = "Legend1";
        series3.Name = "Series1";
        chart2.Series.Add(series3);
        chart2.Size = new Size(457, 128);
        chart2.TabIndex = 8;
        chart2.Text = "chart1";
        // 
        // cobLichHen
        // 
        cobLichHen.DropDownStyle = ComboBoxStyle.DropDownList;
        cobLichHen.FormattingEnabled = true;
        cobLichHen.Location = new Point(324, 40);
        cobLichHen.Name = "cobLichHen";
        cobLichHen.Size = new Size(132, 28);
        cobLichHen.TabIndex = 1;
        // 
        // label4
        // 
        label4.AutoSize = true;
        label4.Font = new Font("Segoe UI", 12F);
        label4.Location = new Point(31, 36);
        label4.Name = "label4";
        label4.Size = new Size(170, 28);
        label4.TabIndex = 0;
        label4.Text = "Lịch hẹn đang chờ";
        // 
        // panelXuHuong
        // 
        // panelXuHuong.AutoSize = true;
        // panelXuHuong.BorderStyle = BorderStyle.FixedSingle;
        // panelXuHuong.Controls.Add(cobXuHuong);
        // panelXuHuong.Controls.Add(label5);
        // panelXuHuong.Location = new Point(80, 297);
        // panelXuHuong.Margin = new Padding(80, 20, 20, 20);
        // panelXuHuong.Name = "panelXuHuong";
        // panelXuHuong.Size = new Size(960, 64);
        // panelXuHuong.TabIndex = 12;
        // // 
        // // cobXuHuong
        // // 
        // cobXuHuong.DropDownStyle = ComboBoxStyle.DropDownList;
        // cobXuHuong.FormattingEnabled = true;
        // cobXuHuong.Location = new Point(823, 31);
        // cobXuHuong.Name = "cobXuHuong";
        // cobXuHuong.Size = new Size(132, 28);
        // cobXuHuong.TabIndex = 1;
        // 
        // label5
        // 
        // label5.AutoSize = true;
        // label5.Font = new Font("Segoe UI", 12F);
        // label5.Location = new Point(35, 27);
        // label5.Name = "label5";
        // label5.Size = new Size(170, 28);
        // label5.TabIndex = 0;
        // label5.Text = "Xu hướng lịch hẹn";
        
        // panelCongXuat
        
        // panelCongXuat.AutoSize = true;
        // panelCongXuat.BorderStyle = BorderStyle.FixedSingle;
        // panelCongXuat.Controls.Add(cobCongXuat);
        // panelCongXuat.Controls.Add(label6);
        // panelCongXuat.Location = new Point(1080, 297);
        // panelCongXuat.Margin = new Padding(20, 20, 3, 3);
        // panelCongXuat.Name = "panelCongXuat";
        // panelCongXuat.Size = new Size(461, 65);
        // panelCongXuat.TabIndex = 12;
        // // 
        // // cobCongXuat
        // // 
        // cobCongXuat.DropDownStyle = ComboBoxStyle.DropDownList;
        // cobCongXuat.FormattingEnabled = true;
        // cobCongXuat.Location = new Point(324, 19);
        // cobCongXuat.Name = "cobCongXuat";
        // cobCongXuat.Size = new Size(132, 28);
        // cobCongXuat.TabIndex = 1;
        // // 
        // // label6
        // // 
        label6.AutoSize = true;
        label6.Font = new Font("Segoe UI", 12F);
        label6.Location = new Point(25, 7);
        label6.Name = "label6";
        label6.Size = new Size(176, 56);
        label6.TabIndex = 0;
        label6.Text = "Công suất sử dụng\r\n hàng ngày";
        // 
        // panel1
        // 
        panel1.AutoSize = true;
        panel1.BorderStyle = BorderStyle.FixedSingle;
        panel1.Controls.Add(dgvCuocHen);
        panel1.Controls.Add(cobCuocHen);
        panel1.Controls.Add(cobTrangThai);
        panel1.Controls.Add(label7);
        panel1.Location = new Point(80, 401);
        panel1.Margin = new Padding(80, 20, 20, 20);
        panel1.Name = "panel1";
        panel1.Size = new Size(964, 332);
        panel1.TabIndex = 13;
        // 
        // dgvCuocHen
        // 
        dgvCuocHen.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dgvCuocHen.Location = new Point(7, 106);
        dgvCuocHen.Name = "dgvCuocHen";
        dgvCuocHen.RowHeadersWidth = 51;
        dgvCuocHen.Size = new Size(948, 221);
        dgvCuocHen.TabIndex = 3;
        // 
        // cobCuocHen
        // 
        cobCuocHen.DropDownStyle = ComboBoxStyle.DropDownList;
        cobCuocHen.FormattingEnabled = true;
        cobCuocHen.Location = new Point(588, 39);
        cobCuocHen.Name = "cobCuocHen";
        cobCuocHen.Size = new Size(132, 28);
        cobCuocHen.TabIndex = 2;
        // 
        // cobTrangThai
        // 
        cobTrangThai.DropDownStyle = ComboBoxStyle.DropDownList;
        cobTrangThai.FormattingEnabled = true;
        cobTrangThai.Location = new Point(750, 39);
        cobTrangThai.Name = "cobTrangThai";
        cobTrangThai.Size = new Size(209, 28);
        cobTrangThai.TabIndex = 1;
        // 
        // label7
        // 
        label7.AutoSize = true;
        label7.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        label7.Location = new Point(35, 39);
        label7.Name = "label7";
        label7.Size = new Size(136, 28);
        label7.TabIndex = 0;
        label7.Text = "Các cuộc hẹn";
        // 
        // panelHieuXuat
        // 
        panelHieuXuat.AutoSize = true;
        panelHieuXuat.BorderStyle = BorderStyle.FixedSingle;
        panelHieuXuat.Controls.Add(cobHieuXuat);
        panelHieuXuat.Controls.Add(label8);
        panelHieuXuat.Controls.Add(tabControlHieuXuat);
        panelHieuXuat.Location = new Point(1084, 401);
        panelHieuXuat.Margin = new Padding(20, 20, 3, 3);
        panelHieuXuat.Name = "panelHieuXuat";
        panelHieuXuat.Size = new Size(461, 332);
        panelHieuXuat.TabIndex = 14;
        // 
        // cobHieuXuat
        // 
        cobHieuXuat.DropDownStyle = ComboBoxStyle.DropDownList;
        cobHieuXuat.FormattingEnabled = true;
        cobHieuXuat.Location = new Point(320, 40);
        cobHieuXuat.Name = "cobHieuXuat";
        cobHieuXuat.Size = new Size(136, 28);
        cobHieuXuat.TabIndex = 2;
        // 
        // label8
        // 
        label8.AutoSize = true;
        label8.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        label8.Location = new Point(19, 40);
        label8.Name = "label8";
        label8.Size = new Size(102, 28);
        label8.TabIndex = 1;
        label8.Text = "Hiệu suất";
        // 
        // tabControlHieuXuat
        // 
        tabControlHieuXuat.Controls.Add(tabPageChuyenKhoa);
        tabControlHieuXuat.Controls.Add(tabPageBacSi);
        tabControlHieuXuat.Font = new Font("Segoe UI", 10F);
        tabControlHieuXuat.Location = new Point(3, 109);
        tabControlHieuXuat.Name = "tabControlHieuXuat";
        tabControlHieuXuat.SelectedIndex = 0;
        tabControlHieuXuat.Size = new Size(449, 218);
        tabControlHieuXuat.TabIndex = 0;
        // 
        // tabPageChuyenKhoa
        // 
        tabPageChuyenKhoa.AutoScroll = true;
        tabPageChuyenKhoa.Controls.Add(dgvChuyenKhoa);
        tabPageChuyenKhoa.Location = new Point(4, 32);
        tabPageChuyenKhoa.Name = "tabPageChuyenKhoa";
        tabPageChuyenKhoa.Padding = new Padding(3);
        tabPageChuyenKhoa.Size = new Size(441, 182);
        tabPageChuyenKhoa.TabIndex = 0;
        tabPageChuyenKhoa.Text = "Chuyên khoa";
        tabPageChuyenKhoa.UseVisualStyleBackColor = true;
        // 
        // dgvChuyenKhoa
        // 
        dgvChuyenKhoa.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dgvChuyenKhoa.Location = new Point(6, 6);
        dgvChuyenKhoa.Name = "dgvChuyenKhoa";
        dgvChuyenKhoa.RowHeadersWidth = 51;
        dgvChuyenKhoa.Size = new Size(429, 170);
        dgvChuyenKhoa.TabIndex = 0;
        // 
        // tabPageBacSi
        // 
        tabPageBacSi.AutoScroll = true;
        tabPageBacSi.Controls.Add(dgvBacSi);
        tabPageBacSi.Location = new Point(4, 32);
        tabPageBacSi.Name = "tabPageBacSi";
        tabPageBacSi.Padding = new Padding(3);
        tabPageBacSi.Size = new Size(441, 145);
        tabPageBacSi.TabIndex = 1;
        tabPageBacSi.Text = "Bác sĩ";
        tabPageBacSi.UseVisualStyleBackColor = true;
        // 
        // dgvBacSi
        // 
        dgvBacSi.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dgvBacSi.Location = new Point(6, 6);
        dgvBacSi.Name = "dgvBacSi";
        dgvBacSi.RowHeadersWidth = 51;
        dgvBacSi.Size = new Size(429, 133);
        dgvBacSi.TabIndex = 0;
        // 
        // DashboardForm
        // 
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        AutoScroll = true;
        ClientSize = new Size(1614, 919);
        Controls.Add(flowLayoutPanel1);
        Controls.Add(HeaderPanel);
        Margin = new Padding(3, 4, 3, 4);
        Name = "DashboardForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Dashboar";
        TopMost = true;
        HeaderPanel.ResumeLayout(false);
        HeaderPanel.PerformLayout();
        flowLayoutPanel1.ResumeLayout(false);
        flowLayoutPanel1.PerformLayout();
        panelKhachHang.ResumeLayout(false);
        panelKhachHang.PerformLayout();
        ((ISupportInitialize)chartKhachHang).EndInit();
        panelDoanhThu.ResumeLayout(false);
        panelDoanhThu.PerformLayout();
        ((ISupportInitialize)chart1).EndInit();
        panelLichHen.ResumeLayout(false);
        panelLichHen.PerformLayout();
        ((ISupportInitialize)chart2).EndInit();
        panelXuHuong.ResumeLayout(false);
        panelXuHuong.PerformLayout();
        panelCongXuat.ResumeLayout(false);
        panelCongXuat.PerformLayout();
        panel1.ResumeLayout(false);
        panel1.PerformLayout();
        ((ISupportInitialize)dgvCuocHen).EndInit();
        panelHieuXuat.ResumeLayout(false);
        panelHieuXuat.PerformLayout();
        tabControlHieuXuat.ResumeLayout(false);
        tabPageChuyenKhoa.ResumeLayout(false);
        ((ISupportInitialize)dgvChuyenKhoa).EndInit();
        tabPageBacSi.ResumeLayout(false);
        ((ISupportInitialize)dgvBacSi).EndInit();
        ResumeLayout(false);

    }
    private void PopulateDataGrids()
    {
        // === DataGridView Các cuộc hẹn (dgvCuocHen) ===
        DataTable dtAppointments = new DataTable();

        // Khởi tạo các cột
        dtAppointments.Columns.Add("Tên bệnh nhân", typeof(string));
        dtAppointments.Columns.Add("Tên bác sĩ", typeof(string));
        dtAppointments.Columns.Add("Ngày", typeof(string));
        dtAppointments.Columns.Add("Giờ", typeof(string));
        dtAppointments.Columns.Add("Trạng thái", typeof(string));

        // Thêm dữ liệu cứng (hàng)
        dtAppointments.Rows.Add("Nguyễn Văn A", "Bs. Trần Thị B", "20/11/2025", "09:00", "Đã xác nhận");
        dtAppointments.Rows.Add("Lê Thị C", "Bs. Nguyễn Văn D", "20/11/2025", "14:30", "Chờ xác nhận");
        dtAppointments.Rows.Add("Phạm Văn E", "Bs. Trần Thị B", "19/11/2025", "11:00", "Đã hủy");
        dtAppointments.Rows.Add("Vũ Thị G", "Bs. Lý H", "21/11/2025", "16:00", "Đã xác nhận");

        // Gán DataSource
        dgvCuocHen.DataSource = dtAppointments;
        dgvCuocHen.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        // === DataGridView Hiệu suất Chuyên khoa (dgvChuyenKhoa) ===
        DataTable dtSpecialties = new DataTable();
        dtSpecialties.Columns.Add("Chuyên khoa", typeof(string));
        dtSpecialties.Columns.Add("SL Lịch hẹn", typeof(int));
        dtSpecialties.Columns.Add("Doanh thu (₫)", typeof(string)); // Dùng string để hiển thị tiền tệ

        dtSpecialties.Rows.Add("Nội tiết", 45, "15.000.000");
        dtSpecialties.Rows.Add("Cơ xương khớp", 30, "10.500.000");
        dtSpecialties.Rows.Add("Da liễu", 55, "18.000.000");

        dgvChuyenKhoa.DataSource = dtSpecialties;
        dgvChuyenKhoa.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;


        // === DataGridView Hiệu suất Bác sĩ (dgvBacSi) ===
        DataTable dtDoctors = new DataTable();
        dtDoctors.Columns.Add("Tên bác sĩ", typeof(string));
        dtDoctors.Columns.Add("Lịch hẹn Đã xác nhận", typeof(int));
        dtDoctors.Columns.Add("Lịch hẹn Đã hủy", typeof(int));

        dtDoctors.Rows.Add("Bs. Trần Thị B", 25, 2);
        dtDoctors.Rows.Add("Bs. Nguyễn Văn D", 15, 5);
        dtDoctors.Rows.Add("Bs. Lý H", 10, 1);

        dgvBacSi.DataSource = dtDoctors;
        dgvBacSi.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
    }
    private void PopulateChartsWithMockData()
    {
        // --- 1. Biểu đồ Khách hàng mới (chartKhachHang) ---
        chartKhachHang.Series.Clear();
        chartKhachHang.ChartAreas[0].AxisX.LabelStyle.Enabled = true; // QUAN TRỌNG: Bật hiển thị nhãn trục X

        Series customerSeries = new Series("NewCustomers")
        {
            ChartType = SeriesChartType.Spline,
            Color = Color.LimeGreen,
            BorderWidth = 3,
            IsVisibleInLegend = false
        };
        chartKhachHang.Series.Add(customerSeries);

        // Sử dụng số thay vì chuỗi cho trục X để đường cong hiển thị đúng
        customerSeries.Points.AddY(3);
        customerSeries.Points.AddY(5);
        customerSeries.Points.AddY(2);
        customerSeries.Points.AddY(7);
        customerSeries.Points.AddY(4);
        customerSeries.Points.AddY(6);
        customerSeries.Points.AddY(8);

        // Thiết lập nhãn cho trục X
        chartKhachHang.ChartAreas[0].AxisX.CustomLabels.Clear();
        string[] days = { "Th 2", "Th 3", "Th 4", "Th 5", "Th 6", "Th 7", "CN" };
        for (int i = 0; i < days.Length; i++)
        {
            chartKhachHang.ChartAreas[0].AxisX.CustomLabels.Add(i + 0.5, i + 1.5, days[i]);
        }

        // Cấu hình chart area
        ChartArea areaCustomer = chartKhachHang.ChartAreas[0];
        areaCustomer.AxisY.Minimum = 0;
        areaCustomer.AxisY.Maximum = 10;
        areaCustomer.AxisX.MajorGrid.Enabled = false;
        areaCustomer.AxisY.MajorGrid.Enabled = false;

        // --- 2. Biểu đồ Doanh thu (chart1) ---
        chart1.Series.Clear();
        chart1.ChartAreas[0].AxisX.LabelStyle.Enabled = true;

        Series revenueSeries = new Series("Revenue")
        {
            ChartType = SeriesChartType.Spline,
            Color = Color.Gray,
            BorderWidth = 3,
            IsVisibleInLegend = false
        };
        chart1.Series.Add(revenueSeries);

        // Dữ liệu doanh thu
        revenueSeries.Points.AddY(1.5);
        revenueSeries.Points.AddY(2.0);
        revenueSeries.Points.AddY(1.0);
        revenueSeries.Points.AddY(3.5);
        revenueSeries.Points.AddY(2.5);
        revenueSeries.Points.AddY(4.0);
        revenueSeries.Points.AddY(3.0);

        // Thiết lập nhãn cho trục X
        chart1.ChartAreas[0].AxisX.CustomLabels.Clear();
        for (int i = 0; i < days.Length; i++)
        {
            chart1.ChartAreas[0].AxisX.CustomLabels.Add(i + 0.5, i + 1.5, days[i]);
        }

        ChartArea areaRevenue = chart1.ChartAreas[0];
        areaRevenue.AxisY.Minimum = 0;
        areaRevenue.AxisY.Maximum = 5.0;
        areaRevenue.AxisX.MajorGrid.Enabled = false;
        areaRevenue.AxisY.MajorGrid.Enabled = false;

        lbDoanhThu.Text = "610.000 ₫";

        // --- 3. Biểu đồ Lịch hẹn đang chờ (chart2) ---
        chart2.Series.Clear();
        chart2.ChartAreas[0].AxisX.LabelStyle.Enabled = true;

        Series pendingSeries = new Series("PendingAppointments")
        {
            ChartType = SeriesChartType.Spline,
            Color = Color.Gold,
            BorderWidth = 3,
            IsVisibleInLegend = false
        };
        chart2.Series.Add(pendingSeries);

        // Dữ liệu lịch hẹn
        pendingSeries.Points.AddY(1);
        pendingSeries.Points.AddY(3);
        pendingSeries.Points.AddY(1);
        pendingSeries.Points.AddY(2);
        pendingSeries.Points.AddY(0);
        pendingSeries.Points.AddY(4);
        pendingSeries.Points.AddY(1);

        // Thiết lập nhãn cho trục X
        chart2.ChartAreas[0].AxisX.CustomLabels.Clear();
        for (int i = 0; i < days.Length; i++)
        {
            chart2.ChartAreas[0].AxisX.CustomLabels.Add(i + 0.5, i + 1.5, days[i]);
        }

        ChartArea areaPending = chart2.ChartAreas[0];
        areaPending.AxisY.Minimum = 0;
        areaPending.AxisY.Maximum = 5;
        areaPending.AxisX.MajorGrid.Enabled = false;
        areaPending.AxisY.MajorGrid.Enabled = false;

        lbLichHen.Text = "0";
    }
}
