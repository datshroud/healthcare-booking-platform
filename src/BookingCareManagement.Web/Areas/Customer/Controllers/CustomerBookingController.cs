using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;
using System.Linq;
using BookingCareManagement.Application.Common.Exceptions;
using BookingCareManagement.Application.Features.Appointments.Commands;
using BookingCareManagement.Application.Features.Appointments.Dtos;
using BookingCareManagement.Application.Features.Notifications.Commands;
using BookingCareManagement.Application.Features.Specialties.Dtos;
using BookingCareManagement.Application.Features.Specialties.Queries;
using BookingCareManagement.Domain.Abstractions;
using BookingCareManagement.Domain.Aggregates.Appointment;
using BookingCareManagement.Domain.Aggregates.ClinicRoom;
using BookingCareManagement.Domain.Aggregates.Doctor;
using BookingCareManagement.Domain.Aggregates.User;
using BookingCareManagement.Infrastructure.Persistence;
using BookingCareManagement.Web.Areas.Customer.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DomainDoctor = BookingCareManagement.Domain.Aggregates.Doctor.Doctor;
using DomainSpecialty = BookingCareManagement.Domain.Aggregates.Doctor.Specialty;

namespace BookingCareManagement.Web.Areas.Customer.Controllers;

[Route("api/customer-booking")]
[ApiController]
public class CustomerBookingController : ControllerBase
{
    private readonly ApplicationDBContext _dbContext;
    private static readonly CultureInfo VietnamCulture = CultureInfo.GetCultureInfo("vi-VN");
    private static readonly TimeZoneInfo VietnamTimeZone = ResolveVietnamTimeZone();
    private static readonly IReadOnlyDictionary<string, CustomerBookingStatusPresentation> StatusPresentationMap =
        new Dictionary<string, CustomerBookingStatusPresentation>(StringComparer.OrdinalIgnoreCase)
        {
            [AppointmentStatus.Pending] = new(AppointmentStatus.Pending, "Chờ xác nhận", "pending", "fa-clock"),
            [AppointmentStatus.Approved] = new(AppointmentStatus.Approved, "Đã xác nhận", "approved", "fa-circle-check"),
            [AppointmentStatus.Canceled] = new(AppointmentStatus.Canceled, "Đã hủy", "canceled", "fa-ban"),
            [AppointmentStatus.Rejected] = new(AppointmentStatus.Rejected, "Bị từ chối", "rejected", "fa-circle-xmark"),
            [AppointmentStatus.NoShow] = new(AppointmentStatus.NoShow, "Vắng mặt", "noshow", "fa-user-xmark")
        };

    public CustomerBookingController(ApplicationDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("specialties")]
    public async Task<ActionResult<IEnumerable<CustomerSpecialtyDto>>> GetSpecialties(
        [FromServices] GetAllSpecialtiesQueryHandler handler,
        CancellationToken cancellationToken)
    {
        var specialties = await handler.Handle(new GetAllSpecialtiesQuery(), cancellationToken);

        var dtos = specialties.Select(ToCustomerSpecialty);
        return Ok(dtos);
    }

    [HttpGet("specialties/{specialtyId:guid}/doctors")]
    public async Task<ActionResult<IEnumerable<CustomerDoctorSummaryDto>>> GetDoctorsBySpecialty(
        Guid specialtyId,
        [FromServices] ISpecialtyRepository specialtyRepository,
        CancellationToken cancellationToken)
    {
        var specialty = await specialtyRepository.GetByIdAsync(specialtyId, cancellationToken);
        if (specialty is null)
        {
            return NotFound(new ProblemDetails { Title = "Không tìm thấy chuyên khoa" });
        }

        var doctors = specialty.Doctors
            .Select(d => new CustomerDoctorSummaryDto(
                d.Id,
                d.AppUser.GetFullName(),
                d.AppUser.AvatarUrl ?? string.Empty))
            .ToArray();

        return Ok(doctors);
    }

    [HttpGet("doctors/{doctorId:guid}/time-slots")]
    public async Task<ActionResult<IEnumerable<DoctorTimeSlotDto>>> GetDoctorSlots(
        Guid doctorId,
        [FromQuery] DateOnly? date,
        [FromQuery] Guid? excludeAppointmentId,
        [FromServices] IDoctorRepository doctorRepository,
        CancellationToken cancellationToken)
    {
        var minLeadDate = DateOnly.FromDateTime(DateTime.Now.AddDays(2));
        var targetDate = date ?? minLeadDate;
        if (targetDate < minLeadDate)
        {
            return BadRequest(new ProblemDetails { Title = "Ngày đặt phải cách hiện tại ít nhất 2 ngày" });
        }

        var doctor = await doctorRepository.GetByIdAsync(doctorId, cancellationToken);
        if (doctor is null)
        {
            return NotFound(new ProblemDetails { Title = "Không tìm thấy bác sĩ" });
        }

        if (IsDoctorOnDayOff(targetDate, doctor.DaysOff))
        {
            return Ok(Array.Empty<DoctorTimeSlotDto>());
        }

        var windows = doctor.WorkingHours
            .Where(h => h.DayOfWeek == targetDate.DayOfWeek)
            .OrderBy(h => h.StartTime)
            .ToArray();

        if (windows.Length == 0)
        {
            return Ok(Array.Empty<DoctorTimeSlotDto>());
        }

        var dayStartLocal = SpecifyAsLocalVietnam(targetDate.ToDateTime(TimeOnly.MinValue));
        var dayEndLocal = dayStartLocal.AddDays(1);
        var dayStartUtc = TimeZoneInfo.ConvertTimeToUtc(dayStartLocal, VietnamTimeZone);
        var dayEndUtc = TimeZoneInfo.ConvertTimeToUtc(dayEndLocal, VietnamTimeZone);

        var targetAppointmentId = excludeAppointmentId ?? Guid.Empty;

        var takenEntries = await _dbContext.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorId == doctorId
                && (targetAppointmentId == Guid.Empty || a.Id != targetAppointmentId)
                && a.StartUtc >= dayStartUtc && a.StartUtc < dayEndUtc)
            .Select(a => new
            {
                StartUtc = DateTime.SpecifyKind(a.StartUtc, DateTimeKind.Utc),
                EndUtc = DateTime.SpecifyKind(a.EndUtc, DateTimeKind.Utc)
            })
            .ToArrayAsync(cancellationToken);

        var taken = takenEntries
            .Select(a => (a.StartUtc, a.EndUtc))
            .ToArray();

        var slots = BuildSlots(targetDate, windows, taken);
        return Ok(slots);
    }

    [HttpGet("my-bookings")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyCollection<CustomerBookingSummaryDto>>> GetMyBookings(
        [FromQuery] string? filter,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var normalizedFilter = NormalizeFilter(filter);
        var nowUtc = DateTime.UtcNow;

        var query =
            from appointment in _dbContext.Appointments.AsNoTracking()
            where appointment.PatientId == userId
            join doctor in _dbContext.Doctors.AsNoTracking() on appointment.DoctorId equals doctor.Id
            join doctorUser in _dbContext.Users.AsNoTracking() on doctor.AppUserId equals doctorUser.Id
            join specialty in _dbContext.Specialties.AsNoTracking() on appointment.SpecialtyId equals specialty.Id
            join room in _dbContext.ClinicRooms.AsNoTracking() on appointment.ClinicRoomId equals room.Id into roomGroup
            from room in roomGroup.DefaultIfEmpty()
            select new
            {
                Appointment = appointment,
                DoctorUser = doctorUser,
                Specialty = specialty,
                ClinicRoom = room
            };

        query = normalizedFilter switch
        {
            "past" => query.Where(x => x.Appointment.StartUtc < nowUtc),
            "upcoming" => query.Where(x => x.Appointment.StartUtc >= nowUtc),
            _ => query
        };

        var records = await query
            .OrderByDescending(x => x.Appointment.StartUtc)
            .ToListAsync(cancellationToken);

        var dtos = records
            .Select(x => ToCustomerBookingSummaryDto(x.Appointment, x.Specialty, x.DoctorUser, x.ClinicRoom))
            .ToArray();

        return Ok(dtos);
    }

    [HttpPost]
    public async Task<ActionResult<AppointmentDto>> CreateBooking(
        [FromBody] CreateCustomerBookingRequest request,
        [FromServices] CreateAppointmentCommandHandler handler,
        [FromServices] IDoctorRepository doctorRepository,
        [FromServices] ISpecialtyRepository specialtyRepository,
        [FromServices] CreateAdminNotificationCommandHandler notificationHandler,
        CancellationToken cancellationToken)
    {
        if (request.SpecialtyId == Guid.Empty)
        {
            return BadRequest(new ProblemDetails { Title = "Chuyên khoa bắt buộc" });
        }

        if (request.DoctorId == Guid.Empty)
        {
            return BadRequest(new ProblemDetails { Title = "Bác sĩ bắt buộc" });
        }

        if (request.SlotStartUtc == default)
        {
            return BadRequest(new ProblemDetails { Title = "Chưa chọn thời gian" });
        }

        var trimmedName = request.CustomerName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            return BadRequest(new ProblemDetails { Title = "Vui lòng nhập họ tên" });
        }

        var trimmedPhone = request.CustomerPhone?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmedPhone))
        {
            return BadRequest(new ProblemDetails { Title = "Vui lòng nhập số điện thoại" });
        }

        var slotStartUtc = DateTime.SpecifyKind(request.SlotStartUtc, DateTimeKind.Utc);
        var durationMinutes = request.DurationMinutes <= 0 ? 30 : request.DurationMinutes;
        var slotEndUtc = slotStartUtc.AddMinutes(durationMinutes);

        var minLeadLocal = GetVietnamDate().AddDays(2);
        var minLeadUtc = TimeZoneInfo.ConvertTimeToUtc(minLeadLocal, VietnamTimeZone);
        if (slotStartUtc < minLeadUtc)
        {
            var limitMessage = $"Ngày đặt phải từ {minLeadLocal.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)} trở đi";
            return BadRequest(new ProblemDetails { Title = limitMessage });
        }

        var specialty = await specialtyRepository.GetByIdAsync(request.SpecialtyId, cancellationToken);
        if (specialty is null)
        {
            return NotFound(new ProblemDetails { Title = "Không tìm thấy chuyên khoa" });
        }

        var doctor = await doctorRepository.GetByIdAsync(request.DoctorId, cancellationToken);
        if (doctor is null)
        {
            return NotFound(new ProblemDetails { Title = "Không tìm thấy bác sĩ" });
        }

        if (!doctor.Specialties.Any(s => s.Id == request.SpecialtyId))
        {
            return BadRequest(new ProblemDetails { Title = "Bác sĩ không thuộc chuyên khoa đã chọn" });
        }

        var slotLocalDate = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(slotStartUtc, VietnamTimeZone));
        if (IsDoctorOnDayOff(slotLocalDate, doctor.DaysOff))
        {
            return BadRequest(new ProblemDetails { Title = "Bác sĩ nghỉ trong ngày này, vui lòng chọn ngày khác" });
        }

        var clinicRoomId = await _dbContext.ClinicRooms
            .AsNoTracking()
            .Select(r => r.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (clinicRoomId == Guid.Empty)
        {
            var fallbackRoom = new ClinicRoom("CR-001");
            await _dbContext.ClinicRooms.AddAsync(fallbackRoom, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            clinicRoomId = fallbackRoom.Id;
        }

        var slotTaken = await _dbContext.Appointments
            .AsNoTracking()
            .AnyAsync(
                a => a.DoctorId == doctor.Id && slotStartUtc < a.EndUtc && a.StartUtc < slotEndUtc,
                cancellationToken);

        if (slotTaken)
        {
            return Conflict(new ProblemDetails { Title = "Khung giờ này đã được đặt" });
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var command = new CreateAppointmentCommand
        {
            DoctorId = request.DoctorId,
            SpecialtyId = request.SpecialtyId,
            ClinicRoomId = clinicRoomId,
            StartUtc = slotStartUtc,
            DurationMinutes = durationMinutes,
            PatientName = trimmedName,
            CustomerPhone = trimmedPhone,
            PatientId = string.IsNullOrWhiteSpace(currentUserId) ? null : currentUserId,
            Price = specialty.Price
        };

        var dto = await handler.Handle(command, cancellationToken);

        if (!string.IsNullOrWhiteSpace(currentUserId))
        {
            var currentUser = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == currentUserId, cancellationToken);

            if (currentUser is not null)
            {
                var updated = false;

                if (!string.Equals(currentUser.FullName, trimmedName, StringComparison.Ordinal))
                {
                    currentUser.FullName = trimmedName;
                    updated = true;
                }

                if (!string.Equals(currentUser.PhoneNumber, trimmedPhone, StringComparison.Ordinal))
                {
                    currentUser.PhoneNumber = trimmedPhone;
                    updated = true;
                }

                if (updated)
                {
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
            }
        }

        var doctorDisplayName = ResolveDoctorDisplayName(doctor);
        var specialtyName = ResolveSpecialtyDisplayName(specialty);
        var startLocal = TimeZoneInfo.ConvertTimeFromUtc(slotStartUtc, VietnamTimeZone);

        await TryCreateAdminNotificationAsync(
            notificationHandler,
            "Đặt lịch mới",
            $"{trimmedName} đã đặt {specialtyName} với {doctorDisplayName} ({startLocal:HH:mm dd/MM}).",
            dto.Id,
            cancellationToken);

        return Ok(dto);
    }

    [HttpPost("{appointmentId:guid}/reschedule")]
    [Authorize]
    public async Task<ActionResult<CustomerBookingSummaryDto>> RescheduleBooking(
        Guid appointmentId,
        [FromBody] RescheduleCustomerBookingRequest request,
        [FromServices] CreateAdminNotificationCommandHandler notificationHandler,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        if (request is null || request.SlotStartUtc == default)
        {
            return BadRequest(new ProblemDetails { Title = "Vui lòng chọn thời gian mới" });
        }

        var appointment = await _dbContext.Appointments
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.PatientId == userId, cancellationToken);

        if (appointment is null)
        {
            return NotFound(new ProblemDetails { Title = "Không tìm thấy lịch hẹn" });
        }

        if (appointment.StartUtc <= DateTime.UtcNow)
        {
            return BadRequest(new ProblemDetails { Title = "Không thể đổi lịch cho cuộc hẹn đã diễn ra" });
        }

        var durationMinutes = request.DurationMinutes <= 0 ? 30 : request.DurationMinutes;
        var newStartUtc = DateTime.SpecifyKind(request.SlotStartUtc, DateTimeKind.Utc);
        var newEndUtc = newStartUtc.AddMinutes(durationMinutes);

        var minLeadLocal = GetVietnamDate().AddDays(2);
        var minLeadUtc = TimeZoneInfo.ConvertTimeToUtc(minLeadLocal, VietnamTimeZone);
        if (newStartUtc < minLeadUtc)
        {
            var limitMessage = $"Ngày đổi lịch phải từ {minLeadLocal.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)} trở đi";
            return BadRequest(new ProblemDetails { Title = limitMessage });
        }

        var rescheduleLocalDate = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(newStartUtc, VietnamTimeZone));
        var dayOffs = await _dbContext.DoctorDayOffs
            .AsNoTracking()
            .Where(x => x.DoctorId == appointment.DoctorId)
            .ToListAsync(cancellationToken);

        if (IsDoctorOnDayOff(rescheduleLocalDate, dayOffs))
        {
            return BadRequest(new ProblemDetails { Title = "Bác sĩ nghỉ trong ngày này, vui lòng chọn thời gian khác" });
        }

        var slotTaken = await _dbContext.Appointments
            .AsNoTracking()
            .AnyAsync(
                a => a.DoctorId == appointment.DoctorId && a.Id != appointment.Id && newStartUtc < a.EndUtc && a.StartUtc < newEndUtc,
                cancellationToken);

        if (slotTaken)
        {
            return Conflict(new ProblemDetails { Title = "Khung giờ này đã được đặt" });
        }

        var oldStartUtc = appointment.StartUtc;

        appointment.Reschedule(newStartUtc, TimeSpan.FromMinutes(durationMinutes));
        appointment.ResetToPending();

        await _dbContext.SaveChangesAsync(cancellationToken);

        var doctorName = await ResolveDoctorDisplayNameAsync(appointment.DoctorId, cancellationToken);
        var specialtyName = await ResolveSpecialtyDisplayNameAsync(appointment.SpecialtyId, cancellationToken);
        var oldStartLocal = TimeZoneInfo.ConvertTimeFromUtc(oldStartUtc, VietnamTimeZone);
        var newStartLocal = TimeZoneInfo.ConvertTimeFromUtc(newStartUtc, VietnamTimeZone);

        await TryCreateAdminNotificationAsync(
            notificationHandler,
            "Đổi lịch hẹn",
            $"{appointment.PatientName} đổi lịch {specialtyName} với {doctorName} từ {oldStartLocal:HH:mm dd/MM} sang {newStartLocal:HH:mm dd/MM}.",
            appointment.Id,
            cancellationToken);

        var dto = await BuildCustomerBookingSummary(appointment.Id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost("{appointmentId:guid}/cancel")]
    [Authorize]
    public async Task<ActionResult<CustomerBookingSummaryDto>> CancelBooking(
        Guid appointmentId,
        [FromServices] CreateAdminNotificationCommandHandler notificationHandler,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var appointment = await _dbContext.Appointments
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.PatientId == userId, cancellationToken);

        if (appointment is null)
        {
            return NotFound(new ProblemDetails { Title = "Không tìm thấy lịch hẹn" });
        }

        if (appointment.StartUtc <= DateTime.UtcNow)
        {
            return BadRequest(new ProblemDetails { Title = "Không thể hủy lịch đã diễn ra" });
        }

        var normalizedStatus = AppointmentStatus.NormalizeOrDefault(appointment.Status);
        if (normalizedStatus == AppointmentStatus.Canceled)
        {
            return BadRequest(new ProblemDetails { Title = "Lịch hẹn đã được hủy trước đó" });
        }

        appointment.Cancel();
        await _dbContext.SaveChangesAsync(cancellationToken);

        var cancelDoctorName = await ResolveDoctorDisplayNameAsync(appointment.DoctorId, cancellationToken);
        var cancelSpecialtyName = await ResolveSpecialtyDisplayNameAsync(appointment.SpecialtyId, cancellationToken);
        var cancelStartLocal = TimeZoneInfo.ConvertTimeFromUtc(appointment.StartUtc, VietnamTimeZone);

        await TryCreateAdminNotificationAsync(
            notificationHandler,
            "Hủy lịch hẹn",
            $"{appointment.PatientName} đã hủy lịch {cancelSpecialtyName} với {cancelDoctorName} ({cancelStartLocal:HH:mm dd/MM}).",
            appointment.Id,
            cancellationToken);

        var dto = await BuildCustomerBookingSummary(appointment.Id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<CustomerProfileDto>> GetProfile(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            return NotFound(new ProblemDetails { Title = "Không tìm thấy thông tin người dùng" });
        }

        var dto = new CustomerProfileDto
        {
            FullName = user.FullName ?? string.Empty,
            PhoneNumber = user.PhoneNumber ?? string.Empty
        };

        return Ok(dto);
    }

    private async Task<CustomerBookingSummaryDto?> BuildCustomerBookingSummary(
        Guid appointmentId,
        CancellationToken cancellationToken)
    {
        var record = await (
            from appointment in _dbContext.Appointments.AsNoTracking()
            where appointment.Id == appointmentId
            join doctor in _dbContext.Doctors.AsNoTracking() on appointment.DoctorId equals doctor.Id
            join doctorUser in _dbContext.Users.AsNoTracking() on doctor.AppUserId equals doctorUser.Id
            join specialty in _dbContext.Specialties.AsNoTracking() on appointment.SpecialtyId equals specialty.Id
            join room in _dbContext.ClinicRooms.AsNoTracking() on appointment.ClinicRoomId equals room.Id into roomGroup
            from room in roomGroup.DefaultIfEmpty()
            select new
            {
                Appointment = appointment,
                DoctorUser = doctorUser,
                Specialty = specialty,
                ClinicRoom = room
            }).FirstOrDefaultAsync(cancellationToken);

        if (record is null)
        {
            return null;
        }

        return ToCustomerBookingSummaryDto(record.Appointment, record.Specialty, record.DoctorUser, record.ClinicRoom);
    }

    private static string ResolveDoctorDisplayName(DomainDoctor doctor)
    {
        if (doctor is null)
        {
            return "Bác sĩ";
        }

        var candidate = doctor.AppUser?.GetFullName();
        if (!string.IsNullOrWhiteSpace(candidate))
        {
            return candidate.Trim();
        }

        return NormalizeLabel(doctor.AppUser?.Email, "Bác sĩ");
    }

    private async Task<string> ResolveDoctorDisplayNameAsync(Guid doctorId, CancellationToken cancellationToken)
    {
        var label = await (
            from doctor in _dbContext.Doctors.AsNoTracking()
            where doctor.Id == doctorId
            join user in _dbContext.Users.AsNoTracking() on doctor.AppUserId equals user.Id
            select user.FullName ?? user.Email
        ).FirstOrDefaultAsync(cancellationToken);

        return NormalizeLabel(label, "Bác sĩ");
    }

    private static string ResolveSpecialtyDisplayName(DomainSpecialty specialty)
    {
        return NormalizeLabel(specialty?.Name, "Chuyên khoa");
    }

    private async Task<string> ResolveSpecialtyDisplayNameAsync(Guid specialtyId, CancellationToken cancellationToken)
    {
        var name = await _dbContext.Specialties
            .AsNoTracking()
            .Where(s => s.Id == specialtyId)
            .Select(s => s.Name)
            .FirstOrDefaultAsync(cancellationToken);

        return NormalizeLabel(name, "Chuyên khoa");
    }

    private static string NormalizeLabel(string? candidate, string fallback)
    {
        return string.IsNullOrWhiteSpace(candidate) ? fallback : candidate.Trim();
    }

    private static async Task TryCreateAdminNotificationAsync(
        CreateAdminNotificationCommandHandler notificationHandler,
        string title,
        string message,
        Guid appointmentId,
        CancellationToken cancellationToken)
    {
        if (notificationHandler is null)
        {
            return;
        }

        try
        {
            await notificationHandler.Handle(new CreateAdminNotificationCommand
            {
                Title = title,
                Message = message,
                Category = "booking",
                AppointmentId = appointmentId
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to create admin notification: {ex.Message}");
        }
    }

    private static CustomerBookingSummaryDto ToCustomerBookingSummaryDto(
        Domain.Aggregates.Appointment.Appointment appointment,
        Specialty specialty,
        AppUser doctorUser,
        ClinicRoom? clinicRoom)
    {
        var startUtc = DateTime.SpecifyKind(appointment.StartUtc, DateTimeKind.Utc);
        var endUtc = DateTime.SpecifyKind(appointment.EndUtc, DateTimeKind.Utc);
        var startLocal = TimeZoneInfo.ConvertTimeFromUtc(startUtc, VietnamTimeZone);
        var endLocal = TimeZoneInfo.ConvertTimeFromUtc(endUtc, VietnamTimeZone);

        var statusCode = AppointmentStatus.NormalizeOrDefault(appointment.Status);
        if (!StatusPresentationMap.TryGetValue(statusCode, out var presentation))
        {
            presentation = StatusPresentationMap[AppointmentStatus.Pending];
        }

        return new CustomerBookingSummaryDto
        {
            Id = appointment.Id,
            DoctorId = appointment.DoctorId,
            SpecialtyId = appointment.SpecialtyId,
            DoctorName = doctorUser.GetFullName(),
            DoctorAvatarUrl = doctorUser.AvatarUrl ?? string.Empty,
            SpecialtyName = specialty.Name,
            SpecialtyColor = specialty.Color,
            ClinicRoom = BuildClinicRoomLabel(clinicRoom),
            DateText = BuildDateLabel(startLocal),
            TimeText = $"{startLocal:HH:mm} - {endLocal:HH:mm}",
            Status = presentation.Code,
            StatusLabel = presentation.Label,
            StatusTone = presentation.Tone,
            StatusIcon = presentation.Icon,
            PatientName = appointment.PatientName,
            DurationMinutes = (int)Math.Round((appointment.EndUtc - appointment.StartUtc).TotalMinutes),
            StartUtc = startUtc,
            EndUtc = endUtc,
            Price = appointment.Price
        };
    }

    private static string BuildClinicRoomLabel(ClinicRoom? room)
    {
        if (room is null)
        {
            return "Tư vấn trực tuyến";
        }

        return string.IsNullOrWhiteSpace(room.Code) ? "Phòng khám" : $"Phòng khám {room.Code}";
    }

    private static string BuildDateLabel(DateTime startLocal)
    {
        var dayName = CapitalizeFirst(VietnamCulture.DateTimeFormat.GetDayName(startLocal.DayOfWeek));
        return $"{dayName}, {startLocal:dd/MM/yyyy}";
    }

    private static string NormalizeFilter(string? filter)
    {
        return filter?.Trim().ToLowerInvariant() switch
        {
            "past" => "past",
            "upcoming" => "upcoming",
            _ => "all"
        };
    }

    private static string CapitalizeFirst(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        if (value.Length == 1)
        {
            return value.ToUpper(VietnamCulture);
        }

        return char.ToUpper(value[0], VietnamCulture) + value[1..];
    }

    private static DateTime GetVietnamDate()
    {
        var todayLocal = TimeZoneInfo.ConvertTime(DateTime.UtcNow, VietnamTimeZone).Date;
        return DateTime.SpecifyKind(todayLocal, DateTimeKind.Unspecified);
    }

    private static DateTime SpecifyAsLocalVietnam(DateTime dateTime)
    {
        return DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
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

    private static CustomerSpecialtyDto ToCustomerSpecialty(SpecialtyDto specialty)
    {
        var doctors = specialty.Doctors
            .Select(d => new CustomerDoctorSummaryDto(d.Id, d.FullName, d.AvatarUrl))
            .ToArray();

        return new CustomerSpecialtyDto
        {
            Id = specialty.Id,
            Name = specialty.Name,
            Description = specialty.Description,
            Color = specialty.Color,
            ImageUrl = specialty.ImageUrl,
            Price = specialty.Price,
            DurationMinutes = null,
            Doctors = doctors
        };
    }

    private static IReadOnlyCollection<DoctorTimeSlotDto> BuildSlots(
        DateOnly date,
        IEnumerable<DoctorWorkingHour> windows,
        IEnumerable<(DateTime Start, DateTime End)> taken)
    {
        var slots = new List<DoctorTimeSlotDto>();
        var slotLength = TimeSpan.FromMinutes(30);
        var dateStartLocal = SpecifyAsLocalVietnam(date.ToDateTime(TimeOnly.MinValue));

        foreach (var window in windows)
        {
            var current = window.StartTime;
            while (current + slotLength <= window.EndTime)
            {
                var startLocal = dateStartLocal.Add(current);
                var endLocal = startLocal.Add(slotLength);

                var startUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal, VietnamTimeZone);
                var endUtc = TimeZoneInfo.ConvertTimeToUtc(endLocal, VietnamTimeZone);

                var overlaps = taken.Any(existing =>
                    startUtc < existing.End && existing.Start < endUtc);

                if (!overlaps)
                {
                    slots.Add(new DoctorTimeSlotDto
                    {
                        StartLocal = startLocal,
                        EndLocal = endLocal,
                        StartUtc = startUtc,
                        EndUtc = endUtc,
                        IsAvailable = true
                    });
                }

                current += slotLength;
            }
        }

        return slots;
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

    private sealed record CustomerBookingStatusPresentation(string Code, string Label, string Tone, string Icon);
}
