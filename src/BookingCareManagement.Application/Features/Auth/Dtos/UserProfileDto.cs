using System;

namespace BookingCareManagement.Application.Features.Auth.Dtos
{
    public sealed class UserProfileDto
    {
        public string UserId { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string[] Roles { get; set; } = Array.Empty<string>();
        public bool IsAdmin { get; set; }
        public bool IsDoctor { get; set; }
    }
}
