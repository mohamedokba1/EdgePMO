using EdgePMO.API.Contracts;
using EdgePMO.API.Dtos;
using EdgePMO.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace EdgePMO.API.Services
{
    public class CourseVideoServices : ICourseVideoServices
    {
        private readonly EdgepmoDbContext _context;
        private readonly IContentServices _contentServices;

        public CourseVideoServices(EdgepmoDbContext context, IContentServices contentServices)
        {
            _context = context;
            _contentServices = contentServices;
        }

        public async Task<Response> GetByCourseIdAsync(Guid courseId)
        {
            Response response = new Response();

            Course? course = await _context.Courses
                .AsNoTracking()
                .Include(c => c.CourseVideos)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course == null)
            {
                response.IsSuccess = false;
                response.Message = "Course not found.";
                response.Code = HttpStatusCode.NotFound;
                return response;
            }

            List<CourseVideo>? videos = course.CourseVideos.OrderBy(v => v.Order).ToList();


            response.IsSuccess = true;
            response.Message = "Videos retrieved.";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("videos", JsonSerializer.SerializeToNode(videos) ?? JsonValue.Create(Array.Empty<object>()));
            return response;
        }

        public async Task<Response> GetByIdAsync(Guid id)
        {
            Response response = new Response();

            var video = await _context.CourseVideos
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.CourseVideoId == id);

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
                CourseId = dto.CourseId,
                Title = dto.Title?.Trim(),
                Description = dto.Description?.Trim(),
                VideoUrl = dto.VideoUrl?.Trim(),
                DurationSeconds = dto.DurationSeconds,
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
            if (existing.CourseId != dto.CourseId)
            {
                Course? course = await _context.Courses.FindAsync(dto.CourseId);
                if (course == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Target course not found.";
                    response.Code = HttpStatusCode.BadRequest;
                    return response;
                }
            }

            existing.CourseId = dto.CourseId;
            existing.Title = dto.Title?.Trim();
            existing.Description = dto.Description?.Trim();
            existing.VideoUrl = dto.VideoUrl?.Trim();
            existing.DurationSeconds = dto.DurationSeconds;
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

            string? videoUrl = existing.VideoUrl;

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
    }
}