using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BookingCareManagement.Domain.Aggregates.Appointment;
using BookingCareManagement.Domain.Aggregates.ClinicRoom;
using BookingCareManagement.Domain.Aggregates.Doctor;
using BookingCareManagement.Domain.Aggregates.User;
using BookingCareManagement.Infrastructure.Persistence;
using BookingCareManagement.Web.Areas.Admin.Dtos;
using BookingCareManagement.Web.Areas.Doctor.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookingCareManagement.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
[ApiController]
[Route("api/admin/appointments")]
public sealed class AdminAppointmentsController : ControllerBase
{
    private readonly ApplicationDBContext _dbContext;
    private static readonly CultureInfo DisplayCulture = CultureInfo.GetCultureInfo("vi-VN");
    private static readonly TimeZoneInfo DisplayTimeZone = ResolveVietnamTimeZone();
    private static readonly IReadOnlyDictionary<string, AppointmentStatusPresentation> StatusPresentationMap =
        new Dictionary<string, AppointmentStatusPresentation>(StringComparer.OrdinalIgnoreCase)
        {
            [AppointmentStatus.Pending] = new(AppointmentStatus.Pending, "Chờ xác nhận", "pending", "fa-clock"),
            [AppointmentStatus.Approved] = new(AppointmentStatus.Approved, "Đã xác nhận", "approved", "fa-circle-check"),
            [AppointmentStatus.Canceled] = new(AppointmentStatus.Canceled, "Đã hủy", "canceled", "fa-ban"),
            [AppointmentStatus.Rejected] = new(AppointmentStatus.Rejected, "Từ chối", "rejected", "fa-circle-xmark"),
            [AppointmentStatus.NoShow] = new(AppointmentStatus.NoShow, "Vắng mặt", "noshow", "fa-user-xmark")
        };

    public AdminAppointmentsController(ApplicationDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("metadata")]
    public async Task<ActionResult<AdminAppointmentMetadataDto>> GetMetadata(CancellationToken cancellationToken)
    {
        var specialties = await _dbContext.Specialties
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => new AdminAppointmentSpecialtyOptionDto
            {
                Id = s.Id,
                Name = s.Name,
                Color = string.IsNullOrWhiteSpace(s.Color) ? "#0ea5e9" : s.Color
            })
            .ToArrayAsync(cancellationToken);

        var statuses = StatusPresentationMap
            .Values
            .Select(x => new AdminAppointmentStatusOptionDto
            {
                Code = x.Code,
                Label = x.Label,
                Tone = x.Tone,
                Icon = x.Icon
            })
            .ToArray();

        var doctors = await _dbContext.Doctors
            .AsNoTracking()
            .Where(d => d.Active)
            .OrderBy(d => d.AppUser.FirstName)
            .ThenBy(d => d.AppUser.LastName)
            .Select(d => new
            {
                d.Id,
                d.AppUser.FirstName,
                d.AppUser.LastName,
                d.AppUser.Email,
                d.AppUser.UserName,
                d.AppUser.AvatarUrl
            })
            .ToListAsync(cancellationToken);

        var doctorOptions = doctors
            .Select(d => new AdminAppointmentDoctorOptionDto
            {
                Id = d.Id,
                Name = BuildDisplayName(d.FirstName, d.LastName, d.Email, d.UserName),
                AvatarUrl = d.AvatarUrl ?? string.Empty
            })
            .ToArray();

        var customerRoleId = await _dbContext.Roles
            .AsNoTracking()
            .Where(r => r.NormalizedName == "CUSTOMER")
            .Select(r => r.Id)
            .FirstOrDefaultAsync(cancellationToken);

        IQueryable<AppUser> patientQuery = _dbContext.Users.AsNoTracking();
        if (!string.IsNullOrEmpty(customerRoleId))
        {
            patientQuery = patientQuery.Where(u => _dbContext.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == customerRoleId));
        }

        var patients = await patientQuery
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .Select(u => new
            {
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email,
                u.UserName,
                u.PhoneNumber
            })
            .ToListAsync(cancellationToken);

        var patientOptions = patients
            .Select(u => new AdminAppointmentPatientOptionDto
            {
                Id = u.Id,
                Name = BuildDisplayName(u.FirstName, u.LastName, u.Email, u.UserName),
                PhoneNumber = u.PhoneNumber ?? string.Empty
            })
            .ToArray();

        var dto = new AdminAppointmentMetadataDto
        {
            Specialties = specialties,
            Statuses = statuses,
            Doctors = doctorOptions,
            Patients = patientOptions
        };

        return Ok(dto);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<DoctorAppointmentListItemDto>>> GetAppointments(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken cancellationToken)
    {
        var query =
            from appointment in _dbContext.Appointments.AsNoTracking()
            join specialty in _dbContext.Specialties.AsNoTracking() on appointment.SpecialtyId equals specialty.Id
            join doctor in _dbContext.Doctors.AsNoTracking() on appointment.DoctorId equals doctor.Id
            join doctorUser in _dbContext.Users.AsNoTracking() on doctor.AppUserId equals doctorUser.Id
            join room in _dbContext.ClinicRooms.AsNoTracking() on appointment.ClinicRoomId equals room.Id into roomGroup
            from room in roomGroup.DefaultIfEmpty()
            select new { appointment, specialty, doctorUser, room };

        if (from.HasValue)
        {
            var fromLocal = DateTime.SpecifyKind(from.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Local);
            var fromUtc = fromLocal.ToUniversalTime();
            query = query.Where(x => x.appointment.StartUtc >= fromUtc);
        }

        if (to.HasValue)
        {
            var toLocal = DateTime.SpecifyKind(to.Value.ToDateTime(new TimeOnly(23, 59, 59)), DateTimeKind.Local);
            var toUtc = toLocal.ToUniversalTime();
            query = query.Where(x => x.appointment.StartUtc <= toUtc);
        }

        var records = await query
            .OrderBy(x => x.appointment.StartUtc)
            .ToListAsync(cancellationToken);

        var dtos = records
            .Select(x => ToListItemDto(x.appointment, x.specialty, x.doctorUser, x.room))
            .ToArray();

        return Ok(dtos);
    }

    private static DoctorAppointmentListItemDto ToListItemDto(
        Domain.Aggregates.Appointment.Appointment appointment,
        Specialty specialty,
        AppUser doctorUser,
        ClinicRoom? room)
    {
        var startUtc = DateTime.SpecifyKind(appointment.StartUtc, DateTimeKind.Utc);
        var endUtc = DateTime.SpecifyKind(appointment.EndUtc, DateTimeKind.Utc);
        var startLocal = TimeZoneInfo.ConvertTimeFromUtc(startUtc, DisplayTimeZone);
        var endLocal = TimeZoneInfo.ConvertTimeFromUtc(endUtc, DisplayTimeZone);

        var dateLabel = $"{CapitalizeFirst(DisplayCulture.DateTimeFormat.GetDayName(startLocal.DayOfWeek))}, {startLocal:dd/MM/yyyy}";
        var timeLabel = $"{startLocal:HH:mm} - {endLocal:HH:mm}";
        var statusCode = AppointmentStatus.NormalizeOrDefault(appointment.Status);
        if (!StatusPresentationMap.TryGetValue(statusCode, out var presentation))
        {
            presentation = StatusPresentationMap[AppointmentStatus.Pending];
        }

        return new DoctorAppointmentListItemDto
        {
            Id = appointment.Id,
            SpecialtyId = appointment.SpecialtyId,
            SpecialtyName = specialty.Name,
            SpecialtyColor = string.IsNullOrWhiteSpace(specialty.Color) ? "#0ea5e9" : specialty.Color,
            PatientName = appointment.PatientName,
            CustomerPhone = appointment.CustomerPhone,
            Status = presentation.Code,
            StatusLabel = presentation.Label,
            StatusTone = presentation.Tone,
            StatusIcon = presentation.Icon,
            DoctorName = doctorUser.GetFullName(),
            DoctorInitials = BuildInitials(doctorUser.GetFullName()),
            DoctorAvatarUrl = ResolveDoctorAvatar(doctorUser),
            StartUtc = startUtc,
            EndUtc = endUtc,
            DateLabel = dateLabel,
            DateKey = startLocal.ToString("yyyy-MM-dd"),
            TimeLabel = timeLabel,
            DurationMinutes = (int)Math.Round((endUtc - startUtc).TotalMinutes),
            ClinicRoom = BuildClinicRoomLabel(room)
        };
    }

    private static string BuildClinicRoomLabel(ClinicRoom? room)
    {
        if (room is null)
        {
            return "Tư vấn trực tuyến";
        }

        return string.IsNullOrWhiteSpace(room.Code) ? "Phòng khám" : $"Phòng {room.Code}";
    }

    private static string BuildInitials(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "DR";
        }

        var parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 1)
        {
            return parts[0][0].ToString().ToUpper(DisplayCulture);
        }

        var first = parts[0][0];
        var last = parts[^1][0];
        return string.Concat(char.ToUpper(first, DisplayCulture), char.ToUpper(last, DisplayCulture));
    }

    private static string CapitalizeFirst(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value.Length == 1
            ? value.ToUpper(DisplayCulture)
            : char.ToUpper(value[0], DisplayCulture) + value[1..];
    }

    private static string ResolveDoctorAvatar(AppUser doctorUser)
    {
        if (!string.IsNullOrWhiteSpace(doctorUser.AvatarUrl))
        {
            return doctorUser.AvatarUrl;
        }

        var fullName = doctorUser.GetFullName();
        var fallback = string.IsNullOrWhiteSpace(fullName) ? "Bac Si" : fullName;
        var encoded = Uri.EscapeDataString(fallback);
        return $"https://ui-avatars.com/api/?name={encoded}&background=0ea5e9&color=fff&size=96";
    }

    private static string BuildDisplayName(string? firstName, string? lastName, string? email, string? userName)
    {
        var parts = new[] { firstName, lastName }
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s!.Trim());

        var displayName = string.Join(' ', parts);
        if (!string.IsNullOrWhiteSpace(displayName))
        {
            return displayName;
        }

        return (email ?? userName ?? string.Empty).Trim();
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

        return TimeZoneInfo.Local;
    }

    private sealed record AppointmentStatusPresentation(string Code, string Label, string Tone, string Icon);
}
