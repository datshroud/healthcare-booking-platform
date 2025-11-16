using System;
using System.Threading;
using System.Threading.Tasks;
using BookingCareManagement.Application.Common.Exceptions;
using BookingCareManagement.Application.Features.Specialties.Dtos;
using BookingCareManagement.Domain.Abstractions;

namespace BookingCareManagement.Application.Features.Specialties.Commands;

public sealed class UpdateSpecialtyStatusCommand
{
    public Guid Id { get; set; }
    public bool Active { get; set; }
}

public sealed class UpdateSpecialtyStatusCommandHandler
{
    private readonly ISpecialtyRepository _specialtyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSpecialtyStatusCommandHandler(
        ISpecialtyRepository specialtyRepository,
        IUnitOfWork unitOfWork)
    {
        _specialtyRepository = specialtyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SpecialtyDto> Handle(UpdateSpecialtyStatusCommand command, CancellationToken cancellationToken)
    {
        if (command.Id == Guid.Empty)
        {
            throw new ValidationException("Id chuyên khoa không hợp lệ.");
        }

        var specialty = await _specialtyRepository.GetByIdWithTrackingAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException($"Specialty with ID {command.Id} was not found.");

        if (command.Active)
        {
            specialty.Activate();
        }
        else
        {
            specialty.Deactivate();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return specialty.ToDto();
    }
}
