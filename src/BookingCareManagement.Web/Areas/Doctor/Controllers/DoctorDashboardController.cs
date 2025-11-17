using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using BookingCareManagement.Domain.Aggregates.Appointment;
using BookingCareManagement.Domain.Aggregates.Doctor;
using BookingCareManagement.Infrastructure.Persistence;
using BookingCareManagement.Web.Areas.Doctor.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DomainDoctor = BookingCareManagement.Domain.Aggregates.Doctor.Doctor;
namespace BookingCareManagement.Web.Areas.Doctor.Controllers;

[Authorize(Policy = "DoctorOrAbove")]
[Route("api/doctor/dashboard")]
[ApiController]
public class DoctorDashboardController : ControllerBase
{
    private readonly ApplicationDBContext _dbContext;
    private static readonly TimeZoneInfo VietnamTimeZone = ResolveVietnamTimeZone();

    public DoctorDashboardController(ApplicationDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("new-customers")]
    public async Task<ActionResult<DashboardSparklineResponse>> GetNewCustomersAsync(
        [FromQuery] string? range = null,
        CancellationToken cancellationToken = default)
    {
        var doctor = await FindDoctorForCurrentUserAsync(cancellationToken);
        if (doctor is null)
        {
            return Forbid();
        }

        if (!TryResolveRange(range, out var resolvedRange, out var problem))
        {
            return BadRequest(problem);
        }

        var earliestAppointments = await _dbContext.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorId == doctor.Id)
            .GroupBy(a => a.PatientId ?? a.CustomerPhone)
            .Select(group => group.Min(a => a.StartUtc))
            .ToListAsync(cancellationToken);

        var localDates = earliestAppointments
            .Where(date => date >= resolvedRange.StartUtc && date < resolvedRange.EndUtc)
            .Select(ToVietnamLocalDate)
            .ToList();

        var aggregated = localDates
            .GroupBy(date => date)
            .ToDictionary(group => group.Key, group => (decimal)group.Count());

        var response = new DashboardSparklineResponse
        {
            Total = aggregated.Values.Sum(),
            RangeLabel = resolvedRange.DisplayName,
            Points = BuildDailyPoints(resolvedRange, aggregated)
        };

        return Ok(response);
    }

    [HttpGet("appointments-trend")]
    public async Task<ActionResult<DashboardAppointmentTrendResponse>> GetAppointmentsTrendAsync(
        [FromQuery] string? range = null,
        CancellationToken cancellationToken = default)
    {
        var doctor = await FindDoctorForCurrentUserAsync(cancellationToken);
        if (doctor is null)
        {
            return Forbid();
        }

        if (!TryResolveRange(range, out var resolvedRange, out var problem))
        {
            return BadRequest(problem);
        }

        var appointments = await _dbContext.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorId == doctor.Id && a.StartUtc >= resolvedRange.StartUtc && a.StartUtc < resolvedRange.EndUtc)
            .Select(a => new { a.StartUtc, a.Status })
            .ToListAsync(cancellationToken);

        var perDay = appointments
            .Select(a => ToVietnamLocalDate(a.StartUtc))
            .GroupBy(date => date)
            .ToDictionary(group => group.Key, group => (decimal)group.Count());

        var confirmedCount = appointments.Count(a => string.Equals(a.Status, AppointmentStatus.Approved, StringComparison.OrdinalIgnoreCase) || string.Equals(a.Status, AppointmentStatus.Pending, StringComparison.OrdinalIgnoreCase));
        var canceledCount = appointments.Count(a => string.Equals(a.Status, AppointmentStatus.Canceled, StringComparison.OrdinalIgnoreCase) || string.Equals(a.Status, AppointmentStatus.Rejected, StringComparison.OrdinalIgnoreCase));

        var response = new DashboardAppointmentTrendResponse
        {
            Points = BuildDailyPoints(resolvedRange, perDay),
            ConfirmedCount = confirmedCount,
            CanceledCount = canceledCount,
            RangeLabel = resolvedRange.DisplayName
        };

        return Ok(response);
    }

    [HttpGet("revenue")]
    public async Task<ActionResult<DashboardSparklineResponse>> GetRevenueAsync(
        [FromQuery] string? range = null,
        CancellationToken cancellationToken = default)
    {
        var doctor = await FindDoctorForCurrentUserAsync(cancellationToken);
        if (doctor is null)
        {
            return Forbid();
        }

        if (!TryResolveRange(range, out var resolvedRange, out var problem))
        {
            return BadRequest(problem);
        }

        var perDay = await LoadRevenuePerDayAsync(doctor.Id, resolvedRange, cancellationToken);
        var response = new DashboardSparklineResponse
        {
            Total = perDay.Values.Sum(),
            RangeLabel = resolvedRange.DisplayName,
            Points = BuildDailyPoints(resolvedRange, perDay)
        };

        return Ok(response);
    }

    [HttpGet("customer-mix")]
    public async Task<ActionResult<DashboardCustomerMixResponse>> GetCustomerMixAsync(
        [FromQuery] string? range = null,
        CancellationToken cancellationToken = default)
    {
        var doctor = await FindDoctorForCurrentUserAsync(cancellationToken);
        if (doctor is null)
        {
            return Forbid();
        }

        if (!TryResolveRange(range, out var resolvedRange, out var problem))
        {
            return BadRequest(problem);
        }

        var appointments = await _dbContext.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorId == doctor.Id && a.StartUtc >= resolvedRange.StartUtc && a.StartUtc < resolvedRange.EndUtc)
            .Select(a => new { a.PatientId, a.CustomerPhone })
            .ToListAsync(cancellationToken);

        if (appointments.Count == 0)
        {
            return Ok(new DashboardCustomerMixResponse
            {
                RangeLabel = resolvedRange.DisplayName
            });
        }

        var keys = appointments
            .Select(a => a.PatientId ?? a.CustomerPhone)
            .Where(key => !string.IsNullOrWhiteSpace(key))
            .Distinct()
            .ToList();

        var earliestByKey = await _dbContext.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorId == doctor.Id && keys.Contains(a.PatientId ?? a.CustomerPhone))
            .GroupBy(a => a.PatientId ?? a.CustomerPhone)
            .Select(group => new
            {
                Key = group.Key,
                FirstStartUtc = group.Min(x => x.StartUtc)
            })
            .ToListAsync(cancellationToken);

        var newCustomers = earliestByKey.Count(x => x.FirstStartUtc >= resolvedRange.StartUtc && x.FirstStartUtc < resolvedRange.EndUtc);
        var returningCustomers = earliestByKey.Count - newCustomers;

        return Ok(new DashboardCustomerMixResponse
        {
            NewCustomers = newCustomers,
            ReturningCustomers = returningCustomers,
            RangeLabel = resolvedRange.DisplayName
        });
    }

    [HttpGet("occupancy")]
    public async Task<ActionResult<DashboardSparklineResponse>> GetOccupancyAsync(
        [FromQuery] string? range = null,
        CancellationToken cancellationToken = default)
    {
        var doctor = await FindDoctorForCurrentUserAsync(cancellationToken);
        if (doctor is null)
        {
            return Forbid();
        }

        if (!TryResolveRange(range, out var resolvedRange, out var problem))
        {
            return BadRequest(problem);
        }

        var workingHours = await _dbContext.DoctorWorkingHours
            .AsNoTracking()
            .Where(x => x.DoctorId == doctor.Id)
            .Select(x => new { x.DayOfWeek, x.StartTime, x.EndTime })
            .ToListAsync(cancellationToken);

        var capacityByDay = workingHours
            .GroupBy(x => x.DayOfWeek)
            .ToDictionary(
                group => (DayOfWeek)group.Key,
                group => group.Sum(x => (decimal)(x.EndTime - x.StartTime).TotalMinutes));

        var appointments = await _dbContext.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorId == doctor.Id && a.StartUtc < resolvedRange.EndUtc && a.EndUtc > resolvedRange.StartUtc)
            .Select(a => new { a.StartUtc, a.EndUtc })
            .ToListAsync(cancellationToken);

        var dayCount = resolvedRange.TotalDays;
        var points = new List<DashboardMetricPointDto>(dayCount);
        decimal totalPercent = 0;

        for (var date = resolvedRange.StartDate; date < resolvedRange.EndDateExclusive; date = date.AddDays(1))
        {
            var capacityMinutes = capacityByDay.TryGetValue(date.DayOfWeek, out var minutes)
                ? minutes
                : 0m;

            var dayStartUtc = ConvertLocalDateToUtc(date);
            var dayEndUtc = ConvertLocalDateToUtc(date.AddDays(1));

            double bookedMinutes = 0;
            foreach (var appointment in appointments)
            {
                if (appointment.StartUtc >= dayEndUtc || appointment.EndUtc <= dayStartUtc)
                {
                    continue;
                }

                var overlapStart = appointment.StartUtc > dayStartUtc ? appointment.StartUtc : dayStartUtc;
                var overlapEnd = appointment.EndUtc < dayEndUtc ? appointment.EndUtc : dayEndUtc;
                bookedMinutes += Math.Max(0, (overlapEnd - overlapStart).TotalMinutes);
            }

            var occupancyPercent = capacityMinutes <= 0
                ? 0m
                : (decimal)(bookedMinutes / (double)capacityMinutes * 100d);

            totalPercent += occupancyPercent;
            points.Add(new DashboardMetricPointDto
            {
                Date = date,
                Value = Math.Round(occupancyPercent, 2, MidpointRounding.AwayFromZero)
            });
        }

        var response = new DashboardSparklineResponse
        {
            Total = dayCount <= 0 ? 0 : Math.Round(totalPercent / dayCount, 2, MidpointRounding.AwayFromZero),
            RangeLabel = resolvedRange.DisplayName,
            Points = points
        };

        return Ok(response);
    }

    [HttpGet("heatmap")]
    public async Task<ActionResult<DashboardHeatmapResponse>> GetHeatmapAsync(
        [FromQuery] string? month = null,
        CancellationToken cancellationToken = default)
    {
        var doctor = await FindDoctorForCurrentUserAsync(cancellationToken);
        if (doctor is null)
        {
            return Forbid();
        }

        if (!TryResolveMonth(month, out var monthRange, out var problem))
        {
            return BadRequest(problem);
        }

        var workingHours = await _dbContext.DoctorWorkingHours
            .AsNoTracking()
            .Where(x => x.DoctorId == doctor.Id)
            .Select(x => new { x.DayOfWeek, x.StartTime, x.EndTime })
            .ToListAsync(cancellationToken);

        var capacityByDay = workingHours
            .GroupBy(x => x.DayOfWeek)
            .ToDictionary(
                group => (DayOfWeek)group.Key,
                group => group.Sum(x => (decimal)(x.EndTime - x.StartTime).TotalMinutes));

        var appointments = await _dbContext.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorId == doctor.Id && a.StartUtc < monthRange.EndUtc && a.EndUtc > monthRange.StartUtc)
            .Select(a => new { a.StartUtc, a.EndUtc })
            .ToListAsync(cancellationToken);

        var cells = new List<DashboardHeatmapCellDto>();
        for (var date = monthRange.StartDate; date < monthRange.EndDateExclusive; date = date.AddDays(1))
        {
            var capacityMinutes = capacityByDay.TryGetValue(date.DayOfWeek, out var minutes)
                ? minutes
                : 0m;

            var dayStartUtc = ConvertLocalDateToUtc(date);
            var dayEndUtc = ConvertLocalDateToUtc(date.AddDays(1));

            double bookedMinutes = 0;
            foreach (var appointment in appointments)
            {
                if (appointment.StartUtc >= dayEndUtc || appointment.EndUtc <= dayStartUtc)
                {
                    continue;
                }

                var overlapStart = appointment.StartUtc > dayStartUtc ? appointment.StartUtc : dayStartUtc;
                var overlapEnd = appointment.EndUtc < dayEndUtc ? appointment.EndUtc : dayEndUtc;
                bookedMinutes += Math.Max(0, (overlapEnd - overlapStart).TotalMinutes);
            }

            var percent = capacityMinutes <= 0
                ? 0d
                : Math.Min(100d, Math.Max(0d, bookedMinutes / (double)capacityMinutes * 100d));

            cells.Add(new DashboardHeatmapCellDto
            {
                Date = date,
                OccupancyPercent = Math.Round(percent, 2, MidpointRounding.AwayFromZero)
            });
        }

        return Ok(new DashboardHeatmapResponse
        {
            Year = monthRange.StartDate.Year,
            Month = monthRange.StartDate.Month,
            Cells = cells
        });
    }

    [HttpGet("performance")]
    public async Task<ActionResult<DoctorPerformanceResponseDto>> GetPerformanceAsync(
        [FromQuery] string? range = null,
        CancellationToken cancellationToken = default)
    {
        var doctor = await FindDoctorForCurrentUserAsync(cancellationToken);
        if (doctor is null)
        {
            return Forbid();
        }

        if (!TryResolveRange(range, out var resolvedRange, out var problem))
        {
            return BadRequest(problem);
        }

        var appointments = await _dbContext.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorId == doctor.Id && a.StartUtc >= resolvedRange.StartUtc && a.StartUtc < resolvedRange.EndUtc)
            .Select(a => new
            {
                a.SpecialtyId,
                a.PatientId,
                a.PatientName,
                a.CustomerPhone,
                a.StartUtc,
                a.EndUtc
            })
            .ToListAsync(cancellationToken);

        if (appointments.Count == 0)
        {
            return Ok(new DoctorPerformanceResponseDto
            {
                RangeLabel = resolvedRange.DisplayName
            });
        }

        var specialtyIds = appointments
            .Select(a => a.SpecialtyId)
            .Distinct()
            .ToArray();

        var specialties = specialtyIds.Length == 0
            ? new Dictionary<Guid, SpecialtyBrief>()
            : await _dbContext.Specialties
                .AsNoTracking()
                .Where(s => specialtyIds.Contains(s.Id))
                .Select(s => new { s.Id, s.Name, s.Color })
                .ToDictionaryAsync(
                    s => s.Id,
                    s => new SpecialtyBrief(
                        s.Name ?? "Dịch vụ",
                        string.IsNullOrWhiteSpace(s.Color) ? "#0ea5e9" : s.Color!),
                    cancellationToken);

        var totalAppointments = appointments.Count;
        var serviceDtos = appointments
            .GroupBy(a => a.SpecialtyId)
            .Select(group =>
            {
                var info = specialties.TryGetValue(group.Key, out var matched)
                    ? matched
                    : new SpecialtyBrief("Dịch vụ tổng hợp", "#0ea5e9");
                var appointmentCount = group.Count();
                var occupancyPercent = totalAppointments == 0
                    ? 0m
                    : Math.Round((decimal)appointmentCount / totalAppointments * 100m, 2, MidpointRounding.AwayFromZero);

                return new DoctorPerformanceServiceDto
                {
                    SpecialtyId = group.Key,
                    Name = info.Name,
                    Color = info.Color,
                    AppointmentCount = appointmentCount,
                    Revenue = 0m,
                    OccupancyPercent = occupancyPercent
                };
            })
            .OrderByDescending(x => x.AppointmentCount)
            .ThenBy(x => x.Name)
            .Take(5)
            .ToArray();

        var patientDtos = appointments
            .GroupBy(a => BuildPatientKey(a.PatientId, a.CustomerPhone, a.PatientName))
            .Select(group =>
            {
                var representative = group.OrderByDescending(x => x.StartUtc).First();
                var lastVisitUtc = DateTime.SpecifyKind(representative.StartUtc, DateTimeKind.Utc);
                return new DoctorPerformancePatientDto
                {
                    PatientId = representative.PatientId,
                    Key = BuildPatientKey(representative.PatientId, representative.CustomerPhone, representative.PatientName),
                    Name = string.IsNullOrWhiteSpace(representative.PatientName) ? "Bệnh nhân" : representative.PatientName,
                    Phone = representative.CustomerPhone ?? string.Empty,
                    AppointmentCount = group.Count(),
                    LastVisitUtc = lastVisitUtc,
                    LastVisitLabel = FormatVietnamDateTime(lastVisitUtc)
                };
            })
            .OrderByDescending(x => x.AppointmentCount)
            .ThenBy(x => x.Name)
            .Take(5)
            .ToArray();

        return Ok(new DoctorPerformanceResponseDto
        {
            RangeLabel = resolvedRange.DisplayName,
            Services = serviceDtos,
            Patients = patientDtos
        });
    }

    private static string BuildPatientKey(string? patientId, string? phone, string? name)
    {
        if (!string.IsNullOrWhiteSpace(patientId))
        {
            return patientId.Trim();
        }

        if (!string.IsNullOrWhiteSpace(phone))
        {
            return phone.Trim();
        }

        return string.IsNullOrWhiteSpace(name) ? "unknown-patient" : name.Trim();
    }

    private static string FormatVietnamDateTime(DateTime utcDateTime)
    {
        var utc = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
        var local = TimeZoneInfo.ConvertTimeFromUtc(utc, VietnamTimeZone);
        return local.ToString("dd/MM/yyyy HH:mm", CultureInfo.GetCultureInfo("vi-VN"));
    }

    private sealed record SpecialtyBrief(string Name, string Color);

    private async Task<Dictionary<DateOnly, decimal>> LoadRevenuePerDayAsync(Guid doctorId, DashboardRange range, CancellationToken cancellationToken)
    {
        var perDay = new Dictionary<DateOnly, decimal>();
        var appointments = await _dbContext.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorId == doctorId && a.StartUtc >= range.StartUtc && a.StartUtc < range.EndUtc)
            .Select(a => a.StartUtc)
            .ToListAsync(cancellationToken);

        foreach (var appointmentStart in appointments)
        {
            var localDate = ToVietnamLocalDate(appointmentStart);
            if (!perDay.ContainsKey(localDate))
            {
                perDay[localDate] = 0m;
            }
        }

        // Giá trị vẫn bằng 0 cho đến khi hoàn tất tính năng lưu giá booking.
        return perDay;
    }

    private async Task<DomainDoctor?> FindDoctorForCurrentUserAsync(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        return await _dbContext.Doctors
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.AppUserId == userId, cancellationToken);
    }

    private static bool TryResolveRange(string? token, out DashboardRange range, out ProblemDetails? problem)
    {
        var normalized = string.IsNullOrWhiteSpace(token)
            ? "this-week"
            : token.Trim().ToLowerInvariant();

        var nowLocal = TimeZoneInfo.ConvertTime(DateTime.UtcNow, VietnamTimeZone);
        var today = DateOnly.FromDateTime(nowLocal);
        var startOfWeek = GetStartOfWeek(today);
        var startOfMonth = new DateOnly(today.Year, today.Month, 1);
        var startOfYear = new DateOnly(today.Year, 1, 1);

        DashboardRange? candidate = normalized switch
        {
            "this-week" or "current-week" => new DashboardRange(startOfWeek, startOfWeek.AddDays(7), "Tuần hiện tại"),
            "last-week" or "previous-week" => new DashboardRange(startOfWeek.AddDays(-7), startOfWeek, "Tuần trước"),
            "this-month" or "current-month" => new DashboardRange(startOfMonth, startOfMonth.AddMonths(1), "Tháng này"),
            "last-month" or "previous-month" => new DashboardRange(startOfMonth.AddMonths(-1), startOfMonth, "Tháng trước"),
            "three-months" or "3-months" => new DashboardRange(startOfMonth.AddMonths(-2), startOfMonth.AddMonths(1), "3 tháng qua"),
            "six-months" or "6-months" => new DashboardRange(startOfMonth.AddMonths(-5), startOfMonth.AddMonths(1), "6 tháng qua"),
            "twelve-months" or "12-months" or "year" => new DashboardRange(startOfMonth.AddMonths(-11), startOfMonth.AddMonths(1), "12 tháng qua"),
            "this-year" or "current-year" => new DashboardRange(startOfYear, startOfYear.AddYears(1), "Năm nay"),
            "last-year" or "previous-year" => new DashboardRange(startOfYear.AddYears(-1), startOfYear, "Năm trước"),
            _ => null
        };

        if (candidate is null)
        {
            problem = new ProblemDetails { Title = "Khoảng thời gian không hợp lệ" };
            range = default!;
            return false;
        }

        range = candidate;
        problem = null;
        return true;
    }

    private static bool TryResolveMonth(string? input, out DashboardRange range, out ProblemDetails? problem)
    {
        var nowLocal = TimeZoneInfo.ConvertTime(DateTime.UtcNow, VietnamTimeZone);
        var fallbackStart = new DateOnly(nowLocal.Year, nowLocal.Month, 1);

        if (string.IsNullOrWhiteSpace(input))
        {
            range = new DashboardRange(fallbackStart, fallbackStart.AddMonths(1), $"Tháng {fallbackStart.Month}, {fallbackStart.Year}");
            problem = null;
            return true;
        }

        if (!DateTime.TryParseExact(input, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            range = default!;
            problem = new ProblemDetails { Title = "Định dạng tháng không hợp lệ" };
            return false;
        }

        var start = new DateOnly(parsed.Year, parsed.Month, 1);
        range = new DashboardRange(start, start.AddMonths(1), $"Tháng {start.Month}, {start.Year}");
        problem = null;
        return true;
    }

    private static IReadOnlyList<DashboardMetricPointDto> BuildDailyPoints(DashboardRange range, IReadOnlyDictionary<DateOnly, decimal> aggregated)
    {
        var points = new List<DashboardMetricPointDto>(range.TotalDays);
        for (var date = range.StartDate; date < range.EndDateExclusive; date = date.AddDays(1))
        {
            var value = aggregated.TryGetValue(date, out var current) ? current : 0m;
            points.Add(new DashboardMetricPointDto
            {
                Date = date,
                Value = value
            });
        }

        return points;
    }

    private static DateOnly ToVietnamLocalDate(DateTime utcDateTime)
    {
        var utc = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
        var local = TimeZoneInfo.ConvertTimeFromUtc(utc, VietnamTimeZone);
        return DateOnly.FromDateTime(local);
    }

    private static DateTime ConvertLocalDateToUtc(DateOnly date)
    {
        var localDateTime = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(localDateTime, VietnamTimeZone);
    }

    private static DateOnly GetStartOfWeek(DateOnly date)
    {
        var diff = ((int)date.DayOfWeek + 6) % 7;
        return date.AddDays(-diff);
    }

    private static TimeZoneInfo ResolveVietnamTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Asia/Bangkok");
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.CreateCustomTimeZone("SE Asia Standard Time", TimeSpan.FromHours(7), "SE Asia", "SE Asia");
        }
    }

    private sealed class DashboardRange
    {
        public DashboardRange(DateOnly startDate, DateOnly endDateExclusive, string displayName)
        {
            StartDate = startDate;
            EndDateExclusive = endDateExclusive;
            DisplayName = displayName;
        }

        public DateOnly StartDate { get; }
        public DateOnly EndDateExclusive { get; }
        public string DisplayName { get; set; }
        public int TotalDays => EndDateExclusive.DayNumber - StartDate.DayNumber;
        public DateTime StartUtc => ConvertLocalDateToUtc(StartDate);
        public DateTime EndUtc => ConvertLocalDateToUtc(EndDateExclusive);
    }
}
