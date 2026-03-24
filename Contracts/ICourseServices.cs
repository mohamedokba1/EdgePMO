using EdgePMO.API.Dtos;

namespace EdgePMO.API.Contracts
{
    public interface ICourseServices
    {
        Task<Response> AttachCourseVideoAsync(CourseVideoCreateDto dto);

        Task<Response> CreateAsync(CourseCreateDto dto);

        Task<Response> DeleteAsync(Guid id);

        Task<Response> DeleteCourseVideoAsync(Guid courseVideoId);

        Task<Response> EnrollUsersByEmailsAsync(Guid courseId, IEnumerable<string> email);

        Task<Response> GetAllAsync();

        Task<Response> GetByIdAsync(Guid id);

        Task<Response> GetEnrolledUsersAsync(Guid courseId);

        Task<Response> IsUsersEnrolledAsync(Guid courseId, IEnumerable<string> emails);

        Task<Response> UnenrollUsersByEmailsAsync(Guid courseId, IEnumerable<string> emails);

        Task<Response> UpdateAsync(CourseUpdateDto dto);

        Task<Response> UpdateCourseVideoAsync(CourseVideoUpdateDto dto);

        Task<Response> UpdateUserProgressAsync(Guid userId, Guid courseId, double progress);
    }
}