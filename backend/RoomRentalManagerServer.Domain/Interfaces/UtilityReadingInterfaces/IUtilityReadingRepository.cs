using RoomRentalManagerServer.Domain.ModelEntities.UtilityReadings;

namespace RoomRentalManagerServer.Domain.Interfaces.UtilityReadingInterfaces
{
    public interface IUtilityReadingRepository
    {
        IQueryable<UtilityReading> Query();
        Task<UtilityReading?> GetByIdAsync(long id, bool asNoTracking = true);
        Task<UtilityReading?> GetByContractMonthYearAsync(long contractId, int month, int year, bool asNoTracking = true);
        Task AddAsync(UtilityReading utilityReading);
        Task UpdateAsync(UtilityReading utilityReading);
    }
}
