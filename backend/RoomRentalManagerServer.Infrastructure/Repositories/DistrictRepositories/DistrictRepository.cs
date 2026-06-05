using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoomRentalManagerServer.Domain.Interfaces.DistrictInterfaces;
using RoomRentalManagerServer.Domain.ModelEntities.Districts;
using RoomRentalManagerServer.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomRentalManagerServer.Infrastructure.Repositories.DistrictRepositories
{
    public class DistrictRepository : IDistrictRepository
    {
        private readonly RoomRentalManagerServerDbContext _context;
        private readonly ILogger<DistrictRepository> _logger;
        public DistrictRepository(RoomRentalManagerServerDbContext context, ILogger<DistrictRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<IQueryable<District>> GetAllQueryAsync()
        {
            try
            {
                return _context.District.AsQueryable().AsNoTracking();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get all district: {ex.Message}");
                throw;
            }
        }
    }
}
