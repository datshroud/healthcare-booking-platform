using System;

namespace BookingCareManagement.WinForms.Areas.Customer.Models
{
    // Keep a small DTO in WinForms project to model API responses for the customer bookings list.
    public sealed class CustomerBookingDto
    {
        public Guid Id { get; set; }
        public Guid DoctorId { get; set; }
        public Guid SpecialtyId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string DoctorAvatarUrl { get; set; } = string.Empty;
        public string SpecialtyName { get; set; } = string.Empty;
        public string ClinicRoom { get; set; } = string.Empty;
        public string DateText { get; set; } = string.Empty;
        public string TimeText { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusLabel { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
