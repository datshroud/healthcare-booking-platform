using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;

namespace BookingCareManagement.WinForms.Areas.Admin.Forms
{
    public sealed partial class AppointmentEditorForm : Form
    {
        private readonly List<CheckedListBox> _filterDropdowns = new();
        private readonly Dictionary<CheckedListBox, List<string>> _filterOptions = new();
        private readonly Dictionary<CheckedListBox, Button> _dropdownButtons = new();
        private readonly Dictionary<CheckedListBox, Func<AppointmentRow, string>> _filterSelectors = new();

        private readonly List<AppointmentRow> _appointments = new();
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
            DateTime Start,
            string Specialty,
            string Patient,
            int DurationMinutes,
            string StatusCode,
            string StatusLabel,
            string Doctor,
            string Note);

        public AppointmentEditorForm()
        {
            InitializeComponent();
            InitializeGridColumns();
            ApplyGridStyling();
            ConfigureInputs();
            InitializeFilterDropdowns();
            LoadSampleData();
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

        private void LoadSampleData()
        {
            _appointments.Clear();

            var baseDate = DateTime.Today.AddDays(3);
            _appointments.AddRange(new[]
            {
                new AppointmentRow(baseDate.AddHours(9), "Khám tim mạch", "Nguyễn Văn A", 30, "pending", "Chờ xác nhận", "BS. Lê Ngọc Bảo Chân", "Mang theo kết quả xét nghiệm mới nhất"),
                new AppointmentRow(baseDate.AddHours(11), "Khám tổng quát", "Trần Thị B", 45, "approved", "Đã xác nhận", "BS. Cấn Văn Thắng", ""),
                new AppointmentRow(baseDate.AddDays(1).AddHours(14), "Nha khoa", "Phạm Minh C", 60, "approved", "Đã xác nhận", "BS. Lê Ngọc Bảo Chân", "Khám và tư vấn phục hình"),
                new AppointmentRow(baseDate.AddDays(1).AddHours(16), "Khám da liễu", "Lê Ngọc Bảo Chân", 30, "canceled", "Đã hủy", "BS. Lê Ngọc Bảo Chân", "Khách hàng báo bận"),
                new AppointmentRow(baseDate.AddDays(2).AddHours(10), "Tai mũi họng", "Đỗ Khánh D", 30, "rejected", "Bị từ chối", "BS. Cấn Văn Thắng", "Lịch trùng bác sĩ"),
                new AppointmentRow(baseDate.AddDays(2).AddHours(15), "Nội tổng quát", "Phạm Quỳnh E", 30, "noshow", "Vắng mặt", "BS. Lê Ngọc Bảo Chân", "Không liên lạc được"),
            });

            UpdateFilterOptionsFromData();
            RefreshGrid();
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

            dtFrom.ValueChanged += (_, _) => RefreshGrid();
            dtTo.ValueChanged += (_, _) => RefreshGrid();
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
    }
}
