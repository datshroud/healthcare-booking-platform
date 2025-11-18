using System;

namespace BookingCareManagement.WinForms.Shared.State;

public interface IAuthStorage
{
    void Save(SessionSnapshot snapshot);
    SessionSnapshot? Load();
    void Clear();
}

public sealed record SessionSnapshot(string? AccessToken, string? RefreshToken, Guid? UserId);
