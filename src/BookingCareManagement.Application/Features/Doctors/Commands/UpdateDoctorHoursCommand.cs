using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BookingCareManagement.Application.Common.Exceptions;
using BookingCareManagement.Domain.Abstractions;

namespace BookingCareManagement.Application.Features.Doctors.Commands;

public class UpdateDoctorHoursRequest
{
    public bool LimitAppointments { get; set; }
    public IEnumerable<UpdateDoctorHoursSlotRequest> Hours { get; set; } = Array.Empty<UpdateDoctorHoursSlotRequest>();
}

public class UpdateDoctorHoursSlotRequest
{
    public int DayOfWeek { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public string? Location { get; set; }
}

public class UpdateDoctorHoursCommand
{
    public Guid DoctorId { get; set; }
    public bool LimitAppointments { get; set; }
    public IEnumerable<UpdateDoctorHoursSlotRequest> Hours { get; set; } = Array.Empty<UpdateDoctorHoursSlotRequest>();
}

public class UpdateDoctorHoursCommandHandler
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateDoctorHoursCommandHandler(IDoctorRepository doctorRepository, IUnitOfWork unitOfWork)
    {
        _doctorRepository = doctorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateDoctorHoursCommand command, CancellationToken cancellationToken)
    {
        var doctor = await _doctorRepository.GetByIdWithTrackingAsync(command.DoctorId, cancellationToken);
        if (doctor is null)
        {
            throw new NotFoundException($"Doctor with ID {command.DoctorId} was not found.");
        }

        doctor.SetAppointmentLimit(command.LimitAppointments);

        var existingHours = doctor.WorkingHours.ToList();
        var schedules = new List<(DayOfWeek Day, TimeSpan Start, TimeSpan End, string? Location)>();
        foreach (var slot in command.Hours)
        {
            if (!Enum.IsDefined(typeof(DayOfWeek), slot.DayOfWeek))
            {
                throw new ArgumentException($"Invalid day of week value: {slot.DayOfWeek}.");
            }

            if (!TimeSpan.TryParseExact(slot.StartTime, "hh\\:mm", CultureInfo.InvariantCulture, out var start))
            {
                throw new ArgumentException($"Invalid start time format for day {slot.DayOfWeek}. Use HH:mm.");
            }

            if (!TimeSpan.TryParseExact(slot.EndTime, "hh\\:mm", CultureInfo.InvariantCulture, out var end))
            {
                throw new ArgumentException($"Invalid end time format for day {slot.DayOfWeek}. Use HH:mm.");
            }

            schedules.Add(((DayOfWeek)slot.DayOfWeek, start, end, slot.Location));
        }

        _doctorRepository.RemoveWorkingHours(existingHours);
        doctor.ReplaceWorkingHours(schedules);
        _doctorRepository.AddWorkingHours(doctor.WorkingHours);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
