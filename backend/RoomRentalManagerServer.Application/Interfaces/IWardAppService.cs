using RoomRentalManagerServer.Domain.ModelEntities.Wards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomRentalManagerServer.Application.Interfaces
{
    public interface IWardAppService
    {
        Task<List<Ward>> GetAllWardsAsync(string? districtCode);
    }
}
