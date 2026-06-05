using Microsoft.AspNetCore.Http;

namespace RoomRentalManagerServer.Application.Interfaces
{
    public interface ILocalFileStorageAppService
    {
        Task<(List<string> Paths, List<string> Errors)> UploadFilesAsync(IEnumerable<IFormFile> files, string relativeFolder, string webRootPath);
        Task<bool> DeleteFileAsync(string relativePath, string webRootPath);

        // Upload single file convenience helper
        Task<(string Path, string? Error)> UploadFileAsync(IFormFile file, string relativeFolder, string webRootPath);
        Task<(string Path, string? Error)> DownloadImageFromUrlAsync(string imageUrl, string relativeFolder, string webRootPath);
    }
}
