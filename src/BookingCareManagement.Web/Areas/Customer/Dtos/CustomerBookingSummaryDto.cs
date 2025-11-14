using System;

namespace BookingCareManagement.Web.Areas.Customer.Dtos;

public class CustomerBookingSummaryDto
{
    public Guid Id { get; set; }
    public Guid DoctorId { get; set; }
    public Guid SpecialtyId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string DoctorAvatarUrl { get; set; } = string.Empty;
    public string SpecialtyName { get; set; } = string.Empty;
    public string SpecialtyColor { get; set; } = string.Empty;
    public string ClinicRoom { get; set; } = string.Empty;
    public string DateText { get; set; } = string.Empty;
    public string TimeText { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public string StatusTone { get; set; } = string.Empty;
    public string StatusIcon { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
}