using BookingCareManagement.Application.Common.Exceptions;
using BookingCareManagement.Application.Features.Customers.Commands;
using BookingCareManagement.Application.Features.Customers.Dtos;
using BookingCareManagement.Application.Features.Customers.Queries;
using Microsoft.AspNetCore.Authorization; // <-- THÊM DÒNG NÀY
using Microsoft.AspNetCore.Mvc;
using System; // Thêm
using System.Collections.Generic; // Thêm
using System.Threading; // Thêm
using System.Threading.Tasks; // Thêm

namespace BookingCareManagement.Web.Areas.Doctor.Controllers;

// === THÊM DÒNG NÀY ĐỂ BẢO VỆ API ===
[Authorize(Policy = "DoctorOrAbove")]
[Route("api/[controller]")]
[ApiController]
public class CustomerController : ControllerBase
{
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
}