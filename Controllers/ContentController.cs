using EdgePMO.API.Contracts;
using EdgePMO.API.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace EdgePMO.API.Controllers
{
    [Route("v1.0/[controller]")]
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
            if (response.IsSuccess && response.Result.TryGetPropertyValue("files", out JsonNode? filesNode) && filesNode is JsonArray filesArray)
            {
                string[]? fullUrls = filesArray
                    .Select(n => (n as JsonValue)?.GetValue<string>() ?? string.Empty)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .Select(s => $"{Request.Scheme}://{Request.Host}/{s.TrimStart('/')}")
                    .ToArray();

                response.Result["files"] = JsonSerializer.SerializeToNode(fullUrls) ?? JsonValue.Create(Array.Empty<object>());
            }

            return StatusCode((int)response.Code, response);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadMedia(IFormFile file)
        {
            Response response = await _contentServices.UploadMediaAsync(file);
            return StatusCode((int)response.Code, response);
        }

        [HttpDelete("assets/{fileName}")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> DeleteAsset(string fileName)
        {
            Response response = await _contentServices.DeleteAssetAsync(fileName);
            return StatusCode((int)response.Code, response);
        }
    }
}
