using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookingCareManagement.Domain.Abstractions;
using BookingCareManagement.Domain.Aggregates.User;
using BookingCareManagement.Domain.Aggregates.Doctor;
using Microsoft.AspNetCore.Identity;
using DoctorEntity = BookingCareManagement.Domain.Aggregates.Doctor.Doctor;

namespace BookingCareManagement.Web.Areas.Admin.Pages.Doctors.Edit;

public class SpecialtiesModel : PageModel
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly ISpecialtyRepository _specialtyRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    public SpecialtiesModel(
        IDoctorRepository doctorRepository,
        ISpecialtyRepository specialtyRepository,
        UserManager<AppUser> userManager,
        IUnitOfWork unitOfWork)
    {
        _doctorRepository = doctorRepository;
        _specialtyRepository = specialtyRepository;
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }

    [BindProperty(SupportsGet = true)]
    public Guid DoctorId { get; set; }

    public string DoctorName { get; set; } = string.Empty;
    public List<Specialty> AllSpecialties { get; set; } = new();
    public Guid? AssignedSpecialtyId { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var doctor = await _doctorRepository.GetByIdWithTrackingAsync(DoctorId);
        if (doctor == null)
        {
            return NotFound();
        }

        await PopulatePageStateAsync(doctor);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid? selectedSpecialtyId)
    {
        var doctor = await _doctorRepository.GetByIdWithTrackingAsync(DoctorId);
        if (doctor == null)
        {
            return NotFound();
        }

        if (!selectedSpecialtyId.HasValue || selectedSpecialtyId == Guid.Empty)
        {
            ModelState.AddModelError(string.Empty, "Vui lòng chọn một chuyên khoa.");
            await PopulatePageStateAsync(doctor);
            return Page();
        }

        var specialty = await _specialtyRepository.GetByIdAsync(selectedSpecialtyId.Value);
        if (specialty == null)
        {
            ModelState.AddModelError(string.Empty, "Chuyên khoa đã chọn không hợp lệ.");
            await PopulatePageStateAsync(doctor);
            return Page();
        }

        doctor.ClearSpecialties();
        doctor.AddSpecialty(specialty);

        await _unitOfWork.SaveChangesAsync();

        StatusMessage = "Đã lưu chuyên khoa thành công.";
        return RedirectToPage(new { doctorId = DoctorId });
    }

    private async Task PopulatePageStateAsync(DoctorEntity doctor)
    {
        var user = await _userManager.FindByIdAsync(doctor.AppUserId);
        var displayName = user.GetFullName();
        DoctorName = string.IsNullOrWhiteSpace(displayName) ? (user?.Email ?? "Bác sĩ") : displayName;

        AllSpecialties = (await _specialtyRepository.GetAllAsync()).ToList();
        AssignedSpecialtyId = doctor.Specialties.Select(s => (Guid?)s.Id).FirstOrDefault();
    }
}
