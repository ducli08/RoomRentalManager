using RoomRentalManagerServer.Domain.ModelEntities.RoleGroupPermission;

namespace RoomRentalManagerServer.Domain.Interfaces.RoleGroupPermissionInterfaces
{
    public interface IRoleGroupPermissionRepository
    {
        Task<RoleGroupPermission> AddAsync(RoleGroupPermission entity);
        Task<bool> DeleteActivePermissionByRoleGroupIdAsync(long id);
        Task<IQueryable<RoleGroupPermission>> GetByIdAsync(long id);
        Task<bool> UpdateAsync(RoleGroupPermission entity);
        Task<IQueryable<RoleGroupPermission>> GetAllQueryAsync();
        Task<IQueryable<RoleGroupPermission>> GetByRoleGroupIdAsync(long roleGroupId);
    }
}
