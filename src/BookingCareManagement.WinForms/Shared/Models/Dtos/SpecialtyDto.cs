using System;
using System.Collections.Generic;

namespace BookingCareManagement.WinForms.Shared.Models.Dtos;

public sealed class SpecialtyDto
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Slug { get; set; } = string.Empty;
	public bool Active { get; set; }
	public string? Description { get; set; }
	public string? ImageUrl { get; set; }
	public string Color { get; set; } = "#1a73e8";
	public decimal Price { get; set; }
	public IReadOnlyCollection<SpecialtyDoctorDto> Doctors { get; set; } = Array.Empty<SpecialtyDoctorDto>();
}
