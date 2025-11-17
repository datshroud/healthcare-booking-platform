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
    public string PatientName { get; private set; } = string.Empty;
    public string CustomerPhone { get; private set; } = string.Empty;
    public string Status { get; private set; } = AppointmentStatus.Pending;
    public string? PatientId { get; private set; } // Khóa ngoại trỏ đến AppUser.Id
    public decimal Price { get; private set; }
    private Appointment() { }
    public Appointment(
        Guid doctorId,
        Guid specialtyId,
        Guid roomId,
        DateTime startUtc,
        TimeSpan duration,
        string patientName,
        string customerPhone,
        string? patientId = null,
        decimal price = 0)
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
        SetPrice(price);
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

    public void UpdatePatientProfile(string patientName, string customerPhone)
    {
        if (string.IsNullOrWhiteSpace(patientName))
        {
            throw new ArgumentException("Patient name is required.", nameof(patientName));
        }

        if (string.IsNullOrWhiteSpace(customerPhone))
        {
            throw new ArgumentException("Customer phone is required.", nameof(customerPhone));
        }

        PatientName = patientName.Trim();
        CustomerPhone = customerPhone.Trim();
    }

    public void ChangeSpecialty(Guid specialtyId)
    {
        if (specialtyId == Guid.Empty)
        {
            throw new ArgumentException("SpecialtyId is required.", nameof(specialtyId));
        }

        SpecialtyId = specialtyId;
    }

    public void AssignClinicRoom(Guid clinicRoomId)
    {
        if (clinicRoomId == Guid.Empty)
        {
            throw new ArgumentException("ClinicRoomId is required.", nameof(clinicRoomId));
        }

        ClinicRoomId = clinicRoomId;
    }

    public void AssignDoctor(Guid doctorId)
    {
        if (doctorId == Guid.Empty)
        {
            throw new ArgumentException("DoctorId is required.", nameof(doctorId));
        }

        DoctorId = doctorId;
    }

    public void AssignPatient(string? patientId)
    {
        PatientId = string.IsNullOrWhiteSpace(patientId) ? null : patientId.Trim();
    }

    public void SetPrice(decimal price)
    {
        if (price < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");
        }

        Price = decimal.Round(price, 0, MidpointRounding.AwayFromZero);
    }

    public void SetStatus(string status)
    {
        Status = AppointmentStatus.NormalizeOrDefault(status);
    }
}
