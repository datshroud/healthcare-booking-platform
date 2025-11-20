using System;
using System.Collections.Generic;

namespace BookingCareManagement.WinForms.Shared.Models.Dtos;

public sealed class CustomerSpecialtyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Color { get; set; } = "#1a73e8";
    public string? ImageUrl { get; set; }
    public decimal? Price { get; set; }
    public int? DurationMinutes { get; set; }
    public IReadOnlyCollection<SpecialtyDoctorDto> Doctors { get; set; } = Array.Empty<SpecialtyDoctorDto>();
}
