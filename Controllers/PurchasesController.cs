using EdgePMO.API.Contracts;
using EdgePMO.API.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EdgePMO.API.Controllers
{
    [Route("api/v1.0/[controller]")]
    [ApiController]
    public class PurchasesController : ControllerBase
    {
        private readonly IPurchaseServices _purchaseServices;

        public PurchasesController(IPurchaseServices purchaseServices)
        {
            _purchaseServices = purchaseServices;
        }

        [HttpGet]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            Response resp = await _purchaseServices.GetAllAsync();
            return StatusCode((int)resp.Code, resp);
        }

        [HttpGet("{id:guid}")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Get(Guid id)
        {
            Response resp = await _purchaseServices.GetByIdAsync(id);
            return StatusCode((int)resp.Code, resp);
        }

        [HttpPost]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Create([FromBody] PurchaseCreateDto dto)
        {
            Response resp = await _purchaseServices.CreateAsync(dto);
            return StatusCode((int)resp.Code, resp);
        }

        [HttpPatch("{id:guid}/status")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusDto body)
        {
            if (string.IsNullOrWhiteSpace(body.Status)) return BadRequest(new { message = "status is required" });
            Response resp = await _purchaseServices.UpdateStatusAsync(id, body.Status);
            return StatusCode((int)resp.Code, resp);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            Response resp = await _purchaseServices.DeleteAsync(id);
            return StatusCode((int)resp.Code, resp);
        }
    }
}
