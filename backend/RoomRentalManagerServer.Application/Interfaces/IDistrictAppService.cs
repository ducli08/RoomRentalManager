using RoomRentalManagerServer.Domain.ModelEntities.Districts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomRentalManagerServer.Application.Interfaces
{
    public interface IDistrictAppService
    {
        Task<List<District>> GetAllDistrictsAsync(string? provinceCode);
    }
}
