using Microsoft.AspNetCore.Mvc;

namespace BookingCareManagement.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
