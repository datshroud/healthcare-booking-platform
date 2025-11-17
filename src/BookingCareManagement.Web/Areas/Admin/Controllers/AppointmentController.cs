using BookingCareManagement.Application.Common.Exceptions;
using BookingCareManagement.Application.Features.Appointments.Commands;
using BookingCareManagement.Application.Features.Appointments.Dtos;
using BookingCareManagement.Application.Features.Appointments.Queries;
using Microsoft.AspNetCore.Mvc;

namespace BookingCareManagement.Web.Areas.Admin.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AppointmentController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetAll(
        [FromServices] GetAllAppointmentsQueryHandler handler,
        CancellationToken cancellationToken)
    {
        var dtos = await handler.Handle(cancellationToken);
        return Ok(dtos);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AppointmentDto>> GetById(
        [FromServices] GetAppointmentByIdQueryHandler handler,
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var dto = await handler.Handle(new GetAppointmentByIdQuery { Id = id }, cancellationToken);
            return Ok(dto);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ProblemDetails { Title = "Not Found", Detail = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<AppointmentDto>> Create(
        [FromServices] CreateAppointmentCommandHandler handler,
        [FromBody] CreateAppointmentCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var dto = await handler.Handle(command, cancellationToken);
            return Ok(dto);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new ProblemDetails { Title = "Validation Failed", Detail = ex.Message });
        }
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(
        [FromServices] CancelAppointmentCommandHandler handler,
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            await handler.Handle(new CancelAppointmentCommand { Id = id }, cancellationToken);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ProblemDetails { Title = "Not Found", Detail = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        [FromServices] DeleteAppointmentCommandHandler handler,
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            await handler.Handle(new DeleteAppointmentCommand { Id = id }, cancellationToken);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ProblemDetails { Title = "Not Found", Detail = ex.Message });
        }
    }
}
