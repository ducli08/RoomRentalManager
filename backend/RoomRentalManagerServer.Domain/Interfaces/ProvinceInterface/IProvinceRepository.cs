using RoomRentalManagerServer.Domain.ModelEntities.Provinces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomRentalManagerServer.Domain.Interfaces.ProvinceInterface
{
    public interface IProvinceRepository
    {
        Task<IQueryable<Province>> GetAllQueryAsync();
    }
}
