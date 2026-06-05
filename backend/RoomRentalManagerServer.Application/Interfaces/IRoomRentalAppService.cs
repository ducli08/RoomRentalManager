using RoomRentalManagerServer.Application.Common.CommonDto;
using RoomRentalManagerServer.Application.Model.RoomRentalsModel.Dto;
using RoomRentalManagerServer.Domain.ModelEntities.RoomRentals;
using Microsoft.AspNetCore.Http;

namespace RoomRentalManagerServer.Application.Interfaces
{
    public interface IRoomRentalAppService
    {
        Task<PagedResultDto<RoomRentalDto>> GetAllRoomRentalAsync(PagedRequestDto<RoomRentalFilterDto> pagedRequestDto);
        Task<RoomRentalDto> GetRoomRentalByIdAsync(long id);
        Task<bool> CreateOrEditRoomRentalAsync(CreateOrEditRoomRentalDto input);
        Task DeleteRoomRentalAsync(long id, string webRoot);
        Task<List<RoomRental>> GetAllRoomRentalForSelectListItem();

        // Upload images and return saved relative paths and any errors
        Task<(List<string> Paths, List<string> Errors)> UploadImageDescriptionAsync(List<IFormFile> uploadImages, string webRoot);
    }
}
