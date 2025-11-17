using BookingCareManagement.Application.Common.Exceptions;
using BookingCareManagement.Application.Features.Doctors.Dtos;
using BookingCareManagement.Domain.Abstractions;

namespace BookingCareManagement.Application.Features.Doctors.Queries;

public class GetDoctorProfileQuery
{
    public Guid DoctorId { get; set; }
}

public class GetDoctorProfileQueryHandler
{
    private readonly IDoctorRepository _doctorRepository;

    public GetDoctorProfileQueryHandler(IDoctorRepository doctorRepository)
    {
        _doctorRepository = doctorRepository;
    }

    public async Task<DoctorProfileDto> Handle(GetDoctorProfileQuery query, CancellationToken cancellationToken)
    {
        var doctor = await _doctorRepository.GetByIdAsync(query.DoctorId, cancellationToken);
        if (doctor is null)
        {
            throw new NotFoundException($"Doctor with ID {query.DoctorId} was not found.");
        }

        var user = doctor.AppUser;

        return new DoctorProfileDto
        {
            DoctorId = doctor.Id,
            AppUserId = doctor.AppUserId,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            DateOfBirth = user.DateOfBirth,
            Description = user.Description,
            AvatarUrl = user.AvatarUrl
        };
    }
}
