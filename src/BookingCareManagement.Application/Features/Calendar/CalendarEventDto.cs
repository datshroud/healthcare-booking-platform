using System;

namespace BookingCareManagement.Application.Features.Calendar;

public sealed class CalendarEventDto
{
    public Guid Id { get; set; }
    public Guid DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string DoctorAvatarUrl { get; set; } = string.Empty;
    public Guid SpecialtyId { get; set; }
    public string SpecialtyName { get; set; } = string.Empty;
    public string SpecialtyColor { get; set; } = "#0ea5e9";
    public string PatientName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public string StatusTone { get; set; } = string.Empty;
    public string StatusIcon { get; set; } = string.Empty;
    public string ClinicRoom { get; set; } = string.Empty;
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public decimal Price { get; set; }
}
