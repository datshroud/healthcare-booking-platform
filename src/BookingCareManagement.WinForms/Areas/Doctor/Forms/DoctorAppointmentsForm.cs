using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BookingCareManagement.WinForms.Areas.Admin.Services;
using BookingCareManagement.WinForms.Areas.Doctor.Services;
using BookingCareManagement.WinForms.Areas.Customer.Services.Models;
using BookingCareManagement.WinForms.Shared.Forms;
using BookingCareManagement.WinForms.Shared.Models.Dtos;
using BookingCareManagement.WinForms.Shared.Services;

namespace BookingCareManagement.WinForms.Areas.Doctor.Forms;

public sealed partial class DoctorAppointmentsForm : Form
{
    private readonly DoctorAppointmentsApiClient _appointmentsApiClient;
    private readonly DialogService _dialogService;
    private readonly CustomerBookingApiClient _bookingApiClient;
    private readonly CustomerService _customerService;

    private readonly List<CheckedListBox> _filterDropdowns = new();
    private readonly Dictionary<CheckedListBox, List<string>> _filterOptions = new();
    private readonly Dictionary<CheckedListBox, Button> _dropdownButtons = new();
    private readonly Dictionary<CheckedListBox, Func<AppointmentRow, string>> _filterSelectors = new();

    // UI
    private readonly Label lblTitle = new() { AutoSize = true };
    private readonly Label lblSearchIcon = new() { AutoSize = true };
    private readonly Button btnNew = new() { Text = "Tạo mới" };
    private readonly Button btnExport = new() { Text = "Xuất" };
    private readonly Button btnFilter = new() { Text = "Bộ lọc" };
    private readonly Button btnServiceFilter = new() { Text = "Dịch vụ" };
    private readonly Button btnCustomerFilter = new() { Text = "Bệnh nhân" };
    private readonly Button btnEmployeeFilter = new() { Text = "Bác sĩ" };
    private readonly Button btnStatusFilter = new() { Text = "Trạng thái" };
    private readonly TextBox txtSearch = new() { Width = 280 };
    private readonly DateTimePicker dtFrom = new() { Format = DateTimePickerFormat.Short, ShowCheckBox = true };
    private readonly DateTimePicker dtTo = new() { Format = DateTimePickerFormat.Short, ShowCheckBox = true };
    private readonly DataGridView appointmentGrid = new() { Dock = DockStyle.Fill, AllowUserToAddRows = false, RowHeadersVisible = false };
    private readonly Panel filterContainerPanel = new() { BackColor = Color.White, Height = 100, Dock = DockStyle.Top, Visible = false };
    private readonly Panel emptyStatePanel = new() { Dock = DockStyle.Fill, Visible = false };
    private ContextMenuStrip? actionMenu;

    private readonly Dictionary<string, (Color Background, Color Foreground)> _statusStyles =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["approved"] = (Color.FromArgb(220, 252, 231), Color.FromArgb(22, 101, 52)),
            ["pending"] = (Color.FromArgb(239, 246, 255), Color.FromArgb(37, 99, 235)),
            ["canceled"] = (Color.FromArgb(254, 242, 242), Color.FromArgb(185, 28, 28)),
            ["rejected"] = (Color.FromArgb(255, 247, 237), Color.FromArgb(180, 83, 9)),
            ["noshow"] = (Color.FromArgb(248, 250, 252), Color.FromArgb(107, 114, 128)),
        };

    private CheckedListBox? _serviceDropdown;
    private CheckedListBox? _customerDropdown;
    private CheckedListBox? _employeeDropdown;
    private CheckedListBox? _statusDropdown;

    private DoctorAppointmentMetadataDto? _metadata;
    private readonly List<AppointmentRow> _appointments = new();
    private List<AppointmentRow> _filteredAppointments = new();

    // Pagination state
    private int _currentPage = 1;
    private int _pageSize = 10;
    private int _totalPages = 1;

    // Pagination controls
    private Button? _btnPrevPage;
    private Button? _btnNextPage;
    private Label? _lblPageInfo;
    private ComboBox? _cbPageSize;

    private sealed record AppointmentRow(
        Guid Id,
        Guid DoctorId,
        Guid SpecialtyId,
        DateTime Start,
        DateTime StartUtc,
        string Specialty,
        string Patient,
        string? PatientId,
        int DurationMinutes,
        string StatusCode,
        string StatusLabel,
        string Doctor,
        string Room,
        string Phone,
        Guid ClinicRoomId);

    public DoctorAppointmentsForm(DialogService dialogService, DoctorAppointmentsApiClient appointmentsApiClient, CustomerBookingApiClient bookingApiClient, CustomerService customerService)
    {
        _dialogService = dialogService;
        _appointmentsApiClient = appointmentsApiClient;
        _bookingApiClient = bookingApiClient;
        _customerService = customerService;

        InitializeComponentManual();
        ConfigureInputs();
        InitializeFilterDropdowns();
        ConfigureActions();
        InitializePaginationControls();

        Shown += async (_, _) => await LoadAppointmentsAsync();
    }

    private void InitializeComponentManual()
    {
        // Form basic
        Text = "Lịch Hẹn";
        Width = 1600;
        Height = 1055;
        StartPosition = FormStartPosition.CenterParent;
        Font = new Font("Segoe UI", 9F);
        BackColor = Color.FromArgb(243, 244, 246);

        // Header panel
        var headerPanel = new Panel
        {
            BackColor = Color.FromArgb(243, 244, 246),
            Dock = DockStyle.Top,
            Padding = new Padding(34, 27, 34, 27),
            Height = 107
        };

        lblTitle.Text = "Lịch Hẹn (0)";
        lblTitle.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
        lblTitle.ForeColor = Color.FromArgb(17, 24, 39);
        lblTitle.Location = new Point(34, 27);

        btnNew.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnNew.BackColor = Color.FromArgb(37, 99, 235);
        btnNew.FlatAppearance.BorderSize = 0;
        btnNew.FlatStyle = FlatStyle.Flat;
        btnNew.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnNew.ForeColor = Color.White;
        btnNew.Size = new Size(183, 59);
        btnNew.Text = "+  Tạo Lịch Hẹn";

        btnExport.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnExport.BackColor = Color.White;
        btnExport.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
        btnExport.FlatStyle = FlatStyle.Flat;
        btnExport.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnExport.ForeColor = Color.FromArgb(55, 65, 81);
        btnExport.Size = new Size(160, 59);
        btnExport.Text = "↓  Xuất Dữ Liệu";

        headerPanel.Controls.Add(btnNew);
        headerPanel.Controls.Add(btnExport);
        headerPanel.Controls.Add(lblTitle);

        headerPanel.SizeChanged += (_, _) => PositionHeaderButtons(headerPanel);
        PositionHeaderButtons(headerPanel);

        // Content panel
        var contentPanel = new Panel
        {
            BackColor = Color.FromArgb(243, 244, 246),
            Dock = DockStyle.Fill,
            // reduce excessive bottom padding so content is visible
            Padding = new Padding(34, 13, 34, 34)
        };

        // White panel inside content
        var whitePanel = new Panel
        {
            BackColor = Color.White,
            Dock = DockStyle.Fill,
            Padding = new Padding(34, 13, 34, 67)
        };

        // Search panel (dock top so it's visible)
        var searchPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(6),
            Height = 64,
            WrapContents = false
        };

        var searchBoxPanel = new Panel
        {
            Anchor = AnchorStyles.Top | AnchorStyles.Left,
            BackColor = Color.FromArgb(249, 250, 251),
            BorderStyle = BorderStyle.FixedSingle,
            Location = new Point(23, 23),
            Padding = new Padding(10, 6, 10, 6),
            // smaller width for compact search box
            Size = new Size(360, 42)
        };

        // use existing lblSearchIcon field
        lblSearchIcon.Font = new Font("Segoe UI", 11F);
        lblSearchIcon.ForeColor = Color.FromArgb(107,114,128);
        lblSearchIcon.Location = new Point(5,9);
        lblSearchIcon.Text = "🔍";
        txtSearch.BackColor = Color.FromArgb(249, 250, 251);
        txtSearch.BorderStyle = BorderStyle.None;
        txtSearch.Font = new Font("Segoe UI", 11F);
        txtSearch.Location = new Point(42, 10);
        txtSearch.PlaceholderText = "Tìm kiếm cuộc hẹn (chuyên khoa, bệnh nhân, bác sĩ...)";
        // reduce width so search box is compact
        txtSearch.Width = 300;

        // place search textbox and icon into searchBoxPanel; dock left so it's visible at panel start
        txtSearch.Dock = DockStyle.Left;
        lblSearchIcon.Dock = DockStyle.Left;
        searchBoxPanel.Controls.Add(lblSearchIcon);
        searchBoxPanel.Controls.Add(txtSearch);

        // date range inputs
        dtFrom.Width = 140;
        dtTo.Width = 140;
        dtFrom.Font = new Font("Segoe UI", 10F);
        dtTo.Font = new Font("Segoe UI", 10F);

        // filter button
        btnFilter.BackColor = Color.FromArgb(229, 231, 235);
        btnFilter.FlatAppearance.BorderSize = 0;
        btnFilter.FlatStyle = FlatStyle.Flat;
        btnFilter.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnFilter.ForeColor = Color.Black;
        btnFilter.Size = new Size(120, 40);

        // Filter container
        var filterPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(11, 10, 11, 10),
            WrapContents = false,
            Height = 80
        };

        ConfigureFilterButton(btnServiceFilter);
        ConfigureFilterButton(btnCustomerFilter);
        ConfigureFilterButton(btnEmployeeFilter);
        ConfigureFilterButton(btnStatusFilter);

        filterPanel.Controls.Add(btnServiceFilter);
        filterPanel.Controls.Add(btnCustomerFilter);
        filterPanel.Controls.Add(btnEmployeeFilter);
        filterPanel.Controls.Add(btnStatusFilter);

        filterContainerPanel.Controls.Add(filterPanel);

        whitePanel.Controls.Add(filterContainerPanel);
        whitePanel.Controls.Add(searchPanel);
        searchPanel.Controls.Add(searchBoxPanel);
        searchPanel.Controls.Add(dtFrom);
        searchPanel.Controls.Add(dtTo);
        searchPanel.Controls.Add(btnFilter);

        // list container
        var listContainer = new Panel { Dock = DockStyle.Fill, Location = new Point(34, 199), Size = new Size(1463, 655) };

        // empty state - center the labels using a TableLayoutPanel
        emptyStatePanel.BackColor = Color.FromArgb(249, 250, 251);
        emptyStatePanel.Dock = DockStyle.Fill;
        emptyStatePanel.Visible = false;

        var table = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = Color.Transparent, ColumnCount = 1, RowCount = 2 };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        table.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        table.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

        var lblEmptyMessage = new Label { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.FromArgb(17,24,39), Text = "Không có cuộc hẹn nào", TextAlign = ContentAlignment.MiddleCenter };
        var lblEmptySubtitle = new Label { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10F), ForeColor = Color.FromArgb(107,114,128), Text = "Vui lòng điều chỉnh bộ lọc hoặc phạm vi ngày để tìm kiếm cuộc hẹn phù hợp.", TextAlign = ContentAlignment.TopCenter };

        table.Controls.Add(lblEmptyMessage, 0, 0);
        table.Controls.Add(lblEmptySubtitle, 0, 1);

        emptyStatePanel.Controls.Add(table);

        // appointment grid
        appointmentGrid.AllowUserToAddRows = false;
        appointmentGrid.AllowUserToDeleteRows = false;
        appointmentGrid.AllowUserToResizeRows = false;
        appointmentGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        appointmentGrid.BackgroundColor = Color.White;
        appointmentGrid.BorderStyle = BorderStyle.None;
        appointmentGrid.ColumnHeadersHeight = 60;
        appointmentGrid.Dock = DockStyle.Fill;
        appointmentGrid.GridColor = Color.FromArgb(243, 244, 246);
        appointmentGrid.MultiSelect = false;
        appointmentGrid.ReadOnly = true;
        appointmentGrid.RowHeadersVisible = false;
        appointmentGrid.RowTemplate.Height = 60;
        appointmentGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

        listContainer.Controls.Add(appointmentGrid);
        listContainer.Controls.Add(emptyStatePanel);

        // ensure empty state is on top when visible
        emptyStatePanel.BringToFront();
        appointmentGrid.BringToFront();

        whitePanel.Controls.Add(listContainer);

        contentPanel.Controls.Add(whitePanel);

        Controls.Add(contentPanel);
        Controls.Add(headerPanel);

        // finalize grid columns and styling
        InitializeGridColumns();
        ApplyGridStyling();
    }

    private void InitializeGridColumns()
    {
        appointmentGrid.Columns.Clear();
        appointmentGrid.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Select", HeaderText = "", FillWeight = 5 });
        appointmentGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Time", HeaderText = "Thời Gian", FillWeight = 15 });
        appointmentGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Service", HeaderText = "Dịch Vụ", FillWeight = 15 });
        appointmentGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Customer", HeaderText = "Khách Hàng", FillWeight = 25 });
        appointmentGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Duration", HeaderText = "Thời Lượng", FillWeight = 10 });
        appointmentGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Trạng Thái", FillWeight = 15 });
        appointmentGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Employee", HeaderText = "Nhân Viên", FillWeight = 10 });
        appointmentGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Note", HeaderText = "Ghi Chú", FillWeight = 10 });
        appointmentGrid.Columns.Add(new DataGridViewButtonColumn { Name = "Action", HeaderText = "", Text = "•••", UseColumnTextForButtonValue = true, FillWeight = 5 });
    }

    private void PositionHeaderButtons(Panel headerPanel)
    {
        var rightPadding = headerPanel.Padding.Right;
        var spacing = 12;

        btnNew.Location = new Point(
            Math.Max(0, headerPanel.ClientSize.Width - rightPadding - btnNew.Width),
            24);

        btnExport.Location = new Point(
            Math.Max(0, btnNew.Left - spacing - btnExport.Width),
            24);
    }

    private void ApplyGridStyling()
    {
        var vietnameseFont = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
        var vietnameseFontBold = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);

        appointmentGrid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.White,
            ForeColor = Color.FromArgb(107, 114, 128),
            Font = vietnameseFontBold,
            Alignment = DataGridViewContentAlignment.MiddleLeft,
            Padding = new Padding(15, 0, 0, 0)
        };

        appointmentGrid.EnableHeadersVisualStyles = false;

        appointmentGrid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.White,
            ForeColor = Color.FromArgb(17, 24, 39),
            SelectionBackColor = Color.FromArgb(243, 244, 246),
            SelectionForeColor = Color.FromArgb(17, 24, 39),
            Padding = new Padding(15, 10, 0, 10),
            Font = vietnameseFont
        };

        appointmentGrid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.FromArgb(249, 250, 251),
            Font = vietnameseFont
        };
    }

    private void ConfigureInputs()
    {
        txtSearch.PlaceholderText = "Tìm kiếm...";
        dtFrom.ShowCheckBox = true;
        dtTo.ShowCheckBox = true;
        dtFrom.Value = DateTime.Today.AddDays(-7);
        dtTo.Value = DateTime.Today.AddDays(7);
        dtFrom.Checked = false;
        dtTo.Checked = false;

        txtSearch.TextChanged += (_, _) => RefreshGrid();
        dtFrom.ValueChanged += async (_, _) => await LoadAppointmentsAsync();
        dtTo.ValueChanged += async (_, _) => await LoadAppointmentsAsync();
    }

    private void InitializeFilterDropdowns()
    {
        _serviceDropdown = CreateFilterDropdown(btnServiceFilter, a => a.Specialty);
        _customerDropdown = CreateFilterDropdown(btnCustomerFilter, a => a.Patient);
        _employeeDropdown = CreateFilterDropdown(btnEmployeeFilter, a => a.Doctor);
        _statusDropdown = CreateFilterDropdown(btnStatusFilter, a => a.StatusLabel);

        ResetFilterButtonStyle(btnServiceFilter);
        ResetFilterButtonStyle(btnCustomerFilter);
        ResetFilterButtonStyle(btnEmployeeFilter);
        ResetFilterButtonStyle(btnStatusFilter);

        btnFilter.Click += (_, _) =>
        {
            filterContainerPanel.Visible = !filterContainerPanel.Visible;
            if (filterContainerPanel.Visible)
            {
                btnFilter.BackColor = Color.FromArgb(37, 99, 235);
                btnFilter.ForeColor = Color.White;
            }
            else
            {
                btnFilter.BackColor = Color.FromArgb(229, 231, 235);
                btnFilter.ForeColor = Color.Black;
                foreach (var dropdown in _filterDropdowns)
                {
                    dropdown.Parent?.Hide();
                }
            }
        };
    }

    private void ConfigureActions()
    {
        btnNew.Click += async (_, _) => await ShowUpsertDialogAsync();
        btnExport.Click += (_, _) => _dialogService.ShowInfo("Chức năng xuất dữ liệu sẽ sớm có mặt.");

        appointmentGrid.CellContentClick += async (_, args) =>
        {
            if (args.RowIndex >= 0 && appointmentGrid.Columns[args.ColumnIndex].Name == "Action")
            {
                if (appointmentGrid.Rows[args.RowIndex].Tag is not AppointmentRow row)
                {
                    return;
                }

                // build context menu on demand so it reflects current status
                actionMenu?.Dispose();
                actionMenu = new ContextMenuStrip();

                // If appointment not approved, offer confirm action
                var status = (row.StatusCode ?? string.Empty).ToLowerInvariant();
                if (status != "approved" && status != "paidtransfer")
                {
                    var confirmItem = new ToolStripMenuItem("Xác nhận cuộc hẹn");
                    confirmItem.ForeColor = Color.FromArgb(22, 101, 52);
                    confirmItem.Click += async (_, _) => await ConfirmAppointmentAsync(row.Id);
                    actionMenu.Items.Add(confirmItem);
                }

                var editItem = new ToolStripMenuItem("Chỉnh sửa");
                editItem.Click += async (_, _) => await ShowUpsertDialogAsync(row);
                actionMenu.Items.Add(editItem);

                var deleteItem = new ToolStripMenuItem("Xóa cuộc hẹn");
                deleteItem.ForeColor = Color.FromArgb(185, 28, 28);
                deleteItem.Click += async (_, _) => await DeleteAppointmentAsync(row.Id);
                actionMenu.Items.Add(deleteItem);

                // show menu at cell location
                var cellRect = appointmentGrid.GetCellDisplayRectangle(args.ColumnIndex, args.RowIndex, true);
                var pt = new Point(cellRect.Right - 5, cellRect.Bottom);
                actionMenu.Show(appointmentGrid, pt);
            }
        };

        // When user double-clicks a cell (not action button) show compact details then clear selection so row color does not remain highlighted.
        appointmentGrid.CellDoubleClick += (_, args) =>
        {
            if (args.RowIndex < 0) return;

            // if clicked on Action column, ignore here (handled in CellContentClick)
            var colName = appointmentGrid.Columns[args.ColumnIndex].Name;
            if (string.Equals(colName, "Action", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (appointmentGrid.Rows[args.RowIndex].Tag is not AppointmentRow row)
            {
                return;
            }

            // reuse helper to show details
            ShowAppointmentDetails(row);
            // clear selection to avoid persistent highlight
            appointmentGrid.ClearSelection();
        };
    }

    private async Task ConfirmAppointmentAsync(Guid appointmentId)
    {
        try
        {
            ToggleLoading(true);
            var updated = await _appointmentsApiClient.UpdateStatusAsync(appointmentId, new DoctorAppointmentStatusRequest { Status = "approved" });
            if (updated != null)
            {
                var row = ToRow(updated);
                var idx = _appointments.FindIndex(a => a.Id == row.Id);
                if (idx >= 0)
                {
                    _appointments[idx] = row;
                }
                else
                {
                    _appointments.Add(row);
                }

                UpdateFilterOptionsFromData();
                RefreshGrid();
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Không thể xác nhận cuộc hẹn: {ex.Message}");
        }
        finally
        {
            ToggleLoading(false);
        }
    }

    private async Task DeleteAppointmentAsync(Guid appointmentId)
    {
        if (!_dialogService.Confirm("Bạn có chắc chắn muốn xóa cuộc hẹn này?"))
        {
            return;
        }

        try
        {
            ToggleLoading(true);
            await _appointmentsApiClient.DeleteAsync(appointmentId);

            var idx = _appointments.FindIndex(a => a.Id == appointmentId);
            if (idx >= 0)
            {
                _appointments.RemoveAt(idx);
            }

            UpdateFilterOptionsFromData();
            RefreshGrid();
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Không thể xóa cuộc hẹn: {ex.Message}");
        }
        finally
        {
            ToggleLoading(false);
        }
    }

    private async Task ShowUpsertDialogAsync(AppointmentRow? existing = null)
    {
        try
        {
            _metadata ??= await _appointmentsApiClient.GetMetadataAsync();

            if (_metadata == null)
            {
                _dialogService.ShowError("Không tải được dữ liệu bác sĩ.");
                return;
            }

            using var dialog = new DoctorAppointmentUpsertDialog(_metadata, existing, _bookingApiClient, _customerService);
            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            var payload = dialog.BuiltRequest;
            if (payload == null)
            {
                return;
            }

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

    private sealed class DoctorAppointmentUpsertDialog : Form
    {
        private const int MinAppointmentLeadDays = 2;
        private const int MaxAppointmentLeadMonths = 1;
        private const int DefaultDurationMinutes = 30;

        private readonly ComboBox _specialtyBox = new() { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly TextBox _patientDisplay = new() { ReadOnly = true };
        private readonly Button _selectPatientButton = new() { Text = "Chọn bệnh nhân" };
        private readonly DateTimePicker _datePicker = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", ShowCheckBox = true };
        private readonly ComboBox _timeSlotBox = new() { DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };
        private readonly NumericUpDown _durationBox = new() { Minimum = DefaultDurationMinutes, Maximum = DefaultDurationMinutes, Increment = DefaultDurationMinutes, Value = DefaultDurationMinutes, ReadOnly = true, Enabled = false };
        private readonly TextBox _manualPhone = new() { PlaceholderText = "Nhập SĐT nếu hồ sơ thiếu" };
        private readonly ComboBox _statusBox = new() { DropDownStyle = ComboBoxStyle.DropDownList };

        private readonly AppointmentRow? _existing;
        private readonly DoctorAppointmentMetadataDto _metadata;
        private readonly CustomerBookingApiClient _bookingApiClient;
        private readonly CustomerService _customerService;
        private readonly List<DoctorTimeSlotDto> _slotOptions = new();
        private bool _suppressEvents;
        private bool _hasSelectedDate;
        private DateTime? _existingSlotStart;
        private CustomerDto? _selectedCustomer;

        public DoctorAppointmentUpsertRequest? BuiltRequest { get; private set; }

        public DoctorAppointmentUpsertDialog(DoctorAppointmentMetadataDto metadata, AppointmentRow? existing, CustomerBookingApiClient bookingApiClient, CustomerService customerService)
        {
            _metadata = metadata;
            _existing = existing;
            _bookingApiClient = bookingApiClient;
            _customerService = customerService;

            Text = existing == null ? "Thêm cuộc hẹn" : "Cập nhật cuộc hẹn";
            Width = 460;
            Height = 380;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            BuildLayout();
            PopulateOptions();
            BindExisting();
        }

        private void BuildLayout()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 7,
                Padding = new Padding(12),
                AutoSize = true
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));

            AddRow(layout, 0, "Chuyên khoa", _specialtyBox);
            AddRow(layout, 1, "Bệnh nhân", BuildPatientSelector());
            AddRow(layout, 2, "SĐT", _manualPhone);
            AddRow(layout, 3, "Ngày khám", _datePicker);
            AddRow(layout, 4, "Khung giờ", _timeSlotBox);
            AddRow(layout, 5, "Thời lượng (phút)", _durationBox);

            var nextRow = 6;
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

        private Control BuildPatientSelector()
        {
            var panel = new TableLayoutPanel
            {
                ColumnCount = 2,
                Dock = DockStyle.Fill,
                AutoSize = true
            };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));

            _patientDisplay.Dock = DockStyle.Fill;
            _patientDisplay.PlaceholderText = "Chưa chọn bệnh nhân";
            _selectPatientButton.Dock = DockStyle.Fill;
            _selectPatientButton.Click += async (_, _) => await SelectPatientAsync();

            panel.Controls.Add(_patientDisplay, 0, 0);
            panel.Controls.Add(_selectPatientButton, 1, 0);
            return panel;
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
            if (!_hasSelectedDate)
            {
                _timeSlotBox.Enabled = false;
                _timeSlotBox.DataSource = null;
                _timeSlotBox.Text = "Chọn ngày để xem khung giờ";
                return;
            }

            var date = DateOnly.FromDateTime(_datePicker.Value.Date);
            try
            {
                var slots = await _bookingApiClient.GetDoctorSlotsAsync(_metadata.DoctorId, date);
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
            _datePicker.MaxDate = DateTime.Today.AddMonths(MaxAppointmentLeadMonths);
            _datePicker.Checked = false;
            _datePicker.ValueChanged += async (_, _) => await HandleDateChangedAsync();

            _specialtyBox.DisplayMember = nameof(DoctorAppointmentSpecialtyOptionDto.Name);
            _specialtyBox.ValueMember = nameof(DoctorAppointmentSpecialtyOptionDto.Id);
            _specialtyBox.DataSource = _metadata.Specialties.ToList();

            _statusBox.DataSource = _metadata.Statuses.ToList();
            _statusBox.DisplayMember = nameof(DoctorAppointmentStatusOptionDto.Label);
            _statusBox.ValueMember = nameof(DoctorAppointmentStatusOptionDto.Code);

            if (_existing == null)
            {
                _suppressEvents = true;
                _specialtyBox.SelectedIndex = _specialtyBox.Items.Count > 0 ? 0 : -1;
                _suppressEvents = false;
            }
        }

        private void BindExisting()
        {
            if (_existing == null)
            {
                return;
            }

            _specialtyBox.SelectedValue = _existing.SpecialtyId;

            if (!string.IsNullOrWhiteSpace(_existing.PatientId))
            {
                _selectedCustomer = new CustomerDto
                {
                    Id = _existing.PatientId,
                    FullName = _existing.Patient ?? string.Empty,
                    PhoneNumber = _existing.Phone ?? string.Empty
                };
                UpdatePatientDisplay();
            }

            _manualPhone.Text = _existing.Phone;

            var existingLocal = _existing.StartUtc.ToLocalTime();
            _existingSlotStart = existingLocal;

            _datePicker.Value = existingLocal.Date;
            _datePicker.Checked = true;

            if (!string.IsNullOrWhiteSpace(_existing.StatusCode))
            {
                _statusBox.SelectedValue = _existing.StatusCode;
            }

            _ = RefreshSlotsAsync(existingLocal);
        }

        private async Task SelectPatientAsync()
        {
            using var dialog = new CustomerPickerDialog("Chọn bệnh nhân", () => _customerService.GetAllAsync());
            if (dialog.ShowDialog(this) == DialogResult.OK && dialog.SelectedCustomer != null)
            {
                _selectedCustomer = dialog.SelectedCustomer;
                UpdatePatientDisplay();
            }
        }

        private void UpdatePatientDisplay()
        {
            if (_selectedCustomer == null)
            {
                _patientDisplay.Text = string.Empty;
                return;
            }

            var display = _selectedCustomer.FullName;
            if (!string.IsNullOrWhiteSpace(_selectedCustomer.Email))
            {
                display = string.IsNullOrWhiteSpace(display)
                    ? _selectedCustomer.Email
                    : $"{display} - {_selectedCustomer.Email}";
            }

            _patientDisplay.Text = display;

            if (!string.IsNullOrWhiteSpace(_selectedCustomer.PhoneNumber))
            {
                _manualPhone.Text = _selectedCustomer.PhoneNumber;
            }
            else if (string.IsNullOrWhiteSpace(_manualPhone.Text))
            {
                _manualPhone.PlaceholderText = "Bệnh nhân chưa có SĐT, vui lòng nhập.";
            }
        }

        private DoctorAppointmentUpsertRequest BuildRequest()
        {
            if (_specialtyBox.SelectedValue is not Guid specialtyId || specialtyId == Guid.Empty)
            {
                throw new InvalidOperationException("Vui lòng chọn chuyên khoa.");
            }

            if (_selectedCustomer == null)
            {
                throw new InvalidOperationException("Vui lòng chọn bệnh nhân.");
            }

            var patientName = (_selectedCustomer.FullName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(patientName))
            {
                throw new InvalidOperationException("Vui lòng nhập tên bệnh nhân.");
            }

            if (!IsValidPersonName(patientName))
            {
                throw new InvalidOperationException("Tên bệnh nhân không hợp lệ (không chứa số hoặc ký tự đặc biệt).");
            }

            var manualPhone = _manualPhone.Text.Trim();
            var resolvedPhone = string.IsNullOrWhiteSpace(_selectedCustomer.PhoneNumber) ? manualPhone : _selectedCustomer.PhoneNumber;
            if (string.IsNullOrWhiteSpace(resolvedPhone))
            {
                throw new InvalidOperationException("Bệnh nhân chưa có số điện thoại, vui lòng nhập để tiếp tục.");
            }

            if (!IsValidVietnamPhone(resolvedPhone))
            {
                throw new InvalidOperationException("Số điện thoại không hợp lệ (phải đúng 10 chữ số).");
            }

            if (!_datePicker.Checked)
            {
                throw new InvalidOperationException("Vui lòng chọn ngày khám.");
            }

            if (_timeSlotBox.SelectedItem is not DoctorTimeSlotDto slot)
            {
                throw new InvalidOperationException("Vui lòng chọn khung giờ.");
            }

            var startLocal = slot.StartLocal;
            var minDate = DateTime.Today.AddDays(MinAppointmentLeadDays);
            var maxDate = DateTime.Today.AddMonths(MaxAppointmentLeadMonths);
            if (startLocal.Date < minDate)
            {
                throw new InvalidOperationException($"Ngày khám phải cách hiện tại ít nhất {MinAppointmentLeadDays} ngày.");
            }

            if (startLocal.Date > maxDate)
            {
                throw new InvalidOperationException($"Ngày khám phải nằm trong vòng {MaxAppointmentLeadMonths} tháng kể từ hôm nay.");
            }

            var duration = DefaultDurationMinutes;
            var status = _statusBox.SelectedValue?.ToString() ?? "pending";

            return new DoctorAppointmentUpsertRequest
            {
                SpecialtyId = specialtyId,
                PatientName = patientName,
                PatientId = _selectedCustomer.Id,
                CustomerPhone = resolvedPhone,
                SlotStartUtc = slot.StartUtc,
                DurationMinutes = duration,
                Status = status,
                ClinicRoomId = _existing?.ClinicRoomId
            };
        }

        private static bool IsValidVietnamPhone(string phone)
        {
            var digits = new string(phone.Where(char.IsDigit).ToArray());
            return digits.Length == 10;
        }

        private static bool IsValidPersonName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            foreach (var ch in name)
            {
                if (char.IsDigit(ch))
                {
                    return false;
                }
            }

            foreach (var ch in name)
            {
                if (!(char.IsLetter(ch) || char.IsWhiteSpace(ch) || ch is '.' or '-' or '\''))
                {
                    return false;
                }
            }

            return true;
        }
    }

    private async Task LoadAppointmentsAsync()
    {
        try
        {
            ToggleLoading(true);
            _metadata ??= await _appointmentsApiClient.GetMetadataAsync();

            var from = dtFrom.Checked ? DateOnly.FromDateTime(dtFrom.Value.Date) : (DateOnly?)null;
            var to = dtTo.Checked ? DateOnly.FromDateTime(dtTo.Value.Date) : (DateOnly?)null;

            var items = await _appointmentsApiClient.GetAppointmentsAsync(from, to);
            _appointments.Clear();
            _appointments.AddRange(items.Select(ToRow).OrderByDescending(a => a.Start));

            UpdateFilterOptionsFromData();
            RefreshGrid();
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Không tải được cuộc hẹn: {ex.Message}");
            _appointments.Clear();
            RefreshGrid();
        }
        finally
        {
            ToggleLoading(false);
        }
    }

    private void ToggleLoading(bool loading)
    {
        appointmentGrid.Enabled = !loading;
        btnNew.Enabled = !loading;
        btnFilter.Enabled = !loading;
        btnServiceFilter.Enabled = !loading;
        btnCustomerFilter.Enabled = !loading;
        btnEmployeeFilter.Enabled = !loading;
        btnStatusFilter.Enabled = !loading;
    }

    private void RefreshGrid()
    {
        IEnumerable<AppointmentRow> data = _appointments;

        if (dtFrom.Checked)
        {
            data = data.Where(a => a.Start.Date >= dtFrom.Value.Date);
        }

        if (dtTo.Checked)
        {
            data = data.Where(a => a.Start.Date <= dtTo.Value.Date);
        }

        var search = txtSearch.Text?.Trim();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var low = search.ToLowerInvariant();
            data = data.Where(a => (a.Specialty ?? string.Empty).ToLowerInvariant().Contains(low)
                                    || (a.Patient ?? string.Empty).ToLowerInvariant().Contains(low)
                                    || (a.Doctor ?? string.Empty).ToLowerInvariant().Contains(low)
                                    || (a.Room ?? string.Empty).ToLowerInvariant().Contains(low));
        }

        data = ApplyDropdownFilter(_serviceDropdown, data);
        data = ApplyDropdownFilter(_customerDropdown, data);
        data = ApplyDropdownFilter(_employeeDropdown, data);
        data = ApplyDropdownFilter(_statusDropdown, data);

        var rows = data.OrderByDescending(a => a.Start).ToList();

        _filteredAppointments = rows;
        _currentPage = 1;
        RenderPage();

        lblTitle.Text = $"Lịch Hẹn ({rows.Count})";
    }

    private AppointmentRow ToRow(DoctorAppointmentListItemDto dto)
    {
        var startUtc = DateTime.SpecifyKind(dto.StartUtc, DateTimeKind.Utc);
        var startLocal = startUtc.ToLocalTime();
        return new AppointmentRow(
            dto.Id,
            dto.DoctorId,
            dto.SpecialtyId,
            startLocal,
            startUtc,
            dto.SpecialtyName,
            dto.PatientName,
            dto.PatientId,
            dto.DurationMinutes,
            dto.Status,
            dto.StatusLabel,
            dto.DoctorName,
            dto.ClinicRoom ?? string.Empty,
            dto.CustomerPhone,
            dto.ClinicRoomId);
    }

    private void ShowAppointmentDetails(AppointmentRow row)
    {
        if (row == null) return;
        var details = new System.Text.StringBuilder();
        details.AppendLine($"ID: {row.Id}");
        details.AppendLine($"Bác sĩ: {row.Doctor}");
        details.AppendLine($"Chuyên khoa: {row.Specialty}");
        details.AppendLine($"Phòng: {row.Room}");
        details.AppendLine($"Thời gian: {row.Start:HH:mm} - {row.Start:dd/MM/yyyy}");
        details.AppendLine($"Thời lượng: {row.DurationMinutes} phút");
        details.AppendLine($"Bệnh nhân: {row.Patient}");
        details.AppendLine($"SĐT: {row.Phone}");
        details.AppendLine($"Trạng thái: {row.StatusLabel}");

        _dialogService.ShowInfo(details.ToString(), "Thông tin cuộc hẹn");
    }

    private void ConfigureFilterButton(Button button)
    {
        button.BackColor = Color.White;
        button.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
        button.FlatStyle = FlatStyle.Flat;
        button.Font = new Font("Segoe UI", 10F);
        button.ForeColor = Color.Black;
        button.Padding = new Padding(11, 0, 6, 0);
        button.Size = new Size(171, 53);
        button.TextAlign = ContentAlignment.MiddleLeft;
    }

    private void ResetFilterButtonStyle(Button button)
    {
        button.BackColor = Color.White;
        button.ForeColor = Color.Black;
        button.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
    }

    private CheckedListBox CreateFilterDropdown(Button parentButton, Func<AppointmentRow, string> selector)
    {
        var dropdownPanel = new Panel
        {
            Visible = false,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.White,
            AutoSize = false,
            Width = parentButton.Width + 100,
            Height = 220,
            Padding = new Padding(5)
        };

        var searchBox = new TextBox
        {
            PlaceholderText = "Tìm kiếm...",
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point),
            Dock = DockStyle.Top,
            Height = 25
        };

        var dropdown = new CheckedListBox
        {
            CheckOnClick = true,
            BorderStyle = BorderStyle.None,
            BackColor = Color.White,
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
            Dock = DockStyle.Fill,
            IntegralHeight = false
        };

        _filterOptions[dropdown] = new List<string>();
        _filterSelectors[dropdown] = selector;
        _dropdownButtons[dropdown] = parentButton;

        dropdownPanel.Controls.Add(dropdown);
        dropdownPanel.Controls.Add(searchBox);

        searchBox.TextChanged += (s, e) =>
        {
            var searchText = searchBox.Text.ToLower();
            dropdown.Items.Clear();

            var source = _filterOptions.TryGetValue(dropdown, out var values)
                ? values
                : new List<string>();

            var filteredItems = source
                .Where(item => item.ToLower().Contains(searchText))
                .ToArray();

            if (filteredItems.Length > 0)
            {
                dropdown.Items.AddRange(filteredItems);
            }
        };

        parentButton.Click += (s, e) =>
        {
            foreach (var otherDropdown in _filterDropdowns)
            {
                if (otherDropdown != dropdown)
                {
                    otherDropdown.Parent?.Hide();
                }
            }

            dropdownPanel.Visible = !dropdownPanel.Visible;

            if (dropdownPanel.Visible)
            {
                var btnLocationInForm = this.PointToClient(parentButton.PointToScreen(Point.Empty));
                dropdownPanel.Location = new Point(
                    btnLocationInForm.X,
                    btnLocationInForm.Y + parentButton.Height + 5
                );

                dropdownPanel.BringToFront();
            }
        };

        dropdown.ItemCheck += (s, e) =>
        {
            this.BeginInvoke((MethodInvoker)delegate
            {
                UpdateFilterButtonVisual(parentButton, dropdown.CheckedItems.Count);
                RefreshGrid();
            });
        };

        this.Controls.Add(dropdownPanel);
        dropdownPanel.BringToFront();

        _filterDropdowns.Add(dropdown);
        return dropdown;
    }

    private void UpdateFilterButtonVisual(Button parentButton, int checkedCount)
    {
        var baseText = GetBaseButtonText(parentButton.Text);
        if (checkedCount > 0)
        {
            parentButton.Text = $"{baseText} ({checkedCount})";
            parentButton.BackColor = Color.FromArgb(219, 234, 254);
            parentButton.ForeColor = Color.FromArgb(37, 99, 235);
            parentButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        }
        else
        {
            parentButton.Text = baseText;
            ResetFilterButtonStyle(parentButton);
        }
    }

    private string GetBaseButtonText(string buttonText)
    {
        var parts = buttonText.Split('(');
        return parts[0].Trim();
    }

    private IEnumerable<AppointmentRow> ApplyDropdownFilter(CheckedListBox? dropdown, IEnumerable<AppointmentRow> source)
    {
        if (dropdown == null || !_filterSelectors.TryGetValue(dropdown, out var selector))
        {
            return source;
        }

        var selected = dropdown.CheckedItems.Cast<string>().ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (selected.Count == 0)
        {
            return source;
        }

        return source.Where(a => selected.Contains(selector(a)));
    }

    private void UpdateFilterOptionsFromData()
    {
        UpdateDropdownItems(_serviceDropdown, _appointments.Select(a => a.Specialty));
        UpdateDropdownItems(_customerDropdown, _appointments.Select(a => a.Patient));
        UpdateDropdownItems(_employeeDropdown, _appointments.Select(a => a.Doctor));
        UpdateDropdownItems(_statusDropdown, _appointments.Select(a => a.StatusLabel));
    }

    private void UpdateDropdownItems(CheckedListBox? dropdown, IEnumerable<string> items)
    {
        if (dropdown == null)
        {
            return;
        }

        var orderedItems = items
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();

        var selected = dropdown.CheckedItems.Cast<string>().ToHashSet(StringComparer.OrdinalIgnoreCase);
        _filterOptions[dropdown] = orderedItems;

        dropdown.Items.Clear();
        dropdown.Items.AddRange(orderedItems.ToArray());

        for (var i = 0; i < dropdown.Items.Count; i++)
        {
            var item = dropdown.Items[i].ToString();
            if (item != null && selected.Contains(item))
            {
                dropdown.SetItemChecked(i, true);
            }
        }

        if (_dropdownButtons.TryGetValue(dropdown, out var button))
        {
            UpdateFilterButtonVisual(button, dropdown.CheckedItems.Count);
        }
    }

    private void InitializePaginationControls()
    {
        var paginationPanel = new Panel
        {
            Height = 40,
            Dock = DockStyle.Bottom,
            BackColor = Color.Transparent
        };

        _btnPrevPage = new Button
        {
            Text = "Trước",
            Width = 80,
            Height = 30,
            Left = 10,
            Top = 5
        };
        _btnPrevPage.Click += (s, e) => ChangePage(-1);

        _btnNextPage = new Button
        {
            Text = "Tiếp",
            Width = 80,
            Height = 30,
            Left = 100,
            Top = 5
        };
        _btnNextPage.Click += (s, e) => ChangePage(1);

        _lblPageInfo = new Label
        {
            AutoSize = false,
            Width = 240,
            Height = 30,
            Left = 200,
            Top = 8,
            TextAlign = ContentAlignment.MiddleLeft
        };

        _cbPageSize = new ComboBox
        {
            Width = 80,
            Height = 30,
            Left = 460,
            Top = 5,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _cbPageSize.Items.AddRange(new object[] { "5", "10", "20", "50" });
        _cbPageSize.SelectedItem = _pageSize.ToString();
        _cbPageSize.SelectedIndexChanged += (s, e) =>
        {
            if (int.TryParse(_cbPageSize.SelectedItem?.ToString(), out var newSize) && newSize > 0)
            {
                _pageSize = newSize;
                _currentPage = 1;
                RenderPage();
            }
        };

        paginationPanel.Controls.Add(_btnPrevPage);
        paginationPanel.Controls.Add(_btnNextPage);
        paginationPanel.Controls.Add(_lblPageInfo);
        paginationPanel.Controls.Add(_cbPageSize);

        this.Controls.Add(paginationPanel);
        paginationPanel.BringToFront();
    }

    private void ChangePage(int delta)
    {
        _currentPage += delta;
        if (_currentPage < 1) _currentPage = 1;
        if (_currentPage > _totalPages) _currentPage = _totalPages;
        RenderPage();
    }

    private void RenderPage()
    {
        appointmentGrid.Rows.Clear();

        if (_filteredAppointments == null) _filteredAppointments = new List<AppointmentRow>();

        var total = _filteredAppointments.Count;
        _totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)_pageSize));
        if (_currentPage < 1) _currentPage = 1;
        if (_currentPage > _totalPages) _currentPage = _totalPages;

        var pageItems = _filteredAppointments.Skip((_currentPage - 1) * _pageSize).Take(_pageSize).ToList();

        foreach (var row in pageItems)
        {
            RenderRow(row);
        }

        UpdatePaginationControls();

        appointmentGrid.Visible = total > 0;
        emptyStatePanel.Visible = total == 0;
    }

    private void UpdatePaginationControls()
    {
        if (_lblPageInfo != null)
        {
            _lblPageInfo.Text = $"Trang {_currentPage} / {_totalPages}   (Tổng {_filteredAppointments.Count})";
        }

        if (_btnPrevPage != null)
        {
            _btnPrevPage.Enabled = _currentPage > 1;
        }

        if (_btnNextPage != null)
        {
            _btnNextPage.Enabled = _currentPage < _totalPages;
        }

        if (_cbPageSize != null && _cbPageSize.SelectedItem == null)
        {
            _cbPageSize.SelectedItem = _pageSize.ToString();
        }
    }

    private void RenderRow(AppointmentRow row)
    {
        var durationText = row.DurationMinutes switch
        {
            >= 60 when row.DurationMinutes % 60 == 0 => $"{row.DurationMinutes / 60} giờ",
            >= 60 => $"{row.DurationMinutes / 60} giờ {row.DurationMinutes % 60} phút",
            _ => $"{row.DurationMinutes} phút"
        };

        var formattedTime = row.Start.ToString("HH:mm - dd/MM/yyyy");

        var index = appointmentGrid.Rows.Add(false, formattedTime, row.Specialty, row.Patient, durationText, row.StatusLabel, row.Doctor, row.Room, "•••");
        appointmentGrid.Rows[index].Tag = row;

        var statusCell = appointmentGrid.Rows[index].Cells["Status"];

        if (_statusStyles.TryGetValue(row.StatusCode, out var style))
        {
            statusCell.Style.BackColor = style.Background;
            statusCell.Style.ForeColor = style.Foreground;
            statusCell.Style.SelectionBackColor = style.Background;
            statusCell.Style.SelectionForeColor = style.Foreground;
        }

        statusCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        statusCell.Style.Padding = new Padding(8, 6, 8, 6);
        statusCell.Style.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

        appointmentGrid.Rows[index].Cells["Action"].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
    }
}
