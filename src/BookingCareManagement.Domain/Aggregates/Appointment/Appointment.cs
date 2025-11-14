using System;

namespace BookingCareManagement.Domain.Aggregates.Appointment;

public class Appointment
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid DoctorId { get; private set; }
    public Guid SpecialtyId { get; private set; }
    public Guid ClinicRoomId { get; private set; }
    public DateTime StartUtc { get; private set; }
    public DateTime EndUtc { get; private set; }
    public string PatientName { get; private set; }
    public string CustomerPhone { get; private set; } = string.Empty;
    public string Status { get; private set; } = AppointmentStatus.Pending;
    public string? PatientId { get; private set; } // Khóa ngoại trỏ đến AppUser.Id
    private Appointment() { }
    public Appointment(
        Guid doctorId,
        Guid specialtyId,
        Guid roomId,
        DateTime startUtc,
        TimeSpan duration,
        string patientName,
        string customerPhone,
        string? patientId = null)
    {
        DoctorId = doctorId;
        SpecialtyId = specialtyId;
        ClinicRoomId = roomId;
        StartUtc = startUtc;
        EndUtc = startUtc.Add(duration);
        PatientName = patientName;
        CustomerPhone = customerPhone;
        PatientId = string.IsNullOrWhiteSpace(patientId) ? null : patientId.Trim();
        Status = AppointmentStatus.Pending;
    }

    public void Approve() => Status = AppointmentStatus.Approved;
    public void MarkNoShow() => Status = AppointmentStatus.NoShow;
    public void Reject() => Status = AppointmentStatus.Rejected;
    public void ResetToPending() => Status = AppointmentStatus.Pending;
    public void Cancel() => Status = AppointmentStatus.Canceled;
    public void Reschedule(DateTime newStartUtc, TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero)
        {
            throw new ArgumentException("Duration must be greater than zero.", nameof(duration));
        }

        StartUtc = newStartUtc;
        EndUtc = newStartUtc.Add(duration);
    }
}
