using System.Linq;
using BookingCareManagement.Application.Features.Doctors.Dtos;
using BookingCareManagement.Domain.Abstractions;
using BookingCareManagement.Domain.Aggregates.Doctor;
using BookingCareManagement.Domain.Aggregates.User;

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
        var doctorDtos = doctors.Select(d =>
        {
            var firstName = d.AppUser.FirstName ?? string.Empty;
            var lastName = d.AppUser.LastName ?? string.Empty;
            var fullName = d.AppUser.GetFullName();
            if (string.IsNullOrWhiteSpace(fullName))
            {
                fullName = d.AppUser.Email ?? d.AppUser.UserName ?? "Bác sĩ";
            }

            return new DoctorDto
            {
                Id = d.Id,
                AppUserId = d.AppUserId,
                FirstName = firstName,
                LastName = lastName,
                FullName = fullName,
                Email = d.AppUser.Email ?? string.Empty,
                PhoneNumber = d.AppUser.PhoneNumber ?? string.Empty,
                Active = d.Active,
                AvatarUrl = d.AppUser.AvatarUrl ?? string.Empty,
                Specialties = d.Specialties.Select(s => s.Name)
            };
        });

        return doctorDtos;
    }
}