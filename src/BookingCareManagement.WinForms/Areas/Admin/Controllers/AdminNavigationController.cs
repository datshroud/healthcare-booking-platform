using System;
using System.Windows.Forms;
using BookingCareManagement.WinForms.Areas.Admin.ViewModels;
using BookingCareManagement.WinForms.Shared.Services;

namespace BookingCareManagement.WinForms.Areas.Admin.Controllers;

public sealed class AdminNavigationController
{
    private readonly IServiceProvider _serviceProvider;
    private readonly AdminDashboardViewModel _dashboardViewModel;
    private readonly DialogService _dialogService;

    public AdminNavigationController(
        IServiceProvider serviceProvider,
        AdminDashboardViewModel dashboardViewModel,
        DialogService dialogService)
    {
        _serviceProvider = serviceProvider;
        _dashboardViewModel = dashboardViewModel;
        _dialogService = dialogService;
    }

    public void Initialize(Control host)
    {
        host.Controls.Clear();
        host.Controls.Add(BuildPlaceholderPanel("Chọn một module để bắt đầu"));
    }

    public void ShowSpecialties(Control host)
    {
        host.Controls.Clear();
        host.Controls.Add(BuildPlaceholderPanel("Đang triển khai màn hình quản lý chuyên khoa"));
    }

    public void ShowDoctors(Control host)
    {
        host.Controls.Clear();
        host.Controls.Add(BuildPlaceholderPanel("Đang triển khai màn hình quản lý bác sĩ"));
    }

    public void ShowSettings(Control host)
    {
        _dialogService.ShowInfo("Chức năng cài đặt sẽ sớm có mặt.");
    }

    private static Control BuildPlaceholderPanel(string message)
    {
        return new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = System.Drawing.Color.White,
            Controls =
            {
                new Label
                {
                    AutoSize = false,
                    Dock = DockStyle.Fill,
                    TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                    Font = new System.Drawing.Font("Segoe UI", 14, System.Drawing.FontStyle.Bold),
                    Text = message,
                    ForeColor = System.Drawing.Color.FromArgb(55, 65, 81)
                }
            }
        };
    }
}
