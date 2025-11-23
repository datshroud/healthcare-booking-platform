using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using BookingCareManagement.WinForms.Areas.Admin.Services;
using BookingCareManagement.WinForms.Areas.Doctor.Services;
using BookingCareManagement.WinForms.Areas.Doctor.Services.Models;
using BookingCareManagement.WinForms.Shared.Models.Dtos;
using BookingCareManagement.WinForms.Shared.State;
namespace BookingCareManagement.WinForms;

public sealed class DashboardForm : Form
{
    private readonly AdminAppointmentsApiClient? _adminAppointmentsApiClient;
    private readonly DoctorDashboardApiClient? _dashboardApiClient = null;
    private readonly DoctorAppointmentsApiClient? _appointmentsApiClient = null;
    private readonly SessionState? _sessionState;
    private readonly bool _useAdminDashboard;
    private static readonly CultureInfo VietnamCulture = CultureInfo.GetCultureInfo("vi-VN");

    private static readonly Dictionary<string, string> AppointmentStatusMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Đã xác nhận", "Approved" },
        { "Chờ xác nhận", "Pending" },
        { "Đã hủy", "Canceled" },
        { "Đã từ chối", "Rejected" },
        { "Vắng mặt", "NoShow" }
    };

    public DashboardForm()
    {
        // Initialize designer controls
        InitializeComponent();

        // Populate runtime-only data (guarded to avoid Designer errors)
        if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
        {
            ApplyLocalization();
            PopulateTimeRangeComboBoxes();
            ConfigureDataGrids();
            ConfigureTrendChart();
            ConfigureSparklineCharts();
            RegisterEventHandlers();
            flowLayoutPanel1.SizeChanged += (_, _) => AdjustTrendPanelWidth();
            Resize += (_, _) => AdjustTrendPanelWidth();
            AdjustTrendPanelWidth();
        }

        Text = "Dashboard";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(800, 600);
        Font = new Font("Segoe UI", 10);
        BackColor = Color.FromArgb(243, 244, 246);
        _adminAppointmentsApiClient = null;
        _sessionState = null;
        _useAdminDashboard = false;
    }

    public DashboardForm(
        DoctorDashboardApiClient dashboardApiClient,
        DoctorAppointmentsApiClient appointmentsApiClient,
        AdminAppointmentsApiClient adminAppointmentsApiClient,
        SessionState sessionState) : this()
    {
        _dashboardApiClient = dashboardApiClient;
        _appointmentsApiClient = appointmentsApiClient;
        _adminAppointmentsApiClient = adminAppointmentsApiClient;
        _sessionState = sessionState;
        _useAdminDashboard = sessionState?.IsAdmin == true && sessionState?.IsDoctor != true;
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
        {
            return;
        }

        if (_useAdminDashboard)
        {
            if (_adminAppointmentsApiClient is null)
            {
                return;
            }

            var adminTasks = new List<Task>
            {
                LoadNewCustomersAsync(),
                LoadRevenueAsync(),
                LoadOccupancyAsync(),
                LoadAppointmentTrendAsync(),
                LoadAppointmentsAsync()
            };

            await Task.WhenAll(adminTasks);
            return;
        }

        if (_dashboardApiClient is null || _appointmentsApiClient is null)
        {
            return;
        }

        var loadTasks = new List<Task>
        {
            LoadNewCustomersAsync(),
            LoadRevenueAsync(),
            LoadOccupancyAsync(),
            LoadAppointmentTrendAsync(),
            LoadAppointmentsAsync()
        };

        await Task.WhenAll(loadTasks);
    }

    // Populate combo boxes at runtime only
    private void PopulateTimeRangeComboBoxes()
    {
        var timeRanges = new[] { "Tuần này", "Tuần trước", "Tháng này", "Tháng trước", "3 tháng qua", "6 tháng qua", "12 tháng qua" };

        cobXuHuong.Items.AddRange(timeRanges);
        if (cobXuHuong.Items.Count > 0) cobXuHuong.SelectedIndex = 0;

        cobDoanhThu.Items.AddRange(timeRanges);
        if (cobDoanhThu.Items.Count > 0) cobDoanhThu.SelectedIndex = 0;

        cobKhachHang.Items.AddRange(timeRanges);
        if (cobKhachHang.Items.Count > 0) cobKhachHang.SelectedIndex = 0;

        cobLichHen.Items.AddRange(timeRanges);
        if (cobLichHen.Items.Count > 0) cobLichHen.SelectedIndex = 0;

        cobCuocHen.Items.AddRange(timeRanges);
        if (cobCuocHen.Items.Count > 0) cobCuocHen.SelectedIndex = 0;

        // Populate status combobox for appointments
        var statuses = new[] { "Tất cả các trạng thái", "Đã xác nhận", "Chờ xác nhận", "Đã hủy", "Đã từ chối", "Vắng mặt" };
        cobTrangThai.Items.AddRange(statuses);
        if (cobTrangThai.Items.Count > 0) cobTrangThai.SelectedIndex = 0;
    }

    private void ConfigureDataGrids()
    {
        dgvCuocHen.AutoGenerateColumns = false;
        dgvCuocHen.Columns.Clear();
        dgvCuocHen.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvCuocHen.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tên bệnh nhân", DataPropertyName = nameof(AppointmentGridRow.PatientName) });
        dgvCuocHen.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tên bác sĩ", DataPropertyName = nameof(AppointmentGridRow.DoctorName) });
        dgvCuocHen.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ngày", DataPropertyName = nameof(AppointmentGridRow.Date) });
        dgvCuocHen.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Giờ", DataPropertyName = nameof(AppointmentGridRow.Time) });
        dgvCuocHen.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Trạng thái", DataPropertyName = nameof(AppointmentGridRow.Status) });

        dgvChuyenKhoa.AutoGenerateColumns = false;
        dgvChuyenKhoa.Columns.Clear();
        dgvChuyenKhoa.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvChuyenKhoa.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Chuyên khoa", DataPropertyName = nameof(SpecialtyPerformanceRow.Specialty) });
        dgvChuyenKhoa.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "SL Lịch hẹn", DataPropertyName = nameof(SpecialtyPerformanceRow.AppointmentCount) });
        dgvChuyenKhoa.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Doanh thu (₫)", DataPropertyName = nameof(SpecialtyPerformanceRow.Revenue) });

        dgvBacSi.AutoGenerateColumns = false;
        dgvBacSi.Columns.Clear();
        dgvBacSi.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvBacSi.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tên bác sĩ", DataPropertyName = nameof(DoctorPerformanceRow.Doctor) });
        dgvBacSi.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Lịch hẹn Đã xác nhận", DataPropertyName = nameof(DoctorPerformanceRow.Confirmed) });
        dgvBacSi.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Lịch hẹn Đã hủy", DataPropertyName = nameof(DoctorPerformanceRow.Canceled) });
    }

    private void ConfigureTrendChart()
    {
        var area = chartAppointmentTrend.ChartAreas[0];
        area.AxisX.MajorGrid.Enabled = false;
        area.AxisY.MajorGrid.Enabled = false;
        area.AxisY.Minimum = 0;

        var series = chartAppointmentTrend.Series[0];
        series.ChartType = SeriesChartType.Line;
        series.BorderWidth = 3;
        series.Color = Color.FromArgb(59, 130, 246);
        series.IsVisibleInLegend = false;
    }

    private void RegisterEventHandlers()
    {
        cobKhachHang.SelectedIndexChanged += async (_, _) => await LoadNewCustomersAsync();
        cobDoanhThu.SelectedIndexChanged += async (_, _) => await LoadRevenueAsync();
        cobLichHen.SelectedIndexChanged += async (_, _) => await LoadOccupancyAsync();
        cobCuocHen.SelectedIndexChanged += async (_, _) => await LoadAppointmentsAsync();
        cobHieuXuat.SelectedIndexChanged += async (_, _) => await LoadAppointmentsAsync();
        cobTrangThai.SelectedIndexChanged += async (_, _) => await LoadAppointmentsAsync();
        cobXuHuong.SelectedIndexChanged += cobXuHuong_SelectedIndexChanged;
    }

    private void ApplyLocalization()
    {
        label1.Text = "Khách hàng mới";
        label3.Text = "Doanh thu";
        label4.Text = "Tỷ lệ lấp đầy";
        label5.Text = "Xu hướng lịch hẹn";
        label7.Text = "Các cuộc hẹn";
        label8.Text = "Hiệu suất";
        labelConfirmedTitle.Text = "Đã đặt lịch hẹn";
        labelCanceledTitle.Text = "Các cuộc hẹn đã hủy";
        lbTrendRange.Text = "Tuần này";
        label2.Text = "Xin chào,";
    }

    private void ConfigureSparklineCharts()
    {
        ConfigureSparklineChartControl(chartKhachHang, Color.FromArgb(34, 197, 94));
        ConfigureSparklineChartControl(chart1, Color.FromArgb(107, 114, 128));
        ConfigureSparklineChartControl(chart2, Color.FromArgb(234, 179, 8));
        ConfigureSparklineChartControl(chartAppointmentTrend, Color.FromArgb(59, 130, 246));
    }

    private static void ConfigureSparklineChartControl(Chart chart, Color lineColor)
    {
        if (chart.ChartAreas.Count == 0 || chart.Series.Count == 0)
        {
            return;
        }

        var area = chart.ChartAreas[0];
        area.AxisX.LabelStyle.Enabled = false;
        area.AxisX.LineColor = Color.Transparent;
        area.AxisX.MajorGrid.Enabled = false;
        area.AxisX.MajorTickMark.Enabled = false;
        area.AxisX.IsMarginVisible = false;
        area.AxisY.MajorGrid.Enabled = false;
        area.AxisY.MajorTickMark.Enabled = false;
        area.AxisY.LineColor = Color.Transparent;
        area.AxisY.Minimum = 0;
        area.InnerPlotPosition = new ElementPosition(5, 5, 90, 90);
        area.Position = new ElementPosition(1, 1, 98, 98);

        var series = chart.Series[0];
        series.ChartType = SeriesChartType.Spline;
        series.BorderWidth = 3;
        series.Color = lineColor;
        series.IsVisibleInLegend = false;
        series.IsXValueIndexed = true;
        series.XValueType = ChartValueType.Int32;
        series.MarkerStyle = MarkerStyle.Circle;
        series.MarkerSize = 8;
        series.MarkerColor = lineColor;
        series.MarkerBorderColor = Color.White;
        series.MarkerBorderWidth = 2;
        series.ToolTip = "#VALY";
    }

    private void AdjustTrendPanelWidth()
    {
        if (panelXuHuong is null || flowLayoutPanel1 is null)
        {
            return;
        }

        var available = flowLayoutPanel1.ClientSize.Width - panelXuHuong.Margin.Left - panelXuHuong.Margin.Right;
        if (available <= 0)
        {
            return;
        }

        panelXuHuong.Width = available;

        if (chartAppointmentTrend is not null)
        {
            var width = panelXuHuong.Width - chartAppointmentTrend.Left - 20;
            chartAppointmentTrend.Width = Math.Max(200, width);
            var height = panelXuHuong.Height - chartAppointmentTrend.Top - 20;
            chartAppointmentTrend.Height = Math.Max(150, height);
        }

        flowLayoutPanel1.AutoScrollMinSize = new Size(0, panelHieuXuat.Bottom + 60);
    }

    private static string ResolveRangeToken(string? selection)
    {
        return selection switch
        {
            "Tuần này" => "this-week",
            "Tuần trước" => "last-week",
            "Tháng này" => "this-month",
            "Tháng trước" => "last-month",
            "3 tháng qua" => "three-months",
            "6 tháng qua" => "six-months",
            "12 tháng qua" => "twelve-months",
            _ => "this-week"
        };
    }

    private static (DateOnly From, DateOnly To) ResolveDateRange(string? selection)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var offset = ((int)today.DayOfWeek + 6) % 7;
        var startOfWeek = today.AddDays(-offset);
        var startOfMonth = new DateOnly(today.Year, today.Month, 1);

        return selection switch
        {
            "Tuần này" => (startOfWeek, startOfWeek.AddDays(7)),
            "Tuần trước" => (startOfWeek.AddDays(-7), startOfWeek),
            "Tháng này" => (startOfMonth, startOfMonth.AddMonths(1)),
            "Tháng trước" => (startOfMonth.AddMonths(-1), startOfMonth),
            "3 tháng qua" => (startOfMonth.AddMonths(-2), startOfMonth.AddMonths(1)),
            "6 tháng qua" => (startOfMonth.AddMonths(-5), startOfMonth.AddMonths(1)),
            "12 tháng qua" => (startOfMonth.AddMonths(-11), startOfMonth.AddMonths(1)),
            _ => (today, today.AddDays(7))
        };
    }

    private static string FormatCurrency(decimal value)
    {
        return value.ToString("C0", VietnamCulture);
    }

    private static string FormatNumber(decimal value)
    {
        return value.ToString("N0", VietnamCulture);
    }

    private static string FormatPercent(decimal value)
    {
        var clamped = Math.Clamp(value, 0, 100);
        return $"{clamped:N1}%";
    }

    private static string FormatRangeLabel(DateOnly from, DateOnly toExclusive)
    {
        return $"{from:dd/MM} - {toExclusive.AddDays(-1):dd/MM}";
    }

    private static IReadOnlyList<DashboardMetricPointDto> BuildDailyPoints(
        DateOnly start,
        DateOnly endExclusive,
        IReadOnlyDictionary<DateOnly, decimal> aggregated)
    {
        var points = new List<DashboardMetricPointDto>();
        for (var date = start; date < endExclusive; date = date.AddDays(1))
        {
            var value = aggregated.TryGetValue(date, out var current) ? current : 0m;
            points.Add(new DashboardMetricPointDto
            {
                Date = date,
                Value = value
            });
        }

        return points;
    }

    private static DateOnly ToLocalDate(DateTime utcDateTime)
    {
        var normalized = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
        return DateOnly.FromDateTime(normalized.ToLocalTime());
    }

    private static string BuildPatientKey(string? patientId, string? phone, string? name)
    {
        if (!string.IsNullOrWhiteSpace(patientId))
        {
            return patientId.Trim();
        }

        if (!string.IsNullOrWhiteSpace(phone))
        {
            return phone.Trim();
        }

        return string.IsNullOrWhiteSpace(name) ? "unknown-patient" : name.Trim();
    }

    private static bool IsConfirmed(string status)
    {
        return string.Equals(status, "Approved", StringComparison.OrdinalIgnoreCase)
               || string.Equals(status, "Pending", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsCanceled(string status)
    {
        return string.Equals(status, "Canceled", StringComparison.OrdinalIgnoreCase)
               || string.Equals(status, "Rejected", StringComparison.OrdinalIgnoreCase);
    }

    private void RenderSparklineChart(Chart chart, IReadOnlyList<DashboardMetricPointDto> points, bool clampToHundred = false)
    {
        var ordered = points.OrderBy(p => p.Date).ToList();
        var series = chart.Series[0];
        var area = chart.ChartAreas[0];

        series.Points.Clear();
        series.XValueType = ChartValueType.Int32;
        series.IsXValueIndexed = true;
        area.AxisY.Minimum = 0;

        // pad single-point series so the sparkline stretches horizontally
        var paddedPoints = ordered.Count == 1
            ? new List<DashboardMetricPointDto>
            {
                new DashboardMetricPointDto { Date = ordered[0].Date.AddDays(-1), Value = ordered[0].Value },
                ordered[0],
                new DashboardMetricPointDto { Date = ordered[0].Date.AddDays(1), Value = ordered[0].Value }
            }
            : ordered;

        decimal maxValue = 1;
        for (int i = 0; i < paddedPoints.Count; i++)
        {
            var point = paddedPoints[i];
            var value = clampToHundred ? Math.Clamp(point.Value, 0, 100) : point.Value;
            var index = series.Points.AddXY(i, (double)value);
            if (index >= 0)
            {
                var dataPoint = series.Points[index];
                dataPoint.AxisLabel = point.Date.ToString("dd/MM", VietnamCulture);
                dataPoint.ToolTip = $"{point.Date:dd/MM}: {value:N0}";
            }
            maxValue = Math.Max(maxValue, value);
        }

        area.AxisY.Maximum = paddedPoints.Count == 0
            ? 1
            : Math.Ceiling((double)maxValue) + 1;
        area.AxisX.Minimum = 0;
        area.AxisX.Maximum = Math.Max(paddedPoints.Count - 1, 1);
    }

    private async Task LoadNewCustomersAsync(CancellationToken cancellationToken = default)
    {
        if (_useAdminDashboard)
        {
            await LoadAdminNewCustomersAsync(cancellationToken);
            return;
        }

        try
        {
            cobKhachHang.Enabled = false;
            lbKhachHang.Text = "--";
            label1.Text = "Khách hàng mới";

            var rangeToken = ResolveRangeToken(cobKhachHang.SelectedItem?.ToString());
            var response = await _dashboardApiClient!.GetNewCustomersAsync(rangeToken, cancellationToken);

            lbKhachHang.Text = FormatNumber(response.Total);
            label1.Text = $"Khách hàng mới ({response.RangeLabel})";
            RenderSparklineChart(chartKhachHang, response.Points);
        }
        catch (Exception ex)
        {
            lbKhachHang.Text = "--";
            MessageBox.Show($"Không thể tải khách hàng mới: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            cobKhachHang.Enabled = true;
        }
    }

    private async Task LoadRevenueAsync(CancellationToken cancellationToken = default)
    {
        if (_useAdminDashboard)
        {
            await LoadAdminRevenueAsync(cancellationToken);
            return;
        }

        try
        {
            cobDoanhThu.Enabled = false;
            lbDoanhThu.Text = "--";
            label3.Text = "Doanh thu";

            var rangeToken = ResolveRangeToken(cobDoanhThu.SelectedItem?.ToString());
            var response = await _dashboardApiClient!.GetRevenueAsync(rangeToken, cancellationToken);

            lbDoanhThu.Text = FormatCurrency(response.Total);
            label3.Text = $"Doanh thu ({response.RangeLabel})";
            RenderSparklineChart(chart1, response.Points);
        }
        catch (Exception ex)
        {
            lbDoanhThu.Text = "--";
            MessageBox.Show($"Không thể tải doanh thu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            cobDoanhThu.Enabled = true;
        }
    }

    private async Task LoadOccupancyAsync(CancellationToken cancellationToken = default)
    {
        if (_useAdminDashboard)
        {
            await LoadAdminOccupancyAsync(cancellationToken);
            return;
        }

        try
        {
            cobLichHen.Enabled = false;
            lbLichHen.Text = "--";
            label4.Text = "Tỷ lệ lấp đầy";

            var rangeToken = ResolveRangeToken(cobLichHen.SelectedItem?.ToString());
            var response = await _dashboardApiClient!.GetOccupancyAsync(rangeToken, cancellationToken);

            lbLichHen.Text = FormatPercent(response.Total);
            label4.Text = $"Tỷ lệ lấp đầy ({response.RangeLabel})";
            RenderSparklineChart(chart2, response.Points, clampToHundred: true);
        }
        catch (Exception ex)
        {
            lbLichHen.Text = "--";
            MessageBox.Show($"Không thể tải lấp đầy lịch hẹn: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            cobLichHen.Enabled = true;
        }
    }

    private async Task LoadAppointmentTrendAsync(CancellationToken cancellationToken = default)
    {
        if (_useAdminDashboard)
        {
            await LoadAdminAppointmentTrendAsync(cancellationToken);
            return;
        }

        try
        {
            cobXuHuong.Enabled = false;
            lbTrendRange.Text = "Đang tải...";

            var rangeToken = ResolveRangeToken(cobXuHuong.SelectedItem?.ToString());
            var response = await _dashboardApiClient!.GetAppointmentTrendAsync(rangeToken, cancellationToken);

            lbConfirmedTrend.Text = response.ConfirmedCount.ToString();
            lbCanceledTrend.Text = response.CanceledCount.ToString();
            lbTrendRange.Text = response.RangeLabel;

            RenderTrendChart(response);
        }
        catch (Exception ex)
        {
            lbTrendRange.Text = "Không thể tải dữ liệu";
            MessageBox.Show($"Không thể tải xu hướng lịch hẹn: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            cobXuHuong.Enabled = true;
        }
    }

    private void RenderTrendChart(DashboardAppointmentTrendResponse response)
    {
        var series = chartAppointmentTrend.Series[0];
        var area = chartAppointmentTrend.ChartAreas[0];

        series.Points.Clear();
        area.AxisX.CustomLabels.Clear();
        area.AxisX.LabelStyle.Enabled = true;
        area.AxisX.Interval = 1;

        var culture = CultureInfo.GetCultureInfo("vi-VN");
        decimal maxValue = 1;

        for (int i = 0; i < response.Points.Count; i++)
        {
            var point = response.Points[i];
            maxValue = Math.Max(maxValue, point.Value);
            series.Points.AddXY(point.Date.ToString("yyyy-MM-dd"), point.Value);
            var label = point.Date.ToString("ddd", culture);
            area.AxisX.CustomLabels.Add(i + 0.5, i + 1.5, label);
        }

        area.AxisY.Maximum = Math.Ceiling((double)maxValue) + 1;
    }

    private async Task LoadAppointmentsAsync(CancellationToken cancellationToken = default)
    {
        if (_useAdminDashboard)
        {
            await LoadAdminAppointmentsAsync(cancellationToken);
            return;
        }

        try
        {
            var (from, to) = ResolveDateRange(cobCuocHen.SelectedItem?.ToString());
            var appointments = await _appointmentsApiClient!.GetAppointmentsAsync(from, to, cancellationToken);
            var filtered = FilterAppointmentsByStatus(appointments);

            label7.Text = $"Cuộc hẹn ({from:dd/MM} - {to.AddDays(-1):dd/MM})";
            BindAppointmentData(filtered);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể tải danh sách cuộc hẹn: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task LoadAdminNewCustomersAsync(CancellationToken cancellationToken)
    {
        if (_adminAppointmentsApiClient is null)
        {
            return;
        }

        try
        {
            cobKhachHang.Enabled = false;
            lbKhachHang.Text = "--";
            label1.Text = "Kh�ch h�ng";

            var (from, to) = ResolveDateRange(cobKhachHang.SelectedItem?.ToString());
            var appointments = await _adminAppointmentsApiClient.GetAppointmentsAsync(from, to, cancellationToken);

            var allCustomers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var dailyCustomers = new Dictionary<DateOnly, HashSet<string>>();
            foreach (var appointment in appointments)
            {
                var date = ToLocalDate(appointment.StartUtc);
                var key = BuildPatientKey(appointment.PatientId, appointment.CustomerPhone, appointment.PatientName);
                allCustomers.Add(key);

                if (!dailyCustomers.TryGetValue(date, out var set))
                {
                    set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    dailyCustomers[date] = set;
                }

                set.Add(key);
            }

            var perDay = dailyCustomers.ToDictionary(pair => pair.Key, pair => (decimal)pair.Value.Count);
            lbKhachHang.Text = FormatNumber(allCustomers.Count);
            label1.Text = $"Kh�ch h�ng ({FormatRangeLabel(from, to)})";
            RenderSparklineChart(chartKhachHang, BuildDailyPoints(from, to, perDay));
        }
        catch (Exception ex)
        {
            lbKhachHang.Text = "--";
            MessageBox.Show($"Kh�ng th? t?i kh�ch h�ng (admin): {ex.Message}", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            cobKhachHang.Enabled = true;
        }
    }

    private async Task LoadAdminRevenueAsync(CancellationToken cancellationToken)
    {
        if (_adminAppointmentsApiClient is null)
        {
            return;
        }

        try
        {
            cobDoanhThu.Enabled = false;
            lbDoanhThu.Text = "--";
            label3.Text = "Doanh thu";

            var (from, to) = ResolveDateRange(cobDoanhThu.SelectedItem?.ToString());
            var appointments = await _adminAppointmentsApiClient.GetAppointmentsAsync(from, to, cancellationToken);
            var perDay = appointments
                .GroupBy(a => ToLocalDate(a.StartUtc))
                .ToDictionary(group => group.Key, group => group.Sum(x => x.Price));

            var total = perDay.Values.Sum();
            lbDoanhThu.Text = FormatCurrency(total);
            label3.Text = $"Doanh thu ({FormatRangeLabel(from, to)})";
            RenderSparklineChart(chart1, BuildDailyPoints(from, to, perDay));
        }
        catch (Exception ex)
        {
            lbDoanhThu.Text = "--";
            MessageBox.Show($"Kh�ng th? t?i doanh thu (admin): {ex.Message}", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            cobDoanhThu.Enabled = true;
        }
    }

    private async Task LoadAdminOccupancyAsync(CancellationToken cancellationToken)
    {
        if (_adminAppointmentsApiClient is null)
        {
            return;
        }

        try
        {
            cobLichHen.Enabled = false;
            lbLichHen.Text = "--";
            label4.Text = "T? l? l?p d?y";

            var (from, to) = ResolveDateRange(cobLichHen.SelectedItem?.ToString());
            var appointments = await _adminAppointmentsApiClient.GetAppointmentsAsync(from, to, cancellationToken);
            var perDay = appointments
                .GroupBy(a => ToLocalDate(a.StartUtc))
                .ToDictionary(group => group.Key, group =>
                {
                    var total = group.Count();
                    if (total == 0)
                    {
                        return 0m;
                    }

                    var confirmed = group.Count(a => IsConfirmed(a.Status));
                    return Math.Round((decimal)confirmed / total * 100m, 2, MidpointRounding.AwayFromZero);
                });

            var average = perDay.Count == 0 ? 0m : perDay.Values.Average();
            lbLichHen.Text = FormatPercent(average);
            label4.Text = $"T? l? l?p d?y ({FormatRangeLabel(from, to)})";
            RenderSparklineChart(chart2, BuildDailyPoints(from, to, perDay), clampToHundred: true);
        }
        catch (Exception ex)
        {
            lbLichHen.Text = "--";
            MessageBox.Show($"Kh�ng th? t?i l?p d?y (admin): {ex.Message}", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            cobLichHen.Enabled = true;
        }
    }

    private async Task LoadAdminAppointmentTrendAsync(CancellationToken cancellationToken)
    {
        if (_adminAppointmentsApiClient is null)
        {
            return;
        }

        try
        {
            cobXuHuong.Enabled = false;
            lbTrendRange.Text = "Dang t?i...";

            var (from, to) = ResolveDateRange(cobXuHuong.SelectedItem?.ToString());
            var appointments = await _adminAppointmentsApiClient.GetAppointmentsAsync(from, to, cancellationToken);
            var perDay = appointments
                .GroupBy(a => ToLocalDate(a.StartUtc))
                .ToDictionary(group => group.Key, group => (decimal)group.Count());

            var response = new DashboardAppointmentTrendResponse
            {
                RangeLabel = FormatRangeLabel(from, to),
                ConfirmedCount = appointments.Count(a => IsConfirmed(a.Status)),
                CanceledCount = appointments.Count(a => IsCanceled(a.Status)),
                Points = BuildDailyPoints(from, to, perDay)
            };

            lbTrendRange.Text = response.RangeLabel;
            lbConfirmedTrend.Text = response.ConfirmedCount.ToString("N0", VietnamCulture);
            lbCanceledTrend.Text = response.CanceledCount.ToString("N0", VietnamCulture);
            RenderTrendChart(response);
        }
        catch (Exception ex)
        {
            lbTrendRange.Text = "Kh�ng th? t?i d? li?u";
            MessageBox.Show($"Kh�ng th? t?i xu hu?ng (admin): {ex.Message}", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            cobXuHuong.Enabled = true;
        }
    }

    private async Task LoadAdminAppointmentsAsync(CancellationToken cancellationToken)
    {
        if (_adminAppointmentsApiClient is null)
        {
            return;
        }

        try
        {
            var (from, to) = ResolveDateRange(cobCuocHen.SelectedItem?.ToString());
            var appointments = await _adminAppointmentsApiClient.GetAppointmentsAsync(from, to, cancellationToken);
            var filtered = FilterAppointmentsByStatus(appointments);

            label7.Text = $"Cu?c h?n ({FormatRangeLabel(from, to)})";
            BindAppointmentData(filtered);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Kh�ng th? t?i danh s�ch cu?c h?n (admin): {ex.Message}", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private IReadOnlyList<DoctorAppointmentListItemDto> FilterAppointmentsByStatus(IReadOnlyList<DoctorAppointmentListItemDto> appointments)
    {
        var selection = cobTrangThai.SelectedItem?.ToString();
        if (string.IsNullOrWhiteSpace(selection) || !AppointmentStatusMap.TryGetValue(selection, out var statusCode))
        {
            return appointments;
        }

        return appointments
            .Where(a => string.Equals(a.Status, statusCode, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private void BindAppointmentData(IReadOnlyList<DoctorAppointmentListItemDto> appointments)
    {
        var appointmentRows = appointments
            .OrderBy(a => a.StartUtc)
            .Select(a => new AppointmentGridRow(
                a.PatientName,
                a.DoctorName,
                string.IsNullOrWhiteSpace(a.DateLabel) ? a.StartUtc.ToLocalTime().ToString("dd/MM/yyyy", VietnamCulture) : a.DateLabel,
                string.IsNullOrWhiteSpace(a.TimeLabel) ? a.StartUtc.ToLocalTime().ToString("HH:mm", VietnamCulture) : a.TimeLabel,
                string.IsNullOrWhiteSpace(a.StatusLabel) ? a.Status : a.StatusLabel))
            .ToList();

        dgvCuocHen.DataSource = appointmentRows;

        var specialtyRows = appointments
            .GroupBy(a => a.SpecialtyName)
            .Select(group => new SpecialtyPerformanceRow(
                string.IsNullOrWhiteSpace(group.Key) ? "Chưa xác định" : group.Key,
                group.Count(),
                group.Sum(x => x.Price).ToString("N0", VietnamCulture)))
            .ToList();

        dgvChuyenKhoa.DataSource = specialtyRows;

        var doctorRows = appointments
            .GroupBy(a => a.DoctorName)
            .Select(group => new DoctorPerformanceRow(
                string.IsNullOrWhiteSpace(group.Key) ? "Chưa xác định" : group.Key,
                group.Count(a => IsConfirmed(a.Status)),
                group.Count(a => IsCanceled(a.Status))))
            .ToList();

        dgvBacSi.DataSource = doctorRows;
    }

    private async void cobXuHuong_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
        {
            return;
        }

        if (!_useAdminDashboard && _dashboardApiClient is null)
        {
            return;
        }

        await LoadAppointmentTrendAsync();
    }

    private sealed record AppointmentGridRow(string PatientName, string DoctorName, string Date, string Time, string Status);

    private sealed record SpecialtyPerformanceRow(string Specialty, int AppointmentCount, string Revenue);

    private sealed record DoctorPerformanceRow(string Doctor, int Confirmed, int Canceled);

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
    private Label lbTrendRange;
    private Label lbConfirmedTrend;
    private Label lbCanceledTrend;
    private Label labelConfirmedTitle;
    private Label labelCanceledTitle;
    private System.Windows.Forms.DataVisualization.Charting.Chart chartAppointmentTrend;
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
        ChartArea chartArea4 = new ChartArea();
        Legend legend4 = new Legend();
        Series series4 = new Series();
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
        lbTrendRange = new Label();
        lbConfirmedTrend = new Label();
        lbCanceledTrend = new Label();
        labelConfirmedTitle = new Label();
        labelCanceledTitle = new Label();
        chartAppointmentTrend = new Chart();
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
        flowLayoutPanel1.Controls.Add(panel1);
        flowLayoutPanel1.Controls.Add(panelHieuXuat);
        flowLayoutPanel1.SetFlowBreak(panelXuHuong, true);
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
        label4.Text = "Tỷ lệ lấp đầy";
        //
        // panelXuHuong
        //
        panelXuHuong.AutoSize = false;
        panelXuHuong.BorderStyle = BorderStyle.FixedSingle;
        panelXuHuong.Controls.Add(chartAppointmentTrend);
        panelXuHuong.Controls.Add(labelCanceledTitle);
        panelXuHuong.Controls.Add(lbCanceledTrend);
        panelXuHuong.Controls.Add(labelConfirmedTitle);
        panelXuHuong.Controls.Add(lbConfirmedTrend);
        panelXuHuong.Controls.Add(lbTrendRange);
        panelXuHuong.Controls.Add(cobXuHuong);
        panelXuHuong.Controls.Add(label5);
        panelXuHuong.MinimumSize = new Size(960, 250);
        panelXuHuong.Location = new Point(80, 297);
        panelXuHuong.Margin = new Padding(80, 20, 20, 20);
        panelXuHuong.Name = "panelXuHuong";
        panelXuHuong.Size = new Size(1440, 260);
        panelXuHuong.TabIndex = 12;
        //
        // cobXuHuong
        //
        cobXuHuong.DropDownStyle = ComboBoxStyle.DropDownList;
        cobXuHuong.FormattingEnabled = true;
        cobXuHuong.Location = new Point(791, 25);
        cobXuHuong.Name = "cobXuHuong";
        cobXuHuong.Size = new Size(150, 28);
        cobXuHuong.TabIndex = 1;
        cobXuHuong.SelectedIndexChanged += cobXuHuong_SelectedIndexChanged;
        //
        // label5
        //
        label5.AutoSize = true;
        label5.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        label5.Location = new Point(25, 23);
        label5.Name = "label5";
        label5.Size = new Size(173, 28);
        label5.TabIndex = 0;
        label5.Text = "Xu hướng lịch hẹn";
        //
        // lbTrendRange
        //
        lbTrendRange.AutoSize = true;
        lbTrendRange.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        lbTrendRange.ForeColor = Color.FromArgb(107, 114, 128);
        lbTrendRange.Location = new Point(27, 61);
        lbTrendRange.Name = "lbTrendRange";
        lbTrendRange.Size = new Size(73, 20);
        lbTrendRange.TabIndex = 2;
        lbTrendRange.Text = "Tuần này";
        //
        // lbConfirmedTrend
        //
        lbConfirmedTrend.AutoSize = true;
        lbConfirmedTrend.Font = new Font("Segoe UI", 24F, FontStyle.Bold, GraphicsUnit.Point);
        lbConfirmedTrend.ForeColor = Color.FromArgb(16, 185, 129);
        lbConfirmedTrend.Location = new Point(23, 97);
        lbConfirmedTrend.Name = "lbConfirmedTrend";
        lbConfirmedTrend.Size = new Size(46, 54);
        lbConfirmedTrend.TabIndex = 3;
        lbConfirmedTrend.Text = "-";
        //
        // lbCanceledTrend
        //
        lbCanceledTrend.AutoSize = true;
        lbCanceledTrend.Font = new Font("Segoe UI", 24F, FontStyle.Bold, GraphicsUnit.Point);
        lbCanceledTrend.ForeColor = Color.FromArgb(107, 114, 128);
        lbCanceledTrend.Location = new Point(247, 97);
        lbCanceledTrend.Name = "lbCanceledTrend";
        lbCanceledTrend.Size = new Size(46, 54);
        lbCanceledTrend.TabIndex = 4;
        lbCanceledTrend.Text = "-";
        //
        // labelConfirmedTitle
        //
        labelConfirmedTitle.AutoSize = true;
        labelConfirmedTitle.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
        labelConfirmedTitle.Location = new Point(26, 151);
        labelConfirmedTitle.Name = "labelConfirmedTitle";
        labelConfirmedTitle.Size = new Size(130, 25);
        labelConfirmedTitle.TabIndex = 5;
        labelConfirmedTitle.Text = "Đã đặt lịch hẹn";
        //
        // labelCanceledTitle
        //
        labelCanceledTitle.AutoSize = true;
        labelCanceledTitle.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
        labelCanceledTitle.Location = new Point(250, 151);
        labelCanceledTitle.Name = "labelCanceledTitle";
        labelCanceledTitle.Size = new Size(163, 25);
        labelCanceledTitle.TabIndex = 6;
        labelCanceledTitle.Text = "Các cuộc hẹn đã hủy";
        //
        // chartAppointmentTrend
        //
        chartArea4.Name = "ChartArea1";
        chartAppointmentTrend.ChartAreas.Add(chartArea4);
        legend4.Enabled = false;
        legend4.Name = "Legend1";
        chartAppointmentTrend.Legends.Add(legend4);
        chartAppointmentTrend.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        chartAppointmentTrend.Location = new Point(430, 75);
        chartAppointmentTrend.Name = "chartAppointmentTrend";
        series4.ChartArea = "ChartArea1";
        series4.Legend = "Legend1";
        series4.Name = "Series1";
        chartAppointmentTrend.Series.Add(series4);
        chartAppointmentTrend.Size = new Size(980, 190);
        chartAppointmentTrend.TabIndex = 7;
        chartAppointmentTrend.Text = "chart3";
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
}
