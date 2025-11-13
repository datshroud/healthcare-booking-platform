using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BookingCareManagement.Application.Features.Auth.Commands;
using BookingCareManagement.Domain.Aggregates.User;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BookingCareManagement.Infrastructure.Identity.Jwt
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly JwtSettings _setting;
        private readonly TokenValidationParameters _tokenValidationParameters;

        public JwtTokenGenerator(IOptions<JwtSettings> options)
        {
            _setting = options.Value;
            _tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _setting.Issuer,
                ValidAudience = _setting.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_setting.Key))
            };
        }

        public (string Token, DateTime expiresAt) GenerateToken(AppUser user, IEnumerable<string> roles)
        {
            var now = DateTime.UtcNow;
            var displayName = user.GetFullName();
            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = user.Email ?? string.Empty;
            }

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                // Use derived full name (first + last) as the registered 'name' claim, fallback to email
                new(JwtRegisteredClaimNames.Name, displayName),
                // also include ClaimTypes.Name for frameworks that read that
                new(ClaimTypes.Name, displayName)
            };
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var creds = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_setting.Key)),
                SecurityAlgorithms.HmacSha256);

            var expires = now.AddMinutes(_setting.ExpireMinutes);

            var jwt = new JwtSecurityToken(
                issuer: _setting.Issuer,
                audience: _setting.Audience,
                claims: claims,
                notBefore: now,
                expires: expires,
                signingCredentials: creds);

            return (new JwtSecurityTokenHandler().WriteToken(jwt), expires);
        }
        
        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(token, _tokenValidationParameters, out var validatedToken);
                if (validatedToken is not JwtSecurityToken jwtToken ||
                    !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }
                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}
