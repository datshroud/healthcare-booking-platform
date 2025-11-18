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
        if (User?.Identity?.IsAuthenticated == true && IsAuthReturnRequest())
        {
            if (User.IsInRole("Admin"))
            {
                return Redirect("/dashboard");
            }

            if (User.IsInRole("Doctor"))
            {
                return Redirect("/doctor/dashboard");
            }
        }

        return Page();
    }

    private bool IsAuthReturnRequest()
    {
        if (Request is null)
        {
            return false;
        }

        if (Request.Query.TryGetValue("authRedirect", out var flag)
            && flag.Any(value => string.Equals(value, "1", StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        var referer = Request.Headers["Referer"].ToString();
        if (string.IsNullOrWhiteSpace(referer))
        {
            return false;
        }

        return referer.Contains("/Identity/Account/Login", StringComparison.OrdinalIgnoreCase)
            || referer.Contains("/Identity/Account/Register", StringComparison.OrdinalIgnoreCase)
            || referer.Contains("/Identity/Account/ExternalLogin", StringComparison.OrdinalIgnoreCase);
    }
}