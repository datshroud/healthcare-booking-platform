using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookingCareManagement.WinForms.Areas.Admin.Services;
using BookingCareManagement.WinForms.Shared.Models.Dtos;
using BookingCareManagement.WinForms.Shared.Services;

namespace BookingCareManagement.WinForms.Areas.Admin.Forms
{
    public sealed partial class AppointmentEditorForm : Form
    {
        private readonly AdminAppointmentsApiClient _appointmentsApiClient;
        private readonly DialogService _dialogService;

        private readonly List<CheckedListBox> _filterDropdowns = new();
        private readonly Dictionary<CheckedListBox, List<string>> _filterOptions = new();
        private readonly Dictionary<CheckedListBox, Button> _dropdownButtons = new();
        private readonly Dictionary<CheckedListBox, Func<AppointmentRow, string>> _filterSelectors = new();

        private readonly List<AppointmentRow> _appointments = new();
        private AdminAppointmentMetadataDto? _metadata;
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

        private sealed record AppointmentRow(
            Guid Id,
            Guid DoctorId,
            Guid SpecialtyId,
            DateTime Start,
            string Specialty,
            string Patient,
            int DurationMinutes,
            string StatusCode,
            string StatusLabel,
            string Doctor,
            string Note,
            string Phone);

        public AppointmentEditorForm(DialogService dialogService, AdminAppointmentsApiClient appointmentsApiClient)
        {
            _dialogService = dialogService;
            _appointmentsApiClient = appointmentsApiClient;
            InitializeComponent();
            InitializeGridColumns();
            ApplyGridStyling();
            ConfigureInputs();
            InitializeFilterDropdowns();
            ConfigureActions();
            Shown += async (_, _) => await LoadAppointmentsAsync();
        }

        private void InitializeGridColumns()
        {
            appointmentGrid.Columns.Clear();

            // Checkbox column
            appointmentGrid.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "Select",
                HeaderText = "",
                FillWeight = 5
            });

            // Other columns
            appointmentGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Time", HeaderText = "Thời Gian", FillWeight = 10 });
            appointmentGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Service", HeaderText = "Dịch Vụ", FillWeight = 15 });
            appointmentGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Customer", HeaderText = "Khách Hàng", FillWeight = 25 });
            appointmentGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Duration", HeaderText = "Thời Lượng", FillWeight = 10 });
            appointmentGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Trạng Thái", FillWeight = 15 });
            appointmentGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Employee", HeaderText = "Nhân Viên", FillWeight = 10 });
            appointmentGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Note", HeaderText = "Ghi Chú", FillWeight = 10 });
            appointmentGrid.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "Action",
                HeaderText = "",
                Text = "...",
                UseColumnTextForButtonValue = true,
                FillWeight = 5
            });
        }

        private void ApplyGridStyling()
        {
            // Sử dụng font hỗ trợ tiếng Việt tốt hơn
            var vietnameseFont = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            var vietnameseFontBold = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);

            // Column header style
            appointmentGrid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = Color.FromArgb(107, 114, 128),
                Font = vietnameseFontBold,
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0)
            };

            appointmentGrid.EnableHeadersVisualStyles = false;

            // Default cell style
            appointmentGrid.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = Color.FromArgb(17, 24, 39),
                SelectionBackColor = Color.FromArgb(243, 244, 246),
                SelectionForeColor = Color.FromArgb(17, 24, 39),
                Padding = new Padding(15, 10, 0, 10),
                Font = vietnameseFont
            };

            // Alternating row style
            appointmentGrid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(249, 250, 251),
                Font = vietnameseFont
            };
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
        }

        private void ResetFilterButtonStyle(Button button)
        {
            button.BackColor = Color.White;
            button.ForeColor = Color.Black;
            button.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
        }

        private CheckedListBox CreateFilterDropdown(Button parentButton, Func<AppointmentRow, string> selector)
        {
            // Sử dụng Panel làm container cho dropdown để tránh bị che
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

            // TextBox tìm kiếm
            var searchBox = new TextBox
            {
                PlaceholderText = "Tìm kiếm...",
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point),
                Dock = DockStyle.Top,
                Height = 25
            };

            // CheckedListBox với font hỗ trợ tiếng Việt
            var dropdown = new CheckedListBox
            {
                CheckOnClick = true,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
                Dock = DockStyle.Fill,
                IntegralHeight = false
            };

            // Lưu selector và options cho dropdown
            _filterOptions[dropdown] = new List<string>();
            _filterSelectors[dropdown] = selector;
            _dropdownButtons[dropdown] = parentButton;

            // Thêm controls vào panel
            dropdownPanel.Controls.Add(dropdown);
            dropdownPanel.Controls.Add(searchBox);

            // Xử lý tìm kiếm
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

            // Đặt vị trí dropdown - QUAN TRỌNG: Thêm vào Form thay vì filterPanel
            parentButton.Click += (s, e) =>
            {
                // Ẩn tất cả dropdown khác
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
                    // Tính toán vị trí tuyệt đối trên form
                    var btnLocationInForm = this.PointToClient(parentButton.PointToScreen(Point.Empty));
                    
                    dropdownPanel.Location = new Point(
                        btnLocationInForm.X,
                        btnLocationInForm.Y + parentButton.Height + 5
                    );
                    
                    dropdownPanel.BringToFront();
                }
            };

            // Xử lý khi check/uncheck item
            dropdown.ItemCheck += (s, e) =>
            {
                // Đếm số item đã chọn
                this.BeginInvoke((MethodInvoker)delegate
                {
                    UpdateFilterButtonVisual(parentButton, dropdown.CheckedItems.Count);
                    RefreshGrid();
                });
            };

            // Thêm panel vào Form (không phải filterPanel)
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
            // Lấy text gốc không có số đếm
            var parts = buttonText.Split('(');
            return parts[0].Trim();
        }

        private void ConfigureActions()
        {
            btnNew.Click += async (_, _) => await ShowUpsertDialogAsync();
            btnExport.Click += (_, _) => _dialogService.ShowInfo("Chức năng xuất dữ liệu sẽ sớm có mặt.");
            appointmentGrid.CellDoubleClick += async (_, args) =>
            {
                if (args.RowIndex >= 0 && args.RowIndex < appointmentGrid.Rows.Count)
                {
                    await ShowUpsertDialogAsync(GetRowAtIndex(args.RowIndex));
                }
            };

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Sửa", null, async (_, _) => await ShowUpsertDialogAsync(GetSelectedRow()));
            contextMenu.Items.Add("Xóa", null, async (_, _) => await DeleteSelectedAsync());
            appointmentGrid.ContextMenuStrip = contextMenu;
            appointmentGrid.CellContentClick += async (_, args) =>
            {
                if (args.ColumnIndex == appointmentGrid.Columns["Action"].Index && args.RowIndex >= 0)
                {
                    await ShowUpsertDialogAsync(GetRowAtIndex(args.RowIndex));
                }
            };
        }

        private void ConfigureInputs()
        {
            txtSearch.TextChanged += (_, _) => RefreshGrid();

            dtFrom.ShowCheckBox = true;
            dtTo.ShowCheckBox = true;
            dtFrom.Value = DateTime.Today.AddDays(-7);
            dtTo.Value = DateTime.Today.AddDays(7);
            dtFrom.Checked = false;
            dtTo.Checked = false;

            dtFrom.ValueChanged += async (_, _) => await LoadAppointmentsAsync();
            dtTo.ValueChanged += async (_, _) => await LoadAppointmentsAsync();
        }

        private void btnFilter_Click(object sender, EventArgs e)
        {
            // Toggle hiển thị filter panel
            filterContainerPanel.Visible = !filterContainerPanel.Visible;
            
            // Cập nhật vị trí grid
            if (filterContainerPanel.Visible)
            {
                btnFilter.BackColor = Color.FromArgb(37, 99, 235);
                btnFilter.ForeColor = Color.White;
            }
            else
            {
                btnFilter.BackColor = Color.FromArgb(229, 231, 235);
                btnFilter.ForeColor = Color.Black;
                
                // Ẩn tất cả dropdown khi đóng filter panel
                foreach (var dropdown in _filterDropdowns)
                {
                    dropdown.Parent?.Hide();
                }
            }
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

            var search = txtSearch.Text.Trim().ToLower();
            if (!string.IsNullOrWhiteSpace(search))
            {
                data = data.Where(a =>
                    a.Specialty.ToLower().Contains(search)
                    || a.Patient.ToLower().Contains(search)
                    || a.Doctor.ToLower().Contains(search)
                    || a.Note.ToLower().Contains(search));
            }

            data = ApplyDropdownFilter(_serviceDropdown, data);
            data = ApplyDropdownFilter(_customerDropdown, data);
            data = ApplyDropdownFilter(_employeeDropdown, data);
            data = ApplyDropdownFilter(_statusDropdown, data);

            var rows = data.OrderBy(a => a.Start).ToList();

            appointmentGrid.Rows.Clear();
            for (var i = 0; i < rows.Count; i++)
            {
                RenderRow(rows[i]);
            }

            lblTitle.Text = $"Lịch Hẹn ({rows.Count})";
            appointmentGrid.Visible = rows.Count > 0;
            emptyStatePanel.Visible = rows.Count == 0;
        }

        private async Task LoadAppointmentsAsync()
        {
            try
            {
                ToggleLoadingState(true);
                _metadata ??= await _appointmentsApiClient.GetMetadataAsync();

                var from = dtFrom.Checked ? DateOnly.FromDateTime(dtFrom.Value.Date) : (DateOnly?)null;
                var to = dtTo.Checked ? DateOnly.FromDateTime(dtTo.Value.Date) : (DateOnly?)null;
                var appointments = await _appointmentsApiClient.GetAppointmentsAsync(from, to);

                _appointments.Clear();
                _appointments.AddRange(appointments.Select(ToRow).OrderBy(a => a.Start));

                UpdateFilterOptionsFromData();
                RefreshGrid();
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Không tải được dữ liệu cuộc hẹn: {ex.Message}");
                _appointments.Clear();
                RefreshGrid();
            }
            finally
            {
                ToggleLoadingState(false);
            }
        }

        private void ToggleLoadingState(bool isLoading)
        {
            appointmentGrid.Enabled = !isLoading;
            btnNew.Enabled = !isLoading;
            btnFilter.Enabled = !isLoading;
            btnServiceFilter.Enabled = !isLoading;
            btnCustomerFilter.Enabled = !isLoading;
            btnEmployeeFilter.Enabled = !isLoading;
            btnStatusFilter.Enabled = !isLoading;
            lblTitle.Text = isLoading ? "Đang tải dữ liệu cuộc hẹn..." : lblTitle.Text;
        }

        private AppointmentRow ToRow(DoctorAppointmentListItemDto dto)
        {
            var startLocal = DateTime.SpecifyKind(dto.StartUtc, DateTimeKind.Utc).ToLocalTime();
            var duration = dto.DurationMinutes;
            return new AppointmentRow(
                dto.Id,
                dto.DoctorId,
                dto.SpecialtyId,
                startLocal,
                dto.SpecialtyName,
                dto.PatientName,
                duration,
                dto.Status,
                dto.StatusLabel,
                dto.DoctorName,
                dto.ClinicRoom ?? string.Empty,
                dto.CustomerPhone);
        }

        private AppointmentRow? GetSelectedRow()
        {
            if (appointmentGrid.CurrentRow?.Index is { } idx && idx >= 0)
            {
                return GetRowAtIndex(idx);
            }

            return null;
        }

        private AppointmentRow? GetRowAtIndex(int index)
        {
            if (index < 0 || index >= appointmentGrid.Rows.Count)
            {
                return null;
            }

            var row = appointmentGrid.Rows[index];
            if (row.Cells["Action"].Tag is AppointmentRow appointment)
            {
                return appointment;
            }

            return null;
        }

        private async Task ShowUpsertDialogAsync(AppointmentRow? existing = null)
        {
            try
            {
                _metadata ??= await _appointmentsApiClient.GetMetadataAsync();
                using var dialog = new AppointmentUpsertDialog(_metadata, existing);
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                var payload = dialog.BuildRequest();

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

        private void RenderRow(AppointmentRow row)
        {
            var durationText = row.DurationMinutes switch
            {
                >= 60 when row.DurationMinutes % 60 == 0 => $"{row.DurationMinutes / 60} giờ",
                >= 60 => $"{row.DurationMinutes / 60} giờ {row.DurationMinutes % 60} phút",
                _ => $"{row.DurationMinutes} phút"
            };

            var formattedTime = row.Start.ToString("HH:mm - dd/MM");

            var index = appointmentGrid.Rows.Add(false, formattedTime, row.Specialty, row.Patient, durationText, row.StatusLabel, row.Doctor, row.Note, "•••");
            var statusCell = appointmentGrid.Rows[index].Cells["Status"];

            appointmentGrid.Rows[index].Cells["Action"].Tag = row;

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

        private sealed class AppointmentUpsertDialog : Form
        {
            private readonly ComboBox _doctorBox = new() { DropDownStyle = ComboBoxStyle.DropDownList };
            private readonly ComboBox _specialtyBox = new() { DropDownStyle = ComboBoxStyle.DropDownList };
            private readonly DateTimePicker _startPicker = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy HH:mm" };
            private readonly NumericUpDown _durationBox = new() { Minimum = 15, Maximum = 240, Increment = 5, Value = 30 };
            private readonly TextBox _patientName = new();
            private readonly TextBox _phone = new();
            private readonly ComboBox _statusBox = new() { DropDownStyle = ComboBoxStyle.DropDownList };
            private readonly AppointmentRow? _existing;
            private readonly AdminAppointmentMetadataDto _metadata;

            public AppointmentUpsertDialog(AdminAppointmentMetadataDto metadata, AppointmentRow? existing)
            {
                _metadata = metadata;
                _existing = existing;

                Text = existing == null ? "Thêm cuộc hẹn" : "Cập nhật cuộc hẹn";
                Width = 420;
                Height = 360;
                StartPosition = FormStartPosition.CenterParent;
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = false;

                BuildLayout();
                PopulateOptions();
                BindExisting();
            }

            public AdminAppointmentUpsertRequest BuildRequest()
            {
                return new AdminAppointmentUpsertRequest
                {
                    DoctorId = (Guid)_doctorBox.SelectedValue!,
                    SpecialtyId = (Guid)_specialtyBox.SelectedValue!,
                    SlotStartUtc = DateTime.SpecifyKind(_startPicker.Value, DateTimeKind.Local).ToUniversalTime(),
                    DurationMinutes = (int)_durationBox.Value,
                    PatientName = _patientName.Text.Trim(),
                    CustomerPhone = _phone.Text.Trim(),
                    Status = _statusBox.SelectedValue?.ToString() ?? "pending"
                };
            }

            private void BuildLayout()
            {
                var layout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 6,
                    Padding = new Padding(12),
                    AutoSize = true
                };

                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));

                AddRow(layout, 0, "Bác sĩ", _doctorBox);
                AddRow(layout, 1, "Chuyên khoa", _specialtyBox);
                AddRow(layout, 2, "Bắt đầu", _startPicker);
                AddRow(layout, 3, "Thời lượng (phút)", _durationBox);
                AddRow(layout, 4, "Bệnh nhân", _patientName);
                AddRow(layout, 5, "SĐT", _phone);

                if (_metadata.Statuses.Any())
                {
                    layout.RowCount += 1;
                    layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                    AddRow(layout, 6, "Trạng thái", _statusBox);
                }

                var buttonPanel = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.RightToLeft,
                    Dock = DockStyle.Bottom,
                    Padding = new Padding(12)
                };

                var saveButton = new Button { Text = "Lưu", DialogResult = DialogResult.OK, AutoSize = true };
                var cancelButton = new Button { Text = "Hủy", DialogResult = DialogResult.Cancel, AutoSize = true };
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

            private void PopulateOptions()
            {
                _doctorBox.DataSource = _metadata.Doctors.ToList();
                _doctorBox.DisplayMember = nameof(AdminAppointmentDoctorOptionDto.Name);
                _doctorBox.ValueMember = nameof(AdminAppointmentDoctorOptionDto.Id);

                _specialtyBox.DataSource = _metadata.Specialties.ToList();
                _specialtyBox.DisplayMember = nameof(AdminAppointmentSpecialtyOptionDto.Name);
                _specialtyBox.ValueMember = nameof(AdminAppointmentSpecialtyOptionDto.Id);

                _statusBox.DataSource = _metadata.Statuses.ToList();
                _statusBox.DisplayMember = nameof(AdminAppointmentStatusOptionDto.Label);
                _statusBox.ValueMember = nameof(AdminAppointmentStatusOptionDto.Code);
            }

            private void BindExisting()
            {
                if (_existing == null)
                {
                    return;
                }

                _doctorBox.SelectedValue = _existing.DoctorId;
                _specialtyBox.SelectedValue = _existing.SpecialtyId;
                _startPicker.Value = _existing.Start;
                _durationBox.Value = _existing.DurationMinutes;
                _patientName.Text = _existing.Patient;
                _phone.Text = _existing.Phone;
                if (!string.IsNullOrWhiteSpace(_existing.StatusCode))
                {
                    _statusBox.SelectedValue = _existing.StatusCode;
                }
            }
        }
    }
}
