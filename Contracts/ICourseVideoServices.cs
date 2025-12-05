using EdgePMO.API.Dtos;

namespace EdgePMO.API.Contracts
{
    public interface ICourseVideoServices
    {
        Task<Response> GetByCourseIdAsync(Guid courseId);
        Task<Response> GetByIdAsync(Guid id);
        Task<Response> CreateAsync(CourseVideoCreateDto dto);
        Task<Response> UpdateAsync(Guid id, CourseVideoCreateDto dto);
        Task<Response> DeleteAsync(Guid id, bool deleteFile = false);
    }
}
