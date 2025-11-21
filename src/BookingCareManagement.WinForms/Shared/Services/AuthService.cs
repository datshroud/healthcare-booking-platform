using System;
using System.Net.Http;
using System.Linq;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using BookingCareManagement.WinForms.Shared.Models;
using BookingCareManagement.WinForms.Shared.State;

namespace BookingCareManagement.WinForms.Shared.Services;

public sealed class AuthService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly SessionState _session;
    private readonly IAuthStorage _storage;
    private readonly DialogService _dialogs;

    public AuthService(IHttpClientFactory httpFactory, SessionState session, IAuthStorage storage, DialogService dialogs)
    {
        _httpFactory = httpFactory;
        _session = session;
        _storage = storage;
        _dialogs = dialogs;
    }

    private static string? FindTokenValue(JsonElement el)
    {
        switch (el.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var prop in el.EnumerateObject())
                {
                    var name = prop.Name.ToLowerInvariant();
                    if ((name.Contains("accesstoken") || name.Contains("access_token") || name == "token" || name.Contains("token"))
                        && prop.Value.ValueKind == JsonValueKind.String)
                    {
                        return prop.Value.GetString();
                    }

                    // Common wrapper fields
                    if (name == "data" || name == "result" || name == "value")
                    {
                        var found = FindTokenValue(prop.Value);
                        if (!string.IsNullOrWhiteSpace(found)) return found;
                    }

                    // Recurse into any object/array
                    if (prop.Value.ValueKind == JsonValueKind.Object || prop.Value.ValueKind == JsonValueKind.Array)
                    {
                        var found = FindTokenValue(prop.Value);
                        if (!string.IsNullOrWhiteSpace(found)) return found;
                    }
                }
                break;
            case JsonValueKind.Array:
                foreach (var item in el.EnumerateArray())
                {
                    var found = FindTokenValue(item);
                    if (!string.IsNullOrWhiteSpace(found)) return found;
                }
                break;
            case JsonValueKind.String:
                // If the entire document is a string, unlikely but return it
                return el.GetString();
        }

        return null;
    }

    private static string? FindRefreshValue(JsonElement el)
    {
        switch (el.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var prop in el.EnumerateObject())
                {
                    var name = prop.Name.ToLowerInvariant();
                    if ((name.Contains("refreshtoken") || name.Contains("refresh_token") || name.Contains("refresh"))
                        && prop.Value.ValueKind == JsonValueKind.String)
                    {
                        return prop.Value.GetString();
                    }

                    if (name == "data" || name == "result" || name == "value")
                    {
                        var found = FindRefreshValue(prop.Value);
                        if (!string.IsNullOrWhiteSpace(found)) return found;
                    }

                    if (prop.Value.ValueKind == JsonValueKind.Object || prop.Value.ValueKind == JsonValueKind.Array)
                    {
                        var found = FindRefreshValue(prop.Value);
                        if (!string.IsNullOrWhiteSpace(found)) return found;
                    }
                }
                break;
            case JsonValueKind.Array:
                foreach (var item in el.EnumerateArray())
                {
                    var found = FindRefreshValue(item);
                    if (!string.IsNullOrWhiteSpace(found)) return found;
                }
                break;
        }

        return null;
    }

    public async Task<bool> LoginAsync(LoginRequestDto request)
    {
        try
        {
            var client = _httpFactory.CreateClient("BookingCareApi");

            var reqJson = JsonSerializer.Serialize(request);
            Console.WriteLine($"[AuthService] POST api/account/Auth/login REQUEST: {reqJson}");

            var resp = await client.PostAsJsonAsync("api/account/Auth/login", request);

            var respText = await resp.Content.ReadAsStringAsync();
            Console.WriteLine($"[AuthService] POST api/account/Auth/login RESPONSE: {(int)resp.StatusCode} {resp.StatusCode} - {respText}");

            if (!resp.IsSuccessStatusCode)
            {
                _dialogs.ShowError($"Login failed ({(int)resp.StatusCode}): {respText}");
                return false;
            }

            var auth = NormalizeAuthResponse(respText, JsonSerializer.Deserialize<AuthResponseDto>(respText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }));
            if (!string.IsNullOrWhiteSpace(auth.Redirect))
            {
                _session.ApplyRoleHint(auth.Redirect);
            }
            if (!string.IsNullOrWhiteSpace(auth.AccessToken))
            {
                _session.AccessToken = auth.AccessToken;
                _session.RefreshToken = auth.RefreshToken;
            }
            else
            {
                _session.MarkCookieAuthenticated();
            }

            await LoadProfileAsync(client);

            _storage.Save(new SessionSnapshot(
                _session.AccessToken,
                _session.RefreshToken,
                _session.CurrentUserId,
                _session.DisplayName,
                _session.Email,
                _session.FirstName,
                _session.LastName,
                _session.AvatarUrl,
                _session.DateOfBirth,
                _session.Roles.ToArray(),
                _session.IsAdmin,
                _session.IsDoctor,
                _session.HasCookieSession,
                string.IsNullOrWhiteSpace(auth.Redirect) ? _session.LastRedirect : auth.Redirect));
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AuthService] Login exception: {ex}");
            _dialogs.ShowError("An error occurred while trying to log in. See console for details.");
            return false;
        }
    }

    public async Task<bool> RegisterAsync(RegisterRequestDto request)
    {
        try
        {
            var client = _httpFactory.CreateClient("BookingCareApi");

            var reqJson = JsonSerializer.Serialize(request);
            Console.WriteLine($"[AuthService] POST api/account/Auth/register REQUEST: {reqJson}");

            var resp = await client.PostAsJsonAsync("api/account/Auth/register", request);
            var respText = await resp.Content.ReadAsStringAsync();
            Console.WriteLine($"[AuthService] POST api/account/Auth/register RESPONSE: {(int)resp.StatusCode} {resp.StatusCode} - {respText}");

            if (!resp.IsSuccessStatusCode)
            {
                _dialogs.ShowError($"Register failed ({(int)resp.StatusCode}): {respText}");
                return false;
            }

            var auth = NormalizeAuthResponse(respText, JsonSerializer.Deserialize<AuthResponseDto>(respText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }));
            if (!string.IsNullOrWhiteSpace(auth.Redirect))
            {
                _session.ApplyRoleHint(auth.Redirect);
            }

            if (auth == null || string.IsNullOrWhiteSpace(auth.AccessToken))
            {
                _dialogs.ShowError($"Register response did not contain an access token.\nResponse body:\n{respText}");
                return false;
            }

            _session.AccessToken = auth.AccessToken;
            _session.RefreshToken = auth.RefreshToken;

            await LoadProfileAsync(client);

            _storage.Save(new SessionSnapshot(
                _session.AccessToken,
                _session.RefreshToken,
                _session.CurrentUserId,
                _session.DisplayName,
                _session.Email,
                _session.FirstName,
                _session.LastName,
                _session.AvatarUrl,
                _session.DateOfBirth,
                _session.Roles.ToArray(),
                _session.IsAdmin,
                _session.IsDoctor,
                _session.HasCookieSession,
                string.IsNullOrWhiteSpace(auth.Redirect) ? _session.LastRedirect : auth.Redirect));
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AuthService] Register exception: {ex}");
            _dialogs.ShowError("An error occurred while trying to register. See console for details.");
            return false;
        }
    }

    private async Task LoadProfileAsync(HttpClient client)
    {
        try
        {
            var response = await client.GetAsync("api/account/auth/profile");
            if (!response.IsSuccessStatusCode)
            {
                return;
            }

            var profile = await response.Content.ReadFromJsonAsync<UserProfileDto>();
            if (profile is not null)
            {
                _session.ApplyProfile(profile);
            }
        }
        catch
        {
            // ignore profile errors to avoid breaking login
        }
    }
    public async Task<bool> LogoutAsync()
    {
        try
        {
            var client = _httpFactory.CreateClient("BookingCareApi");
            try
            {
                var resp = await client.PostAsync("api/account/Auth/logout", null);
                // ignore response status; proceed to clear local state
            }
            catch
            {
                // ignore network errors on logout
            }

            _storage.Clear();
            _session.Clear();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AuthService] Logout exception: {ex}");
            return false;
        }
    }
    private AuthResponseDto NormalizeAuthResponse(string rawResponse, AuthResponseDto? auth)
    {
        auth ??= new AuthResponseDto();
        if (!string.IsNullOrWhiteSpace(auth.AccessToken) && !string.IsNullOrWhiteSpace(auth.RefreshToken))
        {
            return auth;
        }

        try
        {
            using var doc = JsonDocument.Parse(rawResponse ?? string.Empty);
            var token = string.IsNullOrWhiteSpace(auth.AccessToken) ? FindTokenValue(doc.RootElement) : auth.AccessToken;
            var refresh = string.IsNullOrWhiteSpace(auth.RefreshToken) ? FindRefreshValue(doc.RootElement) : auth.RefreshToken;
            if (!string.IsNullOrWhiteSpace(token))
            {
                auth.AccessToken = token;
            }
            if (!string.IsNullOrWhiteSpace(refresh))
            {
                auth.RefreshToken = refresh;
            }
        }
        catch (JsonException)
        {
            // ignore parse issues
        }

        return auth;
    }
}
