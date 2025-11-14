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
        [FromServices] IAppointmentRepository appointmentRepository,
        CancellationToken cancellationToken)
    {
        var targetDate = date ?? DateOnly.FromDateTime(DateTime.Now);
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

        var existingAppointments = await appointmentRepository.GetAllAsync(cancellationToken);
        var taken = existingAppointments
            .Where(a => a.DoctorId == doctorId)
            .Select(a =>
            {
                var startUtc = DateTime.SpecifyKind(a.StartUtc, DateTimeKind.Utc);
                var endUtc = DateTime.SpecifyKind(a.EndUtc, DateTimeKind.Utc);
                var startLocal = startUtc.ToLocalTime();
                return new
                {
                    StartUtc = startUtc,
                    EndUtc = endUtc,
                    StartLocal = startLocal
                };
            })
            .Where(entry => DateOnly.FromDateTime(entry.StartLocal) == targetDate)
            .Select(entry => (Start: entry.StartUtc, End: entry.EndUtc))
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

        if (string.IsNullOrWhiteSpace(request.CustomerName))
        {
            return BadRequest(new ProblemDetails { Title = "Vui lòng nhập họ tên" });
        }

        if (string.IsNullOrWhiteSpace(request.CustomerPhone))
        {
            return BadRequest(new ProblemDetails { Title = "Vui lòng nhập số điện thoại" });
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

        var command = new CreateAppointmentCommand
        {
            DoctorId = request.DoctorId,
            ServiceId = request.SpecialtyId,
            ClinicRoomId = clinicRoomId,
            StartUtc = DateTime.SpecifyKind(request.SlotStartUtc, DateTimeKind.Utc),
            DurationMinutes = request.DurationMinutes <= 0 ? 30 : request.DurationMinutes,
            PatientName = request.CustomerName.Trim(),
            CustomerPhone = request.CustomerPhone.Trim()
        };

        var dto = await handler.Handle(command, cancellationToken);
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
