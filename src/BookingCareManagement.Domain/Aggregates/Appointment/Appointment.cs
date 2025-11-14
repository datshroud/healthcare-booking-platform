using System;

namespace BookingCareManagement.Domain.Aggregates.Appointment;

public class Appointment
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid DoctorId { get; private set; }
    public Guid ServiceId { get; private set; }
    public Guid ClinicRoomId { get; private set; }
    public DateTime StartUtc { get; private set; }
    public DateTime EndUtc { get; private set; }
    public string PatientName { get; private set; }
    public string CustomerPhone { get; private set; } = string.Empty;
    public string Status { get; private set; } = "Confirmed";
    public string? PatientId { get; private set; } // Khóa ngoại trỏ đến AppUser.Id
    private Appointment() { }
    public Appointment(
        Guid doctorId,
        Guid serviceId,
        Guid roomId,
        DateTime startUtc,
        TimeSpan duration,
        string patientName,
        string customerPhone,
        string? patientId = null)
    {
        DoctorId = doctorId;
        ServiceId = serviceId;
        ClinicRoomId = roomId;
        StartUtc = startUtc;
        EndUtc = startUtc.Add(duration);
        PatientName = patientName;
        CustomerPhone = customerPhone;
        PatientId = string.IsNullOrWhiteSpace(patientId) ? null : patientId.Trim();
    }

    public void Cancel() => Status = "Cancelled";
}
