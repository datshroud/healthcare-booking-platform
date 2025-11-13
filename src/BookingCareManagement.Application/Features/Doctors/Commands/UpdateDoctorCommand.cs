using BookingCareManagement.Application.Common.Exceptions;
using BookingCareManagement.Domain.Abstractions;
using BookingCareManagement.Domain.Aggregates.User; // Cần cho AppUser
using Microsoft.AspNetCore.Identity; // Cần cho UserManager

namespace BookingCareManagement.Application.Features.Doctors.Commands;

// 1. DTO cho Request Body (không đổi)
public class UpdateDoctorRequest
{
    // Sửa lại cho khớp Form
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public IEnumerable<Guid> SpecialtyIds { get; set; } = new List<Guid>();
}

// 2. Command (sửa lại)
public class UpdateDoctorCommand
{
    public Guid Id { get; set; } // Doctor.Id
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public IEnumerable<Guid> SpecialtyIds { get; set; } = new List<Guid>();
}

// 3. Handler (thay đổi logic)
public class UpdateDoctorCommandHandler
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly ISpecialtyRepository _specialtyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<AppUser> _userManager; // Cần để update User

    public UpdateDoctorCommandHandler(
        IDoctorRepository doctorRepository,
        ISpecialtyRepository specialtyRepository,
        IUnitOfWork unitOfWork,
        UserManager<AppUser> userManager) // Tiêm vào
    {
        _doctorRepository = doctorRepository;
        _specialtyRepository = specialtyRepository;
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public async Task Handle(UpdateDoctorCommand command, CancellationToken cancellationToken)
    {
        // 1. Lấy Doctor (bản CÓ Tracking)
        var doctor = await _doctorRepository.GetByIdWithTrackingAsync(command.Id, cancellationToken);

        if (doctor is null)
        {
            throw new NotFoundException($"Doctor with ID {command.Id} was not found.");
        }

        // 2. Cập nhật AppUser liên kết
        // (doctor.AppUser đã được gộp vào nhờ GetByIdWithTrackingAsync)
        var appUser = doctor.AppUser;
        appUser.FirstName = command.FirstName;
        appUser.LastName = command.LastName;
        appUser.Email = command.Email;
        appUser.UserName = command.Email;
        appUser.PhoneNumber = command.PhoneNumber;

        var userResult = await _userManager.UpdateAsync(appUser);
        if (!userResult.Succeeded)
        {
            throw new Exception($"Failed to update user: {string.Join(", ", userResult.Errors.Select(e => e.Description))}");
        }

        // 3. Cập nhật chuyên khoa (logic không đổi)
        doctor.ClearSpecialties();
        if (command.SpecialtyIds.Any())
        {
            var specialties = await _specialtyRepository.GetByIdsAsync(command.SpecialtyIds, cancellationToken);
            foreach (var specialty in specialties)
            {
                doctor.AddSpecialty(specialty);
            }
        }

        // 4. Lưu thay đổi
        // (Vì AppUser và Doctor cùng 1 DbContext, nó sẽ lưu cả 2)
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}