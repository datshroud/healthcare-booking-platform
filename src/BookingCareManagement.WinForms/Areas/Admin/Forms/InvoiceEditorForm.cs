using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BookingCareManagement.WinForms.Areas.Admin.Services;
using BookingCareManagement.WinForms.Shared.Models.Dtos;
using BookingCareManagement.WinForms.Shared.Services;
using System.IO;
using System.Text.Json;
using System.Diagnostics;

namespace BookingCareManagement.WinForms.Areas.Admin.Forms
{
    public sealed partial class InvoiceEditorForm : Form
    {
        private readonly List<CheckedListBox> _filterDropdowns = new();
        private readonly AdminInvoiceApiClient _invoiceApiClient;
        private readonly DialogService _dialogService;
        private List<InvoiceDto> _invoiceCache = new();

        public InvoiceEditorForm(AdminInvoiceApiClient invoiceApiClient, DialogService dialogService)
        {
            _invoiceApiClient = invoiceApiClient;
            _dialogService = dialogService;
            
            InitializeComponent();
            InitializeGridColumns();
            ApplyGridStyling();
            // InitializeFilterDropdowns() will be called after invoices are loaded
        }

        private async void InvoiceEditorForm_Load(object sender, EventArgs e)
        {
            await LoadInvoiceDataAsync();
        }

        private async Task LoadInvoiceDataAsync()
        {
            try
            {
                // Show loading state in UI while fetching
                lblTitle.Text = "Hóa đơn (Đang tải...)";

                _invoiceCache = (await _invoiceApiClient.GetAllAsync()).ToList();

                // Display and initialize filters
                DisplayInvoices(_invoiceCache);
                InitializeFilterDropdowns();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoadInvoiceDataAsync error: {ex}");
                _dialogService.ShowError($"Không thể tải danh sách hóa đơn: {ex.Message}\n{ex.StackTrace}");
                UpdateInvoiceCount(0, true);
            }
        }

        private string TryReadApiBaseUrl()
        {
            try
            {
                var cfgPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
                if (!File.Exists(cfgPath)) return "(appsettings.json not found)";

                var text = File.ReadAllText(cfgPath);
                using var doc = JsonDocument.Parse(text);
                if (doc.RootElement.TryGetProperty("Api", out var apiElem) && apiElem.TryGetProperty("BaseUrl", out var url))
                {
                    return url.GetString() ?? "(empty)";
                }

                return "(Api:BaseUrl not found)";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"TryReadApiBaseUrl error: {ex}");
                return "(error reading appsettings)";
            }
        }

        private void DisplayInvoices(List<InvoiceDto> invoices)
        {
            invoiceGrid.Rows.Clear();
            foreach (var invoice in invoices.OrderByDescending(i => i.InvoiceNumber))
            {
                var status = invoice.Status.Equals("Paid", StringComparison.OrdinalIgnoreCase) 
                    ? "Đã thanh toán" 
                    : "Đang chờ";
                
                invoiceGrid.Rows.Add(
                    invoice.InvoiceNumber,
                    invoice.CustomerName,
                    invoice.InvoiceDate.ToString("dd/MM/yyyy"),
                    $"📋 {invoice.ServiceName}",
                    status,
                    $"{invoice.Total:N0} ₫"
                );
                
                // Lưu Id vào Tag của row để sử dụng sau
                invoiceGrid.Rows[invoiceGrid.Rows.Count - 1].Tag = invoice.Id;
            }
            
            UpdateInvoiceCount(invoices.Count);
        }

        private void UpdateInvoiceCount(int count, bool isError = false)
        {
            if (isError)
            {
                lblTitle.Text = "Hóa đơn (Lỗi)";
            }
            else
            {
                lblTitle.Text = $"Hóa đơn ({count})";
            }
        }

        private void InitializeGridColumns()
        {
            invoiceGrid.Columns.Clear();

            // Các cột tiếng Việt
            invoiceGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "InvoiceNo",
                HeaderText = "Số HĐ",
                FillWeight = 10
            });
            invoiceGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Customer",
                HeaderText = "Khách hàng",
                FillWeight = 25
            });
            invoiceGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "InvoiceDate",
                HeaderText = "Ngày lập",
                FillWeight = 15
            });
            invoiceGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Service",
                HeaderText = "Dịch vụ",
                FillWeight = 20
            });
            invoiceGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Status",
                HeaderText = "Trạng thái",
                FillWeight = 15
            });
            invoiceGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Total",
                HeaderText = "Tổng tiền",
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
            invoiceGrid.CellContentClick += InvoiceGrid_CellContentClick;
        }

        private async void InvoiceGrid_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // Kiểm tra nếu click vào cột Action
            if (invoiceGrid.Columns[e.ColumnIndex].Name == "Action")
            {
                var row = invoiceGrid.Rows[e.RowIndex];
                var invoiceId = (Guid)row.Tag;
                var invoice = _invoiceCache.FirstOrDefault(i => i.Id == invoiceId);
                
                if (invoice == null) return;

                var contextMenu = new ContextMenuStrip();
                
                if (!invoice.Status.Equals("Paid", StringComparison.OrdinalIgnoreCase))
                {
                    contextMenu.Items.Add("✅ Đánh dấu đã thanh toán", null, async (s, args) => 
                    {
                        await MarkAsPaidAsync(invoice);
                    });
                }
                
                contextMenu.Items.Add("📄 Tải PDF", null, async (s, args) => 
                {
                    await DownloadPdfAsync(invoice);
                });
                
                contextMenu.Items.Add("👁️ Xem chi tiết", null, (s, args) => 
                {
                    ShowInvoiceDetails(invoice);
                });

                var cellRect = invoiceGrid.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
                var point = invoiceGrid.PointToScreen(new Point(cellRect.Left, cellRect.Bottom));
                contextMenu.Show(point);
            }
        }

        private async Task MarkAsPaidAsync(InvoiceDto invoice)
        {
            if (!_dialogService.Confirm($"Đánh dấu hóa đơn #{invoice.InvoiceNumber} là đã thanh toán?"))
                return;

            try
            {
                await _invoiceApiClient.MarkAsPaidAsync(invoice.Id);
                _dialogService.ShowInfo("Đã cập nhật trạng thái thanh toán.");
                await LoadInvoiceDataAsync();
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Không thể cập nhật: {ex.Message}");
            }
        }

        private async Task DownloadPdfAsync(InvoiceDto invoice)
        {
            try
            {
                // Mở form report thay vì download trực tiếp
                var reportForm = new InvoiceReportForm(invoice);
                reportForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Không thể mở report: {ex.Message}");
            }
        }

        private void ShowInvoiceDetails(InvoiceDto invoice)
        {
            var details = $"Hóa đơn #{invoice.InvoiceNumber}\n\n" +
                         $"Khách hàng: {invoice.CustomerName}\n" +
                         $"Email: {invoice.CustomerEmail}\n" +
                         $"Dịch vụ: {invoice.ServiceName}\n" +
                         $"Ngày: {invoice.InvoiceDate:dd/MM/yyyy}\n" +
                         $"Tổng tiền: {invoice.Total:N0} ₫\n" +
                         $"Trạng thái: {(invoice.Status.Equals("Paid", StringComparison.OrdinalIgnoreCase) ? "Đã thanh toán" : "Đang chờ")}";

            _dialogService.ShowInfo(details);
        }

        private void InvoiceGrid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (invoiceGrid.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                string status = e.Value.ToString() ?? "";

                if (status.Contains("Pending") || status.Contains("Đang chờ"))
                {
                    e.CellStyle.BackColor = Color.FromArgb(254, 243, 199); // Màu vàng nhạt
                    e.CellStyle.ForeColor = Color.FromArgb(133, 77, 14);
                    e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                    e.CellStyle.Padding = new Padding(8, 4, 8, 4);
                }
                else if (status.Contains("Paid") || status.Contains("Đã thanh toán"))
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
            // Clear any previously created dropdowns/panels
            foreach (var dd in _filterDropdowns.ToArray())
            {
                dd.Parent?.Dispose();
            }
            _filterDropdowns.Clear();

            // Tạo dropdown cho Customer
            var customerDropdown = CreateFilterDropdown(btnCustomerFilter, GetUniqueCustomers());

            // Tạo dropdown cho Service
            var serviceDropdown = CreateFilterDropdown(btnServiceFilter, GetUniqueServices());

            // Tạo dropdown cho Status
            var statusDropdown = CreateFilterDropdown(btnStatusFilter, new[]
            {
                "Đang chờ", "Đã thanh toán"
            });

            // Reset tất cả buttons về màu trắng ban đầu
            ResetFilterButtonStyle(btnCustomerFilter);
            ResetFilterButtonStyle(btnEmployeeFilter);
            ResetFilterButtonStyle(btnServiceFilter);
            ResetFilterButtonStyle(btnStatusFilter);

            // Wire search event
            txtSearch.TextChanged += (s, e) => ApplyFilters();
        }

        private string[] GetUniqueCustomers()
        {
            return _invoiceCache
                .Select(i => i.CustomerName)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .OrderBy(c => c)
                .ToArray();
        }

        private string[] GetUniqueServices()
        {
            return _invoiceCache
                .Select(i => i.ServiceName)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct()
                .OrderBy(s => s)
                .ToArray();
        }

        private void ApplyFilters()
        {
            var searchTerm = txtSearch.Text.Trim().ToLower();
            var filtered = _invoiceCache.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                filtered = filtered.Where(i =>
                    i.CustomerName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    i.ServiceName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    i.InvoiceNumber.ToString().Contains(searchTerm));
            }

            var filteredList = filtered.ToList();
            DisplayInvoices(filteredList);
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

            // Lưu dropdown để quản lý (không thêm 2 lần)
            _filterDropdowns.Add(dropdown);
            return dropdown;
        }

        private string GetBaseButtonText(string buttonText)
        {
            // Lấy text gốc không có số đếm
            var parts = buttonText.Split('(');
            return parts[0].Trim();
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