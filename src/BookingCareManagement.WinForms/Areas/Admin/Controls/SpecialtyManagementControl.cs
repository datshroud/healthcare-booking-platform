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

public sealed class SpecialtyManagementControl : UserControl
{
    private readonly AdminSpecialtyApiClient _specialtyApiClient;
    private readonly AdminDoctorApiClient _doctorApiClient;
    private readonly DialogService _dialogService;

    private readonly TextBox _txtSearch = new() { PlaceholderText = "Tìm kiếm chuyên khoa...", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10F), BorderStyle = BorderStyle.FixedSingle };
    private readonly Button _btnRefresh = new() { Text = "Làm mới", AutoSize = true, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(71, 85, 105), ForeColor = Color.White, Font = new Font("Segoe UI", 9.5F), Padding = new Padding(12, 6, 12, 6), Cursor = Cursors.Hand };
    private readonly Button _btnAdd = new() { Text = "Thêm", AutoSize = true, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(26, 115, 232), ForeColor = Color.White, Font = new Font("Segoe UI", 9.5F), Padding = new Padding(12, 6, 12, 6), Cursor = Cursors.Hand };
    private readonly Button _btnEdit = new() { Text = "Sửa", AutoSize = true, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(245, 158, 11), ForeColor = Color.White, Font = new Font("Segoe UI", 9.5F), Padding = new Padding(12, 6, 12, 6), Cursor = Cursors.Hand };
    private readonly Button _btnDelete = new() { Text = "Xóa", AutoSize = true, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(239, 68, 68), ForeColor = Color.White, Font = new Font("Segoe UI", 9.5F), Padding = new Padding(12, 6, 12, 6), Cursor = Cursors.Hand };
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, ReadOnly = true, MultiSelect = false, AutoGenerateColumns = false, AllowUserToAddRows = false };
    private readonly BindingSource _bindingSource = new();
    private readonly LoadingOverlay _overlay = new();

    private List<SpecialtyDto> _specialtyCache = new();
    private IReadOnlyList<DoctorDto> _doctorOptions = Array.Empty<DoctorDto>();
    private CancellationTokenSource? _loadCts;

    public SpecialtyManagementControl(
        AdminSpecialtyApiClient specialtyApiClient,
        AdminDoctorApiClient doctorApiClient,
        DialogService dialogService)
    {
        _specialtyApiClient = specialtyApiClient;
        _doctorApiClient = doctorApiClient;
        _dialogService = dialogService;

        Dock = DockStyle.Fill;
        BackColor = Color.White;

        BuildLayout();
        WireEvents();
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await EnsureDoctorOptionsAsync(cancellationToken);
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
        _grid.BorderStyle = BorderStyle.None;
        _grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        _grid.GridColor = Color.FromArgb(240, 242, 245);
        _grid.BackgroundColor = Color.White;
        _grid.EnableHeadersVisualStyles = false;
        _grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        _grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(55, 65, 81);
        _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        _grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
        _grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 64, 175);
        _grid.DefaultCellStyle.Font = new Font("Segoe UI", 10);
        _grid.DefaultCellStyle.Padding = new Padding(8, 4, 8, 4);

        var colorColumn = new DataGridViewTextBoxColumn
        {
            HeaderText = "Màu",
            DataPropertyName = nameof(SpecialtyRow.Color),
            Width = 100,
            ReadOnly = true
        };
        _grid.Columns.Add(colorColumn);

        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Tên chuyên khoa",
            DataPropertyName = nameof(SpecialtyRow.Name),
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            MinimumWidth = 200
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Trạng thái",
            DataPropertyName = nameof(SpecialtyRow.StatusLabel),
            Width = 120
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Mô tả",
            DataPropertyName = nameof(SpecialtyRow.Description),
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            MinimumWidth = 200
        });

        _grid.CellPainting += OnGridCellPainting;
    }

    private void WireEvents()
    {
        _btnRefresh.Click += async (_, _) => await RefreshAsync();
        _btnAdd.Click += async (_, _) => await AddSpecialtyAsync();
        _btnEdit.Click += async (_, _) => await EditSpecialtyAsync();
        _btnDelete.Click += async (_, _) => await DeleteSpecialtyAsync();
        _txtSearch.TextChanged += (_, _) => ApplyFilter();
        _grid.CellDoubleClick += async (_, _) => await EditSpecialtyAsync();
    }

    private async Task EnsureDoctorOptionsAsync(CancellationToken cancellationToken = default)
    {
        if (_doctorOptions.Count > 0)
        {
            return;
        }

        try
        {
            SetBusy(true);
            _doctorOptions = await _doctorApiClient.GetAllAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Không thể tải danh sách bác sĩ: {ex.Message}");
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
            _specialtyCache = (await _specialtyApiClient.GetAllAsync(_loadCts.Token)).ToList();
            ApplyFilter();
        }
        catch (OperationCanceledException)
        {
            // ignore
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Không thể tải chuyên khoa: {ex.Message}");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void ApplyFilter()
    {
        var keyword = (_txtSearch.Text ?? string.Empty).Trim();
        IEnumerable<SpecialtyDto> source = _specialtyCache;
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            source = source.Where(dto =>
                dto.Name.Contains(keyword, StringComparison.CurrentCultureIgnoreCase) ||
                (dto.Description ?? string.Empty).Contains(keyword, StringComparison.CurrentCultureIgnoreCase));
        }

        var rows = source
            .OrderBy(dto => dto.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select(MapRow)
            .ToList();

        _bindingSource.DataSource = rows;
    }

    private SpecialtyRow MapRow(SpecialtyDto dto)
    {
        var doctorNames = dto.Doctors.Select(d => d.FullName).Where(name => !string.IsNullOrWhiteSpace(name)).ToArray();
        return new SpecialtyRow
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = (dto.Description ?? string.Empty).Trim(),
            Color = dto.Color,
            StatusLabel = dto.Active ? "Hoạt động" : "Vô hiệu hóa",
            IsActive = dto.Active,
            DoctorSummary = doctorNames.Length == 0 ? "Chưa gán bác sĩ" : string.Join(", ", doctorNames)
        };
    }

    private void OnGridCellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0 || e.Graphics == null)
        {
            return;
        }

        var colorColumnIndex = 0;
        if (e.ColumnIndex == colorColumnIndex && _grid.Rows[e.RowIndex].DataBoundItem is SpecialtyRow row)
        {
            e.PaintBackground(e.CellBounds, true);
            
            var badgeSize = new Size(80, 28);
            var badgeRect = new Rectangle(
                e.CellBounds.X + (e.CellBounds.Width - badgeSize.Width) / 2,
                e.CellBounds.Y + (e.CellBounds.Height - badgeSize.Height) / 2,
                badgeSize.Width,
                badgeSize.Height);

            var color = ParseColor(row.Color);
            using (var brush = new SolidBrush(color))
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
                e.Graphics.DrawString(row.Color, font, textBrush, badgeRect, textFormat);
            }

            e.Handled = true;
        }
    }

    private static Color ParseColor(string hex)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(hex))
            {
                return Color.FromArgb(26, 115, 232);
            }
            var clean = hex.TrimStart('#');
            if (clean.Length == 6)
            {
                return Color.FromArgb(
                    Convert.ToInt32(clean.Substring(0, 2), 16),
                    Convert.ToInt32(clean.Substring(2, 2), 16),
                    Convert.ToInt32(clean.Substring(4, 2), 16));
            }
        }
        catch
        {
            // fallback
        }
        return Color.FromArgb(26, 115, 232);
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

    private SpecialtyDto? GetSelectedSpecialty()
    {
        if (_grid.CurrentRow?.DataBoundItem is SpecialtyRow row)
        {
            return _specialtyCache.FirstOrDefault(dto => dto.Id == row.Id);
        }

        return null;
    }

    private async Task AddSpecialtyAsync()
    {
        await EnsureDoctorOptionsAsync();
        var editor = new SpecialtyEditorForm(_doctorOptions);
        if (editor.ShowDialog(FindForm()) != DialogResult.OK)
        {
            return;
        }

        var request = editor.BuildRequest();
        try
        {
            SetBusy(true);
            await _specialtyApiClient.CreateAsync(request);
            await RefreshAsync();
            _dialogService.ShowInfo("Thêm chuyên khoa thành công.");
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Không thể tạo chuyên khoa: {ex.Message}");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task EditSpecialtyAsync()
    {
        var specialty = GetSelectedSpecialty();
        if (specialty is null)
        {
            _dialogService.ShowInfo("Vui lòng chọn chuyên khoa cần chỉnh sửa.");
            return;
        }

        await EnsureDoctorOptionsAsync();
        var editor = new SpecialtyEditorForm(_doctorOptions, specialty);
        if (editor.ShowDialog(FindForm()) != DialogResult.OK)
        {
            return;
        }

        var request = editor.BuildRequest();
        try
        {
            SetBusy(true);
            await _specialtyApiClient.UpdateAsync(specialty.Id, request);
            await RefreshAsync();
            _dialogService.ShowInfo("Cập nhật chuyên khoa thành công.");
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Không thể cập nhật chuyên khoa: {ex.Message}");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task DeleteSpecialtyAsync()
    {
        var specialty = GetSelectedSpecialty();
        if (specialty is null)
        {
            _dialogService.ShowInfo("Vui lòng chọn chuyên khoa cần xóa.");
            return;
        }

        if (!_dialogService.Confirm($"Bạn chắc chắn muốn xóa chuyên khoa '{specialty.Name}'?"))
        {
            return;
        }

        try
        {
            SetBusy(true);
            await _specialtyApiClient.DeleteAsync(specialty.Id);
            await RefreshAsync();
            _dialogService.ShowInfo("Đã xóa chuyên khoa.");
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Không thể xóa chuyên khoa: {ex.Message}");
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

    private sealed class SpecialtyRow
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string Color { get; init; } = string.Empty;
        public string StatusLabel { get; init; } = string.Empty;
        public bool IsActive { get; init; }
        public string DoctorSummary { get; init; } = string.Empty;
    }
}
