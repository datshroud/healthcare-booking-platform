using BookingCareManagement.WinForms.Shared.Models.ViewModels;

namespace BookingCareManagement.WinForms.Areas.Customer.ViewModels;

public sealed class CustomerQueueViewModel : ViewModelBase
{
    private int _waitingCustomers;

    public int WaitingCustomers
    {
        get => _waitingCustomers;
        set => SetProperty(ref _waitingCustomers, value);
    }
}
