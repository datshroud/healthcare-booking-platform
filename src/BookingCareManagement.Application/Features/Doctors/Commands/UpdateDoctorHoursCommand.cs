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

        var schedules = NormalizeSchedules(command.Hours);

        await _doctorRepository.RemoveWorkingHoursAsync(doctor.Id, cancellationToken);
        doctor.ReplaceWorkingHours(schedules);
        _doctorRepository.AddWorkingHours(doctor.WorkingHours);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static IReadOnlyCollection<(DayOfWeek Day, TimeSpan Start, TimeSpan End, string? Location)> NormalizeSchedules(
        IEnumerable<UpdateDoctorHoursSlotRequest>? slots)
    {
        if (slots is null)
        {
            return Array.Empty<(DayOfWeek, TimeSpan, TimeSpan, string?)>();
        }

        var normalized = new List<(DayOfWeek Day, TimeSpan Start, TimeSpan End, string? Location)>();

        foreach (var slot in slots)
        {
            if (!Enum.IsDefined(typeof(DayOfWeek), slot.DayOfWeek))
            {
                throw new ArgumentException($"Invalid day of week value: {slot.DayOfWeek}.");
            }

            var start = ParseTime(slot.StartTime, "start");
            var end = ParseTime(slot.EndTime, "end");

            if (start >= end)
            {
                throw new ArgumentException("Start time must be earlier than end time.");
            }

            normalized.Add(((DayOfWeek)slot.DayOfWeek, start, end, NormalizeLocation(slot.Location)));
        }

        ValidateOverlaps(normalized);
        return normalized;
    }

    private static TimeSpan ParseTime(string value, string label)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"The {label} time is required.");
        }

        var trimmed = value.Trim();
        var formats = new[] { "hh\\:mm", "h\\:mm", "hh\\:mm\\:ss", "h\\:mm\\:ss" };

        if (TimeSpan.TryParseExact(trimmed, formats, CultureInfo.InvariantCulture, out var parsed) ||
            TimeSpan.TryParse(trimmed, CultureInfo.InvariantCulture, out parsed))
        {
            return parsed;
        }

        throw new ArgumentException($"Invalid {label} time format. Use HH:mm.");
    }

    private static string? NormalizeLocation(string? location)
    {
        return string.IsNullOrWhiteSpace(location) ? null : location.Trim();
    }

    private static void ValidateOverlaps(List<(DayOfWeek Day, TimeSpan Start, TimeSpan End, string? Location)> schedules)
    {
        var grouped = schedules
            .GroupBy(s => s.Day)
            .ToDictionary(g => g.Key, g => g.OrderBy(s => s.Start).ToList());

        foreach (var kvp in grouped)
        {
            var slots = kvp.Value;
            for (var i = 1; i < slots.Count; i++)
            {
                var previous = slots[i - 1];
                var current = slots[i];
                if (current.Start < previous.End)
                {
                    throw new ArgumentException($"Working hours for {kvp.Key} cannot overlap.");
                }
            }
        }
    }
}
