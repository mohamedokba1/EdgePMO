using EdgePMO.API.Contracts;
using EdgePMO.API.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace EdgePMO.API.Controllers
{
    [Route("api/v1.0/[controller]")]
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
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/",
                Expires = DateTime.Now.AddMinutes(60).ToLocalTime()
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

            Response.Cookies.Delete("accessToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/"
            });

            return StatusCode((int)response.Code, response);
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterUserDto dto)
        {
            Response response = await _userServices.Register(dto);
            return StatusCode((int)response.Code, response);
        }

        [HttpPost("register-admin")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterUserDto dto)
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

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            string? currentIdClaim = User.FindFirstValue("id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            Guid? currentUserId = Guid.TryParse(currentIdClaim, out Guid tmpId) ? tmpId : null;
            string? currentEmail = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");

            if (!currentUserId.HasValue && string.IsNullOrWhiteSpace(currentEmail))
            {
                return StatusCode(StatusCodes.Status401Unauthorized, new Response
                {
                    IsSuccess = false,
                    Message = "Unauthorized: User ID or Email claim is missing.",
                    Code = HttpStatusCode.Unauthorized
                });
            }

            Response resp = await _userServices.GetProfileAsync(currentUserId, currentEmail);
            return StatusCode((int)resp.Code, resp);
        }
    }
}
