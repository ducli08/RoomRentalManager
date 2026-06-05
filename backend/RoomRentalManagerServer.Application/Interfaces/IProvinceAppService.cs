using RoomRentalManagerServer.Domain.ModelEntities.Provinces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomRentalManagerServer.Application.Interfaces
{
    public interface IProvinceAppService
    {
        Task<List<Province>> GetAllProvincesAsync();
    }
}
