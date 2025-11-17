using BookingCareManagement.WinForms.Shared.Models.ViewModels;

namespace BookingCareManagement.WinForms.Areas.Admin.ViewModels;

public sealed class AdminDashboardViewModel : ViewModelBase
{
    private int _totalDoctors;
    private int _totalSpecialties;

    public int TotalDoctors
    {
        get => _totalDoctors;
        set => SetProperty(ref _totalDoctors, value);
    }

    public int TotalSpecialties
    {
        get => _totalSpecialties;
        set => SetProperty(ref _totalSpecialties, value);
    }
}
