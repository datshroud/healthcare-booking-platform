using BookingCareManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookingCareManagement.Web.Areas.Doctor.Controllers;

[ApiController]
[Route("api/invoice/filters")]
public class InvoiceFilterController : ControllerBase
{
    private readonly ApplicationDBContext _context;

    public InvoiceFilterController(ApplicationDBContext context)
    {
        _context = context;
    }

    [HttpGet("services")]
    public async Task<IActionResult> GetServices(CancellationToken ct)
    {
        var items = await _context.Specialties
            .Where(s => s.Active)
            .Select(s => s.Name)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(ct);

        return Ok(items);
    }

    [HttpGet("employees")]
    public async Task<IActionResult> GetEmployees(CancellationToken ct)
    {
        // L?y danh sách AppointmentId t? Invoices
        var apptIds = await _context.Invoices
            .Select(i => i.AppointmentId)
            .ToListAsync(ct);

        // Join Appointment -> Doctor -> AppUser
        var q = from appt in _context.Appointments.AsNoTracking()
                where apptIds.Contains(appt.Id)
                join d in _context.Doctors.AsNoTracking() on appt.DoctorId equals d.Id
                join u in _context.Users.AsNoTracking() on d.AppUserId equals u.Id
                select
                    !string.IsNullOrWhiteSpace(u.FullName)
                        ? u.FullName
                        : !string.IsNullOrWhiteSpace(((u.FirstName ?? "") + " " + (u.LastName ?? "")).Trim())
                            ? ((u.FirstName ?? "") + " " + (u.LastName ?? "")).Trim()
                            : u.Email;

        var items = await q.Distinct().OrderBy(x => x).ToListAsync(ct);
        return Ok(items);
    }

    [HttpGet("customers")]
    public async Task<IActionResult> GetCustomers(CancellationToken ct)
    {
        // Prefer users in Customer role, fallback to appointment patient names
        var roleName = "Customer";

        var usersInRole = await (from u in _context.Users.AsNoTracking()
                         join ur in _context.Set<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>().AsNoTracking() on u.Id equals ur.UserId
                         join r in _context.Roles.AsNoTracking() on ur.RoleId equals r.Id
                         where r.Name == roleName
                         select
                            !string.IsNullOrWhiteSpace(u.FullName)
                                ? u.FullName
                                : !string.IsNullOrWhiteSpace(((u.FirstName ?? "") + " " + (u.LastName ?? "")).Trim())
                                    ? ((u.FirstName ?? "") + " " + (u.LastName ?? "")).Trim()
                                    : u.Email
                        ).Distinct().OrderBy(x => x).ToListAsync(ct);

        if (usersInRole.Any()) return Ok(usersInRole);

        var apptNames = await _context.Appointments
            .Select(a => a.PatientName)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(ct);

        return Ok(apptNames);
    }

    [HttpGet("statuses")]
    public async Task<IActionResult> GetStatuses(CancellationToken ct)
    {
        var items = await _context.Invoices
            .Select(i => i.Status)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(ct);

        return Ok(items);
    }
}
