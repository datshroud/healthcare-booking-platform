using System;

namespace BookingCareManagement.Web.Areas.Customer.Dtos;

public class RescheduleCustomerBookingRequest
{
    public DateTime SlotStartUtc { get; set; }
    public int DurationMinutes { get; set; } = 30;
}