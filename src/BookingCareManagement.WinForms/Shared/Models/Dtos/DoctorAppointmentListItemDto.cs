using System;

namespace BookingCareManagement.WinForms.Shared.Models.Dtos
{
    public class DoctorAppointmentListItemDto
    {
        public Guid Id { get; set; }
        public Guid DoctorId { get; set; }
        public Guid SpecialtyId { get; set; }
        public string SpecialtyName { get; set; } = string.Empty;
        public string SpecialtyColor { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string? PatientId { get; set; }
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
        public Guid ClinicRoomId { get; set; }
        public string ClinicRoom { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}