using EdgePMO.API.Contracts;
using EdgePMO.API.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EdgePMO.API.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/v2.0/[controller]")]
    public class PromoCodesController : ControllerBase
    {
        private readonly IPromoCodeServices _promoServices;
        public PromoCodesController(IPromoCodeServices promoServices) => _promoServices = promoServices;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PromoCodeCreateDto dto) =>
            Ok(await _promoServices.CreateAsync(dto));

        [HttpGet("validate/{code}")]
        [AllowAnonymous] // Users need to validate at checkout
        public async Task<IActionResult> Validate(string code, [FromQuery] Guid? courseId) =>
            Ok(await _promoServices.ValidateCodeAsync(code, courseId));

        // Additional GET, PUT, DELETE methods...
    }
}
