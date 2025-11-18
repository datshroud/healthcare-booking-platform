using System;
using System.Collections.Generic;

namespace BookingCareManagement.Web.Areas.Doctor.Dtos;

public sealed class DoctorPerformanceResponseDto
{
    public string RangeLabel { get; set; } = string.Empty;
    public IReadOnlyCollection<DoctorPerformanceServiceDto> Services { get; set; }
        = Array.Empty<DoctorPerformanceServiceDto>();
    public IReadOnlyCollection<DoctorPerformancePatientDto> Patients { get; set; }
        = Array.Empty<DoctorPerformancePatientDto>();
}

public sealed class DoctorPerformanceServiceDto
{
    public Guid SpecialtyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int AppointmentCount { get; set; }
    public decimal Revenue { get; set; }
    public decimal OccupancyPercent { get; set; }
}

public sealed class DoctorPerformancePatientDto
{
    public string? PatientId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int AppointmentCount { get; set; }
    public DateTime? LastVisitUtc { get; set; }
    public string LastVisitLabel { get; set; } = string.Empty;
}
