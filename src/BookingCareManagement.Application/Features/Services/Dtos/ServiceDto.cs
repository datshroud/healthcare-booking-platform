namespace BookingCareManagement.Application.Features.Services.Dtos;

public class ServiceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int DurationInMinutes { get; set; }
    public bool Active { get; set; }
    public string? Color { get; set; }
    public string? ImageUrl { get; set; }
    public Guid SpecialtyId { get; set; }

    // DTO này cũng cần trả về danh sách Doctor IDs
    public IEnumerable<string> DoctorIds { get; set; } = new List<string>();
}