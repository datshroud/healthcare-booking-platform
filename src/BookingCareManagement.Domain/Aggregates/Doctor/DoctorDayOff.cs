using System;

namespace BookingCareManagement.Domain.Aggregates.Doctor;

public class DoctorDayOff
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid DoctorId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public bool RepeatYearly { get; private set; }

    public Doctor Doctor { get; private set; } = null!;

    private DoctorDayOff() { }

    private DoctorDayOff(Guid doctorId, string name, DateOnly startDate, DateOnly endDate, bool repeatYearly)
    {
        DoctorId = doctorId;
        Update(name, startDate, endDate, repeatYearly);
    }

    internal static DoctorDayOff Create(Guid doctorId, string name, DateOnly startDate, DateOnly endDate, bool repeatYearly)
    {
        return new DoctorDayOff(doctorId, name, startDate, endDate, repeatYearly);
    }

    public void Update(string name, DateOnly startDate, DateOnly endDate, bool repeatYearly)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Day off name is required.");
        }

        if (startDate > endDate)
        {
            throw new ArgumentException("Start date must be before or equal to end date.");
        }

        Name = name.Trim();
        StartDate = startDate;
        EndDate = endDate;
        RepeatYearly = repeatYearly;
    }

    internal void AttachDoctor(Doctor doctor)
    {
        Doctor = doctor ?? throw new ArgumentNullException(nameof(doctor));
        DoctorId = doctor.Id;
    }
}
