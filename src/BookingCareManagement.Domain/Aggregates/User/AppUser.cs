using Microsoft.AspNetCore.Identity;
using System; // Thêm
using System.Collections.Generic; // Thêm

namespace BookingCareManagement.Domain.Aggregates.User
{
    public class AppUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? InternalNote { get; set; }
        public string? Description { get; set; }

        // THÊM DÒNG NÀY ĐỂ SỬA LỖI BUILD:
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<RefreshToken> RefreshTokens { get; set; } = new();
    }
}