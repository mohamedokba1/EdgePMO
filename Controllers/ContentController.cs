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

        [HttpPost("upload-stream")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> Upload([FromQuery] string fileName)
        {
            Response response = new Response();
            if (string.IsNullOrWhiteSpace(fileName))
                return BadRequest("File name is required");

            string? extension = Path.GetExtension(fileName);

            Directory.CreateDirectory("/var/www/uploads");

            string? storedFileName = $"{fileName}{extension}";
            string? fullPath = Path.Combine("/var/www/uploads", storedFileName);

            await using var output = new FileStream(
                fullPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 1024 * 1024,
                useAsync: true);

            if (!IsValidFileSignature(Request.Body, extension))
                return BadRequest("Invalid file signature");

            await Request.Body.CopyToAsync(output);

            response.IsSuccess = true;
            response.Message = "File uploaded successfully";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("filename", storedFileName);
            response.Result.Add("path", fullPath);
            response.Result.Add("size", output.Length);

            return Ok(response);

        }

        private static bool IsValidFileSignature(Stream stream, string ext)
        {
            stream.Position = 0;

            Span<byte> header = stackalloc byte[8];
            stream.Read(header);

            stream.Position = 0;

            return ext switch
            {
                ".pdf" => header[0] == 0x25 && header[1] == 0x50,
                ".png" => header[..8].SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
                ".jpg" or ".jpeg" => header[0] == 0xFF && header[1] == 0xD8,
                ".zip" => header[0] == 0x50 && header[1] == 0x4B,
                ".mp4" => header[4] == 0x66 && header[5] == 0x74,
                _ => true
            };
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
