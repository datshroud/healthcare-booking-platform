using BookingCareManagement.Application.Common.Exceptions;
using BookingCareManagement.Domain.Abstractions;

namespace BookingCareManagement.Application.Features.Specialties.Commands;

// 1. Command
public class UpdateSpecialtyCommand
{
    public Guid Id { get; set; } // Lấy từ URL
    public string Name { get; set; } = string.Empty; // Lấy từ Body
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
}

// 2. Handler
public class UpdateSpecialtyCommandHandler
{
    private readonly ISpecialtyRepository _specialtyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSpecialtyCommandHandler(ISpecialtyRepository specialtyRepository, IUnitOfWork unitOfWork)
    {
        _specialtyRepository = specialtyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateSpecialtyCommand command, CancellationToken cancellationToken)
    {
        var specialty = await _specialtyRepository.GetByIdWithTrackingAsync(command.Id, cancellationToken);
        if (specialty is null)
        {
            throw new NotFoundException($"Specialty (Category) with ID {command.Id} not found.");
        }

        specialty.Update(
            command.Name,
            slug: null, // Slug tự cập nhật trong hàm Update
            command.Description,
            command.ImageUrl
        );

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}