using EdgePMO.API.Contracts;
using EdgePMO.API.Dtos;
using EdgePMO.API.Settings;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace EdgePMO.API.Services
{
    public class ContentServices : IContentServices
    {
        private readonly IWebHostEnvironment _env;
        private readonly ContentSettings _settings;

        public ContentServices(IWebHostEnvironment env, IOptions<ContentSettings> settings)
        {
            _env = env;
            _settings = settings.Value;
        }

        public async Task<Response> UploadMediaAsync(IFormFile file)
        {
            Response response = new Response();

            long maxSize = _settings.MaxFileSizeBytes;
            string[] allowedExtensions = _settings.AllowedExtensions ?? Array.Empty<string>();
            string uploadsRelative = string.IsNullOrWhiteSpace(_settings.UploadsRelative) ? "uploads" : _settings.UploadsRelative;

            if (file == null || file.Length == 0)
            {
                response.IsSuccess = false;
                response.Message = "File is required.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            if (file.Length > maxSize)
            {
                response.IsSuccess = false;
                response.Message = $"File is too large. Max allowed is {maxSize} bytes.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            string? ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(ext))
            {
                response.IsSuccess = false;
                response.Message = $"Invalid file type. Allowed: {string.Join(", ", allowedExtensions)}";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            string? webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            string? uploads = Path.Combine(webRoot, uploadsRelative);
            Directory.CreateDirectory(uploads);

            string? filePath = Path.Combine(uploads, file.FileName);

            await using (FileStream? stream = File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }

            string? relativeUrl = $"{uploadsRelative}/{file.FileName}";

            response.IsSuccess = true;
            response.Message = "File uploaded successfully.";
            response.Code = HttpStatusCode.Created;
            response.Result.Add("relativePath", JsonValue.Create(uploads));
            response.Result.Add("fileName", JsonValue.Create(file.FileName));
            response.Result.Add("size", JsonValue.Create(file.Length));
            return response;
        }

        public Task<Response> ListAssetsAsync()
        {
            Response response = new Response();

            string uploadsRelative = string.IsNullOrWhiteSpace(_settings.UploadsRelative) ? "uploads" : _settings.UploadsRelative;
            string? webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            string? uploads = Path.Combine(webRoot, uploadsRelative);

            if (!Directory.Exists(uploads))
            {
                response.IsSuccess = true;
                response.Message = "No assets found.";
                response.Code = HttpStatusCode.OK;
                response.Result.Add("files", JsonSerializer.SerializeToNode(Array.Empty<string>()) ?? JsonValue.Create(Array.Empty<object>()));
                return Task.FromResult(response);
            }

            string[]? files = Directory.EnumerateFiles(uploads, "*", SearchOption.TopDirectoryOnly)
                                       .Select(f => $"{uploadsRelative}/{Path.GetFileName(f)}")
                                       .ToArray();

            response.IsSuccess = true;
            response.Message = "Assets retrieved successfully.";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("files", JsonSerializer.SerializeToNode(files) ?? JsonValue.Create(Array.Empty<object>()));
            return Task.FromResult(response);
        }

        public Task<Response> DeleteAssetAsync(string fileName)
        {
            Response response = new Response();

            if (string.IsNullOrWhiteSpace(fileName))
            {
                response.IsSuccess = false;
                response.Message = "fileName is required.";
                response.Code = HttpStatusCode.BadRequest;
                return Task.FromResult(response);
            }

            fileName = Path.GetFileName(fileName);

            string uploadsRelative = string.IsNullOrWhiteSpace(_settings.UploadsRelative) ? "uploads" : _settings.UploadsRelative;
            string? webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            string? filePath = Path.Combine(webRoot, uploadsRelative, fileName);

            if (!File.Exists(filePath))
            {
                response.IsSuccess = false;
                response.Message = "File not found.";
                response.Code = HttpStatusCode.BadRequest;
                return Task.FromResult(response);
            }

            File.Delete(filePath);

            response.IsSuccess = true;
            response.Message = "File deleted successfully.";
            response.Code = HttpStatusCode.NoContent;
            return Task.FromResult(response);
        }
    }
}
