using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BookingCareManagement.WinForms.Areas.Account.ViewModels;
using BookingCareManagement.WinForms.Shared.Models;
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
            _sessionState.ApplyRoleHint(payload.Redirect);

            await LoadProfileAsync(client, cancellationToken);

            // Persist snapshot
            _storage.Save(new SessionSnapshot(
                _sessionState.AccessToken,
                _sessionState.RefreshToken,
                _sessionState.CurrentUserId,
                _sessionState.DisplayName,
                _sessionState.Email,
                _sessionState.FirstName,
                _sessionState.LastName,
                _sessionState.AvatarUrl,
                _sessionState.DateOfBirth,
                _sessionState.Roles.ToArray(),
                _sessionState.IsAdmin,
                _sessionState.IsDoctor,
                _sessionState.HasCookieSession,
                _sessionState.LastRedirect));
            return true;
        }
        finally
        {
            _viewModel.IsBusy = false;
        }
    }

    private async Task LoadProfileAsync(HttpClient client, CancellationToken cancellationToken)
    {
        try
        {
            var response = await client.GetAsync("api/account/auth/profile", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return;
            }

            var profile = await response.Content.ReadFromJsonAsync<UserProfileDto>(cancellationToken: cancellationToken);
            if (profile is not null)
            {
                _sessionState.ApplyProfile(profile);
            }
        }
        catch
        {
            // ignore profile errors to avoid breaking login
        }
    }

    private sealed record LoginResponse(string AccessToken, string RefreshToken, string? Redirect = null, string? ExpiresAt = null);
}
