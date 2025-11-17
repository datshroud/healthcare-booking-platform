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
        
        public AppointmentEditorForm()
        {
            InitializeComponent();
            InitializeGridColumns();
            ApplyGridStyling();
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
            // Column header style
            appointmentGrid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = Color.FromArgb(107, 114, 128),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0)
            };

            // Default cell style
            appointmentGrid.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = Color.FromArgb(17, 24, 39),
                SelectionBackColor = Color.FromArgb(243, 244, 246),
                SelectionForeColor = Color.FromArgb(17, 24, 39),
                Padding = new Padding(15, 10, 0, 10),
                Font = new Font("Segoe UI", 10F)
            };

            // Alternating row style
            appointmentGrid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(249, 250, 251)
            };
        }

        private void InitializeFilterDropdowns()
        {
            // Tạo dropdown cho Service
            var serviceDropdown = CreateFilterDropdown(btnServiceFilter, new[]
            {
                "Kiểm tra", "Nhổ răng", "Phẫu thuật nha khoa"
            });
            _filterDropdowns.Add(serviceDropdown);

            // Tạo dropdown cho Customer
            var customerDropdown = CreateFilterDropdown(btnCustomerFilter, new[]
            {
                "Jone Doe", "Le Ngoc Bao Chan", "Nguyen Van A"
            });
            _filterDropdowns.Add(customerDropdown);

            // Tạo dropdown cho Employee
            var employeeDropdown = CreateFilterDropdown(btnEmployeeFilter, new[]
            {
                "Nhân viên A", "Nhân viên B", "Nhân viên C"
            });
            _filterDropdowns.Add(employeeDropdown);

            // Tạo dropdown cho Status
            var statusDropdown = CreateFilterDropdown(btnStatusFilter, new[]
            {
                "Approved", "Pending", "Canceled", "No Show"
            });
            _filterDropdowns.Add(statusDropdown);
        }

        private CheckedListBox CreateFilterDropdown(Button parentButton, string[] items)
        {
            var dropdown = new CheckedListBox
            {
                Visible = false,
                CheckOnClick = true,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 10F),
                Height = Math.Min(items.Length * 25 + 30, 200),
                Width = parentButton.Width + 100
            };

            // Thêm TextBox tìm kiếm vào đầu
            var searchBox = new TextBox
            {
                PlaceholderText = "Tìm kiếm...",
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9F),
                Width = dropdown.Width - 10,
                Location = new Point(5, 5)
            };

            // Thêm items
            dropdown.Items.AddRange(items);

            // Đặt vị trí dropdown dưới button
            parentButton.Click += (s, e) =>
            {
                // Ẩn tất cả dropdown khác
                foreach (var otherDropdown in _filterDropdowns)
                {
                    if (otherDropdown != dropdown)
                        otherDropdown.Visible = false;
                }

                dropdown.Visible = !dropdown.Visible;
                if (dropdown.Visible)
                {
                    var btnLocation = parentButton.PointToScreen(Point.Empty);
                    var formLocation = filterPanel.PointToScreen(Point.Empty);
                    dropdown.Location = new Point(
                        btnLocation.X - formLocation.X,
                        btnLocation.Y - formLocation.Y + parentButton.Height + 5
                    );
                    dropdown.BringToFront();
                }
            };

            // Xử lý khi check/uncheck item
            dropdown.ItemCheck += (s, e) =>
            {
                // Đếm số item đã chọn
                this.BeginInvoke((MethodInvoker)delegate
                {
                    var checkedCount = dropdown.CheckedItems.Count;
                    if (checkedCount > 0)
                    {
                        parentButton.Text = $"    {parentButton.Text.Trim().Split('(')[0].Trim()} ({checkedCount})";
                        parentButton.BackColor = Color.FromArgb(219, 234, 254);
                        parentButton.ForeColor = Color.FromArgb(37, 99, 235);
                        parentButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                    }
                    else
                    {
                        parentButton.Text = $"    {parentButton.Text.Trim().Split('(')[0].Trim()}";
                        parentButton.BackColor = Color.White;
                        parentButton.ForeColor = Color.Black;
                        parentButton.Font = new Font("Segoe UI", 10F);
                    }
                });
            };

            filterPanel.Controls.Add(dropdown);
            return dropdown;
        }

        private void LoadSampleData()
        {
            // Load demo data giống web
            appointmentGrid.Rows.Clear();
            appointmentGrid.Rows.Add(false, "4:00 pm", "Check Up", ". Le Ngoc Bao Chan", "1h", "Approved", "C", "", "...");
            appointmentGrid.Rows.Add(false, "2:00 pm", "Check Up", ". Le Ngoc Bao Chan", "1h", "Approved", "C", "", "...");

            // Update title với số lượng
            lblTitle.Text = $"Lịch Hẹn ({appointmentGrid.Rows.Count})";
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
                    dropdown.Visible = false;
                }
            }
        }
    }
}
