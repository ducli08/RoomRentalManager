using RoomRentalManagerServer.Domain.ModelEntities.BankAccounts;

namespace RoomRentalManagerServer.Application.Interfaces
{
    public interface IBankAccountAppService
    {
        Task<BankAccount?> GetActiveAsync();
    }
}

