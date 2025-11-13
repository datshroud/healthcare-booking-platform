using BookingCareManagement.Application.Common.Exceptions;
using BookingCareManagement.Domain.Abstractions;

namespace BookingCareManagement.Application.Features.Specialties.Commands;

public class DeleteSpecialtyCommand
{
    public Guid Id { get; set; }
}

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
        if (specialty == null)
        {
            throw new NotFoundException($"Specialty with ID {command.Id} was not found.");
        }

        specialty.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
