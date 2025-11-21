using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using BookingCareManagement.Application.Common.Exceptions;
using BookingCareManagement.Application.Features.Auth.Commands;
using BookingCareManagement.Application.Features.Auth.Dtos;
using BookingCareManagement.Domain.Aggregates.User;
using BookingCareManagement.Infrastructure.Identity;
using BookingCareManagement.Utils;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace BookingCareManagement.Web.Areas.Account.Controllers
{
    [Area("Account")]
    [Route("api/account/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
    [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>>
            Register([FromServices] RegisterHandler handler, [FromBody] RegisterRequest req)
        {
            try {
                var resp = await handler.Handle(req);
                CookieHelper.SetAuthCookies(Response, resp.AccessToken, resp.ExpiresAt.ToUniversalTime(),
                    resp.RefreshToken, DateTime.UtcNow.AddDays(7));
                // For API clients return JSON containing frontend redirect URL (avoid redirecting to API GET)
                // hiển thị thông báo đăng ký thành công (bằng sweetalert) và chuyển hướng đến trang đăng nhập

                return Ok(new { redirect = "/auth/login" });
                
            } catch (AuthException ex) {
                return Unauthorized(new ProblemDetails {
                    Title = "Đăng ký thất bại",
                    Detail = ex.Message, // ví dụ: "Sai email hoặc mật khẩu."
                    Status = StatusCodes.Status401Unauthorized
                });
            } catch (Exception) {
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails {
                    Title = "Lỗi máy chủ",
                    Detail = "Có lỗi xảy ra trong quá trình đăng ký, vui lòng thử lại sau.",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
            
        }

    [HttpPost("login")]
    // kiểm tra nếu đăng nhập thành công thì không vào trang này được nữa
        public async Task<ActionResult<AuthResponse>>
            Login([FromServices] LoginHandler handler, [FromBody] LoginRequest req)
        {
            // log the login attempt email for debugging (do NOT log passwords)
            Console.WriteLine($"Login attempt for: {req?.Email}");
            if (req == null) return BadRequest(new ProblemDetails { Title = "Yêu cầu không hợp lệ" });
            try {
                var resp = await handler.Handle(req);
                CookieHelper.SetAuthCookies(Response, resp.AccessToken, resp.ExpiresAt.ToUniversalTime(),
                    resp.RefreshToken, DateTime.UtcNow.AddDays(7));

                var user = await _userManager.FindByEmailAsync(req.Email);
                var userRoles = user is not null
                    ? await _userManager.GetRolesAsync(user)
                    : Array.Empty<string>();

                var redirect = ResolveDashboardRedirect(userRoles);
                return Ok(new
                {
                    redirect,
                    accessToken = resp.AccessToken,
                    refreshToken = resp.RefreshToken,
                    expiresAt = resp.ExpiresAt
                });
            } catch (AuthException ex) {
                Console.WriteLine($"Login failed for {req?.Email}: {ex.Message}");
                return Unauthorized(new ProblemDetails {
                    Title = "Đăng nhập thất bại",
                    Detail = ex.Message,
                    Status = StatusCodes.Status401Unauthorized
                });
            } catch (Exception) {
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails {
                    Title = "Lỗi máy chủ",
                    Detail = "Có lỗi xảy ra trong quá trình đăng nhập, vui lòng thử lại sau.",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponse>>
            RefreshToken([FromServices] RefreshTokenHandler handler, [FromBody] RefreshTokenRequest req)
        {
            var resp = await handler.Handle(req);
            CookieHelper.SetAuthCookies(Response, resp.AccessToken, resp.ExpiresAt.ToUniversalTime(),
                resp.RefreshToken, DateTime.UtcNow.AddDays(7));
            return NoContent();
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            CookieHelper.ClearAuthCookies(Response);
            return NoContent();
        }

    [HttpPost("forgot-password")]
        public IActionResult ForgotPassword()
        {
            return NoContent();
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<ActionResult<UserProfileDto>> GetProfileAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return Unauthorized();
            }

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(MapUserToProfile(user, roles));
        }

        [Authorize]
        [HttpPut("profile")]
        public async Task<ActionResult<UserProfileDto>> UpdateProfileAsync([FromBody] UserProfileDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return Unauthorized();
            }

            user.FirstName = dto.FirstName?.Trim();
            user.LastName = dto.LastName?.Trim();
            user.FullName = string.IsNullOrWhiteSpace(dto.FullName)
                ? $"{user.FirstName} {user.LastName}".Trim()
                : dto.FullName.Trim();
            user.Email = string.IsNullOrWhiteSpace(dto.Email) ? user.Email : dto.Email.Trim();
            user.UserName = user.Email;
            user.AvatarUrl = dto.AvatarUrl;
            user.DateOfBirth = dto.DateOfBirth;

            var update = await _userManager.UpdateAsync(user);
            if (!update.Succeeded)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Cập nhật hồ sơ không thành công",
                    Detail = string.Join("; ", update.Errors.Select(e => e.Description))
                });
            }

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(MapUserToProfile(user, roles));
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<ActionResult> ChangePasswordAsync([FromBody] ChangePasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CurrentPassword))
            {
                return BadRequest(new ProblemDetails { Title = "Mật khẩu hiện tại không được để trống" });
            }

            if (string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest(new ProblemDetails { Title = "Mật khẩu mới không được để trống" });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return Unauthorized();
            }

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!result.Succeeded)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Đổi mật khẩu thất bại",
                    Detail = string.Join("; ", result.Errors.Select(e => e.Description))
                });
            }

            return NoContent();
        }

        private readonly UserManager<AppUser> _userManager;
        public readonly IJwtTokenGenerator _jwt;

        private readonly GoogleOAuthSettings _google;

        public AuthController(UserManager<AppUser> userManager, IJwtTokenGenerator jwt, IOptions<GoogleOAuthSettings> googleOptions)
        {
            _userManager = userManager;
            _jwt = jwt;
            _google = googleOptions.Value;

        }

        private static string ResolveDashboardRedirect(IList<string> roles)
        {
            if (roles is null || roles.Count == 0)
            {
                return "/";
            }

            if (roles.Contains("Admin"))
            {
                return "/dashboard";
            }

            if (roles.Contains("Doctor"))
            {
                return "/doctor/dashboard";
            }

            return "/";
        }

        private static UserProfileDto MapUserToProfile(AppUser user, IList<string> roles)
        {
            var normalizedRoles = roles ?? Array.Empty<string>();
            return new UserProfileDto
            {
                UserId = user.Id,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                FullName = user.GetFullName() ?? user.Email ?? user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                AvatarUrl = user.AvatarUrl,
                DateOfBirth = user.DateOfBirth,
                Roles = normalizedRoles.ToArray(),
                IsAdmin = normalizedRoles.Any(r => string.Equals(r, "Admin", StringComparison.OrdinalIgnoreCase)),
                IsDoctor = normalizedRoles.Any(r => string.Equals(r, "Doctor", StringComparison.OrdinalIgnoreCase))
            };
        }

        [HttpGet("google/start")]
        [HttpGet("~/api/auth/google/start")]
        public IActionResult GoogleStart([FromQuery] string? returnUrl = "/")
        {
            // Use configured RedirectUri so it exactly matches the URI registered in Google Console
            var callback = _google.RedirectUri?.TrimEnd('/') ?? $"{Request.Scheme}://{Request.Host}/api/auth/google/callback";

            // tao state + PKCE
            var state = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            var codeVerifier = Base64UrlEncoder(RandomNumberGenerator.GetBytes(32));
            var codeChallenge = Base64UrlEncoder(Sha256(codeVerifier));

            Response.Cookies.Append("g_state", state, new CookieOptions
            { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Lax, Expires = DateTime.UtcNow.AddMinutes(5) });
            Response.Cookies.Append("g_code_verifier", codeVerifier, new CookieOptions
            { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Lax, Expires = DateTime.UtcNow.AddMinutes(5) });
            // store return url under a consistent name
            Response.Cookies.Append("g_return_url", returnUrl ?? "/", new CookieOptions
            { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Lax, Expires = DateTime.UtcNow.AddMinutes(5) });

            var query = new Dictionary<string, string?>
            {
                ["client_id"] = _google.ClientId,
                ["redirect_uri"] = callback,
                ["response_type"] = "code",
                ["scope"] = "openid profile email",
                ["state"] = state,
                ["code_challenge"] = codeChallenge,
                ["code_challenge_method"] = "S256",
                ["access_type"] = "offline",
                ["prompt"] = "consent"
            };
            var authUrl = QueryHelpers.AddQueryString("https://accounts.google.com/o/oauth2/v2/auth", query!);
            Console.WriteLine("AUTH URL: " + authUrl);

            return Redirect(authUrl);
        }

    [HttpGet("google/callback")]
    [HttpGet("~/api/auth/google/callback")]
        public async Task<IActionResult> GoogleCallback([FromQuery] string code, [FromQuery] string state)
        {
            var savedState = Request.Cookies["g_state"];
            var codeVerifier = Request.Cookies["g_code_verifier"];
            // read the return url we saved earlier (consistent name)
            var returnUrl = Request.Cookies["g_return_url"] ?? "/";

            Response.Cookies.Delete("g_state");
            Response.Cookies.Delete("g_code_verifier");
            Response.Cookies.Delete("g_return_url");

            if (string.IsNullOrEmpty(savedState) || savedState != state || string.IsNullOrEmpty(codeVerifier))
                return BadRequest("Invalid state or verifier.");

            using var http = new HttpClient();
            var tokenReq = new Dictionary<string, string?>
            {
                ["client_id"] = _google.ClientId,
                ["client_secret"] = _google.ClientSecret,
                ["code"] = code,
                ["code_verifier"] = codeVerifier,
                // ensure token exchange uses same redirect uri that was sent in the initial auth request
                ["redirect_uri"] = _google.RedirectUri,
                ["grant_type"] = "authorization_code",
            };

            var resp = await http.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(tokenReq));
            if (!resp.IsSuccessStatusCode)
            {
                var err = await resp.Content.ReadAsStringAsync();
                return BadRequest($"Token exchange failed: {err}");
            }
            var payload = System.Text.Json.JsonDocument.Parse(await resp.Content.ReadAsStringAsync()).RootElement;
            var idToken = payload.GetProperty("id_token").GetString();
            if (string.IsNullOrEmpty(idToken))
                return BadRequest("ID token missing");

            var gPayLoad = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _google.ClientId }
            });

            var email = gPayLoad.Email;
            var emailVerified = gPayLoad.EmailVerified;
            var name = gPayLoad.Name;
            var picture = gPayLoad.Picture;
            var sub = gPayLoad.Subject;

            if (!emailVerified)
                return BadRequest("Email is not verified by Google.");

            // upsert user theo email (robust against existing username/email collisions)
            var user = await _userManager.FindByEmailAsync(email);
            var isNew = false;
            if (user == null)
            {
                var (firstName, lastName) = SplitName(name);
                user = new AppUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FirstName = firstName,
                    LastName = lastName
                };

                var created = await _userManager.CreateAsync(user);
                if (!created.Succeeded)
                {
                    // If a user with the same username or email already exists, try to recover by loading that user
                    var duplicateUser = (await _userManager.FindByNameAsync(user.UserName)) ?? await _userManager.FindByEmailAsync(user.Email);
                    if (duplicateUser != null)
                    {
                        // reuse existing account instead of failing
                        user = duplicateUser;
                    }
                    else
                    {
                        // unknown failure, return errors
                        return BadRequest(string.Join("; ", created.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    isNew = true;
                }
            }
            else if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                var (firstName, lastName) = SplitName(name);
                var shouldUpdateName = false;

                if (!string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(user.FirstName))
                {
                    user.FirstName = firstName;
                    shouldUpdateName = true;
                }

                if (!string.IsNullOrWhiteSpace(lastName) && string.IsNullOrWhiteSpace(user.LastName))
                {
                    user.LastName = lastName;
                    shouldUpdateName = true;
                }

                if (shouldUpdateName)
                {
                    await _userManager.UpdateAsync(user);
                }
            }

            if (isNew)
            {
                var roleStoreOk = await _userManager.IsInRoleAsync(user, "Customer");
                if (!roleStoreOk)
                    await _userManager.AddToRoleAsync(user, "Customer");
            } else
            {
                var rolesNow = await _userManager.GetRolesAsync(user);
                if (rolesNow == null || rolesNow.Count == 0)
                    await _userManager.AddToRoleAsync(user, "Customer");
            }

            var roles = await _userManager.GetRolesAsync(user);

            var (token, exp) = _jwt.GenerateToken(user, roles);

            var refresh = RefreshTokenFactory.Create();
            user.RefreshTokens.Add(refresh);
            await _userManager.UpdateAsync(user);

            CookieHelper.SetAuthCookies(Response, token, exp.ToUniversalTime(),
                refresh.Token, DateTime.UtcNow.AddDays(7));

            return Redirect(returnUrl);
        }

        private static byte[] Sha256(string input)
        {
            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
        }

        private static string Base64UrlEncoder(byte[] input)
            => WebEncoders.Base64UrlEncode(input).Replace("=", string.Empty);

        private static (string FirstName, string LastName) SplitName(string? displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                return (string.Empty, string.Empty);
            }

            var parts = displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                return (displayName.Trim(), string.Empty);
            }

            if (parts.Length == 1)
            {
                return (parts[0].Trim(), string.Empty);
            }

            return (parts[0].Trim(), parts[^1].Trim());
        }

    }

}
