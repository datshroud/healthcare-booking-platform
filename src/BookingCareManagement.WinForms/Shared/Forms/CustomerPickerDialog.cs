using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BookingCareManagement.WinForms.Shared.Models.Dtos;

namespace BookingCareManagement.WinForms.Shared.Forms;

public sealed class CustomerPickerDialog : Form
{
    private readonly Func<Task<IReadOnlyList<CustomerDto>>> _loader;
    private readonly List<CustomerDto> _allCustomers = new();
    private List<CustomerDto> _filteredCustomers = new();

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
    private readonly Label _statusLabel = new() { AutoSize = true };

    private Panel _pagerPanel = null!;
    private Button _btnPrev = null!;
    private Button _btnNext = null!;
    private ComboBox _pageSizeBox = null!;
    private Label _pageInfo = null!;

    private int _currentPage = 1;
    private int _pageSize = 10;

    public CustomerDto? SelectedCustomer { get; private set; }

    public CustomerPickerDialog(string title, Func<Task<IReadOnlyList<CustomerDto>>> loader)
    {
        _loader = loader ?? throw new ArgumentNullException(nameof(loader));
        Text = title;
        Width = 980;
        Height = 620;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        BuildLayout();
        ConfigureGrid();
        WireEvents();

        Shown += async (_, _) => await LoadCustomersAsync();
    }

    private void BuildLayout()
    {
        var headerPanel = new Panel
        {
            BackColor = Color.FromArgb(243, 244, 246),
            Dock = DockStyle.Top,
            Padding = new Padding(24, 16, 24, 12),
            Height = 70
        };

        var titleLabel = new Label
        {
            Text = "Chọn bệnh nhân",
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            ForeColor = Color.FromArgb(17, 24, 39),
            AutoSize = true,
            Location = new Point(24, 18)
        };

        headerPanel.Controls.Add(titleLabel);

        var contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24, 10, 24, 50),
            BackColor = Color.FromArgb(243, 244, 246)
        };

        var whitePanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(18, 10, 18, 10)
        };

        var searchPanel = new Panel { Dock = DockStyle.Top, Height = 56 };
        var searchBoxPanel = new Panel
        {
            Anchor = AnchorStyles.Top | AnchorStyles.Left,
            BackColor = Color.FromArgb(249, 250, 251),
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(10, 6, 10, 6),
            Size = new Size(420, 44)
        };
        var searchIcon = new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 11F),
            ForeColor = Color.FromArgb(107, 114, 128),
            Text = "🔍",
            Location = new Point(8, 10)
        };

        _searchBox.BorderStyle = BorderStyle.None;
        _searchBox.BackColor = Color.FromArgb(249, 250, 251);
        _searchBox.Font = new Font("Segoe UI", 11F);
        _searchBox.Location = new Point(36, 8);
        _searchBox.Width = 360;

        searchBoxPanel.Controls.Add(searchIcon);
        searchBoxPanel.Controls.Add(_searchBox);
        searchPanel.Controls.Add(searchBoxPanel);

        _refreshButton.Size = new Size(90, 30);
        _refreshButton.Font = new Font("Segoe UI", 9F);
        var actions = new FlowLayoutPanel { AutoSize = true, Location = new Point(470, 8) };
        actions.Controls.Add(_refreshButton);
        searchPanel.Controls.Add(actions);

        whitePanel.Controls.Add(_grid);
        whitePanel.Controls.Add(searchPanel);
        contentPanel.Controls.Add(whitePanel);

        _pagerPanel = new Panel { Dock = DockStyle.Bottom, Height = 55, Padding = new Padding(18, 6, 18, 6) };
        var pagerInner = new FlowLayoutPanel { Dock = DockStyle.Right, AutoSize = true, FlowDirection = FlowDirection.LeftToRight };

        _pageInfo = new Label { AutoSize = true, Text = "Trang 0 / 0", Padding = new Padding(0, 10, 6, 0) };
        _btnPrev = new Button { Text = "‹ Trước", AutoSize = true, Enabled = false, FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Color.Black };
        _btnNext = new Button { Text = "Tiếp ›", AutoSize = true, Enabled = false, FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Color.Black };
        _pageSizeBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 80 };
        _pageSizeBox.Items.AddRange(new object[] { "10", "25", "50", "100" });
        _pageSizeBox.SelectedItem = _pageSize.ToString();

        pagerInner.Controls.Add(_pageInfo);
        pagerInner.Controls.Add(new Label { Width = 8 });
        pagerInner.Controls.Add(_btnPrev);
        pagerInner.Controls.Add(_btnNext);
        pagerInner.Controls.Add(new Label { Width = 8 });
        pagerInner.Controls.Add(new Label { Text = "Hiển thị:", AutoSize = true, Padding = new Padding(6, 10, 0, 0) });
        pagerInner.Controls.Add(_pageSizeBox);
        _pagerPanel.Controls.Add(pagerInner);

        var footer = new Panel { Dock = DockStyle.Bottom, Height = 36, Padding = new Padding(18, 4, 18, 4) };
        _statusLabel.Font = new Font("Segoe UI", 10F);
        footer.Controls.Add(_statusLabel);

        Controls.Add(contentPanel);
        Controls.Add(_pagerPanel);
        Controls.Add(footer);
        Controls.Add(headerPanel);
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
            Name = "Phone",
            HeaderText = "Số điện thoại",
            FillWeight = 12,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "AppointmentCount",
            HeaderText = "Số lần khám",
            FillWeight = 10,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "LastAppointment",
            HeaderText = "Lần khám gần nhất",
            FillWeight = 15,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });
        _grid.Columns.Add(new DataGridViewButtonColumn
        {
            Name = "Select",
            HeaderText = "",
            Text = "Chọn",
            UseColumnTextForButtonValue = true,
            FillWeight = 8
        });

        _grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            Alignment = DataGridViewContentAlignment.MiddleLeft,
            BackColor = Color.FromArgb(236, 236, 236),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(60, 60, 60),
            Padding = new Padding(10, 0, 0, 0)
        };
        _grid.ColumnHeadersHeight = 46;
        _grid.EnableHeadersVisualStyles = false;

        _grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.White,
            Font = new Font("Segoe UI", 11F),
            SelectionBackColor = Color.FromArgb(243, 244, 246),
            SelectionForeColor = Color.FromArgb(15, 23, 42)
        };

        _grid.RowsDefaultCellStyle = new DataGridViewCellStyle
        {
            SelectionBackColor = Color.FromArgb(243, 244, 246),
            SelectionForeColor = Color.FromArgb(15, 23, 42)
        };

        _grid.GridColor = Color.FromArgb(200, 200, 200);
    }

    private void WireEvents()
    {
        _refreshButton.Click += async (_, _) => await LoadCustomersAsync();
        _searchBox.TextChanged += (_, _) => ApplyFilter();
        _btnPrev.Click += (_, _) => { if (_currentPage > 1) { _currentPage--; RefreshGrid(); } };
        _btnNext.Click += (_, _) => { _currentPage++; RefreshGrid(); };
        _pageSizeBox.SelectedIndexChanged += (_, _) =>
        {
            if (int.TryParse(_pageSizeBox.SelectedItem?.ToString(), out var size))
            {
                _pageSize = size;
                _currentPage = 1;
                RefreshGrid();
            }
        };
        _grid.CellClick += (_, e) =>
        {
            if (e.RowIndex < 0) return;
            if (_grid.Columns[e.ColumnIndex].Name != "Select") return;
            var row = _grid.Rows[e.RowIndex];
            if (row.Tag is CustomerDto customer)
            {
                SelectedCustomer = customer;
                DialogResult = DialogResult.OK;
                Close();
            }
        };
    }

    private async Task LoadCustomersAsync()
    {
        try
        {
            ToggleControls(false);
            var customers = await _loader();
            _allCustomers.Clear();
            _allCustomers.AddRange(customers.OrderByDescending(c => c.CreatedAt));
            _currentPage = 1;
            ApplyFilter();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            ToggleControls(true);
        }
    }

    private void ApplyFilter()
    {
        var keyword = (_searchBox.Text ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(keyword))
        {
            _filteredCustomers = _allCustomers.ToList();
        }
        else
        {
            var lower = keyword.ToLowerInvariant();
            _filteredCustomers = _allCustomers
                .Where(c =>
                    (c.FullName ?? string.Empty).ToLowerInvariant().Contains(lower)
                    || (c.Email ?? string.Empty).ToLowerInvariant().Contains(lower)
                    || (c.PhoneNumber ?? string.Empty).Contains(lower))
                .ToList();
        }

        _currentPage = 1;
        RefreshGrid();
    }

    private void RefreshGrid()
    {
        _grid.Rows.Clear();

        if (_pageSize <= 0) _pageSize = 10;
        var totalItems = _filteredCustomers.Count;
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)_pageSize));
        if (_currentPage > totalPages) _currentPage = totalPages;
        if (_currentPage < 1) _currentPage = 1;

        var start = (_currentPage - 1) * _pageSize;
        var pageItems = _filteredCustomers.Skip(start).Take(_pageSize).ToList();

        foreach (var customer in pageItems)
        {
            var rowIndex = _grid.Rows.Add(
                customer.FullName,
                customer.Email,
                customer.PhoneNumber,
                customer.AppointmentCount.ToString(),
                customer.LastAppointment?.ToString("dd/MM/yyyy HH:mm") ?? "Chưa có",
                "Chọn");
            _grid.Rows[rowIndex].Tag = customer;
        }

        _statusLabel.Text = $"Tổng: {totalItems} khách hàng";
        _pageInfo.Text = $"Trang {_currentPage} / {totalPages}";
        _btnPrev.Enabled = _currentPage > 1;
        _btnNext.Enabled = _currentPage < totalPages;
    }

    private void ToggleControls(bool enabled)
    {
        _grid.Enabled = enabled;
        _searchBox.Enabled = enabled;
        _refreshButton.Enabled = enabled;
        _btnPrev.Enabled = enabled && _btnPrev.Enabled;
        _btnNext.Enabled = enabled && _btnNext.Enabled;
        _pageSizeBox.Enabled = enabled;
    }
}
