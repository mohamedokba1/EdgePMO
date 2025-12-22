using AutoMapper;
using EdgePMO.API.Contracts;
using EdgePMO.API.Dtos;
using EdgePMO.API.Dtos.Courses;
using EdgePMO.API.Models;

namespace EdgePMO.API.Services
{
    public class CourseReviewServices : ICourseContentServices
    {
        private readonly EdgepmoDbContext _context;
        private readonly IMapper _mapper;

        public CourseReviewServices(EdgepmoDbContext context)
        {
            _context = context;
        }
        public Task<Response> CreateAsync(CourseVideoCreateDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<Response> CreateCourseDocuemnt(CourseDocumentCreateDto dto, Guid outlineId)
        {
            throw new NotImplementedException();
        }

        public Task<Response> CreateCourseOutline(CourseContentDto dto, Guid courseId)
        {
            throw new NotImplementedException();
        }

        public Task<Response> CreateCourseVideo(CourseVideoCreateDto dto, Guid outlineId)
        {
            throw new NotImplementedException();
        }

        public Task<Response> DeleteAsync(Guid id, bool deleteFile = false)
        {
            throw new NotImplementedException();
        }

        public Task<Response> DeleteCourseDocuemnt(Guid id, bool deleteFile = false)
        {
            throw new NotImplementedException();
        }

        public Task<Response> DeleteCourseVideo(Guid id, bool deleteFile = false)
        {
            throw new NotImplementedException();
        }

        public Task<Response> GetByCourseIdAsync(Guid courseId)
        {
            throw new NotImplementedException();
        }

        public Task<Response> GetByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<Response> GetCourseOutlinesByCourseId(Guid courseId)
        {
            throw new NotImplementedException();
        }

        public Task<Response> UpdateAsync(Guid id, CourseVideoCreateDto dto)
        {
            throw new NotImplementedException();
        }
    }
}
