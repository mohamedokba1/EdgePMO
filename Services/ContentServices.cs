using EdgePMO.API.Contracts;
using EdgePMO.API.Dtos;
using EdgePMO.API.Settings;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

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

        public async Task<Response> UploadMediaAsync(IFormFile file, string? relativePath = null)
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
                response.Message = $"File is too large. Max allowed is {maxSize / (1024 * 1024 * 1024)} GB.";
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

            // Build the upload path
            string uploadPath = uploadsRelative;
            if (!string.IsNullOrWhiteSpace(relativePath))
            {
                string sanitizedPath = SanitizePath(relativePath);
                uploadPath = Path.Combine(uploadsRelative, sanitizedPath);
            }

            string webRoot = Path.Combine("/var/www/", uploadPath);

            try
            {
                Directory.CreateDirectory(webRoot);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"Failed to create directory: {ex.Message}";
                response.Code = HttpStatusCode.InternalServerError;
                return response;
            }

            string filePath = Path.Combine(webRoot, file.FileName);
            string tempFilePath = filePath + ".tmp";

            try
            {
                // Write to temporary file first for safety
                await using (var fileStream = new FileStream(
                    tempFilePath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 1024 * 1024, // 1 MB buffer
                    useAsync: true))
                {
                    using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromHours(2)))
                    {
                        await file.CopyToAsync(fileStream, cancellationTokenSource.Token);
                    }
                }

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                File.Move(tempFilePath, filePath, overwrite: true);

                response.IsSuccess = true;
                response.Message = "File uploaded successfully.";
                response.Code = HttpStatusCode.Created;
                response.Result.Add("relativePath", JsonValue.Create(uploadPath));
                response.Result.Add("fullPath", JsonValue.Create(webRoot));
                response.Result.Add("fileName", JsonValue.Create(file.FileName));
                response.Result.Add("size", JsonValue.Create(file.Length));
                return response;
            }
            catch (OperationCanceledException)
            {
                if (File.Exists(tempFilePath))
                    File.Delete(tempFilePath);

                response.IsSuccess = false;
                response.Message = "File upload timed out. Please try again with a smaller file or better connection.";
                response.Code = HttpStatusCode.RequestTimeout;
                return response;
            }
            catch (Exception ex)
            {
                // Clean up temp file
                if (File.Exists(tempFilePath))
                    File.Delete(tempFilePath);

                response.IsSuccess = false;
                response.Message = $"File upload failed: {ex.Message}";
                response.Code = HttpStatusCode.InternalServerError;
                return response;
            }
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
            string? webRoot = Path.Combine("/var/www/", uploadsRelative);
            string? filePath = Path.Combine(webRoot, fileName);

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

        public Task<bool> FileExistsAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || string.IsNullOrEmpty(filePath))
                return Task.FromResult(false);

            //string uploadsRelative = string.IsNullOrWhiteSpace(relativePath) ? (_settings.UploadsRelative ?? "uploads") : relativePath;
            //string? webRoot = Path.Combine(relativePath ?? "/var/www/", uploadsRelative);
            //string safeFileName = Path.GetFileName(fileName);
            //string filePath = Path.Combine(webRoot, safeFileName);

            return Task.FromResult(File.Exists(filePath));
        }

        public string SanitizePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;

            path = path.Trim();
            path = path.TrimStart('/', '\\');
            path = path.Replace("\\", "/");
            path = Regex.Replace(path, @"\.\.(/|\\|$)", string.Empty);
            path = path.Replace("./", string.Empty);
            path = path.Replace("/..", string.Empty);
            path = Regex.Replace(path, @"/+", "/");

            return path;
        }

        public Task<Response> ListCoursesAssetsAsync()
        {
            Response response = new Response();

            string uploadsRelative = string.IsNullOrWhiteSpace(_settings.UploadsRelative) ? "uploads" : _settings.UploadsRelative;
            string? webRoot = Path.Combine("/var/www/", uploadsRelative);

            string coursesPath = Path.Combine(webRoot, "courses");

            if (!Directory.Exists(webRoot))
            {
                response.IsSuccess = true;
                response.Message = "No assets found.";
                response.Code = HttpStatusCode.OK;
                response.Result.Add("courses", JsonSerializer.SerializeToNode(Array.Empty<string>()) ?? JsonValue.Create(Array.Empty<object>()));
                return Task.FromResult(response);
            }

            string[]? files = Directory.EnumerateFiles(coursesPath, "*", SearchOption.AllDirectories)
                                       .Select(f => $"{Path.GetFullPath(f)}")
                                       .ToArray();

            response.IsSuccess = true;
            response.Message = "Assets retrieved successfully.";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("courses", JsonSerializer.SerializeToNode(files) ?? JsonValue.Create(Array.Empty<object>()));
            return Task.FromResult(response);
        }

        public Task<Response> ListAssetsAsync()
        {
            Response response = new Response();
            string uploadsRelative = string.IsNullOrWhiteSpace(_settings.UploadsRelative) ? "uploads" : _settings.UploadsRelative;
            string webRoot = Path.Combine("/var/www/", uploadsRelative);

            if (!Directory.Exists(webRoot))
            {
                response.IsSuccess = true;
                response.Message = "No assets found.";
                response.Code = HttpStatusCode.OK;
                return Task.FromResult(response);
            }

            string[]? topLevelFolders = Directory.GetDirectories(webRoot);

            foreach (string folder in topLevelFolders)
            {
                string folderName = Path.GetFileName(folder);
                object? folderStructure = BuildDynamicStructure(folder, webRoot);
                response.Result.Add(folderName, JsonSerializer.SerializeToNode(folderStructure) ?? JsonValue.Create(new { }));
            }

            response.IsSuccess = true;
            response.Message = "Assets retrieved successfully.";
            response.Code = HttpStatusCode.OK;

            return Task.FromResult(response);
        }

        private object BuildDynamicStructure(string path, string webRoot)
        {
            string[]? subDirs = Directory.GetDirectories(path);

            // If no subdirectories, return files in current folder
            if (subDirs.Length == 0)
            {
                string[]? files = Directory.GetFiles(path)
                    .Select(f => GetRelativePath(f, webRoot))
                    .ToArray();
                return files;
            }

            // If subdirectories exist, process them recursively
            Dictionary<string, object>? result = new Dictionary<string, object>();

            foreach (string subDir in subDirs)
            {
                string subDirName = Path.GetFileName(subDir);
                result.Add(subDirName, BuildDynamicStructure(subDir, webRoot));
            }

            return result;
        }

        private string GetRelativePath(string fullPath, string basePath)
        {
            string relativePath = fullPath.Replace(basePath, "").TrimStart(Path.DirectorySeparatorChar);
            return relativePath.Replace(Path.DirectorySeparatorChar, '/');
        }
    }
}
