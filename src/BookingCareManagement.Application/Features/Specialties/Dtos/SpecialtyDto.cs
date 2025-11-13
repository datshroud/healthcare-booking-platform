namespace BookingCareManagement.Application.Features.Specialties.Dtos;

// Đây là dữ liệu trả về cho API
public class SpecialtyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool Active { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    // Chúng ta có thể thêm "ServiceCount" sau
}