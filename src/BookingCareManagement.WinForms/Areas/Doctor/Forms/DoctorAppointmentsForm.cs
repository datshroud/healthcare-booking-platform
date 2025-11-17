using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using BookingCareManagement.WinForms.Areas.Doctor.Controllers;
using BookingCareManagement.WinForms.Areas.Doctor.ViewModels;
using BookingCareManagement.WinForms.Shared.Controls;

namespace BookingCareManagement.WinForms.Areas.Doctor.Forms;

public sealed class DoctorAppointmentsForm : Form
{
    private readonly DoctorAppointmentsController _controller;
    private readonly DoctorAppointmentsViewModel _viewModel;
    private readonly Label _todayAppointmentsLabel = new() { AutoSize = true, Font = new Font("Segoe UI", 28, FontStyle.Bold), Text = "Đang tải..." };
    private readonly LoadingOverlay _loadingOverlay = new() { Dock = DockStyle.Fill };

    public DoctorAppointmentsForm(DoctorAppointmentsController controller, DoctorAppointmentsViewModel viewModel)
    {
        _controller = controller;
        _viewModel = viewModel;

        Text = "Lịch khám hôm nay";
        Width = 900;
        Height = 600;
        StartPosition = FormStartPosition.CenterParent;

        var refreshButton = new Button { Text = "Tải lại", AutoSize = true };
        refreshButton.Click += async (_, _) => await RefreshAsync();

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24),
            ColumnCount = 1,
            RowCount = 3
        };

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        layout.Controls.Add(_todayAppointmentsLabel, 0, 0);
        layout.Controls.Add(refreshButton, 0, 1);
        Controls.Add(layout);
        Controls.Add(_loadingOverlay);

        Shown += async (_, _) => await RefreshAsync();

        _viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(DoctorAppointmentsViewModel.TodayAppointments))
            {
                _todayAppointmentsLabel.Text = $"Hôm nay có {_viewModel.TodayAppointments} lịch hẹn";
            }

            if (args.PropertyName == nameof(DoctorAppointmentsViewModel.IsBusy))
            {
                _loadingOverlay.Visible = _viewModel.IsBusy;
            }
        };
    }

    private async Task RefreshAsync()
    {
        await _controller.RefreshDashboardAsync();
    }
}
