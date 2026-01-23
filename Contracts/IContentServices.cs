using EdgePMO.API.Dtos;

namespace EdgePMO.API.Contracts
{
    public interface IContentServices
    {
        Task<Response> DeleteAssetAsync(string fileName);

        Task<bool> FileExistsAsync(string filePath);

        Task<Response> ListAssetsAsync();

        Task<Response> ListCoursesAssetsAsync();

        Task<Response> ListUploadsAssetsAsync();

        string SanitizePath(string path);

        Task<Response> UploadMediaAsync(IFormFile file, string? relativePath);

        Task<Response> UploadMediaStreamAsync(HttpRequest request, string fileName);
    }
}