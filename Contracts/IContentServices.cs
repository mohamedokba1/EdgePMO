using EdgePMO.API.Dtos;

namespace EdgePMO.API.Contracts
{
    public interface IContentServices
    {
        Task<Response> UploadMediaAsync(IFormFile file);
        Task<Response> ListAssetsAsync();
        Task<Response> DeleteAssetAsync(string fileName);
        Task<bool> FileExistsAsync(string filePath);
    }
}
