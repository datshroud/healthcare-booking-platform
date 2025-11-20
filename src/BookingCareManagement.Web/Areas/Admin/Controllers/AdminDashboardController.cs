using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BookingCareManagement.Domain.Aggregates.Appointment;
using BookingCareManagement.Infrastructure.Persistence;
using BookingCareManagement.Web.Areas.Admin.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookingCareManagement.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
[ApiController]
[Route("api/admin/dashboard")]
public sealed class AdminDashboardController : ControllerBase
{
    private readonly ApplicationDBContext _dbContext;
    private static readonly CultureInfo DisplayCulture = CultureInfo.GetCultureInfo("vi-VN");
    private static readonly TimeZoneInfo VietnamTimeZone = ResolveVietnamTimeZone();

    public AdminDashboardController(ApplicationDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("overview")]
    public async Task<ActionResult<AdminDashboardOverviewDto>> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        var nowLocal = TimeZoneInfo.ConvertTime(DateTime.UtcNow, VietnamTimeZone);
        var todayStartLocal = new DateTime(nowLocal.Year, nowLocal.Month, nowLocal.Day, 0, 0, 0, DateTimeKind.Unspecified);
        var todayStartUtc = TimeZoneInfo.ConvertTimeToUtc(todayStartLocal, VietnamTimeZone);
        var tomorrowUtc = todayStartUtc.AddDays(1);
        var yesterdayUtc = todayStartUtc.AddDays(-1);

        var appointmentsTodayTask = _dbContext.Appointments
            .AsNoTracking()
            .Where(a => a.StartUtc >= todayStartUtc && a.StartUtc < tomorrowUtc)
            .CountAsync(cancellationToken);

        var appointmentsYesterdayTask = _dbContext.Appointments
            .AsNoTracking()
            .Where(a => a.StartUtc >= yesterdayUtc && a.StartUtc < todayStartUtc)
            .CountAsync(cancellationToken);

        var activeDoctorsTask = _dbContext.Doctors
            .AsNoTracking()
            .CountAsync(d => d.Active, cancellationToken);

        var nowUtc = DateTime.UtcNow;
        var newCustomerWindowStart = nowUtc.AddDays(-7);
        var prevCustomerWindowStart = newCustomerWindowStart.AddDays(-7);

        var customerRoleId = await _dbContext.Roles
            .AsNoTracking()
            .Where(r => r.NormalizedName == "CUSTOMER")
            .Select(r => r.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var newCustomersTask = CountCustomersAsync(newCustomerWindowStart, nowUtc, customerRoleId, cancellationToken);
        var prevCustomersTask = CountCustomersAsync(prevCustomerWindowStart, newCustomerWindowStart, customerRoleId, cancellationToken);

        var (weekStartUtc, weekEndUtc, previousWeekStartUtc, previousWeekEndUtc) = ResolveWeekBoundaries(todayStartLocal);
        var revenueTask = SumInvoicesAsync(weekStartUtc, weekEndUtc, cancellationToken);
        var prevRevenueTask = SumInvoicesAsync(previousWeekStartUtc, previousWeekEndUtc, cancellationToken);

        await Task.WhenAll(appointmentsTodayTask, appointmentsYesterdayTask, activeDoctorsTask, newCustomersTask, prevCustomersTask, revenueTask, prevRevenueTask);

        var cards = new[]
        {
            new AdminDashboardCardDto
            {
                Title = "Cuộc hẹn hôm nay",
                Subtitle = "Tổng số ca khám",
                Value = appointmentsTodayTask.Result.ToString("N0", DisplayCulture),
                TrendLabel = FormatTrend(appointmentsTodayTask.Result, appointmentsYesterdayTask.Result, "so với hôm qua"),
                AccentColor = "#2563eb"
            },
            new AdminDashboardCardDto
            {
                Title = "Bác sĩ đang hoạt động",
                Subtitle = "Đủ điều kiện nhận lịch",
                Value = activeDoctorsTask.Result.ToString("N0", DisplayCulture),
                TrendLabel = activeDoctorsTask.Result == 0 ? "Chưa có bác sĩ trực" : "Sẵn sàng tiếp nhận",
                AccentColor = "#16a34a"
            },
            new AdminDashboardCardDto
            {
                Title = "Bệnh nhân mới",
                Subtitle = "Trong 7 ngày qua",
                Value = newCustomersTask.Result.ToString("N0", DisplayCulture),
                TrendLabel = FormatTrend(newCustomersTask.Result, prevCustomersTask.Result, "so với tuần trước"),
                AccentColor = "#f97316"
            },
            new AdminDashboardCardDto
            {
                Title = "Doanh thu dự kiến",
                Subtitle = "Tuần hiện tại",
                Value = revenueTask.Result.ToString("C0", DisplayCulture),
                TrendLabel = FormatTrend((double)revenueTask.Result, (double)prevRevenueTask.Result, "so với tuần trước"),
                AccentColor = "#8b5cf6"
            }
        }.ToList();

        var upcoming = await LoadUpcomingAppointmentsAsync(nowUtc, cancellationToken);
        var activities = await LoadAdminActivitiesAsync(cancellationToken);

        var overview = new AdminDashboardOverviewDto
        {
            Cards = cards,
            UpcomingAppointments = upcoming,
            Activities = activities
        };

        return Ok(overview);
    }

    private async Task<int> CountCustomersAsync(DateTime startUtc, DateTime endUtc, string? customerRoleId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(customerRoleId))
        {
            return 0;
        }

        return await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.CreatedAt >= startUtc && u.CreatedAt < endUtc)
            .Where(u => _dbContext.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == customerRoleId))
            .CountAsync(cancellationToken);
    }

    private async Task<decimal> SumInvoicesAsync(DateTime startUtc, DateTime endUtc, CancellationToken cancellationToken)
    {
        var sum = await _dbContext.Invoices
            .AsNoTracking()
            .Where(i => i.InvoiceDate >= startUtc && i.InvoiceDate < endUtc)
            .SumAsync(i => (decimal?)i.Total, cancellationToken);

        return sum ?? 0m;
    }

    private async Task<List<AdminDashboardAppointmentDto>> LoadUpcomingAppointmentsAsync(DateTime nowUtc, CancellationToken cancellationToken)
    {
        var records = await (
            from appointment in _dbContext.Appointments.AsNoTracking()
            where appointment.StartUtc >= nowUtc
            join specialty in _dbContext.Specialties.AsNoTracking() on appointment.SpecialtyId equals specialty.Id
            join doctor in _dbContext.Doctors.AsNoTracking() on appointment.DoctorId equals doctor.Id
            join user in _dbContext.Users.AsNoTracking() on doctor.AppUserId equals user.Id
            orderby appointment.StartUtc
            select new
            {
                appointment.Id,
                appointment.StartUtc,
                appointment.PatientName,
                appointment.Status,
                SpecialtyName = specialty.Name,
                DoctorName = ResolveDoctorName(user)
            })
            .Take(5)
            .ToListAsync(cancellationToken);

        return records
            .Select(r =>
            {
                var localTime = TimeZoneInfo.ConvertTimeFromUtc(r.StartUtc, VietnamTimeZone);
                return new AdminDashboardAppointmentDto
                {
                    Id = r.Id,
                    StartUtc = r.StartUtc,
                    TimeLabel = $"{localTime:HH:mm} · {localTime:dd/MM}",
                    CustomerName = string.IsNullOrWhiteSpace(r.PatientName) ? "Bệnh nhân" : r.PatientName,
                    DoctorName = r.DoctorName,
                    ServiceName = string.IsNullOrWhiteSpace(r.SpecialtyName) ? "Dịch vụ" : r.SpecialtyName,
                    Status = AppointmentStatus.NormalizeOrDefault(r.Status)
                };
            })
            .ToList();
    }

    private async Task<List<AdminDashboardActivityDto>> LoadAdminActivitiesAsync(CancellationToken cancellationToken)
    {
        var notifications = await _dbContext.AdminNotifications
            .AsNoTracking()
            .OrderByDescending(n => n.CreatedAtUtc)
            .Take(5)
            .ToListAsync(cancellationToken);

        return notifications
            .Select(n =>
            {
                var local = TimeZoneInfo.ConvertTimeFromUtc(n.CreatedAtUtc, VietnamTimeZone);
                return new AdminDashboardActivityDto
                {
                    Time = local.ToString("HH:mm", DisplayCulture),
                    Description = string.IsNullOrWhiteSpace(n.Message) ? n.Title : n.Message
                };
            })
            .ToList();
    }

    private static string ResolveDoctorName(Domain.Aggregates.User.AppUser user)
    {
        if (!string.IsNullOrWhiteSpace(user.FullName))
        {
            return user.FullName;
        }

        var first = user.FirstName;
        var last = user.LastName;
        if (!string.IsNullOrWhiteSpace(first) || !string.IsNullOrWhiteSpace(last))
        {
            return $"{(first ?? string.Empty)} {(last ?? string.Empty)}".Trim();
        }

        return user.Email ?? user.UserName ?? "Bác sĩ";
    }

    private static (DateTime StartUtc, DateTime EndUtc, DateTime PrevStartUtc, DateTime PrevEndUtc) ResolveWeekBoundaries(DateTime todayStartLocal)
    {
        var diff = ((int)todayStartLocal.DayOfWeek + 6) % 7;
        var weekStartLocal = todayStartLocal.AddDays(-diff);
        var previousWeekStartLocal = weekStartLocal.AddDays(-7);

        var weekStartUtc = TimeZoneInfo.ConvertTimeToUtc(weekStartLocal, VietnamTimeZone);
        var weekEndUtc = weekStartUtc.AddDays(7);
        var previousWeekStartUtc = TimeZoneInfo.ConvertTimeToUtc(previousWeekStartLocal, VietnamTimeZone);

        return (weekStartUtc, weekEndUtc, previousWeekStartUtc, previousWeekStartUtc.AddDays(7));
    }

    private static string FormatTrend(double current, double previous, string suffix)
    {
        if (previous == 0 && current == 0)
        {
            return $"Không thay đổi {suffix}";
        }

        if (previous == 0)
        {
            return $"↑ {current.ToString("N0", DisplayCulture)} {suffix}";
        }

        var delta = current - previous;
        var symbol = delta >= 0 ? "↑" : "↓";
        return $"{symbol} {Math.Abs(delta).ToString("N0", DisplayCulture)} {suffix}";
    }

    private static TimeZoneInfo ResolveVietnamTimeZone()
    {
        var candidates = new[] { "Asia/Ho_Chi_Minh", "SE Asia Standard Time" };
        foreach (var candidate in candidates)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(candidate);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        return TimeZoneInfo.Utc;
    }
}
