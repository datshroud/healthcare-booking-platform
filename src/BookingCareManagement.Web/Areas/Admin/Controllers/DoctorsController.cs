using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BookingCareManagement.Domain.Aggregates.User;
using BookingCareManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookingCareManagement.Web.Areas.Admin.Controllers;

/// <summary>
/// Admin-facing endpoints for doctor queries.
/// </summary>
[Area("Admin")]
[ApiController]
[Route("api/admin/[controller]")]
[Produces("application/json")]
public sealed class DoctorsController : ControllerBase
{
    private readonly ApplicationDBContext _dbContext;

    public DoctorsController(ApplicationDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Returns all active doctors with their specialties.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<DoctorSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<DoctorSummaryResponse>>> GetAsync(CancellationToken cancellationToken)
    {
        var doctors = await _dbContext.Doctors
            .AsNoTracking()
            .Include(d => d.Specialties)
            .Include(d => d.AppUser)
            .Where(d => d.Active)
            .ToListAsync(cancellationToken);

        var response = doctors
            .Select(d => new DoctorSummaryResponse(
                d.Id,
                ResolveDisplayName(d.AppUser),
                d.Specialties.Select(s => s.Name).ToArray()))
            .OrderBy(d => d.FullName)
            .ToList();

        return Ok(response);
    }

    /// <summary>
    /// Returns a single doctor and their specialties.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DoctorDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DoctorDetailsResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var doctor = await _dbContext.Doctors
            .AsNoTracking()
            .Include(d => d.Specialties)
            .Include(d => d.AppUser)
            .Where(d => d.Id == id && d.Active)
            .FirstOrDefaultAsync(cancellationToken);

        if (doctor is null)
        {
            return NotFound();
        }

        var response = new DoctorDetailsResponse(
            doctor.Id,
            ResolveDisplayName(doctor.AppUser),
            doctor.Specialties.Select(s => new DoctorSpecialtyResponse(s.Id, s.Name)).ToArray());

        return Ok(response);
    }

    public sealed record DoctorSummaryResponse(Guid Id, string FullName, IReadOnlyCollection<string> Specialties);

    public sealed record DoctorDetailsResponse(Guid Id, string FullName, IReadOnlyCollection<DoctorSpecialtyResponse> Specialties);

    public sealed record DoctorSpecialtyResponse(Guid Id, string Name);

    private static string ResolveDisplayName(AppUser? user)
    {
        if (user is null)
        {
            return "Unknown";
        }

        if (!string.IsNullOrWhiteSpace(user.FullName))
        {
            return user.FullName;
        }

        var first = user.FirstName?.Trim() ?? string.Empty;
        var last = user.LastName?.Trim() ?? string.Empty;
        var combined = string.Join(' ', new[] { first, last }.Where(s => !string.IsNullOrWhiteSpace(s)));

        if (!string.IsNullOrWhiteSpace(combined))
        {
            return combined;
        }

        return user.Email ?? user.UserName ?? "Unknown";
    }
}
