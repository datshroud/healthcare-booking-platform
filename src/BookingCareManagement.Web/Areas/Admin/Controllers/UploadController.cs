using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BookingCareManagement.Web.Areas.Admin.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UploadController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ProblemDetails { Title = "No file uploaded." });
            }

            // 1. Chỉ chấp nhận file ảnh
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !allowedExtensions.Contains(ext))
            {
                return BadRequest(new ProblemDetails { Title = "Invalid file type. Only JPG, PNG, GIF are allowed." });
            }

            // 2. Lấy đường dẫn thư mục `wwwroot`
            var wwwRootPath = _webHostEnvironment.WebRootPath;

            // 3. Tạo đường dẫn lưu file (giống hệt ảnh của bạn)
            // Kết quả: "wwwroot/uploads/avatars"
            var uploadPath = Path.Combine(wwwRootPath, "uploads", "avatars");

            // Tạo thư mục nếu nó không tồn tại
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // 4. Tạo tên file duy nhất (để tránh trùng lặp)
            // Kết quả: "8ce664b3-cccf-4895-af65-acb487aae29a_ten-file.png"
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var fullPath = Path.Combine(uploadPath, fileName);

            // 5. Lưu file vào thư mục
            try
            {
                await using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ProblemDetails { Title = "File save failed.", Detail = ex.Message });
            }

            // 6. Trả về đường dẫn URL (rất quan trọng)
            // Kết quả: "/uploads/avatars/ten-file-moi.png"
            var fileUrl = $"/uploads/avatars/{fileName}";

            // 7. Trả về JSON khớp với JavaScript
            return Ok(new { avatarUrl = fileUrl });
        }
    }
}