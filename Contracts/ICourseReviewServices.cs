using EdgePMO.API.Dtos;
using EdgePMO.API.Dtos.Courses;

namespace EdgePMO.API.Contracts
{
    public interface ICourseReviewServices
    {
        Task<Response> GetAllAsync();
        Task<Response> GetByIdAsync(Guid id);
        Task<Response> GetByCourseIdAsync(Guid courseId);
        Task<Response> CreateAsync(CreateCourseReviewDto dto);
        Task<Response> UpdateAsync(UpdateCourseReviewDto dto);
        Task<Response> DeleteAsync(Guid id);
    }
}
