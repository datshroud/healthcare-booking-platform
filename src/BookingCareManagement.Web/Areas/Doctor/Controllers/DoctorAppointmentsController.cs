using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using BookingCareManagement.Domain.Aggregates.Appointment;
using BookingCareManagement.Domain.Aggregates.ClinicRoom;
using BookingCareManagement.Domain.Aggregates.Doctor;
using BookingCareManagement.Domain.Aggregates.User;
using BookingCareManagement.Infrastructure.Persistence;
using BookingCareManagement.Web.Areas.Doctor.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using DomainDoctor = BookingCareManagement.Domain.Aggregates.Doctor.Doctor;

namespace BookingCareManagement.Web.Areas.Doctor.Controllers;

[Authorize(Policy = "DoctorOrAbove")]
[Route("api/doctor/appointments")]
[ApiController]
public class DoctorAppointmentsController : ControllerBase
{
    private readonly ApplicationDBContext _dbContext;
    private static readonly CultureInfo VietnamCulture = CultureInfo.GetCultureInfo("vi-VN");
    private static readonly TimeZoneInfo VietnamTimeZone = ResolveVietnamTimeZone();
    private static readonly IReadOnlyDictionary<string, AppointmentStatusPresentation> StatusPresentationMap =
        new Dictionary<string, AppointmentStatusPresentation>(StringComparer.OrdinalIgnoreCase)
        {
            [AppointmentStatus.Pending] = new(AppointmentStatus.Pending, "Chờ xác nhận", "pending", "fa-clock"),
            [AppointmentStatus.Approved] = new(AppointmentStatus.Approved, "Đã xác nhận", "approved", "fa-circle-check"),
            [AppointmentStatus.Canceled] = new(AppointmentStatus.Canceled, "Đã hủy", "canceled", "fa-ban"),
            [AppointmentStatus.Rejected] = new(AppointmentStatus.Rejected, "Bị từ chối", "rejected", "fa-circle-xmark"),
            [AppointmentStatus.NoShow] = new(AppointmentStatus.NoShow, "Vắng mặt", "noshow", "fa-user-xmark")
        };

    public DoctorAppointmentsController(ApplicationDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("metadata")]
    public async Task<ActionResult<DoctorAppointmentMetadataDto>> GetMetadata(CancellationToken cancellationToken)
    {
        var doctor = await FindDoctorForCurrentUserAsync(includeSpecialties: true, includeUser: true, cancellationToken);
        if (doctor is null || doctor.AppUser is null)
        {
            return Forbid();
        }

        var dto = new DoctorAppointmentMetadataDto
        {
            DoctorId = doctor.Id,
            DoctorName = doctor.AppUser.GetFullName(),
            Specialties = doctor.Specialties
                .OrderBy(s => s.Name)
                .Select(s => new DoctorAppointmentSpecialtyOptionDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Color = string.IsNullOrWhiteSpace(s.Color) ? "#0ea5e9" : s.Color
                })
                .ToArray(),
            Statuses = StatusPresentationMap
                .Values
                .Select(x => new DoctorAppointmentStatusOptionDto
                {
                    Code = x.Code,
                    Label = x.Label,
                    Tone = x.Tone,
                    Icon = x.Icon
                })
                .ToArray()
        };

        return Ok(dto);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<DoctorAppointmentListItemDto>>> GetAppointments(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken cancellationToken)
    {
        var doctor = await FindDoctorForCurrentUserAsync(includeUser: true, cancellationToken: cancellationToken);
        if (doctor is null || doctor.AppUser is null)
        {
            return Forbid();
        }

        var query =
            from appointment in _dbContext.Appointments.AsNoTracking()
            where appointment.DoctorId == doctor.Id
            join specialty in _dbContext.Specialties.AsNoTracking() on appointment.SpecialtyId equals specialty.Id
            join room in _dbContext.ClinicRooms.AsNoTracking() on appointment.ClinicRoomId equals room.Id into roomGroup
            from room in roomGroup.DefaultIfEmpty()
            select new { appointment, specialty, room };

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
            .Select(x => ToDoctorAppointmentDto(x.appointment, x.specialty, doctor.AppUser!, x.room))
            .ToArray();

        return Ok(dtos);
    }

    [HttpPost]
    public async Task<ActionResult<DoctorAppointmentListItemDto>> Create(
        [FromBody] DoctorAppointmentUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var doctor = await FindDoctorForCurrentUserAsync(includeSpecialties: true, includeUser: true, cancellationToken);
        if (doctor is null || doctor.AppUser is null)
        {
            return Forbid();
        }

        var (validation, problem) = await ValidateUpsertRequestAsync(request, doctor, excludeAppointmentId: null, cancellationToken);
        if (problem is not null)
        {
            return problem.Status == StatusCodes.Status409Conflict ? Conflict(problem) : BadRequest(problem);
        }

        var clinicRoomId = request.ClinicRoomId ?? await EnsureClinicRoomAsync(cancellationToken);
        var appointment = new Appointment(
            doctor.Id,
            request.SpecialtyId,
            clinicRoomId,
            validation!.StartUtc,
            TimeSpan.FromMinutes(validation.DurationMinutes),
            request.PatientName.Trim(),
            request.CustomerPhone.Trim(),
            patientId: null);

        var statusCode = string.IsNullOrWhiteSpace(request.Status)
            ? AppointmentStatus.Pending
            : request.Status;
        appointment.SetStatus(statusCode);
        _dbContext.Appointments.Add(appointment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var dto = await BuildDoctorAppointmentDtoAsync(appointment.Id, doctor, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPut("{appointmentId:guid}")]
    public async Task<ActionResult<DoctorAppointmentListItemDto>> Update(
        Guid appointmentId,
        [FromBody] DoctorAppointmentUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var doctor = await FindDoctorForCurrentUserAsync(includeSpecialties: true, includeUser: true, cancellationToken);
        if (doctor is null || doctor.AppUser is null)
        {
            return Forbid();
        }

        var appointment = await _dbContext.Appointments
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.DoctorId == doctor.Id, cancellationToken);

        if (appointment is null)
        {
            return NotFound(new ProblemDetails { Title = "Không tìm thấy cuộc hẹn" });
        }

        var (validation, problem) = await ValidateUpsertRequestAsync(request, doctor, appointmentId, cancellationToken);
        if (problem is not null)
        {
            return problem.Status == StatusCodes.Status409Conflict ? Conflict(problem) : BadRequest(problem);
        }

        appointment.ChangeSpecialty(request.SpecialtyId);
        appointment.AssignClinicRoom(request.ClinicRoomId ?? appointment.ClinicRoomId);
        appointment.UpdatePatientProfile(request.PatientName, request.CustomerPhone);
        appointment.Reschedule(validation!.StartUtc, TimeSpan.FromMinutes(validation.DurationMinutes));
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            appointment.SetStatus(request.Status);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var dto = await BuildDoctorAppointmentDtoAsync(appointment.Id, doctor, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost("{appointmentId:guid}/status")]
    public async Task<ActionResult<DoctorAppointmentListItemDto>> UpdateStatus(
        Guid appointmentId,
        [FromBody] DoctorAppointmentStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (!AppointmentStatus.IsValid(request.Status))
        {
            return BadRequest(new ProblemDetails { Title = "Trạng thái không hợp lệ" });
        }

        var doctor = await FindDoctorForCurrentUserAsync(includeUser: true, cancellationToken: cancellationToken);
        if (doctor is null || doctor.AppUser is null)
        {
            return Forbid();
        }

        var appointment = await _dbContext.Appointments
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.DoctorId == doctor.Id, cancellationToken);

        if (appointment is null)
        {
            return NotFound(new ProblemDetails { Title = "Không tìm thấy cuộc hẹn" });
        }

        appointment.SetStatus(request.Status);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var dto = await BuildDoctorAppointmentDtoAsync(appointment.Id, doctor, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpDelete("{appointmentId:guid}")]
    public async Task<IActionResult> Delete(Guid appointmentId, CancellationToken cancellationToken)
    {
        var doctor = await FindDoctorForCurrentUserAsync(cancellationToken: cancellationToken);
        if (doctor is null)
        {
            return Forbid();
        }

        var appointment = await _dbContext.Appointments
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.DoctorId == doctor.Id, cancellationToken);

        if (appointment is null)
        {
            return NotFound(new ProblemDetails { Title = "Không tìm thấy cuộc hẹn" });
        }

        _dbContext.Appointments.Remove(appointment);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private async Task<(UpsertValidationResult? Result, ProblemDetails? Problem)> ValidateUpsertRequestAsync(
        DoctorAppointmentUpsertRequest request,
        DomainDoctor doctor,
        Guid? excludeAppointmentId,
        CancellationToken cancellationToken)
    {
        if (request.SpecialtyId == Guid.Empty)
        {
            return (null, new ProblemDetails { Title = "Vui lòng chọn chuyên khoa" });
        }

        if (!doctor.Specialties.Any(s => s.Id == request.SpecialtyId))
        {
            return (null, new ProblemDetails { Title = "Chuyên khoa không thuộc bác sĩ" });
        }

        if (string.IsNullOrWhiteSpace(request.PatientName))
        {
            return (null, new ProblemDetails { Title = "Vui lòng nhập tên bệnh nhân" });
        }

        if (string.IsNullOrWhiteSpace(request.CustomerPhone))
        {
            return (null, new ProblemDetails { Title = "Vui lòng nhập số điện thoại" });
        }

        if (request.SlotStartUtc == default)
        {
            return (null, new ProblemDetails { Title = "Vui lòng chọn thời gian" });
        }

        var durationMinutes = request.DurationMinutes <= 0 ? 30 : request.DurationMinutes;
        var slotStartUtc = DateTime.SpecifyKind(request.SlotStartUtc, DateTimeKind.Utc);
        var slotEndUtc = slotStartUtc.AddMinutes(durationMinutes);

        var minLeadLocal = DateTime.SpecifyKind(DateTime.Now.Date.AddDays(2), DateTimeKind.Local);
        var minLeadUtc = minLeadLocal.ToUniversalTime();
        if (slotStartUtc < minLeadUtc)
        {
            var limitMessage = $"Ngày đặt phải từ {minLeadLocal:dd/MM/yyyy} trở đi";
            return (null, new ProblemDetails { Title = limitMessage });
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

        return (new UpsertValidationResult(slotStartUtc, durationMinutes), null);
    }

    private async Task<DomainDoctor?> FindDoctorForCurrentUserAsync(
        bool includeSpecialties = false,
        bool includeUser = false,
        CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        IQueryable<DomainDoctor> query = _dbContext.Doctors;
        if (includeSpecialties)
        {
            query = query.Include(d => d.Specialties);
        }

        if (includeUser)
        {
            query = query.Include(d => d.AppUser);
        }

        return await query.FirstOrDefaultAsync(d => d.AppUserId == userId, cancellationToken);
    }

    private async Task<Guid> EnsureClinicRoomAsync(CancellationToken cancellationToken)
    {
        var existing = await _dbContext.ClinicRooms
            .AsNoTracking()
            .Select(r => r.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing != Guid.Empty)
        {
            return existing;
        }

        var fallbackRoom = new ClinicRoom("CR-001");
        await _dbContext.ClinicRooms.AddAsync(fallbackRoom, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return fallbackRoom.Id;
    }

    private async Task<DoctorAppointmentListItemDto?> BuildDoctorAppointmentDtoAsync(
        Guid appointmentId,
        DomainDoctor doctor,
        CancellationToken cancellationToken)
    {
        var record = await (
            from appointment in _dbContext.Appointments.AsNoTracking()
            where appointment.Id == appointmentId && appointment.DoctorId == doctor.Id
            join specialty in _dbContext.Specialties.AsNoTracking() on appointment.SpecialtyId equals specialty.Id
            join room in _dbContext.ClinicRooms.AsNoTracking() on appointment.ClinicRoomId equals room.Id into roomGroup
            from room in roomGroup.DefaultIfEmpty()
            select new { appointment, specialty, room })
            .FirstOrDefaultAsync(cancellationToken);

        if (record is null || doctor.AppUser is null)
        {
            return null;
        }

        return ToDoctorAppointmentDto(record.appointment, record.specialty, doctor.AppUser, record.room);
    }

    private DoctorAppointmentListItemDto ToDoctorAppointmentDto(
        Domain.Aggregates.Appointment.Appointment appointment,
        Specialty specialty,
        AppUser doctorUser,
        ClinicRoom? room)
    {
        var startUtc = DateTime.SpecifyKind(appointment.StartUtc, DateTimeKind.Utc);
        var endUtc = DateTime.SpecifyKind(appointment.EndUtc, DateTimeKind.Utc);
        var startLocal = TimeZoneInfo.ConvertTimeFromUtc(startUtc, VietnamTimeZone);
        var endLocal = TimeZoneInfo.ConvertTimeFromUtc(endUtc, VietnamTimeZone);

        var dateLabel = $"{CapitalizeFirst(VietnamCulture.DateTimeFormat.GetDayName(startLocal.DayOfWeek))}, {startLocal:dd/MM/yyyy}";
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
            return "BS";
        }

        var parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 1)
        {
            return parts[0][0].ToString().ToUpper(VietnamCulture);
        }

        var first = parts[0][0];
        var last = parts[^1][0];
        return string.Concat(char.ToUpper(first, VietnamCulture), char.ToUpper(last, VietnamCulture));
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

    private static string CapitalizeFirst(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value.Length == 1
            ? value.ToUpper(VietnamCulture)
            : char.ToUpper(value[0], VietnamCulture) + value[1..];
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

    private sealed record UpsertValidationResult(DateTime StartUtc, int DurationMinutes);
}
