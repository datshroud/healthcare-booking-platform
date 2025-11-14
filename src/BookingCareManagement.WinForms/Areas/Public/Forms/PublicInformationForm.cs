using System.Windows.Forms;
using BookingCareManagement.WinForms.Areas.Public.Controllers;
using BookingCareManagement.WinForms.Areas.Public.ViewModels;

namespace BookingCareManagement.WinForms.Areas.Public.Forms;

public sealed class PublicInformationForm : Form
{
    private readonly PublicContentController _controller;
    private readonly PublicContentViewModel _viewModel;
    private readonly Label _announcementLabel = new() { AutoSize = true };

    public PublicInformationForm(PublicContentController controller, PublicContentViewModel viewModel)
    {
        _controller = controller;
        _viewModel = viewModel;

        Text = "Thông tin chung";
        Width = 500;
        Height = 300;
        StartPosition = FormStartPosition.CenterParent;

        var layout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            Padding = new Padding(24)
        };

        layout.Controls.Add(_announcementLabel);
        Controls.Add(layout);

        _viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(PublicContentViewModel.Announcement))
            {
                _announcementLabel.Text = _viewModel.Announcement;
            }
        };

        Shown += async (_, _) => await _controller.LoadAnnouncementAsync();
    }
}
