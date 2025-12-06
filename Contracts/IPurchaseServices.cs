using EdgePMO.API.Dtos;

namespace EdgePMO.API.Contracts
{
    public interface IPurchaseServices
    {
        Task<Response> GetAllAsync();
        Task<Response> GetByIdAsync(Guid id);
        Task<Response> CreateAsync(PurchaseCreateDto dto);
        Task<Response> UpdateStatusAsync(Guid id, string status);
        Task<Response> DeleteAsync(Guid id);
    }
}
