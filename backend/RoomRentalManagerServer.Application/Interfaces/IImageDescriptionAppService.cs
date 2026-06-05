using RoomRentalManagerServer.Application.Model.ImageDescriptionsModel.Dto;
using RoomRentalManagerServer.Application.Model.UsersModel.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomRentalManagerServer.Application.Interfaces
{
    public interface IImageDescriptionAppService
    {
        Task<List<ImageDescriptionDto>> GetAllImageAsync();
        Task<ImageDescriptionDto> GetImageByIdAsync(long id);
        Task<ImageDescriptionDto> CreateOrEditImageAsync(CreateOrEditImageDescriptionDto input);
        Task<bool> DeleteImageAsync(long id);
    }
}
