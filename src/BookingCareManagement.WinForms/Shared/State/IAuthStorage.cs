using System;

namespace BookingCareManagement.WinForms.Shared.State;

public interface IAuthStorage
{
    void Save(SessionSnapshot snapshot);
    SessionSnapshot? Load();
    void Clear();
}

public sealed record SessionSnapshot(
    string? AccessToken,
    string? RefreshToken,
    string? UserId,
    string? DisplayName = null,
    string? Email = null,
    string[]? Roles = null,
    bool IsAdmin = false,
    bool IsDoctor = false,
    bool CookieAuthenticated = false,
    string? Redirect = null);
