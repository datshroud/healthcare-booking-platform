using System;
using System.Collections.Generic;

namespace BookingCareManagement.WinForms.Shared.Models.Dtos;

public sealed class DoctorDto
{
	public Guid Id { get; set; }
	public string AppUserId { get; set; } = string.Empty;
	public string FirstName { get; set; } = string.Empty;
	public string LastName { get; set; } = string.Empty;
	public string FullName { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public string PhoneNumber { get; set; } = string.Empty;
	public bool Active { get; set; }
	public string AvatarUrl { get; set; } = string.Empty;
	public IReadOnlyCollection<string> Specialties { get; set; } = Array.Empty<string>();
}
