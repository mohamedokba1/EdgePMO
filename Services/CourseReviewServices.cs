using AutoMapper;
using EdgePMO.API.Contracts;
using EdgePMO.API.Dtos;
using EdgePMO.API.Dtos.Courses;
using EdgePMO.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace EdgePMO.API.Services
{
    public class CourseReviewServices : ICourseReviewServices
    {
        private readonly EdgepmoDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<CourseReviewServices> _logger;

        public CourseReviewServices(EdgepmoDbContext context, IMapper mapper, ILogger<CourseReviewServices> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Response> CreateAsync(CreateCourseReviewDto dto)
        {
            Response response = new Response();
            Course? course = await _context.Courses.FindAsync(dto.CourseId);
            if(course is null)
            {
                response.IsSuccess = false;
                response.Message = "Course not found";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }
            CourseReview? newCourseReview = new CourseReview()
            {
                CourseId = dto.CourseId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Header = dto.Header,
                Rating = dto.Rating.HasValue ? dto.Rating.Value : 5,
                Content = dto.Content,
                UserId = dto.UserId
            };

            await _context.Set<CourseReview>().AddAsync(newCourseReview);
            int rowsAffected =  await _context.SaveChangesAsync();

            if(rowsAffected > 0)
            {
                response.IsSuccess = true;
                response.Message = "New course review created successfully!";
                response.Code = HttpStatusCode.Created;
            }
            else
            {
                response.IsSuccess = false;
                response.Message = "Can not create a new course review";
                response.Code = HttpStatusCode.BadRequest;
            }
            return response;
        }

        public async Task<Response> DeleteAsync(Guid id)
        {
            Response response = new Response();

            CourseReview? courseReview = await _context.CourseReviews.FindAsync(id);
            if (courseReview != null)
            {
                _context.CourseReviews.Remove(courseReview);
                await _context.SaveChangesAsync();
                response.IsSuccess = true;
                response.Message = $"Course review with id = {id} deleted";
                response.Code = HttpStatusCode.NoContent;
            }
            else
            {
                response.IsSuccess= false;
                response.Message = $"Course review with id = {id} not found";
                response.Code = HttpStatusCode.BadRequest;
            }
            return response;
        }

        public async Task<Response> SetHiddenAsync(Guid id, bool isHidden)
        {
            Response response = new Response();

            CourseReview? courseReview = await _context.CourseReviews.FindAsync(id);
            if (courseReview == null)
            {
                response.IsSuccess = false;
                response.Message = $"Course review with id = {id} not found";
                response.Code = HttpStatusCode.NotFound;
                return response;
            }

            courseReview.IsHidden = isHidden;
            courseReview.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            response.IsSuccess = true;
            response.Message = isHidden ? "Review hidden." : "Review unhidden.";
            response.Code = HttpStatusCode.OK;
            return response;
        }

        public async Task<Response> GetAllAsync()
        {
            Response response = new Response();

            try
            {
                // Deliberately NOT querying _context.CourseReviews directly — live-reproduced
                // against staging (twice, including after a redeploy) that
                // _context.CourseReviews.Include(cr => cr.User) crashes the connection outright
                // (not a catchable .NET exception — curl sees "Unsupported HTTP/1 subversion in
                // response", i.e. the response is corrupted below the app layer), regardless of
                // whether any rows/User rows are actually involved. The identical User join
                // reached via Courses.Include(Reviews).ThenInclude(User) works fine (confirmed
                // live), so routing through that navigation instead sidesteps whatever is wrong
                // with the direct DbSet<CourseReview> query shape.
                List<Course> courses = await _context.Courses
                                                                .AsNoTracking()
                                                                .Include(c => c.Reviews)
                                                                    .ThenInclude(cr => cr.User)
                                                                .ToListAsync();
                List<CourseReview> listOfCourseReviews = courses.SelectMany(c => c.Reviews).ToList();
                response.IsSuccess = true;
                response.Message = $"All course reviews retrieved successfully!";
                // Was serializing the raw entity graph (including the full User navigation —
                // PasswordHash/PasswordSalt/RefreshToken and all) via JsonSerializer.SerializeToNode.
                // Besides being a real data-exposure risk, that raw graph is also a likely source
                // of the "Failed to fetch" / net::ERR_FAILED reproduced live on the sibling
                // GetByCourseIdAsync — a mapped, flat DTO has neither problem.
                List<CourseReviewReadDto> reviewDtos = _mapper.Map<List<CourseReviewReadDto>>(listOfCourseReviews);
                response.Result.Add("reviews", JsonSerializer.SerializeToNode(reviewDtos) ?? JsonValue.Create(Array.Empty<object>()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAllAsync (course reviews) failed");
                response.IsSuccess = false;
                response.Message = "Could not load reviews. Please try again later.";
                response.Code = HttpStatusCode.InternalServerError;
            }

            return response;
        }

        public async Task<Response> GetByCourseIdAsync(Guid courseId)
        {
            Response response = new Response();

            try
            {
                // See GetAllAsync for why this goes through Courses instead of querying
                // _context.CourseReviews directly.
                Course? course = await _context.Courses
                                                                .AsNoTracking()
                                                                .Where(c => c.CourseId == courseId)
                                                                .Include(c => c.Reviews)
                                                                    .ThenInclude(cr => cr.User)
                                                                .FirstOrDefaultAsync();
                List<CourseReview> listOfCourseReviews = course?.Reviews ?? new List<CourseReview>();
                response.IsSuccess = true;
                response.Message = $"All course reviews retrieved successfully!";
                List<CourseReviewReadDto> reviewDtos = _mapper.Map<List<CourseReviewReadDto>>(listOfCourseReviews);
                response.Result.Add("reviews", JsonSerializer.SerializeToNode(reviewDtos) ?? JsonValue.Create(Array.Empty<object>()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByCourseIdAsync (course reviews) failed for courseId={CourseId}", courseId);
                response.IsSuccess = false;
                response.Message = "Could not load reviews for this course. Please try again later.";
                response.Code = HttpStatusCode.InternalServerError;
            }

            return response;
        }

        public async Task<Response> GetByIdAsync(Guid id)
        {
            Response response = new Response();

            try
            {
                // See GetAllAsync for why this goes through Courses instead of querying
                // _context.CourseReviews directly. We don't know the CourseId up front here,
                // so first resolve it with a plain scalar projection (no Include at all, so it
                // can't hit the crashing join), then reuse the Courses.Include(Reviews)
                // .ThenInclude(User) path that's confirmed safe.
                Guid? courseId = await _context.CourseReviews
                                                                .AsNoTracking()
                                                                .Where(cr => cr.Id == id)
                                                                .Select(cr => (Guid?)cr.CourseId)
                                                                .FirstOrDefaultAsync();

                CourseReview? courseReview = null;
                if (courseId.HasValue)
                {
                    Course? course = await _context.Courses
                                                                .AsNoTracking()
                                                                .Where(c => c.CourseId == courseId.Value)
                                                                .Include(c => c.Reviews.Where(r => r.Id == id))
                                                                    .ThenInclude(cr => cr.User)
                                                                .FirstOrDefaultAsync();
                    courseReview = course?.Reviews.FirstOrDefault(r => r.Id == id);
                }
                response.IsSuccess = true;
                response.Message = $"Course review retrieved successfully!";
                CourseReviewReadDto? reviewDto = courseReview != null ? _mapper.Map<CourseReviewReadDto>(courseReview) : null;
                response.Result.Add("reviews", JsonSerializer.SerializeToNode(reviewDto) ?? JsonValue.Create(Array.Empty<object>()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByIdAsync (course review) failed for id={Id}", id);
                response.IsSuccess = false;
                response.Message = "Could not load this review. Please try again later.";
                response.Code = HttpStatusCode.InternalServerError;
            }

            return response;
        }

        public async Task<Response> UpdateAsync(UpdateCourseReviewDto dto)
        {
            Response response = new Response();

            CourseReview? courseReview = await _context.CourseReviews
                                                .Where(cr => cr.Id == dto.Id)
                                                .FirstOrDefaultAsync();

            if (courseReview != null)
            {
                if(!string.IsNullOrEmpty(dto.Header) && !string.IsNullOrWhiteSpace(dto.Header))
                {
                    courseReview.Header = dto.Header;               
                }

                if (dto.Rating.HasValue)
                {
                    courseReview.Rating = dto.Rating.Value;
                }

                if (!string.IsNullOrEmpty(dto.Content) && !string.IsNullOrWhiteSpace(dto.Content))
                {
                    courseReview.Content = dto.Content;
                }
                courseReview.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                response.IsSuccess = true;
                response.Message = $"Course review with id = {dto.Id} updated successfully";
                response.Code = HttpStatusCode.OK;
                
            }
            else
            {
                response.IsSuccess = false;
                response.Message = $"Course review with id = {dto.Id} not found";
                response.Code = HttpStatusCode.BadRequest;
            }

            return response;
        }
    }
}
