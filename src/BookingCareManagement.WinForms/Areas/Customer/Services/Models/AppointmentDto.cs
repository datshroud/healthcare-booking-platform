using System;

namespace BookingCareManagement.WinForms.Areas.Customer.Services.Models;

public sealed class AppointmentDto
{
     public Guid Id { get; set; }
     public DateTime StartUtc { get; set; }
     public DateTime EndUtc { get; set; }
     public decimal Price { get; set; }
}
