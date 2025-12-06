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
        Task<Response> DeleteCourseVideoAsync(Guid courseVideoId);
        Task<Response> GetEnrolledUsersAsync(Guid courseId);
        Task<Response> EnrollUsersByEmailsAsync(Guid courseId, IEnumerable<string> email);
        Task<Response> UnenrollUsersByEmailsAsync(Guid courseId, IEnumerable<string> emails);
        Task<Response> IsUsersEnrolledAsync(Guid courseId, IEnumerable<string> emails);
    }
}
