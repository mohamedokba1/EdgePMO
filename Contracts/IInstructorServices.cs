using EdgePMO.API.Dtos;

namespace EdgePMO.API.Contracts
{
    public interface IInstructorServices
    {
        Task<Response> GetAllAsync();
        Task<Response> GetByIdAsync(Guid id);
        Task<Response> CreateAsync(InstructorCreateDto dto);
        Task<Response> UpdateAsync(InstructorUpdateDto dto);
        Task<Response> DeleteAsync(Guid id);
    }
}
