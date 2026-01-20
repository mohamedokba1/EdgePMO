using EdgePMO.API.Contracts;
using EdgePMO.API.Dtos;
using EdgePMO.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EdgePMO.API.Controllers
{
    [Route("api/v1.0/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IPageContentServices _pageServices;

        public DashboardController(IPageContentServices pageServices)
        {
            _pageServices = pageServices;
        }

        [Authorize(Policy = "Admin")]
        [HttpPost("pages")]
        public async Task<IActionResult> Create([FromBody] PageContentCreateDto dto)
        {
            Response resp = await _pageServices.CreateAsync(dto);
            return StatusCode((int)resp.Code, resp);
        }

        [Authorize(Policy = "Admin")]
        [HttpDelete("pages/{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            Response resp = await _pageServices.DeleteAsync(id);
            return StatusCode((int)resp.Code, resp);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPagesContent()
        {
            Response resp = await _pageServices.GetAllAsync();
            return StatusCode((int)resp.Code, resp);
        }

        [HttpGet("pages/{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            Response resp = await _pageServices.GetByIdAsync(id);
            return StatusCode((int)resp.Code, resp);
        }

        [HttpGet("pages/{slug}")]
        public async Task<IActionResult> GetBySlug(string slug)
        {
            Response resp = await _pageServices.GetBySlugAsync(slug);
            return StatusCode((int)resp.Code, resp);
        }

        [Authorize(Policy = "Admin")]
        [HttpPatch("pages")]
        public async Task<IActionResult> Update([FromBody] PageContentUpdateDto dto)
        {
            Response resp = await _pageServices.UpdateAsync(dto);
            return StatusCode((int)resp.Code, resp);
        }
    }
}