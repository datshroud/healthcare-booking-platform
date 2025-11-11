using BookingCareManagement.Application.Features.Doctors.Dtos;
using BookingCareManagement.Domain.Abstractions;
using BookingCareManagement.Domain.Aggregates.Doctor;
using BookingCareManagement.Domain.Aggregates.User; // Cần cho AppUser
using Microsoft.AspNetCore.Identity; // Cần cho UserManager

namespace BookingCareManagement.Application.Features.Doctors.Commands;

// 1. Command DTO: Nhận đầy đủ thông tin từ Form
public class CreateDoctorCommand
{
    // Thông tin cho AppUser
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;

    // Thông tin cho Doctor
    public IEnumerable<Guid> SpecialtyIds { get; set; } = new List<Guid>();
}

// 2. Handler: Logic nghiệp vụ mới
public class CreateDoctorCommandHandler
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly ISpecialtyRepository _specialtyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<AppUser> _userManager; // Dịch vụ để tạo User

    public CreateDoctorCommandHandler(
        IDoctorRepository doctorRepository,
        ISpecialtyRepository specialtyRepository,
        IUnitOfWork unitOfWork,
        UserManager<AppUser> userManager) // Tiêm UserManager vào
    {
        _doctorRepository = doctorRepository;
        _specialtyRepository = specialtyRepository;
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public async Task<DoctorDto> Handle(CreateDoctorCommand command, CancellationToken cancellationToken)
    {
        // 1. Tạo AppUser trước
        var appUser = new AppUser
        {
            FirstName = command.FirstName,
            LastName = command.LastName,
            FullName = $"{command.FirstName} {command.LastName}",
            Email = command.Email,
            UserName = command.Email, // Dùng Email làm UserName
            PhoneNumber = command.PhoneNumber,
            EmailConfirmed = true // Tạm thời để true cho dễ test
        };

        // Mật khẩu mặc định cho bác sĩ mới, họ nên đổi sau
        var result = await _userManager.CreateAsync(appUser, "Doctor123!");
        if (!result.Succeeded)
        {
            // Ném lỗi nếu tạo user thất bại (ví dụ: Email trùng)
            throw new Exception($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        // 2. Gán Role "Doctor" cho AppUser vừa tạo
        await _userManager.AddToRoleAsync(appUser, "Doctor");

        // 3. Tạo Doctor Entity (dùng AppUser.Id)
        var doctor = new Doctor(appUser.Id);

        // 4. Tìm và thêm chuyên khoa
        if (command.SpecialtyIds.Any())
        {
            var specialties = await _specialtyRepository.GetByIdsAsync(command.SpecialtyIds, cancellationToken);
            foreach (var specialty in specialties)
            {
                doctor.AddSpecialty(specialty);
            }
        }

        // 5. Thêm Doctor vào Repository
        _doctorRepository.Add(doctor);

        // 6. Lưu tất cả thay đổi (cả User và Doctor)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 7. Map sang DTO để trả về
        return new DoctorDto
        {
            Id = doctor.Id,
            AppUserId = doctor.AppUserId,
            FirstName = appUser.FirstName ?? string.Empty,
            LastName = appUser.LastName ?? string.Empty,
            FullName = appUser.FullName ?? string.Empty,
            Email = appUser.Email ?? string.Empty,
            PhoneNumber = appUser.PhoneNumber ?? string.Empty,
            Active = doctor.Active,
            AvatarUrl = appUser.AvatarUrl ?? string.Empty,
            Specialties = doctor.Specialties.Select(s => s.Name)
        };
    }
}