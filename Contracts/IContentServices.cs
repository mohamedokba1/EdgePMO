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

        Task<Response> CreateFolderAsync(string folderName, Guid? parentFolderId);

        Task<bool> FolderExistsAsync(string folderName, Guid? parentFolderId);

        Task<Response> UploadMediaStreamAsync(HttpRequest request, string fileName);

        Task<Response> UploadMediaStreamWithFolderIdAsync(HttpRequest request, string fileName, Guid? targetFolderId);

        Task<Response> GetPhysicalStructureWithIdsAsync();

        Task<Response> DeleteFolderAsync(Guid folderId);

        Task<Response> SyncFileSystemToDbAsync();

    }
}