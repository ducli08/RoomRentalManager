using RoomRentalManagerServer.Domain.ModelEntities.Contracts;

namespace RoomRentalManagerServer.Domain.Interfaces.ContractInterfaces
{
    public interface IContractRepository
    {
        IQueryable<Contract> Query();
        Task<Contract?> GetByIdAsync(long id, bool asNoTracking = true);
        Task AddAsync(Contract contract);
        Task UpdateAsync(Contract contract);
        Task DeleteAsync(long id);
    }
}
