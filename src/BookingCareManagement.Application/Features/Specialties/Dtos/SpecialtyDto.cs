namespace BookingCareManagement.Application.Features.Specialties.Dtos;

public class SpecialtyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool Active { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string Color { get; set; } = "#1a73e8";
    public IEnumerable<SpecialtyDoctorDto> Doctors { get; set; } = new List<SpecialtyDoctorDto>();
}