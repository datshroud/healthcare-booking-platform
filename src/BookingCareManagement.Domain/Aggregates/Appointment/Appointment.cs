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

    private Appointment() { }
    public Appointment(
        Guid doctorId,
        Guid serviceId,
        Guid roomId,
        DateTime startUtc,
        TimeSpan duration,
        string patientName,
        string customerPhone)
    {
        DoctorId = doctorId;
        ServiceId = serviceId;
        ClinicRoomId = roomId;
        StartUtc = startUtc;
        EndUtc = startUtc.Add(duration);
        PatientName = patientName;
        CustomerPhone = customerPhone;
    }

    public void Cancel() => Status = "Cancelled";
}
