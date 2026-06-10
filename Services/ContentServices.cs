using EdgePMO.API.Contracts;
using EdgePMO.API.Dtos;
using EdgePMO.API.Models;
using EdgePMO.API.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace EdgePMO.API.Services
{
    public class ContentServices : IContentServices
    {
        private static readonly Dictionary<string, List<byte[]>> FileSignatures = new(StringComparer.OrdinalIgnoreCase)
        {
            // ===================== Documents =====================
            [".pdf"] = new()
                {
                    new byte[] { 0x25, 0x50, 0x44, 0x46 } // %PDF
                },
            [".docx"] = new()
                {
                    new byte[] { 0x50, 0x4B, 0x03, 0x04 },
                    new byte[] { 0x50, 0x4B, 0x05, 0x06 },
                    new byte[] { 0x50, 0x4B, 0x07, 0x08 }
                },
            [".xlsx"] = new()
                {
                    new byte[] { 0x50, 0x4B, 0x03, 0x04 },
                    new byte[] { 0x50, 0x4B, 0x05, 0x06 },
                    new byte[] { 0x50, 0x4B, 0x07, 0x08 }
                },
            [".pptx"] = new()
                {
                    new byte[] { 0x50, 0x4B, 0x03, 0x04 },
                    new byte[] { 0x50, 0x4B, 0x05, 0x06 },
                    new byte[] { 0x50, 0x4B, 0x07, 0x08 }
                },

            // ===================== Images =====================
            [".png"] = new()
                {
                    new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }
                },
            [".jpg"] = new()
                {
                    new byte[] { 0xFF, 0xD8 }
                },
            [".jpeg"] = new()
                {
                    new byte[] { 0xFF, 0xD8 }
                },
            [".gif"] = new()
                {
                    new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 }, // GIF87a
                    new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }  // GIF89a
                },
            [".bmp"] = new()
                {
                    new byte[] { 0x42, 0x4D }
                },

            // ===================== Audio =====================
            [".mp3"] = new()
                {
                    new byte[] { 0x49, 0x44, 0x33 }, // ID3
                    new byte[] { 0xFF, 0xFB }
                },
            [".wav"] = new()
                {
                    new byte[] { 0x52, 0x49, 0x46, 0x46 } // RIFF
                },

            // ===================== Video =====================
            [".mp4"] = new()
                {
                    new byte[] { 0x66, 0x74, 0x79, 0x70 } // ftyp (offset-based)
                },
            [".mov"] = new()
                {
                    new byte[] { 0x66, 0x74, 0x79, 0x70 }
                },
            [".avi"] = new()
                {
                     new byte[] { 0x52, 0x49, 0x46, 0x46 } // RIFF
                },
            [".mkv"] = new()
                {
                    new byte[] { 0x1A, 0x45, 0xDF, 0xA3 }
                },

            // ===================== Archives =====================
            [".zip"] = new()
                {
                    new byte[] { 0x50, 0x4B, 0x03, 0x04 },
                    new byte[] { 0x50, 0x4B, 0x05, 0x06 },
                    new byte[] { 0x50, 0x4B, 0x07, 0x08 }
                },
            [".rar"] = new()
                {
                    new byte[] { 0x52, 0x61, 0x72, 0x21, 0x1A, 0x07 }
                },
            [".7z"] = new()
                {
                    new byte[] { 0x37, 0x7A, 0xBC, 0xAF, 0x27, 0x1C }
                }
        };

        private readonly EdgepmoDbContext _context;
        private readonly ContentSettings _settings;

        public ContentServices(IOptions<ContentSettings> settings, EdgepmoDbContext context)
        {
            _settings = settings.Value;
            _context = context;
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

        public async Task<Response> ListUploadsAssetsAsync()
        {
            Response response = new Response();
            string uploadsRelative = string.IsNullOrWhiteSpace(_settings.UploadsRelative) ? "uploads" : _settings.UploadsRelative;
            string uploadsPath = Path.Combine("/var/www/", uploadsRelative);

            if (!Directory.Exists(uploadsPath))
            {
                response.IsSuccess = true;
                response.Message = "No assets found.";
                response.Code = HttpStatusCode.OK;
                response.Result.Add("assets", JsonSerializer.SerializeToNode(Array.Empty<object>()));
                return response;
            }

            // Get all files recursively
            var files = Directory.EnumerateFiles(uploadsPath, "*", SearchOption.AllDirectories).ToList();

            // Get all media files from DB
            List<MediaFile>? dbFiles = await _context.MediaFiles
                                                .AsNoTracking()
                                                .ToListAsync();

            // Build result list
            var assets = files.Select(file =>
            {
                string fileName = Path.GetFileName(file);
                string extension = Path.GetExtension(file);
                var dbEntry = dbFiles.FirstOrDefault(mf => mf.FileName == fileName && mf.Extension == extension);

                return new
                {
                    Id = dbEntry?.Id,
                    FileName = fileName,
                    Extension = extension,
                    Path = file,
                    ExistsInDb = dbEntry != null
                };
            }).ToList();

            response.IsSuccess = true;
            response.Message = "Assets retrieved successfully.";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("assets", JsonSerializer.SerializeToNode(assets));
            return response;
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

        public async Task<Response> UploadMediaStreamAsync(HttpRequest request, string fileName)
        {
            Response response = new Response();
            string? extension = Path.GetExtension(fileName);
            Directory.CreateDirectory("/var/www/uploads");
            string? fullPath = Path.Combine("/var/www/uploads", fileName);
            const int signatureLength = 16;
            byte[] header = new byte[signatureLength];

            int bytesRead = await ReadExactlyAsync(request.Body, header, signatureLength);

            //if (!IsValidFileSignature(header, bytesRead, extension))
            //{
            //    response.IsSuccess = false;
            //    response.Message = "Invalid file signature";
            //    response.Code = HttpStatusCode.BadRequest;
            //    response.Code = HttpStatusCode.BadRequest;
            //    return response;
            //}

            await using FileStream? output = new FileStream(
                fullPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 1024 * 1024,
                useAsync: true);

            await output.WriteAsync(header, 0, bytesRead);

            await request.Body.CopyToAsync(output);

            // Store the file metadata in the database if not exists

            MediaFile? existingFile = _context.MediaFiles
                .FirstOrDefault(mf => mf.FileName == fileName && mf.Extension == extension);
            if (existingFile != null)
            {
                response.IsSuccess = true;
                response.Message = "File already exists";
                response.Code = HttpStatusCode.OK;
                response.Result.Add("CertificateId", existingFile.Id);
                response.Result.Add("filename", existingFile.FileName);
                response.Result.Add("path", existingFile.FilePath);
                response.Result.Add("size", existingFile.FileSize);
                return response;
            }

            MediaFile mediaFile = new MediaFile
            {
                Id = Guid.NewGuid(),
                FileName = fileName,
                FilePath = fullPath,
                UploadedAt = DateTime.UtcNow,
                FileSize = output.Length,
                Extension = extension
            };

            _context.MediaFiles.Add(mediaFile);
            await _context.SaveChangesAsync();

            response.IsSuccess = true;
            response.Message = "File uploaded successfully";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("CertificateId", mediaFile.Id);
            response.Result.Add("filename", fileName);
            response.Result.Add("path", fullPath);
            response.Result.Add("size", output.Length);

            return response;
        }

        private static bool IsValidFileSignature(byte[] header, int length, string ext)
        {
            if (length < 2)
                return false;

            ext = ext.ToLowerInvariant();

            // ================= JPEG =================
            if (ext == ".jpg" || ext == ".jpeg")
                return header[0] == 0xFF && header[1] == 0xD8;

            // ================= PNG =================
            if (ext == ".png")
            {
                if (length < 8) return false;

                return header[0] == 0x89 &&
                       header[1] == 0x50 &&
                       header[2] == 0x4E &&
                       header[3] == 0x47 &&
                       header[4] == 0x0D &&
                       header[5] == 0x0A &&
                       header[6] == 0x1A &&
                       header[7] == 0x0A;
            }

            // ================= PDF =================
            if (ext == ".pdf")
                return length >= 4 &&
                       header[0] == 0x25 &&
                       header[1] == 0x50 &&
                       header[2] == 0x44 &&
                       header[3] == 0x46;

            // ================= ZIP-BASED (DOCX, XLSX, PPTX, ZIP) =================
            if (ext == ".zip" || ext == ".docx" || ext == ".xlsx" || ext == ".pptx")
            {
                if (length < 4) return false;

                return header[0] == 0x50 &&
                       header[1] == 0x4B &&
                       (header[2] == 0x03 ||
                        header[2] == 0x05 ||
                        header[2] == 0x07);
            }

            // ================= MP4 / MOV =================
            if (ext == ".mp4" || ext == ".mov")
            {
                if (length < 12) return false;

                return header[4] == 0x66 &&
                       header[5] == 0x74 &&
                       header[6] == 0x79 &&
                       header[7] == 0x70;
            }

            // ================= MKV =================
            if (ext == ".mkv")
                return length >= 4 &&
                       header[0] == 0x1A &&
                       header[1] == 0x45 &&
                       header[2] == 0xDF &&
                       header[3] == 0xA3;

            // ================= OTHER TYPES =================
            return true; // allowed by policy
        }

        private static async Task<int> ReadExactlyAsync(Stream stream, byte[] buffer, int count)
        {
            int totalRead = 0;
            while (totalRead < count)
            {
                int read = await stream.ReadAsync(
                    buffer,
                    totalRead,
                    count - totalRead);

                if (read == 0)
                    break;

                totalRead += read;
            }

            return totalRead;
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

        public async Task<Response> CreateFolderAsync(string folderName, Guid? parentFolderId)
        {
            Response? response = new Response();

            if (string.IsNullOrWhiteSpace(folderName))
            {
                return new Response { IsSuccess = false, Message = "Folder name is required", Code = HttpStatusCode.BadRequest };
            }

            bool exists = await _context.MediaFolders
                .AnyAsync(f => f.Name.ToLower() == folderName.ToLower() && f.ParentFolderId == parentFolderId);

            if (exists)
            {
                return new Response { IsSuccess = false, Message = "Folder already exists in this location", Code = HttpStatusCode.Conflict };
            }

            string parentPath = string.Empty;
            if (parentFolderId.HasValue)
            {
                MediaFolder? parent = await _context.MediaFolders.FindAsync(parentFolderId);
                parentPath = parent?.RelativePath ?? string.Empty;
            }

            string relativePath = Path.Combine(parentPath, folderName);
            string uploadsRelative = string.IsNullOrWhiteSpace(_settings.UploadsRelative) ? "uploads" : _settings.UploadsRelative;
            string fullPath = Path.Combine("/var/www/", uploadsRelative, relativePath);

            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }

            MediaFolder? newFolder = new MediaFolder
            {
                Id = Guid.NewGuid(),
                Name = folderName,
                RelativePath = relativePath,
                ParentFolderId = parentFolderId,
                CreatedAt = DateTime.UtcNow
            };

            _context.MediaFolders.Add(newFolder);
            await _context.SaveChangesAsync();

            response.IsSuccess = true;
            response.Result.Add("folderId", JsonValue.Create(newFolder.Id));
            response.Code = HttpStatusCode.OK;
            response.Message = "Folder created successfully";
            return response;
        }

        public async Task<bool> FolderExistsAsync(string folderName, Guid? parentFolderId)
        {
            if (string.IsNullOrWhiteSpace(folderName)) return false;
            string sanitizedName = folderName.Trim();

            MediaFolder? folderRecord = await _context.MediaFolders
                .AsNoTracking()
                .FirstOrDefaultAsync(f =>
                    f.Name.ToLower() == sanitizedName.ToLower() &&
                    f.ParentFolderId == parentFolderId);

            if (folderRecord == null)
            {
                return false;
            }

            string uploadsRelative = string.IsNullOrWhiteSpace(_settings.UploadsRelative) ? "uploads" : _settings.UploadsRelative;
            string fullPath = Path.Combine("/var/www/", uploadsRelative, folderRecord.RelativePath);

            return Directory.Exists(fullPath);
        }

        public async Task<Response> UploadMediaStreamWithFolderIdAsync(HttpRequest request, string fileName, Guid? targetFolderId)
        {
            Response response = new Response();
            string uploadsRelative = string.IsNullOrWhiteSpace(_settings.UploadsRelative) ? "uploads" : _settings.UploadsRelative;
            string baseWebRoot = Path.Combine("/var/www/", uploadsRelative);

            string targetSubPath = string.Empty;
            if (targetFolderId.HasValue)
            {
                MediaFolder? folder = await _context.MediaFolders
                                                    .AsNoTracking()
                                                    .FirstOrDefaultAsync(f => f.Id == targetFolderId.Value);

                if (folder == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Target folder record not found in database.";
                    response.Code = HttpStatusCode.BadRequest;
                    return response;
                }
                targetSubPath = folder.RelativePath;
            }

            string finalDirectory = Path.Combine(baseWebRoot, targetSubPath);
            string fullPath = Path.Combine(finalDirectory, Path.GetFileName(fileName));

            MediaFile? existingFile = await _context.MediaFiles
                                                    .AsNoTracking()
                                                    .FirstOrDefaultAsync(mf => mf.FileName == fileName && mf.FolderId == targetFolderId);

            if (existingFile != null || File.Exists(fullPath))
            {
                response.IsSuccess = false;
                response.Message = "File already exists in this location.";
                response.Code = HttpStatusCode.Conflict;
                return response;
            }

            if (!Directory.Exists(finalDirectory))
            {
                Directory.CreateDirectory(finalDirectory);
            }
            try
            {
                await using (FileStream? fileStream = new FileStream(
                    fullPath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 1024 * 1024,
                    useAsync: true))
                {
                    await request.Body.CopyToAsync(fileStream);

                    MediaFile? mediaFile = new MediaFile
                    {
                        Id = Guid.NewGuid(),
                        FileName = fileName,
                        FilePath = fullPath,
                        Extension = Path.GetExtension(fileName).ToLowerInvariant(),
                        FileSize = fileStream.Length,
                        UploadedAt = DateTime.UtcNow,
                        FolderId = targetFolderId
                    };

                    _context.MediaFiles.Add(mediaFile);
                    await _context.SaveChangesAsync();

                    response.IsSuccess = true;
                    response.Message = "File streamed and saved successfully.";
                    response.Code = HttpStatusCode.Created;
                    response.Result.Add("fileId", JsonValue.Create(mediaFile.Id));
                    response.Result.Add("path", JsonValue.Create(targetSubPath));
                }
            }
            catch (Exception ex)
            {
                // Cleanup physical file if DB save or streaming fails
                if (File.Exists(fullPath)) File.Delete(fullPath);

                response.IsSuccess = false;
                response.Message = $"Something went wrong please try again later";
                response.Code = HttpStatusCode.InternalServerError;
            }

            return response;
        }

        public async Task<Response> GetPhysicalStructureWithIdsAsync()
        {
            Response? response = new Response();
            string uploadsRelative = _settings.UploadsRelative ?? "uploads";
            string rootPath = Path.Combine("/var/www/", uploadsRelative);

            if (!Directory.Exists(rootPath))
            {
                return new Response { IsSuccess = false, Message = "Uploads root not found", Code = HttpStatusCode.BadRequest };
            }

            Dictionary<string, Guid>? folderMap = await _context.MediaFolders
                                                            .AsNoTracking()
                                                            .ToDictionaryAsync(f => f.RelativePath.ToLower(), f => f.Id);
            Dictionary<string, DateTime>? folderDateMap = await _context.MediaFolders
                                                            .AsNoTracking()
                                                            .ToDictionaryAsync(f => f.RelativePath.ToLower(), f => f.CreatedAt);

            Dictionary<string, Guid>? fileMap = await _context.MediaFiles
                                                            .AsNoTracking()
                                                            .ToDictionaryAsync(f => f.FilePath.ToLower(), f => f.Id);

            Dictionary<string, DateTime> fileDateMap = await _context.MediaFiles
                                                            .AsNoTracking()
                                                            .ToDictionaryAsync(f => f.FilePath.ToLower(), f => f.UploadedAt);



            FileSystemNodeDto? tree = CrawlWithIds(rootPath, rootPath, folderMap, fileMap, folderDateMap, fileDateMap);

            response.IsSuccess = true;
            response.Code = HttpStatusCode.OK;
            response.Message = "Physical structure with IDs retrieved successfully.";
            response.Result.Add("assets", JsonSerializer.SerializeToNode(tree));
            return response;
        }

        private FileSystemNodeDto CrawlWithIds(
            string currentPath,
            string rootPath,
            Dictionary<string, Guid> folderMap,
            Dictionary<string, Guid> fileMap,
            Dictionary<string, DateTime> folderDateMap,
            Dictionary<string, DateTime> fileDateMap)
        {
            string relativePath = Path.GetRelativePath(rootPath, currentPath).Replace("\\", "/");
            if (relativePath == ".") relativePath = "";

            string folderKey = relativePath.ToLower();

            FileSystemNodeDto node = new()
            {
                Name = Path.GetFileName(currentPath),
                RelativePath = relativePath,
                IsFolder = true,
                Id = folderMap.TryGetValue(folderKey, out Guid folderId) ? folderId : null,
                UploadedAt = folderDateMap.TryGetValue(folderKey, out DateTime folderDate) ? folderDate : DateTime.MinValue
            };

            foreach (string dir in Directory.EnumerateDirectories(currentPath))
            {
                node.Children.Add(CrawlWithIds(dir, rootPath,folderMap, fileMap, folderDateMap, fileDateMap));
            }

            foreach (string filePath in Directory.EnumerateFiles(currentPath))
            {
                FileInfo fileInfo = new(filePath);
                string fileKey = filePath.ToLower();

                node.Children.Add(new FileSystemNodeDto
                {
                    Name = fileInfo.Name,
                    RelativePath = Path.GetRelativePath(rootPath, filePath).Replace("\\", "/"),
                    IsFolder = false,
                    Size = fileInfo.Length,
                    Extension = fileInfo.Extension.ToLowerInvariant(),
                    Id = fileMap.TryGetValue(fileKey, out Guid fileId) ? fileId : null,
                    UploadedAt = fileDateMap.TryGetValue(fileKey, out DateTime fileDate) ? fileDate : DateTime.MinValue
                });
            }

            node.Children.Sort((a, b) => b.UploadedAt.CompareTo(a.UploadedAt)); // newest first

            return node;
        }

        public async Task<Response> SyncFileSystemToDbAsync()
        {
            Response? response = new Response();
            string uploadsRelative = _settings.UploadsRelative ?? "uploads";
            string rootPath = Path.Combine("/var/www/", uploadsRelative);

            if (!Directory.Exists(rootPath))
                return new Response { IsSuccess = false, Message = "Root not found" };

            await DeepSync(rootPath, null, rootPath);

            response.IsSuccess = true;
            response.Message = "Database synchronized with VPS filesystem successfully.";
            response.Code = HttpStatusCode.OK;
            return response;
        }

        private async Task DeepSync(string currentPath, Guid? parentId, string rootPath)
        {
            foreach (string dir in Directory.GetDirectories(currentPath))
            {
                string folderName = Path.GetFileName(dir);
                string relPath = Path.GetRelativePath(rootPath, dir).Replace("\\", "/");

                MediaFolder? folder = await _context.MediaFolders
                                                    .FirstOrDefaultAsync(f => f.RelativePath == relPath);

                if (folder == null)
                {
                    folder = new MediaFolder
                    {
                        Id = Guid.NewGuid(),
                        Name = folderName,
                        RelativePath = relPath,
                        ParentFolderId = parentId
                    };
                    _context.MediaFolders.Add(folder);
                    await _context.SaveChangesAsync();
                }

                await DeepSync(dir, folder.Id, rootPath);
            }

            foreach (string file in Directory.GetFiles(currentPath))
            {
                string fileName = Path.GetFileName(file);
                bool existing = await _context.MediaFiles
                    .AnyAsync(f => f.FileName == fileName && f.FolderId == parentId);

                if (!existing)
                {
                    FileInfo? info = new FileInfo(file);
                    _context.MediaFiles.Add(new MediaFile
                    {
                        Id = Guid.NewGuid(),
                        FileName = fileName,
                        FilePath = file,
                        FileSize = info.Length,
                        Extension = info.Extension,
                        FolderId = parentId,
                        UploadedAt = DateTime.UtcNow
                    });
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task<Response> DeleteFolderAsync(Guid folderId)
        {
            Response? response = new Response();
            string uploadsRelative = _settings.UploadsRelative ?? "uploads";
            string baseWebRoot = Path.Combine("/var/www/", uploadsRelative);

            MediaFolder? folder = await _context.MediaFolders.FindAsync(folderId);

            if (folder == null)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "Folder not found in database.",
                    Code = HttpStatusCode.NotFound
                };
            }

            if (string.IsNullOrWhiteSpace(folder.RelativePath) || folder.RelativePath == "/" || folder.RelativePath == "courses")
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "System protected folders cannot be deleted.",
                    Code = HttpStatusCode.Forbidden
                };
            }

            string physicalPath = Path.Combine(baseWebRoot, folder.RelativePath);

            try
            {
                if (Directory.Exists(physicalPath))
                {
                    Directory.Delete(physicalPath, recursive: true);
                }

                _context.MediaFolders.Remove(folder);
                await _context.SaveChangesAsync();

                response.IsSuccess = true;
                response.Message = "Folder and all its contents deleted successfully.";
                response.Code = HttpStatusCode.OK;
            }
            catch (IOException ex)
            {
                response.IsSuccess = false;
                response.Message = $"File system error: A file might be in use. {ex.Message}";
                response.Code = HttpStatusCode.InternalServerError;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"An error occurred: {ex.Message}";
                response.Code = HttpStatusCode.InternalServerError;
            }

            return response;
        }

        public async Task<Response> DeleteFileAsync(Guid fileId)
        {
            MediaFile? fileRecord = await _context.MediaFiles.FindAsync(fileId);

            if (fileRecord == null)
                return new Response
                {
                    IsSuccess = false,
                    Message = "File not found.",
                    Code = HttpStatusCode.BadRequest
                };

            Response response = new();

            try
            {
                if (File.Exists(fileRecord.FilePath))
                    File.Delete(fileRecord.FilePath);

                _context.MediaFiles.Remove(fileRecord);
                await _context.SaveChangesAsync();

                response.IsSuccess = true;
                response.Message = "File deleted successfully.";
                response.Code = HttpStatusCode.OK;
            }
            catch (UnauthorizedAccessException ex)
            {
                response.IsSuccess = false;
                response.Message = $"Permission denied: {ex.Message}";
                response.Code = HttpStatusCode.InternalServerError;
            }
            catch (IOException ex)
            {
                response.IsSuccess = false;
                response.Message = $"File is in use or locked: {ex.Message}";
                response.Code = HttpStatusCode.InternalServerError;
            }

            return response;
        }

        public async Task<Response> RenameContentAsync(Guid id, string newName)
        {
            if (string.IsNullOrWhiteSpace(newName) || newName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                return new Response
                {
                    IsSuccess = false,
                    Message = "Invalid name.",
                    Code = HttpStatusCode.BadRequest
                };

            string rootPath = "/var/www/uploads";
            Response response = new();

            await using IDbContextTransaction tx = await _context.Database.BeginTransactionAsync();

            try
            {
                MediaFolder? folder = await _context.MediaFolders.FindAsync(id);

                if (folder != null)
                {
                    string parentRelative = Path.GetDirectoryName(folder.RelativePath.Replace("/", Path.DirectorySeparatorChar.ToString())) ?? "";
                    string newRelative = string.IsNullOrEmpty(parentRelative)? newName : $"{parentRelative.Replace(Path.DirectorySeparatorChar, '/')}/{newName}";
                    bool conflict = await _context.MediaFolders.AnyAsync(f => f.RelativePath == newRelative);

                    if (conflict)
                        return new Response
                        {
                            IsSuccess = false,
                            Message = "A folder with this name already exists here.",
                            Code = HttpStatusCode.Conflict
                        };

                    string oldPhysical = Path.Combine(rootPath, folder.RelativePath);
                    string newPhysical = Path.Combine(rootPath, newRelative);

                    if (Directory.Exists(oldPhysical))
                        Directory.Move(oldPhysical, newPhysical);

                    string oldRelative = folder.RelativePath;
                    folder.Name = newName;
                    folder.RelativePath = newRelative;

                    await UpdateChildPathsAsync(oldRelative, newRelative, rootPath);
                }
                else
                {
                    MediaFile? file = await _context.MediaFiles.FindAsync(id);

                    if (file == null)
                        return new Response
                        {
                            IsSuccess = false,
                            Message = "No folder or file found with that ID.",
                            Code = HttpStatusCode.BadRequest
                        };

                    string extension = Path.GetExtension(file.FilePath);
                    string directory = Path.GetDirectoryName(file.FilePath)!;
                    string newFilePath = Path.Combine(directory, newName + extension);

                    if (File.Exists(newFilePath))
                        return new Response
                        {
                            IsSuccess = false,
                            Message = "A file with this name already exists in this folder.",
                            Code = HttpStatusCode.Conflict
                        };

                    if (File.Exists(file.FilePath))
                        File.Move(file.FilePath, newFilePath);

                    file.FileName = newName + extension;
                    file.FilePath = newFilePath;

                    if (!string.IsNullOrEmpty(file.FilePath))
                    {
                        string fileDir = Path.GetDirectoryName(file.FilePath.Replace("/", Path.DirectorySeparatorChar.ToString())) ?? "";
                        file.FilePath = string.IsNullOrEmpty(fileDir) ? file.FileName : $"{fileDir.Replace(Path.DirectorySeparatorChar, '/')}/{file.FileName}";
                    }
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                response.IsSuccess = true;
                response.Message = "Renamed successfully.";
                response.Code = HttpStatusCode.OK;
            }
            catch (IOException ex)
            {
                await tx.RollbackAsync();
                response.IsSuccess = false;
                response.Message = $"File system error: {ex.Message}";
                response.Code = HttpStatusCode.InternalServerError;
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                response.IsSuccess = false;
                response.Message = $"Rename failed: {ex.Message}";
                response.Code = HttpStatusCode.InternalServerError;
            }

            return response;
        }

        public async Task<Response> MoveContentAsync(Guid id, Guid? newParentFolderId)
        {
            string rootPath = "/var/www/uploads";
            Response response = new();

            await using IDbContextTransaction tx = await _context.Database.BeginTransactionAsync();

            try
            {
                string destRelative = string.Empty;
                if (newParentFolderId.HasValue)
                {
                    MediaFolder? destFolder = await _context.MediaFolders.FindAsync(newParentFolderId.Value);

                    if (destFolder == null)
                        return new Response
                        {
                            IsSuccess = false,
                            Message = "Destination folder not found.",
                            Code = HttpStatusCode.NotFound
                        };

                    destRelative = destFolder.RelativePath;
                }

                string destPhysical = string.IsNullOrEmpty(destRelative) ? rootPath : Path.Combine(rootPath, destRelative);

                MediaFolder? folder = await _context.MediaFolders.FindAsync(id);

                if (folder != null)
                {
                    string candidateRelative = string.IsNullOrEmpty(destRelative) ? folder.Name : $"{destRelative}/{folder.Name}";

                    if (candidateRelative.StartsWith(folder.RelativePath + "/",
                            StringComparison.OrdinalIgnoreCase) ||
                        candidateRelative.Equals(folder.RelativePath,
                            StringComparison.OrdinalIgnoreCase))
                        return new Response
                        {
                            IsSuccess = false,
                            Message = "Cannot move a folder into itself or a descendant.",
                            Code = HttpStatusCode.BadRequest
                        };

                    string oldPhysical = Path.Combine(rootPath, folder.RelativePath);
                    string newPhysical = Path.Combine(destPhysical, folder.Name);

                    if (Directory.Exists(newPhysical))
                        return new Response
                        {
                            IsSuccess = false,
                            Message = "A folder with this name already exists at the destination.",
                            Code = HttpStatusCode.Conflict
                        };

                    Directory.Move(oldPhysical, newPhysical);

                    string oldRelative = folder.RelativePath;
                    folder.RelativePath = candidateRelative;
                    folder.ParentFolderId = newParentFolderId;

                    await UpdateChildPathsAsync(oldRelative, candidateRelative, rootPath);
                }
                else
                {
                    MediaFile? file = await _context.MediaFiles.FindAsync(id);

                    if (file == null)
                        return new Response
                        {
                            IsSuccess = false,
                            Message = "No folder or file found with that ID.",
                            Code = HttpStatusCode.BadRequest
                        };

                    string newFilePath = Path.Combine(destPhysical, file.FileName);

                    if (File.Exists(newFilePath))
                        return new Response
                        {
                            IsSuccess = false,
                            Message = "A file with this name already exists at the destination.",
                            Code = HttpStatusCode.Conflict
                        };

                    File.Move(file.FilePath, newFilePath);

                    file.FilePath = newFilePath;
                    file.FolderId = newParentFolderId;
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                response.IsSuccess = true;
                response.Message = "Moved successfully.";
                response.Code = HttpStatusCode.OK;
            }
            catch (IOException ex)
            {
                await tx.RollbackAsync();
                response.IsSuccess = false;
                response.Message = $"File system error: {ex.Message}";
                response.Code = HttpStatusCode.InternalServerError;
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                response.IsSuccess = false;
                response.Message = $"Move failed: {ex.Message}";
                response.Code = HttpStatusCode.InternalServerError;
            }

            return response;
        }

        private async Task UpdateChildPathsAsync(string oldRelative, string newRelative, string rootPath)
        {
            string oldPrefix = oldRelative + "/";
            string newPrefix = newRelative + "/";

            List<MediaFolder> childFolders = await _context.MediaFolders
                .Where(f => f.RelativePath.StartsWith(oldPrefix))
                .ToListAsync();

            foreach (MediaFolder f in childFolders)
                f.RelativePath = newPrefix + f.RelativePath[oldPrefix.Length..];

            string oldPhysicalPrefix = Path.Combine(rootPath, oldRelative) + Path.DirectorySeparatorChar;
            string newPhysicalPrefix = Path.Combine(rootPath, newRelative) + Path.DirectorySeparatorChar;

            List<MediaFile> childFiles = await _context.MediaFiles
                .Where(f => f.FilePath.StartsWith(oldPhysicalPrefix))
                .ToListAsync();

            foreach (MediaFile f in childFiles)
            {
                f.FilePath = newPhysicalPrefix + f.FilePath[oldPhysicalPrefix.Length..];

                if (!string.IsNullOrEmpty(f.FilePath))
                    f.FilePath = newPrefix + f.FilePath[oldPrefix.Length..];
            }
        }
    }
}