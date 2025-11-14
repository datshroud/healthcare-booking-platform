using System;
using BookingCareManagement.Application.Common.Exceptions;
using BookingCareManagement.Application.Features.Appointments.Dtos;
using BookingCareManagement.Domain.Abstractions;
using BookingCareManagement.Domain.Aggregates.Appointment;

namespace BookingCareManagement.Application.Features.Appointments.Commands;

public class CreateAppointmentCommand
{
    public Guid DoctorId { get; set; }
    public Guid ServiceId { get; set; }
    public Guid ClinicRoomId { get; set; }
    public DateTime StartUtc { get; set; }
    public int DurationMinutes { get; set; } = 30;
    public string PatientName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? PatientId { get; set; }
}

public class CreateAppointmentCommandHandler
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAppointmentCommandHandler(
        IAppointmentRepository appointmentRepository,
        IUnitOfWork unitOfWork)
    {
        _appointmentRepository = appointmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AppointmentDto> Handle(CreateAppointmentCommand command, CancellationToken cancellationToken)
    {
        if (command.DurationMinutes <= 0)
        {
            throw new ValidationException("DurationMinutes must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(command.PatientName))
        {
            throw new ValidationException("Patient name is required.");
        }

        if (string.IsNullOrWhiteSpace(command.CustomerPhone))
        {
            throw new ValidationException("Customer phone is required.");
        }

        var appointment = new Appointment(
            command.DoctorId,
            command.ServiceId,
            command.ClinicRoomId,
            command.StartUtc,
            TimeSpan.FromMinutes(command.DurationMinutes),
            command.PatientName.Trim(),
            command.CustomerPhone.Trim(),
            command.PatientId);

        _appointmentRepository.Add(appointment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return appointment.ToDto();
    }
}
