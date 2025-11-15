using System;
using System.Collections.Generic;

namespace BookingCareManagement.Web.Areas.Doctor.Dtos;

public class DoctorAppointmentListItemDto
{
    public Guid Id { get; set; }
    public Guid SpecialtyId { get; set; }
    public string SpecialtyName { get; set; } = string.Empty;
    public string SpecialtyColor { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public string StatusTone { get; set; } = string.Empty;
    public string StatusIcon { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string DoctorInitials { get; set; } = string.Empty;
    public string DoctorAvatarUrl { get; set; } = string.Empty;
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public string DateLabel { get; set; } = string.Empty;
    public string DateKey { get; set; } = string.Empty;
    public string TimeLabel { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public string ClinicRoom { get; set; } = string.Empty;
}

public class DoctorAppointmentMetadataDto
{
    public Guid DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public IReadOnlyCollection<DoctorAppointmentSpecialtyOptionDto> Specialties { get; set; }
        = Array.Empty<DoctorAppointmentSpecialtyOptionDto>();
    public IReadOnlyCollection<DoctorAppointmentStatusOptionDto> Statuses { get; set; }
        = Array.Empty<DoctorAppointmentStatusOptionDto>();
}

public class DoctorAppointmentSpecialtyOptionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#0ea5e9";
}

public class DoctorAppointmentStatusOptionDto
{
    public string Code { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Tone { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}

public class DoctorAppointmentUpsertRequest
{
    public Guid SpecialtyId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public DateTime SlotStartUtc { get; set; }
    public int DurationMinutes { get; set; } = 30;
    public string? Status { get; set; }
    public Guid? ClinicRoomId { get; set; }
}

public class DoctorAppointmentStatusRequest
{
    public string Status { get; set; } = string.Empty;
}
