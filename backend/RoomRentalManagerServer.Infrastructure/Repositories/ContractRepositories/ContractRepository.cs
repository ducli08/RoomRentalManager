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
    }
}

