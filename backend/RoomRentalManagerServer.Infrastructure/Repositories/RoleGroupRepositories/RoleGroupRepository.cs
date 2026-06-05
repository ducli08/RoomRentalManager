using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoomRentalManagerServer.Domain.Interfaces.RoleGroupInterfaces;
using RoomRentalManagerServer.Domain.ModelEntities.RoleGroups;
using RoomRentalManagerServer.Infrastructure.Data;

namespace RoomRentalManagerServer.Infrastructure.Repositories.RoleGroupRepositories
{
    public class RoleGroupRepository : IRoleGroupRepository
    {
        private readonly RoomRentalManagerServerDbContext _context;
        private readonly ILogger<RoleGroupRepository> _logger;
        public RoleGroupRepository(RoomRentalManagerServerDbContext context, ILogger<RoleGroupRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task AddAsync(RoleGroup roleGroup)
        {
            try
            {
                await _context.RoleGroup.AddAsync(roleGroup);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add rooleGroup");
                throw;
            }
        }

        public async Task DeleteAsync(long id)
        {
            try
            {
                var roleGroup = await _context.RoleGroup.FirstOrDefaultAsync(x => x.Id == id);
                if (roleGroup == null)
                {
                    throw new KeyNotFoundException($"Role group with id {id} not found.");
                }

                _context.RoleGroup.Remove(roleGroup);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete room rental with id {Id}", id);
                throw;
            }
        }

        public Task<IQueryable<RoleGroup>> GetAllRoleGroupAsync()
        {
            try
            {
                return Task.FromResult(_context.RoleGroup.AsNoTracking().AsQueryable());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all room rentals");
                throw;
            }
        }

        public async Task<RoleGroup?> GetRoleGroupById(long id)
        {
            try
            {
                return await _context.RoleGroup.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get role group by id {Id}", id);
                throw;
            }
        }

        public async Task UpdateAsync(RoleGroup roleGroup)
        {
            try
            {
                var existRoleGroup= await _context.RoleGroup.FirstOrDefaultAsync(x => x.Id == roleGroup.Id);
                if (existRoleGroup == null)
                {
                    throw new KeyNotFoundException($"Room rental with id {roleGroup.Id} not found.");
                }

                // Update mutable fields only. Preserve CreatedDate/CreatorUser from existing entity.
                existRoleGroup.Name = roleGroup.Name;
                existRoleGroup.Active = roleGroup.Active;
                existRoleGroup.UpdatedAt = DateTime.UtcNow;
                existRoleGroup.LastUpdateUser = roleGroup.LastUpdateUser;
                // keep existRoomRental.CreatedDate and CreatorUser unchanged

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update role group with id {Id}", roleGroup.Id);
                throw;
            }
        }
    }
}
