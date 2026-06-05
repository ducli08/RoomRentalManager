using RoomRentalManagerServer.Application.Interfaces;
using RoomRentalManagerServer.Application.Model.ImageDescriptionsModel.Dto;
using RoomRentalManagerServer.Domain.Interfaces.ImageDescriptionInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomRentalManagerServer.Application.Services
{
    public class ImageDescriptionAppService : IImageDescriptionAppService
    {
        public Task<ImageDescriptionDto> CreateOrEditImageAsync(CreateOrEditImageDescriptionDto input)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteImageAsync(long id)
        {
            throw new NotImplementedException();
        }

        public Task<List<ImageDescriptionDto>> GetAllImageAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ImageDescriptionDto> GetImageByIdAsync(long id)
        {
            throw new NotImplementedException();
        }
    }
}
