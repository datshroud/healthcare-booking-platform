using BookingCareManagement.Domain.Aggregates.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BookingCareManagement.Application.Features.Auth.Commands
{
    public interface IJwtTokenGenerator
    {
        (string Token, DateTime expiresAt) GenerateToken(AppUser user, IEnumerable<string> roles);
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}
