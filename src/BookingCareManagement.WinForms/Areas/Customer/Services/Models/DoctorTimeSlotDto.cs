using System;

namespace BookingCareManagement.WinForms.Areas.Customer.Services.Models;

public sealed class DoctorTimeSlotDto
{
 public DateTime StartLocal { get; set; }
 public DateTime EndLocal { get; set; }
 public DateTime StartUtc { get; set; }
 public DateTime EndUtc { get; set; }
 public bool IsAvailable { get; set; }
}
