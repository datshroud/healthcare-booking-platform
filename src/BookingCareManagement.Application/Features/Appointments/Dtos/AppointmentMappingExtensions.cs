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
            SpecialtyId = appointment.SpecialtyId,
            ClinicRoomId = appointment.ClinicRoomId,
            StartUtc = appointment.StartUtc,
            EndUtc = appointment.EndUtc,
            PatientName = appointment.PatientName,
            CustomerPhone = appointment.CustomerPhone,
            Status = appointment.Status,
            Price = appointment.Price
        };
    }
}
