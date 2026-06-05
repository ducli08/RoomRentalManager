using RoomRentalManagerServer.Domain.ModelEntities.Districts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomRentalManagerServer.Domain.Interfaces.DistrictInterfaces
{
    public interface IDistrictRepository
    {
        Task<IQueryable<District>> GetAllQueryAsync();
    }
}
