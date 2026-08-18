using EdgePMO.API.Dtos;

namespace EdgePMO.API.Contracts
{
    public interface ICourseServices
    {
        Task<Response> AttachCourseVideoAsync(CourseVideoCreateDto dto);

        Task<Response> CreateAsync(CourseCreateDto dto);

        Task<Response> DeleteAsync(Guid id);

        Task<Response> DeleteCourseVideoAsync(Guid courseVideoId);

        Task<Response> EnrollUsersByEmailsAsync(Guid courseId, IEnumerable<string> email, decimal? amountOverride = null, string? currency = null);

        Task<Response> GetAllAsync(Guid? currentUserId, bool isAdmin);

        Task<Response> GetByIdAsync(Guid id, Guid? currentUserId, bool isAdmin);

        Task<Response> ReorderAsync(List<Guid> orderedCourseIds);

        Task<Response> GetEnrolledUsersAsync(Guid courseId);

        Task<Response> IsUsersEnrolledAsync(Guid courseId, IEnumerable<string> emails);

        Task<Response> UnenrollUsersByEmailsAsync(Guid courseId, IEnumerable<string> emails);

        Task<Response> UpdateAsync(CourseUpdateDto dto);

        Task<Response> UpdateCourseVideoAsync(CourseVideoUpdateDto dto);

        Task<Response> UpdateUserProgressAsync(Guid userId, Guid courseId, double progress);

        /// <summary>Requirement 3.5 — upserts how far a user has watched into a video.</summary>
        Task<Response> UpdateVideoWatchProgressAsync(Guid userId, Guid videoId, double watchedSeconds);

        /// <summary>Requirement 5.2 — per-video view counts and watch-time for admins.</summary>
        Task<Response> GetVideoAnalyticsAsync(Guid courseId);
    }
}