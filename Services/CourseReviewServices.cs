using AutoMapper;
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
    public class CourseReviewServices : ICourseReviewServices
    {
        private readonly EdgepmoDbContext _context;
        private readonly IMapper _mapper;

        public CourseReviewServices(EdgepmoDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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

        public async Task<Response> GetAllAsync()
        {
            Response response = new Response();

            List<CourseReview>? listOfCourseReviews = await _context.CourseReviews
                                                            .AsNoTracking()
                                                            .Include(cr => cr.User)
                                                            .Include(cr => cr.Course).ToListAsync();
            response.IsSuccess = true;
            response.Message = $"All course reviews retrieved successfully!";
            response.Result.Add("reviews", JsonSerializer.SerializeToNode(listOfCourseReviews) ?? JsonValue.Create(Array.Empty<object>()));

            return response;
        }

        public async Task<Response> GetByCourseIdAsync(Guid courseId)
        {
            Response response = new Response();

            List<CourseReview>? listOfCourseReviews = await _context.CourseReviews
                                                            .AsNoTracking()
                                                            .Where(cr => cr.CourseId == courseId)
                                                            .Include(cr => cr.User)
                                                            .ToListAsync();
            response.IsSuccess = true;
            response.Message = $"All course reviews retrieved successfully!";
            response.Result.Add("reviews", JsonSerializer.SerializeToNode(listOfCourseReviews) ?? JsonValue.Create(Array.Empty<object>()));

            return response;
        }

        public async Task<Response> GetByIdAsync(Guid id)
        {
            Response response = new Response();

            CourseReview? courseReview = await _context.CourseReviews
                                                            .AsNoTracking()
                                                            .Where(cr => cr.Id == id)
                                                            .Include(cr => cr.User)
                                                            .Include(cr => cr.Course)
                                                            .FirstOrDefaultAsync();
            response.IsSuccess = true;
            response.Message = $"Course review retrieved successfully!";
            response.Result.Add("reviews", JsonSerializer.SerializeToNode(courseReview) ?? JsonValue.Create(Array.Empty<object>()));

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
