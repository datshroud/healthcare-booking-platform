namespace BookingCareManagement.Application.Features.Specialties.Dtos;

public class SpecialtyDoctorDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
}
