using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using BookingCareManagement.WinForms.Areas.Admin.Models;
using BookingCareManagement.WinForms.Areas.Admin.Services;
using BookingCareManagement.WinForms;
using BookingCareManagement.WinForms.Shared.Controls;
using BookingCareManagement.WinForms.Shared.Models.Dtos;
using BookingCareManagement.WinForms.Shared.Services;
using BookingCareManagement.WinForms.Shared.State;

namespace BookingCareManagement.WinForms.Areas.Admin.Controls;

public sealed class DoctorCustomerRelationshipControl : UserControl
{
    private const int PageSize = 10;
    private const string CacheKey = "admin-bs-kh-cache";
    private const int SlideMinWidth = 360;
    private const int SlideMaxWidth = 1600;
    private const int SlideDefaultWidth = 700;

    private readonly AdminAppointmentsApiClient _appointmentsApiClient;
    private readonly AdminDashboardApiClient _dashboardApiClient;
    private readonly AdminDoctorApiClient _doctorApiClient;
    private readonly CustomerService _customerService;
    private readonly DialogService _dialogService;
    private readonly SessionState _sessionState;
    private readonly LocalCacheService _cache;

    private readonly TextBox _txtSearchDoctor = new() { PlaceholderText = "Tìm bác sĩ...", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10F) };
    private readonly TextBox _txtSearchCustomer = new() { PlaceholderText = "Tìm khách hàng...", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10F) };
    private readonly DateTimePicker _dtFrom = new() { Format = DateTimePickerFormat.Short, Width = 130 };
    private readonly DateTimePicker _dtTo = new() { Format = DateTimePickerFormat.Short, Width = 130 };
    private readonly Button _btnRefresh = new() { Text = "Làm mới", AutoSize = true, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White, Padding = new Padding(12, 6, 12, 6) };

    private readonly DataGridView _gridDoctors = new() { Dock = DockStyle.Fill, ReadOnly = true, MultiSelect = false, AutoGenerateColumns = false, AllowUserToAddRows = false };
    private readonly DataGridView _gridCustomers = new() { Dock = DockStyle.Fill, ReadOnly = true, MultiSelect = false, AutoGenerateColumns = false, AllowUserToAddRows = false };
    private readonly DataGridView _gridTopCustomers = new() { Dock = DockStyle.Fill, ReadOnly = true, MultiSelect = false, AutoGenerateColumns = false, AllowUserToAddRows = false };
    private readonly DataGridView _gridRecentAppointments = new() { Dock = DockStyle.Fill, ReadOnly = true, MultiSelect = false, AutoGenerateColumns = false, AllowUserToAddRows = false };
    private readonly DataGridView _gridActivities = new() { Dock = DockStyle.Fill, ReadOnly = true, MultiSelect = false, AutoGenerateColumns = false, AllowUserToAddRows = false };
    private readonly DataGridView _gridUpcoming = new() { Dock = DockStyle.Fill, ReadOnly = true, MultiSelect = false, AutoGenerateColumns = false, AllowUserToAddRows = false };
    private readonly Chart _chartAppointmentsByDay = new();
    private readonly Chart _chartStatusDistribution = new();

    private readonly BindingSource _doctorBinding = new();
    private readonly BindingSource _customerBinding = new();
    private readonly BindingSource _topCustomerBinding = new();
    private readonly BindingSource _recentAppointmentsBinding = new();
    private readonly BindingSource _activitiesBinding = new();
    private readonly BindingSource _upcomingBinding = new();

    private readonly Label _lblTotalDoctors = new() { AutoSize = true, Font = new Font("Segoe UI", 16F, FontStyle.Bold) };
    private readonly Label _lblTotalCustomers = new() { AutoSize = true, Font = new Font("Segoe UI", 16F, FontStyle.Bold) };
    private readonly Label _lblTotalRelations = new() { AutoSize = true, Font = new Font("Segoe UI", 16F, FontStyle.Bold) };
    private readonly Label _lblTotalAppointments = new() { AutoSize = true, Font = new Font("Segoe UI", 16F, FontStyle.Bold) };
    private readonly Label _lblTotalRevenue = new() { AutoSize = true, Font = new Font("Segoe UI", 16F, FontStyle.Bold) };
    private readonly Label _lblCacheNote = new() { AutoSize = true, ForeColor = Color.FromArgb(234, 88, 12), Font = new Font("Segoe UI", 9F) };

    private readonly LoadingOverlay _overlay = new();
    private readonly Panel _slidePanel = new();
    private readonly Panel _slideHost = new();
    private readonly Panel _slideResizer = new();
    private readonly System.Windows.Forms.Timer _slideTimer = new();
    private readonly System.Windows.Forms.Timer _resizeTimer = new();
    private readonly System.Windows.Forms.Timer _retryTimer = new();
    private int _pendingResizeWidth;
    private int _slideTargetWidth;
    private int _slideStartWidth;
    private DateTime _slideStartTime;
    private bool _slideOpening;
    private bool _resizing;
    private int _resizeStartX;
    private int _resizeStartWidth;

    private List<DoctorAppointmentListItemDto> _appointments = new();
    private List<DoctorRelationRow> _doctorRows = new();
    private List<DoctorRelationRow> _filteredDoctorRows = new();
    private List<CustomerRelationRow> _filteredCustomerRows = new();
    private List<CustomerRelationRow> _topCustomers = new();
    private List<RecentAppointmentRow> _recentAppointments = new();
    private List<ActivityRow> _activities = new();
    private List<UpcomingRow> _upcoming = new();
    private int _doctorDisplayCount = PageSize;
    private int _customerDisplayCount = PageSize;
    private int _topCustomerDisplayCount = PageSize;
    private int _recentDisplayCount = PageSize;
    private int _activitiesDisplayCount = PageSize;
    private int _upcomingDisplayCount = PageSize;
    private readonly HashSet<DataGridView> _loadingMore = new();
    private bool _overviewErrorShown;
    private bool _lastLoadFailed;
    private BookingCareManagement.WinForms.Areas.Admin.Services.Models.AdminDashboardOverviewDto? _overview;
    private Guid? _selectedDoctorId;

    public DoctorCustomerRelationshipControl(
        AdminAppointmentsApiClient appointmentsApiClient,
        AdminDashboardApiClient dashboardApiClient,
        AdminDoctorApiClient doctorApiClient,
        CustomerService customerService,
        DialogService dialogService,
        SessionState sessionState,
        LocalCacheService cache)
    {
        _appointmentsApiClient = appointmentsApiClient;
        _dashboardApiClient = dashboardApiClient;
        _doctorApiClient = doctorApiClient;
        _customerService = customerService;
        _dialogService = dialogService;
        _sessionState = sessionState;
        _cache = cache;

        Dock = DockStyle.Fill;
        BackColor = Color.FromArgb(248, 250, 252);

        BuildLayout();
        ConfigureGrids();
        ConfigureCharts();
        ConfigureSlidePanel();
        ConfigureRetry();
        WireEvents();
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        _dtTo.Value = today.AddMonths(1);
        _dtFrom.Value = today.AddDays(-30);
        await RefreshAsync(cancellationToken);
    }

    private void BuildLayout()
    {
        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 84,
            Padding = new Padding(20, 16, 20, 12),
            BackColor = Color.White
        };
        var title = new Label
        {
            Text = "Quan hệ bác sĩ - khách hàng",
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            AutoSize = true,
            ForeColor = Color.FromArgb(15, 23, 42)
        };
        var subtitle = new Label
        {
            Text = "Theo dõi mối quan hệ dựa trên lịch hẹn",
            Font = new Font("Segoe UI", 9.5F),
            AutoSize = true,
            ForeColor = Color.FromArgb(100, 116, 139)
        };
        subtitle.Location = new Point(0, title.Bottom + 6);

        var headerLeft = new Panel { Dock = DockStyle.Left, Width = 520 };
        headerLeft.Controls.Add(title);
        headerLeft.Controls.Add(subtitle);
        headerLeft.Controls.Add(_lblCacheNote);
        _lblCacheNote.Location = new Point(0, subtitle.Bottom + 4);
        _lblCacheNote.Visible = false;

        var headerRight = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 6, 0, 0)
        };
        headerRight.Controls.Add(new Label { Text = "Từ", AutoSize = true, ForeColor = Color.FromArgb(71, 85, 105), Padding = new Padding(0, 6, 4, 0) });
        headerRight.Controls.Add(_dtFrom);
        headerRight.Controls.Add(new Label { Text = "Đến", AutoSize = true, ForeColor = Color.FromArgb(71, 85, 105), Padding = new Padding(12, 6, 4, 0) });
        headerRight.Controls.Add(_dtTo);
        headerRight.Controls.Add(_btnRefresh);

        header.Controls.Add(headerRight);
        header.Controls.Add(headerLeft);

        var statsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 96,
            Padding = new Padding(20, 10, 20, 10),
            BackColor = BackColor,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoScroll = true
        };
        statsPanel.Controls.Add(BuildStatCard("Bác sĩ", _lblTotalDoctors));
        statsPanel.Controls.Add(BuildStatCard("Khách hàng", _lblTotalCustomers));
        statsPanel.Controls.Add(BuildStatCard("Mối quan hệ", _lblTotalRelations));
        statsPanel.Controls.Add(BuildStatCard("Lịch hẹn", _lblTotalAppointments));
        statsPanel.Controls.Add(BuildStatCard("Doanh thu", _lblTotalRevenue));

        var body = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 360,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = BackColor,
            Padding = new Padding(20, 0, 20, 20)
        };
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));

        var leftPane = BuildPane("Danh sách bác sĩ", _txtSearchDoctor, _gridDoctors);
        var rightPane = BuildPane("Khách hàng theo bác sĩ", _txtSearchCustomer, _gridCustomers);

        body.Controls.Add(leftPane, 0, 0);
        body.Controls.Add(rightPane, 1, 0);

        var rowChart = BuildCardPanel("Phân bố lịch hẹn theo ngày", _chartAppointmentsByDay, 320);
        var rowTopCustomers = BuildCardPanel("Khách hàng nổi bật", _gridTopCustomers, 280);
        var rowRecentAppointments = BuildCardPanel("Lịch hẹn gần đây", _gridRecentAppointments, 280);
        var rowActivities = BuildCardPanel("Hoạt động gần đây", _gridActivities, 280);
        var rowStatus = BuildCardPanel("Tỷ lệ trạng thái lịch hẹn", _chartStatusDistribution, 280);
        var rowUpcoming = BuildCardPanel("Cuộc hẹn sắp tới", _gridUpcoming, 280);

        var secondRow = BuildRow(new (Control control, float width)[]
        {
            (rowChart, 0.6f),
            (rowStatus, 0.4f)
        });

        var thirdRow = BuildRow(new (Control control, float width)[]
        {
            (rowTopCustomers, 0.34f),
            (rowRecentAppointments, 0.33f),
            (rowActivities, 0.33f)
        });

        var fourthRow = BuildRow(new (Control control, float width)[]
        {
            (rowUpcoming, 1f)
        });

        var scrollHost = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = BackColor };
        scrollHost.Controls.Add(fourthRow);
        scrollHost.Controls.Add(thirdRow);
        scrollHost.Controls.Add(secondRow);
        scrollHost.Controls.Add(body);
        scrollHost.Controls.Add(statsPanel);
        scrollHost.Controls.Add(header);

        Controls.Add(scrollHost);
        Controls.Add(_slidePanel);
        Controls.Add(_overlay);
        _overlay.BringToFront();
    }

    private void ConfigureSlidePanel()
    {
        _slidePanel.Dock = DockStyle.None;
        _slidePanel.Width = 0;
        _slidePanel.Visible = false;
        _slidePanel.BackColor = Color.White;
        _slidePanel.Padding = new Padding(0);
        _slidePanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;

        _slideResizer.Dock = DockStyle.Left;
        _slideResizer.Width = 6;
        _slideResizer.Cursor = Cursors.VSplit;
        _slideResizer.BackColor = Color.FromArgb(226, 232, 240);

        _slideHost.Dock = DockStyle.Fill;
        _slideHost.BackColor = Color.White;

        _slidePanel.Controls.Add(_slideHost);
        _slidePanel.Controls.Add(_slideResizer);

        _slideTimer.Interval = 16;
        _slideTimer.Tick += (_, _) => AnimateSlide();

        _resizeTimer.Interval = 16;
        _resizeTimer.Tick += (_, _) => ApplyPendingResize();

        EnableDoubleBuffer(_slidePanel);
        EnableDoubleBuffer(_slideHost);
        EnableDoubleBuffer(_slideResizer);

        SizeChanged += (_, _) => UpdateSlideBounds();

        _slideResizer.MouseDown += (_, e) =>
        {
            _resizing = true;
            _resizeStartX = System.Windows.Forms.Cursor.Position.X;
            _resizeStartWidth = _slidePanel.Width;
            _slidePanel.SuspendLayout();
            _slideHost.SuspendLayout();
            _slideHost.Visible = false;
            _resizeTimer.Start();
        };
        _slideResizer.MouseMove += (_, _) =>
        {
            if (!_resizing)
            {
                return;
            }

            var delta = _resizeStartX - System.Windows.Forms.Cursor.Position.X;
            var maxWidth = GetSlideMaxWidth();
            var newWidth = Math.Max(SlideMinWidth, Math.Min(maxWidth, _resizeStartWidth + delta));
            _pendingResizeWidth = newWidth;
        };
        _slideResizer.MouseUp += (_, _) =>
        {
            _resizing = false;
            _resizeTimer.Stop();
            ApplyPendingResize();
            _slideHost.Visible = true;
            _slideHost.ResumeLayout(true);
            _slidePanel.ResumeLayout(true);
        };
    }

    private void ConfigureRetry()
    {
        _retryTimer.Interval = 60000;
        _retryTimer.Tick += async (_, _) =>
        {
            if (_lastLoadFailed)
            {
                await RefreshAsync();
            }
        };
    }

    private static void EnableDoubleBuffer(Control control)
    {
        typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(control, true, null);
    }

    private int GetSlideMaxWidth()
    {
        var maxWidth = Math.Max(SlideMinWidth, Width - 40);
        return Math.Min(SlideMaxWidth, maxWidth);
    }

    private void UpdateSlideBounds()
    {
        if (!_slidePanel.Visible && _slidePanel.Width == 0)
        {
            return;
        }

        var width = Math.Max(0, Math.Min(GetSlideMaxWidth(), _slidePanel.Width));
        _slidePanel.Height = Height;
        _slidePanel.Location = new Point(Math.Max(0, Width - width), 0);
        _slidePanel.BringToFront();
    }

    private static Panel BuildRow((Control control, float width)[] items)
    {
        var row = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = items.Max(i => i.control.Height) + 24,
            ColumnCount = items.Length,
            RowCount = 1,
            BackColor = Color.Transparent,
            Padding = new Padding(20, 0, 20, 20),
            GrowStyle = TableLayoutPanelGrowStyle.FixedSize
        };

        row.RowStyles.Add(new RowStyle(SizeType.Absolute, Math.Max(1, row.Height - row.Padding.Vertical)));

        foreach (var item in items)
        {
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, item.width * 100));
        }

        for (int i = 0; i < items.Length; i++)
        {
            row.Controls.Add(items[i].control, i, 0);
        }

        return row;
    }

    private static Panel BuildCardPanel(string title, Control content, int height)
    {
        var card = new Panel
        {
            Dock = DockStyle.Fill,
            Height = height,
            Margin = new Padding(0, 0, 16, 0),
            BackColor = Color.White,
            Padding = new Padding(16),
            MinimumSize = new Size(220, height)
        };

        var lbl = new Label
        {
            Text = title,
            AutoSize = true,
            Font = new Font("Segoe UI", 11.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(15, 23, 42),
            Dock = DockStyle.Top
        };

        var contentHost = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 8, 0, 0)
        };
        content.MinimumSize = new Size(200, Math.Max(140, height - 70));
        content.Dock = DockStyle.Fill;
        contentHost.Controls.Add(content);

        card.Controls.Add(contentHost);
        card.Controls.Add(lbl);
        return card;
    }

    private static Panel BuildPane(string title, Control searchBox, Control grid)
    {
        var wrapper = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(16) };
        var lblTitle = new Label
        {
            Text = title,
            Font = new Font("Segoe UI", 11.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(15, 23, 42),
            AutoSize = true
        };

        var searchPanel = new Panel { Dock = DockStyle.Top, Height = 38 };
        searchPanel.Controls.Add(searchBox);

        var header = new Panel { Dock = DockStyle.Top, Height = 60 };
        header.Controls.Add(lblTitle);
        header.Controls.Add(searchPanel);
        searchPanel.Location = new Point(0, lblTitle.Bottom + 8);

        wrapper.Controls.Add(grid);
        wrapper.Controls.Add(header);
        return wrapper;
    }

    private static Panel BuildStatCard(string title, Label valueLabel)
    {
        var card = new Panel
        {
            Width = 220,
            Height = 72,
            Margin = new Padding(0, 0, 16, 0),
            BackColor = Color.White
        };
        var lblTitle = new Label
        {
            Text = title,
            AutoSize = true,
            ForeColor = Color.FromArgb(100, 116, 139),
            Location = new Point(16, 10)
        };
        valueLabel.ForeColor = Color.FromArgb(15, 23, 42);
        valueLabel.Location = new Point(16, 30);

        card.Controls.Add(lblTitle);
        card.Controls.Add(valueLabel);
        return card;
    }

    private void ConfigureGrids()
    {
        ConfigureGridBase(_gridDoctors);
        ConfigureGridBase(_gridCustomers);
        ConfigureGridBase(_gridTopCustomers);
        ConfigureGridBase(_gridRecentAppointments);
        ConfigureGridBase(_gridActivities);
        ConfigureGridBase(_gridUpcoming);

        _gridDoctors.DataSource = _doctorBinding;
        _gridDoctors.Columns.Add(BuildLinkColumn("Bác sĩ", nameof(DoctorRelationRow.DoctorName), "DoctorLink"));
        _gridDoctors.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Khách hàng",
            DataPropertyName = nameof(DoctorRelationRow.TotalCustomers),
            Width = 110
        });
        _gridDoctors.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Lịch hẹn",
            DataPropertyName = nameof(DoctorRelationRow.TotalAppointments),
            Width = 90
        });
        _gridDoctors.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Gần nhất",
            DataPropertyName = nameof(DoctorRelationRow.LastAppointment),
            Width = 120
        });

        _gridCustomers.DataSource = _customerBinding;
        _gridCustomers.Columns.Add(BuildLinkColumn("Khách hàng", nameof(CustomerRelationRow.CustomerName), "CustomerLink"));
        _gridCustomers.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "SĐT",
            DataPropertyName = nameof(CustomerRelationRow.Phone),
            Width = 140
        });
        _gridCustomers.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Lịch hẹn",
            DataPropertyName = nameof(CustomerRelationRow.TotalAppointments),
            Width = 90
        });
        _gridCustomers.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Gần nhất",
            DataPropertyName = nameof(CustomerRelationRow.LastAppointment),
            Width = 120
        });

        _gridTopCustomers.DataSource = _topCustomerBinding;
        _gridTopCustomers.Columns.Add(BuildLinkColumn("Khách hàng", nameof(CustomerRelationRow.CustomerName), "TopCustomerLink"));
        _gridTopCustomers.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "SĐT",
            DataPropertyName = nameof(CustomerRelationRow.Phone),
            Width = 140
        });
        _gridTopCustomers.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Lịch hẹn",
            DataPropertyName = nameof(CustomerRelationRow.TotalAppointments),
            Width = 90
        });

        _gridRecentAppointments.DataSource = _recentAppointmentsBinding;
        _gridRecentAppointments.Columns.Add(BuildLinkColumn("Khách hàng", nameof(RecentAppointmentRow.CustomerName), "RecentCustomerLink"));
        _gridRecentAppointments.Columns.Add(BuildLinkColumn("Bác sĩ", nameof(RecentAppointmentRow.DoctorName), "RecentDoctorLink", 160));
        _gridRecentAppointments.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Ngày",
            DataPropertyName = nameof(RecentAppointmentRow.DateLabel),
            Width = 120
        });

        _gridActivities.DataSource = _activitiesBinding;
        _gridActivities.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Thời gian",
            DataPropertyName = nameof(ActivityRow.Time),
            Width = 120
        });
        _gridActivities.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Mô tả",
            DataPropertyName = nameof(ActivityRow.Description),
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });

        _gridUpcoming.DataSource = _upcomingBinding;
        _gridUpcoming.Columns.Add(BuildLinkColumn("Khách hàng", nameof(UpcomingRow.CustomerName), "UpcomingCustomerLink"));
        _gridUpcoming.Columns.Add(BuildLinkColumn("Bác sĩ", nameof(UpcomingRow.DoctorName), "UpcomingDoctorLink", 160));
        _gridUpcoming.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Giờ",
            DataPropertyName = nameof(UpcomingRow.TimeLabel),
            Width = 120
        });
        _gridUpcoming.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Trạng thái",
            DataPropertyName = nameof(UpcomingRow.Status),
            Width = 120
        });

        AttachInfiniteScroll(_gridDoctors, LoadMoreDoctorsAsync);
        AttachInfiniteScroll(_gridCustomers, LoadMoreCustomersAsync);
        AttachInfiniteScroll(_gridTopCustomers, LoadMoreTopCustomersAsync);
        AttachInfiniteScroll(_gridRecentAppointments, LoadMoreRecentAsync);
        AttachInfiniteScroll(_gridActivities, LoadMoreActivitiesAsync);
        AttachInfiniteScroll(_gridUpcoming, LoadMoreUpcomingAsync);

        _gridUpcoming.CellFormatting += OnStatusCellFormatting;
    }

    private void OnStatusCellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (sender is not DataGridView grid)
        {
            return;
        }

        if (grid.Columns[e.ColumnIndex].DataPropertyName != nameof(UpcomingRow.Status))
        {
            return;
        }

        var text = e.Value?.ToString() ?? string.Empty;
        e.CellStyle.ForeColor = GetStatusColor(text);
    }

    private void ConfigureCharts()
    {
        ConfigureChartBase(_chartAppointmentsByDay, SeriesChartType.Column);
        ConfigureChartBase(_chartStatusDistribution, SeriesChartType.Doughnut);
    }

    private static void ConfigureChartBase(Chart chart, SeriesChartType type)
    {
        chart.ChartAreas.Clear();
        chart.Series.Clear();
        chart.Legends.Clear();
        chart.MinimumSize = new Size(240, 200);

        var area = new ChartArea("MainArea");
        area.AxisX.MajorGrid.Enabled = false;
        area.AxisY.MajorGrid.LineColor = Color.FromArgb(226, 232, 240);
        area.AxisX.LabelStyle.Font = new Font("Segoe UI", 8F);
        area.AxisY.LabelStyle.Font = new Font("Segoe UI", 8F);
        area.AxisX.MajorTickMark.Enabled = false;
        area.AxisY.MajorTickMark.Enabled = false;
        chart.ChartAreas.Add(area);

        var series = new Series("Series")
        {
            ChartType = type,
            ChartArea = "MainArea",
            IsVisibleInLegend = false
        };
        chart.Series.Add(series);
    }

    private static void ConfigureGridBase(DataGridView grid)
    {
        grid.BackgroundColor = Color.White;
        grid.BorderStyle = BorderStyle.None;
        grid.RowHeadersVisible = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.ColumnHeadersHeight = 44;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9.5F);
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(71, 85, 105);
        grid.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);
        grid.DefaultCellStyle.ForeColor = Color.FromArgb(30, 41, 59);
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(226, 232, 240);
        grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);
        grid.RowTemplate.Height = 44;
        grid.MinimumSize = new Size(200, grid.ColumnHeadersHeight + grid.RowTemplate.Height * 5 + 8);
    }

    private void WireEvents()
    {
        _btnRefresh.Click += async (_, _) => await RefreshAsync();
        _txtSearchDoctor.TextChanged += (_, _) => ApplyDoctorFilter();
        _txtSearchCustomer.TextChanged += (_, _) => ApplyCustomerFilter();
        _gridDoctors.SelectionChanged += (_, _) => LoadSelectedDoctorCustomers();
        _dtFrom.ValueChanged += async (_, _) => await RefreshAsync();
        _dtTo.ValueChanged += async (_, _) => await RefreshAsync();

        _gridDoctors.CellContentClick += OnGridLinkClick;
        _gridCustomers.CellContentClick += OnGridLinkClick;
        _gridTopCustomers.CellContentClick += OnGridLinkClick;
        _gridRecentAppointments.CellContentClick += OnGridLinkClick;
        _gridUpcoming.CellContentClick += OnGridLinkClick;
    }

    private static DataGridViewLinkColumn BuildLinkColumn(string header, string dataProperty, string name, int width = 0)
    {
        var column = new DataGridViewLinkColumn
        {
            HeaderText = header,
            DataPropertyName = dataProperty,
            Name = name,
            LinkColor = Color.FromArgb(37, 99, 235),
            ActiveLinkColor = Color.FromArgb(37, 99, 235),
            VisitedLinkColor = Color.FromArgb(37, 99, 235),
            LinkBehavior = LinkBehavior.AlwaysUnderline,
            TrackVisitedState = false,
            UseColumnTextForLinkValue = false
        };

        if (width > 0)
        {
            column.Width = width;
        }
        else
        {
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        return column;
    }

    private void OnGridLinkClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0)
        {
            return;
        }

        if (sender is not DataGridView grid)
        {
            return;
        }

        if (grid.Columns[e.ColumnIndex] is not DataGridViewLinkColumn)
        {
            return;
        }

        var item = grid.Rows[e.RowIndex].DataBoundItem;
        switch (item)
        {
            case DoctorRelationRow doctor when grid.Columns[e.ColumnIndex].Name == "DoctorLink":
                OpenDoctorDashboard(doctor.DoctorId, doctor.DoctorName);
                break;
            case CustomerRelationRow customer when grid.Columns[e.ColumnIndex].Name is "CustomerLink" or "TopCustomerLink":
                OpenCustomerDashboard(customer.CustomerKey, customer.CustomerName);
                break;
            case RecentAppointmentRow recent when grid.Columns[e.ColumnIndex].Name == "RecentCustomerLink":
                OpenCustomerDashboard(recent.CustomerKey, recent.CustomerName);
                break;
            case RecentAppointmentRow recent when grid.Columns[e.ColumnIndex].Name == "RecentDoctorLink":
                OpenDoctorDashboard(recent.DoctorId, recent.DoctorName);
                break;
            case UpcomingRow upcoming when grid.Columns[e.ColumnIndex].Name == "UpcomingCustomerLink":
                OpenCustomerDashboard(upcoming.CustomerKey, upcoming.CustomerName);
                break;
            case UpcomingRow upcoming when grid.Columns[e.ColumnIndex].Name == "UpcomingDoctorLink":
                OpenDoctorDashboard(upcoming.DoctorId, upcoming.DoctorName);
                break;
        }
    }

    private void OpenDoctorDashboard(Guid doctorId, string doctorName)
    {
        ShowSlideDashboard(() => new DashboardForm(_appointmentsApiClient, _sessionState, doctorId, null, doctorName, "Bác sĩ", HideSlidePanel));
    }

    private void OpenCustomerDashboard(string customerKey, string customerName)
    {
        ShowSlideDashboard(() => new DashboardForm(_appointmentsApiClient, _sessionState, null, customerKey, customerName, "Bệnh nhân", HideSlidePanel));
    }

    private void ShowSlideDashboard(Func<Form> createForm)
    {
        _slideHost.Controls.Clear();
        var form = createForm();
        form.TopLevel = false;
        form.FormBorderStyle = FormBorderStyle.None;
        form.Dock = DockStyle.Fill;
        _slideHost.Controls.Add(form);
        form.Show();

        _slidePanel.Visible = true;
        _slideHost.Visible = false;
        var target = Math.Max(SlideMinWidth, Math.Min(GetSlideMaxWidth(), (int)Math.Round(Width * 0.8)));
        StartSlideAnimation(true, target);
    }

    private void HideSlidePanel()
    {
        if (!_slidePanel.Visible)
        {
            return;
        }

        _slideHost.Visible = false;
        StartSlideAnimation(false, 0);
    }

    private void StartSlideAnimation(bool opening, int targetWidth)
    {
        _slideOpening = opening;
        _slideStartWidth = _slidePanel.Width;
        _slideTargetWidth = Math.Max(0, Math.Min(GetSlideMaxWidth(), targetWidth));
        _slideStartTime = DateTime.UtcNow;
        _slideTimer.Start();
    }

    private void AnimateSlide()
    {
        const double durationMs = 260;
        var elapsed = (DateTime.UtcNow - _slideStartTime).TotalMilliseconds;
        var t = Math.Min(1.0, elapsed / durationMs);
        var eased = 1 - Math.Pow(1 - t, 3);
        var width = (int)Math.Round(_slideStartWidth + (_slideTargetWidth - _slideStartWidth) * eased);

        _slidePanel.Width = Math.Max(0, width);
        UpdateSlideBounds();
        if (t >= 1.0)
        {
            _slideTimer.Stop();
            if (!_slideOpening && _slideTargetWidth == 0)
            {
                _slidePanel.Visible = false;
                _slideHost.Controls.Clear();
            }
            else if (_slideOpening)
            {
                _slideHost.Visible = true;
            }
        }
    }

    private void ApplyPendingResize()
    {
        if (!_resizing || _pendingResizeWidth <= 0)
        {
            return;
        }

        _slidePanel.Width = _pendingResizeWidth;
        _slideTargetWidth = _pendingResizeWidth;
        UpdateSlideBounds();
    }

    private async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            SetBusy(true);
            var from = DateOnly.FromDateTime(_dtFrom.Value);
            var to = DateOnly.FromDateTime(_dtTo.Value);
            var appointmentsFailed = false;
            var usingCache = false;
            var cache = _cache.Load<DoctorCustomerCacheSnapshot>(CacheKey);
            try
            {
                _appointments = (await _appointmentsApiClient.GetAppointmentsAsync(from, to, cancellationToken)).ToList();
            }
            catch (Exception ex)
            {
                _appointments = new List<DoctorAppointmentListItemDto>();
                appointmentsFailed = true;
                if (cache?.Appointments?.Count > 0)
                {
                    _appointments = cache.Appointments;
                    usingCache = true;
                }
                else
                {
                    _dialogService.ShowError($"Không thể tải lịch hẹn: {ex.Message}");
                }
            }

            try
            {
                _overview = await _dashboardApiClient.GetOverviewAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _overview = null;
                if (!_overviewErrorShown && appointmentsFailed)
                {
                    _overviewErrorShown = true;
                    if (cache?.Overview is not null)
                    {
                        _overview = cache.Overview;
                        usingCache = true;
                    }
                    else
                    {
                        _dialogService.ShowError($"Không thể tải tổng quan: {ex.Message}");
                    }
                }
            }

            var doctors = new List<DoctorDto>();
            var customers = new List<CustomerDto>();
            var doctorCount = 0;
            var customerCount = 0;

            try
            {
                doctors = (await _doctorApiClient.GetAllAsync(cancellationToken)).ToList();
            }
            catch (Exception ex)
            {
                if (cache is not null)
                {
                    doctorCount = cache.DoctorCount;
                    usingCache = true;
                }
                else
                {
                    _dialogService.ShowError($"Không thể tải bác sĩ: {ex.Message}");
                }
            }

            try
            {
                customers = (await _customerService.GetAllAsync(cancellationToken)).ToList();
            }
            catch (Exception ex)
            {
                if (cache is not null)
                {
                    customerCount = cache.CustomerCount;
                    usingCache = true;
                }
                else
                {
                    _dialogService.ShowError($"Không thể tải khách hàng: {ex.Message}");
                }
            }

            BuildDoctorRows();
            ApplyDoctorFilter();
            BuildTopCustomers();
            BuildRecentAppointments();
            BuildActivities();
            BuildUpcoming();
            BuildCharts();
            doctorCount = doctors.Count > 0 ? doctors.Count : doctorCount;
            customerCount = customers.Count > 0 ? customers.Count : customerCount;
            UpdateStats(doctorCount, customerCount);

            if (!appointmentsFailed)
            {
                var snapshot = new DoctorCustomerCacheSnapshot(
                    from,
                    to,
                    DateTime.UtcNow,
                    _appointments.ToList(),
                    _overview,
                    doctorCount,
                    customerCount);
                _cache.Save(CacheKey, snapshot);
            }

            if (usingCache && cache is not null)
            {
                _lblCacheNote.Text = $"Đang hiển thị dữ liệu offline (cập nhật {cache.UpdatedAtUtc.ToLocalTime():dd/MM/yyyy HH:mm})";
                _lblCacheNote.Visible = true;
            }
            else
            {
                _lblCacheNote.Visible = false;
            }

            _lastLoadFailed = appointmentsFailed;
            if (_lastLoadFailed)
            {
                _retryTimer.Start();
            }
            else
            {
                _retryTimer.Stop();
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Không thể tải mối quan hệ: {ex.Message}");
            _lastLoadFailed = true;
            _retryTimer.Start();
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void BuildDoctorRows()
    {
        _doctorRows = _appointments
            .GroupBy(a => new { a.DoctorId, a.DoctorName })
            .Select(group =>
            {
                var distinctCustomers = group.Select(BuildCustomerKey).Distinct().Count();
                var last = group.Max(a => a.StartUtc);
                return new DoctorRelationRow
                {
                    DoctorId = group.Key.DoctorId,
                    DoctorName = string.IsNullOrWhiteSpace(group.Key.DoctorName) ? "(Chưa có tên)" : group.Key.DoctorName,
                    TotalCustomers = distinctCustomers,
                    TotalAppointments = group.Count(),
                    LastAppointment = last.ToString("dd/MM/yyyy")
                };
            })
            .OrderByDescending(r => r.TotalCustomers)
            .ThenBy(r => r.DoctorName, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    private void ApplyDoctorFilter()
    {
        var keyword = (_txtSearchDoctor.Text ?? string.Empty).Trim();
        IEnumerable<DoctorRelationRow> source = _doctorRows;
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            source = source.Where(r => r.DoctorName.Contains(keyword, StringComparison.CurrentCultureIgnoreCase));
        }

        _filteredDoctorRows = source.ToList();
        _doctorDisplayCount = PageSize;
        BindPaged(_doctorBinding, _filteredDoctorRows, _doctorDisplayCount);
        if (_filteredDoctorRows.Count > 0)
        {
            _gridDoctors.ClearSelection();
            _gridDoctors.Rows[0].Selected = true;
        }
        else
        {
            _customerBinding.DataSource = Array.Empty<CustomerRelationRow>();
        }
    }

    private void LoadSelectedDoctorCustomers()
    {
        if (_gridDoctors.CurrentRow?.DataBoundItem is DoctorRelationRow selected)
        {
            _selectedDoctorId = selected.DoctorId;
            ApplyCustomerFilter();
        }
        else
        {
            _selectedDoctorId = null;
            _customerBinding.DataSource = Array.Empty<CustomerRelationRow>();
        }
    }

    private void ApplyCustomerFilter()
    {
        if (_selectedDoctorId is null)
        {
            _customerBinding.DataSource = Array.Empty<CustomerRelationRow>();
            return;
        }

        var keyword = (_txtSearchCustomer.Text ?? string.Empty).Trim();
        IEnumerable<CustomerRelationRow> items = _appointments
            .Where(a => a.DoctorId == _selectedDoctorId)
            .GroupBy(BuildCustomerKey)
            .Select(group =>
            {
                var sample = group.First();
                var last = group.Max(a => a.StartUtc);
                return new CustomerRelationRow
                {
                    CustomerKey = group.Key,
                    CustomerName = string.IsNullOrWhiteSpace(sample.PatientName) ? "(Chưa có tên)" : sample.PatientName,
                    Phone = sample.CustomerPhone,
                    TotalAppointments = group.Count(),
                    LastAppointment = last.ToString("dd/MM/yyyy")
                };
            })
            .OrderByDescending(r => r.TotalAppointments)
            .ThenBy(r => r.CustomerName, StringComparer.CurrentCultureIgnoreCase);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            items = items.Where(r => r.CustomerName.Contains(keyword, StringComparison.CurrentCultureIgnoreCase)
                                     || (!string.IsNullOrWhiteSpace(r.Phone) && r.Phone.Contains(keyword, StringComparison.CurrentCultureIgnoreCase)));
        }

        _filteredCustomerRows = items.ToList();
        _customerDisplayCount = PageSize;
        BindPaged(_customerBinding, _filteredCustomerRows, _customerDisplayCount);
    }

    private void UpdateStats(int totalDoctors, int totalCustomers)
    {
        var totalRelations = _appointments
            .GroupBy(a => new { a.DoctorId, Customer = BuildCustomerKey(a) })
            .Count();
        var totalAppointments = _appointments.Count;
        var totalRevenue = _appointments.Sum(a => a.Price);

        _lblTotalDoctors.Text = totalDoctors.ToString("N0");
        _lblTotalCustomers.Text = totalCustomers.ToString("N0");
        _lblTotalRelations.Text = totalRelations.ToString("N0");
        _lblTotalAppointments.Text = totalAppointments.ToString("N0");
        _lblTotalRevenue.Text = $"{totalRevenue:N0} ₫";
    }

    private void BuildTopCustomers()
    {
        _topCustomers = _appointments
            .GroupBy(BuildCustomerKey)
            .Select(group =>
            {
                var sample = group.First();
                return new CustomerRelationRow
                {
                    CustomerKey = group.Key,
                    CustomerName = string.IsNullOrWhiteSpace(sample.PatientName) ? "(Chưa có tên)" : sample.PatientName,
                    Phone = sample.CustomerPhone,
                    TotalAppointments = group.Count(),
                    LastAppointment = group.Max(a => a.StartUtc).ToString("dd/MM/yyyy")
                };
            })
            .OrderByDescending(r => r.TotalAppointments)
            .ThenBy(r => r.CustomerName, StringComparer.CurrentCultureIgnoreCase)
            .Take(10)
            .ToList();

        _topCustomerDisplayCount = PageSize;
        BindPaged(_topCustomerBinding, _topCustomers, _topCustomerDisplayCount);
    }

    private void BuildRecentAppointments()
    {
        _recentAppointments = _appointments
            .OrderByDescending(a => a.StartUtc)
            .Take(10)
            .Select(a => new RecentAppointmentRow
            {
                CustomerName = string.IsNullOrWhiteSpace(a.PatientName) ? "(Chưa có tên)" : a.PatientName,
                DoctorName = a.DoctorName,
                DateLabel = a.StartUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm"),
                DoctorId = a.DoctorId,
                CustomerKey = BuildCustomerKey(a)
            })
            .ToList();

        _recentDisplayCount = PageSize;
        BindPaged(_recentAppointmentsBinding, _recentAppointments, _recentDisplayCount);
    }

    private void BuildActivities()
    {
        _activities = _overview?.Activities
            .Select(a => new ActivityRow { Time = a.Time, Description = a.Description })
            .ToList() ?? new List<ActivityRow>();

        if (_activities.Count == 0 && _appointments.Count > 0)
        {
            _activities = _appointments
                .OrderByDescending(a => a.StartUtc)
                .Take(10)
                .Select(a => new ActivityRow
                {
                    Time = a.StartUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm"),
                    Description = $"{a.PatientName} đặt lịch với {a.DoctorName}"
                })
                .ToList();
        }

        _activitiesDisplayCount = PageSize;
        BindPaged(_activitiesBinding, _activities, _activitiesDisplayCount);
    }

    private void BuildUpcoming()
    {
        _upcoming = _overview?.UpcomingAppointments
            .Select(a => new UpcomingRow
            {
                CustomerName = a.CustomerName,
                DoctorName = a.DoctorName,
                TimeLabel = a.TimeLabel,
                Status = a.Status
            })
            .ToList() ?? new List<UpcomingRow>();

        if (_upcoming.Count == 0 && _appointments.Count > 0)
        {
            var now = DateTime.Now;
            _upcoming = _appointments
                .Where(a => a.StartUtc.ToLocalTime() >= now)
                .OrderBy(a => a.StartUtc)
                .Take(10)
                .Select(a => new UpcomingRow
                {
                    CustomerName = a.PatientName,
                    DoctorName = a.DoctorName,
                    TimeLabel = a.StartUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm"),
                    Status = string.IsNullOrWhiteSpace(a.StatusLabel) ? a.Status : a.StatusLabel,
                    DoctorId = a.DoctorId,
                    CustomerKey = BuildCustomerKey(a)
                })
                .ToList();
        }

        _upcomingDisplayCount = PageSize;
        BindPaged(_upcomingBinding, _upcoming, _upcomingDisplayCount);
    }

    private static void BindPaged<T>(BindingSource source, List<T> allItems, int count)
    {
        source.DataSource = allItems.Take(Math.Min(count, allItems.Count)).ToList();
    }

    private static void AttachInfiniteScroll(DataGridView grid, Func<Task> loadMoreAsync)
    {
        grid.Scroll += async (_, _) => await loadMoreAsync();
        grid.MouseWheel += async (_, _) => await loadMoreAsync();
    }

    private bool CanLoadMore(DataGridView grid, int currentCount, int totalCount)
    {
        if (totalCount <= currentCount)
        {
            return false;
        }

        if (_loadingMore.Contains(grid))
        {
            return false;
        }

        if (grid.RowCount == 0 || grid.FirstDisplayedScrollingRowIndex < 0)
        {
            return false;
        }

        var displayed = grid.DisplayedRowCount(false);
        return grid.FirstDisplayedScrollingRowIndex + displayed >= grid.RowCount;
    }

    private async Task LoadMoreDoctorsAsync()
    {
        if (!CanLoadMore(_gridDoctors, _doctorDisplayCount, _filteredDoctorRows.Count))
        {
            return;
        }

        _loadingMore.Add(_gridDoctors);
        try
        {
            await Task.Delay(1200);
            _doctorDisplayCount = Math.Min(_doctorDisplayCount + PageSize, _filteredDoctorRows.Count);
            BindPaged(_doctorBinding, _filteredDoctorRows, _doctorDisplayCount);
        }
        finally
        {
            _loadingMore.Remove(_gridDoctors);
        }
    }

    private async Task LoadMoreCustomersAsync()
    {
        if (!CanLoadMore(_gridCustomers, _customerDisplayCount, _filteredCustomerRows.Count))
        {
            return;
        }

        _loadingMore.Add(_gridCustomers);
        try
        {
            await Task.Delay(1200);
            _customerDisplayCount = Math.Min(_customerDisplayCount + PageSize, _filteredCustomerRows.Count);
            BindPaged(_customerBinding, _filteredCustomerRows, _customerDisplayCount);
        }
        finally
        {
            _loadingMore.Remove(_gridCustomers);
        }
    }

    private async Task LoadMoreTopCustomersAsync()
    {
        if (!CanLoadMore(_gridTopCustomers, _topCustomerDisplayCount, _topCustomers.Count))
        {
            return;
        }

        _loadingMore.Add(_gridTopCustomers);
        try
        {
            await Task.Delay(1200);
            _topCustomerDisplayCount = Math.Min(_topCustomerDisplayCount + PageSize, _topCustomers.Count);
            BindPaged(_topCustomerBinding, _topCustomers, _topCustomerDisplayCount);
        }
        finally
        {
            _loadingMore.Remove(_gridTopCustomers);
        }
    }

    private async Task LoadMoreRecentAsync()
    {
        if (!CanLoadMore(_gridRecentAppointments, _recentDisplayCount, _recentAppointments.Count))
        {
            return;
        }

        _loadingMore.Add(_gridRecentAppointments);
        try
        {
            await Task.Delay(1200);
            _recentDisplayCount = Math.Min(_recentDisplayCount + PageSize, _recentAppointments.Count);
            BindPaged(_recentAppointmentsBinding, _recentAppointments, _recentDisplayCount);
        }
        finally
        {
            _loadingMore.Remove(_gridRecentAppointments);
        }
    }

    private async Task LoadMoreActivitiesAsync()
    {
        if (!CanLoadMore(_gridActivities, _activitiesDisplayCount, _activities.Count))
        {
            return;
        }

        _loadingMore.Add(_gridActivities);
        try
        {
            await Task.Delay(1200);
            _activitiesDisplayCount = Math.Min(_activitiesDisplayCount + PageSize, _activities.Count);
            BindPaged(_activitiesBinding, _activities, _activitiesDisplayCount);
        }
        finally
        {
            _loadingMore.Remove(_gridActivities);
        }
    }

    private async Task LoadMoreUpcomingAsync()
    {
        if (!CanLoadMore(_gridUpcoming, _upcomingDisplayCount, _upcoming.Count))
        {
            return;
        }

        _loadingMore.Add(_gridUpcoming);
        try
        {
            await Task.Delay(1200);
            _upcomingDisplayCount = Math.Min(_upcomingDisplayCount + PageSize, _upcoming.Count);
            BindPaged(_upcomingBinding, _upcoming, _upcomingDisplayCount);
        }
        finally
        {
            _loadingMore.Remove(_gridUpcoming);
        }
    }

    private void BuildCharts()
    {
        var byDay = _appointments
            .GroupBy(a => a.StartUtc.ToLocalTime().Date)
            .OrderBy(g => g.Key)
            .Select(g => new { Day = g.Key, Count = g.Count() })
            .ToList();

        var series = _chartAppointmentsByDay.Series[0];
        series.Points.Clear();
        series.Color = Color.FromArgb(37, 99, 235);
        foreach (var item in byDay)
        {
            var index = series.Points.AddXY(item.Day.ToString("dd/MM"), item.Count);
            series.Points[index].ToolTip = $"Ngày: {item.Day:dd/MM/yyyy}\nLịch hẹn: {item.Count:N0}";
        }

        var statusGroups = _appointments
            .GroupBy(a => string.IsNullOrWhiteSpace(a.StatusLabel) ? a.Status : a.StatusLabel)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToList();

        var statusSeries = _chartStatusDistribution.Series[0];
        statusSeries.Points.Clear();
        statusSeries.IsValueShownAsLabel = true;
        statusSeries.LabelForeColor = Color.FromArgb(15, 23, 42);
        foreach (var item in statusGroups)
        {
            var point = statusSeries.Points[statusSeries.Points.AddXY(item.Status, item.Count)];
            point.Label = item.Count.ToString("N0");
            point.ToolTip = $"Trạng thái: {item.Status}\nSố lượng: {item.Count:N0}";
            point.Color = GetStatusColor(item.Status);
        }
    }

    private static Color GetStatusColor(string status)
    {
        if (status.Contains("Đã xác nhận", StringComparison.CurrentCultureIgnoreCase)
            || status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
        {
            return Color.FromArgb(34, 197, 94); // green
        }

        if (status.Contains("Chờ xác nhận", StringComparison.CurrentCultureIgnoreCase)
            || status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
        {
            return Color.FromArgb(234, 179, 8); // yellow
        }

        if (status.Contains("Đã hủy", StringComparison.CurrentCultureIgnoreCase)
            || status.Equals("Canceled", StringComparison.OrdinalIgnoreCase))
        {
            return Color.FromArgb(239, 68, 68); // red
        }

        if (status.Contains("Vắng mặt", StringComparison.CurrentCultureIgnoreCase)
            || status.Equals("NoShow", StringComparison.OrdinalIgnoreCase)
            || status.Equals("No-Show", StringComparison.OrdinalIgnoreCase))
        {
            return Color.FromArgb(107, 114, 128); // gray
        }

        return Color.FromArgb(59, 130, 246); // fallback blue
    }

    private static string BuildCustomerKey(DoctorAppointmentListItemDto appointment)
    {
        if (!string.IsNullOrWhiteSpace(appointment.PatientId))
        {
            return appointment.PatientId;
        }

        if (!string.IsNullOrWhiteSpace(appointment.CustomerPhone))
        {
            return appointment.CustomerPhone;
        }

        return $"{appointment.PatientName}".Trim();
    }

    private void SetBusy(bool isBusy)
    {
        _overlay.Visible = isBusy;
        _overlay.BringToFront();
    }

    private sealed class DoctorRelationRow
    {
        public Guid DoctorId { get; init; }
        public string DoctorName { get; init; } = string.Empty;
        public int TotalCustomers { get; init; }
        public int TotalAppointments { get; init; }
        public string LastAppointment { get; init; } = string.Empty;
    }

    private sealed class CustomerRelationRow
    {
        public string CustomerKey { get; init; } = string.Empty;
        public string CustomerName { get; init; } = string.Empty;
        public string Phone { get; init; } = string.Empty;
        public int TotalAppointments { get; init; }
        public string LastAppointment { get; init; } = string.Empty;
    }

    private sealed class RecentAppointmentRow
    {
        public string CustomerName { get; init; } = string.Empty;
        public string DoctorName { get; init; } = string.Empty;
        public string DateLabel { get; init; } = string.Empty;
        public Guid DoctorId { get; init; }
        public string CustomerKey { get; init; } = string.Empty;
    }

    private sealed class ActivityRow
    {
        public string Time { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
    }

    private sealed class UpcomingRow
    {
        public string CustomerName { get; init; } = string.Empty;
        public string DoctorName { get; init; } = string.Empty;
        public string TimeLabel { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public Guid DoctorId { get; init; }
        public string CustomerKey { get; init; } = string.Empty;
    }
}
