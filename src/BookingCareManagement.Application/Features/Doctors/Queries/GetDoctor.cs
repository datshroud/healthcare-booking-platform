using BookingCareManagement.Application.Common.Exceptions;
using BookingCareManagement.Application.Features.Doctors.Dtos;
using BookingCareManagement.Domain.Abstractions;

namespace BookingCareManagement.Application.Features.Doctors.Queries;

// Query không đổi
public class GetDoctorByIdQuery
{
    public Guid Id { get; set; }
}

// Handler thay đổi hoàn toàn
public class GetDoctorByIdQueryHandler
{
    private readonly IDoctorRepository _doctorRepository;

    public GetDoctorByIdQueryHandler(IDoctorRepository doctorRepository)
    {
        _doctorRepository = doctorRepository;
    }

    public async Task<DoctorDto> Handle(GetDoctorByIdQuery query, CancellationToken cancellationToken)
    {
        var doctor = await _doctorRepository.GetByIdAsync(query.Id, cancellationToken);

        if (doctor is null)
        {
            throw new NotFoundException($"Doctor with ID {query.Id} was not found.");
        }

        // Map sang DTO
        return new DoctorDto
        {
            Id = doctor.Id,
            AppUserId = doctor.AppUserId,
            // Lấy thông tin từ AppUser liên kết
            FirstName = doctor.AppUser.FirstName ?? string.Empty,
            LastName = doctor.AppUser.LastName ?? string.Empty,
            FullName = doctor.AppUser.FullName ?? string.Empty,
            Email = doctor.AppUser.Email ?? string.Empty,
            PhoneNumber = doctor.AppUser.PhoneNumber ?? string.Empty,
            Active = doctor.Active,
            Specialties = doctor.Specialties.Select(s => s.Name)
        };
    }
}