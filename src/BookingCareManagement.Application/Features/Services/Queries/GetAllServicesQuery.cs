using BookingCareManagement.Application.Abstractions;
using BookingCareManagement.Application.Features.Services.Dtos;
using BookingCareManagement.Domain.Abstractions;
using BookingCareManagement.Domain.Aggregates.Service;

namespace BookingCareManagement.Application.Features.Services.Queries;

// 1. DTO (Data Transfer Object)
// DTO này dùng cho danh sách chính
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
    public string SpecialtyName { get; set; } = string.Empty;

    // DTO này cũng cần trả về danh sách Doctor (ID và Tên)
    public IEnumerable<object> Doctors { get; set; } = new List<object>();
}


// 2. Query
public class GetAllServicesQuery { }

// 3. Handler (Logic)
public class GetAllServicesQueryHandler
{
    private readonly IServiceRepository _serviceRepository;

    public GetAllServicesQueryHandler(IServiceRepository serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }

    public async Task<IEnumerable<ServiceDto>> Handle(CancellationToken cancellationToken)
    {
        var services = await _serviceRepository.GetAllAsync(cancellationToken);

        // Map sang DTO
        return services.Select(s => new ServiceDto
        {
            Id = s.Id,
            Name = s.Name,
            Description = s.Description,
            Price = s.Price,
            DurationInMinutes = s.DurationInMinutes,
            Active = s.Active,
            Color = s.Color,
            ImageUrl = s.ImageUrl,
            SpecialtyId = s.SpecialtyId,
            SpecialtyName = s.Specialty.Name, // Lấy tên từ Specialty đã gộp
            Doctors = s.Doctors.Select(d => new { d.AppUser.AvatarUrl, d.AppUser.FullName })
        });
    }
}