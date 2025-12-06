using EdgePMO.API.Contracts;
using EdgePMO.API.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EdgePMO.API.Controllers
{
    [Route("api/v1.0/[controller]")]
    [ApiController]
    [Authorize(Policy = "Admin")]
    public class ContentController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly IContentServices _contentServices;

        public ContentController(IWebHostEnvironment env, IContentServices contentServices)
        {
            _env = env;
            _contentServices = contentServices;
        }

        [HttpGet("assets")]
        public async Task<IActionResult> ListAssets()
        {
            Response response = await _contentServices.ListAssetsAsync();
            return StatusCode((int)response.Code, response);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadMedia(IFormFile file)
        {
            Response response = await _contentServices.UploadMediaAsync(file);
            return StatusCode((int)response.Code, response);
        }

        [HttpDelete("assets")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> DeleteAsset([FromQuery] string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName) || fileName.Contains(".."))
            {
                return StatusCode(400, new Response
                {
                    IsSuccess = false,
                    Message = "Invalid filename",
                    Code = System.Net.HttpStatusCode.BadRequest
                });
            }

            Response response = await _contentServices.DeleteAssetAsync(fileName);
            return StatusCode((int)response.Code, response);
        }
    }
}
