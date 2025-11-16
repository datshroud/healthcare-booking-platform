using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BookingCareManagement.WinForms.Areas.Admin.Forms;
using BookingCareManagement.WinForms.Areas.Admin.Models;
using BookingCareManagement.WinForms.Areas.Admin.Services;
using BookingCareManagement.WinForms.Shared.Controls;
using BookingCareManagement.WinForms.Shared.Models.Dtos;
using BookingCareManagement.WinForms.Shared.Services;

namespace BookingCareManagement.WinForms.Areas.Admin.Controls;

public sealed class DoctorManagementControl : UserControl
{
    private readonly AdminDoctorApiClient _doctorApiClient;
    private readonly AdminSpecialtyApiClient _specialtyApiClient;
    private readonly DialogService _dialogService;

    private readonly TextBox _txtSearch = new() { PlaceholderText = "Tìm kiếm bác sĩ...", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10F), BorderStyle = BorderStyle.FixedSingle };
    private readonly Button _btnRefresh = new() { Text = "Làm mới", AutoSize = true, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(71, 85, 105), ForeColor = Color.White, Font = new Font("Segoe UI", 9.5F), Padding = new Padding(12, 6, 12, 6), Cursor = Cursors.Hand };
    private readonly Button _btnAdd = new() { Text = "Thêm", AutoSize = true, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(26, 115, 232), ForeColor = Color.White, Font = new Font("Segoe UI", 9.5F), Padding = new Padding(12, 6, 12, 6), Cursor = Cursors.Hand };
    private readonly Button _btnEdit = new() { Text = "Sửa", AutoSize = true, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(245, 158, 11), ForeColor = Color.White, Font = new Font("Segoe UI", 9.5F), Padding = new Padding(12, 6, 12, 6), Cursor = Cursors.Hand };
    private readonly Button _btnDelete = new() { Text = "Xóa", AutoSize = true, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(239, 68, 68), ForeColor = Color.White, Font = new Font("Segoe UI", 9.5F), Padding = new Padding(12, 6, 12, 6), Cursor = Cursors.Hand };
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, ReadOnly = true, MultiSelect = false, AutoGenerateColumns = false, AllowUserToAddRows = false };
    private readonly BindingSource _bindingSource = new();
    private readonly LoadingOverlay _overlay = new();

    private List<DoctorDto> _doctorCache = new();
    private IReadOnlyList<SpecialtyDto> _specialtyOptions = Array.Empty<SpecialtyDto>();
    private CancellationTokenSource? _loadCts;

    public DoctorManagementControl(
        AdminDoctorApiClient doctorApiClient,
        AdminSpecialtyApiClient specialtyApiClient,
        DialogService dialogService)
    {
        _doctorApiClient = doctorApiClient;
        _specialtyApiClient = specialtyApiClient;
        _dialogService = dialogService;

        Dock = DockStyle.Fill;
        BackColor = Color.White;

        BuildLayout();
        WireEvents();
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await EnsureSpecialtyOptionsAsync(cancellationToken);
        await RefreshAsync(cancellationToken);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _loadCts?.Cancel();
            _loadCts?.Dispose();
        }
        base.Dispose(disposing);
    }

    private void BuildLayout()
    {
        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 68,
            Padding = new Padding(16),
            BackColor = Color.FromArgb(248, 250, 252)
        };

        var searchPanel = new Panel { Dock = DockStyle.Fill };
        searchPanel.Controls.Add(_txtSearch);

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight
        };
        buttonPanel.Controls.Add(_btnRefresh);
        buttonPanel.Controls.Add(_btnAdd);
        buttonPanel.Controls.Add(_btnEdit);
        buttonPanel.Controls.Add(_btnDelete);

        header.Controls.Add(searchPanel);
        header.Controls.Add(buttonPanel);

        ConfigureGrid();

        Controls.Add(_grid);
        Controls.Add(header);
        Controls.Add(_overlay);
        _overlay.BringToFront();
    }

    private void ConfigureGrid()
    {
        _grid.DataSource = _bindingSource;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        _grid.RowTemplate.Height = 56;
        _grid.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);
        _grid.DefaultCellStyle.ForeColor = Color.FromArgb(51, 51, 51);
        _grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(232, 240, 254);
        _grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(26, 115, 232);
        _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9.5F);
        _grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(71, 85, 105);

        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Bác sĩ",
            DataPropertyName = nameof(DoctorRow.FullName),
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Email",
            DataPropertyName = nameof(DoctorRow.Email),
            Width = 220
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Điện thoại",
            DataPropertyName = nameof(DoctorRow.PhoneNumber),
            Width = 140
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Chuyên khoa",
            DataPropertyName = nameof(DoctorRow.SpecialtySummary),
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Trạng thái",
            DataPropertyName = nameof(DoctorRow.StatusLabel),
            Width = 120
        });

        _grid.CellPainting += OnGridCellPainting;
    }

    private void WireEvents()
    {
        _btnRefresh.Click += async (_, _) => await RefreshAsync();
        _btnAdd.Click += async (_, _) => await AddDoctorAsync();
        _btnEdit.Click += async (_, _) => await EditDoctorAsync();
        _btnDelete.Click += async (_, _) => await DeleteDoctorAsync();
        _txtSearch.TextChanged += (_, _) => ApplyFilter();
        _grid.CellDoubleClick += async (_, _) => await EditDoctorAsync();
    }

    private async Task EnsureSpecialtyOptionsAsync(CancellationToken cancellationToken = default)
    {
        if (_specialtyOptions.Count > 0)
        {
            return;
        }

        try
        {
            SetBusy(true);
            _specialtyOptions = await _specialtyApiClient.GetAllAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Không thể tải danh sách chuyên khoa: {ex.Message}");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        _loadCts?.Cancel();
        _loadCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        try
        {
            SetBusy(true);
            _doctorCache = (await _doctorApiClient.GetAllAsync(_loadCts.Token)).ToList();
            ApplyFilter();
        }
        catch (OperationCanceledException)
        {
            // ignore
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Không thể tải bác sĩ: {ex.Message}");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void ApplyFilter()
    {
        var keyword = (_txtSearch.Text ?? string.Empty).Trim();
        IEnumerable<DoctorDto> source = _doctorCache;
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            source = source.Where(dto =>
                dto.FullName.Contains(keyword, StringComparison.CurrentCultureIgnoreCase) ||
                dto.Email.Contains(keyword, StringComparison.CurrentCultureIgnoreCase) ||
                dto.PhoneNumber.Contains(keyword, StringComparison.CurrentCultureIgnoreCase));
        }

        var rows = source
            .OrderBy(dto => dto.FullName, StringComparer.CurrentCultureIgnoreCase)
            .Select(MapRow)
            .ToList();

        _bindingSource.DataSource = rows;
    }

    private static DoctorRow MapRow(DoctorDto dto)
    {
        return new DoctorRow
        {
            Id = dto.Id,
            FullName = string.IsNullOrWhiteSpace(dto.FullName) ? "(Chưa có tên)" : dto.FullName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            StatusLabel = dto.Active ? "Hoạt động" : "Vô hiệu hóa",
            IsActive = dto.Active,
            SpecialtySummary = dto.Specialties is { Count: > 0 }
                ? string.Join(", ", dto.Specialties)
                : "Chưa gán chuyên khoa"
        };
    }

    private void OnGridCellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0 || e.Graphics == null)
        {
            return;
        }

        var statusColumnIndex = 4;
        if (e.ColumnIndex == statusColumnIndex && _grid.Rows[e.RowIndex].DataBoundItem is DoctorRow row)
        {
            e.PaintBackground(e.CellBounds, true);

            var badgeSize = new Size(100, 28);
            var badgeRect = new Rectangle(
                e.CellBounds.X + (e.CellBounds.Width - badgeSize.Width) / 2,
                e.CellBounds.Y + (e.CellBounds.Height - badgeSize.Height) / 2,
                badgeSize.Width,
                badgeSize.Height);

            var badgeColor = row.IsActive ? Color.FromArgb(34, 197, 94) : Color.FromArgb(156, 163, 175);
            using (var brush = new SolidBrush(badgeColor))
            using (var path = CreateRoundedRectangle(badgeRect, 14))
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.FillPath(brush, path);
            }

            using (var textBrush = new SolidBrush(Color.White))
            using (var font = new Font("Segoe UI", 9, FontStyle.Bold))
            {
                var textFormat = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                e.Graphics.DrawString(row.StatusLabel, font, textBrush, badgeRect, textFormat);
            }

            e.Handled = true;
        }
    }

    private static System.Drawing.Drawing2D.GraphicsPath CreateRoundedRectangle(Rectangle bounds, int radius)
    {
        var path = new System.Drawing.Drawing2D.GraphicsPath();
        var diameter = radius * 2;
        var arc = new Rectangle(bounds.Location, new Size(diameter, diameter));

        path.AddArc(arc, 180, 90);
        arc.X = bounds.Right - diameter;
        path.AddArc(arc, 270, 90);
        arc.Y = bounds.Bottom - diameter;
        path.AddArc(arc, 0, 90);
        arc.X = bounds.Left;
        path.AddArc(arc, 90, 90);
        path.CloseFigure();

        return path;
    }

    private DoctorDto? GetSelectedDoctor()
    {
        if (_grid.CurrentRow?.DataBoundItem is DoctorRow row)
        {
            return _doctorCache.FirstOrDefault(dto => dto.Id == row.Id);
        }

        return null;
    }

    private async Task AddDoctorAsync()
    {
        await EnsureSpecialtyOptionsAsync();
        var editor = new DoctorEditorForm(_specialtyOptions);
        if (editor.ShowDialog(FindForm()) != DialogResult.OK)
        {
            return;
        }

        var request = editor.BuildRequest();
        try
        {
            SetBusy(true);
            await _doctorApiClient.CreateAsync(request);
            await RefreshAsync();
            _dialogService.ShowInfo("Thêm bác sĩ thành công.");
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Không thể tạo bác sĩ: {ex.Message}");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task EditDoctorAsync()
    {
        var doctor = GetSelectedDoctor();
        if (doctor is null)
        {
            _dialogService.ShowInfo("Vui lòng chọn bác sĩ cần chỉnh sửa.");
            return;
        }

        await EnsureSpecialtyOptionsAsync();
        var editor = new DoctorEditorForm(_specialtyOptions, doctor);
        if (editor.ShowDialog(FindForm()) != DialogResult.OK)
        {
            return;
        }

        var request = editor.BuildRequest();
        try
        {
            SetBusy(true);
            await _doctorApiClient.UpdateAsync(doctor.Id, request);
            await RefreshAsync();
            _dialogService.ShowInfo("Cập nhật bác sĩ thành công.");
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Không thể cập nhật bác sĩ: {ex.Message}");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task DeleteDoctorAsync()
    {
        var doctor = GetSelectedDoctor();
        if (doctor is null)
        {
            _dialogService.ShowInfo("Vui lòng chọn bác sĩ cần xóa.");
            return;
        }

        if (!_dialogService.Confirm($"Bạn chắc chắn muốn xóa bác sĩ '{doctor.FullName}'?"))
        {
            return;
        }

        try
        {
            SetBusy(true);
            await _doctorApiClient.DeleteAsync(doctor.Id);
            await RefreshAsync();
            _dialogService.ShowInfo("Đã xóa (vô hiệu hóa) bác sĩ.");
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Không thể xóa bác sĩ: {ex.Message}");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void SetBusy(bool isBusy)
    {
        _overlay.Visible = isBusy;
        _overlay.Enabled = isBusy;
        if (isBusy)
        {
            _overlay.BringToFront();
        }
        else
        {
            _overlay.SendToBack();
        }
    }

    private sealed class DoctorRow
    {
        public Guid Id { get; init; }
        public string FullName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string PhoneNumber { get; init; } = string.Empty;
        public string StatusLabel { get; init; } = string.Empty;
        public bool IsActive { get; init; }
        public string SpecialtySummary { get; init; } = string.Empty;
    }
}
