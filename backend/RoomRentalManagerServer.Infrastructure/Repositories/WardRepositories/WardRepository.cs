using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoomRentalManagerServer.Domain.Interfaces.WardInterfaces;
using RoomRentalManagerServer.Domain.ModelEntities.Provinces;
using RoomRentalManagerServer.Domain.ModelEntities.Wards;
using RoomRentalManagerServer.Infrastructure.Data;
using RoomRentalManagerServer.Infrastructure.Repositories.ProvinceRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomRentalManagerServer.Infrastructure.Repositories.WardRepositories
{
    public class WardRepository : IWardRepository
    {
        private readonly RoomRentalManagerServerDbContext _context;
        private readonly ILogger<WardRepository> _logger;
        public WardRepository(RoomRentalManagerServerDbContext context, ILogger<WardRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IQueryable<Ward>> GetAllQueryAsync()
        {
            try
            {
                return _context.Ward.AsQueryable().AsNoTracking();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get all ward: {ex.Message}");
                throw;
            }
        }
    }
}
