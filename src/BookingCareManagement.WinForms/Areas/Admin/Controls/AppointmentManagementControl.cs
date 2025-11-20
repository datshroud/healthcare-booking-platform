using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BookingCareManagement.WinForms.Shared.Services;

namespace BookingCareManagement.WinForms.Areas.Admin.Controls;

public sealed class AppointmentManagementControl : UserControl
{
    private readonly DialogService _dialogService;

    private readonly Label _titleLabel = new()
    {
        AutoSize = true,
        Font = new Font("Segoe UI", 20, FontStyle.Bold),
        ForeColor = Color.FromArgb(17, 24, 39),
        Text = "Cuộc hẹn (0)"
    };
    private readonly Button _exportButton = BuildPrimaryButton("↓  Xuất dữ liệu", Color.White, Color.FromArgb(55, 65, 81));
    private readonly Button _newButton = BuildPrimaryButton("+  Thêm cuộc hẹn", Color.FromArgb(37, 99, 235), Color.White, isBold: true);

    private readonly TextBox _searchBox = new()
    {
        PlaceholderText = "Tìm kiếm cuộc hẹn (Chuyên khoa, bệnh nhân, nhân viên...)",
        BorderStyle = BorderStyle.FixedSingle,
        Font = new Font("Segoe UI", 10),
        Dock = DockStyle.Fill
    };

    private readonly DateTimePicker _startDate = new() { Format = DateTimePickerFormat.Short, Width = 140 };
    private readonly DateTimePicker _endDate = new() { Format = DateTimePickerFormat.Short, Width = 140 };
    private readonly Button _toggleFiltersButton = BuildSecondaryButton("🧰 Bộ lọc");

    private readonly Panel _filtersContainer = new() { Dock = DockStyle.Top, Visible = false, Height = 96, BackColor = Color.White };
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToResizeRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, ColumnHeadersHeight = 48, RowTemplate = { Height = 64 }, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false };
    private readonly BindingSource _bindingSource = new();

    private readonly Panel _loadingPanel = new()
    {
        Dock = DockStyle.Fill,
        BackColor = Color.White,
        Visible = true,
        Padding = new Padding(16),
        Controls =
        {
            new Label
            {
                Text = "Đang tải dữ liệu cuộc hẹn...",
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Italic),
                ForeColor = Color.FromArgb(107, 114, 128),
                Dock = DockStyle.Top
            }
        }
    };

    private readonly Panel _emptyStatePanel = new()
    {
        Dock = DockStyle.Fill,
        BackColor = Color.White,
        Visible = false,
        Controls =
        {
            new Label
            {
                Text = "Không có cuộc hẹn nào. Điều chỉnh bộ lọc hoặc phạm vi ngày để tìm kiếm cuộc hẹn phù hợp.",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.FromArgb(107, 114, 128)
            }
        }
    };

    private IReadOnlyList<AppointmentRow> _appointments = Array.Empty<AppointmentRow>();

    public AppointmentManagementControl(DialogService dialogService)
    {
        _dialogService = dialogService;
        Dock = DockStyle.Fill;
        BackColor = Color.FromArgb(243, 244, 246);

        BuildLayout();
        ConfigureGrid();
        WireEvents();
    }

    public Task InitializeAsync()
    {
        return LoadSampleDataAsync();
    }

    private void BuildLayout()
    {
        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 96,
            Padding = new Padding(28, 24, 28, 16),
            BackColor = Color.FromArgb(243, 244, 246)
        };

        var actionPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            WrapContents = false
        };

        actionPanel.Controls.Add(_exportButton);
        actionPanel.Controls.Add(_newButton);

        header.Controls.Add(actionPanel);
        header.Controls.Add(_titleLabel);

        var contentWrapper = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(28, 8, 28, 24),
            BackColor = Color.FromArgb(243, 244, 246)
        };

        var card = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(28, 20, 28, 20),
            BackColor = Color.White,
            BorderStyle = BorderStyle.None
        };

        var searchRow = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            RowCount = 1,
            Height = 72
        };
        searchRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
        searchRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

        var searchContainer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 12, 12, 12) };
        searchContainer.Controls.Add(_searchBox);

        var dateAndFilterContainer = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 12, 0, 12),
            AutoSize = true
        };
        dateAndFilterContainer.Controls.Add(BuildDateRangePanel());
        dateAndFilterContainer.Controls.Add(_toggleFiltersButton);

        searchRow.Controls.Add(searchContainer, 0, 0);
        searchRow.Controls.Add(dateAndFilterContainer, 1, 0);

        BuildFilters();

        card.Controls.Add(_grid);
        card.Controls.Add(_filtersContainer);
        card.Controls.Add(searchRow);
        card.Controls.Add(_loadingPanel);
        card.Controls.Add(_emptyStatePanel);

        contentWrapper.Controls.Add(card);

        Controls.Add(contentWrapper);
        Controls.Add(header);
    }

    private Control BuildDateRangePanel()
    {
        var container = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 6, 0, 0),
            Margin = new Padding(0, 0, 12, 0)
        };

        container.Controls.Add(new Label { Text = "Từ", AutoSize = true, ForeColor = Color.FromArgb(107, 114, 128), Padding = new Padding(0, 10, 4, 0) });
        container.Controls.Add(_startDate);
        container.Controls.Add(new Label { Text = "-", AutoSize = true, ForeColor = Color.FromArgb(107, 114, 128), Padding = new Padding(6, 10, 6, 0) });
        container.Controls.Add(_endDate);
        return container;
    }

    private void BuildFilters()
    {
        var filterFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(0, 12, 0, 12),
            AutoScroll = true
        };

        filterFlow.Controls.Add(BuildFilterCombo("Chuyên khoa", new[] { "Răng hàm mặt", "Nội tổng quát", "Nhi khoa" }));
        filterFlow.Controls.Add(BuildFilterCombo("Khách hàng", new[] { "Lê Ngọc Bảo Chân", "Nguyễn Văn A", "Trần Thu Hà" }));
        filterFlow.Controls.Add(BuildFilterCombo("Nhân viên", new[] { "BS. Ngọc Anh", "BS. Minh Châu", "BS. Minh Quang" }));
        filterFlow.Controls.Add(BuildFilterCombo("Trạng thái", new[] { "Chờ duyệt", "Đã chấp nhận", "Đã hủy", "Từ chối", "Vắng" }));

        _filtersContainer.Controls.Add(filterFlow);
    }

    private Control BuildFilterCombo(string label, IEnumerable<string> options)
    {
        var container = new Panel { Width = 200, Height = 64, Margin = new Padding(0, 0, 12, 0) };
        var combo = new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 10),
            BackColor = Color.White
        };

        combo.Items.AddRange(options.ToArray());
        if (combo.Items.Count > 0)
        {
            combo.SelectedIndex = 0;
        }

        container.Controls.Add(combo);
        var lbl = new Label
        {
            Text = label,
            Dock = DockStyle.Top,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.FromArgb(75, 85, 99)
        };
        container.Controls.Add(lbl);
        return container;
    }

    private void ConfigureGrid()
    {
        _grid.DataSource = _bindingSource;
        _grid.EnableHeadersVisualStyles = false;
        _grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.White,
            ForeColor = Color.FromArgb(107, 114, 128),
            Font = new Font("Segoe UI Semibold", 9.5F),
            Alignment = DataGridViewContentAlignment.MiddleLeft,
            Padding = new Padding(12, 0, 0, 0)
        };
        _grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.White,
            ForeColor = Color.FromArgb(17, 24, 39),
            SelectionBackColor = Color.FromArgb(243, 244, 246),
            SelectionForeColor = Color.FromArgb(17, 24, 39),
            Padding = new Padding(12, 10, 12, 10),
            Font = new Font("Segoe UI", 10)
        };
        _grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(249, 250, 251) };

        _grid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = string.Empty, Width = 50, FillWeight = 5 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Thời gian", DataPropertyName = nameof(AppointmentRow.Time), FillWeight = 15 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Chuyên khoa", DataPropertyName = nameof(AppointmentRow.Service), FillWeight = 20 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Bệnh nhân", DataPropertyName = nameof(AppointmentRow.Customer), FillWeight = 25 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Thời lượng", DataPropertyName = nameof(AppointmentRow.Duration), FillWeight = 10 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Trạng thái", DataPropertyName = nameof(AppointmentRow.Status), FillWeight = 15 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Bác sĩ", DataPropertyName = nameof(AppointmentRow.Employee), FillWeight = 15 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ghi chú", DataPropertyName = nameof(AppointmentRow.Note), FillWeight = 20 });

        _grid.CellFormatting += OnGridCellFormatting;
    }

    private void WireEvents()
    {
        _toggleFiltersButton.Click += (_, _) => _filtersContainer.Visible = !_filtersContainer.Visible;
        _searchBox.TextChanged += (_, _) => ApplyFilters();
        _startDate.ValueChanged += (_, _) => ApplyFilters();
        _endDate.ValueChanged += (_, _) => ApplyFilters();
        _exportButton.Click += (_, _) => _dialogService.ShowInfo("Chức năng xuất dữ liệu sẽ sớm có mặt.");
        _newButton.Click += (_, _) => _dialogService.ShowInfo("Thêm cuộc hẹn mới sẽ sớm có mặt.");
    }

    private async Task LoadSampleDataAsync()
    {
        _loadingPanel.Visible = true;
        _emptyStatePanel.Visible = false;
        _grid.Visible = false;

        await Task.Delay(150); // Simulate load time

        _appointments = new List<AppointmentRow>
        {
            new("4:00 PM", "Kiểm tra tổng quát", "Lê Ngọc Bảo Chân", "60 phút", "Đã chấp nhận", "BS. Minh Quang", string.Empty),
            new("2:00 PM", "Chỉnh nha", "Nguyễn Văn A", "45 phút", "Chờ duyệt", "BS. Minh Châu", "Ưu tiên xếp lịch sớm"),
            new("9:30 AM", "Răng hàm mặt", "Trần Thu Hà", "30 phút", "Đã hủy", "BS. Ngọc Anh", "Khách bận đột xuất")
        };

        _titleLabel.Text = $"Cuộc hẹn ({_appointments.Count})";
        _bindingSource.DataSource = _appointments;

        _loadingPanel.Visible = false;
        _grid.Visible = _appointments.Any();
        _emptyStatePanel.Visible = !_appointments.Any();
    }

    private void ApplyFilters()
    {
        if (!_appointments.Any())
        {
            _bindingSource.DataSource = _appointments;
            _grid.Visible = false;
            _emptyStatePanel.Visible = true;
            return;
        }

        var search = _searchBox.Text.Trim().ToLowerInvariant();
        var filtered = _appointments.Where(a =>
            string.IsNullOrWhiteSpace(search) ||
            a.Customer.ToLowerInvariant().Contains(search) ||
            a.Employee.ToLowerInvariant().Contains(search) ||
            a.Service.ToLowerInvariant().Contains(search)).ToList();

        _bindingSource.DataSource = filtered;
        _grid.Visible = filtered.Any();
        _emptyStatePanel.Visible = !filtered.Any();
        _titleLabel.Text = $"Cuộc hẹn ({filtered.Count})";
    }

    private void OnGridCellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (_grid.Columns[e.ColumnIndex].HeaderText != "Trạng thái")
        {
            return;
        }

        if (e.Value is not string status)
        {
            return;
        }

        var style = _grid.Rows[e.RowIndex].Cells[e.ColumnIndex].Style;
        style.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        style.ForeColor = status switch
        {
            "Đã chấp nhận" => Color.FromArgb(22, 163, 74),
            "Chờ duyệt" => Color.FromArgb(245, 158, 11),
            "Đã hủy" => Color.FromArgb(239, 68, 68),
            "Từ chối" => Color.FromArgb(190, 24, 93),
            "Vắng" => Color.FromArgb(107, 114, 128),
            _ => Color.FromArgb(17, 24, 39)
        };
    }

    private static Button BuildPrimaryButton(string text, Color backColor, Color foreColor, bool isBold = false)
    {
        return new Button
        {
            Text = text,
            AutoSize = true,
            FlatStyle = FlatStyle.Flat,
            FlatAppearance = { BorderSize = 0 },
            BackColor = backColor,
            ForeColor = foreColor,
            Font = new Font("Segoe UI", 10, isBold ? FontStyle.Bold : FontStyle.Regular),
            Padding = new Padding(12, 10, 12, 10),
            Margin = new Padding(8, 0, 0, 0),
            Cursor = Cursors.Hand
        };
    }

    private static Button BuildSecondaryButton(string text)
    {
        return new Button
        {
            Text = text,
            AutoSize = true,
            FlatStyle = FlatStyle.Flat,
            FlatAppearance = { BorderSize = 1, BorderColor = Color.FromArgb(209, 213, 219) },
            BackColor = Color.White,
            ForeColor = Color.FromArgb(55, 65, 81),
            Font = new Font("Segoe UI", 10),
            Padding = new Padding(12, 8, 12, 8),
            Margin = new Padding(12, 0, 0, 0),
            Cursor = Cursors.Hand
        };
    }

    private sealed record AppointmentRow(
        string Time,
        string Service,
        string Customer,
        string Duration,
        string Status,
        string Employee,
        string Note);
}
