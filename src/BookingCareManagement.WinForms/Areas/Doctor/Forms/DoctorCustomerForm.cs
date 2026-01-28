using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using BookingCareManagement.WinForms.Areas.Admin.Services;
using BookingCareManagement.WinForms.Shared.Models.Dtos;

namespace BookingCareManagement.WinForms.Areas.Doctor.Forms;

public sealed class DoctorCustomerForm : Form
{
    private readonly CustomerService _customerService;
    private readonly List<CustomerDto> _allCustomers = new();
    private List<CustomerDto> _filteredCustomers = new();
    private List<CustomerDto> _displayedCustomers = new();
    private readonly ContextMenuStrip _actionMenu = new();
    private int _selectedRowIndex = -1;

    private Panel _whitePanel = null!;
    private Panel _pagerPanel = null!;
    private Button _btnPrevPage = null!;
    private Button _btnNextPage = null!;
    private ComboBox _pageSizeBox = null!;
    private Label _pageInfo = null!;
    private int _currentPage = 1;
    private int _pageSize = 7;

    private readonly DataGridView _grid = new()
    {
        Dock = DockStyle.Fill,
        AutoGenerateColumns = false,
        ReadOnly = true,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        AllowUserToAddRows = false,
        RowHeadersVisible = false,
        MultiSelect = false,
        BackgroundColor = Color.White
    };

    private readonly TextBox _searchBox = new() { PlaceholderText = "Tìm kiếm theo tên, email hoặc SĐT" };
    private readonly Button _refreshButton = new() { Text = "Tải lại" };
    private readonly Button _addButton = new() { Text = "Thêm" };
    private readonly Button _editButton = new() { Text = "Sửa" };
    private readonly Button _deleteButton = new() { Text = "Xóa" };
    private readonly Label _statusLabel = new() { AutoSize = true };
    private readonly Label _titleLabel = new() { AutoSize = true };

    public DoctorCustomerForm(CustomerService customerService)
    {
        _customerService = customerService;
        Text = "Quản lý khách hàng";
        Width = 1100;
        Height = 700;
        StartPosition = FormStartPosition.CenterParent;

        BuildLayout();
        InitializeActionMenu();
        ConfigureGrid();
        BuildPager();
        WireEvents();
        Shown += async (_, _) => await LoadCustomersAsync();
    }

    private void InitializeActionMenu()
    {
        _actionMenu.Items.Add("Chỉnh sửa", null, async (_, _) => await ShowEditorAsync(GetCustomerByRow(_selectedRowIndex)));
        _actionMenu.Items.Add("Xóa", null, async (_, _) => await DeleteSelectedAsync());
    }

    private void BuildLayout()
    {
        // Header panel (title + actions) inspired by Admin Customer form, keep original fields unchanged
        var headerPanel = new Panel
        {
            BackColor = Color.FromArgb(243, 244, 246),
            Dock = DockStyle.Top,
            Padding = new Padding(30, 20, 30, 20),
            Height = 80
        };

        // use field so we can update count dynamically
        _titleLabel.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
        _titleLabel.ForeColor = Color.FromArgb(17, 24, 39);
        _titleLabel.Location = new Point(30, 20);
        _titleLabel.Text = "Khách hàng (0)";

        // lightweight export button (visual) - local only
        var exportBtn = new Button
        {
            BackColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            FlatAppearance = { BorderColor = Color.FromArgb(209, 213, 219) },
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(55, 65, 81),
            Size = new Size(140, 44),
            Text = "⬇ Xuất khách hàng"
        };

        // style existing add button to match admin look but keep the instance
        _addButton.BackColor = Color.FromArgb(37, 99, 235);
        _addButton.FlatStyle = FlatStyle.Flat;
        _addButton.FlatAppearance.BorderSize = 0;
        _addButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _addButton.ForeColor = Color.White;
        _addButton.Size = new Size(180, 44);
        _addButton.Text = "+ Thêm khách hàng";

        // right side actions container
        var actionsPanel = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true };
        actionsPanel.Controls.Add(exportBtn);
        actionsPanel.Controls.Add(_addButton);
        actionsPanel.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        // Content + white panel
        var contentPanel = new Panel
        {
            BackColor = Color.FromArgb(243, 244, 246),
            Dock = DockStyle.Fill,
            Padding = new Padding(30, 10, 30, 50)
        };

        _whitePanel = new Panel
        {
            BackColor = Color.White,
            Dock = DockStyle.Fill,
            Padding = new Padding(30, 10, 30, 50)
        };

        // Search row at top of white panel
        var searchPanel = new Panel { Dock = DockStyle.Top, Height = 60 };
        var searchBoxPanel = new Panel
        {
            Anchor = AnchorStyles.Top | AnchorStyles.Left,
            BackColor = Color.FromArgb(249, 250, 251),
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(10, 6, 10, 6),
            Size = new Size(420, 44)
        };
        var searchIcon = new Label { AutoSize = true, Font = new Font("Segoe UI", 11F), ForeColor = Color.FromArgb(107, 114, 128), Text = "🔍", Location = new Point(8, 10) };
        _searchBox.BorderStyle = BorderStyle.None;
        _searchBox.BackColor = Color.FromArgb(249, 250, 251);
        _searchBox.Font = new Font("Segoe UI", 11F);
        _searchBox.Location = new Point(36, 8);
        _searchBox.Width = 360;

        searchBoxPanel.Controls.Add(searchIcon);
        searchBoxPanel.Controls.Add(_searchBox);
        searchPanel.Controls.Add(searchBoxPanel);

        // small action buttons near search (refresh, edit, delete) keep original buttons
        // enlarge buttons slightly so text is not clipped
        _refreshButton.Size = new Size(90, 30);
        _editButton.Size = new Size(90, 30);
        _deleteButton.Size = new Size(90, 30);
        _refreshButton.Font = new Font("Segoe UI", 9F);
        _editButton.Font = new Font("Segoe UI", 9F);
        _deleteButton.Font = new Font("Segoe UI", 9F);

        var smallButtons = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true, Location = new Point(470, 8) };
        smallButtons.Controls.AddRange(new Control[] { _refreshButton, _editButton, _deleteButton });
        searchPanel.Controls.Add(smallButtons);

        // add searchPanel and grid into whitePanel
        _whitePanel.Controls.Add(_grid);
        _whitePanel.Controls.Add(searchPanel);

        // footer status
        var footer = new Panel { Dock = DockStyle.Bottom, Height = 40, Padding = new Padding(12, 4, 12, 4) };
        _statusLabel.Font = new Font("Segoe UI", 10F);
        footer.Controls.Add(_statusLabel);

        // compose final layout
        headerPanel.Controls.Add(_titleLabel);
        headerPanel.Controls.Add(actionsPanel);
        contentPanel.Controls.Add(_whitePanel);
        Controls.Add(contentPanel);
        Controls.Add(headerPanel);
        Controls.Add(footer);
    }

    private void BuildPager()
    {
        _pagerPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 55,
            BackColor = Color.White,
            Padding = new Padding(18, 6, 18, 6)
        };

        var pagerInner = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true
        };

        _pageInfo = new Label { AutoSize = true, Text = "Trang 0 / 0", Padding = new Padding(0, 10, 6, 0) };
        _btnPrevPage = new Button { Text = "‹ Trước", AutoSize = true, Enabled = false, FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Color.Black };
        _btnNextPage = new Button { Text = "Tiếp ›", AutoSize = true, Enabled = false, FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Color.Black };
        _pageSizeBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 80 };
        _pageSizeBox.Items.AddRange(new object[] { "7", "10", "25", "50", "100" });
        _pageSizeBox.SelectedItem = _pageSize.ToString();

        _btnPrevPage.Click += (_, _) => { if (_currentPage > 1) { _currentPage--; RefreshGrid(); } };
        _btnNextPage.Click += (_, _) => { _currentPage++; RefreshGrid(); };
        _pageSizeBox.SelectedIndexChanged += (_, _) =>
        {
            if (int.TryParse(_pageSizeBox.SelectedItem?.ToString(), out var size))
            {
                _pageSize = size;
                _currentPage = 1;
                RefreshGrid();
            }
        };

        pagerInner.Controls.Add(_pageInfo);
        pagerInner.Controls.Add(new Label { Width = 8 });
        pagerInner.Controls.Add(_btnPrevPage);
        pagerInner.Controls.Add(_btnNextPage);
        pagerInner.Controls.Add(new Label { Width = 8 });
        pagerInner.Controls.Add(new Label { Text = "Hiển thị:", AutoSize = true, Padding = new Padding(6, 10, 0, 0) });
        pagerInner.Controls.Add(_pageSizeBox);

        _pagerPanel.Controls.Add(pagerInner);
        _whitePanel.Controls.Add(_pagerPanel);
        _pagerPanel.BringToFront();
    }

    private void ConfigureGrid()
    {
        _grid.Columns.Clear();
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Customer",
            HeaderText = "Khách hàng",
            FillWeight = 30,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Email",
            HeaderText = "Email",
            FillWeight = 20,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Appointments",
            HeaderText = "# Số cuộc hẹn",
            FillWeight = 12,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "LastAppointment",
            HeaderText = "# Cuộc hẹn cuối cùng",
            FillWeight = 18,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Created",
            HeaderText = "# Ngày tạo tài khoản",
            FillWeight = 15,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });
        _grid.Columns.Add(new DataGridViewButtonColumn
        {
            Name = "Actions",
            HeaderText = "",
            Text = "⋯",
            UseColumnTextForButtonValue = true,
            FillWeight = 6
        });

        // styling inspired by Admin Customer form
        var headerStyle = new DataGridViewCellStyle
        {
            Alignment = DataGridViewContentAlignment.MiddleLeft,
            BackColor = Color.FromArgb(236, 236, 236),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(60, 60, 60),
            Padding = new Padding(10, 0, 0, 0)
        };
        _grid.ColumnHeadersDefaultCellStyle = headerStyle;
        _grid.ColumnHeadersHeight = 50;
        _grid.EnableHeadersVisualStyles = false;

        _grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.White,
            Font = new Font("Segoe UI", 12F),
            SelectionBackColor = Color.FromArgb(243, 244, 246),
            SelectionForeColor = Color.FromArgb(15, 23, 42),
            WrapMode = DataGridViewTriState.False
        };

        _grid.RowsDefaultCellStyle = new DataGridViewCellStyle
        {
            SelectionBackColor = Color.FromArgb(243, 244, 246),
            SelectionForeColor = Color.FromArgb(15, 23, 42)
        };

        _grid.GridColor = Color.FromArgb(200, 200, 200);
        _grid.RowTemplate.Height = 70;
        if (_grid.Columns["Actions"] != null)
        {
            _grid.Columns["Actions"].ReadOnly = false;
        }
        UpdateTitleCount();
    }

    private void WireEvents()
    {
        _refreshButton.Click += async (_, _) => await LoadCustomersAsync();
        _addButton.Click += async (_, _) => await ShowEditorAsync();
        _editButton.Click += async (_, _) => await ShowEditorAsync(GetSelectedCustomer());
        _deleteButton.Click += async (_, _) => await DeleteSelectedAsync();
        _searchBox.TextChanged += (_, _) => ApplyFilter();
        _grid.CellClick += Grid_CellClick;
        _grid.CellPainting += Grid_CellPainting;
    }

    private void Grid_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;
        if (_grid.Columns[e.ColumnIndex].Name != "Actions") return;

        _selectedRowIndex = e.RowIndex;
        _grid.CurrentCell = _grid.Rows[e.RowIndex].Cells[e.ColumnIndex];
        _grid.Rows[e.RowIndex].Selected = true;
        var rect = _grid.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
        var pos = _grid.PointToScreen(new Point(rect.Right, rect.Top));
        _actionMenu.Show(pos);
    }

    private void Grid_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.ColumnIndex == 0 && e.RowIndex >= 0)
        {
            e.PaintBackground(e.CellBounds, true);

            if (e.RowIndex < _displayedCustomers.Count)
            {
                var customer = _displayedCustomers[e.RowIndex];
                bool isSelected = (_grid.Rows[e.RowIndex].Selected) || (e.State & DataGridViewElementStates.Selected) != 0;
                DrawCustomerCell(e, customer, isSelected);
            }

            e.Handled = true;
        }
    }

    private void DrawCustomerCell(DataGridViewCellPaintingEventArgs e, CustomerDto customer, bool isSelected)
    {
        DrawAvatar(e, customer);
        DrawCustomerInfo(e, customer, isSelected);
    }

    private void DrawAvatar(DataGridViewCellPaintingEventArgs e, CustomerDto customer)
    {
        if (!string.IsNullOrEmpty(customer.AvatarUrl))
        {
            DrawDefaultAvatar(e, customer.FullName);
        }
        else
        {
            DrawDefaultAvatar(e, customer.FullName);
        }
    }

    private void DrawDefaultAvatar(DataGridViewCellPaintingEventArgs e, string fullName)
    {
        using (var brush = new SolidBrush(Color.FromArgb(147, 197, 253)))
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.FillEllipse(brush, e.CellBounds.X + 15, e.CellBounds.Y + 10, 50, 50);
        }

        using (var font = new Font("Segoe UI", 12, FontStyle.Bold))
        using (var textBrush = new SolidBrush(Color.White))
        {
            var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            var avatarRect = new Rectangle(e.CellBounds.X + 15, e.CellBounds.Y + 10, 50, 50);
            string initials = GetInitials(fullName);
            e.Graphics.DrawString(initials, font, textBrush, avatarRect, sf);
        }
    }

    private void DrawCustomerInfo(DataGridViewCellPaintingEventArgs e, CustomerDto customer, bool isSelected)
    {
        var nameColor = isSelected ? Color.FromArgb(107, 114, 128) : Color.FromArgb(37, 99, 235);
        var emailColor = isSelected ? Color.FromArgb(156, 163, 175) : Color.FromArgb(107, 114, 128);

        using (var nameFont = new Font("Segoe UI", 11, FontStyle.Bold))
        using (var emailFont = new Font("Segoe UI", 9))
        using (var nameBrush = new SolidBrush(nameColor))
        using (var emailBrush = new SolidBrush(emailColor))
        {
            e.Graphics.DrawString(customer.FullName, nameFont, nameBrush,
                e.CellBounds.X + 75, e.CellBounds.Y + 18);
            e.Graphics.DrawString(customer.Email ?? string.Empty, emailFont, emailBrush,
                e.CellBounds.X + 75, e.CellBounds.Y + 40);
        }
    }

    private string GetInitials(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return "ND";

        string[] names = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (names.Length >= 2)
            return $"{names[0][0]}{names[1][0]}".ToUpper();
        if (names.Length == 1 && names[0].Length >= 2)
            return names[0].Substring(0, 2).ToUpper();
        return "ND";
    }

    private CustomerDto? GetSelectedCustomer()
    {
        return GetCustomerByRow(_grid.CurrentRow?.Index ?? -1);
    }

    private CustomerDto? GetCustomerByRow(int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= _displayedCustomers.Count)
        {
            return null;
        }

        return _displayedCustomers[rowIndex];
    }

    private async Task LoadCustomersAsync()
    {
        await RunBusyAsync(async () =>
        {
            var customers = await _customerService.GetForDoctorAsync();
            _allCustomers.Clear();
            _allCustomers.AddRange(customers.OrderByDescending(c => c.CreatedAt));
            _currentPage = 1;
            ApplyFilter();
        }, "Không thể tải danh sách khách hàng.");
    }

    private async Task ShowEditorAsync(CustomerDto? existing = null)
    {
        using var dialog = new DoctorCustomerEditorDialog(existing);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        await RunBusyAsync(async () =>
        {
            if (existing == null)
            {
                await _customerService.CreateAsync(dialog.BuildCreateRequest());
            }
            else
            {
                await _customerService.UpdateAsync(existing.Id, dialog.BuildUpdateRequest(existing.Id));
            }

            await LoadCustomersAsync();
        }, existing == null ? "Không thể thêm khách hàng." : "Không thể cập nhật khách hàng.");
    }

    private async Task DeleteSelectedAsync()
    {
        var selected = GetSelectedCustomer();
        if (selected == null)
        {
            MessageBox.Show(this, "Vui lòng chọn khách hàng cần xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (MessageBox.Show(this, "Bạn có chắc chắn muốn xóa khách hàng này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
        {
            return;
        }

        await RunBusyAsync(async () =>
        {
            await _customerService.DeleteAsync(selected.Id);
            _allCustomers.RemoveAll(c => c.Id == selected.Id);
            ApplyFilter();
        }, "Không thể xóa khách hàng.");
    }

    private async Task RunBusyAsync(Func<Task> action, string fallbackMessage)
    {
        try
        {
            ToggleControls(false);
            await action();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message ?? fallbackMessage, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            ToggleControls(true);
        }
    }

    private void ToggleControls(bool enabled)
    {
        _grid.Enabled = enabled;
        _searchBox.Enabled = enabled;
        _refreshButton.Enabled = enabled;
        _addButton.Enabled = enabled;
        _editButton.Enabled = enabled;
        _deleteButton.Enabled = enabled;
        if (_btnPrevPage != null) _btnPrevPage.Enabled = enabled && _btnPrevPage.Enabled;
        if (_btnNextPage != null) _btnNextPage.Enabled = enabled && _btnNextPage.Enabled;
        if (_pageSizeBox != null) _pageSizeBox.Enabled = enabled;
    }

    private void ApplyFilter()
    {
        var keyword = (_searchBox.Text ?? string.Empty).Trim();
        _filteredCustomers = _allCustomers.Where(c =>
            string.IsNullOrWhiteSpace(keyword)
            || (!string.IsNullOrWhiteSpace(c.FullName) && c.FullName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            || (!string.IsNullOrWhiteSpace(c.Email) && c.Email.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            || (!string.IsNullOrWhiteSpace(c.PhoneNumber) && c.PhoneNumber.Contains(keyword, StringComparison.OrdinalIgnoreCase))
        ).ToList();

        _currentPage = 1;
        RefreshGrid();
    }

    private void RefreshGrid()
    {
        _grid.Rows.Clear();
        if (_pageSize <= 0) _pageSize = 7;
        var totalItems = _filteredCustomers.Count;
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)_pageSize));
        if (_currentPage > totalPages) _currentPage = totalPages;
        if (_currentPage < 1) _currentPage = 1;

        var startIndex = (_currentPage - 1) * _pageSize;
        var pageItems = _filteredCustomers.Skip(startIndex).Take(_pageSize).ToList();
        _displayedCustomers = pageItems;

        foreach (var customer in pageItems)
        {
            _grid.Rows.Add(
                $"{customer.FullName}\n{customer.Email}",
                customer.Email,
                customer.AppointmentCount.ToString(),
                customer.LastAppointment?.ToString("dd/MM/yyyy HH:mm") ?? "Chưa có",
                customer.CreatedAt.ToString("dd/MM/yyyy"),
                "⋯");
        }

        _statusLabel.Text = $"Hiển thị {pageItems.Count}/{totalItems} khách hàng";
        _pageInfo.Text = $"Trang {_currentPage} / {totalPages}";
        _btnPrevPage.Enabled = _currentPage > 1;
        _btnNextPage.Enabled = _currentPage < totalPages;
        UpdateTitleCount();
    }

    private void UpdateTitleCount()
    {
        _titleLabel.Text = $"Khách hàng ({_filteredCustomers.Count})";
    }

    private sealed class DoctorCustomerEditorDialog : Form
    {
        private readonly TextBox _firstName = new();
        private readonly TextBox _lastName = new();
        private readonly TextBox _email = new();
        private readonly TextBox _phone = new();
        private readonly ComboBox _gender = new() { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly DateTimePicker _dob = new() { Format = DateTimePickerFormat.Short, ShowCheckBox = true };
        private readonly TextBox _note = new() { Multiline = true, Height = 70 };

        public DoctorCustomerEditorDialog(CustomerDto? existing)
        {
            Text = existing == null ? "Thêm khách hàng" : "Cập nhật khách hàng";
            Width = 520;
            Height = 430;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            BuildLayout();
            PopulateGender();
            BindExisting(existing);
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

            AddRow(layout, 0, "Tên", _firstName);
            AddRow(layout, 1, "Họ", _lastName);
            AddRow(layout, 2, "Email", _email);
            AddRow(layout, 3, "Số điện thoại", _phone);
            AddRow(layout, 4, "Giới tính", _gender);
            AddRow(layout, 5, "Ngày sinh", _dob);
            AddRow(layout, 6, "Ghi chú nội bộ", _note);

            var buttons = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Bottom,
                Padding = new Padding(12)
            };

            var save = new Button { Text = "Lưu", AutoSize = true };
            var cancel = new Button { Text = "Hủy", AutoSize = true, DialogResult = DialogResult.Cancel };
            save.Click += (_, _) =>
            {
                try
                {
                    ValidateInputs();
                    DialogResult = DialogResult.OK;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };

            buttons.Controls.Add(save);
            buttons.Controls.Add(cancel);
            Controls.Add(layout);
            Controls.Add(buttons);
            AcceptButton = save;
            CancelButton = cancel;
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

        private void PopulateGender()
        {
            _gender.Items.Clear();
            _gender.Items.AddRange(new object[] { "", "Nam", "Nữ", "Khác" });
        }

        private void BindExisting(CustomerDto? existing)
        {
            if (existing == null)
            {
                return;
            }

            var firstName = existing.FirstName?.Trim() ?? string.Empty;
            var lastName = existing.LastName?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(lastName))
            {
                var fullName = (existing.FullName ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(fullName))
                {
                    var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 1)
                    {
                        firstName = parts[0];
                    }
                    else
                    {
                        firstName = parts[^1];
                        lastName = string.Join(" ", parts.Take(parts.Length - 1));
                    }
                }
            }

            _firstName.Text = firstName;
            _lastName.Text = lastName;
            _email.Text = existing.Email ?? string.Empty;
            _phone.Text = existing.PhoneNumber;
            _gender.SelectedItem = existing.Gender ?? string.Empty;
            if (existing.DateOfBirth.HasValue)
            {
                _dob.Value = existing.DateOfBirth.Value;
                _dob.Checked = true;
            }
            _note.Text = existing.InternalNote ?? string.Empty;
        }

        private void ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(_firstName.Text))
            {
                throw new InvalidOperationException("Vui lòng nhập tên khách hàng.");
            }
            if (string.IsNullOrWhiteSpace(_lastName.Text))
            {
                throw new InvalidOperationException("Vui lòng nhập họ khách hàng.");
            }
            if (string.IsNullOrWhiteSpace(_email.Text))
            {
                throw new InvalidOperationException("Vui lòng nhập email khách hàng.");
            }
            if (!BookingCareManagement.WinForms.ValidationHelpers.IsValidPersonName(_firstName.Text))
            {
                throw new InvalidOperationException("Tên không hợp lệ (không chứa số hoặc ký tự đặc biệt).");
            }
            if (!BookingCareManagement.WinForms.ValidationHelpers.IsValidPersonName(_lastName.Text))
            {
                throw new InvalidOperationException("Họ không hợp lệ (không chứa số hoặc ký tự đặc biệt).");
            }
            if (!BookingCareManagement.WinForms.ValidationHelpers.IsValidEmail(_email.Text.Trim()))
            {
                throw new InvalidOperationException("Email không hợp lệ.");
            }

            var phone = _phone.Text.Trim();
            if (string.IsNullOrWhiteSpace(phone))
            {
                throw new InvalidOperationException("Vui lòng nhập số điện thoại.");
            }
            var digits = new string(phone.Where(char.IsDigit).ToArray());
            if (!digits.StartsWith("0", StringComparison.Ordinal) || (digits.Length != 10 && digits.Length != 11))
            {
                throw new InvalidOperationException("Số điện thoại không hợp lệ (bắt đầu bằng 0 và có 10 hoặc 11 chữ số).");
            }

            if (_dob.Checked)
            {
                var minDate = DateTime.Today.AddMonths(-3);
                if (_dob.Value.Date > minDate)
                {
                    throw new InvalidOperationException("Ngày sinh phải lớn hơn hoặc bằng 3 tháng tuổi.");
                }
            }
        }

        public CreateCustomerRequest BuildCreateRequest()
        {
            ValidateInputs();
            return new CreateCustomerRequest
            {
                FirstName = _firstName.Text.Trim(),
                LastName = _lastName.Text.Trim(),
                Email = _email.Text.Trim(),
                PhoneNumber = _phone.Text.Trim(),
                Gender = string.IsNullOrWhiteSpace(_gender.Text) ? null : _gender.Text,
                DateOfBirth = _dob.Checked ? _dob.Value.Date : null,
                InternalNote = string.IsNullOrWhiteSpace(_note.Text) ? null : _note.Text,
                SendWelcomeEmail = false
            };
        }

        public UpdateCustomerRequest BuildUpdateRequest(string id)
        {
            ValidateInputs();
            return new UpdateCustomerRequest
            {
                Id = id,
                FirstName = _firstName.Text.Trim(),
                LastName = _lastName.Text.Trim(),
                Email = _email.Text.Trim(),
                PhoneNumber = _phone.Text.Trim(),
                Gender = string.IsNullOrWhiteSpace(_gender.Text) ? null : _gender.Text,
                DateOfBirth = _dob.Checked ? _dob.Value.Date : null,
                InternalNote = string.IsNullOrWhiteSpace(_note.Text) ? null : _note.Text,
            };
        }
    }
}
