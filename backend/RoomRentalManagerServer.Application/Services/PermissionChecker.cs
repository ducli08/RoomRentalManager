using RoomRentalManagerServer.Application.Interfaces;
using RoomRentalManagerServer.Domain.Interfaces.UserInterfaces;

namespace RoomRentalManagerServer.Application.Services
{
    public class PermissionChecker : IPermissionChecker
    {
        private readonly ICurrentUserAppService _currentUser;
        private readonly IUserRepository _userRepository;
        private readonly IRoleGroupPermissionAppService _roleGroupPermissionAppService;
        private readonly IRoleAppService _roleAppService;

        public PermissionChecker(
            ICurrentUserAppService currentUser,
            IUserRepository userRepository,
            IRoleGroupPermissionAppService roleGroupPermissionAppService,
            IRoleAppService roleAppService)
        {
            _currentUser = currentUser;
            _userRepository = userRepository;
            _roleGroupPermissionAppService = roleGroupPermissionAppService;
            _roleAppService = roleAppService;
        }

        public async Task<bool> HasPermissionAsync(string permission)
        {
            var permissions = await GetUserPermissionsAsync();
            return permissions.Contains(permission);
        }

        public async Task<bool> HasAnyPermissionAsync(params string[] permissions)
        {
            if (permissions.Length == 0) return false;
            var userPermissions = await GetUserPermissionsAsync();
            return permissions.Any(userPermissions.Contains);
        }

        private async Task<HashSet<string>> GetUserPermissionsAsync()
        {
            if (!_currentUser.IsAuthenticated || _currentUser.GetUserId is null)
            {
                return new HashSet<string>();
            }

            var user = await _userRepository.GetByIdAsync(_currentUser.GetUserId.Value);
            if (user == null || user.RoleGroupId <= 0)
            {
                return new HashSet<string>();
            }

            var permissionIds = await _roleGroupPermissionAppService.GetActivePermissionByRoleGroupIdAsync(user.RoleGroupId);
            if (permissionIds.Count == 0)
            {
                return new HashSet<string>();
            }

            var rolePermissions = await _roleGroupPermissionAppService.GetActiveRolePermissionByPermissionId(permissionIds);
            var roles = await _roleAppService.GetAllRoleAsync();
            var rolesDic = roles.ToDictionary(x => x.Id);

            var result = new HashSet<string>(StringComparer.Ordinal);
            foreach (var item in rolePermissions)
            {
                if (!rolesDic.TryGetValue(item.RoleId, out var role) || role?.Permissions == null)
                {
                    continue;
                }

                var permissionName = role.Permissions.FirstOrDefault(x => x.Id == item.PermissionId)?.Name;
                if (!string.IsNullOrEmpty(permissionName))
                {
                    result.Add($"{role.Name}.{permissionName}");
                }
            }

            return result;
        }
    }
}
