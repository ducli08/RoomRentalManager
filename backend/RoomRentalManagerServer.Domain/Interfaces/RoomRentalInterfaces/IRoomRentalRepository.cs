using RoomRentalManagerServer.Domain.ModelEntities.RoomRentals;

namespace RoomRentalManagerServer.Domain.Interfaces.RoomRentalInterfaces
{
    public interface IRoomRentalRepository
    {
        Task<IQueryable<RoomRental>> GetAllRoomRentalAsync();
        Task<RoomRental?> GetRoomRetalById(long id);
        Task AddAsync(RoomRental roomRental);
        Task UpdateAsync(RoomRental roomRental);
        Task DeleteAsync(long id);
    }
}
