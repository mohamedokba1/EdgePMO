using EdgePMO.API.Contracts;
using EdgePMO.API.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace EdgePMO.API.Controllers
{
    [Route("api/v1.0/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserServices _userService;

        public AuthController(IUserServices userService)
        {
            _userService = userService;
        }

        [HttpPost("password-reset-request")]
        public async Task<IActionResult> PasswordResetRequest(PasswordResetRequestDto dto)
        {
            Response response = await _userService.SendPasswordResetTokenAsync(dto.Email);
            return StatusCode((int)response.Code, response);
        }

        [HttpPost("password-reset")]
        public async Task<IActionResult> PasswordReset(PasswordResetDto dto)
        {
            Response response = await _userService.ResetPasswordAsync(dto);
            return StatusCode((int)response.Code, response);
        }
    }
}
