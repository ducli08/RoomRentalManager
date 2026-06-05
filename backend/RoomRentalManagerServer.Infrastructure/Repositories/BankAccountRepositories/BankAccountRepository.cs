using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoomRentalManagerServer.Domain.Interfaces.BankAccountInterfaces;
using RoomRentalManagerServer.Domain.ModelEntities.BankAccounts;
using RoomRentalManagerServer.Infrastructure.Data;

namespace RoomRentalManagerServer.Infrastructure.Repositories.BankAccountRepositories
{
    public class BankAccountRepository : IBankAccountRepository
    {
        private readonly RoomRentalManagerServerDbContext _context;
        private readonly ILogger<BankAccountRepository> _logger;

        public BankAccountRepository(RoomRentalManagerServerDbContext context, ILogger<BankAccountRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IQueryable<BankAccount> Query() => _context.BankAccounts.AsQueryable();

        public async Task<BankAccount?> GetByIdAsync(long id, bool asNoTracking = true)
        {
            try
            {
                var query = _context.BankAccounts.Where(x => x.Id == id);
                if (asNoTracking) query = query.AsNoTracking();
                return await query.FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get bank account by id {Id}", id);
                throw;
            }
        }

        public async Task AddAsync(BankAccount bankAccount)
        {
            try
            {
                await _context.BankAccounts.AddAsync(bankAccount);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add bank account");
                throw;
            }
        }

        public async Task UpdateAsync(BankAccount bankAccount)
        {
            try
            {
                _context.BankAccounts.Update(bankAccount);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update bank account {Id}", bankAccount.Id);
                throw;
            }
        }
    }
}

