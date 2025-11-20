using System;
using System.Collections.Generic;

namespace BookingCareManagement.WinForms.Shared.Models.Dtos;

public sealed class DoctorAppointmentMetadataDto
{
    public Guid DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string DoctorAvatarUrl { get; set; } = string.Empty;
    public IReadOnlyCollection<DoctorAppointmentSpecialtyOptionDto> Specialties { get; set; }
        = Array.Empty<DoctorAppointmentSpecialtyOptionDto>();

    public IReadOnlyCollection<DoctorAppointmentStatusOptionDto> Statuses { get; set; }
        = Array.Empty<DoctorAppointmentStatusOptionDto>();

    public IReadOnlyCollection<DoctorAppointmentPatientOptionDto> Patients { get; set; }
        = Array.Empty<DoctorAppointmentPatientOptionDto>();
}

public sealed class DoctorAppointmentSpecialtyOptionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}

public sealed class DoctorAppointmentStatusOptionDto
{
    public string Code { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Tone { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}

public sealed class DoctorAppointmentPatientOptionDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}

public sealed class DoctorAppointmentUpsertRequest
{
    public Guid SpecialtyId { get; set; }
    public DateTime SlotStartUtc { get; set; }
    public int DurationMinutes { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? PatientId { get; set; }
    public Guid? ClinicRoomId { get; set; }
}

public sealed class DoctorAppointmentStatusRequest
{
    public string Status { get; set; } = string.Empty;
}
