using System;
using System.Collections.Generic;

namespace BookingCareManagement.Web.Areas.Admin.Dtos;

public class AdminAppointmentMetadataDto
{
    public IReadOnlyCollection<AdminAppointmentSpecialtyOptionDto> Specialties { get; set; }
        = Array.Empty<AdminAppointmentSpecialtyOptionDto>();

    public IReadOnlyCollection<AdminAppointmentStatusOptionDto> Statuses { get; set; }
        = Array.Empty<AdminAppointmentStatusOptionDto>();

    public IReadOnlyCollection<AdminAppointmentDoctorOptionDto> Doctors { get; set; }
        = Array.Empty<AdminAppointmentDoctorOptionDto>();

    public IReadOnlyCollection<AdminAppointmentPatientOptionDto> Patients { get; set; }
        = Array.Empty<AdminAppointmentPatientOptionDto>();
}

public class AdminAppointmentSpecialtyOptionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#0ea5e9";
}

public class AdminAppointmentStatusOptionDto
{
    public string Code { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Tone { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}

public class AdminAppointmentDoctorOptionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public IReadOnlyCollection<Guid> SpecialtyIds { get; set; } = Array.Empty<Guid>();
}

public class AdminAppointmentPatientOptionDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}

public class AdminAppointmentUpsertRequest
{
    public Guid DoctorId { get; set; }
    public Guid SpecialtyId { get; set; }
    public string? PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public DateTime SlotStartUtc { get; set; }
    public int DurationMinutes { get; set; } = 30;
    public string? Status { get; set; }
    public Guid? ClinicRoomId { get; set; }
}

public class AdminAppointmentStatusRequest
{
    public string Status { get; set; } = string.Empty;
}
