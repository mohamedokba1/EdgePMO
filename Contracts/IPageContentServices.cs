using EdgePMO.API.Dtos;

namespace EdgePMO.API.Contracts
{
    public interface IPageContentServices
    {
        Task<Response> GetBySlugAsync(string slug);
        Task<Response> GetByIdAsync(Guid id);
        Task<Response> GetAllAsync();
        Task<Response> CreateAsync(PageContentCreateDto dto);
        Task<Response> UpdateAsync(PageContentUpdateDto dto);
        Task<Response> DeleteAsync(Guid id);
    }
}
