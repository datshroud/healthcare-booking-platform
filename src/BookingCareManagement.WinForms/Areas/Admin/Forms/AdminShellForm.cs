using System.Drawing;
using System.Windows.Forms;
using BookingCareManagement.WinForms.Areas.Admin.Controllers;
using BookingCareManagement.WinForms.Shared.Controls;
using BookingCareManagement.WinForms.Shared.State;

namespace BookingCareManagement.WinForms.Areas.Admin.Forms;

public sealed class AdminShellForm : Form
{
    private readonly AdminNavigationController _navigationController;
    private readonly SessionState _sessionState;
    private readonly Panel _contentHost = new() { Dock = DockStyle.Fill, BackColor = Color.White };
    private readonly LoadingOverlay _loadingOverlay = new() { Dock = DockStyle.Fill };

    public AdminShellForm(AdminNavigationController navigationController, SessionState sessionState)
    {
        _navigationController = navigationController;
        _sessionState = sessionState;

        Text = "BookingCare Admin";
        MinimumSize = new Size(1280, 800);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 10);

        var sidebar = BuildSidebar();
        Controls.Add(_contentHost);
        Controls.Add(sidebar);
        Controls.Add(_loadingOverlay);

        Load += (_, _) => _navigationController.Initialize(_contentHost);
    }

    private Control BuildSidebar()
    {
        var sidebar = new FlowLayoutPanel
        {
            Dock = DockStyle.Left,
            FlowDirection = FlowDirection.TopDown,
            Width = 260,
            BackColor = Color.FromArgb(17, 24, 39),
            Padding = new Padding(16)
        };

        sidebar.Controls.Add(BuildNavButton("Chuyên khoa", (_, _) => _navigationController.ShowSpecialties(_contentHost)));
        sidebar.Controls.Add(BuildNavButton("Bác sĩ", (_, _) => _navigationController.ShowDoctors(_contentHost)));
        sidebar.Controls.Add(BuildNavButton("Cuộc hẹn", (_, _) => _navigationController.ShowAppointments(_contentHost)));
        sidebar.Controls.Add(BuildNavButton("Hóa đơn", (_, _) => _navigationController.ShowInvoices(_contentHost)));
        sidebar.Controls.Add(BuildNavButton("Cài đặt", (_, _) => _navigationController.ShowSettings(_contentHost)));

        return sidebar;
    }

    private Button BuildNavButton(string text, EventHandler onClick)
    {
        var button = new Button
        {
            Text = text,
            AutoSize = false,
            Width = 200,
            Height = 48,
            Margin = new Padding(0, 0, 0, 12),
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.White,
            BackColor = Color.FromArgb(55, 65, 81)
        };

        button.FlatAppearance.BorderSize = 0;
        button.Click += onClick;
        return button;
    }
}
