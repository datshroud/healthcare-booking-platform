using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BookingCareManagement.WinForms.Areas.Admin.Services;
using BookingCareManagement.WinForms.Shared.Controls;
using BookingCareManagement.WinForms.Shared.Models.Dtos;
using BookingCareManagement.WinForms.Shared.Services;

namespace BookingCareManagement.WinForms.Areas.Admin.Controls;

public sealed class InvoiceManagementControl : UserControl
{
    private readonly AdminInvoiceApiClient _invoiceApiClient;
    private readonly DialogService _dialogService;

    private readonly TextBox _txtSearch = new() { PlaceholderText = "?? Tìm ki?m hóa ??n...", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 11F), BorderStyle = BorderStyle.FixedSingle };
    private readonly Button _btnRefresh = new() { Text = "?? Làm m?i", AutoSize = true, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(71, 85, 105), ForeColor = Color.White, Font = new Font("Segoe UI", 9.5F), Padding = new Padding(12, 6, 12, 6), Cursor = Cursors.Hand };
    private readonly Button _btnFilter = new() { Text = "?? B? l?c", AutoSize = true, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(229, 231, 235), ForeColor = Color.FromArgb(55, 65, 81), Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Padding = new Padding(12, 6, 12, 6), Cursor = Cursors.Hand };
    private readonly Button _btnMarkPaid = new() { Text = "? ?ánh d?u ?ã thanh toán", AutoSize = true, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(34, 197, 94), ForeColor = Color.White, Font = new Font("Segoe UI", 9.5F), Padding = new Padding(12, 6, 12, 6), Cursor = Cursors.Hand };
    private readonly Button _btnDownloadPdf = new() { Text = "?? T?i PDF", AutoSize = true, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(59, 130, 246), ForeColor = Color.White, Font = new Font("Segoe UI", 9.5F), Padding = new Padding(12, 6, 12, 6), Cursor = Cursors.Hand };
    
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, ReadOnly = true, MultiSelect = false, AutoGenerateColumns = false, AllowUserToAddRows = false };
    private readonly BindingSource _bindingSource = new();
    private readonly LoadingOverlay _overlay = new();

    private readonly Panel _filterPanel = new() { Dock = DockStyle.Top, Height = 80, Visible = false, BackColor = Color.FromArgb(249, 250, 251), Padding = new Padding(16) };
    private readonly CheckedListBox _customerFilter = new();
    private readonly CheckedListBox _serviceFilter = new();
    private readonly CheckedListBox _statusFilter = new();

    private List<InvoiceDto> _invoiceCache = new();
    private CancellationTokenSource? _loadCts;

    public InvoiceManagementControl(
        AdminInvoiceApiClient invoiceApiClient,
        DialogService dialogService)
    {
        _invoiceApiClient = invoiceApiClient;
        _dialogService = dialogService;

        Dock = DockStyle.Fill;
        BackColor = Color.White;

        BuildLayout();
        WireEvents();
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
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
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(0, 0, 0, 0)
        };
        buttonPanel.Controls.Add(_btnRefresh);
        buttonPanel.Controls.Add(_btnFilter);
        buttonPanel.Controls.Add(_btnMarkPaid);
        buttonPanel.Controls.Add(_btnDownloadPdf);

        header.Controls.Add(searchPanel);
        header.Controls.Add(buttonPanel);

        BuildFilterPanel();
        ConfigureGrid();

        Controls.Add(_grid);
        Controls.Add(_filterPanel);
        Controls.Add(header);
        Controls.Add(_overlay);
        _overlay.BringToFront();
    }

    private void BuildFilterPanel()
    {
        var flowLayout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            Padding = new Padding(0)
        };

        // Customer Filter
        var customerBtn = new Button { Text = "?? Khách hàng", Width = 150, Height = 40, FlatStyle = FlatStyle.Flat, BackColor = Color.White };
        // Service Filter
        var serviceBtn = new Button { Text = "?? D?ch v?", Width = 150, Height = 40, FlatStyle = FlatStyle.Flat, BackColor = Color.White };
        // Status Filter
        var statusBtn = new Button { Text = "? Tr?ng thái", Width = 150, Height = 40, FlatStyle = FlatStyle.Flat, BackColor = Color.White };

        flowLayout.Controls.Add(customerBtn);
        flowLayout.Controls.Add(serviceBtn);
        flowLayout.Controls.Add(statusBtn);

        _filterPanel.Controls.Add(flowLayout);
    }

    private void ConfigureGrid()
    {
        _grid.DataSource = _bindingSource;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        _grid.RowTemplate.Height = 60;
        _grid.DefaultCellStyle.Font = new Font("Segoe UI", 10F);
        _grid.DefaultCellStyle.ForeColor = Color.FromArgb(17, 24, 39);
        _grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(243, 244, 246);
        _grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(17, 24, 39);
        _grid.DefaultCellStyle.Padding = new Padding(15, 10, 0, 10);
        _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(107, 114, 128);
        _grid.ColumnHeadersDefaultCellStyle.BackColor = Color.White;
        _grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(15, 0, 0, 0);
        _grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 251);
        _grid.GridColor = Color.FromArgb(229, 231, 235);

        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "S? H?",
            DataPropertyName = nameof(InvoiceRow.InvoiceNumber),
            Width = 100
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Khách hàng",
            DataPropertyName = nameof(InvoiceRow.CustomerName),
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Ngày l?p",
            DataPropertyName = nameof(InvoiceRow.InvoiceDateDisplay),
            Width = 150
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "D?ch v?",
            DataPropertyName = nameof(InvoiceRow.ServiceName),
            Width = 200
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Tr?ng thái",
            DataPropertyName = nameof(InvoiceRow.Status),
            Width = 150
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "T?ng ti?n",
            DataPropertyName = nameof(InvoiceRow.TotalDisplay),
            Width = 120
        });

        _grid.CellFormatting += OnGridCellFormatting;
    }

    private void OnGridCellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (_grid.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
        {
            string status = e.Value.ToString() ?? "";

            if (status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
            {
                e.CellStyle.BackColor = Color.FromArgb(254, 243, 199);
                e.CellStyle.ForeColor = Color.FromArgb(133, 77, 14);
                e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                e.CellStyle.Padding = new Padding(8, 4, 8, 4);
            }
            else if (status.Equals("Paid", StringComparison.OrdinalIgnoreCase))
            {
                e.CellStyle.BackColor = Color.FromArgb(209, 250, 229);
                e.CellStyle.ForeColor = Color.FromArgb(22, 101, 52);
                e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                e.CellStyle.Padding = new Padding(8, 4, 8, 4);
            }
        }
    }

    private void WireEvents()
    {
        _btnRefresh.Click += async (_, _) => await RefreshAsync();
        _btnFilter.Click += (_, _) => ToggleFilter();
        _btnMarkPaid.Click += async (_, _) => await MarkAsPaidAsync();
        _btnDownloadPdf.Click += async (_, _) => await DownloadPdfAsync();
        _txtSearch.TextChanged += (_, _) => ApplyFilter();
        _grid.CellDoubleClick += async (_, _) => await ShowInvoiceDetailsAsync();
    }

    private void ToggleFilter()
    {
        _filterPanel.Visible = !_filterPanel.Visible;
        if (_filterPanel.Visible)
        {
            _btnFilter.BackColor = Color.FromArgb(37, 99, 235);
            _btnFilter.ForeColor = Color.White;
        }
        else
        {
            _btnFilter.BackColor = Color.FromArgb(229, 231, 235);
            _btnFilter.ForeColor = Color.FromArgb(55, 65, 81);
        }
    }

    private async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        _loadCts?.Cancel();
        _loadCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        try
        {
            SetBusy(true);
            _invoiceCache = (await _invoiceApiClient.GetAllAsync(_loadCts.Token)).ToList();
            ApplyFilter();
        }
        catch (OperationCanceledException)
        {
            // ignore
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Không th? t?i danh sách hóa ??n: {ex.Message}");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void ApplyFilter()
    {
        var keyword = (_txtSearch.Text ?? string.Empty).Trim();
        IEnumerable<InvoiceDto> source = _invoiceCache;
        
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            source = source.Where(dto =>
                dto.CustomerName.Contains(keyword, StringComparison.CurrentCultureIgnoreCase) ||
                dto.ServiceName.Contains(keyword, StringComparison.CurrentCultureIgnoreCase) ||
                dto.InvoiceNumber.ToString().Contains(keyword) ||
                dto.Status.Contains(keyword, StringComparison.CurrentCultureIgnoreCase));
        }

        var rows = source
            .OrderByDescending(dto => dto.InvoiceNumber)
            .Select(MapRow)
            .ToList();

        _bindingSource.DataSource = rows;
    }

    private static InvoiceRow MapRow(InvoiceDto dto)
    {
        var statusText = dto.Status.Equals("Paid", StringComparison.OrdinalIgnoreCase) 
            ? "?ã thanh toán" 
            : "?ang ch?";

        return new InvoiceRow
        {
            Id = dto.Id,
            InvoiceNumber = dto.InvoiceNumber,
            CustomerName = string.IsNullOrWhiteSpace(dto.CustomerName) ? "(Không có tên)" : dto.CustomerName,
            CustomerEmail = dto.CustomerEmail,
            InvoiceDateDisplay = dto.InvoiceDate.ToString("dd/MM/yyyy"),
            ServiceName = string.IsNullOrWhiteSpace(dto.ServiceName) ? "N/A" : $"?? {dto.ServiceName}",
            Status = statusText,
            TotalDisplay = $"{dto.Total:N0} ?",
            Total = dto.Total
        };
    }

    private InvoiceDto? GetSelectedInvoice()
    {
        if (_grid.CurrentRow?.DataBoundItem is InvoiceRow row)
        {
            return _invoiceCache.FirstOrDefault(dto => dto.Id == row.Id);
        }
        return null;
    }

    private async Task MarkAsPaidAsync()
    {
        var invoice = GetSelectedInvoice();
        if (invoice is null)
        {
            _dialogService.ShowInfo("Vui lòng ch?n hóa ??n c?n ?ánh d?u.");
            return;
        }

        if (invoice.Status.Equals("Paid", StringComparison.OrdinalIgnoreCase))
        {
            _dialogService.ShowInfo("Hóa ??n này ?ã ???c thanh toán.");
            return;
        }

        if (!_dialogService.Confirm($"?ánh d?u hóa ??n #{invoice.InvoiceNumber} là ?ã thanh toán?"))
        {
            return;
        }

        try
        {
            SetBusy(true);
            await _invoiceApiClient.MarkAsPaidAsync(invoice.Id);
            await RefreshAsync();
            _dialogService.ShowInfo("?ã c?p nh?t tr?ng thái thanh toán.");
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Không th? c?p nh?t tr?ng thái: {ex.Message}");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task DownloadPdfAsync()
    {
        var invoice = GetSelectedInvoice();
        if (invoice is null)
        {
            _dialogService.ShowInfo("Vui lòng ch?n hóa ??n c?n t?i PDF.");
            return;
        }

        try
        {
            SetBusy(true);
            var pdfBytes = await _invoiceApiClient.GetPdfAsync(invoice.Id);
            
            using var saveDialog = new SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                FileName = $"Invoice-{invoice.InvoiceNumber}.pdf",
                DefaultExt = "pdf"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                await File.WriteAllBytesAsync(saveDialog.FileName, pdfBytes);
                _dialogService.ShowInfo($"?ã l?u file: {saveDialog.FileName}");
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Không th? t?i PDF: {ex.Message}");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task ShowInvoiceDetailsAsync()
    {
        var invoice = GetSelectedInvoice();
        if (invoice is null) return;

        var statusText = invoice.Status.Equals("Paid", StringComparison.OrdinalIgnoreCase) 
            ? "?ã thanh toán" 
            : "?ang ch?";

        var details = $"Hóa ??n #{invoice.InvoiceNumber}\n\n" +
                     $"Khách hàng: {invoice.CustomerName}\n" +
                     $"Email: {invoice.CustomerEmail}\n" +
                     $"D?ch v?: {invoice.ServiceName}\n" +
                     $"Ngày: {invoice.InvoiceDate:dd/MM/yyyy}\n" +
                     $"T?ng ti?n: {invoice.Total:N0} ?\n" +
                     $"Tr?ng thái: {statusText}";

        _dialogService.ShowInfo(details);
        await Task.CompletedTask;
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

    private sealed class InvoiceRow
    {
        public Guid Id { get; init; }
        public int InvoiceNumber { get; init; }
        public string CustomerName { get; init; } = string.Empty;
        public string CustomerEmail { get; init; } = string.Empty;
        public string InvoiceDateDisplay { get; init; } = string.Empty;
        public string ServiceName { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public string TotalDisplay { get; init; } = string.Empty;
        public decimal Total { get; init; }
    }
}
