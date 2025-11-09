using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookingCareManagement.Web.Areas.Public.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public IActionResult OnGet()
    {
        // If the user is already authenticated and is an admin, redirect them to the admin dashboard.
        // This preserves the last-signed-in admin cookie so restarting the app will land the admin
        // on /dashboard instead of the public root.
        if (User?.Identity?.IsAuthenticated == true && (User.IsInRole("Admin") || User.IsInRole("Doctor")))
        {
            return Redirect("/dashboard");
        }

        return Page();
    }
}