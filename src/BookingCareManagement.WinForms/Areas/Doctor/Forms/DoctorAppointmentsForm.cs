using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BookingCareManagement.WinForms.Areas.Doctor.Services;
using BookingCareManagement.WinForms.Areas.Customer.Services.Models;
using BookingCareManagement.WinForms.Shared.Models.Dtos;
using BookingCareManagement.WinForms.Shared.Services;

namespace BookingCareManagement.WinForms.Areas.Doctor.Forms;

public sealed partial class DoctorAppointmentsForm : Form
{
    private readonly DoctorAppointmentsApiClient _appointmentsApiClient;
    private readonly DialogService _dialogService;
    private readonly CustomerBookingApiClient _bookingApiClient;

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
    private readonly Panel filterContainerPanel = new() { BackColor = Color.White, Height = 120, Dock = DockStyle.Top, Visible = false };
    private readonly Panel emptyStatePanel = new() { Dock = DockStyle.Fill, Visible = false };
    private ContextMenuStrip? actionMenu;

    private AdminAppointmentMetadataDto? _metadata; // reuse admin dto shape for UI labels where compatible
    private List<DoctorAppointmentListItemDto> _appointments = new();

    public DoctorAppointmentsForm(DialogService dialogService, DoctorAppointmentsApiClient appointmentsApiClient, CustomerBookingApiClient bookingApiClient)
    {
        _dialogService = dialogService;
        _appointmentsApiClient = appointmentsApiClient;
        _bookingApiClient = bookingApiClient;

        InitializeComponentManual();
        ConfigureInputs();
        InitializeFilterControls();
        ConfigureActions();

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
        btnNew.Location = new Point(1383, 24);
        btnNew.Text = "+  Tạo Lịch Hẹn";

        btnExport.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnExport.BackColor = Color.White;
        btnExport.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
        btnExport.FlatStyle = FlatStyle.Flat;
        btnExport.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnExport.ForeColor = Color.FromArgb(55, 65, 81);
        btnExport.Size = new Size(160, 59);
        btnExport.Location = new Point(1211, 24);
        btnExport.Text = "↓  Xuất Dữ Liệu";

        headerPanel.Controls.Add(btnNew);
        headerPanel.Controls.Add(btnExport);
        headerPanel.Controls.Add(lblTitle);

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
        var searchPanel = new FlowLayoutPanel { Dock = DockStyle.Top, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(6), Height = 64 };
        // Only keep compact search box - remove other labels and date pickers per request

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

        // hide filter button and filter container from UI (keep fields for potential future use)
        btnFilter.Visible = false;
        filterContainerPanel.Visible = false;

        // add only searchBoxPanel to the searchPanel
        whitePanel.Controls.Add(searchPanel);
        searchPanel.Controls.Add(searchBoxPanel);

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
        appointmentGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Time", HeaderText = "Thời Gian", FillWeight = 12 });
        appointmentGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Service", HeaderText = "Dịch Vụ", FillWeight = 20 });
        appointmentGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Customer", HeaderText = "Bệnh Nhân", FillWeight = 25 });
        // slightly increase Duration column weight so header text doesn't get truncated
        appointmentGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Duration", HeaderText = "Thời Lượng", FillWeight = 10 });
        appointmentGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Trạng Thái", FillWeight = 12 });
        appointmentGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Room", HeaderText = "Phòng", FillWeight = 10 });
        appointmentGrid.Columns.Add(new DataGridViewButtonColumn { Name = "Action", HeaderText = "", Text = "...", UseColumnTextForButtonValue = true, FillWeight = 8 });
    }

    private void ApplyGridStyling()
    {
        var vietnameseFont = new Font("Segoe UI", 10F);
        appointmentGrid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.White, ForeColor = Color.FromArgb(107,114,128), Font = new Font("Segoe UI", 10F, FontStyle.Bold), Padding = new Padding(8) };
        appointmentGrid.EnableHeadersVisualStyles = false;
        // make selection subtle by matching normal background and keeping foreground unchanged
        appointmentGrid.DefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.White, ForeColor = Color.FromArgb(17,24,39), SelectionBackColor = Color.FromArgb(249,250,251), SelectionForeColor = Color.FromArgb(17,24,39), Padding = new Padding(8), Font = vietnameseFont };
        appointmentGrid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(249,250,251) };
    }

    private void ConfigureInputs()
    {
        txtSearch.PlaceholderText = "Tìm kiếm...";
        dtFrom.Checked = false; dtTo.Checked = false;

        // real-time search filtering
        txtSearch.TextChanged += (_, _) => RefreshGrid();
    }

    private void InitializeFilterControls()
    {
        btnServiceFilter.Click += (_, _) => ToggleFilterPanel(btnServiceFilter);
        btnCustomerFilter.Click += (_, _) => ToggleFilterPanel(btnCustomerFilter);
        btnEmployeeFilter.Click += (_, _) => ToggleFilterPanel(btnEmployeeFilter);
        btnStatusFilter.Click += (_, _) => ToggleFilterPanel(btnStatusFilter);
        btnFilter.Click += (_, _) => filterContainerPanel.Visible = !filterContainerPanel.Visible;
    }

    private void ToggleFilterPanel(Button btn)
    {
        // placeholder: show a messagebox for now
        _dialogService.ShowInfo($"Bộ lọc: {btn.Text} (chức năng lọc sẽ được mở rộng)");
    }

    private void ConfigureActions()
    {
        btnNew.Click += async (_, _) => { _dialogService.ShowInfo("Tạo cuộc hẹn (chức năng chưa bật trong bản WinForms bác sĩ)"); };
        appointmentGrid.CellContentClick += async (_, args) =>
        {
            if (args.RowIndex >= 0 && appointmentGrid.Columns[args.ColumnIndex].Name == "Action")
            {
                var dto = appointmentGrid.Rows[args.RowIndex].Tag as DoctorAppointmentListItemDto;
                if (dto == null) return;

                // build context menu on demand so it reflects current status
                actionMenu?.Dispose();
                actionMenu = new ContextMenuStrip();

                // If appointment not approved, offer confirm action
                var status = (dto.Status ?? string.Empty).ToLowerInvariant();
                if (status != "approved" && status != "paidtransfer")
                {
                    var confirmItem = new ToolStripMenuItem("Xác nhận cuộc hẹn");
                    confirmItem.ForeColor = Color.FromArgb(22, 101, 52);
                    confirmItem.Click += async (_, _) => await ConfirmAppointmentAsync(dto.Id);
                    actionMenu.Items.Add(confirmItem);
                }

                // common info action -> show full details same as double-click
                var infoItem = new ToolStripMenuItem("Chi tiết");
                infoItem.Click += (_, _) => ShowAppointmentDetails(dto);
                actionMenu.Items.Add(infoItem);

                var deleteItem = new ToolStripMenuItem("Xóa cuộc hẹn");
                deleteItem.ForeColor = Color.FromArgb(185, 28, 28);
                deleteItem.Click += async (_, _) => await DeleteAppointmentAsync(dto.Id);
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

            var dto = appointmentGrid.Rows[args.RowIndex].Tag as DoctorAppointmentListItemDto;
            if (dto == null)
            {
                return;
            }

            // reuse helper to show details
            ShowAppointmentDetails(dto);
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
                // update local list
                var idx = _appointments.FindIndex(a => a.Id == updated.Id);
                if (idx >= 0)
                {
                    _appointments[idx] = updated;
                }
                else
                {
                    _appointments.Add(updated);
                }

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

    private async Task LoadAppointmentsAsync()
    {
        try
        {
            ToggleLoading(true);
            var from = dtFrom.Checked ? DateOnly.FromDateTime(dtFrom.Value.Date) : (DateOnly?)null;
            var to = dtTo.Checked ? DateOnly.FromDateTime(dtTo.Value.Date) : (DateOnly?)null;

            var items = await _appointmentsApiClient.GetAppointmentsAsync(from, to);
            _appointments = items?.ToList() ?? new List<DoctorAppointmentListItemDto>();
            RefreshGrid();
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Không tải được cuộc hẹn: {ex.Message}");
            _appointments = new List<DoctorAppointmentListItemDto>();
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
    }

    private void RefreshGrid()
    {
        var data = _appointments.AsEnumerable();
        var search = txtSearch.Text?.Trim();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var low = search.ToLowerInvariant();
            data = data.Where(a => (a.SpecialtyName ?? string.Empty).ToLowerInvariant().Contains(low)
                                    || (a.PatientName ?? string.Empty).ToLowerInvariant().Contains(low)
                                    || (a.StatusLabel ?? string.Empty).ToLowerInvariant().Contains(low));
        }

        var rows = data.OrderBy(a => a.StartUtc).ToList();
        // update title with current visible appointment count
        lblTitle.Text = $"Lịch Hẹn ({rows.Count})";
        appointmentGrid.Rows.Clear();
        foreach (var dto in rows)
        {
            var startLocal = dto.StartUtc.ToLocalTime();
            var time = $"{startLocal:HH:mm} - {startLocal:dd/MM/yyyy}";
            var duration = dto.DurationMinutes > 0 ? $"{dto.DurationMinutes} phút" : "--";
            var idx = appointmentGrid.Rows.Add(false, time, dto.SpecialtyName, dto.PatientName, duration, dto.StatusLabel, dto.ClinicRoom ?? string.Empty, "...");
            appointmentGrid.Rows[idx].Tag = dto;

            var statusCell = appointmentGrid.Rows[idx].Cells[5];
            switch ((dto.Status ?? string.Empty).ToLowerInvariant())
            {
                case "approved":
                case "paidtransfer":
                    statusCell.Style.BackColor = Color.FromArgb(220, 252, 231);
                    statusCell.Style.ForeColor = Color.FromArgb(22, 101, 52);
                    break;
                case "pending":
                    statusCell.Style.BackColor = Color.FromArgb(239, 246, 255);
                    statusCell.Style.ForeColor = Color.FromArgb(37, 99, 235);
                    break;
                case "canceled":
                    statusCell.Style.BackColor = Color.FromArgb(254, 242, 242);
                    statusCell.Style.ForeColor = Color.FromArgb(185, 28, 28);
                    break;
                default:
                    statusCell.Style.BackColor = Color.FromArgb(248, 250, 252);
                    statusCell.Style.ForeColor = Color.FromArgb(107, 114, 128);
                    break;
            }
        }

        appointmentGrid.Visible = appointmentGrid.Rows.Count > 0;
        emptyStatePanel.Visible = appointmentGrid.Rows.Count == 0;
    }

    private void ShowAppointmentDetails(DoctorAppointmentListItemDto dto)
    {
        if (dto == null) return;
        var details = new System.Text.StringBuilder();
        details.AppendLine($"ID: {dto.Id}");
        details.AppendLine($"Bác sĩ: {dto.DoctorName}");
        details.AppendLine($"Chuyên khoa: {dto.SpecialtyName}");
        details.AppendLine($"Phòng: {dto.ClinicRoom}");
        details.AppendLine($"Thời gian: {dto.TimeLabel}");
        details.AppendLine($"Thời lượng: {dto.DurationMinutes} phút");
        details.AppendLine($"Bệnh nhân: {dto.PatientName}");
        details.AppendLine($"SĐT: {dto.CustomerPhone}");
        details.AppendLine($"Trạng thái: {dto.StatusLabel}");
        details.AppendLine($"Giá: {dto.Price}");

        _dialogService.ShowInfo(details.ToString(), "Thông tin cuộc hẹn");
    }
}
