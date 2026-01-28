using BookingCareManagement.Application.Common.Exceptions;
using BookingCareManagement.Application.Features.Customers.Commands;
using BookingCareManagement.Application.Features.Customers.Dtos;
using BookingCareManagement.Application.Features.Customers.Queries;
using BookingCareManagement.Domain.Aggregates.User;
using BookingCareManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization; // <-- THÊM DÒNG NÀY
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System; // Thêm
using System.Collections.Generic; // Thêm
using System.Linq;
using System.Security.Claims;
using System.Threading; // Thêm
using System.Threading.Tasks; // Thêm

namespace BookingCareManagement.Web.Areas.Doctor.Controllers;

// === THÊM DÒNG NÀY ĐỂ BẢO VỆ API ===
[Authorize(Policy = "DoctorOrAbove")]
[Route("api/[controller]")]
[ApiController]
public class CustomerController : ControllerBase
{
    private readonly ApplicationDBContext _dbContext;

    public CustomerController(ApplicationDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    // GET: /api/Customer
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAll(
        [FromServices] GetAllCustomersQueryHandler handler,
        CancellationToken cancellationToken)
    {
        var customers = await handler.Handle(cancellationToken);
        return Ok(customers);
    }

    // POST: /api/Customer
    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Create(
        [FromServices] CreateCustomerCommandHandler handler,
        [FromBody] CreateCustomerCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var dto = await handler.Handle(command, cancellationToken);
            return Ok(dto);
        }
        catch (Exception ex)
        {
            return BadRequest(new ProblemDetails { Title = "Create Failed", Detail = ex.Message });
        }
    }

    // PUT: /api/Customer/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        [FromServices] UpdateCustomerCommandHandler handler,
        string id,
        [FromBody] UpdateCustomerCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest(new ProblemDetails { Title = "ID mismatch" });
        }

        try
        {
            await handler.Handle(command, cancellationToken);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ProblemDetails { Title = "Not Found", Detail = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new ProblemDetails { Title = "Update Failed", Detail = ex.Message });
        }
    }

    // DELETE: /api/Customer/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(
        [FromServices] DeleteCustomerCommandHandler handler,
        string id,
        CancellationToken cancellationToken)
    {
        try
        {
            await handler.Handle(new DeleteCustomerCommand { Id = id }, cancellationToken);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ProblemDetails { Title = "Not Found", Detail = ex.Message });
        }
        catch (ValidationException ex)
        {
            return Conflict(new ProblemDetails { Title = "Delete Blocked", Detail = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new ProblemDetails { Title = "Delete Failed", Detail = ex.Message });
        }
    }

    // GET: /api/Customer/for-doctor
    [HttpGet("for-doctor")]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetForDoctor(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Forbid();
        }

        var doctor = await _dbContext.Doctors
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.AppUserId == userId, cancellationToken);
        if (doctor == null)
        {
            return Forbid();
        }

        var appointments = await _dbContext.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorId == doctor.Id && !string.IsNullOrWhiteSpace(a.PatientId))
            .Select(a => new { a.PatientId, a.StartUtc })
            .ToListAsync(cancellationToken);

        if (appointments.Count == 0)
        {
            return Ok(Array.Empty<CustomerDto>());
        }

        var patientIds = appointments
            .Select(a => a.PatientId!)
            .Distinct()
            .ToList();

        var customerRoleId = await _dbContext.Roles
            .AsNoTracking()
            .Where(r => r.NormalizedName == "CUSTOMER")
            .Select(r => r.Id)
            .FirstOrDefaultAsync(cancellationToken);

        IQueryable<AppUser> patientsQuery = _dbContext.Users.AsNoTracking().Where(u => patientIds.Contains(u.Id));
        if (!string.IsNullOrWhiteSpace(customerRoleId))
        {
            patientsQuery = patientsQuery.Where(u => _dbContext.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == customerRoleId));
        }

        var patients = await patientsQuery.ToListAsync(cancellationToken);

        var appointmentStats = appointments
            .GroupBy(a => a.PatientId)
            .ToDictionary(
                g => g.Key!,
                g => new
                {
                    Count = g.Count(),
                    Last = g.Max(x => x.StartUtc)
                });

        var results = new List<CustomerDto>();
        foreach (var user in patients)
        {
            appointmentStats.TryGetValue(user.Id, out var stats);

            var firstName = user.FirstName?.Trim() ?? string.Empty;
            var lastName = user.LastName?.Trim() ?? string.Empty;
            var composedName = string.Join(" ", new[] { firstName, lastName }.Where(x => !string.IsNullOrWhiteSpace(x)));
            var fullName = string.IsNullOrWhiteSpace(composedName)
                ? (user.FullName?.Trim() ?? string.Empty)
                : composedName;

            results.Add(new CustomerDto
            {
                Id = user.Id,
                FirstName = firstName,
                LastName = lastName,
                FullName = fullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                AvatarUrl = user.AvatarUrl,
                Gender = user.Gender,
                DateOfBirth = user.DateOfBirth,
                InternalNote = user.InternalNote,
                CreatedAt = user.CreatedAt,
                AppointmentCount = stats?.Count ?? 0,
                LastAppointment = stats?.Last
            });
        }

        return Ok(results);
    }
}