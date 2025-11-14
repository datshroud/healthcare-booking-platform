using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BookingCareManagement.Application.Common.Exceptions;
using BookingCareManagement.Domain.Abstractions;
using BookingCareManagement.Domain.Aggregates.Doctor;

namespace BookingCareManagement.Application.Features.Specialties.Commands;

public sealed class DeleteSpecialtyCommand
{
    public Guid Id { get; set; }
}

public sealed class DeleteSpecialtyCommandHandler
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
        if (command.Id == Guid.Empty)
        {
            throw new ValidationException("Id chuyên khoa không hợp lệ.");
        }

        var specialty = await _specialtyRepository.GetByIdWithTrackingAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException($"Specialty with ID {command.Id} was not found.");

        if (specialty.Doctors.Any())
        {
            specialty.ReplaceDoctors(Array.Empty<Doctor>());
        }

        _specialtyRepository.Remove(specialty);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
