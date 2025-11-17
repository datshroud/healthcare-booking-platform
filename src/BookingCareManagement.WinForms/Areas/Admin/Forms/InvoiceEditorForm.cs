using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BookingCareManagement.WinForms.Areas.Admin.Forms
{
    public sealed partial class InvoiceEditorForm : Form
    {
        private readonly List<CheckedListBox> _filterDropdowns = new();

        public InvoiceEditorForm()
        {
            InitializeComponent();
            InitializeGridColumns();
            ApplyGridStyling();
            InitializeFilterDropdowns();
            LoadSampleData();
        }

        private void InvoicesEditorForm_Load(object sender, EventArgs e)
        {

        }

        private void InvoiceEditorForm_Load(object sender, EventArgs e)
        {

        }

        private void InitializeGridColumns()
        {
            invoiceGrid.Columns.Clear();

            // Các cột theo hình
            invoiceGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "InvoiceNo",
                HeaderText = "Invoice #",
                FillWeight = 10
            });
            invoiceGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Customer",
                HeaderText = "Customer",
                FillWeight = 25
            });
            invoiceGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "InvoiceDate",
                HeaderText = "Invoice date",
                FillWeight = 15
            });
            invoiceGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Service",
                HeaderText = "Service",
                FillWeight = 20
            });
            invoiceGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Status",
                HeaderText = "Status",
                FillWeight = 15
            });
            invoiceGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Total",
                HeaderText = "Total",
                FillWeight = 10
            });
            invoiceGrid.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "Action",
                HeaderText = "",
                Text = "⋯",
                UseColumnTextForButtonValue = true,
                FillWeight = 5
            });
        }

        private void ApplyGridStyling()
        {
            var vietnameseFont = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            var vietnameseFontBold = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);

            // Column header style
            invoiceGrid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = Color.FromArgb(107, 114, 128),
                Font = vietnameseFontBold,
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0)
            };

            // Default cell style
            invoiceGrid.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = Color.FromArgb(17, 24, 39),
                SelectionBackColor = Color.FromArgb(243, 244, 246),
                SelectionForeColor = Color.FromArgb(17, 24, 39),
                Padding = new Padding(15, 10, 0, 10),
                Font = vietnameseFont
            };

            // Alternating row style
            invoiceGrid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(249, 250, 251),
                Font = vietnameseFont
            };

            // Style cho cột Status - tô màu nền
            invoiceGrid.CellFormatting += InvoiceGrid_CellFormatting;
        }

        private void InvoiceGrid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (invoiceGrid.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                string status = e.Value.ToString() ?? "";

                if (status.Contains("Pending"))
                {
                    e.CellStyle.BackColor = Color.FromArgb(254, 243, 199); // Màu vàng nhạt
                    e.CellStyle.ForeColor = Color.FromArgb(133, 77, 14);
                    e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                    e.CellStyle.Padding = new Padding(8, 4, 8, 4);
                }
                else if (status.Contains("Paid in full"))
                {
                    e.CellStyle.BackColor = Color.FromArgb(209, 250, 229); // Màu xanh lá nhạt
                    e.CellStyle.ForeColor = Color.FromArgb(22, 101, 52);
                    e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                    e.CellStyle.Padding = new Padding(8, 4, 8, 4);
                }
            }
        }

        private void InitializeFilterDropdowns()
        {
            // Tạo dropdown cho Customer
            var customerDropdown = CreateFilterDropdown(btnCustomerFilter, new[]
            {
                ". Le Ngoc Bao Chan", "Jone Doe", "Nguyen Van A"
            });
            _filterDropdowns.Add(customerDropdown);

            // Tạo dropdown cho Employee
            var employeeDropdown = CreateFilterDropdown(btnEmployeeFilter, new[]
            {
                "Employee A", "Employee B", "Employee C"
            });
            _filterDropdowns.Add(employeeDropdown);

            // Tạo dropdown cho Service
            var serviceDropdown = CreateFilterDropdown(btnServiceFilter, new[]
            {
                "Check Up", "Nhổ răng", "Phẫu thuật nha khoa"
            });
            _filterDropdowns.Add(serviceDropdown);

            // Tạo dropdown cho Status
            var statusDropdown = CreateFilterDropdown(btnStatusFilter, new[]
            {
                "Pending", "Paid in full"
            });
            _filterDropdowns.Add(statusDropdown);

            // Reset tất cả buttons về màu trắng ban đầu
            ResetFilterButtonStyle(btnCustomerFilter);
            ResetFilterButtonStyle(btnEmployeeFilter);
            ResetFilterButtonStyle(btnServiceFilter);
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

            // Đặt vị trí dropdown
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
            // Load data mẫu theo hình
            invoiceGrid.Rows.Clear();
            invoiceGrid.Rows.Add("4", ". Le Ngoc Bao Chan", "November 1...", "📋 Check Up", "Pending", "$200.0");
            invoiceGrid.Rows.Add("3", ". Le Ngoc Bao Chan", "November 1...", "📋 Check Up", "Pending", "$200.0");
            invoiceGrid.Rows.Add("2", ". Le Ngoc Bao Chan", "November 1...", "📋 Check Up", "Paid in full", "$200.0");
            invoiceGrid.Rows.Add("1", "Jone Doe", "November 1...", "📋 Check Up", "Pending", "$200.0");

            // Update title
            lblTitle.Text = "Invoices";
        }

        private void btnFilter_Click(object sender, EventArgs e)
        {
            // Toggle hiển thị filter panel
            filterContainerPanel.Visible = !filterContainerPanel.Visible;

            // Cập nhật màu nút
            if (filterContainerPanel.Visible)
            {
                btnFilter.BackColor = Color.FromArgb(37, 99, 235);
                btnFilter.ForeColor = Color.White;
            }
            else
            {
                btnFilter.BackColor = Color.FromArgb(229, 231, 235);
                btnFilter.ForeColor = Color.FromArgb(55, 65, 81);

                // Ẩn tất cả dropdown khi đóng filter panel
                foreach (var dropdown in _filterDropdowns)
                {
                    dropdown.Parent?.Hide();
                }
            }
        }
    }
}