using System.Collections.Generic;
using System.Linq;
using BookingCareManagement.Application.Features.Appointments.Dtos;
using BookingCareManagement.Domain.Abstractions;

namespace BookingCareManagement.Application.Features.Appointments.Queries;

public class GetAllAppointmentsQuery { }

public class GetAllAppointmentsQueryHandler
{
    private readonly IAppointmentRepository _appointmentRepository;

    public GetAllAppointmentsQueryHandler(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    public async Task<IEnumerable<AppointmentDto>> Handle(CancellationToken cancellationToken)
    {
        var appointments = await _appointmentRepository.GetAllAsync(cancellationToken);
        return appointments.Select(a => a.ToDto());
    }
}
