using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BookingCareManagement.WinForms.Areas.Admin.Models;
using BookingCareManagement.WinForms.Shared.Models.Dtos;

namespace BookingCareManagement.WinForms.Areas.Admin.Forms;

public sealed class DoctorEditorForm : Form
{
    private readonly TextBox _txtFirstName = new() { Dock = DockStyle.Fill };
    private readonly TextBox _txtLastName = new() { Dock = DockStyle.Fill };
    private readonly TextBox _txtEmail = new() { Dock = DockStyle.Fill };
    private readonly TextBox _txtPhone = new() { Dock = DockStyle.Fill };
    private readonly CheckedListBox _specialtyList = new()
    {
        Dock = DockStyle.Fill,
        CheckOnClick = true,
        Height = 200
    };

    private readonly IReadOnlyList<SpecialtyDto> _specialtyOptions;
    private readonly DoctorDto? _existing;
    private readonly Button _btnSave = null!;
    private readonly Button _btnCancel = null!;

    public DoctorEditorForm(IReadOnlyList<SpecialtyDto> specialtyOptions, DoctorDto? existing = null)
    {
        _specialtyOptions = specialtyOptions;
        _existing = existing;

        Text = existing is null ? "Thêm bác sĩ" : "Cập nhật bác sĩ";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(720, 520);
        MinimumSize = new Size(600, 480);
        Font = new Font("Segoe UI", 10);

        (_btnSave, _btnCancel) = BuildFooterButtons();
        AcceptButton = _btnSave;
        CancelButton = _btnCancel;

        BuildLayout();
        PopulateSpecialties();
        if (existing is not null)
        {
            BindExisting(existing);
        }
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16, 16, 16, 72),
            ColumnCount = 2,
            RowCount = 5,
            AutoScroll = true
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        AddLabeledControl(root, "Tên", _txtFirstName, 0);
        AddLabeledControl(root, "Họ", _txtLastName, 1);
        AddLabeledControl(root, "Email", _txtEmail, 2);
        AddLabeledControl(root, "Điện thoại", _txtPhone, 3);
        AddLabeledControl(root, "Chuyên khoa", _specialtyList, 4);

        Controls.Add(root);
        Controls.Add(BuildFooterPanel());
    }

    private void AddLabeledControl(TableLayoutPanel layout, string label, Control control, int row)
    {
        while (layout.RowStyles.Count <= row)
        {
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        }

        var lbl = new Label
        {
            Text = label,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(0, 0, 12, 8)
        };

        layout.Controls.Add(lbl, 0, row);
        layout.Controls.Add(control, 1, row);
    }

    private Panel BuildFooterPanel()
    {
        var footer = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 64,
            Padding = new Padding(16),
            BackColor = Color.FromArgb(246, 248, 252)
        };

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true
        };

        buttons.Controls.Add(_btnSave);
        buttons.Controls.Add(_btnCancel);
        footer.Controls.Add(buttons);
        return footer;
    }

    private (Button Save, Button Cancel) BuildFooterButtons()
    {
        var btnSave = new Button
        {
            Text = _existing is null ? "Thêm" : "Lưu",
            AutoSize = true,
            Padding = new Padding(18, 6, 18, 6)
        };
        btnSave.Click += (_, _) => HandleSave();

        var btnCancel = new Button
        {
            Text = "Hủy",
            AutoSize = true,
            Padding = new Padding(18, 6, 18, 6),
            DialogResult = DialogResult.Cancel
        };

        return (btnSave, btnCancel);
    }

    private void PopulateSpecialties()
    {
        _specialtyList.Items.Clear();
        foreach (var specialty in _specialtyOptions.OrderBy(s => s.Name, StringComparer.CurrentCultureIgnoreCase))
        {
            _specialtyList.Items.Add(new SpecialtyItem(specialty.Id, specialty.Name));
        }
    }

    private void BindExisting(DoctorDto doctor)
    {
        _txtFirstName.Text = doctor.FirstName;
        _txtLastName.Text = doctor.LastName;
        _txtEmail.Text = doctor.Email;
        _txtPhone.Text = doctor.PhoneNumber;

        var specialtyLookup = _specialtyOptions.ToDictionary(s => s.Name, s => s.Id, StringComparer.OrdinalIgnoreCase);
        var selectedIds = doctor.Specialties
            .Select(name => specialtyLookup.TryGetValue(name, out var id) ? id : Guid.Empty)
            .Where(id => id != Guid.Empty)
            .ToHashSet();

        for (var i = 0; i < _specialtyList.Items.Count; i++)
        {
            if (_specialtyList.Items[i] is SpecialtyItem item && selectedIds.Contains(item.Id))
            {
                _specialtyList.SetItemChecked(i, true);
            }
        }
    }

    private void HandleSave()
    {
        if (string.IsNullOrWhiteSpace(_txtFirstName.Text) || string.IsNullOrWhiteSpace(_txtLastName.Text))
        {
            MessageBox.Show(this, "Vui lòng nhập đầy đủ họ và tên bác sĩ.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(_txtEmail.Text))
        {
            MessageBox.Show(this, "Vui lòng nhập email bác sĩ.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }

    public DoctorUpsertRequest BuildRequest()
    {
        var specialtyIds = _specialtyList.CheckedItems
            .OfType<SpecialtyItem>()
            .Select(item => item.Id)
            .ToArray();

        return new DoctorUpsertRequest
        {
            FirstName = _txtFirstName.Text,
            LastName = _txtLastName.Text,
            Email = _txtEmail.Text,
            PhoneNumber = _txtPhone.Text,
            SpecialtyIds = specialtyIds
        };
    }

    private sealed record SpecialtyItem(Guid Id, string Name)
    {
        public override string ToString() => Name;
    }
}
