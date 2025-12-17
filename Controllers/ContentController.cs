using EdgePMO.API.Contracts;
using EdgePMO.API.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EdgePMO.API.Controllers
{
    [Route("api/v1.0/[controller]")]
    [ApiController]
    [Authorize(Policy = "Admin")]
    public class ContentController : ControllerBase
    {
        private const long MaxFileSize = 3L * 1024 * 1024 * 1024;
        private readonly IContentServices _contentServices;

        public ContentController(IContentServices contentServices)
        {
            _contentServices = contentServices;
        }

        [HttpGet("assets")]
        public async Task<IActionResult> ListAssets()
        {
            Response response = await _contentServices.ListAssetsAsync();
            return StatusCode((int)response.Code, response);
        }

        [HttpGet("courses-assets")]
        public async Task<IActionResult> ListCoursesAssets()
        {
            Response response = await _contentServices.ListCoursesAssetsAsync();
            return StatusCode((int)response.Code, response);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadMedia(IFormFile file, [FromForm] string? path)
        {
            Response response = new Response();
            if (file == null || file.Length == 0)
            {
                response.IsSuccess = false;
                response.Message = "No file provided";
                response.Code = HttpStatusCode.BadRequest;
                return StatusCode((int)response.Code, response);
            }

            if (file.Length > MaxFileSize)
            {
                response.IsSuccess = false;
                response.Message = $"File size exceeds {MaxFileSize / (1024 * 1024 * 1024)} GB limit";
                response.Code = HttpStatusCode.BadRequest;
                return StatusCode((int)response.Code, response);
            }
            response = await _contentServices.UploadMediaAsync(file, path);
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
                    Code = HttpStatusCode.BadRequest
                });
            }

            Response response = await _contentServices.DeleteAssetAsync(fileName);
            return StatusCode((int)response.Code, response);
        }
    }
}
