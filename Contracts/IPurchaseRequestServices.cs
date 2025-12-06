using EdgePMO.API.Dtos;

namespace EdgePMO.API.Contracts
{
    public interface IPurchaseRequestServices
    {
        Task<Response> CreateRequestAsync(PurchaseRequestCreateDto dto, Guid requestorId);
        Task<Response> GetByIdAsync(Guid id, Guid? requesterId = null);
        Task<Response> GetForUserAsync(Guid userId);
        Task<Response> GetAllAsync();
        Task<Response> ApproveAsync(Guid requestId, Guid adminId);
        Task<Response> RejectAsync(Guid requestId, Guid adminId, string reason);
    }
}
