using Microsoft.AspNetCore.Identity;

namespace BookingCareManagement.Domain.Aggregates.User
{
    public class AppUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    public string? AvatarUrl { get; set; } // Dòng này bạn đã thêm

    // THÊM DÒNG NÀY ĐỂ SỬA LỖI BUILD:
    public DateTime? DateOfBirth { get; set; } // Dùng DateTime? (nullable)

    public string? Description { get; set; }

        public List<RefreshToken> RefreshTokens { get; set; } = new();
    }
}