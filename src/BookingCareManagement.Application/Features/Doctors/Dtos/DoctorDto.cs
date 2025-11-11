namespace BookingCareManagement.Application.Features.Doctors.Dtos;

// DTO này được cập nhật để phản ánh cấu trúc mới
public class DoctorDto
{
    public Guid Id { get; set; } // Đây là Doctor.Id
    public string AppUserId { get; set; } = string.Empty; // Đây là AppUser.Id
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool Active { get; set; } // Đây là Doctor.Active

    public IEnumerable<string> Specialties { get; set; } = new List<string>();
}