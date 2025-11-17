using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookingCareManagement.Areas.Customer.Pages.MyBookings
{
    [Authorize]
    public class MyBookingsModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
