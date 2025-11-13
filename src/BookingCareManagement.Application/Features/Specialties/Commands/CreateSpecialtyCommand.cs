using BookingCareManagement.Application.Features.Specialties.Dtos;
using BookingCareManagement.Domain.Abstractions;
using BookingCareManagement.Domain.Aggregates.Doctor;

namespace BookingCareManagement.Application.Features.Specialties.Commands;

// 1. Command: Dữ liệu từ Modal "Thêm danh mục"
public class CreateSpecialtyCommand
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    // Chúng ta sẽ dùng API Upload ảnh riêng, nên ImageUrl là đủ
}

// 2. Handler
public class CreateSpecialtyCommandHandler
{
    private readonly ISpecialtyRepository _specialtyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSpecialtyCommandHandler(ISpecialtyRepository specialtyRepository, IUnitOfWork unitOfWork)
    {
        _specialtyRepository = specialtyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SpecialtyDto> Handle(CreateSpecialtyCommand command, CancellationToken cancellationToken)
    {
        // Tạo Entity
        var specialty = new Specialty(
            command.Name,
            slug: null, // Slug sẽ tự tạo trong constructor
            command.Description,
            command.ImageUrl
        );

        _specialtyRepository.Add(specialty);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Trả về DTO
        return new SpecialtyDto
        {
            Id = specialty.Id,
            Name = specialty.Name,
            Slug = specialty.Slug,
            Active = specialty.Active,
            Description = specialty.Description,
            ImageUrl = specialty.ImageUrl
        };
    }
}