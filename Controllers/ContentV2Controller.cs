using EdgePMO.API.Contracts;
using EdgePMO.API.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EdgePMO.API.Controllers
{
    [Route("api/v2.0/content")]
    [ApiController]
    [Authorize(Policy = "Admin")]
    public class ContentV2Controller : ControllerBase
    {
        private readonly IContentServices _contentServices;

        public ContentV2Controller(IContentServices contentServices)
        {
            _contentServices = contentServices;
        }

        [HttpGet("physical-structure")]
        public async Task<IActionResult> GetPhysicalStructure()
        {
            Response? response = await _contentServices.GetPhysicalStructureWithIdsAsync();
            return StatusCode((int)response.Code, response);
        }

        [HttpPost("folders")]
        public async Task<IActionResult> CreateFolder([FromQuery] string folderName, [FromQuery] Guid? parentFolderId)
        {
            var response = await _contentServices.CreateFolderAsync(folderName, parentFolderId);
            return StatusCode((int)response.Code, response);
        }

        [HttpDelete("folders/{id}")]
        public async Task<IActionResult> DeleteFolder(Guid id)
        {
            Response? response = await _contentServices.DeleteFolderAsync(id);
            return StatusCode((int)response.Code, response);
        }

        [HttpPost("upload-stream-to-folder")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> UploadToFolder([FromQuery] string fileName, [FromQuery] Guid? targetFolderId)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return BadRequest(new Response { IsSuccess = false, Message = "File name is required" });
            }

            var response = await _contentServices.UploadMediaStreamWithFolderIdAsync(Request, fileName, targetFolderId);
            return StatusCode((int)response.Code, response);
        }

        [HttpPost("sync-database")]
        public async Task<IActionResult> SyncDatabaseWithFileSystem()
        {
            Response? response = await _contentServices.SyncFileSystemToDbAsync();
            return StatusCode((int)response.Code, response);
        }

        [HttpDelete("files/{fileId}")]
        public async Task<IActionResult> DeleteFile(Guid fileId)
        {
            Response? response = await _contentServices.DeleteFileAsync(fileId);
            return StatusCode((int)response.Code, response);
        }

        [HttpPatch("rename/{id}")]
        public async Task<IActionResult> Rename(Guid id, [FromQuery] string newName) 
        {
            if (string.IsNullOrWhiteSpace(newName))
                return BadRequest(new Response
                {
                    IsSuccess = false,
                    Message = "New name parameter is required.",
                    Code = HttpStatusCode.BadRequest
                });

            Response? response = await _contentServices.RenameContentAsync(id, newName);
            return StatusCode((int)response.Code, response);
        }

        [HttpPost("move/{id}")]
        public async Task<IActionResult> Move(Guid id, [FromQuery] Guid? newParentFolderId)
        {
            Response? response = await _contentServices.MoveContentAsync(id, newParentFolderId);
            return StatusCode((int)response.Code, response);
        }
    }
}
