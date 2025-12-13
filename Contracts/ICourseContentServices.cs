using EdgePMO.API.Dtos;
using EdgePMO.API.Dtos.Courses;

namespace EdgePMO.API.Contracts
{
    public interface ICourseContentServices
    {
        Task<Response> GetByCourseIdAsync(Guid courseId);
        Task<Response> GetByIdAsync(Guid id);
        Task<Response> CreateAsync(CourseVideoCreateDto dto);
        Task<Response> UpdateAsync(Guid id, CourseVideoCreateDto dto);
        Task<Response> DeleteAsync(Guid id, bool deleteFile = false);
        Task<Response> CreateCourseOutline(CourseContentDto dto, Guid courseId);
        Task<Response> GetCourseOutlinesByCourseId(Guid courseId);
        Task<Response> CreateCourseVideo(CourseVideoCreateDto dto, Guid outlineId);
        Task<Response> CreateCourseDocuemnt(CourseDocumentCreateDto dto, Guid outlineId);
        Task<Response> DeleteCourseVideo(Guid id, bool deleteFile = false);
        Task<Response> DeleteCourseDocuemnt(Guid id, bool deleteFile = false);
    }
}
