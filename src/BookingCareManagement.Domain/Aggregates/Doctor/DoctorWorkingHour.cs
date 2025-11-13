using System;

namespace BookingCareManagement.Domain.Aggregates.Doctor;

public class DoctorWorkingHour
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid DoctorId { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public TimeSpan StartTime { get; private set; }
    public TimeSpan EndTime { get; private set; }
    public string? Location { get; private set; }

    public Doctor Doctor { get; private set; } = null!;

    private DoctorWorkingHour() { }

    private DoctorWorkingHour(Guid doctorId, DayOfWeek day, TimeSpan start, TimeSpan end, string? location)
    {
        DoctorId = doctorId;
        UpdateSchedule(day, start, end, location);
    }

    internal static DoctorWorkingHour Create(Guid doctorId, DayOfWeek day, TimeSpan start, TimeSpan end, string? location)
    {
        return new DoctorWorkingHour(doctorId, day, start, end, location);
    }

    public void UpdateSchedule(DayOfWeek day, TimeSpan start, TimeSpan end, string? location)
    {
        if (start >= end)
        {
            throw new ArgumentException("Start time must be earlier than end time.");
        }

        DayOfWeek = day;
        StartTime = start;
        EndTime = end;
        Location = string.IsNullOrWhiteSpace(location) ? null : location.Trim();
    }

    internal void AttachDoctor(Doctor doctor)
    {
        Doctor = doctor ?? throw new ArgumentNullException(nameof(doctor));
        DoctorId = doctor.Id;
    }
}
