using BookingCareManagement.Domain.Abstractions;
using BookingCareManagement.Domain.Aggregates.Doctor;
using BookingCareManagement.Domain.Aggregates.User; // Đảm bảo using này tồn tại
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BookingCareManagement.Web.Areas.Admin.Pages.Doctors.Edit
{
    public class SpecialtiesModel : PageModel
    {
        private readonly IDoctorRepository _doctorRepository;
        private readonly ISpecialtyRepository _specialtyRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SpecialtiesModel(
            IDoctorRepository doctorRepository,
            ISpecialtyRepository specialtyRepository,
            IUnitOfWork unitOfWork)
        {
            _doctorRepository = doctorRepository;
            _specialtyRepository = specialtyRepository;
            _unitOfWork = unitOfWork;
        }

        // --- Properties cho hiển thị (GET) ---

        [BindProperty(SupportsGet = true)]
        public Guid DoctorId { get; set; }

        public string DoctorName { get; set; } = "Bác sĩ";
        public List<Specialty> AllSpecialties { get; set; } = new();

        public Guid? AssignedSpecialtyId { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        // --- Handlers ---

        public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
        {
            var doctor = await _doctorRepository.GetByIdAsync(DoctorId, cancellationToken);
            if (doctor == null)
            {
                return NotFound($"Không tìm thấy bác sĩ với ID {DoctorId}");
            }

            await PopulatePageStateAsync(doctor, cancellationToken);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid? selectedSpecialtyId, CancellationToken cancellationToken)
        {
            var doctor = await _doctorRepository.GetByIdWithTrackingAsync(DoctorId, cancellationToken);
            if (doctor == null)
            {
                return NotFound($"Không tìm thấy bác sĩ với ID {DoctorId}");
            }

            doctor.ClearSpecialties();

            if (selectedSpecialtyId.HasValue && selectedSpecialtyId.Value != Guid.Empty)
            {
                var specialty = await _specialtyRepository.GetByIdAsync(selectedSpecialtyId.Value, cancellationToken);
                if (specialty != null)
                {
                    doctor.AddSpecialty(specialty);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Chuyên khoa đã chọn không hợp lệ.");
                    // Chỗ 'doctor' bị gạch đỏ là ở đây, vì trình biên dịch không hiểu 'doctor' là kiểu gì
                    await PopulatePageStateAsync(doctor, cancellationToken);
                    return Page();
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            StatusMessage = "Cập nhật chuyên khoa cho bác sĩ thành công.";
            return RedirectToPage(new { doctorId = DoctorId });
        }

        // 
        // ⭐️⭐️⭐️ SỬA LỖI Ở ĐÂY ⭐️⭐️⭐️
        //
        // Hàm private helper để tránh lặp code
        // Ghi rõ đầy đủ namespace (BookingCareManagement.Domain.Aggregates.Doctor.Doctor)
        // để trình biên dịch không nhầm lẫn với thư mục 'Doctors'
        private async Task PopulatePageStateAsync(BookingCareManagement.Domain.Aggregates.Doctor.Doctor doctor, CancellationToken cancellationToken)
        {
            var allSpecs = await _specialtyRepository.GetAllAsync(cancellationToken);

            DoctorName = doctor.AppUser.GetFullName() ?? doctor.AppUser.Email;
            AllSpecialties = allSpecs.OrderBy(s => s.Name).ToList();
            AssignedSpecialtyId = doctor.Specialties.Select(s => (Guid?)s.Id).FirstOrDefault();
        }
    }
}