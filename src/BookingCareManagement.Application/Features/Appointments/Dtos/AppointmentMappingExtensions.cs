using BookingCareManagement.Domain.Aggregates.Appointment;

namespace BookingCareManagement.Application.Features.Appointments.Dtos;

public static class AppointmentMappingExtensions
{
    public static AppointmentDto ToDto(this Appointment appointment)
    {
        return new AppointmentDto
        {
            Id = appointment.Id,
            DoctorId = appointment.DoctorId,
            ServiceId = appointment.ServiceId,
            ClinicRoomId = appointment.ClinicRoomId,
            StartUtc = appointment.StartUtc,
            EndUtc = appointment.EndUtc,
            PatientName = appointment.PatientName,
            Status = appointment.Status
        };
    }
}
