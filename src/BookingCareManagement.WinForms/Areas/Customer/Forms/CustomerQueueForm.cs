using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using BookingCareManagement.WinForms.Areas.Customer.Controllers;
using BookingCareManagement.WinForms.Areas.Customer.ViewModels;

namespace BookingCareManagement.WinForms.Areas.Customer.Forms;

public sealed class CustomerQueueForm : Form
{
    private readonly CustomerQueueController _controller;
    private readonly CustomerQueueViewModel _viewModel;
    private readonly Label _statusLabel = new() { AutoSize = true, Font = new Font("Segoe UI", 20, FontStyle.Regular) };

    public CustomerQueueForm(CustomerQueueController controller, CustomerQueueViewModel viewModel)
    {
        _controller = controller;
        _viewModel = viewModel;

        Text = "Quầy tiếp đón";
        Width = 600;
        Height = 400;
        StartPosition = FormStartPosition.CenterParent;

        var refreshButton = new Button { Text = "Cập nhật" };
        refreshButton.Click += async (_, _) => await RefreshAsync();

        var layout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            Padding = new Padding(24)
        };

        layout.Controls.Add(_statusLabel);
        layout.Controls.Add(refreshButton);
        Controls.Add(layout);

        _viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(CustomerQueueViewModel.WaitingCustomers))
            {
                _statusLabel.Text = $"Đang có {_viewModel.WaitingCustomers} khách chờ";
            }
        };

        Shown += async (_, _) => await RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        await _controller.SimulateQueueAsync();
    }
}
