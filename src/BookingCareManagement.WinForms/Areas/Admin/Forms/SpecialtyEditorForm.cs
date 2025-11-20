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
    private readonly TextBox _txtImageUrl = new() { Dock = DockStyle.Fill, ReadOnly = true };
    private readonly Button _btnBrowseImage = new()
    {
        Text = "...",
        Width = 40,
        Height = 27,
        Cursor = Cursors.Hand
    };
    private readonly NumericUpDown _numPrice = new()
    {
        Dock = DockStyle.Fill,
        Minimum = 0,
        Maximum = 99999999,
        DecimalPlaces = 0,
        ThousandsSeparator = true,
        Value = 0
    };
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
        Size = new Size(760, 620);
        MinimumSize = new Size(640, 560);
        Font = new Font("Segoe UI", 10);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

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
            Padding = new Padding(20, 20, 20, 100),
            ColumnCount = 2,
            RowCount = 6,
            AutoScroll = true
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        int row = 0;

        // Tên chuyên khoa
        AddLabeledControl(root, "Tên chuyên khoa", _txtName, row++);

        // Slug
        AddLabeledControl(root, "Slug", _txtSlug, row++);

        // Ảnh với nút browse
        var imagePanel = new Panel { Dock = DockStyle.Fill, Height = 30 };
        _txtImageUrl.Dock = DockStyle.None;
        _txtImageUrl.Width = 460;
        _txtImageUrl.Left = 0;
        _txtImageUrl.Top = 0;
        _btnBrowseImage.Left = 470;
        _btnBrowseImage.Top = 0;
        imagePanel.Controls.Add(_txtImageUrl);
        imagePanel.Controls.Add(_btnBrowseImage);
        AddLabeledControl(root, "Ảnh (URL)", imagePanel, row++);

        // Nút browse click event
        _btnBrowseImage.Click += (s, e) =>
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp",
                Title = "Chọn ảnh chuyên khoa"
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _txtImageUrl.Text = dialog.FileName;
            }
        };

        // Giá khám
        AddLabeledControl(root, "Giá khám (VNĐ)", _numPrice, row++);

        // Bác sĩ đảm nhiệm (moved above description)
        AddLabeledControl(root, "Bác sĩ đảm nhiệm", _doctorList, row++);

        // Mô tả
        AddLabeledControl(root, "Mô tả", _txtDescription, row++);

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
            Padding = new Padding(0, 8, 12, 8),
            AutoSize = false,
            Height = 35
        };

        layout.Controls.Add(label, 0, row);
        layout.Controls.Add(control, 1, row);
    }

    private Panel BuildFooterPanel()
    {
        var footer = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 70,
            BackColor = Color.FromArgb(246, 248, 252)
        };

        // Container cho buttons ở góc phải dưới
        var buttonContainer = new Panel
        {
            AutoSize = false,
            Width = 180,
            Height = 40,
            Location = new Point(footer.Width - 200, 15)
        };

        // Đặt buttons trực tiếp vào container
        _btnSave.Location = new Point(0, 0);
        _btnCancel.Location = new Point(90, 0);

        buttonContainer.Controls.Add(_btnSave);
        buttonContainer.Controls.Add(_btnCancel);
        
        footer.Controls.Add(buttonContainer);
        
        // Đảm bảo buttons luôn ở góc phải khi resize
        footer.Resize += (s, e) =>
        {
            buttonContainer.Location = new Point(footer.Width - 200, 15);
        };
        
        return footer;
    }

    private (Button Save, Button Cancel) BuildFooterButtons()
    {
        var btnSave = new Button
        {
            Text = _existing is null ? "Thêm" : "Lưu",
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
            Text = "Hủy",
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

    private void PopulateDoctors()
    {
        _doctorList.Items.Clear();
        
        // Filter to show only doctors without specialty (or if editing, include current specialty's doctors)
        var doctorsToShow = _doctorOptions.Where(d => 
            d.Specialties == null || 
            !d.Specialties.Any() || 
            (_existing != null && d.Specialties.Contains(_existing.Name))
        ).ToList();

        foreach (var doctor in doctorsToShow)
        {
            _doctorList.Items.Add(new DoctorListItem(doctor.Id, ResolveDoctorName(doctor)), false);
        }
    }

    private void BindExisting(SpecialtyDto dto)
    {
        _txtName.Text = dto.Name;
        _txtSlug.Text = dto.Slug;
        _txtImageUrl.Text = dto.ImageUrl ?? string.Empty;
        _numPrice.Value = dto.Price;
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
            _txtName.Focus();
            return;
        }

        if (_numPrice.Value < 0)
        {
            MessageBox.Show(this, "Giá khám không hợp lệ", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _numPrice.Focus();
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
            Name = _txtName.Text.Trim(),
            Slug = _txtSlug.Text.Trim(),
            Color = "#1a73e8", // Default color since field removed
            Price = _numPrice.Value,
            Description = _txtDescription.Text.Trim(),
            ImageUrl = string.IsNullOrWhiteSpace(_txtImageUrl.Text) ? null : _txtImageUrl.Text.Trim(),
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
