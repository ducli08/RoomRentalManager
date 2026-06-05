using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoomRentalManagerServer.Application.Interfaces;
using RoomRentalManagerServer.Application.Model.PermissionModel.Dto;
using RoomRentalManagerServer.Application.Model.Roles.Dto;
using RoomRentalManagerServer.Domain.Interfaces;
using RoomRentalManagerServer.Domain.Interfaces.RoleInterfaces;

namespace RoomRentalManagerServer.Application.Services
{
    public class RoleAppService : IRoleAppService
    {
        private readonly ILogger<RoleAppService> _logger;
        private readonly IMapper _mapper;
        private readonly IRoleRepository _roleRepository;
        private readonly IRolePermissionRepository _rolePermissionRepository;
        private readonly IPermissionRepository _permissionRepository;
        public RoleAppService(ILogger<RoleAppService> logger, IRoleRepository roleRepository,
            IMapper mapper, IRolePermissionRepository rolePermissionRepository, IPermissionRepository permissionRepository)
        {
            _logger = logger;
            _mapper = mapper;
            _roleRepository = roleRepository;
            _rolePermissionRepository = rolePermissionRepository;
            _permissionRepository = permissionRepository;
        }

        public async Task<List<RoleDto>> GetAllRoleAsync()
        {
            try
            {
                var query = await _roleRepository.GetAllQueryAsync();
                var roles = await query.ToListAsync(); 
                var roleIds = query.Select(r => r.Id).ToList();
                var rolePermissions = await _rolePermissionRepository.GetAllRolePermissionByListRoleIdAsync(roleIds);
                var permissionIds = rolePermissions.Select(x => x.PermissionId).Distinct().ToList();
                var permissions = await _permissionRepository.GetAllPermissionByListIdAsync(permissionIds);
                var roleDtos = new List<RoleDto>();
                foreach (var role in roles )
                {
                    var roleDto = new RoleDto();
                    roleDto.Id = role.Id;
                    roleDto.Name = role.Name;
                    var rolePermissionForRole = rolePermissions.Where(x => x.RoleId == role.Id).ToList();
                    var permissionIdForRole = rolePermissionForRole.Select(x => x.PermissionId).ToList();
                    var permissionsForRole = permissions.Where(x => permissionIdForRole.Contains(x.Id))
                        .Select(x => new PermissionDto{Id = x.Id, Name = x.Name}).ToList();
                    roleDto.Permissions = permissionsForRole;
                    roleDtos.Add(roleDto);
                }
                return _mapper.Map<List<RoleDto>>(roleDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get all roles: {ex.Message}");
                throw;
            }
        }
    }
}
