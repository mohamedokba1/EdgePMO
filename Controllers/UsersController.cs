using EdgePMO.API.Contracts;
using EdgePMO.API.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EdgePMO.API.Controllers
{
    [Route("v1.0/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserServices _userServices;

        public UsersController(IUserServices userServices)
        {
            _userServices = userServices;
        }

        [HttpGet]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            Response users = await _userServices.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            Response response = await _userServices.Login(dto);
            string token = response.Result != null && response.Result.ContainsKey("accessToken") ? response.Result["accessToken"]?.ToString() : null;

            if (string.IsNullOrEmpty(token))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, response);
            }
            CookieOptions? cookieOptions = new CookieOptions
            {
                HttpOnly = true,     // Prevents JavaScript access
                Secure = true,       // Only sent over HTTPS
                SameSite = SameSiteMode.Strict, // Prevents CSRF
                Expires = DateTime.Now.AddMinutes(15).ToLocalTime() // Short expiry
            };

            Response.Cookies.Append("accessToken", token, cookieOptions);
            return StatusCode((int)response.Code, response);
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh(RefreshTokenRequestDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.RefreshToken))
                return BadRequest(new { message = "accessToken is required" });

            Response response = await _userServices.Refresh(dto.RefreshToken);

            // If service returned a new access token, set cookie (same behavior as Login)
            string? accessToken = response.Result != null && response.Result.ContainsKey("accessToken") ? response.Result["accessToken"]?.ToString() : null;
            if (!string.IsNullOrEmpty(accessToken))
            {
                CookieOptions cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.Now.AddMinutes(15).ToLocalTime()
                };
                Response.Cookies.Append("accessToken", accessToken, cookieOptions);
            }

            return StatusCode((int)response.Code, response);
        }

        [HttpGet("logout/{id}")]
        [Authorize]
        public async Task<IActionResult> Logout(Guid id)
        {
            Response response = await _userServices.Logout(id);
            return StatusCode((int)response.Code, response);
        }

        [HttpPost("register")]
        [Authorize("Admin")]
        public async Task<IActionResult> Register(RegisterUserDto dto)
        {
            Response response = await _userServices.Register(dto);
            return StatusCode((int)response.Code, response);
        }

        [HttpPost("verify-email")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmail(VerifyEmailDto dto)
        {
            Response response = await _userServices.EmailVerification(dto);
            return StatusCode((int)response.Code, response);
        }

        [HttpPost("send-verification")]
        [AllowAnonymous]
        public async Task<IActionResult> SendVerification(VerifyRequestDto request)
        {
            Response response = await _userServices.SendVerificationMail(request, "Email Verification");
            return StatusCode((int)response.Code, response);
        }
    }
}
