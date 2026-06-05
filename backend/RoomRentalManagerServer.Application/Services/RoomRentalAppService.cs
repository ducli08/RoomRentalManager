using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoomRentalManagerServer.Application.Common.CommonDto;
using RoomRentalManagerServer.Application.Interfaces;
using RoomRentalManagerServer.Application.Model.RoomRentalsModel.Dto;
using RoomRentalManagerServer.Domain.Interfaces.RoomRentalInterfaces;
using RoomRentalManagerServer.Domain.ModelEntities.RoomRentals;
using System.IO;
using Microsoft.AspNetCore.Http;
using RoomRentalManagerServer.Domain.Interfaces;

namespace RoomRentalManagerServer.Application.Services
{
    public class RoomRentalAppService : IRoomRentalAppService
    {
        private readonly ILogger<RoomRentalAppService> _logger;
        private readonly IMapper _mapper;
        private readonly IRoomRentalRepository _roomRentalRepository;
        private readonly ICurrentUserAppService _currentUserAppService;
        private readonly ICloudinaryService _cloudinaryService;
        public RoomRentalAppService(ILogger<RoomRentalAppService> logger, IRoomRentalRepository roomRentalRepository, IMapper mapper, ICurrentUserAppService currentUserAppService, ICloudinaryService cloudinaryService)
        {
            _logger = logger;
            _mapper = mapper;
            _roomRentalRepository = roomRentalRepository;
            _currentUserAppService = currentUserAppService;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<(List<string> Paths, List<string> Errors)> UploadImageDescriptionAsync(List<IFormFile> uploadImages, string webRoot)
        {
            if (!_currentUserAppService.IsAuthenticated)
            {
                _logger.LogWarning("Unauthenticated user attempted to upload images");
                return (new List<string>(), new List<string> { "User is not authenticated." });
            }

            var paths = new List<string>();
            var errors = new List<string>();

            foreach (var file in uploadImages)
            {
                try
                {
                    var (url, publicId) = await _cloudinaryService.UploadImageAsync(file, "room-rentals");
                    if (!string.IsNullOrEmpty(url))
                    {
                        paths.Add(url);
                    }
                    else
                    {
                        errors.Add($"Failed to upload {file.FileName} to Cloudinary.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to upload image {FileName} to Cloudinary", file.FileName);
                    errors.Add($"Failed to upload {file.FileName}: {ex.Message}");
                }
            }

            return (paths, errors);
        }

        public async Task<bool> CreateOrEditRoomRentalAsync(CreateOrEditRoomRentalDto createOrEditRoomRentalDto)
        {
            if (createOrEditRoomRentalDto == null)
                return false;

            var isUpdate = createOrEditRoomRentalDto.Id.HasValue && createOrEditRoomRentalDto.Id.Value > 0;
            var action = isUpdate ? "Edit" : "Create";

            try
            {
                var roomRental = _mapper.Map<RoomRental>(createOrEditRoomRentalDto);

                // Normalize/parse string fields coming from DTO
                if (!string.IsNullOrWhiteSpace(createOrEditRoomRentalDto.RoomNumber))
                {
                    if (int.TryParse(createOrEditRoomRentalDto.RoomNumber, out var rn))
                        roomRental.RoomNumber = rn;
                }

                if (!string.IsNullOrWhiteSpace(createOrEditRoomRentalDto.Price))
                {
                    if (double.TryParse(createOrEditRoomRentalDto.Price, out var price))
                        roomRental.Price = price;
                }

                if (!string.IsNullOrWhiteSpace(createOrEditRoomRentalDto.Area))
                {
                    if (double.TryParse(createOrEditRoomRentalDto.Area, out var area))
                        roomRental.Area = area;
                }

                if (isUpdate)
                {
                    await UpdateRoomRentalAsync(roomRental);
                }
                else
                {
                    await AddRoomRentalAsync(roomRental);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to {Action} room rental", action);
                return false;
            }
        }

        public async Task DeleteRoomRentalAsync(long id, string webRoot)
        {
            try
            {
                var roomRental = await _roomRentalRepository.GetRoomRetalById(id);
                if (roomRental == null)
                {
                    _logger.LogWarning("Room rental with id {Id} not found when attempting delete", id);
                    throw new KeyNotFoundException($"Room rental with id {id} not found.");
                }

                // Delete image files from disk if present. Don't fail delete if file deletion fails.
                try
                {
                    if (roomRental.ImagesDescription != null && roomRental.ImagesDescription.Any())
                    {
                        foreach (var imgUrl in roomRental.ImagesDescription)
                        {
                            if (string.IsNullOrWhiteSpace(imgUrl))
                                continue;

                            try
                            {
                                // Attempt to extract publicId from Cloudinary URL if possible, or just skip deletion
                                // A typical Cloudinary URL: https://res.cloudinary.com/.../upload/v.../folder/filename.ext
                                // For now we just log it since deleting from Cloudinary needs the exact publicId.
                                // If needed, we can parse it:
                                var uri = new Uri(imgUrl);
                                var segments = uri.Segments;
                                var uploadIndex = Array.FindIndex(segments, s => s == "upload/");
                                if (uploadIndex >= 0 && segments.Length > uploadIndex + 2)
                                {
                                    // Skip the version segment (e.g. v123456/)
                                    var publicIdWithExtension = string.Join("", segments.Skip(uploadIndex + 2));
                                    var publicId = Path.ChangeExtension(publicIdWithExtension, null).Replace("\\", "/");
                                    
                                    await _cloudinaryService.DeleteImageAsync(publicId);
                                    _logger.LogInformation("Deleted image file {PublicId} from Cloudinary for room rental {Id}", publicId, id);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to delete image {Url} from Cloudinary for room rental {Id}", imgUrl, id);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error while attempting to delete image files for room rental {Id}", id);
                }

                await _roomRentalRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete room rental with id {Id}", id);
                throw;
            }
        }

        public async Task<PagedResultDto<RoomRentalDto>> GetAllRoomRentalAsync(PagedRequestDto<RoomRentalFilterDto> pagedRequestRoomRentalDto)
        {
            try
            {
                var queryGetAllRoomRental = await _roomRentalRepository.GetAllRoomRentalAsync();

                // Filter by RoomNumber
                if (!string.IsNullOrEmpty(pagedRequestRoomRentalDto.Filter?.RoomNumber))
                {
                    if (int.TryParse(pagedRequestRoomRentalDto.Filter.RoomNumber, out int roomNumber) && roomNumber > 0)
                    {
                        queryGetAllRoomRental = queryGetAllRoomRental.Where(x => x.RoomNumber == roomNumber);
                    }
                }

                // Filter by RoomType
                if (pagedRequestRoomRentalDto.Filter?.RoomType != 0)
                {
                    queryGetAllRoomRental = queryGetAllRoomRental.Where(x => x.RoomType == pagedRequestRoomRentalDto.Filter.RoomType);
                }

                // Filter by Price range
                if (!string.IsNullOrEmpty(pagedRequestRoomRentalDto.Filter?.PriceStart))
                {
                    if (double.TryParse(pagedRequestRoomRentalDto.Filter.PriceStart, out double priceStart) && priceStart >= 0)
                    {
                        queryGetAllRoomRental = queryGetAllRoomRental.Where(x => x.Price >= priceStart);
                    }
                }

                if (!string.IsNullOrEmpty(pagedRequestRoomRentalDto.Filter?.PriceEnd))
                {
                    if (double.TryParse(pagedRequestRoomRentalDto.Filter.PriceEnd, out double priceEnd) && priceEnd >= 0)
                    {
                        queryGetAllRoomRental = queryGetAllRoomRental.Where(x => x.Price <= priceEnd);
                    }
                }

                // Filter by StatusRoom
                if (pagedRequestRoomRentalDto.Filter?.StatusRoom != 0)
                {
                    queryGetAllRoomRental = queryGetAllRoomRental.Where(x => x.StatusRoom == pagedRequestRoomRentalDto.Filter.StatusRoom);
                }

                // Filter by Note (contains search)
                if (!string.IsNullOrEmpty(pagedRequestRoomRentalDto.Filter?.Note))
                {
                    var pattern = $"%{pagedRequestRoomRentalDto.Filter.Note}%";
                    queryGetAllRoomRental = queryGetAllRoomRental.Where(x =>
                        EF.Functions.ILike(EF.Functions.Unaccent(x.Note), EF.Functions.Unaccent(pattern))
                    );
                }

                // Filter by Area range
                if (!string.IsNullOrEmpty(pagedRequestRoomRentalDto.Filter?.AreaStart))
                {
                    if (double.TryParse(pagedRequestRoomRentalDto.Filter.AreaStart, out double areaStart) && areaStart >= 0)
                    {
                        queryGetAllRoomRental = queryGetAllRoomRental.Where(x => x.Area >= areaStart);
                    }
                }

                if (!string.IsNullOrEmpty(pagedRequestRoomRentalDto.Filter?.AreaEnd))
                {
                    if (double.TryParse(pagedRequestRoomRentalDto.Filter.AreaEnd, out double areaEnd) && areaEnd >= 0)
                    {
                        queryGetAllRoomRental = queryGetAllRoomRental.Where(x => x.Area <= areaEnd);
                    }
                }

                // Filter by CreatedDate
                if (pagedRequestRoomRentalDto.Filter?.CreatedDate.HasValue == true)
                {
                    var filterDate = pagedRequestRoomRentalDto.Filter.CreatedDate.Value.Date;
                    queryGetAllRoomRental = queryGetAllRoomRental.Where(x => x.CreatedDate.Date == filterDate);
                }

                // Filter by UpdatedDate
                if (pagedRequestRoomRentalDto.Filter?.UpdatedDate.HasValue == true)
                {
                    var filterDate = pagedRequestRoomRentalDto.Filter.UpdatedDate.Value.Date;
                    queryGetAllRoomRental = queryGetAllRoomRental.Where(x => x.UpdatedDate.Date == filterDate);
                }

                // Filter by CreatorUser (contains search)
                if (!string.IsNullOrEmpty(pagedRequestRoomRentalDto.Filter?.CreatorUser))
                {
                    queryGetAllRoomRental = queryGetAllRoomRental.Where(x => x.CreatorUser != null && x.CreatorUser.Contains(pagedRequestRoomRentalDto.Filter.CreatorUser));
                }

                // Filter by LastUpdateUser (contains search)
                if (!string.IsNullOrEmpty(pagedRequestRoomRentalDto.Filter?.LastUpdateUser))
                {
                    queryGetAllRoomRental = queryGetAllRoomRental.Where(x => x.LastUpdateUser != null && x.LastUpdateUser.Contains(pagedRequestRoomRentalDto.Filter.LastUpdateUser));
                }

                // Apply sorting
                if (!string.IsNullOrEmpty(pagedRequestRoomRentalDto.SortOrder) && !string.IsNullOrEmpty(pagedRequestRoomRentalDto.SortBy))
                {
                    queryGetAllRoomRental = pagedRequestRoomRentalDto.SortOrder == "desc"
                    ? queryGetAllRoomRental.OrderByDescending(x => EF.Property<object>(x, pagedRequestRoomRentalDto.SortBy))
                    : queryGetAllRoomRental.OrderBy(x => EF.Property<object>(x, pagedRequestRoomRentalDto.SortBy));
                }

                var total = await queryGetAllRoomRental.CountAsync();
                var roomRentals = await queryGetAllRoomRental.Skip((pagedRequestRoomRentalDto.Page - 1) * pagedRequestRoomRentalDto.PageSize).Take(pagedRequestRoomRentalDto.PageSize).ToListAsync();
                var roomRentalDtos = roomRentals.Select(r => _mapper.Map<RoomRentalDto>(r)).ToList();
                return new PagedResultDto<RoomRentalDto>(roomRentalDtos, total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all room rentals");
                throw;
            }
        }

        public async Task<RoomRentalDto?> GetRoomRentalByIdAsync(long id)
        {
            try
            {
                var roomRental = await _roomRentalRepository.GetRoomRetalById(id);
                if (roomRental == null)
                {
                    return null;
                }
                return _mapper.Map<RoomRentalDto>(roomRental);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get room rental by ID {Id}", id);
                throw;
            }
        }

        public async Task<RoomRentalDto> AddRoomRentalAsync(RoomRental roomRental)
        {
            try
            {
                if (!_currentUserAppService.IsAuthenticated)
                {
                    throw new UnauthorizedAccessException("User is not authenticated.");
                }
                var userName = _currentUserAppService.UserName ?? throw new InvalidOperationException("User is null.");
                roomRental.CreatorUser = userName;
                roomRental.LastUpdateUser = userName;
                roomRental.CreatedDate = roomRental.UpdatedDate = DateTime.UtcNow;
                await _roomRentalRepository.AddAsync(roomRental);
                return _mapper.Map<RoomRentalDto>(roomRental);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add room rental");
                throw;
            }
        }

        public async Task UpdateRoomRentalAsync(RoomRental roomRental)
        {
            try
            {
                roomRental.LastUpdateUser = _currentUserAppService.UserName ?? throw new InvalidOperationException("User is null.");
                roomRental.UpdatedDate = DateTime.UtcNow;
                await _roomRentalRepository.UpdateAsync(roomRental);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update room rental");
                throw;
            }
        }

        public async Task<List<RoomRental>> GetAllRoomRentalForSelectListItem()
        {
            try
            {
                var queryGetAllRoomRental = await _roomRentalRepository.GetAllRoomRentalAsync();
                return await queryGetAllRoomRental.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all room rentals for select list item.");
                throw;
            }
        }

        
    }
}
