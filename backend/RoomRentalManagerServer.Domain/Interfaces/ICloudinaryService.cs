using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace RoomRentalManagerServer.Domain.Interfaces
{
    public interface ICloudinaryService
    {
        Task<(string Url, string PublicId)> UploadImageAsync(IFormFile file, string folderName);
        Task<(string Url, string PublicId)> UploadImageFromUrlAsync(string imageUrl, string folderName);
        Task<bool> DeleteImageAsync(string publicId);
    }
}
