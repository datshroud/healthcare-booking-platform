using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookingCareManagement.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetSecureAppointments() => Ok(new { ok = true });
    }
}
