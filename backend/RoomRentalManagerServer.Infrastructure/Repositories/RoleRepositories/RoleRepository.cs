using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoomRentalManagerServer.Domain.Interfaces.RoleInterfaces;
using RoomRentalManagerServer.Domain.ModelEntities.RoleGroups;
using RoomRentalManagerServer.Domain.ModelEntities.Roles;
using RoomRentalManagerServer.Domain.ModelEntities.User;
using RoomRentalManagerServer.Infrastructure.Data;

namespace RoomRentalManagerServer.Infrastructure.Repositories.RoleRepositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly RoomRentalManagerServerDbContext _context;
        private readonly ILogger<RoleRepository> _logger;
        public RoleRepository(RoomRentalManagerServerDbContext context, ILogger<RoleRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Role> AddAsync(Role role)
        {
            try
            {
                await _context.Role.AddAsync(role);
                await _context.SaveChangesAsync();
                return role;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to add role: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(long id)
        {
            try
            {
                var role = await _context.Role.FirstOrDefaultAsync(x => x.Id == id);
                if (role == null) return false;
                _context.Role.Remove(role);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to delete role: {ex.Message}");
                throw;
            }
        }

        public async Task<IQueryable<Role>> GetAllQueryAsync()
        {
            try
            {
                return _context.Role.AsQueryable().AsNoTracking();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get all role: {ex.Message}");
                throw;
            }
        }

        public async Task<Role?> GetByIdAsync(long id)
        {
            try
            {
                return await _context.Role.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get role by id: {ex.Message}");
                throw;
            }

        }

        public async Task<bool> UpdateAsync(Role role)
        {
            try
            {
                var data = _context.Role.FirstOrDefault(x => x.Id == role.Id);
                if (data == null) return false;
                _context.Role.Update(role);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to update role: {ex.Message}");
                throw;
            }

        }
    }
}
