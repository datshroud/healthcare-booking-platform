using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BookingCareManagement.Application.Common.Exceptions;
using BookingCareManagement.Application.Features.Auth.Dtos;
using BookingCareManagement.Domain.Aggregates.User;
using Microsoft.AspNetCore.Identity;

namespace BookingCareManagement.Application.Features.Auth.Commands
{
    public class LoginHandler
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;

        private readonly IJwtTokenGenerator _jwt;

        public LoginHandler(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, IJwtTokenGenerator jwt)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _jwt = jwt;
        }

        public async Task<AuthResponse> Handle(LoginRequest req, CancellationToken ct = default)
        {
            var user = await _userManager.FindByEmailAsync(req.Email)
                ?? throw new AuthException("Email không tồn tại.");

            var check = await _signInManager.CheckPasswordSignInAsync(user, req.Password, lockoutOnFailure: false);
            if (!check.Succeeded) 
                throw new AuthException("Sai mật khẩu.");

            var roles = await _userManager.GetRolesAsync(user);
            var (token, exp) = _jwt.GenerateToken(user, roles);

            var refresh = RefreshTokenFactory.Create();
            user.RefreshTokens.Add(refresh);
            await _userManager.UpdateAsync(user);

            return new AuthResponse(token, exp, refresh.Token);
        }
    }
}
