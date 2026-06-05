using RoomRentalManagerServer.Domain.Interfaces.ImageDescriptionInterfaces;
using RoomRentalManagerServer.Domain.ModelEntities.ImageDescriptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomRentalManagerServer.Infrastructure.Repositories.ImageDescriptionRepositories
{
    public class ImageDescriptionRepository : IImageDescriptionRepository
    {
        public Task AddAsync(ImagesDescription imagesDescription)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(long id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ImagesDescription>> GetAllImageAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ImagesDescription> GetImageById(long id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(ImagesDescription imagesDescription)
        {
            throw new NotImplementedException();
        }
    }
}
