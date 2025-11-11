using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingCareManagement.Domain.Aggregates.User
{
    public class AppUser : IdentityUser
    {
        // public string? Email { get; set; }
        // keep FullName for compatibility, but also store first/last separately
        public string? FullName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        // optional date of birth
        public DateTime? DateOfBirth { get; set; }
        public string? AvatarUrl { get; set; }
        public List<RefreshToken> RefreshTokens { get; set; } = new();
    }
}
