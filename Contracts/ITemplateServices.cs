using EdgePMO.API.Dtos;

namespace EdgePMO.API.Contracts
{
    public interface ITemplateServices
    {
        Task<Response> GetAllAsync();
        Task<Response> GetByIdAsync(Guid id);
        Task<Response> CreateAsync(TemplateCreateDto dto);
        Task<Response> UpdateAsync(TemplateUpdateDto dto);
        Task<Response> DeleteAsync(Guid id);
    }
}
