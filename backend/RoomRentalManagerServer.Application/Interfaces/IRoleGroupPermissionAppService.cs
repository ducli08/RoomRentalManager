using RoomRentalManagerServer.Domain.ModelEntities;

namespace RoomRentalManagerServer.Application.Interfaces
{
    public interface IRoleGroupPermissionAppService
    {
        Task<List<long>> GetActivePermissionByRoleGroupIdAsync(long roleGroupId);
        Task<bool> DeleteActivePermissionByRoleGroupIdAsync(long roleGroupId);
        Task<List<RolePermission>> GetActiveRolePermissionByPermissionId(List<long> permissionIds);
    }
}
