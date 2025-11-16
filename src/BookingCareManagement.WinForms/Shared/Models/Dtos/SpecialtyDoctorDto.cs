using System;

namespace BookingCareManagement.WinForms.Shared.Models.Dtos;

public sealed class SpecialtyDoctorDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
}
