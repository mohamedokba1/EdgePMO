using EdgePMO.API.Contracts;
using EdgePMO.API.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EdgePMO.API.Controllers
{
    [Route("v1.0/[controller]")]
    [ApiController]
    [Authorize(Policy = "Admin")]
    public class InstructorsController : ControllerBase
    {
        private readonly IInstructorServices _instructorServices;

        public InstructorsController(IInstructorServices instructorServices)
        {
            _instructorServices = instructorServices;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            Response response = await _instructorServices.GetAllAsync();
            return StatusCode((int)response.Code, response);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            Response response = await _instructorServices.GetByIdAsync(id);
            return StatusCode((int)response.Code, response);
        }

        [HttpPost]
        public async Task<IActionResult> Create(InstructorCreateDto dto)
        {
            Response response = await _instructorServices.CreateAsync(dto);
            return StatusCode((int)response.Code, response);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, InstructorUpdateDto dto)
        {
            if (id != dto.InstructorId)
                return BadRequest("Route id and body InstructorId must match.");

            Response? updateResponse = await _instructorServices.UpdateAsync(dto);
            return StatusCode((int)updateResponse.Code, updateResponse);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            Response? deleteResponse = await _instructorServices.DeleteAsync(id);
            return StatusCode((int)deleteResponse.Code, deleteResponse);
        }
    }
}