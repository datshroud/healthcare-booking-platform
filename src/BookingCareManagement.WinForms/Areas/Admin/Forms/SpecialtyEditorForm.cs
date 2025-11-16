using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BookingCareManagement.WinForms.Areas.Admin.Models;
using BookingCareManagement.WinForms.Shared.Models.Dtos;

namespace BookingCareManagement.WinForms.Areas.Admin.Forms;

public sealed class SpecialtyEditorForm : Form
{
    private readonly TextBox _txtName = new() { Dock = DockStyle.Fill };
    private readonly TextBox _txtSlug = new() { Dock = DockStyle.Fill };
    private readonly TextBox _txtColor = new() { Dock = DockStyle.Fill, Text = "#1a73e8" };
    private readonly TextBox _txtImageUrl = new() { Dock = DockStyle.Fill };
    private readonly TextBox _txtDescription = new()
    {
        Dock = DockStyle.Fill,
        Multiline = true,
        Height = 120,
        ScrollBars = ScrollBars.Vertical
    };

    private readonly CheckedListBox _doctorList = new()
    {
        Dock = DockStyle.Fill,
        CheckOnClick = true,
        Height = 180
    };

    private readonly Button _btnSave = null!;
    private readonly Button _btnCancel = null!;

    private readonly IReadOnlyList<DoctorDto> _doctorOptions;
    private readonly SpecialtyDto? _existing;

    public SpecialtyEditorForm(IReadOnlyList<DoctorDto> doctorOptions, SpecialtyDto? existing = null)
    {
        _doctorOptions = doctorOptions;
        _existing = existing;

        Text = existing is null ? "Thêm chuyên khoa" : "Cập nhật chuyên khoa";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(760, 580);
        MinimumSize = new Size(640, 520);
        Font = new Font("Segoe UI", 10);

        (_btnSave, _btnCancel) = BuildFooterButtons();
        AcceptButton = _btnSave;
        CancelButton = _btnCancel;

        BuildLayout();
        PopulateDoctors();
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
            RowCount = 6,
            AutoScroll = true
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        AddLabeledControl(root, "Tên chuyên khoa", _txtName, 0);
        AddLabeledControl(root, "Slug", _txtSlug, 1);
        AddLabeledControl(root, "Màu sắc", _txtColor, 2);
        AddLabeledControl(root, "Ảnh (URL)", _txtImageUrl, 3);
        AddLabeledControl(root, "Mô tả", _txtDescription, 4);
        AddLabeledControl(root, "Bác sĩ đảm nhiệm", _doctorList, 5);

        Controls.Add(root);
        Controls.Add(BuildFooterPanel());
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
            Padding = new Padding(0, 0, 8, 8)
        };

        layout.Controls.Add(label, 0, row);
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

    private void PopulateDoctors()
    {
        _doctorList.Items.Clear();
        foreach (var doctor in _doctorOptions)
        {
            _doctorList.Items.Add(new DoctorListItem(doctor.Id, ResolveDoctorName(doctor)), false);
        }
    }

    private void BindExisting(SpecialtyDto dto)
    {
        _txtName.Text = dto.Name;
        _txtSlug.Text = dto.Slug;
        _txtColor.Text = dto.Color;
        _txtImageUrl.Text = dto.ImageUrl ?? string.Empty;
        _txtDescription.Text = dto.Description ?? string.Empty;

        var selectedIds = new HashSet<Guid>(dto.Doctors.Select(d => d.Id));
        for (var i = 0; i < _doctorList.Items.Count; i++)
        {
            if (_doctorList.Items[i] is DoctorListItem item && selectedIds.Contains(item.Id))
            {
                _doctorList.SetItemChecked(i, true);
            }
        }
    }

    private void HandleSave()
    {
        if (string.IsNullOrWhiteSpace(_txtName.Text))
        {
            MessageBox.Show(this, "Vui lòng nhập tên chuyên khoa", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }

    public SpecialtyUpsertRequest BuildRequest()
    {
        var doctorIds = _doctorList.CheckedItems
            .OfType<DoctorListItem>()
            .Select(item => item.Id)
            .ToArray();

        return new SpecialtyUpsertRequest
        {
            Name = _txtName.Text,
            Slug = _txtSlug.Text,
            Color = _txtColor.Text,
            Description = _txtDescription.Text,
            ImageUrl = _txtImageUrl.Text,
            DoctorIds = doctorIds
        };
    }

    private static string ResolveDoctorName(DoctorDto doctor)
    {
        if (!string.IsNullOrWhiteSpace(doctor.FullName))
        {
            return doctor.FullName;
        }

        var parts = new[] { doctor.FirstName, doctor.LastName }
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .Select(part => part!.Trim())
            .ToArray();

        if (parts.Length > 0)
        {
            return string.Join(' ', parts);
        }

        return doctor.Email ?? doctor.PhoneNumber ?? "Bác sĩ";
    }

    private sealed record DoctorListItem(Guid Id, string Name)
    {
        public override string ToString() => Name;
    }
}
