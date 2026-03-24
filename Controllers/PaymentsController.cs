using EdgePMO.API.Contracts;
using EdgePMO.API.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace EdgePMO.API.Controllers
{
    [ApiController]
    [Route("api/v2.0/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentServices _paymentServices;

        public PaymentsController(IPaymentServices paymentServices) => _paymentServices = paymentServices;

        [HttpPost("initiate")]
        [Authorize]
        public async Task<IActionResult> Initiate([FromBody] PaymentRequestDto dto)
        {
            Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            Response? result = await _paymentServices.InitializePurchaseAsync(userId, dto.CourseId, dto.TemplateId, dto.PromoCode);
            return StatusCode((int)result.Code, result);
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
        {
            using StreamReader? reader = new StreamReader(Request.Body);
            string? body = await reader.ReadToEndAsync();

            if (!_paymentServices.VerifySignature(body, Request.Headers["x-tap-signature"]))
                return Unauthorized();

            JsonDocument? json = JsonDocument.Parse(body);
            string chargeId = json.RootElement.GetProperty("id").GetString();

            await _paymentServices.ProcessCallbackAsync(chargeId);
            return Ok();
        }
    }
}
