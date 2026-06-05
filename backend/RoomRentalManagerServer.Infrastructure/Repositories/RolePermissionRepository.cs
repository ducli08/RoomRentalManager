using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoomRentalManagerServer.Domain.Interfaces;
using RoomRentalManagerServer.Domain.ModelEntities;
using RoomRentalManagerServer.Infrastructure.Data;

namespace RoomRentalManagerServer.Infrastructure.Repositories
{
    public class RolePermissionRepository : IRolePermissionRepository
    {
        private readonly RoomRentalManagerServerDbContext _context;
        private readonly ILogger<RolePermissionRepository> _logger;
        public RolePermissionRepository(RoomRentalManagerServerDbContext context, ILogger<RolePermissionRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IQueryable<RolePermission>> GetAllQueryAsync()
        {
            try
            {
                return _context.RolePermission.AsQueryable().AsNoTracking();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get all role permissions: {ex.Message}");
                throw;
            }
        }
        
        public async Task<List<RolePermission>> GetAllRolePermissionByListPermissionIdsAsync(List<long> permissionIds)
        {
            try
            {
                var result = _context.RolePermission
                    .Where(rp => permissionIds.Contains(rp.PermissionId))
                    .AsNoTracking();

                return await result.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get role permissions by list of role IDs: {ex.Message}");
                throw;
            }
        }

        public async Task<List<RolePermission>> GetAllRolePermissionByListRoleIdAsync(List<long> listRoleId)
        {
            try
            {
                var result = _context.RolePermission
                    .Where(rp => listRoleId.Contains(rp.RoleId))
                    .AsNoTracking();

                return await result.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get role permissions by list of role IDs: {ex.Message}");
                throw;
            }
        }
    }
}
