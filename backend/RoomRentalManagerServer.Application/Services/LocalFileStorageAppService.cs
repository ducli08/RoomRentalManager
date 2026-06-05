using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RoomRentalManagerServer.Application.Interfaces;

namespace RoomRentalManagerServer.API.Services
{
    public class LocalFileStorageAppService : ILocalFileStorageAppService
    {
        private readonly ILogger<LocalFileStorageAppService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        
        public LocalFileStorageAppService(ILogger<LocalFileStorageAppService> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<(List<string> Paths, List<string> Errors)> UploadFilesAsync(IEnumerable<IFormFile> files, string relativeFolder, string webRootPath)
        {
            var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".gif" };
            const long maxFileSize = 5 * 1024 * 1024; // 5 MB

            var savedPaths = new List<string>();
            var errors = new List<string>();

            var webRoot = webRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsRootFolder = Path.Combine(webRoot, relativeFolder.Trim('/', '\\'));
            if (!Directory.Exists(uploadsRootFolder))
                Directory.CreateDirectory(uploadsRootFolder);

            foreach (var file in files)
            {
                if (file == null)
                    continue;

                if (file.Length == 0)
                {
                    errors.Add($"File {file?.FileName} is empty.");
                    continue;
                }

                if (file.Length > maxFileSize)
                {
                    errors.Add($"File {file.FileName} exceeds the allowed size of {maxFileSize} bytes.");
                    continue;
                }

                var ext = Path.GetExtension(file.FileName);
                if (string.IsNullOrEmpty(ext) || !allowedExtensions.Contains(ext))
                {
                    errors.Add($"File {file.FileName} has an invalid extension.");
                    continue;
                }

                var uniqueFileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsRootFolder, uniqueFileName);

                try
                {
                    await using var stream = new FileStream(filePath, FileMode.Create);
                    await file.CopyToAsync(stream);

                    var relativePath = $"/{relativeFolder.Trim('/')}/{uniqueFileName}";
                    savedPaths.Add(relativePath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to save uploaded image {FileName}", file.FileName);
                    errors.Add($"Failed to save {file.FileName}: {ex.Message}");
                }
            }

            return (savedPaths, errors);
        }

        public async Task<(string Path, string? Error)> UploadFileAsync(IFormFile file, string relativeFolder, string webRootPath)
        {
            var (paths, errors) = await UploadFilesAsync(new[] { file }, relativeFolder, webRootPath);
            return (paths.FirstOrDefault() ?? string.Empty, errors.FirstOrDefault());
        }

        public Task<bool> DeleteFileAsync(string relativePath, string webRootPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(relativePath))
                    return Task.FromResult(false);

                var webRoot = webRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var relative = relativePath.TrimStart('~').TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                var fullPath = Path.Combine(webRoot, relative);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete file {Path}", relativePath);
                return Task.FromResult(false);
            }
        }

        public async Task<(string Path, string? Error)> DownloadImageFromUrlAsync(string imageUrl, string relativeFolder, string webRootPath)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return (string.Empty, "Image URL is empty");
            }

            if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                return (string.Empty, "Invalid image URL");
            }

            var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".gif" };
            const long maxFileSize = 5 * 1024 * 1024; // 5 MB

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(30);

                var response = await httpClient.GetAsync(imageUrl);
                response.EnsureSuccessStatusCode();

                var contentType = response.Content.Headers.ContentType?.MediaType?.ToLowerInvariant();
                if (contentType == null || (!contentType.StartsWith("image/") && !contentType.Contains("jpeg") && !contentType.Contains("png") && !contentType.Contains("gif")))
                {
                    return (string.Empty, "URL does not point to a valid image");
                }

                var contentLength = response.Content.Headers.ContentLength;
                if (contentLength.HasValue && contentLength.Value > maxFileSize)
                {
                    return (string.Empty, $"Image size ({contentLength.Value} bytes) exceeds maximum allowed size ({maxFileSize} bytes)");
                }

                var imageBytes = await response.Content.ReadAsByteArrayAsync();
                if (imageBytes.Length > maxFileSize)
                {
                    return (string.Empty, $"Image size ({imageBytes.Length} bytes) exceeds maximum allowed size ({maxFileSize} bytes)");
                }

                // Determine file extension from content type or URL
                string extension = ".jpg"; // default
                if (contentType.Contains("png"))
                    extension = ".png";
                else if (contentType.Contains("gif"))
                    extension = ".gif";
                else if (contentType.Contains("jpeg") || contentType.Contains("jpg"))
                    extension = ".jpg";
                else
                {
                    // Try to get extension from URL
                    var urlExtension = Path.GetExtension(uri.AbsolutePath);
                    if (!string.IsNullOrEmpty(urlExtension) && allowedExtensions.Contains(urlExtension))
                    {
                        extension = urlExtension;
                    }
                }

                var webRoot = webRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var uploadsRootFolder = Path.Combine(webRoot, relativeFolder.Trim('/', '\\'));
                if (!Directory.Exists(uploadsRootFolder))
                    Directory.CreateDirectory(uploadsRootFolder);

                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsRootFolder, uniqueFileName);

                await File.WriteAllBytesAsync(filePath, imageBytes);

                var relativePath = $"/{relativeFolder.Trim('/')}/{uniqueFileName}";
                _logger.LogInformation($"Successfully downloaded image from {imageUrl} to {relativePath}");
                return (relativePath, null);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Failed to download image from URL {Url}: HTTP error", imageUrl);
                return (string.Empty, $"Failed to download image: {ex.Message}");
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogWarning(ex, "Timeout while downloading image from URL {Url}", imageUrl);
                return (string.Empty, "Timeout while downloading image");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to download image from URL {Url}", imageUrl);
                return (string.Empty, $"Failed to download image: {ex.Message}");
            }
        }
    }
}
