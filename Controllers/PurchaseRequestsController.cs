using EdgePMO.API.Contracts;
using EdgePMO.API.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Net;

namespace EdgePMO.API.Controllers
{
    [Route("api/v1.0/purchase-requests")]
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
            string? userClaim = User.FindFirstValue("id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userClaim, out Guid userId))
            {
                return StatusCode(StatusCodes.Status400BadRequest, new Response
                {
                    IsSuccess = false,
                    Message = "User id not found in token",
                    Code = HttpStatusCode.Unauthorized
                });
            }
            Response resp = await _requestServices.CreateRequestAsync(dto, userId);
            return StatusCode((int)resp.Code, resp);
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMine()
        {
            string? userClaim = User.FindFirstValue("id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userClaim, out Guid userId))
            {
                return StatusCode(StatusCodes.Status400BadRequest, new Response
                {
                    IsSuccess = false,
                    Message = "User id not found in token",
                    Code = HttpStatusCode.Unauthorized
                });
            }

            Response resp = await _requestServices.GetForUserAsync(userId);
            return StatusCode((int)resp.Code, resp);
        }

        [HttpGet("{id:guid}")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Get(Guid id)
        {
            Response resp = await _requestServices.GetByIdAsync(id);
            return StatusCode((int)resp.Code, resp);
        }

        [HttpGet]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            Response resp = await _requestServices.GetAllAsync();
            return StatusCode((int)resp.Code, resp);
        }

        [HttpPatch("{id:guid}/approve")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Approve(Guid id)
        {
            string? adminClaim = User.FindFirstValue("id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(adminClaim, out Guid adminId))
            {
                return StatusCode(StatusCodes.Status400BadRequest, new Response
                {
                    IsSuccess = false,
                    Message = "User id not found in token",
                    Code = HttpStatusCode.Unauthorized
                });
            }

            Response resp = await _requestServices.ApproveAsync(id, adminId);
            return StatusCode((int)resp.Code, resp);
        }

        [HttpPatch("{id:guid}/reject")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] PurchaseRequestRejectionActionDto body)
        {
            string? adminClaim = User.FindFirstValue("id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(adminClaim, out Guid adminId))
            {
                return StatusCode(StatusCodes.Status400BadRequest, new Response
                {
                    IsSuccess = false,
                    Message = "User id not found in token",
                    Code = HttpStatusCode.Unauthorized
                });
            }

            Response resp = await _requestServices.RejectAsync(id, adminId, body.RejectionReasons ?? new List<string>());
            return StatusCode((int)resp.Code, resp);
        }
    }
}
