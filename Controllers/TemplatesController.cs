using EdgePMO.API.Contracts;
using EdgePMO.API.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EdgePMO.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TemplatesController : ControllerBase
    {
        private readonly ITemplateServices _templatesServices;

        public TemplatesController(ITemplateServices templatesServices)
        {
            _templatesServices = templatesServices;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            Response resp = await _templatesServices.GetAllAsync();
            return StatusCode((int)resp.Code, resp);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            Response resp = await _templatesServices.GetByIdAsync(id);
            return StatusCode((int)resp.Code, resp);
        }

        [HttpPost]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Create([FromBody] TemplateCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            Response resp = await _templatesServices.CreateAsync(dto);
            return StatusCode((int)resp.Code, resp);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] TemplateUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (id != dto.Id) return BadRequest("Route id and DTO id must match.");
            Response resp = await _templatesServices.UpdateAsync(dto);
            return StatusCode((int)resp.Code, resp);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            Response resp = await _templatesServices.DeleteAsync(id);
            return StatusCode((int)resp.Code, resp);
        }
    }
}
