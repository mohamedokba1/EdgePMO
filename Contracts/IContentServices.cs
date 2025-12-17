using EdgePMO.API.Dtos;

namespace EdgePMO.API.Contracts
{
    public interface IContentServices
    {
        Task<Response> UploadMediaAsync(IFormFile file, string? relativePath);
        Task<Response> ListAssetsAsync();
        Task<Response> ListCoursesAssetsAsync();
        Task<Response> DeleteAssetAsync(string fileName);
        Task<bool> FileExistsAsync(string filePath);
        string SanitizePath(string path);
    }
}
