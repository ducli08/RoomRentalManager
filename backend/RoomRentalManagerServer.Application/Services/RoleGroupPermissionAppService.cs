using RoomRentalManagerServer.Application.Interfaces;
using RoomRentalManagerServer.Domain.Interfaces;
using RoomRentalManagerServer.Domain.Interfaces.RoleGroupPermissionInterfaces;
using RoomRentalManagerServer.Domain.ModelEntities;

namespace RoomRentalManagerServer.Application.Services
{
    public class RoleGroupPermissionAppService : IRoleGroupPermissionAppService
    {
        private readonly IRoleGroupPermissionRepository _roleGroupPermissionRepository;
        private readonly IRolePermissionRepository _rolePermissionRepository;
        public RoleGroupPermissionAppService(IRoleGroupPermissionRepository roleGroupPermissionRepository, IRolePermissionRepository rolePermissionRepository)
        {
            _roleGroupPermissionRepository = roleGroupPermissionRepository;
            _rolePermissionRepository = rolePermissionRepository;
        }

        public async Task<List<long>> GetActivePermissionByRoleGroupIdAsync(long roleGroupId)
        {
            var query = await _roleGroupPermissionRepository.GetByRoleGroupIdAsync(roleGroupId);
            var list = query.Select(x => x.PermissionId).ToList();
            return list;
        }

        public async Task<bool> DeleteActivePermissionByRoleGroupIdAsync(long roleGroupId)
        {
            var res = await _roleGroupPermissionRepository.DeleteActivePermissionByRoleGroupIdAsync(roleGroupId);
            return res;
        }

        public async Task<List<RolePermission>> GetActiveRolePermissionByPermissionId(List<long> permissionIds)
        {
            var rolePermissions = await _rolePermissionRepository.GetAllRolePermissionByListPermissionIdsAsync(permissionIds);
            return rolePermissions;
        }

    }
}
