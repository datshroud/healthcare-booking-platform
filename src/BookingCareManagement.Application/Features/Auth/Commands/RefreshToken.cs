using System;
using BookingCareManagement.Application.Features.Auth.Dtos;
using BookingCareManagement.Domain.Aggregates.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookingCareManagement.Application.Features.Auth.Commands;

public class RefreshTokenHandler
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IJwtTokenGenerator _jwt;

    public RefreshTokenHandler(UserManager<AppUser> userManager, IJwtTokenGenerator jwt)
    {
        _userManager = userManager;
        _jwt = jwt;
    }

    public async Task<AuthResponse> Handle(RefreshTokenRequest req, CancellationToken ct = default)
    {
        var user = await _userManager.Users
            .Where(u => u.RefreshTokens.Any(rt => rt.Token == req.RefreshToken && rt.IsActive))
            .FirstOrDefaultAsync(ct) ?? throw new Exception("Refresh token không hợp lệ.");

        var roles = await _userManager.GetRolesAsync(user);
        var (token, exp) = _jwt.GenerateToken(user, roles);

        var old = user.RefreshTokens.First(rt => rt.Token == req.RefreshToken);
        old.RevokedAt = DateTime.UtcNow;
        var @new = RefreshTokenFactory.Create();
        user.RefreshTokens.Add(@new);
        await _userManager.UpdateAsync(user);
        return new AuthResponse(token, exp, @new.Token);
    }

}
