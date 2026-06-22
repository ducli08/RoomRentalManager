using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoomRentalManagerServer.Domain.Interfaces.UtilityReadingInterfaces;
using RoomRentalManagerServer.Domain.ModelEntities.UtilityReadings;
using RoomRentalManagerServer.Infrastructure.Data;

namespace RoomRentalManagerServer.Infrastructure.Repositories.UtilityReadingRepositories
{
    public class UtilityReadingRepository : IUtilityReadingRepository
    {
        private readonly RoomRentalManagerServerDbContext _context;
        private readonly ILogger<UtilityReadingRepository> _logger;

        public UtilityReadingRepository(RoomRentalManagerServerDbContext context, ILogger<UtilityReadingRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IQueryable<UtilityReading> Query() => _context.UtilityReadings.AsQueryable();

        public async Task<UtilityReading?> GetByIdAsync(long id, bool asNoTracking = true)
        {
            try
            {
                var query = _context.UtilityReadings.Where(x => x.Id == id);
                if (asNoTracking) query = query.AsNoTracking();
                return await query.FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get utility reading by id {Id}", id);
                throw;
            }
        }

        public async Task<UtilityReading?> GetByContractMonthYearAsync(long contractId, int month, int year, bool asNoTracking = true)
        {
            try
            {
                var query = _context.UtilityReadings.Where(x => x.ContractId == contractId && x.Month == month && x.Year == year);
                if (asNoTracking) query = query.AsNoTracking();
                return await query.FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get utility reading by contract {ContractId} {Month}/{Year}", contractId, month, year);
                throw;
            }
        }

        public async Task AddAsync(UtilityReading utilityReading)
        {
            try
            {
                await _context.UtilityReadings.AddAsync(utilityReading);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add utility reading");
                throw;
            }
        }

        public async Task UpdateAsync(UtilityReading utilityReading)
        {
            try
            {
                var existing = await _context.UtilityReadings.FirstOrDefaultAsync(x => x.Id == utilityReading.Id);
                if (existing == null)
                {
                    throw new KeyNotFoundException($"Utility reading with id {utilityReading.Id} not found.");
                }

                existing.ContractId = utilityReading.ContractId;
                existing.Month = utilityReading.Month;
                existing.Year = utilityReading.Year;
                existing.OldElectricIndex = utilityReading.OldElectricIndex;
                existing.NewElectricIndex = utilityReading.NewElectricIndex;
                existing.ElectricUsage = utilityReading.ElectricUsage;
                existing.OldWaterIndex = utilityReading.OldWaterIndex;
                existing.NewWaterIndex = utilityReading.NewWaterIndex;
                existing.WaterUsage = utilityReading.WaterUsage;
                existing.ElectricUnitPrice = utilityReading.ElectricUnitPrice;
                existing.WaterUnitPrice = utilityReading.WaterUnitPrice;
                existing.Status = utilityReading.Status;
                existing.UpdatedAt = utilityReading.UpdatedAt;
                existing.UpdaterUser = utilityReading.UpdaterUser;

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update utility reading {Id}", utilityReading.Id);
                throw;
            }
        }
    }
}
