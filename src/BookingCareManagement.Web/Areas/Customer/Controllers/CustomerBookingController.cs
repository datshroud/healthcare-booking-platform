using System.Globalization;
using System.Security.Claims;
using BookingCareManagement.Application.Common.Exceptions;
using BookingCareManagement.Application.Features.Appointments.Commands;
using BookingCareManagement.Application.Features.Appointments.Dtos;
using BookingCareManagement.Application.Features.Specialties.Dtos;
using BookingCareManagement.Application.Features.Specialties.Queries;
using BookingCareManagement.Domain.Abstractions;
using BookingCareManagement.Domain.Aggregates.ClinicRoom;
using BookingCareManagement.Domain.Aggregates.Doctor;
using BookingCareManagement.Domain.Aggregates.User;
using BookingCareManagement.Infrastructure.Persistence;
using BookingCareManagement.Web.Areas.Customer.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookingCareManagement.Web.Areas.Customer.Controllers;

[Route("api/customer-booking")]
[ApiController]
public class CustomerBookingController : ControllerBase
{
    private readonly ApplicationDBContext _dbContext;

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

        var windows = doctor.WorkingHours
            .Where(h => h.DayOfWeek == targetDate.DayOfWeek)
            .OrderBy(h => h.StartTime)
            .ToArray();

        if (windows.Length == 0)
        {
            return Ok(Array.Empty<DoctorTimeSlotDto>());
        }

        var dayStartLocal = DateTime.SpecifyKind(targetDate.ToDateTime(TimeOnly.MinValue), DateTimeKind.Local);
        var dayEndLocal = dayStartLocal.AddDays(1);
        var dayStartUtc = dayStartLocal.ToUniversalTime();
        var dayEndUtc = dayEndLocal.ToUniversalTime();

        var takenEntries = await _dbContext.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorId == doctorId && a.StartUtc >= dayStartUtc && a.StartUtc < dayEndUtc)
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

    [HttpPost]
    public async Task<ActionResult<AppointmentDto>> CreateBooking(
        [FromBody] CreateCustomerBookingRequest request,
        [FromServices] CreateAppointmentCommandHandler handler,
        [FromServices] IDoctorRepository doctorRepository,
        [FromServices] ISpecialtyRepository specialtyRepository,
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

        var minLeadLocal = DateTime.SpecifyKind(DateTime.Now.Date.AddDays(2), DateTimeKind.Local);
        var minLeadUtc = minLeadLocal.ToUniversalTime();
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
            ServiceId = request.SpecialtyId,
            ClinicRoomId = clinicRoomId,
            StartUtc = slotStartUtc,
            DurationMinutes = durationMinutes,
            PatientName = trimmedName,
            CustomerPhone = trimmedPhone,
            PatientId = string.IsNullOrWhiteSpace(currentUserId) ? null : currentUserId
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

        return Ok(dto);
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
            Price = null,
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
        var dateStartLocal = DateTime.SpecifyKind(date.ToDateTime(TimeOnly.MinValue), DateTimeKind.Local);

        foreach (var window in windows)
        {
            var current = window.StartTime;
            while (current + slotLength <= window.EndTime)
            {
                var startLocal = dateStartLocal.Add(current);
                var endLocal = startLocal.Add(slotLength);

                var startUtc = startLocal.ToUniversalTime();
                var endUtc = endLocal.ToUniversalTime();

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
}
