using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoomRentalManagerServer.Domain.Interfaces.RoomRentalInterfaces;
using RoomRentalManagerServer.Domain.ModelEntities.RoomRentals;
using RoomRentalManagerServer.Infrastructure.Data;

namespace RoomRentalManagerServer.Infrastructure.Repositories.RoomRentalRepositories
{
    public class RoomRentalRepository : IRoomRentalRepository
    {
        private readonly RoomRentalManagerServerDbContext _context;
        private readonly ILogger<RoomRentalRepository> _logger;
        public RoomRentalRepository(RoomRentalManagerServerDbContext context, ILogger<RoomRentalRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task AddAsync(RoomRental roomRental)
        {
            try
            {
                await _context.RoomRentals.AddAsync(roomRental);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add room rental");
                throw;
            }
        }

        public async Task DeleteAsync(long id)
        {
            try
            {
                var roomRental = await _context.RoomRentals.FirstOrDefaultAsync(x => x.Id == id);
                if (roomRental == null)
                {
                    throw new KeyNotFoundException($"Room rental with id {id} not found.");
                }

                _context.RoomRentals.Remove(roomRental);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete room rental with id {Id}", id);
                throw;
            }
        }

        public Task<IQueryable<RoomRental>> GetAllRoomRentalAsync()
        {
            try
            {
                // Return IQueryable for further composition; AsNoTracking for read-only
                return Task.FromResult(_context.RoomRentals.AsNoTracking().AsQueryable());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all room rentals");
                throw;
            }
        }

        public async Task<RoomRental?> GetRoomRetalById(long id)
        {
            try
            {
                return await _context.RoomRentals.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get room rental by id {Id}", id);
                throw;
            }
        }

        public async Task UpdateAsync(RoomRental roomRental)
        {
            try
            {
                var existRoomRental = await _context.RoomRentals.FirstOrDefaultAsync(x => x.Id == roomRental.Id);
                if (existRoomRental == null)
                {
                    throw new KeyNotFoundException($"Room rental with id {roomRental.Id} not found.");
                }

                // Update mutable fields only. Preserve CreatedDate/CreatorUser from existing entity.
                existRoomRental.RoomNumber = roomRental.RoomNumber;
                existRoomRental.RoomType = roomRental.RoomType;
                existRoomRental.Price = roomRental.Price;
                existRoomRental.StatusRoom = roomRental.StatusRoom;
                existRoomRental.Note = roomRental.Note;
                existRoomRental.Area = roomRental.Area;
                existRoomRental.ImagesDescription = roomRental.ImagesDescription;
                existRoomRental.UpdatedDate = DateTime.UtcNow;
                existRoomRental.LastUpdateUser = roomRental.LastUpdateUser;
                // keep existRoomRental.CreatedDate and CreatorUser unchanged

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update room rental with id {Id}", roomRental.Id);
                throw;
            }
        }
    }
}
