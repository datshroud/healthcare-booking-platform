using System.Security.Cryptography;
using BookingCareManagement.Application.Features.Auth.Commands;
using BookingCareManagement.Application.Features.Auth.Dtos;
using BookingCareManagement.Domain.Aggregates.User;
using BookingCareManagement.Infrastructure.Identity;
using BookingCareManagement.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
            
        private readonly UserManager<AppUser> _userManager;
        public readonly IJwtTokenGenerator _jwt;

        private readonly GoogleOAuthSettings _google;

        public AuthController(UserManager<AppUser> userManager, IJwtTokenGenerator jwt, GoogleOAuthSettings google)
        {
            _userManager = userManager;
            _jwt = jwt;
            _google = google;

        }

        [HttpGet("google/start")]
        // public IActionResult GoogleStart([FromQuery] string? returnUrl = "/")
        // {
        //     // tao state + PKCE
        //     var state = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        //     var codeVerifier = Base64UrlEncoder(RandomNumberGenerator.GetBytes(32));
        //     var codeChallenge = Base64UrlEncoder(Sha256(codeVerifier));
        // }
    }
}
