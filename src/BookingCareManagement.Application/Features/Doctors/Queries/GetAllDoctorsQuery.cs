using BookingCareManagement.Application.Features.Doctors.Dtos;
using BookingCareManagement.Domain.Abstractions;
using BookingCareManagement.Domain.Aggregates.Doctor;

namespace BookingCareManagement.Application.Features.Doctors.Queries;

// Class Query không đổi
public class GetAllDoctorsQuery
{
}

// Handler thay đổi hoàn toàn
public class GetAllDoctorsQueryHandler
{
    private readonly IDoctorRepository _doctorRepository;

    public GetAllDoctorsQueryHandler(IDoctorRepository doctorRepository)
    {
        _doctorRepository = doctorRepository;
    }

    public async Task<IEnumerable<DoctorDto>> Handle(CancellationToken cancellationToken)
    {
        // 1. Repository giờ sẽ trả về Doctor (với AppUser gộp vào)
        IEnumerable<Doctor> doctors = await _doctorRepository.GetAllAsync(cancellationToken);

        // 2. Map sang DTO
        var doctorDtos = doctors.Select(d => new DoctorDto
        {
            Id = d.Id,
            AppUserId = d.AppUserId,
            // Lấy thông tin từ AppUser liên kết
            FirstName = d.AppUser.FirstName ?? string.Empty,
            LastName = d.AppUser.LastName ?? string.Empty,
            FullName = d.AppUser.FullName ?? string.Empty,
            Email = d.AppUser.Email ?? string.Empty,
            PhoneNumber = d.AppUser.PhoneNumber ?? string.Empty,
            Active = d.Active,
            Specialties = d.Specialties.Select(s => s.Name)
        });

        return doctorDtos;
    }
}