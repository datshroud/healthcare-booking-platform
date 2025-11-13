using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookingCareManagement.Domain.Abstractions;
using BookingCareManagement.Domain.Aggregates.User;
using Microsoft.AspNetCore.Identity;

namespace BookingCareManagement.Web.Areas.Admin.Pages.Doctors.Edit;

public class SpecialDaysModel : PageModel
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly UserManager<AppUser> _userManager;

    public SpecialDaysModel(IDoctorRepository doctorRepository, UserManager<AppUser> userManager)
    {
        _doctorRepository = doctorRepository;
        _userManager = userManager;
    }

    [BindProperty(SupportsGet = true)]
    public Guid DoctorId { get; set; }

    public string DoctorName { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync()
    {
        var doctor = await _doctorRepository.GetByIdAsync(DoctorId);
        if (doctor == null)
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(doctor.AppUserId);
        var displayName = user.GetFullName();
        DoctorName = string.IsNullOrWhiteSpace(displayName) ? (user?.Email ?? "Bác sĩ") : displayName;

        return Page();
    }
}
