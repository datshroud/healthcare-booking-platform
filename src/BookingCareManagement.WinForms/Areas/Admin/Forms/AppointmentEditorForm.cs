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

            // Tạo dropdown cho Status - 5 trạng thái
            var statusDropdown = CreateFilterDropdown(btnStatusFilter, new[]
            {
                "Approved", "Pending", "Canceled", "Rejected", "No Show"
            });
            _filterDropdowns.Add(statusDropdown);

            // Reset tất cả buttons về màu trắng ban đầu
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

        private CheckedListBox CreateFilterDropdown(Button parentButton, string[] items)
        {
            // Sử dụng Panel làm container cho dropdown để tránh bị che
            var dropdownPanel = new Panel
            {
                Visible = false,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                AutoSize = false,
                Width = parentButton.Width + 100,
                Height = Math.Min(items.Length * 25 + 50, 220),
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

            // Thêm items
            dropdown.Items.AddRange(items);

            // Thêm controls vào panel
            dropdownPanel.Controls.Add(dropdown);
            dropdownPanel.Controls.Add(searchBox);

            // Xử lý tìm kiếm
            searchBox.TextChanged += (s, e) =>
            {
                var searchText = searchBox.Text.ToLower();
                dropdown.Items.Clear();
                
                var filteredItems = items.Where(item => 
                    item.ToLower().Contains(searchText)).ToArray();
                
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
                    var checkedCount = dropdown.CheckedItems.Count;
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
                });
            };

            // Thêm panel vào Form (không phải filterPanel)
            this.Controls.Add(dropdownPanel);
            dropdownPanel.BringToFront();
            
            _filterDropdowns.Add(dropdown);
            return dropdown;
        }

        private string GetBaseButtonText(string buttonText)
        {
            // Lấy text gốc không có số đếm
            var parts = buttonText.Split('(');
            return parts[0].Trim();
        }

        private void LoadSampleData()
        {
            // Load demo data giống web
            appointmentGrid.Rows.Clear();
            appointmentGrid.Rows.Add(false, "4:00 pm", "Kiểm tra", ". Le Ngoc Bao Chan", "1h", "Chấp nhận", "C", "", "...");
            appointmentGrid.Rows.Add(false, "2:00 pm", "Kiểm tra", ". Le Ngoc Bao Chan", "1h", "Chấp nhận", "C", "", "...");

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
                    dropdown.Parent?.Hide();
                }
            }
        }
    }
}
