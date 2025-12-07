using EdgePMO.API.Contracts;
using EdgePMO.API.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EdgePMO.API.Controllers
{
    [Route("api/v1.0/[controller]")]
    [ApiController]
    public class TemplatesController : ControllerBase
    {
        private readonly ITemplateServices _templatesServices;

        public TemplatesController(ITemplateServices templatesServices)
        {
            _templatesServices = templatesServices;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            Response resp = await _templatesServices.GetAllAsync();
            return StatusCode((int)resp.Code, resp);
        }

        [HttpGet("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> Get(Guid id)
        {
            Response resp = await _templatesServices.GetByIdAsync(id);
            return StatusCode((int)resp.Code, resp);
        }

        [HttpPost]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Create([FromBody] TemplateCreateDto dto)
        {
            Response resp = await _templatesServices.CreateAsync(dto);
            return StatusCode((int)resp.Code, resp);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] TemplateUpdateDto dto)
        {
            if (id != dto.Id)
            {
                return StatusCode(400, new Response
                {
                    IsSuccess = false,
                    Message = "Route id and DTO id must match.",
                    Code = HttpStatusCode.BadRequest,
                    Result = null
                });
            }
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

        [HttpPost("{id:guid}/access/grant")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> GrantAccess(Guid id, [FromBody] ListOfUsersEmails dto)
        {
            Response resp = await _templatesServices.GrantAccessByEmailsAsync(id, dto.Emails);
            return StatusCode((int)resp.Code, resp);
        }

        [HttpPost("{id:guid}/access/revoke")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> RevokeAccess(Guid id, [FromBody] ListOfUsersEmails dto)
        {
            Response resp = await _templatesServices.RevokeAccessByEmailsAsync(id, dto.Emails);
            return StatusCode((int)resp.Code, resp);
        }
    }
}
