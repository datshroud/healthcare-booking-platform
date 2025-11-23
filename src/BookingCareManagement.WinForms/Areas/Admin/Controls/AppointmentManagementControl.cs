using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BookingCareManagement.WinForms.Areas.Admin.Services;
using BookingCareManagement.WinForms.Areas.Customer.Services.Models;
using BookingCareManagement.WinForms.Shared.Models.Dtos;
using BookingCareManagement.WinForms.Shared.Services;

namespace BookingCareManagement.WinForms.Areas.Admin.Controls;

public sealed class AppointmentManagementControl : UserControl
{
    private readonly DialogService _dialogService;
    private readonly AdminAppointmentsApiClient _appointmentsApiClient;
    private readonly CustomerBookingApiClient _bookingApiClient;
    private readonly ContextMenuStrip _rowMenu = new();
    private int _actionColumnIndex = -1;

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

    private AdminAppointmentMetadataDto? _metadata;
    private ComboBox? _doctorFilter;
    private ComboBox? _specialtyFilter;
    private ComboBox? _statusFilter;
    private IReadOnlyList<AppointmentRow> _appointments = Array.Empty<AppointmentRow>();

    public AppointmentManagementControl(DialogService dialogService, AdminAppointmentsApiClient appointmentsApiClient, CustomerBookingApiClient bookingApiClient)
    {
        _dialogService = dialogService;
        _appointmentsApiClient = appointmentsApiClient;
        _bookingApiClient = bookingApiClient;
        Dock = DockStyle.Fill;
        BackColor = Color.FromArgb(243, 244, 246);

        _startDate.Value = DateTime.Today.AddDays(-7);
        _endDate.Value = DateTime.Today.AddDays(7);

        BuildLayout();
        ConfigureGrid();
        WireEvents();
    }

    public Task InitializeAsync()
    {
        return LoadAppointmentsAsync();
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

        _doctorFilter = BuildFilterCombo(filterFlow, "Bác sĩ");
        _specialtyFilter = BuildFilterCombo(filterFlow, "Chuyên khoa");
        _statusFilter = BuildFilterCombo(filterFlow, "Trạng thái");

        _filtersContainer.Controls.Add(filterFlow);
    }

    private ComboBox BuildFilterCombo(FlowLayoutPanel parent, string label)
    {
        var container = new Panel { Width = 220, Height = 64, Margin = new Padding(0, 0, 12, 0) };
        var combo = new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 10),
            BackColor = Color.White
        };

        combo.Items.Add("Đang tải...");
        combo.SelectedIndex = 0;

        var lbl = new Label
        {
            Text = label,
            Dock = DockStyle.Top,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.FromArgb(75, 85, 99)
        };

        container.Controls.Add(combo);
        container.Controls.Add(lbl);
        parent.Controls.Add(container);
        return combo;
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
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "SĐT", DataPropertyName = nameof(AppointmentRow.Phone), FillWeight = 20 });
        var actionColumn = new DataGridViewButtonColumn
        {
            HeaderText = string.Empty,
            Text = "⋯",
            UseColumnTextForButtonValue = true,
            Width = 60,
            FillWeight = 5,
            FlatStyle = FlatStyle.Flat
        };
        _grid.Columns.Add(actionColumn);
        _actionColumnIndex = _grid.Columns.Count - 1;

        _grid.CellFormatting += OnGridCellFormatting;
    }

    private void WireEvents()
    {
        _toggleFiltersButton.Click += (_, _) => _filtersContainer.Visible = !_filtersContainer.Visible;
        _searchBox.TextChanged += (_, _) => ApplyFilters();
        _startDate.ValueChanged += async (_, _) => await LoadAppointmentsAsync();
        _endDate.ValueChanged += async (_, _) => await LoadAppointmentsAsync();
        _exportButton.Click += (_, _) => _dialogService.ShowInfo("Chức năng xuất dữ liệu sẽ sớm có mặt.");
        _newButton.Click += async (_, _) => await ShowUpsertDialogAsync();
        _grid.CellMouseDoubleClick += async (_, args) =>
        {
            if (args.RowIndex >= 0 && args.RowIndex < _grid.Rows.Count)
            {
                await EditSelectedAsync(GetRowByIndex(args.RowIndex));
            }
        };

        _rowMenu.Items.Add("Chỉnh sửa", null, async (_, _) => await EditSelectedAsync(GetSelectedRow()));
        _rowMenu.Items.Add("Xóa", null, async (_, _) => await DeleteSelectedAsync());
        _grid.CellContentClick += OnGridCellContentClickShowMenu;
    }

    private void OnGridCellContentClickShowMenu(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.RowIndex >= _grid.Rows.Count)
        {
            return;
        }

        if (e.ColumnIndex != _actionColumnIndex)
        {
            return;
        }

        _grid.ClearSelection();
        _grid.Rows[e.RowIndex].Selected = true;
        _grid.CurrentCell = _grid.Rows[e.RowIndex].Cells[Math.Max(0, e.ColumnIndex)];

        var cellRect = _grid.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
        var location = _grid.PointToScreen(new Point(cellRect.Left, cellRect.Bottom));
        _rowMenu.Show(location);
    }

    private async Task LoadAppointmentsAsync()
    {
        _loadingPanel.Visible = true;
        _emptyStatePanel.Visible = false;
        _grid.Visible = false;

        try
        {
            _metadata ??= await _appointmentsApiClient.GetMetadataAsync();
            EnsureMetadataFilters();
            var from = DateOnly.FromDateTime(_startDate.Value.Date);
            var to = DateOnly.FromDateTime(_endDate.Value.Date);
            var appointments = await _appointmentsApiClient.GetAppointmentsAsync(from, to);

            _appointments = appointments
                .Select(ToRow)
                .OrderBy(a => a.Start)
                .ToList();

            _bindingSource.DataSource = _appointments;
            ApplyFilters();
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Không tải được dữ liệu cuộc hẹn: {ex.Message}");
            _appointments = Array.Empty<AppointmentRow>();
            _bindingSource.DataSource = _appointments;
            _titleLabel.Text = "Cuộc hẹn (0)";
        }
        finally
        {
            _loadingPanel.Visible = false;
            _grid.Visible = _appointments.Any();
            _emptyStatePanel.Visible = !_appointments.Any();
        }
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
        var selectedDoctor = NormalizeGuidFilter(_doctorFilter?.SelectedValue);
        var selectedSpecialty = NormalizeGuidFilter(_specialtyFilter?.SelectedValue);
        var selectedStatus = NormalizeStringFilter(_statusFilter?.SelectedValue);
        var filtered = _appointments.Where(a =>
            string.IsNullOrWhiteSpace(search) ||
            a.Customer.ToLowerInvariant().Contains(search) ||
            a.Employee.ToLowerInvariant().Contains(search) ||
            a.Service.ToLowerInvariant().Contains(search))
            .Where(a => !selectedDoctor.HasValue || a.DoctorId == selectedDoctor.Value)
            .Where(a => !selectedSpecialty.HasValue || a.SpecialtyId == selectedSpecialty.Value)
            .Where(a => string.IsNullOrWhiteSpace(selectedStatus) || a.StatusCode == selectedStatus)
            .ToList();

        _bindingSource.DataSource = filtered;
        _grid.Visible = filtered.Any();
        _emptyStatePanel.Visible = !filtered.Any();
        _titleLabel.Text = $"Cuộc hẹn ({filtered.Count})";
    }

    private AppointmentRow? GetSelectedRow()
    {
        if (_grid.CurrentRow?.DataBoundItem is AppointmentRow row)
        {
            return row;
        }

        return null;
    }

    private AppointmentRow? GetRowByIndex(int rowIndex)
    {
        if (_grid.Rows[rowIndex].DataBoundItem is AppointmentRow row)
        {
            return row;
        }

        return null;
    }

    private AppointmentRow ToRow(DoctorAppointmentListItemDto dto)
    {
        var startLocal = DateTime.SpecifyKind(dto.StartUtc, DateTimeKind.Utc).ToLocalTime();
        var time = startLocal.ToString("HH:mm dd/MM");
        var duration = dto.DurationMinutes >= 60
            ? $"{dto.DurationMinutes / 60}h {dto.DurationMinutes % 60}p".TrimEnd(' ', 'p')
            : $"{dto.DurationMinutes} phút";

        return new AppointmentRow(
            dto.Id,
            dto.DoctorId,
            dto.SpecialtyId,
            startLocal,
            time,
            dto.SpecialtyName,
            dto.PatientName,
            dto.PatientId,
            duration,
            dto.StatusLabel,
            dto.DoctorName,
            dto.CustomerPhone,
            dto.Status,
            dto.DurationMinutes);
    }

    private async Task ShowUpsertDialogAsync(AppointmentRow? existing = null)
    {
        _metadata ??= await _appointmentsApiClient.GetMetadataAsync();
        using var dialog = new AppointmentUpsertDialog(_metadata, existing, _bookingApiClient);
        if (dialog.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        var payload = dialog.BuiltRequest;
        if (payload == null)
        {
            return;
        }

        try
        {
            if (existing == null)
            {
                await _appointmentsApiClient.CreateAsync(payload);
            }
            else
            {
                await _appointmentsApiClient.UpdateAsync(existing.Id, payload);
            }

            await LoadAppointmentsAsync();
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Lưu cuộc hẹn thất bại: {ex.Message}");
        }
    }

    private DateTime GetMinEditableTime()
    {
        return DateTime.Now.AddHours(2);
    }

    private bool CanEdit(AppointmentRow row)
    {
        var minTime = GetMinEditableTime();
        if (row.Start < minTime)
        {
            _dialogService.ShowInfo($"Cuộc hẹn đã quá hạn chỉnh sửa.\nThời gian tối thiểu: {minTime:HH:mm dd/MM}.");
            return false;
        }

        return true;
    }

    private async Task EditSelectedAsync(AppointmentRow? row)
    {
        if (row == null)
        {
            _dialogService.ShowInfo("Vui lòng chọn cuộc hẹn để chỉnh sửa.");
            return;
        }

        if (!CanEdit(row))
        {
            return;
        }

        await ShowUpsertDialogAsync(row);
    }

    private async Task DeleteSelectedAsync()
    {
        var row = GetSelectedRow();
        if (row == null)
        {
            _dialogService.ShowInfo("Vui lòng chọn cuộc hẹn để xóa.");
            return;
        }

        if (!_dialogService.Confirm("Bạn có chắc chắn muốn xóa cuộc hẹn này?"))
        {
            return;
        }

        try
        {
            var request = new AdminAppointmentStatusRequest { Status = "canceled" };
            await _appointmentsApiClient.UpdateStatusAsync(row.Id, request);
            await LoadAppointmentsAsync();
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Xóa cuộc hẹn thất bại: {ex.Message}");
        }
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

    private static Guid? NormalizeGuidFilter(object? value)
    {
        if (value is Guid guid && guid != Guid.Empty)
        {
            return guid;
        }

        return null;
    }

    private static string? NormalizeStringFilter(object? value)
    {
        var str = value?.ToString();
        return string.IsNullOrWhiteSpace(str) ? null : str;
    }

    private void EnsureMetadataFilters()
    {
        if (_metadata == null || _doctorFilter == null || _specialtyFilter == null || _statusFilter == null)
        {
            return;
        }

        if (_doctorFilter.DataSource == null)
        {
            _doctorFilter.DisplayMember = nameof(AdminAppointmentDoctorOptionDto.Name);
            _doctorFilter.ValueMember = nameof(AdminAppointmentDoctorOptionDto.Id);
            var doctors = new List<AdminAppointmentDoctorOptionDto>
            {
                new() { Id = Guid.Empty, Name = "Tất cả bác sĩ" }
            };
            doctors.AddRange(_metadata.Doctors);
            _doctorFilter.DataSource = doctors;
            _doctorFilter.SelectedIndexChanged += (_, _) => ApplyFilters();
        }

        if (_specialtyFilter.DataSource == null)
        {
            _specialtyFilter.DisplayMember = nameof(AdminAppointmentSpecialtyOptionDto.Name);
            _specialtyFilter.ValueMember = nameof(AdminAppointmentSpecialtyOptionDto.Id);
            var specialties = new List<AdminAppointmentSpecialtyOptionDto>
            {
                new() { Id = Guid.Empty, Name = "Tất cả chuyên khoa" }
            };
            specialties.AddRange(_metadata.Specialties);
            _specialtyFilter.DataSource = specialties;
            _specialtyFilter.SelectedIndexChanged += (_, _) => ApplyFilters();
        }

        if (_statusFilter.DataSource == null)
        {
            _statusFilter.DisplayMember = nameof(AdminAppointmentStatusOptionDto.Label);
            _statusFilter.ValueMember = nameof(AdminAppointmentStatusOptionDto.Code);
            var statuses = new List<AdminAppointmentStatusOptionDto>
            {
                new() { Code = string.Empty, Label = "Tất cả trạng thái" }
            };
            statuses.AddRange(_metadata.Statuses);
            _statusFilter.DataSource = statuses;
            _statusFilter.SelectedIndexChanged += (_, _) => ApplyFilters();
        }
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
        Guid Id,
        Guid DoctorId,
        Guid SpecialtyId,
        DateTime Start,
        string Time,
        string Service,
        string Customer,
        string? PatientId,
        string Duration,
        string Status,
        string Employee,
        string Phone,
        string StatusCode,
        int DurationMinutes);

    private sealed class AppointmentUpsertDialog : Form
    {
        private const int MinAppointmentLeadDays = 2;
        private const int DefaultDurationMinutes = 30;

        private readonly ComboBox _doctorBox = new() { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox _specialtyBox = new() { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox _patientBox = new() { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly DateTimePicker _datePicker = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", ShowCheckBox = true };
        private readonly ComboBox _timeSlotBox = new() { DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };
        private readonly NumericUpDown _durationBox = new() { Minimum = DefaultDurationMinutes, Maximum = DefaultDurationMinutes, Increment = DefaultDurationMinutes, Value = DefaultDurationMinutes, ReadOnly = true, Enabled = false };
        private readonly TextBox _manualPhone = new() { PlaceholderText = "Nhập SĐT nếu hồ sơ thiếu" };
        private readonly ComboBox _statusBox = new() { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly AppointmentRow? _existing;
        private readonly AdminAppointmentMetadataDto _metadata;
        private readonly CustomerBookingApiClient _bookingApiClient;
        private readonly List<AdminAppointmentDoctorOptionDto> _allDoctors;
        private readonly List<DoctorTimeSlotDto> _slotOptions = new();
        private bool _suppressEvents;
        private bool _hasSelectedDate;
        private DateTime? _existingSlotStart;

        public AdminAppointmentUpsertRequest? BuiltRequest { get; private set; }

        public AppointmentUpsertDialog(AdminAppointmentMetadataDto metadata, AppointmentRow? existing, CustomerBookingApiClient bookingApiClient)
        {
            _metadata = metadata;
            _existing = existing;
            _bookingApiClient = bookingApiClient;
            _allDoctors = _metadata.Doctors.ToList();

            Text = existing == null ? "Thêm cuộc hẹn" : "Cập nhật cuộc hẹn";
            Width = 480;
            Height = 420;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            BuildLayout();
            PopulateOptions();
            BindExisting();
        }

        private AdminAppointmentUpsertRequest BuildRequest()
        {
            if (_doctorBox.SelectedValue is not Guid doctorId)
            {
                throw new InvalidOperationException("Vui lòng chọn bác sĩ phụ trách.");
            }

            if (_specialtyBox.SelectedValue is not Guid specialtyId)
            {
                throw new InvalidOperationException("Vui lòng chọn chuyên khoa.");
            }

            var patient = GetSelectedPatient();
            if (patient == null)
            {
                throw new InvalidOperationException("Vui lòng chọn bệnh nhân.");
            }

            var manualPhone = _manualPhone.Text.Trim();
            var resolvedPhone = string.IsNullOrWhiteSpace(patient.PhoneNumber) ? manualPhone : patient.PhoneNumber;
            if (string.IsNullOrWhiteSpace(resolvedPhone))
            {
                throw new InvalidOperationException("Bệnh nhân chưa có số điện thoại, vui lòng nhập để tiếp tục.");
            }

            if (!_hasSelectedDate || !_datePicker.Checked)
            {
                throw new InvalidOperationException("Vui lòng chọn ngày khám.");
            }

            if (_timeSlotBox.SelectedItem is not DoctorTimeSlotDto slot)
            {
                throw new InvalidOperationException("Vui lòng chọn khung giờ khám.");
            }

            var startLocal = slot.StartLocal;
            var minDate = DateTime.Today.AddDays(MinAppointmentLeadDays);
            if (startLocal.Date < minDate)
            {
                throw new InvalidOperationException($"Ngày khám phải cách hiện tại ít nhất {MinAppointmentLeadDays} ngày.");
            }

            BuiltRequest = new AdminAppointmentUpsertRequest
            {
                DoctorId = doctorId,
                SpecialtyId = specialtyId,
                SlotStartUtc = slot.StartUtc != default ? slot.StartUtc : DateTime.SpecifyKind(startLocal, DateTimeKind.Local).ToUniversalTime(),
                DurationMinutes = DefaultDurationMinutes,
                PatientName = patient.Name,
                CustomerPhone = resolvedPhone,
                PatientId = patient.Id,
                Status = _statusBox.SelectedValue?.ToString() ?? "pending"
            };

            return BuiltRequest;
        }

        private void BuildLayout()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 8,
                Padding = new Padding(12),
                AutoSize = true
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));

            AddRow(layout, 0, "Bác sĩ", _doctorBox);
            AddRow(layout, 1, "Chuyên khoa", _specialtyBox);
            AddRow(layout, 2, "Bệnh nhân", _patientBox);
            AddRow(layout, 3, "SĐT", _manualPhone);
            AddRow(layout, 4, "Ngày khám", _datePicker);
            AddRow(layout, 5, "Khung giờ", _timeSlotBox);
            AddRow(layout, 6, "Thời lượng (phút)", _durationBox);

            var nextRow = 7;
            if (_metadata.Statuses.Any())
            {
                AddRow(layout, nextRow, "Trạng thái", _statusBox);
                nextRow++;
            }

            var buttonPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Bottom,
                Padding = new Padding(12)
            };

            var saveButton = new Button { Text = "Lưu", AutoSize = true };
            var cancelButton = new Button { Text = "Hủy", DialogResult = DialogResult.Cancel, AutoSize = true };
            saveButton.Click += (_, _) =>
            {
                try
                {
                    BuiltRequest = BuildRequest();
                    DialogResult = DialogResult.OK;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    DialogResult = DialogResult.None;
                }
            };

            buttonPanel.Controls.Add(saveButton);
            buttonPanel.Controls.Add(cancelButton);

            Controls.Add(layout);
            Controls.Add(buttonPanel);
            AcceptButton = saveButton;
            CancelButton = cancelButton;
        }

        private static void AddRow(TableLayoutPanel panel, int rowIndex, string label, Control control)
        {
            while (panel.RowStyles.Count <= rowIndex)
            {
                panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

            var lbl = new Label
            {
                Text = label,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = true
            };

            control.Dock = DockStyle.Fill;
            panel.Controls.Add(lbl, 0, rowIndex);
            panel.Controls.Add(control, 1, rowIndex);
        }

        private AdminAppointmentDoctorOptionDto? GetSelectedDoctor()
        {
            if (_doctorBox.SelectedItem is AdminAppointmentDoctorOptionDto doc)
            {
                return doc;
            }

            return null;
        }

        private void ApplyDoctorFilter()
        {
            _suppressEvents = true;

            if (_specialtyBox.DataSource == null)
            {
                _specialtyBox.DataSource = _metadata.Specialties.ToList();
            }

            var selectedDoctorId = _doctorBox.SelectedValue as Guid?;
            var selectedSpecialtyId = _specialtyBox.SelectedValue as Guid?;

            var filtered = selectedSpecialtyId.HasValue
                ? _allDoctors.Where(d => d.SpecialtyIds.Contains(selectedSpecialtyId.Value)).ToList()
                : _allDoctors.ToList();

            _doctorBox.DataSource = filtered;

            if (selectedDoctorId.HasValue && filtered.Any(d => d.Id == selectedDoctorId.Value))
            {
                _doctorBox.SelectedValue = selectedDoctorId.Value;
            }
            else
            {
                _doctorBox.SelectedIndex = filtered.Count > 0 ? 0 : -1;
            }

            _suppressEvents = false;
        }

        private void EnsureSpecialtyForDoctor(AdminAppointmentDoctorOptionDto doctor)
        {
            if (_specialtyBox.SelectedValue is Guid current && current != Guid.Empty)
            {
                return;
            }

            var firstSpecialty = doctor.SpecialtyIds.FirstOrDefault();
            if (firstSpecialty != Guid.Empty)
            {
                _suppressEvents = true;
                _specialtyBox.SelectedValue = firstSpecialty;
                _suppressEvents = false;
            }
        }

        private async Task HandleDoctorChangedAsync()
        {
            if (_suppressEvents)
            {
                return;
            }

            var doctor = GetSelectedDoctor();
            if (doctor != null)
            {
                EnsureSpecialtyForDoctor(doctor);
            }

            await RefreshSlotsAsync();
        }

        private async Task HandleDateChangedAsync()
        {
            _hasSelectedDate = _datePicker.Checked;

            if (!_hasSelectedDate)
            {
                _timeSlotBox.Enabled = false;
                _timeSlotBox.DataSource = null;
                _timeSlotBox.Text = "Chọn ngày để xem khung giờ";
                return;
            }

            await RefreshSlotsAsync();
        }

        private async Task RefreshSlotsAsync(DateTime? existingSlot = null)
        {
            if (!_hasSelectedDate || _doctorBox.SelectedValue is not Guid doctorId)
            {
                _timeSlotBox.Enabled = false;
                _timeSlotBox.DataSource = null;
                _timeSlotBox.Text = "Chọn ngày để xem khung giờ";
                return;
            }

            var date = DateOnly.FromDateTime(_datePicker.Value.Date);
            try
            {
                var slots = await _bookingApiClient.GetDoctorSlotsAsync(doctorId, date);
                _slotOptions.Clear();

                if (existingSlot.HasValue)
                {
                    _slotOptions.Add(new DoctorTimeSlotDto
                    {
                        StartLocal = existingSlot.Value,
                        EndLocal = existingSlot.Value.AddMinutes(DefaultDurationMinutes),
                        StartUtc = DateTime.SpecifyKind(existingSlot.Value, DateTimeKind.Local).ToUniversalTime(),
                        EndUtc = DateTime.SpecifyKind(existingSlot.Value.AddMinutes(DefaultDurationMinutes), DateTimeKind.Local).ToUniversalTime(),
                        IsAvailable = true
                    });
                }

                _slotOptions.AddRange(slots.Where(s => s.IsAvailable));

                _timeSlotBox.DataSource = null;
                _timeSlotBox.DataSource = _slotOptions.ToList();

                _timeSlotBox.Enabled = _slotOptions.Any();
                _timeSlotBox.Text = _slotOptions.Any() ? string.Empty : "Không có khung giờ";

                SelectSlot(existingSlot ?? _existingSlotStart);
            }
            catch (Exception ex)
            {
                _timeSlotBox.Enabled = false;
                _timeSlotBox.DataSource = null;
                _timeSlotBox.Text = "Tải khung giờ thất bại";
                MessageBox.Show(this, $"Không tải được khung giờ: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SelectSlot(DateTime? existingStart)
        {
            if (!_slotOptions.Any())
            {
                return;
            }

            if (existingStart.HasValue)
            {
                var match = _slotOptions.FirstOrDefault(s => s.StartLocal == existingStart.Value);
                if (match != null)
                {
                    _timeSlotBox.SelectedItem = match;
                    return;
                }
            }

            _timeSlotBox.SelectedIndex = 0;
        }

        private void PopulateOptions()
        {
            _timeSlotBox.FormattingEnabled = true;
            _timeSlotBox.Format += (_, e) =>
            {
                if (e.ListItem is DoctorTimeSlotDto slot)
                {
                    e.Value = $"{slot.StartLocal:HH:mm} - {slot.EndLocal:HH:mm}";
                }
            };
            _timeSlotBox.Text = "Chọn ngày để xem khung giờ";

            _datePicker.MinDate = DateTime.Today.AddDays(MinAppointmentLeadDays);
            _datePicker.Checked = false;
            _datePicker.ValueChanged += async (_, _) => await HandleDateChangedAsync();

            _doctorBox.DisplayMember = nameof(AdminAppointmentDoctorOptionDto.Name);
            _doctorBox.ValueMember = nameof(AdminAppointmentDoctorOptionDto.Id);
            _doctorBox.SelectedIndexChanged += async (_, _) => await HandleDoctorChangedAsync();

            _specialtyBox.DisplayMember = nameof(AdminAppointmentSpecialtyOptionDto.Name);
            _specialtyBox.ValueMember = nameof(AdminAppointmentSpecialtyOptionDto.Id);
            _specialtyBox.SelectedIndexChanged += (_, _) => ApplyDoctorFilter();

            ApplyDoctorFilter();

            _patientBox.DataSource = _metadata.Patients.ToList();
            _patientBox.DisplayMember = nameof(AdminAppointmentPatientOptionDto.Name);
            _patientBox.ValueMember = nameof(AdminAppointmentPatientOptionDto.Id);
            _patientBox.SelectedIndexChanged += (_, _) => UpdatePhoneForSelectedPatient();

            _statusBox.DataSource = _metadata.Statuses.ToList();
            _statusBox.DisplayMember = nameof(AdminAppointmentStatusOptionDto.Label);
            _statusBox.ValueMember = nameof(AdminAppointmentStatusOptionDto.Code);

            if (_existing == null)
            {
                _suppressEvents = true;
                _specialtyBox.SelectedIndex = -1;
                _doctorBox.SelectedIndex = _doctorBox.Items.Count > 0 ? 0 : -1;
                _suppressEvents = false;
            }
        }

        private void BindExisting()
        {
            if (_existing == null)
            {
                return;
            }

            _doctorBox.SelectedValue = _existing.DoctorId;
            _specialtyBox.SelectedValue = _existing.SpecialtyId;
            _existingSlotStart = _existing.Start;
            _hasSelectedDate = true;
            _datePicker.Checked = true;
            _datePicker.Value = _existing.Start.Date;

            var patientId = _existing.PatientId;
            if (string.IsNullOrWhiteSpace(patientId))
            {
                var matched = _metadata.Patients.FirstOrDefault(p => string.Equals(p.Name, _existing.Customer, StringComparison.OrdinalIgnoreCase));
                patientId = matched?.Id;
            }

            if (!string.IsNullOrWhiteSpace(patientId))
            {
                _patientBox.SelectedValue = patientId;
            }

            _manualPhone.Text = _existing.Phone;
            if (!string.IsNullOrWhiteSpace(_existing.StatusCode))
            {
                _statusBox.SelectedValue = _existing.StatusCode;
            }

            _durationBox.Value = DefaultDurationMinutes;

            _ = RefreshSlotsAsync(_existing.Start);
        }

        private void UpdatePhoneForSelectedPatient()
        {
            var patient = GetSelectedPatient();
            if (patient == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(patient.PhoneNumber))
            {
                _manualPhone.Text = patient.PhoneNumber;
            }
            else if (string.IsNullOrWhiteSpace(_manualPhone.Text))
            {
                _manualPhone.PlaceholderText = "Bệnh nhân chưa có SĐT, vui lòng nhập.";
            }
        }

        private AdminAppointmentPatientOptionDto? GetSelectedPatient()
        {
            if (_patientBox.SelectedItem is AdminAppointmentPatientOptionDto option)
            {
                return option;
            }

            return null;
        }
    }
}
