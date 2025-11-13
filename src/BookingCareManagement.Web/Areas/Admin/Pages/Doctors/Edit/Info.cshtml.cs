using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookingCareManagement.Domain.Abstractions;
using BookingCareManagement.Domain.Aggregates.User;
using Microsoft.AspNetCore.Identity;

namespace BookingCareManagement.Web.Areas.Admin.Pages.Doctors.Edit;

public class InfoModel : PageModel
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly UserManager<AppUser> _userManager;

    public InfoModel(IDoctorRepository doctorRepository, UserManager<AppUser> userManager)
    {
        _doctorRepository = doctorRepository;
        _userManager = userManager;
    }

    [BindProperty(SupportsGet = true)]
    public Guid DoctorId { get; set; }

    public string DoctorName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Description { get; set; }

    [BindProperty]
    public IFormFile? AvatarFile { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var doctor = await _doctorRepository.GetByIdAsync(DoctorId);
        if (doctor == null)
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(doctor.AppUserId);
        if (user == null)
        {
            return NotFound();
        }

        PopulateViewModel(user);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string firstName, string lastName, DateTime? dateOfBirth, string phoneNumber, string? description)
    {
        var doctor = await _doctorRepository.GetByIdAsync(DoctorId);
        if (doctor == null)
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(doctor.AppUserId);
        if (user == null)
        {
            return NotFound();
        }

        user.FirstName = firstName;
        user.LastName = lastName;
        user.DateOfBirth = dateOfBirth;
        user.PhoneNumber = phoneNumber;
        user.Description = description;

        if (AvatarFile != null)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "avatars");
            Directory.CreateDirectory(uploadsFolder);
            
            var uniqueFileName = $"{Guid.NewGuid()}_{AvatarFile.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await AvatarFile.CopyToAsync(fileStream);
            }

            user.AvatarUrl = $"/images/avatars/{uniqueFileName}";
        }

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            PopulateViewModel(user);
            return Page();
        }

        TempData["SuccessMessage"] = "Cập nhật thông tin bác sĩ thành công.";

        return RedirectToPage(new { doctorId = DoctorId });
    }

    private void PopulateViewModel(AppUser user)
    {
        var displayName = user.GetFullName();
        DoctorName = string.IsNullOrWhiteSpace(displayName) ? (user.Email ?? "Bác sĩ") : displayName;
        AvatarUrl = user.AvatarUrl;
        FirstName = user.FirstName ?? string.Empty;
        LastName = user.LastName ?? string.Empty;
        DateOfBirth = user.DateOfBirth;
        Email = user.Email ?? string.Empty;
        PhoneNumber = user.PhoneNumber;
        Description = user.Description;
    }
}
