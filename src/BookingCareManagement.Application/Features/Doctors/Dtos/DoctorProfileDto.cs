namespace BookingCareManagement.Application.Features.Doctors.Dtos;

public class DoctorProfileDto
{
    public Guid DoctorId { get; set; }
    public string AppUserId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Description { get; set; }
    public string? AvatarUrl { get; set; }
}
