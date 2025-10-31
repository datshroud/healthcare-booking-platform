using System.Security.Cryptography;
using BookingCareManagement.Application.Features.Auth.Commands;
using BookingCareManagement.Application.Features.Auth.Dtos;
using BookingCareManagement.Domain.Aggregates.User;
using BookingCareManagement.Infrastructure.Identity;
using BookingCareManagement.Utils;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BookingCareManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>>
            Register([FromServices] RegisterHandler handler, [FromBody] RegisterRequest req)
        {
            var resp = await handler.Handle(req);
            CookieHelper.SetAuthCookies(Response, resp.AccessToken, resp.ExpiresAt.ToUniversalTime(),
                resp.RefreshToken, DateTime.UtcNow.AddDays(7));
            return NoContent();
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>>
            Login([FromServices] LoginHandler handler, [FromBody] LoginRequest req)
        {
            var resp = await handler.Handle(req);
            CookieHelper.SetAuthCookies(Response, resp.AccessToken, resp.ExpiresAt.ToUniversalTime(),
                resp.RefreshToken, DateTime.UtcNow.AddDays(7));
            return NoContent();
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

        private readonly UserManager<AppUser> _userManager;
        public readonly IJwtTokenGenerator _jwt;

        private readonly GoogleOAuthSettings _google;

        public AuthController(UserManager<AppUser> userManager, IJwtTokenGenerator jwt, IOptions<GoogleOAuthSettings> googleOptions)
        {
            _userManager = userManager;
            _jwt = jwt;
            _google = googleOptions.Value;

        }

        [HttpGet("google/start")]
        public IActionResult GoogleStart([FromQuery] string? returnUrl = "/")
        {
            var callback = $"{Request.Scheme}://{Request.Host}/api/auth/google/callback";
            // tao state + PKCE
            var state = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            var codeVerifier = Base64UrlEncoder(RandomNumberGenerator.GetBytes(32));
            var codeChallenge = Base64UrlEncoder(Sha256(codeVerifier));

            Response.Cookies.Append("g_state", state, new CookieOptions
            { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Lax, Expires = DateTime.UtcNow.AddMinutes(5) });
            Response.Cookies.Append("g_code_verifier", codeVerifier, new CookieOptions
            { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Lax, Expires = DateTime.UtcNow.AddMinutes(5) });
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
        public async Task<IActionResult> GoogleCallback([FromQuery] string code, [FromQuery] string state)
        {
            var savedState = Request.Cookies["g_state"];
            var codeVerifier = Request.Cookies["g_code_verifier"];
            var returnUrl = Request.Cookies["g_return"] ?? "/";
            var callback = Request.Cookies["g_callback"]; 

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
                ["redirect_uri"] = _google.RedirectUri,
                ["grant_type"] = "authorization_code",
                ["RedirectUri"] = callback!
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

            // upsert user theo email
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new AppUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FullName = name

                };
                var create = await _userManager.CreateAsync(user);
                if (!create.Succeeded)
                    return BadRequest(string.Join("; ", create.Errors.Select(e => e.Description)));
            }
            else if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);
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
    }
}