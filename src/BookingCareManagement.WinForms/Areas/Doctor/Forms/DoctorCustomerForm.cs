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
        var header = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 50,
            Padding = new Padding(12, 10, 12, 10),
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            WrapContents = false
        };

        header.Controls.Add(_searchBox);
        _searchBox.Width = 320;

        var buttonPanel = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true, WrapContents = false };
        buttonPanel.Controls.AddRange(new Control[] { _refreshButton, _addButton, _editButton, _deleteButton });
        header.Controls.Add(buttonPanel);

        var footer = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 30,
            Padding = new Padding(12, 4, 12, 4),
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true
        };
        footer.Controls.Add(_statusLabel);

        Controls.Add(_grid);
        Controls.Add(header);
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

        _grid.DataSource = _customers;
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
        if (_grid.DataSource is BindingList<CustomerDto> list)
        {
            var view = list.Where(c =>
                string.IsNullOrWhiteSpace(keyword)
                || (!string.IsNullOrWhiteSpace(c.FullName) && c.FullName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                || (!string.IsNullOrWhiteSpace(c.Email) && c.Email.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                || (!string.IsNullOrWhiteSpace(c.PhoneNumber) && c.PhoneNumber.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            ).ToList();

            _grid.DataSource = new BindingList<CustomerDto>(view);
            _statusLabel.Text = $"Hiển thị {view.Count}/{_customers.Count} khách hàng";
        }
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
