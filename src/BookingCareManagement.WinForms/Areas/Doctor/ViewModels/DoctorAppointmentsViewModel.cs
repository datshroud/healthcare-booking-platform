using BookingCareManagement.WinForms.Shared.Models.ViewModels;

namespace BookingCareManagement.WinForms.Areas.Doctor.ViewModels;

public sealed class DoctorAppointmentsViewModel : ViewModelBase
{
    private int _todayAppointments;
    private bool _isBusy;

    public int TodayAppointments
    {
        get => _todayAppointments;
        set => SetProperty(ref _todayAppointments, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }
}
