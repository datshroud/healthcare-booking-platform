using System;
using System.Threading.Tasks;
using BookingCareManagement.Domain.Aggregates.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookingCareManagement.Web.Areas.Doctor.Pages.Dashboard;

[Authorize(Roles = "Doctor,Admin")]
public class dashboardModel : PageModel
{
    private readonly UserManager<AppUser> _userManager;

    public dashboardModel(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public string DoctorName { get; private set; } = "Bác sĩ";
    public string? AvatarUrl { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        DoctorName = ResolveDisplayName(user);
        AvatarUrl = ResolveAvatar(user);
        return Page();
    }

    private static string ResolveDisplayName(AppUser user)
    {
        var name = user.GetFullName() ?? user.Email ?? user.UserName ?? "Bác sĩ";
        return string.IsNullOrWhiteSpace(name) ? "Bác sĩ" : name.Trim();
    }

    private static string? ResolveAvatar(AppUser user)
    {
        if (!string.IsNullOrWhiteSpace(user.AvatarUrl))
        {
            return user.AvatarUrl;
        }

        var displayName = ResolveDisplayName(user);
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return null;
        }

        var encoded = Uri.EscapeDataString(displayName);
        return $"https://ui-avatars.com/api/?name={encoded}&background=0ea5e9&color=fff";
    }
}
