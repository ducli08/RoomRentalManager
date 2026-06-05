using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoomRentalManagerServer.Application.Interfaces;
using RoomRentalManagerServer.Domain.Interfaces.BankAccountInterfaces;
using RoomRentalManagerServer.Domain.ModelEntities.BankAccounts;

namespace RoomRentalManagerServer.Application.Services
{
    public class BankAccountAppService : IBankAccountAppService
    {
        private readonly ILogger<BankAccountAppService> _logger;
        private readonly IBankAccountRepository _bankAccountRepository;

        public BankAccountAppService(ILogger<BankAccountAppService> logger, IBankAccountRepository bankAccountRepository)
        {
            _logger = logger;
            _bankAccountRepository = bankAccountRepository;
        }

        public async Task<BankAccount?> GetActiveAsync()
        {
            try
            {
                return await _bankAccountRepository.Query()
                    .AsNoTracking()
                    .Where(x => x.IsActive)
                    .OrderByDescending(x => x.UpdatedAt)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get active bank account");
                throw;
            }
        }
    }
}

