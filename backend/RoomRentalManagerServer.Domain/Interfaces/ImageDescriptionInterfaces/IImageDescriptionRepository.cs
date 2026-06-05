using RoomRentalManagerServer.Domain.ModelEntities.ImageDescriptions;
using RoomRentalManagerServer.Domain.ModelEntities.RoomRentals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomRentalManagerServer.Domain.Interfaces.ImageDescriptionInterfaces
{
    public interface IImageDescriptionRepository
    {
        Task<IEnumerable<ImagesDescription>> GetAllImageAsync();
        Task<ImagesDescription> GetImageById(long id);
        Task AddAsync(ImagesDescription imagesDescription);
        Task UpdateAsync(ImagesDescription imagesDescription);
        Task DeleteAsync(long id);
    }
}
