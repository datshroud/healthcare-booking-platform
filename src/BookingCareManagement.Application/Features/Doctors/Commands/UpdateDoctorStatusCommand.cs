using BookingCareManagement.Application.Common.Exceptions;
using BookingCareManagement.Domain.Abstractions;

namespace BookingCareManagement.Application.Features.Doctors.Commands;

public class UpdateDoctorStatusCommand
{
    public Guid DoctorId { get; set; }
    public bool Active { get; set; }
}

public class UpdateDoctorStatusCommandHandler
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateDoctorStatusCommandHandler(IDoctorRepository doctorRepository, IUnitOfWork unitOfWork)
    {
        _doctorRepository = doctorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateDoctorStatusCommand command, CancellationToken cancellationToken)
    {
        var doctor = await _doctorRepository.GetByIdWithTrackingAsync(command.DoctorId, cancellationToken);
        if (doctor is null)
        {
            throw new NotFoundException($"Doctor with ID {command.DoctorId} was not found.");
        }

        if (command.Active)
        {
            doctor.Activate();
        }
        else
        {
            doctor.Deactivate();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
