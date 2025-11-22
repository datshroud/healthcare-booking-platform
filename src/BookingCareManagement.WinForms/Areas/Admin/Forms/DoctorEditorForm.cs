using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BookingCareManagement.WinForms.Areas.Admin.Models;
using BookingCareManagement.WinForms.Areas.Admin.Services;
using BookingCareManagement.WinForms.Shared.Models.Dtos;
using System.Text.RegularExpressions;

namespace BookingCareManagement.WinForms.Areas.Admin.Forms;

public sealed class DoctorEditorForm : Form
{
    // Tab controls
    private Panel _tabHeaderPanel = null!;
    private Button _btnTabInfo = null!;
    private Button _btnTabWorkingHours = null!;
    private Button _btnTabDaysOff = null!;
    private Panel _tabContentPanel = null!;
    
    // Tab 1: Thông tin cơ bản
    private Panel _pnlInfoTab = null!;
    private readonly TextBox _txtFirstName = new() { Dock = DockStyle.Fill };
    private readonly TextBox _txtLastName = new() { Dock = DockStyle.Fill };
    private readonly TextBox _txtEmail = new() { Dock = DockStyle.Fill };
    private readonly TextBox _txtPhone = new() { Dock = DockStyle.Fill };
    private readonly ComboBox _cboSpecialty = new()
    {
        Dock = DockStyle.Fill,
        DropDownStyle = ComboBoxStyle.DropDownList
    };
    private readonly TextBox _txtAvatarUrl = new() { Dock = DockStyle.Fill, ReadOnly = true };
    private readonly Button _btnBrowse = new()
    {
        Text = "...",
        Width = 40,
        Height = 27,
        Cursor = Cursors.Hand
    };

    // Tab 2: Giờ làm việc
    private Panel _pnlWorkingHoursTab = null!;
    
    // Tab 3: Ngày nghỉ
    private Panel _pnlDaysOffTab = null!;

    // Phần 1: Giờ làm việc theo thứ
    private readonly GroupBox _grpWorkingHours = new()
    {
        Text = " 📅 Giờ làm việc theo tuần ",
        Font = new Font("Segoe UI", 10, FontStyle.Bold),
        ForeColor = Color.FromArgb(15, 23, 42)
    };
    private readonly FlowLayoutPanel _flowSchedule = new()
    {
        AutoScroll = true,
        FlowDirection = FlowDirection.TopDown,
        WrapContents = false,
        Dock = DockStyle.Fill,
        Padding = new Padding(15, 10, 15, 100), // Tăng bottom padding lên 100px
        BackColor = Color.FromArgb(248, 250, 252)
    };
    // For multiple slots per day: store (CheckBox, ListBox display, Manage button)
    private readonly Dictionary<DayOfWeek, (CheckBox chk, ListBox slotsDisplay, Button manage)> _scheduleControls = new();
    // Internal storage of slots per day
    private readonly Dictionary<DayOfWeek, List<WorkingHourInfo>> _slotsPerDay = new();

    private readonly string[] _daysOfWeek = { "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7", "Chủ Nhật" };

    // Phần 2: Ngày nghỉ
    private readonly GroupBox _grpDaysOff = new()
    {
        Text = " 🏖️ Quản lý ngày nghỉ ",
        Font = new Font("Segoe UI", 10, FontStyle.Bold),
        ForeColor = Color.FromArgb(15, 23, 42)
    };
    private readonly TextBox _txtDayOffReason = new() { PlaceholderText = "Lý do nghỉ (vd: Nghỉ lễ, Nghỉ phép...)" };
    private readonly DateTimePicker _dtpDayOffStart = new() { Format = DateTimePickerFormat.Short };
    private readonly DateTimePicker _dtpDayOffEnd = new() { Format = DateTimePickerFormat.Short };
    private readonly CheckBox _chkRepeatYearly = new() 
    { 
        Text = "Lặp lại hàng năm",
        AutoSize = true
    };
    private readonly Button _btnAddDayOff = new()
    {
        Text = "➕ Thêm",
        Width = 100,
        Height = 35,
        BackColor = Color.FromArgb(34, 197, 94),
        ForeColor = Color.White,
        FlatStyle = FlatStyle.Flat,
        Cursor = Cursors.Hand
    };
    private readonly Button _btnEditDayOff = new()
    {
        Text = "✏️ Sửa",
        Width = 100,
        Height = 35,
        BackColor = Color.FromArgb(234, 179, 8),
        ForeColor = Color.White,
        FlatStyle = FlatStyle.Flat,
        Cursor = Cursors.Hand,
        Enabled = false
    };
    private readonly Button _btnDeleteDayOff = new()
    {
        Text = "🗑️ Xóa",
        Width = 100,
        Height = 35,
        BackColor = Color.FromArgb(239, 68, 68),
        ForeColor = Color.White,
        FlatStyle = FlatStyle.Flat,
        Cursor = Cursors.Hand,
        Enabled = false
    };
    private readonly DataGridView _dgvDaysOff = new()
    {
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        AllowUserToAddRows = false,
        ReadOnly = true,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        BackgroundColor = Color.White,
        BorderStyle = BorderStyle.None,
        MultiSelect = false
    };
    private readonly List<DoctorDayOffDto> _daysOffList = new();
    private Guid? _editingDayOffId = null;

    private readonly IReadOnlyList<SpecialtyDto> _specialtyOptions;
    private readonly DoctorDto? _existing;
    private readonly Button _btnSave = null!;
    private readonly Button _btnCancel = null!;
    private readonly AdminDoctorApiClient _doctorApiClient;

    public DoctorEditorForm(IReadOnlyList<SpecialtyDto> specialtyOptions, AdminDoctorApiClient doctorApiClient, DoctorDto? existing = null)
    {
        _specialtyOptions = specialtyOptions;
        _existing = existing;
        _doctorApiClient = doctorApiClient;

        Text = existing is null ? "Thêm Bác Sĩ Mới" : "Cập Nhật Thông Tin Bác Sĩ";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(900, 700);
        MinimumSize = new Size(900, 650);
        Font = new Font("Segoe UI", 10);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        (_btnSave, _btnCancel) = BuildFooterButtons();
        AcceptButton = _btnSave;
        CancelButton = _btnCancel;

        BuildTabStructure();
        BuildInfoTab();
        
        if (existing is not null)
        {
            BuildWorkingHoursTab();
            BuildDaysOffTab();
        }
        
        PopulateSpecialties();
        
        if (existing is not null)
        {
            BuildScheduleControls();
            BuildDaysOffControls();
            BindExisting(existing);
        }

        // Hiển thị tab đầu tiên
        ShowTab(_existing is null ? 0 : 0);
    }

    private void BuildTabStructure()
    {
        // Tab header panel
        _tabHeaderPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 60,
            BackColor = Color.FromArgb(248, 250, 252),
            Padding = new Padding(20, 10, 20, 0)
        };

        _btnTabInfo = CreateTabButton("👤 Thông tin cơ bản", 0);
        _btnTabInfo.Location = new Point(20, 10);
        
        _btnTabWorkingHours = CreateTabButton("🕐 Giờ làm việc", 1);
        _btnTabWorkingHours.Location = new Point(220, 10);
        // Hide working hours tab when adding new doctor
        _btnTabWorkingHours.Visible = _existing is not null;
        _btnTabWorkingHours.Enabled = _existing is not null;

        _btnTabDaysOff = CreateTabButton("🏖️ Ngày nghỉ", 2);
        _btnTabDaysOff.Location = new Point(420, 10);
        // Hide days off tab when adding new doctor
        _btnTabDaysOff.Visible = _existing is not null;
        _btnTabDaysOff.Enabled = _existing is not null;

        _tabHeaderPanel.Controls.AddRange(new Control[] { _btnTabInfo, _btnTabWorkingHours, _btnTabDaysOff });

        // Tab content panel
        _tabContentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 0, 0, 80)
        };

        Controls.Add(_tabContentPanel);
        Controls.Add(_tabHeaderPanel);
        Controls.Add(BuildFooterPanel());
    }

    private Button CreateTabButton(string text, int tabIndex)
    {
        var btn = new Button
        {
            Text = text,
            Width = 190,
            Height = 40,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Tag = tabIndex
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.Click += (s, e) => ShowTab(tabIndex);
        
        return btn;
    }

    private void ShowTab(int tabIndex)
    {
        // Update button styles
        _btnTabInfo.BackColor = tabIndex == 0 ? Color.FromArgb(23, 162, 184) : Color.FromArgb(226, 232, 240);
        _btnTabInfo.ForeColor = tabIndex == 0 ? Color.White : Color.FromArgb(51, 65, 85);
        
        if (_btnTabWorkingHours.Visible)
        {
            _btnTabWorkingHours.BackColor = tabIndex == 1 ? Color.FromArgb(23, 162, 184) : Color.FromArgb(226, 232, 240);
            _btnTabWorkingHours.ForeColor = tabIndex == 1 ? Color.White : Color.FromArgb(51, 65, 85);
        }

        if (_btnTabDaysOff.Visible)
        {
            _btnTabDaysOff.BackColor = tabIndex == 2 ? Color.FromArgb(23, 162, 184) : Color.FromArgb(226, 232, 240);
            _btnTabDaysOff.ForeColor = tabIndex == 2 ? Color.White : Color.FromArgb(51, 65, 85);
        }

        // Show/hide tab content
        if (_pnlInfoTab != null)
            _pnlInfoTab.Visible = tabIndex == 0;
        
        if (_pnlWorkingHoursTab != null)
            _pnlWorkingHoursTab.Visible = tabIndex == 1 && _btnTabWorkingHours.Visible;

        if (_pnlDaysOffTab != null)
            _pnlDaysOffTab.Visible = tabIndex == 2 && _btnTabDaysOff.Visible;
    }

    private void BuildInfoTab()
    {
        _pnlInfoTab = new Panel
        {
            Dock = DockStyle.Fill,
            Visible = true
        };

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(30, 30, 30, 30),
            ColumnCount = 2,
            AutoScroll = true
        };

        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        int row = 0;

        // Họ
        AddLabeledControl(root, "Họ:", _txtLastName, row++);

        // Tên
        AddLabeledControl(root, "Tên:", _txtFirstName, row++);

        // Email
        if (_existing is not null)
        {
            _txtEmail.ReadOnly = true;
            _txtEmail.BackColor = Color.FromArgb(240, 240, 240);
        }
        AddLabeledControl(root, "Email:", _txtEmail, row++);

        // SĐT
        AddLabeledControl(root, "Số điện thoại:", _txtPhone, row++);

        // Chuyên khoa
        AddLabeledControl(root, "Chuyên khoa:", _cboSpecialty, row++);

        // Ảnh với nút browse - Label và control cùng hàng
        while (root.RowStyles.Count <= row)
        {
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        }

        var lblAvatar = new Label
        {
            Text = "Ảnh đại diện:",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.TopLeft,
            AutoSize = false,
            Height = 30,

            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };
        lblAvatar.Margin = new Padding(3, 10, 0, 0);

        var avatarPanel = new Panel { Dock = DockStyle.Fill, Height = 30 };
        _txtAvatarUrl.Dock = DockStyle.None;
        _txtAvatarUrl.Width = 500;
        _txtAvatarUrl.Height = 27;
        _txtAvatarUrl.Left = 0;
        _txtAvatarUrl.Top = 0;
        _btnBrowse.Left = 510;
        _btnBrowse.Top = 0;
        avatarPanel.Controls.Add(_txtAvatarUrl);
        avatarPanel.Controls.Add(_btnBrowse);

        root.Controls.Add(lblAvatar, 0, row);
        root.Controls.Add(avatarPanel, 1, row);
        row++;

        _btnBrowse.Click += (s, e) =>
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp",
                Title = "Chọn ảnh đại diện"
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _txtAvatarUrl.Text = dialog.FileName;
            }
        };

        _pnlInfoTab.Controls.Add(root);
        _tabContentPanel.Controls.Add(_pnlInfoTab);
    }

    private void BuildWorkingHoursTab()
    {
        _pnlWorkingHoursTab = new Panel
        {
            Dock = DockStyle.Fill,
            Visible = false,
            AutoScroll = true,
            Padding = new Padding(20)
        };

        _grpWorkingHours.Dock = DockStyle.Fill;
        _pnlWorkingHoursTab.Controls.Add(_grpWorkingHours);
        _tabContentPanel.Controls.Add(_pnlWorkingHoursTab);
    }

    private void BuildDaysOffTab()
    {
        _pnlDaysOffTab = new Panel
        {
            Dock = DockStyle.Fill,
            Visible = false,
            AutoScroll = true,
            Padding = new Padding(20)
        };

        _grpDaysOff.Dock = DockStyle.Fill;
        _pnlDaysOffTab.Controls.Add(_grpDaysOff);
        _tabContentPanel.Controls.Add(_pnlDaysOffTab);
    }

    private void BuildScheduleControls()
    {
        _flowSchedule.Controls.Clear();
        _scheduleControls.Clear();
        _slotsPerDay.Clear();

        // Header
        var headerPanel = new Panel
        {
            Height = 30,
            Width = 800,
            Margin = new Padding(0, 0, 0, 15),
            BackColor = Color.FromArgb(240, 244, 248)
        };

        var lblHeaderDay = new Label
        {
            Text = "Ngày trong tuần",
            Location = new Point(10, 8),
            AutoSize = true,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.FromArgb(51, 65, 85)
        };

        var lblHeaderTime = new Label
        {
            Text = "Giờ làm việc",
            Location = new Point(140, 8),
            AutoSize = true,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.FromArgb(51, 65, 85)
        };

        headerPanel.Controls.AddRange(new Control[] { lblHeaderDay, lblHeaderTime });
        _flowSchedule.Controls.Add(headerPanel);

        // Tạo dòng cho từng ngày
        for (int i = 0; i < _daysOfWeek.Length; i++)
        {
            string day = _daysOfWeek[i];
            DayOfWeek dayOfWeek = (DayOfWeek)((i + 1) % 7);

            var pnlRow = new Panel 
            { 
                Size = new Size(800, 40), 
                Margin = new Padding(0, 3, 0, 3), // Margin giữa các dòng
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var chkDay = new CheckBox
            {
                Text = day,
                Location = new Point(10, 10),
                Width = 110,
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.FromArgb(30, 41, 59)
            };

            // ListBox to display multiple slots
            var lbSlots = new ListBox
            {
                Location = new Point(360, 6),
                Size = new Size(320, 28),
                Font = new Font("Segoe UI", 9),
                Enabled = false
            };

            // Manage button to open dialog for add/edit/delete
            var btnManage = new Button
            {
                Text = "Quản lý",
                Location = new Point(690, 6),
                Size = new Size(80, 28),
                Font = new Font("Segoe UI", 9),
                Enabled = false,
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat
            };
            btnManage.FlatAppearance.BorderSize = 0;

            chkDay.CheckedChanged += (s, e) =>
            {
                lbSlots.Enabled = chkDay.Checked;
                btnManage.Enabled = chkDay.Checked;
                pnlRow.BackColor = chkDay.Checked 
                    ? Color.FromArgb(240, 249, 255) 
                    : Color.White;

                // if unchecked, clear slots display (but keep underlying slots)
                // we leave underlying slots intact so user can re-enable
            };

            // initialize empty list
            _slotsPerDay[dayOfWeek] = new List<WorkingHourInfo>();

            btnManage.Click += (s, e) =>
            {
                var updated = ShowManageSlotsDialog(dayOfWeek, _slotsPerDay[dayOfWeek]);
                if (updated != null)
                {
                    _slotsPerDay[dayOfWeek] = updated;
                    // refresh listbox
                    lbSlots.Items.Clear();
                    foreach (var slot in updated)
                    {
                        lbSlots.Items.Add($"{slot.Start:hh\\:mm} - {slot.End:hh\\:mm}");
                    }
                    lbSlots.SelectedIndex = lbSlots.Items.Count - 1 >= 0 ? lbSlots.Items.Count - 1 : -1;
                }
            };

            pnlRow.Controls.AddRange(new Control[] { 
                chkDay, lbSlots, btnManage
            });
            _flowSchedule.Controls.Add(pnlRow);

            _scheduleControls.Add(dayOfWeek, (chkDay, lbSlots, btnManage));
        }

        // Thêm một panel spacer ở cuối để đảm bảo có thể scroll đến Chủ Nhật
        var spacerPanel = new Panel
        {
            Height = 20,
            Width = 800,
            BackColor = Color.Transparent
        };
        _flowSchedule.Controls.Add(spacerPanel);

        _grpWorkingHours.Controls.Add(_flowSchedule);
    }

    // Dialog to manage slots for a specific day
    private List<WorkingHourInfo>? ShowManageSlotsDialog(DayOfWeek day, List<WorkingHourInfo> current)
    {
        using var dlg = new Form();
        dlg.Text = $"Quản lý giờ - {day}";
        dlg.StartPosition = FormStartPosition.CenterParent;
        dlg.Size = new Size(420, 320);
        dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
        dlg.MaximizeBox = false;

        var lb = new ListBox { Location = new Point(10, 10), Size = new Size(380, 170), Font = new Font("Segoe UI", 9) };
        foreach (var s in current)
            lb.Items.Add($"{s.Start:hh\\:mm} - {s.End:hh\\:mm}");

        var btnAdd = new Button { Text = "➕ Thêm", Location = new Point(10, 190), Size = new Size(90, 30) };
        var btnEdit = new Button { Text = "✏️ Sửa", Location = new Point(110, 190), Size = new Size(90, 30), Enabled = false };
        var btnDelete = new Button { Text = "🗑️ Xóa", Location = new Point(210, 190), Size = new Size(90, 30), Enabled = false };
        var btnOk = new Button { Text = "OK", Location = new Point(220, 240), Size = new Size(80, 30), DialogResult = DialogResult.OK };
        var btnCancel = new Button { Text = "Hủy", Location = new Point(310, 240), Size = new Size(80, 30), DialogResult = DialogResult.Cancel };

        lb.SelectedIndexChanged += (s, e) =>
        {
            bool sel = lb.SelectedIndex >= 0;
            btnEdit.Enabled = sel;
            btnDelete.Enabled = sel;
        };

        btnAdd.Click += (s, e) =>
        {
            var newSlot = ShowEditSlotDialog();
            if (newSlot != null)
            {
                // validate overlap with existing slots
                if (HasOverlap(current, newSlot!))
                {
                    MessageBox.Show("Khung giờ này chồng lấn với khung giờ đã tồn tại. Vui lòng chọn khung giờ khác.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                current.Add(newSlot!);
                lb.Items.Add($"{newSlot!.Start:hh\\:mm} - {newSlot!.End:hh\\:mm}");
            }
        };

        btnEdit.Click += (s, e) =>
        {
            if (lb.SelectedIndex < 0) return;
            var idx = lb.SelectedIndex;
            var existing = current[idx];
            var edited = ShowEditSlotDialog(existing);
            if (edited != null)
            {
                // validate overlap with other slots excluding the one being edited
                if (HasOverlap(current, edited!, idx))
                {
                    MessageBox.Show("Khung giờ chỉnh sửa chồng lấn với khung giờ khác. Vui lòng điều chỉnh.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                current[idx] = edited!;
                lb.Items[idx] = $"{edited!.Start:hh\\:mm} - {edited!.End:hh\\:mm}";
            }
        };

        btnDelete.Click += (s, e) =>
        {
            if (lb.SelectedIndex < 0) return;
            int idx = lb.SelectedIndex;
            if (MessageBox.Show("Bạn có chắc muốn xóa khung giờ này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                current.RemoveAt(idx);
                lb.Items.RemoveAt(idx);
            }
        };

        dlg.Controls.AddRange(new Control[] { lb, btnAdd, btnEdit, btnDelete, btnOk, btnCancel });

        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            // return a copy
            return current.OrderBy(s => s.Start).ToList();
        }

        return null;
    }

    // Small dialog to add/edit a single slot
    private WorkingHourInfo? ShowEditSlotDialog(WorkingHourInfo? existing = null)
    {
        using var dlg = new Form();
        dlg.Text = existing == null ? "Thêm giờ" : "Sửa giờ";
        dlg.StartPosition = FormStartPosition.CenterParent;
        dlg.Size = new Size(360, 160);
        dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
        dlg.MaximizeBox = false;

        var lblFrom = new Label { Text = "Bắt đầu:", Location = new Point(10, 15), AutoSize = true };
        var dtpFrom = new DateTimePicker { Format = DateTimePickerFormat.Time, ShowUpDown = true, Location = new Point(80, 10), Width = 100 };
        var lblTo = new Label { Text = "Kết thúc:", Location = new Point(200, 15), AutoSize = true };
        var dtpTo = new DateTimePicker { Format = DateTimePickerFormat.Time, ShowUpDown = true, Location = new Point(270, 10), Width = 80 };

        if (existing != null)
        {
            dtpFrom.Value = DateTime.Today.Add(existing.Start);
            dtpTo.Value = DateTime.Today.Add(existing.End);
        }
        else
        {
            dtpFrom.Value = DateTime.Today.AddHours(8);
            dtpTo.Value = DateTime.Today.AddHours(17);
        }

        var btnOk = new Button { Text = "OK", Location = new Point(170, 70), Size = new Size(80, 30), DialogResult = DialogResult.OK };
        var btnCancel = new Button { Text = "Hủy", Location = new Point(260, 70), Size = new Size(80, 30), DialogResult = DialogResult.Cancel };

        dlg.Controls.AddRange(new Control[] { lblFrom, dtpFrom, lblTo, dtpTo, btnOk, btnCancel });

        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            var start = dtpFrom.Value.TimeOfDay;
            var end = dtpTo.Value.TimeOfDay;
            if (start >= end)
            {
                MessageBox.Show("Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc.", "Dữ liệu không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }

            return new WorkingHourInfo(start, end);
        }

        return null;
    }

    private void BuildDaysOffControls()
    {
        var container = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(15)
        };

        // Form nhập ngày nghỉ
        var inputPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 120,
            Padding = new Padding(10)
        };

        var lblReason = new Label
        {
            Text = "Lý do nghỉ:",
            Location = new Point(10, 15),
            AutoSize = true,
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };

        _txtDayOffReason.Location = new Point(100, 12);
        _txtDayOffReason.Width = 600;
        _txtDayOffReason.Font = new Font("Segoe UI", 9);

        var lblFrom = new Label
        {
            Text = "Từ ngày:",
            Location = new Point(10, 50),
            AutoSize = true,
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };

        _dtpDayOffStart.Location = new Point(100, 47);
        _dtpDayOffStart.Width = 150;
        _dtpDayOffStart.Font = new Font("Segoe UI", 9);

        var lblTo = new Label
        {
            Text = "Đến ngày:",
            Location = new Point(270, 50),
            AutoSize = true,
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };

        _dtpDayOffEnd.Location = new Point(355, 47);
        _dtpDayOffEnd.Width = 150;
        _dtpDayOffEnd.Font = new Font("Segoe UI", 9);

        _chkRepeatYearly.Location = new Point(520, 50);
        _chkRepeatYearly.Font = new Font("Segoe UI", 9);

        // Panel chứa 3 nút
        var buttonPanel = new Panel
        {
            Location = new Point(10, 80),
            Height = 35,
            Width = 320
        };

        _btnAddDayOff.Location = new Point(0, 0);
        _btnAddDayOff.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        _btnAddDayOff.FlatAppearance.BorderSize = 0;
        _btnAddDayOff.Click += BtnAddDayOff_Click;

        _btnEditDayOff.Location = new Point(110, 0);
        _btnEditDayOff.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        _btnEditDayOff.FlatAppearance.BorderSize = 0;
        _btnEditDayOff.Click += BtnEditDayOff_Click;

        _btnDeleteDayOff.Location = new Point(220, 0);
        _btnDeleteDayOff.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        _btnDeleteDayOff.FlatAppearance.BorderSize = 0;
        _btnDeleteDayOff.Click += BtnDeleteDayOff_Click;

        buttonPanel.Controls.AddRange(new Control[] { _btnAddDayOff, _btnEditDayOff, _btnDeleteDayOff });

        inputPanel.Controls.AddRange(new Control[] { 
            lblReason, _txtDayOffReason, 
            lblFrom, _dtpDayOffStart, 
            lblTo, _dtpDayOffEnd, 
            _chkRepeatYearly,
            buttonPanel 
        });

        // DataGridView hiển thị danh sách ngày nghỉ
        _dgvDaysOff.Location = new Point(10, 130);
        _dgvDaysOff.Size = new Size(800, 250);
        _dgvDaysOff.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
        _dgvDaysOff.ScrollBars = ScrollBars.Both;
        _dgvDaysOff.RowHeadersWidth = 25;
        _dgvDaysOff.AllowUserToResizeRows = false;
        _dgvDaysOff.ColumnHeadersHeight = 35;
        _dgvDaysOff.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        _dgvDaysOff.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        _dgvDaysOff.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 244, 248);
        _dgvDaysOff.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(51, 65, 85);
        
        // Clear existing columns first
        _dgvDaysOff.Columns.Clear();
        
        _dgvDaysOff.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "Reason", 
            HeaderText = "Lý do nghỉ", 
            Width = 350,
            ReadOnly = true
        });
        _dgvDaysOff.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "StartDate", 
            HeaderText = "Từ ngày", 
            Width = 120,
            ReadOnly = true
        });
        _dgvDaysOff.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "EndDate", 
            HeaderText = "Đến ngày", 
            Width = 120,
            ReadOnly = true
        });
        _dgvDaysOff.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "RepeatYearly", 
            HeaderText = "Lặp hàng năm", 
            Width = 120,
            ReadOnly = true
        });

        _dgvDaysOff.SelectionChanged += DgvDaysOff_SelectionChanged;

        container.Controls.Add(_dgvDaysOff);
        container.Controls.Add(inputPanel);

        _grpDaysOff.Controls.Add(container);
    }

    private void DgvDaysOff_SelectionChanged(object? sender, EventArgs e)
    {
        bool hasSelection = _dgvDaysOff.SelectedRows.Count > 0;
        _btnEditDayOff.Enabled = hasSelection;
        _btnDeleteDayOff.Enabled = hasSelection;
    }

    private async void BtnAddDayOff_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_txtDayOffReason.Text))
        {
            MessageBox.Show("Vui lòng nhập lý do nghỉ!", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_dtpDayOffStart.Value > _dtpDayOffEnd.Value)
        {
            MessageBox.Show("Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc!", "Dữ liệu không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_existing == null)
        {
            MessageBox.Show("Vui lòng lưu thông tin bác sĩ trước khi thêm ngày nghỉ!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            var request = new
            {
                Name = _txtDayOffReason.Text.Trim(),
                StartDate = DateOnly.FromDateTime(_dtpDayOffStart.Value.Date),
                EndDate = DateOnly.FromDateTime(_dtpDayOffEnd.Value.Date),
                RepeatYearly = _chkRepeatYearly.Checked
            };

            var result = await _doctorApiClient.CreateDayOffAsync(_existing.Id, request);

            if (result != null)
            {
                _daysOffList.Add(result);
                RefreshDaysOffGrid();
                ClearDayOffInputs();
                MessageBox.Show("Thêm ngày nghỉ thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi thêm ngày nghỉ: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnEditDayOff_Click(object? sender, EventArgs e)
    {
        if (_dgvDaysOff.SelectedRows.Count == 0) return;

        int selectedIndex = _dgvDaysOff.SelectedRows[0].Index;
        if (selectedIndex < 0 || selectedIndex >= _daysOffList.Count) return;

        var dayOff = _daysOffList[selectedIndex];
        
        _editingDayOffId = dayOff.Id;
        _txtDayOffReason.Text = dayOff.Name;
        _dtpDayOffStart.Value = dayOff.StartDate.ToDateTime(TimeOnly.MinValue);
        _dtpDayOffEnd.Value = dayOff.EndDate.ToDateTime(TimeOnly.MinValue);
        _chkRepeatYearly.Checked = dayOff.RepeatYearly;

        _btnAddDayOff.Text = "💾 Cập nhật";
        _btnAddDayOff.BackColor = Color.FromArgb(234, 179, 8);
        _btnAddDayOff.Click -= BtnAddDayOff_Click;
        _btnAddDayOff.Click += BtnUpdateDayOff_Click;
    }

    private async void BtnUpdateDayOff_Click(object? sender, EventArgs e)
    {
        if (!_editingDayOffId.HasValue) return;

        if (string.IsNullOrWhiteSpace(_txtDayOffReason.Text))
        {
            MessageBox.Show("Vui lòng nhập lý do nghỉ!", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_dtpDayOffStart.Value > _dtpDayOffEnd.Value)
        {
            MessageBox.Show("Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc!", "Dữ liệu không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            var request = new
            {
                Name = _txtDayOffReason.Text.Trim(),
                StartDate = DateOnly.FromDateTime(_dtpDayOffStart.Value.Date),
                EndDate = DateOnly.FromDateTime(_dtpDayOffEnd.Value.Date),
                RepeatYearly = _chkRepeatYearly.Checked
            };

            await _doctorApiClient.UpdateDayOffAsync(_existing!.Id, _editingDayOffId.Value, request);

            await LoadDaysOffAsync(_existing.Id);
            ClearDayOffInputs();
            ResetAddButton();
            MessageBox.Show("Cập nhật ngày nghỉ thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi cập nhật ngày nghỉ: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void BtnDeleteDayOff_Click(object? sender, EventArgs e)
    {
        if (_dgvDaysOff.SelectedRows.Count == 0) return;

        int selectedIndex = _dgvDaysOff.SelectedRows[0].Index;
        if (selectedIndex < 0 || selectedIndex >= _daysOffList.Count) return;

        var dayOff = _daysOffList[selectedIndex];

        if (MessageBox.Show($"Bạn có chắc muốn xóa ngày nghỉ '{dayOff.Name}'?", "Xác nhận", 
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
        {
            return;
        }

        try
        {
            await _doctorApiClient.DeleteDayOffAsync(_existing!.Id, dayOff.Id);

            _daysOffList.RemoveAt(selectedIndex);
            RefreshDaysOffGrid();
            ClearDayOffInputs();
            MessageBox.Show("Xóa ngày nghỉ thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi xóa ngày nghỉ: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task LoadWorkingHoursAsync(Guid doctorId)
    {
        try
        {
            var hours = await _doctorApiClient.GetWorkingHoursAsync(doctorId);
            
            if (hours?.Hours != null && hours.Hours.Any())
            {
                // clear existing
                foreach (var kvp in _scheduleControls)
                {
                    kvp.Value.slotsDisplay.Items.Clear();
                    kvp.Value.chk.Checked = false;
                    _slotsPerDay[kvp.Key] = new List<WorkingHourInfo>();
                }

                foreach (var hour in hours.Hours)
                {
                    var dayOfWeek = (DayOfWeek)hour.DayOfWeek;
                    if (_scheduleControls.TryGetValue(dayOfWeek, out var controls))
                    {
                        controls.chk.Checked = true;

                        if (TimeSpan.TryParse(hour.StartTime, out var startTime) && TimeSpan.TryParse(hour.EndTime, out var endTime))
                        {
                            var info = new WorkingHourInfo(startTime, endTime);
                            _slotsPerDay[dayOfWeek].Add(info);
                            controls.slotsDisplay.Items.Add($"{info.Start:hh\\:mm} - {info.End:hh\\:mm}");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading working hours: {ex.Message}");
        }
    }

    private async Task LoadDaysOffAsync(Guid doctorId)
    {
        try
        {
            var daysOff = await _doctorApiClient.GetDayOffsAsync(doctorId);
            
            _daysOffList.Clear();
            if (daysOff != null && daysOff.Any())
            {
                _daysOffList.AddRange(daysOff);
                RefreshDaysOffGrid();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading days off: {ex.Message}");
        }
    }

    private void HandleSave()
    {
        // Validation: required fields
        if (string.IsNullOrWhiteSpace(_txtLastName.Text))
        {
            MessageBox.Show(this, "Vui lòng nhập họ bác sĩ!", "Thiếu thông tin",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            ShowTab(0);
            _txtLastName.Focus();
            return;
        }

        // Last name must not contain digits
        if (_txtLastName.Text.Any(char.IsDigit))
        {
            MessageBox.Show(this, "Họ không được chứa chữ số.", "Dữ liệu không hợp lệ",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            ShowTab(0);
            _txtLastName.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(_txtFirstName.Text))
        {
            MessageBox.Show(this, "Vui lòng nhập tên bác sĩ!", "Thiếu thông tin",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            ShowTab(0);
            _txtFirstName.Focus();
            return;
        }

        // First name must not contain digits
        if (_txtFirstName.Text.Any(char.IsDigit))
        {
            MessageBox.Show(this, "Tên không được chứa chữ số.", "Dữ liệu không hợp lệ",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            ShowTab(0);
            _txtFirstName.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(_txtEmail.Text))
        {
            MessageBox.Show(this, "Vui lòng nhập email bác sĩ!", "Thiếu thông tin",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            ShowTab(0);
            _txtEmail.Focus();
            return;
        }

        // Email format check
        var email = _txtEmail.Text.Trim();
        if (!Regex.IsMatch(email, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
        {
            MessageBox.Show(this, "Vui lòng nhập địa chỉ email hợp lệ.", "Dữ liệu không hợp lệ",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            ShowTab(0);
            _txtEmail.Focus();
            return;
        }

        // Phone optional but if provided, must be 10 digits and start with 0
        var phone = _txtPhone.Text?.Trim() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(phone))
        {
            var digits = new string(phone.Where(char.IsDigit).ToArray());
            // Require exactly 10 digits and start with '0'
            if (digits.Length != 10 || !digits.StartsWith("0"))
            {
                MessageBox.Show(this, "Vui lòng nhập số điện thoại hợp lệ: 10 chữ số và bắt đầu bằng số 0.", "Dữ liệu không hợp lệ",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ShowTab(0);
                _txtPhone.Focus();
                return;
            }
        }

        DialogResult = DialogResult.OK;
        Close();
    }

    // Return dictionary of day -> list of slots
    public Dictionary<DayOfWeek, List<WorkingHourInfo>> GetWorkingHours()
    {
        var result = new Dictionary<DayOfWeek, List<WorkingHourInfo>>();

        foreach (var kvp in _scheduleControls)
        {
            if (kvp.Value.chk.Checked)
            {
                var day = kvp.Key;
                if (_slotsPerDay.TryGetValue(day, out var list) && list.Any())
                {
                    result.Add(day, list.Select(s => new WorkingHourInfo(s.Start, s.End)).ToList());
                }
            }
        }

        return result;
    }

    public List<DoctorDayOffDto> GetDaysOff()
    {
        return _daysOffList;
    }

    public sealed record WorkingHourInfo(TimeSpan Start, TimeSpan End);

    public sealed class DayOffInfo
    {
        public string Reason { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    private sealed record SpecialtyItem(Guid Id, string Name)
    {
        public override string ToString() => Name;
    }

    private (Button Save, Button Cancel) BuildFooterButtons()
    {
        var btnSave = new Button
        {
            Text = "💾 Lưu",
            Width = 80,
            Height = 35,
            BackColor = Color.FromArgb(23, 162, 184),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.Click += (_, _) => HandleSave();

        var btnCancel = new Button
        {
            Text = "❌ Hủy",
            Width = 80,
            Height = 35,
            BackColor = Color.Gray,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10),
            DialogResult = DialogResult.Cancel,
            Cursor = Cursors.Hand
        };
        btnCancel.FlatAppearance.BorderSize = 0;

        return (btnSave, btnCancel);
    }

    private Panel BuildFooterPanel()
    {
        var footer = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 70,
            BackColor = Color.FromArgb(246, 248, 252)
        };

        var buttonContainer = new Panel
        {
            AutoSize = false,
            Width = 180,
            Height = 40,
            Location = new Point(footer.Width - 200, 15)
        };

        _btnSave.Location = new Point(0, 0);
        _btnCancel.Location = new Point(90, 0);

        buttonContainer.Controls.Add(_btnSave);
        buttonContainer.Controls.Add(_btnCancel);
        
        footer.Controls.Add(buttonContainer);
        
        footer.Resize += (s, e) =>
        {
            buttonContainer.Location = new Point(footer.Width - 200, 15);
        };
        
        return footer;
    }

    private void AddLabeledControl(TableLayoutPanel layout, string labelText, Control control, int row)
    {
        while (layout.RowStyles.Count <= row)
        {
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        }

        var label = new Label
        {
            Text = labelText,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(0, 8, 12, 8),
            AutoSize = false,
            Height = 40,
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };

        layout.Controls.Add(label, 0, row);
        layout.Controls.Add(control, 1, row);
    }

    private void PopulateSpecialties()
    {
        _cboSpecialty.Items.Clear();
        _cboSpecialty.Items.Add(new SpecialtyItem(Guid.Empty, "-- Không chọn --"));

        foreach (var specialty in _specialtyOptions.OrderBy(s => s.Name, StringComparer.CurrentCultureIgnoreCase))
        {
            _cboSpecialty.Items.Add(new SpecialtyItem(specialty.Id, specialty.Name));
        }

        _cboSpecialty.SelectedIndex = 0;
    }

    private async void BindExisting(DoctorDto doctor)
    {
        _txtLastName.Text = doctor.LastName;
        _txtFirstName.Text = doctor.FirstName;
        _txtEmail.Text = doctor.Email;
        _txtPhone.Text = doctor.PhoneNumber;
        _txtAvatarUrl.Text = doctor.AvatarUrl;

        if (doctor.Specialties.Any())
        {
            var specialtyName = doctor.Specialties.First();
            var specialtyItem = _cboSpecialty.Items.Cast<object>()
                .OfType<SpecialtyItem>()
                .FirstOrDefault(item => item.Name.Equals(specialtyName, StringComparison.OrdinalIgnoreCase));

            if (specialtyItem != null)
            {
                _cboSpecialty.SelectedItem = specialtyItem;
            }
        }

        await LoadWorkingHoursAsync(doctor.Id);
        await LoadDaysOffAsync(doctor.Id);
    }

    private void RefreshDaysOffGrid()
    {
        _dgvDaysOff.Rows.Clear();
        foreach (var dayOff in _daysOffList.OrderBy(d => d.StartDate))
        {
            _dgvDaysOff.Rows.Add(
                dayOff.Name,
                dayOff.StartDate.ToString("dd/MM/yyyy"),
                dayOff.EndDate.ToString("dd/MM/yyyy"),
                dayOff.RepeatYearly ? "Có" : "Không"
            );
        }
    }

    private void ClearDayOffInputs()
    {
        _txtDayOffReason.Clear();
        _dtpDayOffStart.Value = DateTime.Today;
        _dtpDayOffEnd.Value = DateTime.Today;
        _chkRepeatYearly.Checked = false;
        _editingDayOffId = null;
    }

    private void ResetAddButton()
    {
        _btnAddDayOff.Text = "➕ Thêm";
        _btnAddDayOff.BackColor = Color.FromArgb(34, 197, 94);
        _btnAddDayOff.Click -= BtnUpdateDayOff_Click;
        _btnAddDayOff.Click += BtnAddDayOff_Click;
    }

    public DoctorUpsertRequest BuildRequest()
    {
        var selectedSpecialty = _cboSpecialty.SelectedItem as SpecialtyItem;
        var specialtyIds = selectedSpecialty?.Id != Guid.Empty
            ? new[] { selectedSpecialty!.Id }
            : Array.Empty<Guid>();
        
        return new DoctorUpsertRequest
        {
            FirstName = _txtFirstName.Text.Trim(),
            LastName = _txtLastName.Text.Trim(),
            Email = _txtEmail.Text.Trim(),
            PhoneNumber = _txtPhone.Text.Trim(),
            AvatarUrl = string.IsNullOrWhiteSpace(_txtAvatarUrl.Text) ? null : _txtAvatarUrl.Text.Trim(),
            SpecialtyIds = specialtyIds
        };
    }

    // Check if candidate overlaps any slot in list. If excludeIndex provided, skip that index (useful when editing).
    private static bool HasOverlap(List<WorkingHourInfo> slots, WorkingHourInfo candidate, int? excludeIndex = null)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (excludeIndex.HasValue && excludeIndex.Value == i) continue;
            var s = slots[i];
            if (IntervalsOverlap(s.Start, s.End, candidate.Start, candidate.End))
            {
                return true;
            }
        }
        return false;
    }

    private static bool IntervalsOverlap(TimeSpan aStart, TimeSpan aEnd, TimeSpan bStart, TimeSpan bEnd)
    {
        // overlap if aStart < bEnd && bStart < aEnd
        return aStart < bEnd && bStart < aEnd;
    }
}
