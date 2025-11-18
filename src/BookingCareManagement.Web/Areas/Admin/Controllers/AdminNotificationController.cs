using BookingCareManagement.Application.Common.Exceptions;
using BookingCareManagement.Application.Features.Notifications.Commands;
using BookingCareManagement.Application.Features.Notifications.Dtos;
using BookingCareManagement.Application.Features.Notifications.Queries;
using Microsoft.AspNetCore.Mvc;

namespace BookingCareManagement.Web.Areas.Admin.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AdminNotificationController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdminNotificationDto>>> GetRecent(
        [FromServices] GetRecentAdminNotificationsQueryHandler handler,
        [FromQuery] int take = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetRecentAdminNotificationsQuery { Take = take };
        var items = await handler.Handle(query, cancellationToken);
        return Ok(items);
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> GetUnreadCount(
        [FromServices] GetUnreadAdminNotificationsCountQueryHandler handler,
        CancellationToken cancellationToken = default)
    {
        var count = await handler.Handle(new GetUnreadAdminNotificationsCountQuery(), cancellationToken);
        return Ok(count);
    }

    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(
        Guid id,
        [FromServices] MarkAdminNotificationReadCommandHandler handler,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await handler.Handle(new MarkAdminNotificationReadCommand { Id = id }, cancellationToken);
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

    [HttpPost("mark-all-read")]
    public async Task<IActionResult> MarkAllRead(
        [FromServices] MarkAllAdminNotificationsReadCommandHandler handler,
        CancellationToken cancellationToken = default)
    {
        await handler.Handle(new MarkAllAdminNotificationsReadCommand(), cancellationToken);
        return NoContent();
    }
}
