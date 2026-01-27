using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BookingCareManagement.WinForms.Areas.Admin.Services;
using BookingCareManagement.WinForms.Shared.Models.Dtos;

namespace BookingCareManagement.WinForms.Areas.Doctor.Forms;

public sealed class DoctorCustomerForm : Form
{
    private readonly CustomerService _customerService;
    private readonly BindingList<CustomerDto> _customers = new();

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
        ConfigureGrid();
        WireEvents();
        Shown += async (_, _) => await LoadCustomersAsync();
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

        var whitePanel = new Panel
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
        whitePanel.Controls.Add(_grid);
        whitePanel.Controls.Add(searchPanel);

        // footer status
        var footer = new Panel { Dock = DockStyle.Bottom, Height = 40, Padding = new Padding(12, 4, 12, 4) };
        _statusLabel.Font = new Font("Segoe UI", 10F);
        footer.Controls.Add(_statusLabel);

        // compose final layout
        headerPanel.Controls.Add(_titleLabel);
        headerPanel.Controls.Add(actionsPanel);
        contentPanel.Controls.Add(whitePanel);
        Controls.Add(contentPanel);
        Controls.Add(headerPanel);
        Controls.Add(footer);
    }

    private void ConfigureGrid()
    {
        _grid.Columns.Clear();
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "FullName",
            HeaderText = "Khách hàng",
            DataPropertyName = nameof(CustomerDto.FullName),
            FillWeight = 30,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Email",
            HeaderText = "Email",
            DataPropertyName = nameof(CustomerDto.Email),
            FillWeight = 25,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Phone",
            HeaderText = "Số điện thoại",
            DataPropertyName = nameof(CustomerDto.PhoneNumber),
            FillWeight = 15,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Created",
            HeaderText = "Ngày tạo",
            DataPropertyName = nameof(CustomerDto.CreatedAt),
            DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" },
            FillWeight = 12,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "AppointmentCount",
            HeaderText = "Số lần khám",
            DataPropertyName = nameof(CustomerDto.AppointmentCount),
            FillWeight = 10,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "LastAppointment",
            HeaderText = "Lần khám gần nhất",
            DataPropertyName = nameof(CustomerDto.LastAppointment),
            DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" },
            FillWeight = 18,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
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

        _grid.DataSource = _customers;
        UpdateTitleCount();
    }

    private void WireEvents()
    {
        _refreshButton.Click += async (_, _) => await LoadCustomersAsync();
        _addButton.Click += async (_, _) => await ShowEditorAsync();
        _editButton.Click += async (_, _) => await ShowEditorAsync(GetSelectedCustomer());
        _deleteButton.Click += async (_, _) => await DeleteSelectedAsync();
        _searchBox.TextChanged += (_, _) => ApplyFilter();
    }

    private CustomerDto? GetSelectedCustomer()
    {
        if (_grid.CurrentRow?.DataBoundItem is CustomerDto dto)
        {
            return dto;
        }

        return null;
    }

    private async Task LoadCustomersAsync()
    {
        await RunBusyAsync(async () =>
        {
            var customers = await _customerService.GetAllAsync();
            _customers.Clear();
            foreach (var customer in customers.OrderByDescending(c => c.CreatedAt))
            {
                _customers.Add(customer);
            }
            _statusLabel.Text = $"Tổng: {_customers.Count} khách hàng";
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
            _customers.Remove(selected);
            _statusLabel.Text = $"Tổng: {_customers.Count} khách hàng";
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
    }

    private void ApplyFilter()
    {
        var keyword = (_searchBox.Text ?? string.Empty).Trim();
        // Always filter against master list (_customers) so filtering is consistent
        var view = _customers.Where(c =>
            string.IsNullOrWhiteSpace(keyword)
            || (!string.IsNullOrWhiteSpace(c.FullName) && c.FullName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            || (!string.IsNullOrWhiteSpace(c.Email) && c.Email.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            || (!string.IsNullOrWhiteSpace(c.PhoneNumber) && c.PhoneNumber.Contains(keyword, StringComparison.OrdinalIgnoreCase))
        ).ToList();

        _grid.DataSource = new BindingList<CustomerDto>(view);
        _statusLabel.Text = $"Hiển thị {view.Count}/{_customers.Count} khách hàng";
        UpdateTitleCount();
    }

    private void UpdateTitleCount()
    {
        int visible = 0;
        if (_grid.DataSource is BindingList<CustomerDto> list)
        {
            visible = list.Count;
        }
        else
        {
            visible = _grid.Rows.Count;
        }

        _titleLabel.Text = $"Khách hàng ({visible})";
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

            _firstName.Text = existing.FirstName;
            _lastName.Text = existing.LastName;
            _email.Text = existing.Email;
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
