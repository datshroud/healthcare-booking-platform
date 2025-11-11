using System.Linq;
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
        var firstName = doctor.AppUser.FirstName ?? string.Empty;
        var lastName = doctor.AppUser.LastName ?? string.Empty;
        var combined = string.Join(' ', new[] { firstName, lastName }.Where(s => !string.IsNullOrWhiteSpace(s))).Trim();
        var fullName = !string.IsNullOrWhiteSpace(doctor.AppUser.FullName)
            ? doctor.AppUser.FullName!
            : (!string.IsNullOrWhiteSpace(combined)
                ? combined
                : (doctor.AppUser.Email ?? doctor.AppUser.UserName ?? "Bác sĩ"));

        return new DoctorDto
        {
            Id = doctor.Id,
            AppUserId = doctor.AppUserId,
            FirstName = firstName,
            LastName = lastName,
            FullName = fullName,
            Email = doctor.AppUser.Email ?? string.Empty,
            PhoneNumber = doctor.AppUser.PhoneNumber ?? string.Empty,
            Active = doctor.Active,
            AvatarUrl = doctor.AppUser.AvatarUrl ?? string.Empty,
            Specialties = doctor.Specialties.Select(s => s.Name)
        };
    }
}