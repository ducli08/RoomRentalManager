using RoomRentalManagerServer.Domain.ModelEntities.Wards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomRentalManagerServer.Domain.Interfaces.WardInterfaces
{
    public interface IWardRepository
    {
        Task<IQueryable<Ward>> GetAllQueryAsync();
    }
}
