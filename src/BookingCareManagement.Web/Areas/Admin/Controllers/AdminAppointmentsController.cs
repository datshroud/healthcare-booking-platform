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
using BookingCareManagement.Application.Features.Calendar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
            .Include(d => d.Specialties)
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
                d.AppUser.AvatarUrl,
                SpecialtyIds = d.Specialties.Select(s => s.Id).ToArray()
            })
            .ToListAsync(cancellationToken);

        var doctorOptions = doctors
            .Select(d => new AdminAppointmentDoctorOptionDto
            {
                Id = d.Id,
                Name = BuildDisplayName(d.FirstName, d.LastName, d.Email, d.UserName),
                AvatarUrl = d.AvatarUrl ?? string.Empty,
                SpecialtyIds = d.SpecialtyIds
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

    [HttpGet("calendar")]
    public async Task<ActionResult<IReadOnlyCollection<CalendarEventDto>>> GetCalendarEvents(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] Guid[]? doctorIds,
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

        if (doctorIds is { Length: > 0 })
        {
            var filteredIds = doctorIds.Where(id => id != Guid.Empty).Distinct().ToArray();
            if (filteredIds.Length > 0)
            {
                query = query.Where(x => filteredIds.Contains(x.appointment.DoctorId));
            }
        }

        var records = await query
            .OrderBy(x => x.appointment.StartUtc)
            .ToListAsync(cancellationToken);

        var events = records
            .Select(x => ToCalendarEventDto(x.appointment, x.specialty, x.doctorUser, x.room))
            .ToArray();

        return Ok(events);
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

    [HttpPost]
    public async Task<ActionResult<DoctorAppointmentListItemDto>> Create(
        [FromBody] AdminAppointmentUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var (result, problem) = await ValidateUpsertRequestAsync(request, excludeAppointmentId: null, fallbackClinicRoomId: null, cancellationToken);
        if (problem is not null)
        {
            return problem.Status == StatusCodes.Status409Conflict
                ? Conflict(problem)
                : BadRequest(problem);
        }

        var appointment = new Domain.Aggregates.Appointment.Appointment(
            result!.Doctor.Id,
            result.Specialty.Id,
            result.ClinicRoomId,
            result.SlotStartUtc,
            TimeSpan.FromMinutes(result.DurationMinutes),
            result.PatientName,
            result.CustomerPhone,
            result.PatientId,
            result.Specialty.Price);

        appointment.SetStatus(result.Status);
        await _dbContext.Appointments.AddAsync(appointment, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var dto = await BuildListItemDtoAsync(appointment.Id, cancellationToken);
        return dto is null ? Problem("Không thể tải dữ liệu cuộc hẹn vừa tạo") : Ok(dto);
    }

    [HttpPut("{appointmentId:guid}")]
    public async Task<ActionResult<DoctorAppointmentListItemDto>> Update(
        Guid appointmentId,
        [FromBody] AdminAppointmentUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var appointment = await _dbContext.Appointments
            .FirstOrDefaultAsync(a => a.Id == appointmentId, cancellationToken);

        if (appointment is null)
        {
            return NotFound(new ProblemDetails { Title = "Không tìm thấy cuộc hẹn" });
        }

        var (result, problem) = await ValidateUpsertRequestAsync(
            request,
            excludeAppointmentId: appointmentId,
            fallbackClinicRoomId: appointment.ClinicRoomId,
            cancellationToken);

        if (problem is not null)
        {
            return problem.Status == StatusCodes.Status409Conflict
                ? Conflict(problem)
                : BadRequest(problem);
        }

        appointment.AssignDoctor(result!.Doctor.Id);
        appointment.ChangeSpecialty(result.Specialty.Id);
        appointment.AssignClinicRoom(result.ClinicRoomId);
        appointment.UpdatePatientProfile(result.PatientName, result.CustomerPhone);
        appointment.AssignPatient(result.PatientId);
        appointment.Reschedule(result.SlotStartUtc, TimeSpan.FromMinutes(result.DurationMinutes));
        appointment.SetPrice(result.Specialty.Price);
        appointment.SetStatus(result.Status);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var dto = await BuildListItemDtoAsync(appointment.Id, cancellationToken);
        return dto is null ? Problem("Không thể tải dữ liệu cuộc hẹn vừa cập nhật") : Ok(dto);
    }

    [HttpPost("{appointmentId:guid}/status")]
    public async Task<ActionResult<DoctorAppointmentListItemDto>> UpdateStatus(
        Guid appointmentId,
        [FromBody] AdminAppointmentStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (!AppointmentStatus.IsValid(request.Status))
        {
            return BadRequest(new ProblemDetails { Title = "Trạng thái không hợp lệ" });
        }

        var appointment = await _dbContext.Appointments
            .FirstOrDefaultAsync(a => a.Id == appointmentId, cancellationToken);

        if (appointment is null)
        {
            return NotFound(new ProblemDetails { Title = "Không tìm thấy cuộc hẹn" });
        }

        appointment.SetStatus(request.Status);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var dto = await BuildListItemDtoAsync(appointment.Id, cancellationToken);
        return dto is null ? Problem("Không thể tải dữ liệu cuộc hẹn vừa cập nhật") : Ok(dto);
    }

    private async Task<(AdminUpsertValidationResult? Result, ProblemDetails? Problem)> ValidateUpsertRequestAsync(
        AdminAppointmentUpsertRequest request,
        Guid? excludeAppointmentId,
        Guid? fallbackClinicRoomId,
        CancellationToken cancellationToken)
    {
        if (request.DoctorId == Guid.Empty)
        {
            return (null, new ProblemDetails { Title = "Vui lòng chọn bác sĩ" });
        }

        if (request.SpecialtyId == Guid.Empty)
        {
            return (null, new ProblemDetails { Title = "Vui lòng chọn chuyên khoa" });
        }

        var doctor = await _dbContext.Doctors
            .Include(d => d.Specialties)
            .Include(d => d.AppUser)
            .Include(d => d.DaysOff)
            .FirstOrDefaultAsync(d => d.Id == request.DoctorId, cancellationToken);

        if (doctor is null || doctor.AppUser is null)
        {
            return (null, new ProblemDetails { Title = "Không tìm thấy bác sĩ" });
        }

        var specialty = doctor.Specialties.FirstOrDefault(s => s.Id == request.SpecialtyId);
        if (specialty is null)
        {
            return (null, new ProblemDetails { Title = "Bác sĩ không thuộc chuyên khoa được chọn" });
        }

        var slotStartUtc = DateTime.SpecifyKind(request.SlotStartUtc, DateTimeKind.Utc);
        if (slotStartUtc == default)
        {
            return (null, new ProblemDetails { Title = "Vui lòng chọn thời gian khám" });
        }

        var durationMinutes = request.DurationMinutes > 0 ? request.DurationMinutes : 30;
        var minLeadLocal = DateTime.SpecifyKind(DateTime.Now.Date.AddDays(2), DateTimeKind.Local);
        var minLeadUtc = minLeadLocal.ToUniversalTime();
        if (slotStartUtc < minLeadUtc)
        {
            return (null, new ProblemDetails { Title = $"Ngày đặt phải từ {minLeadLocal:dd/MM/yyyy} trở đi" });
        }

        var slotEndUtc = slotStartUtc.AddMinutes(durationMinutes);
        var slotLocalDate = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(slotStartUtc, DisplayTimeZone));
        if (IsDoctorOnDayOff(slotLocalDate, doctor.DaysOff))
        {
            return (null, new ProblemDetails { Title = "Bác sĩ nghỉ trong ngày này" });
        }
        var hasOverlap = await _dbContext.Appointments
            .AsNoTracking()
            .AnyAsync(
                a => a.DoctorId == doctor.Id
                    && (!excludeAppointmentId.HasValue || a.Id != excludeAppointmentId.Value)
                    && slotStartUtc < a.EndUtc && a.StartUtc < slotEndUtc,
                cancellationToken);

        if (hasOverlap)
        {
            return (null, new ProblemDetails
            {
                Title = "Khung giờ đã có cuộc hẹn khác",
                Status = StatusCodes.Status409Conflict
            });
        }

        Guid clinicRoomId;
        if (request.ClinicRoomId.HasValue && request.ClinicRoomId.Value != Guid.Empty)
        {
            clinicRoomId = await ResolveClinicRoomIdAsync(request.ClinicRoomId, cancellationToken);
        }
        else if (fallbackClinicRoomId.HasValue && fallbackClinicRoomId.Value != Guid.Empty)
        {
            clinicRoomId = fallbackClinicRoomId.Value;
        }
        else
        {
            clinicRoomId = await ResolveClinicRoomIdAsync(null, cancellationToken);
        }

        var patientName = request.PatientName?.Trim();
        var customerPhone = request.CustomerPhone?.Trim();
        if (!string.IsNullOrWhiteSpace(request.PatientId))
        {
            var patientUser = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == request.PatientId, cancellationToken);

            if (patientUser is not null)
            {
                if (string.IsNullOrWhiteSpace(patientName))
                {
                    patientName = BuildDisplayName(patientUser.FirstName, patientUser.LastName, patientUser.Email, patientUser.UserName);
                }

                if (string.IsNullOrWhiteSpace(customerPhone))
                {
                    customerPhone = patientUser.PhoneNumber?.Trim();
                }
            }
        }

        if (string.IsNullOrWhiteSpace(patientName))
        {
            return (null, new ProblemDetails { Title = "Vui lòng chọn bệnh nhân" });
        }

        if (string.IsNullOrWhiteSpace(customerPhone))
        {
            return (null, new ProblemDetails { Title = "Vui lòng nhập số điện thoại" });
        }

        var normalizedStatus = AppointmentStatus.NormalizeOrDefault(request.Status);

        return (new AdminUpsertValidationResult(
            doctor,
            doctor.AppUser,
            specialty,
            clinicRoomId,
            slotStartUtc,
            durationMinutes,
            patientName,
            customerPhone,
            string.IsNullOrWhiteSpace(request.PatientId) ? null : request.PatientId,
            normalizedStatus), null);
    }

    private async Task<Guid> ResolveClinicRoomIdAsync(Guid? requestedRoomId, CancellationToken cancellationToken)
    {
        if (requestedRoomId.HasValue && requestedRoomId.Value != Guid.Empty)
        {
            var exists = await _dbContext.ClinicRooms.AsNoTracking().AnyAsync(r => r.Id == requestedRoomId.Value, cancellationToken);
            if (exists)
            {
                return requestedRoomId.Value;
            }
        }

        var fallback = await _dbContext.ClinicRooms.AsNoTracking().Select(r => r.Id).FirstOrDefaultAsync(cancellationToken);
        if (fallback != Guid.Empty)
        {
            return fallback;
        }

        var defaultRoom = new ClinicRoom("CR-ADMIN-01");
        await _dbContext.ClinicRooms.AddAsync(defaultRoom, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return defaultRoom.Id;
    }

    private async Task<DoctorAppointmentListItemDto?> BuildListItemDtoAsync(Guid appointmentId, CancellationToken cancellationToken)
    {
        return await (
            from appointment in _dbContext.Appointments.AsNoTracking()
            where appointment.Id == appointmentId
            join specialty in _dbContext.Specialties.AsNoTracking() on appointment.SpecialtyId equals specialty.Id
            join doctor in _dbContext.Doctors.AsNoTracking() on appointment.DoctorId equals doctor.Id
            join doctorUser in _dbContext.Users.AsNoTracking() on doctor.AppUserId equals doctorUser.Id
            join room in _dbContext.ClinicRooms.AsNoTracking() on appointment.ClinicRoomId equals room.Id into roomGroup
            from room in roomGroup.DefaultIfEmpty()
            select ToListItemDto(appointment, specialty, doctorUser, room))
            .FirstOrDefaultAsync(cancellationToken);
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
            DoctorId = appointment.DoctorId,
            SpecialtyId = appointment.SpecialtyId,
            SpecialtyName = specialty.Name,
            SpecialtyColor = string.IsNullOrWhiteSpace(specialty.Color) ? "#0ea5e9" : specialty.Color,
            PatientName = appointment.PatientName,
            CustomerPhone = appointment.CustomerPhone,
            PatientId = appointment.PatientId,
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
            ClinicRoomId = appointment.ClinicRoomId,
            ClinicRoom = BuildClinicRoomLabel(room),
            Price = appointment.Price
        };
    }

    private static CalendarEventDto ToCalendarEventDto(
        Domain.Aggregates.Appointment.Appointment appointment,
        Specialty specialty,
        AppUser doctorUser,
        ClinicRoom? room)
    {
        var statusCode = AppointmentStatus.NormalizeOrDefault(appointment.Status);
        if (!StatusPresentationMap.TryGetValue(statusCode, out var presentation))
        {
            presentation = StatusPresentationMap[AppointmentStatus.Pending];
        }

        return new CalendarEventDto
        {
            Id = appointment.Id,
            DoctorId = appointment.DoctorId,
            DoctorName = doctorUser.GetFullName(),
            DoctorAvatarUrl = ResolveDoctorAvatar(doctorUser),
            SpecialtyId = specialty.Id,
            SpecialtyName = specialty.Name,
            SpecialtyColor = string.IsNullOrWhiteSpace(specialty.Color) ? "#0ea5e9" : specialty.Color,
            PatientName = appointment.PatientName,
            CustomerPhone = appointment.CustomerPhone,
            Status = presentation.Code,
            StatusLabel = presentation.Label,
            StatusTone = presentation.Tone,
            StatusIcon = presentation.Icon,
            ClinicRoom = BuildClinicRoomLabel(room),
            StartUtc = DateTime.SpecifyKind(appointment.StartUtc, DateTimeKind.Utc),
            EndUtc = DateTime.SpecifyKind(appointment.EndUtc, DateTimeKind.Utc),
            Price = appointment.Price
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

    private static bool IsDoctorOnDayOff(DateOnly targetDate, IEnumerable<DoctorDayOff>? dayOffs)
    {
        if (dayOffs is null)
        {
            return false;
        }

        foreach (var dayOff in dayOffs)
        {
            if (IsDateWithinDayOff(targetDate, dayOff))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsDateWithinDayOff(DateOnly targetDate, DoctorDayOff dayOff)
    {
        if (dayOff.RepeatYearly)
        {
            var start = NormalizeRepeatDate(dayOff.StartDate, targetDate.Year);
            var end = NormalizeRepeatDate(dayOff.EndDate, targetDate.Year);
            if (end < start)
            {
                return targetDate >= start || targetDate <= end;
            }

            return targetDate >= start && targetDate <= end;
        }

        return targetDate >= dayOff.StartDate && targetDate <= dayOff.EndDate;
    }

    private static DateOnly NormalizeRepeatDate(DateOnly source, int year)
    {
        var day = Math.Min(source.Day, DateTime.DaysInMonth(year, source.Month));
        return new DateOnly(year, source.Month, day);
    }

    private sealed record AppointmentStatusPresentation(string Code, string Label, string Tone, string Icon);

    private sealed record AdminUpsertValidationResult(
        Domain.Aggregates.Doctor.Doctor Doctor,
        AppUser DoctorUser,
        Specialty Specialty,
        Guid ClinicRoomId,
        DateTime SlotStartUtc,
        int DurationMinutes,
        string PatientName,
        string CustomerPhone,
        string? PatientId,
        string Status);
}
