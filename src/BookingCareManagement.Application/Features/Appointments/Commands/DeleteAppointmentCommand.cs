using System;
using BookingCareManagement.Application.Common.Exceptions;
using BookingCareManagement.Domain.Abstractions;

namespace BookingCareManagement.Application.Features.Appointments.Commands;

public class DeleteAppointmentCommand
{
    public Guid Id { get; set; }
}

public class DeleteAppointmentCommandHandler
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAppointmentCommandHandler(
        IAppointmentRepository appointmentRepository,
        IUnitOfWork unitOfWork)
    {
        _appointmentRepository = appointmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteAppointmentCommand command, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdWithTrackingAsync(command.Id, cancellationToken);
        if (appointment is null)
        {
            throw new NotFoundException($"Appointment {command.Id} not found");
        }

        _appointmentRepository.Remove(appointment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
