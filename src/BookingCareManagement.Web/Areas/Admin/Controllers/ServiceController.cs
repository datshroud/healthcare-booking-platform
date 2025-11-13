using BookingCareManagement.Application.Abstractions;
using BookingCareManagement.Application.Common.Exceptions;
using BookingCareManagement.Application.Features.Services.Commands; // Thêm
using BookingCareManagement.Application.Features.Services.Dtos; // Thêm
using BookingCareManagement.Application.Features.Services.Queries;
using BookingCareManagement.Domain.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace BookingCareManagement.Web.Areas.Admin.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ServiceController : ControllerBase
{
    // GET: /api/Service
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromServices] GetAllServicesQueryHandler handler,
        CancellationToken cancellationToken)
    {
        // Sửa DTO trả về
        var dtos = await handler.Handle(cancellationToken);
        return Ok(dtos);
    }

    // POST: /api/Service
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromServices] CreateServiceCommandHandler handler,
        [FromBody] CreateServiceCommand command,
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

    // DELETE (Soft Delete - "Chặn"): /api/Service/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(
        [FromServices] IServiceRepository repo,
        [FromServices] IUnitOfWork uow,
        Guid id, CancellationToken cancellationToken)
    {
        var service = await repo.GetByIdWithTrackingAsync(id, cancellationToken);
        if (service == null) return NotFound();
        service.Deactivate();
        await uow.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    // PUT (Activate - "Bỏ Chặn"): /api/Service/{id}/activate
    [HttpPut("{id:guid}/activate")]
    public async Task<IActionResult> Activate(
        [FromServices] IServiceRepository repo,
        [FromServices] IUnitOfWork uow,
        Guid id, CancellationToken cancellationToken)
    {
        var service = await repo.GetByIdWithTrackingAsync(id, cancellationToken);
        if (service == null) return NotFound();
        service.Activate();
        await uow.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    // DELETE (Permanent - "Xóa vĩnh viễn"): /api/Service/{id}/permanent
    [HttpDelete("{id:guid}/permanent")]
    public async Task<IActionResult> DeletePermanent(
        [FromServices] IServiceRepository repo,
        [FromServices] IUnitOfWork uow,
        Guid id, CancellationToken cancellationToken)
    {
        var service = await repo.GetByIdWithTrackingAsync(id, cancellationToken);
        if (service == null) return NotFound();
        repo.Remove(service);
        await uow.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}