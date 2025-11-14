using BookingCareManagement.Application.Common.Exceptions;
using BookingCareManagement.Application.Features.Doctors.Dtos;
using BookingCareManagement.Domain.Abstractions;
using BookingCareManagement.Domain.Aggregates.Doctor;

namespace BookingCareManagement.Application.Features.Doctors.Queries;

public class GetDoctorDayOffsQuery
{
    public Guid DoctorId { get; set; }
    public int? Year { get; set; }
}

public class GetDoctorDayOffsQueryHandler
{
    private readonly IDoctorRepository _doctorRepository;

    public GetDoctorDayOffsQueryHandler(IDoctorRepository doctorRepository)
    {
        _doctorRepository = doctorRepository;
    }

    public async Task<IEnumerable<DoctorDayOffDto>> Handle(GetDoctorDayOffsQuery query, CancellationToken cancellationToken)
    {
        var doctor = await _doctorRepository.GetByIdAsync(query.DoctorId, cancellationToken);
        if (doctor is null)
        {
            throw new NotFoundException($"Doctor with ID {query.DoctorId} was not found.");
        }

        var year = query.Year;
        return doctor.DaysOff
            .OrderBy(d => d.StartDate)
            .Select(d => ProjectDayOff(d, year))
            .ToList();
    }

    private static DoctorDayOffDto ProjectDayOff(DoctorDayOff dayOff, int? year)
    {
        DateOnly? displayStart = null;
        DateOnly? displayEnd = null;

        if (year is int y)
        {
            if (dayOff.RepeatYearly)
            {
                displayStart = CreateDateForYear(y, dayOff.StartDate);
                displayEnd = CreateDateForYear(y, dayOff.EndDate);
                if (displayEnd < displayStart)
                {
                    displayEnd = displayStart;
                }
            }
            else if (y >= dayOff.StartDate.Year && y <= dayOff.EndDate.Year)
            {
                displayStart = dayOff.StartDate;
                displayEnd = dayOff.EndDate;
            }
        }

        return new DoctorDayOffDto
        {
            Id = dayOff.Id,
            Name = dayOff.Name,
            StartDate = dayOff.StartDate,
            EndDate = dayOff.EndDate,
            RepeatYearly = dayOff.RepeatYearly,
            DisplayStart = displayStart,
            DisplayEnd = displayEnd
        };
    }

    private static DateOnly CreateDateForYear(int year, DateOnly source)
    {
        var day = Math.Min(source.Day, DateTime.DaysInMonth(year, source.Month));
        return new DateOnly(year, source.Month, day);
    }
}
