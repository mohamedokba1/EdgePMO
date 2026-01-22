using EdgePMO.API.Dtos;

namespace EdgePMO.API.Contracts
{
    public interface IConsultationRequestServices
    {
        Task<Response> CreateAsync(ConsultationRequestCreateDto dto);

        Task<Response> DeleteAsync(Guid id);

        Task<Response> GetAllAsync();

        Task<Response> GetByIdAsync(Guid id);

        Task<Response> UpdateAsync(Guid id, ConsultationRequestUpdateDto dto);
    }
}