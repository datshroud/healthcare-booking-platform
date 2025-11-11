using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
            .Where(d => d.Active)
            .Select(d => new DoctorSummaryResponse(
                d.Id,
                d.FullName,
                d.Specialties.Select(s => s.Name).ToArray()))
            .OrderBy(d => d.FullName)
            .ToListAsync(cancellationToken);

        return Ok(doctors);
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
            .Where(d => d.Id == id && d.Active)
            .Select(d => new DoctorDetailsResponse(
                d.Id,
                d.FullName,
                d.Specialties.Select(s => new DoctorSpecialtyResponse(s.Id, s.Name)).ToArray()))
            .FirstOrDefaultAsync(cancellationToken);

        if (doctor is null)
        {
            return NotFound();
        }

        return Ok(doctor);
    }

    public sealed record DoctorSummaryResponse(Guid Id, string FullName, IReadOnlyCollection<string> Specialties);

    public sealed record DoctorDetailsResponse(Guid Id, string FullName, IReadOnlyCollection<DoctorSpecialtyResponse> Specialties);

    public sealed record DoctorSpecialtyResponse(Guid Id, string Name);
}
