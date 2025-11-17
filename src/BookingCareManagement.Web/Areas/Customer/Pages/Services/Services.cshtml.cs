using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookingCareManagement.Areas.Customer.Pages.Services
{
    [Authorize]
    public class ServicesModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
