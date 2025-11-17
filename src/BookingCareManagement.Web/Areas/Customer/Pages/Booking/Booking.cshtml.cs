using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookingCareManagement.Areas.Customer.Pages.Booking
{
    [Authorize]
    public class BookingModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
