using System.ComponentModel.DataAnnotations;
using System.Linq;
using BookingCareManagement.Application.Common.Exceptions;
using BookingCareManagement.Application.Features.Doctors.Commands;
using BookingCareManagement.Application.Features.Doctors.Dtos;
using BookingCareManagement.Application.Features.Doctors.Queries;
using Microsoft.AspNetCore.Mvc;

namespace BookingCareManagement.Web.Areas.Admin.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DoctorController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DoctorDto>>>
        GetAllDoctors([FromServices] GetAllDoctorsQueryHandler handler, CancellationToken cancellationToken)
    {
        var doctors = await handler.Handle(cancellationToken);
        return Ok(doctors);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DoctorDto>> GetDoctorById(
        [FromServices] GetDoctorByIdQueryHandler handler,
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetDoctorByIdQuery { Id = id };
            var doctor = await handler.Handle(query, cancellationToken);
            return Ok(doctor);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ProblemDetails { Title = "Not Found", Detail = ex.Message });
        }
    }

    [HttpGet("{id:guid}/profile")]
    public async Task<ActionResult<DoctorProfileDto>> GetProfile(
        [FromServices] GetDoctorProfileQueryHandler handler,
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var dto = await handler.Handle(new GetDoctorProfileQuery { DoctorId = id }, cancellationToken);
            return Ok(dto);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ProblemDetails { Title = "Not Found", Detail = ex.Message });
        }
    }

    [HttpPut("{id:guid}/profile")]
    public async Task<IActionResult> UpdateProfile(
        [FromServices] UpdateDoctorProfileCommandHandler handler,
        Guid id,
        [FromBody] DoctorProfileRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateDoctorProfileCommand
        {
            DoctorId = id,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            DateOfBirth = request.DateOfBirth,
            Description = request.Description,
            AvatarUrl = request.AvatarUrl
        };

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

    [HttpGet("{id:guid}/hours")]
    public async Task<ActionResult<DoctorWorkingHoursDto>> GetWorkingHours(
        [FromServices] GetDoctorWorkingHoursQueryHandler handler,
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var dto = await handler.Handle(new GetDoctorWorkingHoursQuery(id), cancellationToken);
            return Ok(dto);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ProblemDetails { Title = "Not Found", Detail = ex.Message });
        }
    }

    [HttpPut("{id:guid}/hours")]
    public async Task<IActionResult> UpdateWorkingHours(
        [FromServices] UpdateDoctorHoursCommandHandler handler,
        Guid id,
        [FromBody] UpdateDoctorHoursRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateDoctorHoursCommand
        {
            DoctorId = id,
            LimitAppointments = request.LimitAppointments,
            Hours = request.Hours ?? Array.Empty<UpdateDoctorHoursSlotRequest>()
        };

        try
        {
            await handler.Handle(command, cancellationToken);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ProblemDetails { Title = "Not Found", Detail = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails { Title = "Validation Failed", Detail = ex.Message });
        }
    }

    [HttpGet("{id:guid}/dayoffs")]
    public async Task<ActionResult<IEnumerable<DoctorDayOffDto>>> GetDayOffs(
        [FromServices] GetDoctorDayOffsQueryHandler handler,
        Guid id,
        [FromQuery] int? year,
        CancellationToken cancellationToken)
    {
        try
        {
            var dtos = await handler.Handle(new GetDoctorDayOffsQuery { DoctorId = id, Year = year }, cancellationToken);
            return Ok(dtos);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ProblemDetails { Title = "Not Found", Detail = ex.Message });
        }
    }

    [HttpPost("{id:guid}/dayoffs")]
    public async Task<ActionResult<DoctorDayOffDto>> CreateDayOff(
        [FromServices] CreateDoctorDayOffCommandHandler createHandler,
        [FromServices] GetDoctorDayOffsQueryHandler queryHandler,
        Guid id,
        [FromBody] DoctorDayOffRequest request,
        [FromQuery] int? year,
        CancellationToken cancellationToken)
    {
        try
        {
            var dayOffId = await createHandler.Handle(
                new CreateDoctorDayOffCommand(id, request.Name, request.StartDate, request.EndDate, request.RepeatYearly),
                cancellationToken);

            var dtos = await queryHandler.Handle(new GetDoctorDayOffsQuery { DoctorId = id, Year = year }, cancellationToken);
            var dto = dtos.FirstOrDefault(d => d.Id == dayOffId) ?? new DoctorDayOffDto
            {
                Id = dayOffId,
                Name = request.Name,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                RepeatYearly = request.RepeatYearly
            };

            return Ok(dto);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ProblemDetails { Title = "Not Found", Detail = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails { Title = "Validation Failed", Detail = ex.Message });
        }
    }

    [HttpPut("{doctorId:guid}/dayoffs/{dayOffId:guid}")]
    public async Task<IActionResult> UpdateDayOff(
        [FromServices] UpdateDoctorDayOffCommandHandler handler,
        Guid doctorId,
        Guid dayOffId,
        [FromBody] DoctorDayOffRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await handler.Handle(
                new UpdateDoctorDayOffCommand(doctorId, dayOffId, request.Name, request.StartDate, request.EndDate, request.RepeatYearly),
                cancellationToken);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ProblemDetails { Title = "Not Found", Detail = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails { Title = "Validation Failed", Detail = ex.Message });
        }
    }

    [HttpDelete("{doctorId:guid}/dayoffs/{dayOffId:guid}")]
    public async Task<IActionResult> DeleteDayOff(
        [FromServices] DeleteDoctorDayOffCommandHandler handler,
        Guid doctorId,
        Guid dayOffId,
        CancellationToken cancellationToken)
    {
        try
        {
            await handler.Handle(new DeleteDoctorDayOffCommand(doctorId, dayOffId), cancellationToken);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ProblemDetails { Title = "Not Found", Detail = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<DoctorDto>>
        CreateDoctor([FromServices] CreateDoctorCommandHandler handler,
                     [FromBody] CreateDoctorCommand command,
                     CancellationToken cancellationToken)
    {
        try
        {
            var doctorDto = await handler.Handle(command, cancellationToken);
            return Ok(doctorDto);
        }
        catch (Exception ex)
        {
            return BadRequest(new ProblemDetails { Title = "Create Failed", Detail = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateDoctor(
        [FromServices] UpdateDoctorCommandHandler handler,
        Guid id,
        [FromBody] UpdateDoctorRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateDoctorCommand
        {
            Id = id,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            SpecialtyIds = request.SpecialtyIds
        };

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

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteDoctor(
        [FromServices] DeleteDoctorCommandHandler handler,
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteDoctorCommand { Id = id };

        try
        {
            await handler.Handle(command, cancellationToken);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ProblemDetails { Title = "Not Found", Detail = ex.Message });
        }
    }

    public class DoctorProfileRequest
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Description { get; set; }
        public string? AvatarUrl { get; set; }
    }

    public class DoctorDayOffRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public DateOnly StartDate { get; set; }

        [Required]
        public DateOnly EndDate { get; set; }

        public bool RepeatYearly { get; set; }
    }
}
