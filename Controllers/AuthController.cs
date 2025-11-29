using EdgePMO.API.Contracts;
using EdgePMO.API.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace EdgePMO.API.Controllers
{
    [Route("v1/[controller]")]
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
            Response response = new Response();
            bool result = await _userService.ResetPasswordAsync(dto);

            if (!result)
            {
                response.IsSuccess = false;
                response.Message = "Invalid token or expired.";
                response.Code = System.Net.HttpStatusCode.BadRequest;
                return StatusCode((int)response.Code, response);
            }
            else
            {
                response.IsSuccess = true;
                response.Message = "Password reset successfully.";
                response.Code = System.Net.HttpStatusCode.OK;
                return StatusCode((int)response.Code, response);
            }
        }
    }
}
