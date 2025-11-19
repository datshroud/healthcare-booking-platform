using System.Linq;

// ĐẢM BẢO NAMESPACE NÀY KHỚP 100% VỚI FILE AppUser.cs
namespace BookingCareManagement.Domain.Aggregates.User
{
    public static class AppUserExtensions
    {
        public static string GetFullName(this AppUser? user)
        {
            if (user is null)
            {
                return string.Empty;
            }

            // ✅ SỬA: Họ (LastName) trước, Tên (FirstName) sau (theo quy ước Việt Nam)
            var parts = new[] { user.LastName, user.FirstName }
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
}