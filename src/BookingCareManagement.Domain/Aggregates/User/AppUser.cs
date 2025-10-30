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
        public string? FullName { get; set; } 
        public List<RefreshToken> RefreshTokens { get; set; } = new();
    }
}
