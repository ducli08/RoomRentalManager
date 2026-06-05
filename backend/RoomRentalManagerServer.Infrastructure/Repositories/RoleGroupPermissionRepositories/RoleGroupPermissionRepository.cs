using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoomRentalManagerServer.Domain.Interfaces.RoleGroupPermissionInterfaces;
using RoomRentalManagerServer.Domain.ModelEntities.RoleGroupPermission;
using RoomRentalManagerServer.Infrastructure.Data;

namespace RoomRentalManagerServer.Infrastructure.Repositories.RoleGroupRoleRepositories
{
    public class RoleGroupPermissionRepository : IRoleGroupPermissionRepository
    {
        private readonly RoomRentalManagerServerDbContext _context;
        private readonly ILogger<RoleGroupPermissionRepository> _logger;
        public RoleGroupPermissionRepository(RoomRentalManagerServerDbContext context, ILogger<RoleGroupPermissionRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<RoleGroupPermission> AddAsync(RoleGroupPermission entity)
        {
            try
            {
                await _context.RoleGroupPermission.AddAsync(entity);
                await _context.SaveChangesAsync();
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to add RoleGroupPermission: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteActivePermissionByRoleGroupIdAsync(long id)
        {
            try
            {
                var entity = await _context.RoleGroupPermission.Where(x => x.RoleGroupId == id).ToListAsync();
                if (entity == null) return false;
                _context.RoleGroupPermission.RemoveRange(entity);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to delete RoleGroupPermission: {ex.Message}");
                throw;
            }
        }

        public async Task<IQueryable<RoleGroupPermission>> GetAllQueryAsync()
        {
            try
            {
                return _context.RoleGroupPermission.AsQueryable().AsNoTracking();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get all RoleGroupPermission: {ex.Message}");
                throw;
            }
        }

        public async Task<IQueryable<RoleGroupPermission>> GetByIdAsync(long id)
        {
            try
            {
                return _context.RoleGroupPermission.AsNoTracking().Where(x => x.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get RoleGroupPermission by id: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> UpdateAsync(RoleGroupPermission entity)
        {
            try
            {
                var data = _context.RoleGroupPermission.FirstOrDefault(x => x.Id == entity.Id);
                if (data == null) return false;
                _context.RoleGroupPermission.Update(entity);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to update RoleGroupPermission: {ex.Message}");
                throw;
            }
        }

        public async Task<IQueryable<RoleGroupPermission>> GetByRoleGroupIdAsync(long roleGroupId)
        {
            try
            {
                return _context.RoleGroupPermission.Where(x => x.RoleGroupId == roleGroupId).AsQueryable().AsNoTracking();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get RoleGroupRole by roleGroupId: {ex.Message}");
                throw;
            }
        }
    }
}
