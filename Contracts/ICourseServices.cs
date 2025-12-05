using EdgePMO.API.Dtos;

namespace EdgePMO.API.Contracts
{
    public interface ICourseServices
    {
        Task<Response> GetAllAsync();
        Task<Response> GetByIdAsync(Guid id);
        Task<Response> CreateAsync(CourseCreateDto dto);
        Task<Response> UpdateAsync(CourseUpdateDto dto);
        Task<Response> DeleteAsync(Guid id);
        Task<Response> AttachCourseVideoAsync(CourseVideoCreateDto dto);
        Task<Response> GetEnrolledUsersAsync(Guid courseId);
        Task<Response> EnrollUserAsync(Guid courseId, Guid userId);
        Task<Response> UnenrollUserAsync(Guid courseId, Guid userId);
        Task<Response> IsUserEnrolledAsync(Guid courseId, Guid userId);
    }
}
