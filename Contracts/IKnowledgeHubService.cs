using EdgePMO.API.Dtos;

namespace EdgePMO.API.Contracts
{
    public interface IKnowledgeHubService
    {
        Task<Response> CreateAsync(CreateKnowledgeHubDto dto);
        Task<Response> GetByIdAsync(Guid id);
        Task<Response> GetAllAsync(int pageNumber = 1, int pageSize = 10);
        Task<Response> UpdateAsync(Guid id, UpdateKnowledgeHubDto dto);
        Task<Response> DeleteAsync(Guid id);
    }
}
