using EdgePMO.API.Contracts;
using EdgePMO.API.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EdgePMO.API.Controllers
{
    [Route("api/v1.0/[controller]")]
    [ApiController]
    public class PurchaseRequestsController : ControllerBase
    {
        private readonly IPurchaseRequestServices _requestServices;

        public PurchaseRequestsController(IPurchaseRequestServices requestServices)
        {
            _requestServices = requestServices;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] PurchaseRequestCreateDto dto)
        {
            // resolve requestor id from claims
            string? userClaim = User.FindFirstValue("id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userClaim, out Guid userId))
                return Unauthorized(new { message = "User id not found in token" });

            Response resp = await _requestServices.CreateRequestAsync(dto, userId);
            return StatusCode((int)resp.Code, resp);
        }

        // GET api/v1.0/purchase-requests/me  - current user's requests
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMine()
        {
            string? userClaim = User.FindFirstValue("id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userClaim, out Guid userId))
                return Unauthorized(new { message = "User id not found in token" });

            Response resp = await _requestServices.GetForUserAsync(userId);
            return StatusCode((int)resp.Code, resp);
        }

        // GET api/v1.0/purchase-requests/{id}
        [HttpGet("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> Get(Guid id)
        {
            // allow owner or admin — pass requester id to service for ownership check
            string? userClaim = User.FindFirstValue("id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            Guid? requesterId = null;
            if (Guid.TryParse(userClaim, out Guid userId)) requesterId = userId;

            Response resp = await _requestServices.GetByIdAsync(id, requesterId);
            return StatusCode((int)resp.Code, resp);
        }

        // GET api/v1.0/purchase-requests  (admin)
        [HttpGet]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            Response resp = await _requestServices.GetAllAsync();
            return StatusCode((int)resp.Code, resp);
        }

        // PATCH api/v1.0/purchase-requests/{id}/approve  (admin)
        [HttpPatch("{id:guid}/approve")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Approve(Guid id)
        {
            // admin id from token
            string? adminClaim = User.FindFirstValue("id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(adminClaim, out Guid adminId))
                return Unauthorized(new { message = "Admin id not found in token" });

            Response resp = await _requestServices.ApproveAsync(id, adminId);
            return StatusCode((int)resp.Code, resp);
        }

        // PATCH api/v1.0/purchase-requests/{id}/reject  (admin)
        [HttpPatch("{id:guid}/reject")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] PurchaseRequestAdminActionDto body)
        {
            if (string.IsNullOrWhiteSpace(body.Status) || !body.Status.Equals("rejected", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = "Status must be 'rejected' for this endpoint." });

            string? adminClaim = User.FindFirstValue("id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(adminClaim, out Guid adminId))
                return Unauthorized(new { message = "Admin id not found in token" });

            Response resp = await _requestServices.RejectAsync(id, adminId, body.Notes ?? string.Empty);
            return StatusCode((int)resp.Code, resp);
        }
    }
}
