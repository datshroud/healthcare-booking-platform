using System;

namespace BookingCareManagement.WinForms.Areas.Customer.Services.Models;

public sealed class CreateCustomerBookingRequest
{
 public Guid SpecialtyId { get; set; }
 public Guid DoctorId { get; set; }
 public DateTime SlotStartUtc { get; set; }
 public int DurationMinutes { get; set; } =30;
 public string CustomerName { get; set; } = string.Empty;
 public string CustomerPhone { get; set; } = string.Empty;
}
