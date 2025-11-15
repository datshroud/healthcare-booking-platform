using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BookingCareManagement.Domain.Aggregates.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;

namespace BookingCareManagement.Web.Areas.Customer.Pages.AccountSettings;

[Authorize(Roles = "Customer,Admin")]
public class PersonalInfoModel : PageModel
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<PersonalInfoModel> _logger;

    public PersonalInfoModel(UserManager<AppUser> userManager, IWebHostEnvironment environment, ILogger<PersonalInfoModel> logger)
    {
        _userManager = userManager;
        _environment = environment;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [BindProperty]
    public IFormFile? AvatarFile { get; set; }

    public string? AvatarUrl { get; set; }

    public string? StatusMessage { get; private set; }

    public class InputModel
    {
        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        StatusMessage = TempData[nameof(StatusMessage)] as string;

        var (derivedFirst, derivedLast) = DeriveNames(user);

        Input = new InputModel
        {
            FirstName = derivedFirst,
            LastName = derivedLast,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            DateOfBirth = user.DateOfBirth?.Date
        };

        AvatarUrl = ResolveAvatarUrl(user.AvatarUrl);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        AvatarUrl = ResolveAvatarUrl(user.AvatarUrl);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        user.FirstName = Input.FirstName.Trim();
        user.LastName = Input.LastName.Trim();
        user.DateOfBirth = Input.DateOfBirth?.Date;

        if (!string.Equals(user.Email, Input.Email, StringComparison.OrdinalIgnoreCase))
        {
            var setEmailResult = await _userManager.SetEmailAsync(user, Input.Email);
            AppendErrors(setEmailResult);

            var setUserNameResult = await _userManager.SetUserNameAsync(user, Input.Email);
            AppendErrors(setUserNameResult);
        }

        user.PhoneNumber = string.IsNullOrWhiteSpace(Input.PhoneNumber)
            ? null
            : Input.PhoneNumber.Trim();

        if (AvatarFile != null && AvatarFile.Length > 0)
        {
            var savedPath = await SaveAvatarAsync(user, AvatarFile);
            if (savedPath is not null)
            {
                user.AvatarUrl = savedPath;
                AvatarUrl = ResolveAvatarUrl(savedPath);
            }
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var updateResult = await _userManager.UpdateAsync(user);
        AppendErrors(updateResult);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        TempData[nameof(StatusMessage)] = "Thông tin tài khoản đã được cập nhật.";
        return RedirectToPage();
    }

    private static (string FirstName, string LastName) DeriveNames(AppUser user)
    {
        var first = user.FirstName;
        var last = user.LastName;

        var derived = user.GetFullName();
        if (!string.IsNullOrWhiteSpace(derived))
        {
            var parts = derived.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0 && string.IsNullOrWhiteSpace(first))
            {
                first = parts[0];
            }
            if (parts.Length > 1 && string.IsNullOrWhiteSpace(last))
            {
                last = parts[^1];
            }
        }

        if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(last))
        {
            var identifier = user.UserName ?? user.Email ?? user.Id;
            if (!string.IsNullOrWhiteSpace(identifier))
            {
                var tokens = identifier.Split(new[] { ' ', '.', '_', '-' }, StringSplitOptions.RemoveEmptyEntries);
                if (string.IsNullOrWhiteSpace(first) && tokens.Length > 0)
                {
                    first = tokens[0];
                }

                if (string.IsNullOrWhiteSpace(last) && tokens.Length > 1)
                {
                    last = tokens[^1];
                }
            }
        }

        return (first?.Trim() ?? string.Empty, last?.Trim() ?? string.Empty);
    }

    private void AppendErrors(IdentityResult result)
    {
        if (result.Succeeded) return;

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }

    private string ResolveAvatarUrl(string? storedPath)
    {
        if (!string.IsNullOrWhiteSpace(storedPath))
        {
            return storedPath;
        }

        var displayName = string.Join(' ', new[]
        {
            Input?.FirstName,
            Input?.LastName
        }.Where(s => !string.IsNullOrWhiteSpace(s))).Trim();

        if (string.IsNullOrWhiteSpace(displayName))
        {
            displayName = User.Identity?.Name ?? "User";
        }

        var encoded = WebUtility.UrlEncode(displayName);
        return $"https://ui-avatars.com/api/?name={encoded}";
    }

    private async Task<string?> SaveAvatarAsync(AppUser user, IFormFile file)
    {
        try
        {
            const long maxFileSize = 6 * 1024 * 1024;
            if (file.Length > maxFileSize)
            {
                ModelState.AddModelError(nameof(AvatarFile), "Ảnh đại diện không được vượt quá 6MB.");
                return null;
            }

            if (string.IsNullOrWhiteSpace(file.ContentType) || !file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(AvatarFile), "Chỉ chấp nhận tập tin hình ảnh.");
                return null;
            }

            var uploadsRoot = Path.Combine(_environment.WebRootPath, "uploads", "avatars");
            Directory.CreateDirectory(uploadsRoot);

            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = GuessExtension(file.ContentType);
            }

            var fileName = $"{user.Id}_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
            var destinationPath = Path.Combine(uploadsRoot, fileName);

            await using var stream = System.IO.File.Create(destinationPath);
            await file.CopyToAsync(stream);

            if (!string.IsNullOrWhiteSpace(user.AvatarUrl) && user.AvatarUrl.StartsWith("/uploads/avatars", StringComparison.OrdinalIgnoreCase))
            {
                var existingPath = Path.Combine(_environment.WebRootPath, user.AvatarUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(existingPath))
                {
                    System.IO.File.Delete(existingPath);
                }
            }

            return $"/uploads/avatars/{fileName}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save avatar for user {UserId}", user.Id);
            ModelState.AddModelError(string.Empty, "Không thể lưu ảnh đại diện, vui lòng thử lại.");
            return null;
        }
    }

    private static string GuessExtension(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            return ".jpg";
        }

        var provider = new FileExtensionContentTypeProvider();
        foreach (var kvp in provider.Mappings)
        {
            if (string.Equals(kvp.Value, contentType, StringComparison.OrdinalIgnoreCase))
            {
                return kvp.Key;
            }
        }

        return ".jpg";
    }
}
