using EdgePMO.API.Contracts;
using EdgePMO.API.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EdgePMO.API.Controllers
{
    [ApiController]
    [Route("api/v1.0/[controller]")]
    public class KnowledgeHubController : ControllerBase
    {
        private readonly IKnowledgeHubService _service;

        public KnowledgeHubController(IKnowledgeHubService service)
        {
            _service = service;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateKnowledgeHubDto dto)
        {
            Response? response = await _service.CreateAsync(dto);
            return StatusCode((int)response.Code, response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var response = await _service.GetByIdAsync(id);
            return StatusCode((int)response.Code, response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var response = await _service.GetAllAsync(pageNumber, pageSize);
            return StatusCode((int)response.Code, response);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateKnowledgeHubDto dto)
        {
            var response = await _service.UpdateAsync(id, dto);
            return StatusCode((int)response.Code, response);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var response = await _service.DeleteAsync(id);
            return StatusCode((int)response.Code, response);
        }
    }
}
