using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookingCareManagement.Web.Areas.Account.Pages.Auth
{
    public class ForgotPasswordModel : PageModel
    {
        public IActionResult OnGet()
        {
            if (User?.Identity?.IsAuthenticated == true)
                return Redirect("/");
            return Page();
        }
    }
}
