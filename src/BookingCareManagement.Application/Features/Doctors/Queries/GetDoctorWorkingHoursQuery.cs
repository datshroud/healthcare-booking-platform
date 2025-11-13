using System.Linq;
using BookingCareManagement.Application.Common.Exceptions;
using BookingCareManagement.Domain.Abstractions;

namespace BookingCareManagement.Application.Features.Doctors.Queries;

public record DoctorWorkingHourDto(Guid Id, int DayOfWeek, string StartTime, string EndTime, string? Location);

public record DoctorWorkingHoursDto(Guid DoctorId, bool LimitAppointments, IReadOnlyCollection<DoctorWorkingHourDto> Hours);

public record GetDoctorWorkingHoursQuery(Guid DoctorId);

public class GetDoctorWorkingHoursQueryHandler
{
    private readonly IDoctorRepository _doctorRepository;

    public GetDoctorWorkingHoursQueryHandler(IDoctorRepository doctorRepository)
    {
        _doctorRepository = doctorRepository;
    }

    public async Task<DoctorWorkingHoursDto> Handle(GetDoctorWorkingHoursQuery query, CancellationToken cancellationToken)
    {
        var doctor = await _doctorRepository.GetByIdAsync(query.DoctorId, cancellationToken);
        if (doctor is null)
        {
            throw new NotFoundException($"Doctor with ID {query.DoctorId} was not found.");
        }

        var hours = doctor.WorkingHours
            .OrderBy(h => h.DayOfWeek)
            .ThenBy(h => h.StartTime)
            .Select(h => new DoctorWorkingHourDto(
                h.Id,
                (int)h.DayOfWeek,
                h.StartTime.ToString("hh\\:mm"),
                h.EndTime.ToString("hh\\:mm"),
                h.Location))
            .ToArray();

        return new DoctorWorkingHoursDto(doctor.Id, doctor.LimitAppointments, hours);
    }
}
