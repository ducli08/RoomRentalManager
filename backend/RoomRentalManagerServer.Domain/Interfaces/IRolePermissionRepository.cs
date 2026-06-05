using RoomRentalManagerServer.Domain.ModelEntities;

namespace RoomRentalManagerServer.Domain.Interfaces
{
    public interface IRolePermissionRepository
    {
        Task<IQueryable<RolePermission>> GetAllQueryAsync();
        Task<List<RolePermission>> GetAllRolePermissionByListPermissionIdsAsync(List<long> permissionIds);
        Task<List<RolePermission>> GetAllRolePermissionByListRoleIdAsync(List<long> listRoleId);
    }
}
