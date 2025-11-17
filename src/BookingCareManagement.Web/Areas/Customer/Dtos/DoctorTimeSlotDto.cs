using System;

namespace BookingCareManagement.Web.Areas.Customer.Dtos;

public sealed class DoctorTimeSlotDto
{
    public DateTime StartLocal { get; init; }
    public DateTime EndLocal { get; init; }
    public DateTime StartUtc { get; init; }
    public DateTime EndUtc { get; init; }
    public bool IsAvailable { get; init; }
}
