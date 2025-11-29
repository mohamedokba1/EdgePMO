using EdgePMO.API.Contracts;
using EdgePMO.API.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace EdgePMO.API.Controllers
{
    [Route("v1/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserServices _userServices;

        public UsersController(IUserServices userServices)
        {
            _userServices = userServices;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            Response response = await _userServices.Login(dto);
            return StatusCode((int)response.Code, response);
        }

        [HttpGet("logout/{id}")]
        public async Task<IActionResult> Logout(Guid id)
        {
            Response response = await _userServices.Logout(id);
            return StatusCode((int)response.Code, response);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterUserDto dto)
        {
            Response response = await _userServices.Register(dto);
            return StatusCode((int)response.Code, response);
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail(VerifyEmailDto dto)
        {
            Response response = await _userServices.EmailVerification(dto);
            return StatusCode((int)response.Code, response);
        }

        [HttpPost("send-verification")]
        public async Task<IActionResult> SendVerification(VerifyRequestDto request)
        {
            Response response = await _userServices.SendVerificationMail(request);
            return StatusCode((int)response.Code, response);
        }
    }
}
