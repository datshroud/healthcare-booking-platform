using System.Threading.Tasks;
using BookingCareManagement.WinForms.Areas.Public.ViewModels;

namespace BookingCareManagement.WinForms.Areas.Public.Controllers;

public sealed class PublicContentController
{
    private readonly PublicContentViewModel _viewModel;

    public PublicContentController(PublicContentViewModel viewModel)
    {
        _viewModel = viewModel;
    }

    public Task LoadAnnouncementAsync()
    {
        _viewModel.Announcement = "Chào mừng bạn đến với BookingCare";
        return Task.CompletedTask;
    }
}
