using System;
using BookingCareManagement.Application.Common.Exceptions;
using BookingCareManagement.Application.Features.Appointments.Dtos;
using BookingCareManagement.Domain.Abstractions;

namespace BookingCareManagement.Application.Features.Appointments.Queries;

public class GetAppointmentByIdQuery
{
    public Guid Id { get; set; }
}

public class GetAppointmentByIdQueryHandler
{
    private readonly IAppointmentRepository _appointmentRepository;

    public GetAppointmentByIdQueryHandler(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    public async Task<AppointmentDto> Handle(GetAppointmentByIdQuery query, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(query.Id, cancellationToken);
        if (appointment is null)
        {
            throw new NotFoundException($"Appointment {query.Id} not found");
        }

        return appointment.ToDto();
    }
}
