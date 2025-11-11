using BookingCareManagement.Application.Common.Exceptions;
using BookingCareManagement.Application.Features.Doctors.Commands;
using BookingCareManagement.Application.Features.Doctors.Dtos;
using BookingCareManagement.Application.Features.Doctors.Queries;
using Microsoft.AspNetCore.Mvc;

namespace BookingCareManagement.Areas.Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorController : ControllerBase
    {
        // GET (All)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DoctorDto>>>
            GetAllDoctors([FromServices] GetAllDoctorsQueryHandler handler, CancellationToken cancellationToken)
        {
            var doctors = await handler.Handle(cancellationToken);
            return Ok(doctors);
        }

        // GET (by ID)
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

        // POST (Create)
        [HttpPost]
        public async Task<ActionResult<DoctorDto>>
            CreateDoctor([FromServices] CreateDoctorCommandHandler handler,
                         [FromBody] CreateDoctorCommand command, // Command này đã có FirstName, LastName...
                         CancellationToken cancellationToken)
        {
            try
            {
                var doctorDto = await handler.Handle(command, cancellationToken);
                return Ok(doctorDto); // Trả về 200 OK (hoặc 201 Created)
            }
            catch (Exception ex)
            {
                // Bắt lỗi nếu tạo User thất bại (ví dụ: Trùng Email)
                return BadRequest(new ProblemDetails { Title = "Create Failed", Detail = ex.Message });
            }
        }

        // PUT (Update)
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateDoctor(
            [FromServices] UpdateDoctorCommandHandler handler,
            Guid id,
            [FromBody] UpdateDoctorRequest request, // DTO cho body
            CancellationToken cancellationToken)
        {
            // === DÒNG 68 CỦA BẠN LÀ Ở ĐÂY ===
            // Chúng ta Map từ Request (body) sang Command (logic)
            var command = new UpdateDoctorCommand
            {
                Id = id,
                // CẬP NHẬT Ở ĐÂY: Không còn FullName
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                SpecialtyIds = request.SpecialtyIds
            };

            try
            {
                await handler.Handle(command, cancellationToken);
                return NoContent(); // Trả về 204 NoContent
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ProblemDetails { Title = "Not Found", Detail = ex.Message });
            }
            catch (Exception ex)
            {
                // Bắt lỗi nếu update User thất bại (ví dụ: Trùng Email)
                return BadRequest(new ProblemDetails { Title = "Update Failed", Detail = ex.Message });
            }
        }

        // DELETE (Soft Delete)
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
                return NoContent(); // Trả về 204 NoContent
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ProblemDetails { Title = "Not Found", Detail = ex.Message });
            }
        }
    }
}