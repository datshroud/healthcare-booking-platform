namespace BookingCareManagement.WinForms.Shared.Models;

public sealed class UserProfileDto
{
    public string UserId { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? AvatarUrl { get; init; }
    public DateTime? DateOfBirth { get; init; }
    public bool IsAdmin { get; init; }
    public bool IsDoctor { get; init; }
    public string[] Roles { get; init; } = Array.Empty<string>();
}
