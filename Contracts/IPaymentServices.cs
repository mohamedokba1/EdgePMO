using EdgePMO.API.Dtos;

namespace EdgePMO.API.Contracts
{
    public interface IPaymentServices
    {
        Task<Response> InitializePurchaseAsync(Guid userId, Guid? courseId, Guid? templateId, string promoCode);
        Task<Response> ProcessCallbackAsync(string tapChargeId);
        bool VerifySignature(string rawJson, string headerSignature);
    }
}
