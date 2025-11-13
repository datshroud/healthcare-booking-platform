using BookingCareManagement.Application.Common.Exceptions;
using BookingCareManagement.Domain.Abstractions;

namespace BookingCareManagement.Application.Features.Specialties.Commands;

// 1. Command
public class DeleteSpecialtyCommand
{
    public Guid Id { get; set; }
}

// 2. Handler
public class DeleteSpecialtyCommandHandler
{
    private readonly ISpecialtyRepository _specialtyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteSpecialtyCommandHandler(ISpecialtyRepository specialtyRepository, IUnitOfWork unitOfWork)
    {
        _specialtyRepository = specialtyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteSpecialtyCommand command, CancellationToken cancellationToken)
    {
        var specialty = await _specialtyRepository.GetByIdWithTrackingAsync(command.Id, cancellationToken);
        if (specialty is null)
        {
            throw new NotFoundException($"Specialty (Category) with ID {command.Id} not found.");
        }

        // TODO: Kiểm tra xem Specialty này có Service nào đang dùng không
        // Nếu có, nên ném lỗi "Không thể xóa"

        _specialtyRepository.Remove(specialty);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}