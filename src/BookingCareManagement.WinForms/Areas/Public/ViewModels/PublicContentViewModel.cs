using BookingCareManagement.WinForms.Shared.Models.ViewModels;

namespace BookingCareManagement.WinForms.Areas.Public.ViewModels;

public sealed class PublicContentViewModel : ViewModelBase
{
    private string _announcement = "";

    public string Announcement
    {
        get => _announcement;
        set => SetProperty(ref _announcement, value);
    }
}
