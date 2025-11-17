using System;
using System.Collections.Generic;

namespace BookingCareManagement.Web.Areas.Customer.Dtos;

public sealed class CustomerSpecialtyDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Color { get; init; } = "#1a73e8";
    public string? ImageUrl { get; init; }
    public decimal? Price { get; init; }
    public int? DurationMinutes { get; init; }
    public IReadOnlyCollection<CustomerDoctorSummaryDto> Doctors { get; init; } = Array.Empty<CustomerDoctorSummaryDto>();
}
