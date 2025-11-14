using BookingCareManagement.Application.Common.Exceptions;
using BookingCareManagement.Application.Features.Specialties.Commands;
using BookingCareManagement.Application.Features.Specialties.Dtos;
using BookingCareManagement.Application.Features.Specialties.Queries;
using Microsoft.AspNetCore.Mvc;

namespace BookingCareManagement.Web.Areas.Admin.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SpecialtyController : ControllerBase
{
    // GET: /api/Specialty
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SpecialtyDto>>> GetAll(
        [FromServices] GetAllSpecialtiesQueryHandler handler,
        CancellationToken cancellationToken)
    {
        var specialties = await handler.Handle(new GetAllSpecialtiesQuery(), cancellationToken);
        return Ok(specialties);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SpecialtyDto>> GetById(
        [FromServices] GetSpecialtyByIdQueryHandler handler,
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var dto = await handler.Handle(new GetSpecialtyByIdQuery { Id = id }, cancellationToken);
            return Ok(dto);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ProblemDetails { Title = "Not Found", Detail = ex.Message });
        }
    }

    // POST: /api/Specialty
    [HttpPost]
    public async Task<ActionResult<SpecialtyDto>> Create(
        [FromServices] CreateSpecialtyCommandHandler handler,
        [FromBody] CreateSpecialtyCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var dto = await handler.Handle(command, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
        }
        catch (Exception ex)
        {
            return BadRequest(new ProblemDetails { Title = "Create Failed", Detail = ex.Message });
        }
    }

    // PUT: /api/Specialty/{id}
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SpecialtyDto>> Update(
        [FromServices] UpdateSpecialtyCommandHandler handler,
        Guid id,
        [FromBody] UpdateSpecialtyCommand command,
        CancellationToken cancellationToken)
    {
        command.Id = id;

        try
        {
            var dto = await handler.Handle(command, cancellationToken);
            return Ok(dto);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new ProblemDetails { Title = "Validation Failed", Detail = ex.Message });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ProblemDetails { Title = "Not Found", Detail = ex.Message });
        }
    }

    // DELETE: /api/Specialty/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        [FromServices] DeleteSpecialtyCommandHandler handler,
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            await handler.Handle(new DeleteSpecialtyCommand { Id = id }, cancellationToken);
            return NoContent();
        }
        catch (ValidationException ex)
        {
            return BadRequest(new ProblemDetails { Title = "Validation Failed", Detail = ex.Message });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ProblemDetails { Title = "Not Found", Detail = ex.Message });
        }
    }
}