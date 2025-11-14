using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BookingCareManagement.WinForms.Areas.Account.ViewModels;
using BookingCareManagement.WinForms.Shared.Services;
using BookingCareManagement.WinForms.Shared.State;

namespace BookingCareManagement.WinForms.Areas.Account.Controllers;

public sealed class AuthController
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly SessionState _sessionState;
    private readonly LoginViewModel _viewModel;
    private readonly DialogService _dialogService;

    public AuthController(
        IHttpClientFactory httpClientFactory,
        SessionState sessionState,
        LoginViewModel viewModel,
        DialogService dialogService)
    {
        _httpClientFactory = httpClientFactory;
        _sessionState = sessionState;
        _viewModel = viewModel;
        _dialogService = dialogService;
    }

    public async Task<bool> LoginAsync(CancellationToken cancellationToken = default)
    {
        _viewModel.IsBusy = true;
        try
        {
            var client = _httpClientFactory.CreateClient("BookingCareApi");
            var response = await client.PostAsJsonAsync("api/auth/login", new { _viewModel.UserName, _viewModel.Password }, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _dialogService.ShowError("Sai tài khoản hoặc mật khẩu");
                return false;
            }

            var payload = await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: cancellationToken);
            _sessionState.AccessToken = payload?.AccessToken;
            _sessionState.RefreshToken = payload?.RefreshToken;
            return true;
        }
        finally
        {
            _viewModel.IsBusy = false;
        }
    }

    private sealed record LoginResponse(string AccessToken, string RefreshToken);
}
