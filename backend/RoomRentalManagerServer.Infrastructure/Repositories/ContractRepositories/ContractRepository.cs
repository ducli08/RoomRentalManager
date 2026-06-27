using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoomRentalManagerServer.Domain.Interfaces.ContractInterfaces;
using RoomRentalManagerServer.Domain.ModelEntities.Contracts;
using RoomRentalManagerServer.Infrastructure.Data;

namespace RoomRentalManagerServer.Infrastructure.Repositories.ContractRepositories
{
    public class ContractRepository : IContractRepository
    {
        private readonly RoomRentalManagerServerDbContext _context;
        private readonly ILogger<ContractRepository> _logger;

        public ContractRepository(RoomRentalManagerServerDbContext context, ILogger<ContractRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IQueryable<Contract> Query() => _context.Contracts.AsQueryable();

        public async Task<Contract?> GetByIdAsync(long id, bool asNoTracking = true)
        {
            try
            {
                var query = _context.Contracts.Where(x => x.Id == id);
                if (asNoTracking) query = query.AsNoTracking();
                return await query.FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get contract by id {Id}", id);
                throw;
            }
        }

        public async Task AddAsync(Contract contract)
        {
            try
            {
                await _context.Contracts.AddAsync(contract);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add contract");
                throw;
            }
        }

        public async Task UpdateAsync(Contract contract)
        {
            try
            {
                var existing = await _context.Contracts.FirstOrDefaultAsync(x => x.Id == contract.Id);
                if (existing == null)
                {
                    throw new KeyNotFoundException($"Contract with id {contract.Id} not found.");
                }

                existing.RoomRentalId = contract.RoomRentalId;
                existing.TenantId = contract.TenantId;
                existing.TenantIds = contract.TenantIds;
                existing.StartDate = contract.StartDate;
                existing.EndDate = contract.EndDate;
                existing.DepositAmout = contract.DepositAmout;
                existing.MonthlyRent = contract.MonthlyRent;
                existing.ElectricUnitPrice = contract.ElectricUnitPrice;
                existing.WaterUnitPrice = contract.WaterUnitPrice;
                existing.GarbageFeePerMonthPerPerson = contract.GarbageFeePerMonthPerPerson;
                existing.StatusContract = contract.StatusContract;
                existing.UpdatedAt = contract.UpdatedAt;
                existing.UpdaterUser = contract.UpdaterUser;

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update contract {Id}", contract.Id);
                throw;
            }
        }

        public async Task DeleteAsync(long id)
        {
            try
            {
                var contract = await _context.Contracts.FirstOrDefaultAsync(x => x.Id == id);
                if (contract == null)
                {
                    throw new KeyNotFoundException($"Contract with id {id} not found.");
                }

                _context.Contracts.Remove(contract);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete contract {Id}", id);
                throw;
            }
        }
    }
}
