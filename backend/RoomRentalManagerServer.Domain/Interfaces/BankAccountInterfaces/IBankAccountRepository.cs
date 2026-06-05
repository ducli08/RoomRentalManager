using RoomRentalManagerServer.Domain.ModelEntities.BankAccounts;

namespace RoomRentalManagerServer.Domain.Interfaces.BankAccountInterfaces
{
    public interface IBankAccountRepository
    {
        IQueryable<BankAccount> Query();
        Task<BankAccount?> GetByIdAsync(long id, bool asNoTracking = true);
        Task AddAsync(BankAccount bankAccount);
        Task UpdateAsync(BankAccount bankAccount);
    }
}

