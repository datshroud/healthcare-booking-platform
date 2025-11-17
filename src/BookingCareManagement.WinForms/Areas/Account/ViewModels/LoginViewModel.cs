using BookingCareManagement.WinForms.Shared.Models.ViewModels;

namespace BookingCareManagement.WinForms.Areas.Account.ViewModels;

public sealed class LoginViewModel : ViewModelBase
{
    private string _userName = string.Empty;
    private string _password = string.Empty;
    private bool _isBusy;

    public string UserName
    {
        get => _userName;
        set => SetProperty(ref _userName, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }
}
