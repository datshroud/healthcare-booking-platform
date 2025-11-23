using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BookingCareManagement.WinForms.Areas.Admin.Services;
using BookingCareManagement.WinForms.Areas.Admin.Services.Models;

namespace BookingCareManagement.WinForms;

public sealed class DashboardForm : Form
{
    private readonly AdminDashboardApiClient _apiClient;
    private Button _refreshButton = null!;
    private CancellationTokenSource? _loadCts;

    private readonly FlowLayoutPanel _statCardsPanel = new()
    {
        Dock = DockStyle.Fill,
        AutoSize = true,
        WrapContents = true,
        FlowDirection = FlowDirection.LeftToRight,
        Padding = new Padding(0, 8, 0, 0)
    };

    private readonly DataGridView _upcomingGrid = new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        MultiSelect = false,
        AutoGenerateColumns = false,
        AllowUserToAddRows = false,
        RowHeadersVisible = false,
        BackgroundColor = Color.White,
        BorderStyle = BorderStyle.None
    };

    private readonly FlowLayoutPanel _activityPanel = new()
    {
        Dock = DockStyle.Fill,
        AutoScroll = true,
        FlowDirection = FlowDirection.TopDown,
        WrapContents = false,
        Padding = new Padding(0, 4, 0, 4)
    };

    public DashboardForm(AdminDashboardApiClient apiClient)
    {
        _apiClient = apiClient;

        Text = "Bang dieu khien";
        MinimumSize = new Size(1100, 720);
        StartPosition = FormStartPosition.CenterParent;
        Font = new Font("Segoe UI", 10);
        BackColor = Color.FromArgb(243, 244, 246);

        Controls.Add(BuildLayout());
        ConfigureUpcomingGrid();
        Shown += async (_, _) => await RefreshAsync();
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

    private Control BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(243, 244, 246),
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(20),
            AutoScroll = true
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        root.Controls.Add(BuildHeader(), 0, 0);
        root.Controls.Add(BuildCardRow(), 0, 1);
        root.Controls.Add(BuildBody(), 0, 2);

        return root;
    }

    private Control BuildHeader()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 120,
            Padding = new Padding(18),
            BackColor = Color.White
        };

        var title = new Label
        {
            Text = "Tong quan bang dieu khien",
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 22, FontStyle.Bold),
            ForeColor = Color.FromArgb(15, 23, 42)
        };

        var subtitle = new Label
        {
            Text = "Theo doi hoat dong hang ngay cua phong kham va cac cuoc hen",
            AutoSize = true,
            Font = new Font("Segoe UI", 11),
            ForeColor = Color.FromArgb(100, 116, 139),
            Margin = new Padding(0, 12, 0, 0)
        };

        _refreshButton = new Button
        {
            Text = "Lam moi so lieu",
            AutoSize = true,
            BackColor = Color.FromArgb(37, 99, 235),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Padding = new Padding(14, 8, 14, 8),
            Anchor = AnchorStyles.Right | AnchorStyles.Top,
            Cursor = Cursors.Hand
        };
        _refreshButton.FlatAppearance.BorderSize = 0;
        _refreshButton.Click += async (_, _) => await RefreshAsync();

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));

        var textStack = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            AutoSize = true
        };
        textStack.Controls.Add(title);
        textStack.Controls.Add(subtitle);

        var buttonWrapper = new Panel { Dock = DockStyle.Fill };
        buttonWrapper.Controls.Add(_refreshButton);
        _refreshButton.Location = new Point(buttonWrapper.Width - _refreshButton.Width, 12);
        _refreshButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        layout.Controls.Add(textStack, 0, 0);
        layout.Controls.Add(buttonWrapper, 1, 0);
        panel.Controls.Add(layout);

        return panel;
    }

    private Control BuildCardRow()
    {
        var host = new Panel
        {
            Dock = DockStyle.Top,
            Height = 170,
            Padding = new Padding(0, 12, 0, 12),
            BackColor = Color.FromArgb(243, 244, 246),
            AutoScroll = true
        };

        host.Controls.Add(_statCardsPanel);
        return host;
    }

    private Control BuildBody()
    {
        var body = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.FromArgb(243, 244, 246),
            Padding = new Padding(0, 12, 0, 0)
        };

        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

        body.Controls.Add(BuildUpcomingPanel(), 0, 0);
        body.Controls.Add(BuildActivityPanel(), 1, 0);

        return body;
    }

    private Control BuildUpcomingPanel()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(14),
            BackColor = Color.White
        };

        var title = new Label
        {
            Text = "Cuoc hen sap dien ra",
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 13, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 41, 59),
            Dock = DockStyle.Top
        };

        var description = new Label
        {
            Text = "Cac ca kham trong 7 ngay toi",
            AutoSize = true,
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = Color.FromArgb(100, 116, 139),
            Dock = DockStyle.Top,
            Margin = new Padding(0, 6, 0, 14)
        };

        panel.Controls.Add(_upcomingGrid);
        panel.Controls.Add(description);
        panel.Controls.Add(title);

        description.BringToFront();
        title.BringToFront();

        return panel;
    }

    private Control BuildActivityPanel()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(14),
            BackColor = Color.White
        };

        var title = new Label
        {
            Text = "Hoat dong gan day",
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 13, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 41, 59),
            Dock = DockStyle.Top
        };

        panel.Controls.Add(_activityPanel);
        panel.Controls.Add(title);
        title.BringToFront();
        return panel;
    }

    private void ConfigureUpcomingGrid()
    {
        _upcomingGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        _upcomingGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9.5F);
        _upcomingGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(71, 85, 105);
        _upcomingGrid.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);
        _upcomingGrid.DefaultCellStyle.ForeColor = Color.FromArgb(45, 55, 72);
        _upcomingGrid.RowTemplate.Height = 42;
        _upcomingGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _upcomingGrid.EnableHeadersVisualStyles = false;
        _upcomingGrid.GridColor = Color.FromArgb(226, 232, 240);

        _upcomingGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Thoi gian",
            DataPropertyName = nameof(UpcomingRow.Time),
            Width = 140
        });
        _upcomingGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Bac si",
            DataPropertyName = nameof(UpcomingRow.DoctorName),
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });
        _upcomingGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Dich vu",
            DataPropertyName = nameof(UpcomingRow.Service),
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });
        _upcomingGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Benh nhan",
            DataPropertyName = nameof(UpcomingRow.CustomerName),
            Width = 180
        });
        _upcomingGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Trang thai",
            DataPropertyName = nameof(UpcomingRow.Status),
            Width = 140
        });
    }

    private async Task RefreshAsync()
    {
        _loadCts?.Cancel();
        _loadCts = new CancellationTokenSource();
        var token = _loadCts.Token;
        _refreshButton.Enabled = false;
        try
        {
            var data = await _apiClient.GetOverviewAsync(token);
            RenderStatCards(data.Cards.Select(MapCard));
            RenderUpcoming(data.UpcomingAppointments);
            RenderActivities(data.Activities.Select(a => new ActivityRow(a.Time, a.Description)));
        }
        catch (OperationCanceledException)
        {
            // ignore
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Khong the tai du lieu dashboard:\n{ex.Message}", "Loi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _refreshButton.Enabled = true;
        }
    }

    private StatCard MapCard(AdminDashboardCardDto dto)
    {
        return new StatCard(
            dto.Title,
            dto.Value,
            string.IsNullOrWhiteSpace(dto.TrendLabel) ? dto.Subtitle : dto.TrendLabel,
            ParseColor(dto.AccentColor));
    }

    private void RenderStatCards(IEnumerable<StatCard> cards)
    {
        _statCardsPanel.SuspendLayout();
        _statCardsPanel.Controls.Clear();

        foreach (var card in cards)
        {
            _statCardsPanel.Controls.Add(BuildCard(card));
        }

        _statCardsPanel.ResumeLayout();
    }

    private Control BuildCard(StatCard card)
    {
        var panel = new Panel
        {
            Width = 250,
            Height = 120,
            Margin = new Padding(0, 0, 16, 0),
            BackColor = Color.White,
            Padding = new Padding(14)
        };

        var accent = new Panel
        {
            Dock = DockStyle.Left,
            Width = 6,
            BackColor = card.AccentColor
        };

        var title = new Label
        {
            Text = card.Title,
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(71, 85, 105),
            Location = new Point(16, 12)
        };

        var value = new Label
        {
            Text = card.Value,
            AutoSize = true,
            Font = new Font("Segoe UI", 26, FontStyle.Bold),
            ForeColor = Color.FromArgb(15, 23, 42),
            Location = new Point(16, 34)
        };

        var subtitle = new Label
        {
            Text = card.Subtitle,
            AutoSize = true,
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = Color.FromArgb(99, 115, 129),
            Location = new Point(18, 78)
        };

        panel.Controls.Add(subtitle);
        panel.Controls.Add(value);
        panel.Controls.Add(title);
        panel.Controls.Add(accent);

        return panel;
    }

    private void RenderUpcoming(IEnumerable<AdminDashboardAppointmentDto> appointments)
    {
        var rows = appointments
            .Select(a => new UpcomingRow(
                string.IsNullOrWhiteSpace(a.TimeLabel) ? $"{a.StartUtc:HH:mm dd/MM}" : a.TimeLabel,
                a.DoctorName,
                a.ServiceName,
                a.CustomerName,
                string.IsNullOrWhiteSpace(a.Status) ? "Dang xu ly" : a.Status))
            .ToList();

        _upcomingGrid.DataSource = new BindingList<UpcomingRow>(rows);
    }

    private void RenderActivities(IEnumerable<ActivityRow> activities)
    {
        _activityPanel.SuspendLayout();
        _activityPanel.Controls.Clear();

        foreach (var activity in activities)
        {
            var container = new Panel
            {
                Height = 70,
                Dock = DockStyle.Top,
                Padding = new Padding(10, 6, 10, 6),
                BackColor = Color.FromArgb(249, 250, 251),
                Margin = new Padding(0, 0, 0, 10)
            };

            var time = new Label
            {
                Text = activity.Time,
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(37, 99, 235),
                Location = new Point(6, 6)
            };

            var note = new Label
            {
                Text = activity.Description,
                AutoSize = false,
                MaximumSize = new Size(420, 0),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(46, 59, 73),
                Location = new Point(6, 28),
                Size = new Size(420, 32)
            };

            container.Controls.Add(note);
            container.Controls.Add(time);
            _activityPanel.Controls.Add(container);
        }

        _activityPanel.ResumeLayout();
    }

    private static Color ParseColor(string? hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
        {
            return Color.FromArgb(37, 99, 235);
        }

        try
        {
            return ColorTranslator.FromHtml(hex);
        }
        catch
        {
            return Color.FromArgb(37, 99, 235);
        }
    }

    private sealed record StatCard(string Title, string Value, string Subtitle, Color AccentColor);

    private sealed record UpcomingRow(string Time, string DoctorName, string Service, string CustomerName, string Status);

    private sealed record ActivityRow(string Time, string Description);
}
