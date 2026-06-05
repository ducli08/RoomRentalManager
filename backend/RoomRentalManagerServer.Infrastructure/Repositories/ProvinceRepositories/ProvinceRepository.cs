using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoomRentalManagerServer.Domain.Interfaces.ProvinceInterface;
using RoomRentalManagerServer.Domain.ModelEntities.Provinces;
using RoomRentalManagerServer.Domain.ModelEntities.User;
using RoomRentalManagerServer.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomRentalManagerServer.Infrastructure.Repositories.ProvinceRepositories
{
    public class ProvinceRepository : IProvinceRepository
    {
        private readonly RoomRentalManagerServerDbContext _context;
        private readonly ILogger<ProvinceRepository> _logger;
        public ProvinceRepository(RoomRentalManagerServerDbContext context, ILogger<ProvinceRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IQueryable<Province>> GetAllQueryAsync()
        {
            try
            {
                return _context.Province.AsQueryable().AsNoTracking();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get all province: {ex.Message}");
                throw;
            }
        }
    }
}
