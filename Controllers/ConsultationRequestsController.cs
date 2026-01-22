using EdgePMO.API.Contracts;
using EdgePMO.API.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EdgePMO.API.Controllers
{
    [Route("api/v1.0/consultation-requests")]
    [ApiController]
    public class ConsultationRequestsController : ControllerBase
    {
        private readonly IConsultationRequestServices _consultationRequestServices;

        public ConsultationRequestsController(IConsultationRequestServices consultationRequestServices)
        {
            _consultationRequestServices = consultationRequestServices;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] ConsultationRequestCreateDto dto)
        {
            Response? resp = await _consultationRequestServices.CreateAsync(dto);
            return StatusCode((int)resp.Code, resp);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            Response? resp = await _consultationRequestServices.DeleteAsync(id);
            return StatusCode((int)resp.Code, resp);
        }

        [HttpGet]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            Response? resp = await _consultationRequestServices.GetAllAsync();
            return StatusCode((int)resp.Code, resp);
        }

        [HttpGet("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> GetById(Guid id)
        {
            Response? resp = await _consultationRequestServices.GetByIdAsync(id);
            return StatusCode((int)resp.Code, resp);
        }

        [HttpPatch("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> Update(Guid id, [FromBody] ConsultationRequestUpdateDto dto)
        {
            if (id != dto.Id)
            {
                return StatusCode(400, new Response
                {
                    IsSuccess = false,
                    Message = "Route id and DTO id must match.",
                    Code = HttpStatusCode.BadRequest
                });
            }
            Response? resp = await _consultationRequestServices.UpdateAsync(id, dto);
            return StatusCode((int)resp.Code, resp);
        }
    }
}