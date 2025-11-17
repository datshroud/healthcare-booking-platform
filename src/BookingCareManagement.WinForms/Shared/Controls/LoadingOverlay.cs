using System.Drawing;
using System.Windows.Forms;

namespace BookingCareManagement.WinForms.Shared.Controls;

/// <summary>
/// Simple overlay that can be toggled on top of any container while awaiting API responses.
/// </summary>
public sealed class LoadingOverlay : UserControl
{
    private readonly Label _messageLabel = new()
    {
        AutoSize = true,
        ForeColor = Color.White,
        Font = new Font("Segoe UI", 12, FontStyle.Bold),
        Text = "Đang tải dữ liệu..."
    };

    public LoadingOverlay()
    {
        BackColor = Color.FromArgb(150, 33, 37, 41);
        Visible = false;
        Enabled = false;
        Dock = DockStyle.Fill;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            ColumnCount = 1,
            RowCount = 1
        };

        layout.Controls.Add(_messageLabel, 0, 0);
        layout.SetCellPosition(_messageLabel, new TableLayoutPanelCellPosition(0, 0));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Controls.Add(layout);
    }
}
