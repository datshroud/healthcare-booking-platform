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
    private readonly IAuthStorage _storage;

    public AuthController(
        IHttpClientFactory httpClientFactory,
        SessionState sessionState,
        LoginViewModel viewModel,
        DialogService dialogService,
        IAuthStorage storage)
    {
        _httpClientFactory = httpClientFactory;
        _sessionState = sessionState;
        _viewModel = viewModel;
        _dialogService = dialogService;
        _storage = storage;
    }

    public async Task<bool> LoginAsync(CancellationToken cancellationToken = default)
    {
        _viewModel.IsBusy = true;
        try
        {
            var client = _httpClientFactory.CreateClient("BookingCareApi");
            // Web API route is /api/account/auth/login and expects { email, password }
            var response = await client.PostAsJsonAsync("api/account/auth/login", new { email = _viewModel.UserName, password = _viewModel.Password }, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _dialogService.ShowError("Sai tài khoản hoặc mật khẩu");
                return false;
            }

            var payload = await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: cancellationToken);
            if (payload is null || string.IsNullOrWhiteSpace(payload.AccessToken))
            {
                _dialogService.ShowError("Phản hồi đăng nhập không hợp lệ");
                return false;
            }

            _sessionState.AccessToken = payload.AccessToken;
            _sessionState.RefreshToken = payload.RefreshToken;
            // Persist snapshot
            _storage.Save(new SessionSnapshot(payload.AccessToken, payload.RefreshToken, null));
            return true;
        }
        finally
        {
            _viewModel.IsBusy = false;
        }
    }

    private sealed record LoginResponse(string AccessToken, string RefreshToken, string? Redirect = null, string? ExpiresAt = null);
}
