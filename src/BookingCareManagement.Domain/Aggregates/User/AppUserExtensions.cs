using System.Linq;

namespace BookingCareManagement.Domain.Aggregates.User;

public static class AppUserExtensions
{
    public static string GetFullName(this AppUser? user)
    {
        if (user is null)
        {
            return string.Empty;
        }

        var parts = new[] { user.FirstName, user.LastName }
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s!.Trim());

        var displayName = string.Join(' ', parts);
        if (!string.IsNullOrWhiteSpace(displayName))
        {
            return displayName;
        }

        return (user.Email ?? user.UserName ?? string.Empty).Trim();
    }
}
