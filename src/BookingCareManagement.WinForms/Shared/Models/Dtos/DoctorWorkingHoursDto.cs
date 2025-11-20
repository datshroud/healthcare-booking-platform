using System;
using System.Collections.Generic;

namespace BookingCareManagement.WinForms.Shared.Models.Dtos;

public sealed class DoctorWorkingHourDto
{
    public Guid Id { get; set; }
    public int DayOfWeek { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public string? BreakStartTime { get; set; }
    public string? BreakEndTime { get; set; }
    public string? Location { get; set; }
}

public sealed class DoctorWorkingHoursDto
{
    public Guid DoctorId { get; set; }
    public bool LimitAppointments { get; set; }
    public IReadOnlyCollection<DoctorWorkingHourDto> Hours { get; set; } = Array.Empty<DoctorWorkingHourDto>();
}
