using EdgePMO.API.Dtos;

namespace EdgePMO.API.Contracts
{
    public interface IPromoCodeServices
    {
        Task<Response> GetAllAsync();
        Task<Response> GetByIdAsync(Guid id);
        Task<Response> CreateAsync(PromoCodeCreateDto dto);
        Task<Response> UpdateAsync(PromoCodeUpdateDto dto);
        Task<Response> DeleteAsync(Guid id);
        Task<Response> ValidateCodeAsync(string code, Guid? courseId);
    }
}
