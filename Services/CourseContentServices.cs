using EdgePMO.API.Contracts;
using EdgePMO.API.Dtos;
using EdgePMO.API.Dtos.Courses;
using EdgePMO.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace EdgePMO.API.Services
{
    public class CourseContentServices : ICourseContentServices
    {
        private readonly EdgepmoDbContext _context;
        private readonly IContentServices _contentServices;

        public CourseContentServices(EdgepmoDbContext context, IContentServices contentServices)
        {
            _context = context;
            _contentServices = contentServices;
        }

        public async Task<Response> GetByCourseIdAsync(Guid courseId)
        {
            Response response = new Response();

            Course? course = await _context.Courses
                .AsNoTracking()
                .Include(c => c.CourseOutline)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course == null)
            {
                response.IsSuccess = false;
                response.Message = "Course not found.";
                response.Code = HttpStatusCode.NotFound;
                return response;
            }

            //List<CourseVideo>? videos = course.CourseOutline.OrderBy(v => v.Order).ToList();


            response.IsSuccess = true;
            response.Message = "Videos retrieved.";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("videos", JsonSerializer.SerializeToNode(course) ?? JsonValue.Create(Array.Empty<object>()));
            return response;
        }

        public async Task<Response> GetByIdAsync(Guid id)
        {
            Response response = new Response();

            var video = await _context.CourseVideos
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == id);

            if (video == null)
            {
                response.IsSuccess = false;
                response.Message = "Video not found.";
                response.Code = HttpStatusCode.NotFound;
                return response;
            }

            response.IsSuccess = true;
            response.Message = "Video retrieved.";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("video", JsonSerializer.SerializeToNode(video) ?? JsonValue.Create(new { }));
            return response;
        }

        public async Task<Response> CreateAsync(CourseVideoCreateDto dto)
        {
            Response response = new Response();

            Course? course = await _context.Courses.FindAsync(dto.CourseId);
            if (course == null)
            {
                response.IsSuccess = false;
                response.Message = "Course not found.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            CourseVideo? video = new CourseVideo
            {
                //CourseId = dto.CourseId,
                Title = dto.Title?.Trim(),
                Description = dto.Description?.Trim(),
                Url = dto.Url?.Trim(),
                DurationMinutes = dto.DurationSeconds,
                Order = dto.Order
            };

            _context.CourseVideos.Add(video);
            await _context.SaveChangesAsync();

            response.IsSuccess = true;
            response.Message = "Course video created.";
            response.Code = HttpStatusCode.Created;
            response.Result.Add("courseVideo", JsonSerializer.SerializeToNode(video) ?? JsonValue.Create(new { }));
            return response;
        }

        public async Task<Response> UpdateAsync(Guid id, CourseVideoCreateDto dto)
        {
            Response response = new Response();

            CourseVideo? existing = await _context.CourseVideos.FindAsync(id);
            if (existing == null)
            {
                response.IsSuccess = false;
                response.Message = "Video not found.";
                response.Code = HttpStatusCode.NotFound;
                return response;
            }

            // validate course if changed
            //if (existing.CourseId != dto.CourseId)
            //{
            //    Course? course = await _context.Courses.FindAsync(dto.CourseId);
            //    if (course == null)
            //    {
            //        response.IsSuccess = false;
            //        response.Message = "Target course not found.";
            //        response.Code = HttpStatusCode.BadRequest;
            //        return response;
            //    }
            //}

            //existing.CourseId = dto.CourseId;
            existing.Title = dto.Title?.Trim();
            existing.Description = dto.Description?.Trim();
            existing.Url = dto.Url?.Trim();
            existing.DurationMinutes = dto.DurationSeconds;
            existing.Order = dto.Order;

            await _context.SaveChangesAsync();

            response.IsSuccess = true;
            response.Message = "Course video updated.";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("courseVideo", JsonSerializer.SerializeToNode(existing) ?? JsonValue.Create(new { }));
            return response;
        }

        public async Task<Response> DeleteAsync(Guid id, bool deleteFile = false)
        {
            Response response = new Response();

            CourseVideo? existing = await _context.CourseVideos.FindAsync(id);
            if (existing == null)
            {
                response.IsSuccess = false;
                response.Message = "Video not found.";
                response.Code = HttpStatusCode.NotFound;
                return response;
            }

            string? videoUrl = existing.Url;

            _context.CourseVideos.Remove(existing);
            await _context.SaveChangesAsync();

            if (deleteFile && !string.IsNullOrWhiteSpace(videoUrl))
            {
                try
                {
                    string fileName = Path.GetFileName(videoUrl);
                    if (!string.IsNullOrWhiteSpace(fileName))
                    {
                        await _contentServices.DeleteAssetAsync(fileName);
                    }
                }
                catch
                {
                    // ignore deletion errors; record is already removed
                }
            }

            response.IsSuccess = true;
            response.Message = "Course video deleted.";
            response.Code = HttpStatusCode.NoContent;
            return response;
        }

        public async Task<Response> CreateCourseOutline(CourseContentDto dto, Guid courseId)
        {
            Response response = new Response();

            Course? course = await _context.Courses.FindAsync(courseId);
            if (course == null)
            {
                response.IsSuccess = false;
                response.Message = "Course not found.";
                response.Code = HttpStatusCode.NotFound;
                return response;
            }

            CourseOutline? outline = new CourseOutline
            {
                Id = Guid.NewGuid(),
                CourseId = courseId,
                Title = dto.Title,
                Order = dto.Order,
                CreatedAt = DateTime.UtcNow
            };

            _context.CourseOutlines.Add(outline);
            await _context.SaveChangesAsync();

            response.IsSuccess = true;
            response.Message = "Course outline created.";
            response.Code = HttpStatusCode.Created;
            response.Result.Add("outlineId", JsonSerializer.SerializeToNode(outline.Id) ?? JsonValue.Create(Array.Empty<object>()));
            return response;
        }

        public async Task<Response> GetCourseOutlinesByCourseId(Guid courseId)
        {
            Response response = new Response();

            bool exists = await _context.Courses.AnyAsync(c => c.CourseId == courseId);
            if (!exists)
            {
                response.IsSuccess = false;
                response.Message = "Course not found.";
                response.Code = HttpStatusCode.NotFound;
                return response;
            }

            List<CourseOutline>? outlines = await _context.CourseOutlines
                .AsNoTracking()
                .Where(o => o.CourseId == courseId)
                .Include(o => o.Videos)
                .Include(o => o.Documents)
                .OrderBy(o => o.Order)
                .ToListAsync();

            response.IsSuccess = true;
            response.Message = "Course outlines retrieved.";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("outlines", JsonSerializer.SerializeToNode(outlines) ?? JsonValue.Create(Array.Empty<object>()));
            return response;
        }

        public async Task<Response> CreateCourseVideo(CourseVideoCreateDto dto, Guid outlineId)
        {
            Response response = new Response();

            CourseOutline? outline = await _context.CourseOutlines.FindAsync(outlineId);
            if (outline == null)
            {
                response.IsSuccess = false;
                response.Message = "Outline section not found.";
                response.Code = HttpStatusCode.NotFound;
                return response;
            }

            string fileName = Path.GetFileName(dto.Url ?? string.Empty);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                response.IsSuccess = false;
                response.Message = "Invalid file name or URL.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            string ext = Path.GetExtension(fileName).ToLowerInvariant();
            if (ext != ".mp4")
            {
                response.IsSuccess = false;
                response.Message = "Only .mp4 files are allowed.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            //bool exists = await _contentServices.FileExistsAsync(fileName);
            //if (!exists)
            //{
            //    response.IsSuccess = false;
            //    response.Message = "Uploaded file not found.";
            //    response.Code = HttpStatusCode.BadRequest;
            //    return response;
            //}

            CourseVideo? video = new CourseVideo
            {
                CourseOutlineId = outlineId,
                Title = dto.Title?.Trim(),
                Description = dto.Description?.Trim(),
                Url = dto.Url?.Replace("\\", "/"),
                DurationMinutes = dto.DurationSeconds,
                Order = dto.Order
            };

            _context.CourseVideos.Add(video);
            await _context.SaveChangesAsync();

            //var videoDto = _mapper.Map<CourseVideoReadDto>(video);
            response.IsSuccess = true;
            response.Message = "Course video created under outline.";
            response.Code = HttpStatusCode.Created;
            //response.Result.Add("courseVideo", JsonSerializer.SerializeToNode(video) ?? JsonValue.Create(new { }));
            return response;
        }

        public async Task<Response> CreateCourseDocuemnt(CourseDocumentCreateDto dto, Guid outlineId)
        {
            Response response = new Response();

            CourseOutline? outline = await _context.CourseOutlines.FindAsync(outlineId);
            if (outline == null)
            {
                response.IsSuccess = false;
                response.Message = "Outline section not found.";
                response.Code = HttpStatusCode.NotFound;
                return response;
            }

            string fileName = Path.GetFileName(dto.Url ?? string.Empty);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                response.IsSuccess = false;
                response.Message = "Invalid file path.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            //bool exists = await _contentServices.FileExistsAsync(fileName);
            //if (!exists)
            //{
            //    response.IsSuccess = false;
            //    response.Message = "Uploaded file not found.";
            //    response.Code = HttpStatusCode.BadRequest;
            //    return response;
            //}

            CourseDocument? doc = new CourseDocument
            {
                CourseOutlineId = outlineId,
                Title = dto.Title,
                Description = dto.Description,
                DocumentUrl = dto.Url,
                CreatedAt = DateTime.UtcNow
            };

            _context.CourseDocuments.Add(doc);
            await _context.SaveChangesAsync();


            response.IsSuccess = true;
            response.Message = "Course document created under outline.";
            response.Code = HttpStatusCode.Created;
            //response.Result.Add("document", JsonSerializer.SerializeToNode(doc) ?? JsonValue.Create(new { }));
            return response;
        }

        public async Task<Response> DeleteCourseVideo(Guid id, bool deleteFile = false)
        {
            Response response = new Response();

            CourseVideo? existing = await _context.CourseVideos.FindAsync(id);
            if (existing == null)
            {
                response.IsSuccess = false;
                response.Message = "Video not found.";
                response.Code = HttpStatusCode.NotFound;
                return response;
            }

            string? videoUrl = existing.Url;

            _context.CourseVideos.Remove(existing);
            await _context.SaveChangesAsync();

            if (deleteFile && !string.IsNullOrWhiteSpace(videoUrl))
            {
                try
                {
                    string fileName = Path.GetFileName(videoUrl);
                    if (!string.IsNullOrWhiteSpace(fileName))
                    {
                        await _contentServices.DeleteAssetAsync(fileName);
                    }
                }
                catch
                {
                    // ignore deletion errors; record is already removed
                }
            }

            response.IsSuccess = true;
            response.Message = "Course video deleted.";
            response.Code = HttpStatusCode.NoContent;
            return response;
        }

        public async Task<Response> DeleteCourseDocuemnt(Guid id, bool deleteFile = false)
        {
            Response response = new Response();

            CourseDocument? existing = await _context.CourseDocuments.FindAsync(id);
            if (existing == null)
            {
                response.IsSuccess = false;
                response.Message = "Document not found.";
                response.Code = HttpStatusCode.NotFound;
                return response;
            }

            string? filePath = existing.DocumentUrl;

            _context.CourseDocuments.Remove(existing);
            await _context.SaveChangesAsync();

            if (deleteFile && !string.IsNullOrWhiteSpace(filePath))
            {
                try
                {
                    string fileName = Path.GetFileName(filePath);
                    if (!string.IsNullOrWhiteSpace(fileName))
                    {
                        await _contentServices.DeleteAssetAsync(fileName);
                    }
                }
                catch
                {
                    // ignore deletion errors; record is already removed
                }
            }

            response.IsSuccess = true;
            response.Message = "Course document deleted.";
            response.Code = HttpStatusCode.NoContent;
            return response;
        }
    }
}