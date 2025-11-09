using AutoMapper;
using AutoMapper.QueryableExtensions;
using BookingCareManagement.Application.DTOs;
using BookingCareManagement.Domain.Entities;
using BookingCareManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookingCareManagement.Controllers;

[Route("api/bacsi")]
[ApiController]
public class BacSiController : ControllerBase
{
    private readonly ApplicationDBContext _context;
    private readonly IMapper _mapper;

    public BacSiController(ApplicationDBContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Lấy danh sách tất cả bác sĩ (chỉ bác sĩ ACTIVE)
    /// </summary>
    /// <returns>Danh sách thông tin bác sĩ</returns>
    [HttpGet]
    [AllowAnonymous] // Cho phép tất cả mọi người xem
    public async Task<ActionResult<IEnumerable<BacSiDto>>> GetBacSiList()
    {
        // Dùng ProjectTo của AutoMapper để EF Core chỉ query các cột cần thiết
        var bacSiList = await _context.BacSi
            .Include(bs => bs.NguoiDung) // Bắt buộc Include để join
            .Where(bs => bs.NguoiDung.VaiTro == "DOCTOR"
                      && bs.NguoiDung.TrangThai == "ACTIVE")
            .ProjectTo<BacSiDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return Ok(bacSiList);
    }

    /// <summary>
    /// Lấy thông tin chi tiết của một bác sĩ
    /// </summary>
    /// <param name="id">Mã bác sĩ (maBacSi)</param>
    /// <returns>Thông tin chi tiết của bác sĩ</returns>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<BacSiDto>> GetBacSiDetail(long id)
    {
        var bacSiDto = await _context.BacSi
            .Include(bs => bs.NguoiDung)
            .Where(bs => bs.MaBacSi == id)
            .ProjectTo<BacSiDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (bacSiDto == null)
        {
            return NotFound("Không tìm thấy bác sĩ.");
        }

        return Ok(bacSiDto);
    }

    /// <summary>
    /// (Bác sĩ) Tự cập nhật hồ sơ cá nhân của mình
    /// </summary>
    /// <param name="dto">Thông tin cần cập nhật</param>
    /// <returns>204 No Content nếu thành công</returns>
    [HttpPut("me")]
    [Authorize(Roles = "Doctor")] // Chỉ vai trò "Doctor" mới được dùng
    public async Task<IActionResult> UpdateMyProfile(UpdateBacSiProfileDto dto)
    {
        // Lấy maBacSi từ token (JWT) của người dùng đã đăng nhập
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString))
        {
            return Unauthorized("Token không hợp lệ.");
        }

        var currentDoctorId = long.Parse(userIdString);

        // Tìm bác sĩ trong DB
        var bacSi = await _context.BacSi.FindAsync(currentDoctorId);
        if (bacSi == null)
        {
            // Trường hợp này hiếm khi xảy ra nếu token hợp lệ
            return NotFound("Không tìm thấy hồ sơ bác sĩ tương ứng với tài khoản.");
        }

        // Dùng AutoMapper để map các trường từ DTO vào Entity
        // Các trường null trong DTO sẽ được bỏ qua (nhờ cấu hình ở MappingProfile)
        _mapper.Map(dto, bacSi);

        // Cập nhật thủ công ngày
        bacSi.NgayCapNhat = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent(); // Trả về 204 No Content khi thành công
    }
}