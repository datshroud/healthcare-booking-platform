using BookingCareManagement.Application.Common.Exceptions;
using BookingCareManagement.Application.Features.Auth.Dtos;
using BookingCareManagement.Application.Features.Auth.Interfaces;
using BookingCareManagement.Domain.Aggregates.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingCareManagement.Application.Features.Auth.Commands
{
    public class RegisterHandler
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IJwtTokenGenerator _jwt;

        public RegisterHandler(UserManager<AppUser> userManager, IJwtTokenGenerator jwt)
        {
            _userManager = userManager;
            _jwt = jwt;
        }

        public async Task<AuthResponse> Handle(RegisterRequest req, CancellationToken ct = default)
        {
            // build user from request fields
            var user = new AppUser
            {
                UserName = req.Email,
                Email = req.Email,
                FirstName = req.FirstName,
                LastName = req.LastName,
                FullName = string.Join(' ', new[] { req.FirstName, req.LastName }.Where(s => !string.IsNullOrWhiteSpace(s))).Trim(),
                PhoneNumber = req.PhoneNumber,
                DateOfBirth = req.DateOfBirth,
                EmailConfirmed = true
            };

            var res = await _userManager.CreateAsync(user, req.Password);
            if (!res.Succeeded) throw new AuthException(string.Join("; ", res.Errors.Select(e => e.Description)));

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Any()) {
                await _userManager.AddToRoleAsync(user, "Customer");
                roles = await _userManager.GetRolesAsync(user);
            }
            var (token, exp) = _jwt.GenerateToken(user, roles);
            var refresh = RefreshTokenFactory.Create();
            user.RefreshTokens.Add(refresh);

            await _userManager.UpdateAsync(user);
            return new AuthResponse(token, exp, refresh.Token);
        }
    }
}
