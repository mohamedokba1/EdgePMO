using EdgePMO.API.Dtos;

namespace EdgePMO.API.Contracts
{
    public interface ICertificateServices
    {
        Task<byte[]> GenerateCertificateFileAsync(Guid certificateId);
        Task<Response> ProcessCertificateClaimAsync(Guid userId, Guid courseId);
    }
}
