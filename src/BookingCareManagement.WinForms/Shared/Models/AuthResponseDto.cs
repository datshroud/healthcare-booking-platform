using System;

namespace BookingCareManagement.WinForms.Shared.Models;

public sealed class AuthResponseDto
{
    public string? AccessToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? RefreshToken { get; set; }
    public string? Redirect { get; set; }
}
