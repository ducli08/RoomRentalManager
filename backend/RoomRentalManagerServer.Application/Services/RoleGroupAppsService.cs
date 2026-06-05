using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RoomRentalManagerServer.Application.Common.CommonDto;
using RoomRentalManagerServer.Application.Interfaces;
using RoomRentalManagerServer.Application.Model.RoleGroupsModel.Dto;
using RoomRentalManagerServer.Domain.Interfaces.RedisCache;
using RoomRentalManagerServer.Domain.Interfaces.RoleGroupInterfaces;
using RoomRentalManagerServer.Domain.Interfaces.RoleGroupPermissionInterfaces;
using RoomRentalManagerServer.Domain.ModelEntities.Provinces;
using RoomRentalManagerServer.Domain.ModelEntities.RoleGroupPermission;
using RoomRentalManagerServer.Domain.ModelEntities.RoleGroups;
using System.Diagnostics;

namespace RoomRentalManagerServer.Application.Services
{
    public class RoleGroupAppsService : IRoleGroupAppService
    {
        private readonly ILogger<RoleGroupAppsService> _logger;
        private readonly IMapper _mapper;
        private readonly IRoleGroupRepository _roleGroupRepository;
        private readonly ICurrentUserAppService _currentUserAppService;
        private readonly IRoleGroupPermissionRepository _roleGroupPermissionRepository;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IConfiguration _configuration;
        public RoleGroupAppsService(ILogger<RoleGroupAppsService> logger, IRoleGroupRepository roleGroupRepository, IMapper mapper, ICurrentUserAppService currentUserAppService,
            IRoleGroupPermissionRepository roleGroupPermissionRepository, IRedisCacheService redisCacheService, IConfiguration configuration)
        {
            _logger = logger;
            _mapper = mapper;
            _roleGroupRepository = roleGroupRepository;
            _currentUserAppService = currentUserAppService;
            _roleGroupPermissionRepository = roleGroupPermissionRepository;
            _redisCacheService = redisCacheService;
            _configuration = configuration;
        }

        public async Task<PagedResultDto<RoleGroupDto>> GetAllRoleGroupsAsync(PagedRequestDto<RoleGroupFilterDto> pagedRequestRoleGroupDto)
        {
            try
            {
                var queryGetAllRoleGroup = await _roleGroupRepository.GetAllRoleGroupAsync();

                // Filter by CreatedDate
                if (pagedRequestRoleGroupDto.Filter?.CreatedAt.HasValue == true)
                {
                    var filterDate = pagedRequestRoleGroupDto.Filter.CreatedAt.Value.Date;
                    queryGetAllRoleGroup = queryGetAllRoleGroup.Where(x => x.CreatedAt.Date == filterDate);
                }

                // Filter by UpdatedDate
                if (pagedRequestRoleGroupDto.Filter?.UpdatedAt.HasValue == true)
                {
                    var filterDate = pagedRequestRoleGroupDto.Filter.UpdatedAt.Value.Date;
                    queryGetAllRoleGroup = queryGetAllRoleGroup.Where(x => x.UpdatedAt.Date == filterDate);
                }

                // Filter by CreatorUser (contains search)
                if (!string.IsNullOrEmpty(pagedRequestRoleGroupDto.Filter?.CreatorUser))
                {
                    queryGetAllRoleGroup = queryGetAllRoleGroup.Where(x => x.CreatorUser != null && x.CreatorUser.Contains(pagedRequestRoleGroupDto.Filter.CreatorUser));
                }

                // Filter by LastUpdateUser (contains search)
                if (!string.IsNullOrEmpty(pagedRequestRoleGroupDto.Filter?.LastUpdateUser))
                {
                    queryGetAllRoleGroup = queryGetAllRoleGroup.Where(x => x.LastUpdateUser != null && x.LastUpdateUser.Contains(pagedRequestRoleGroupDto.Filter.LastUpdateUser));
                }

                // Apply sorting
                if (!string.IsNullOrEmpty(pagedRequestRoleGroupDto.SortOrder) && !string.IsNullOrEmpty(pagedRequestRoleGroupDto.SortBy))
                {
                    queryGetAllRoleGroup = pagedRequestRoleGroupDto.SortOrder == "desc"
                    ? queryGetAllRoleGroup.OrderByDescending(x => EF.Property<object>(x, pagedRequestRoleGroupDto.SortBy))
                    : queryGetAllRoleGroup.OrderBy(x => EF.Property<object>(x, pagedRequestRoleGroupDto.SortBy));
                }

                var total = await queryGetAllRoleGroup.CountAsync();
                var roleGroups = await queryGetAllRoleGroup.Skip((pagedRequestRoleGroupDto.Page - 1) * pagedRequestRoleGroupDto.PageSize).Take(pagedRequestRoleGroupDto.PageSize).ToListAsync();
                var roleGroupDtos = roleGroups.Select(r => _mapper.Map<RoleGroupDto>(r)).ToList();
                return new PagedResultDto<RoleGroupDto>(roleGroupDtos, total);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get all RoleGroups");
                throw;
            }
        }

        public async Task<RoleGroupDto?> GetRoleGroupByIdAsync(long id)
        {
            try
            {
                var roleGroup = await _roleGroupRepository.GetRoleGroupById(id);
                if (roleGroup == null)
                {
                    return null;
                }
                return _mapper.Map<RoleGroupDto>(roleGroup);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get room rental by ID {Id}", id);
                throw;
            }
        }

        public async Task<bool> CreateOrEditRoleGroupAsync(CreateOrEditRoleGroupDto createOrEditRoleGroupDto)
        {
            if (createOrEditRoleGroupDto == null)
                return false;

            var isUpdate = createOrEditRoleGroupDto.Id.HasValue && createOrEditRoleGroupDto.Id.Value > 0;
            var action = isUpdate ? "Edit" : "Create";

            try
            {
                var roleGroup = _mapper.Map<RoleGroup>(createOrEditRoleGroupDto);
                if (isUpdate)
                {
                    await UpdateRoleGroupAsync(roleGroup);
                    // Remove existing permissions
                    await _roleGroupPermissionRepository.DeleteActivePermissionByRoleGroupIdAsync(roleGroup.Id);
                }
                else
                {
                    await AddRoleGroupAsync(roleGroup);
                }

                if (createOrEditRoleGroupDto.RoleDtos != null)
                {
                    foreach (var item in createOrEditRoleGroupDto.RoleDtos)
                    {
                        if (item.Permissions != null)
                        {
                            foreach (var permission in item.Permissions)
                            {
                                var roleGroupRole = new RoleGroupPermission
                                {
                                    RoleGroupId = roleGroup.Id,
                                    PermissionId = permission.Id
                                };
                                await AddRoleGroupPermission(roleGroupRole);
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to {Action} room rental", action);
                return false;
            }
        }

        public async Task<RoleGroupDto> AddRoleGroupAsync(RoleGroup roleGroup)
        {
            try
            {
                if (!_currentUserAppService.IsAuthenticated)
                {
                    throw new UnauthorizedAccessException("User is not authenticated.");
                }
                var userName = _currentUserAppService.UserName ?? throw new InvalidOperationException("User is null.");
                roleGroup.CreatorUser = userName;
                roleGroup.LastUpdateUser = userName;
                roleGroup.CreatedAt = roleGroup.UpdatedAt = DateTime.UtcNow;
                await _roleGroupRepository.AddAsync(roleGroup);
                return _mapper.Map<RoleGroupDto>(roleGroup);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add room rental");
                throw;
            }
        }

        public async Task<RoleGroupPermission?> AddRoleGroupPermission(RoleGroupPermission roleGroupPermission)
        {
            try
            {
                if (!_currentUserAppService.IsAuthenticated)
                {
                    throw new UnauthorizedAccessException("User is not authenticated.");
                }
                var userName = _currentUserAppService.UserName ?? throw new InvalidOperationException("User is null.");
                roleGroupPermission.CreatedBy = userName;
                roleGroupPermission.UpdatedBy = userName;
                roleGroupPermission.CreatedAt = DateTime.UtcNow;
                roleGroupPermission.UpdatedAt = DateTime.UtcNow;
                await _roleGroupPermissionRepository.AddAsync(roleGroupPermission);
                return roleGroupPermission;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add RoleGroupRole");
                throw;
            }
        }

        public async Task UpdateRoleGroupAsync(RoleGroup roleGroup)
        {
            try
            {
                roleGroup.LastUpdateUser = _currentUserAppService.UserName ?? throw new InvalidOperationException("User is null.");
                roleGroup.UpdatedAt = DateTime.UtcNow;
                await _roleGroupRepository.UpdateAsync(roleGroup);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update room rental");
                throw;
            }
        }

        public async Task<RoleGroupPermission?> UpdateRoleGroupPermission(RoleGroupPermission roleGroupPermission)
        {
            try
            {
                if (!_currentUserAppService.IsAuthenticated)
                {
                    throw new UnauthorizedAccessException("User is not authenticated.");
                }
                var userName = _currentUserAppService.UserName ?? throw new InvalidOperationException("User is null.");
                roleGroupPermission.UpdatedBy = userName;
                roleGroupPermission.UpdatedAt = DateTime.Now;
                await _roleGroupPermissionRepository.UpdateAsync(roleGroupPermission);
                return roleGroupPermission;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add RoleGroupPermission");
                throw;
            }
        }

        public async Task DeleteRoleGroupAsync(long id)
        {
            try
            {
                var roleGroup = await _roleGroupRepository.GetRoleGroupById(id);
                if (roleGroup == null)
                {
                    _logger.LogWarning("Role Group with id {Id} not found when attempting delete", id);
                    throw new KeyNotFoundException($"Role Group with id {id} not found.");
                }
                await _roleGroupRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete room rental with id {Id}", id);
                throw;
            }
        }

        public async Task<List<RoleGroup>> GetAllRoleGroupAsync()
        {
            try
            {
                Stopwatch stC = new Stopwatch();
                stC.Start();
                Stopwatch st = new Stopwatch();
                st.Start();
                var value = await _redisCacheService.GetAsync<RoleGroup>(_configuration["Redis:Keys:RoleGroup"]);
                var roleGroup = new List<RoleGroup>();
                if (value != null && value?.Count != 0)
                {
                    roleGroup = value;
                    stC.Stop();
                    _logger.LogInformation($"GetAllRoleGroupAsync from Redis Cache: {stC.ElapsedMilliseconds} ms");
                }
                else
                {
                    var roleGroupsQuery = await _roleGroupRepository.GetAllRoleGroupAsync();
                    roleGroup = await roleGroupsQuery.ToListAsync();
                    st.Stop();
                    _logger.LogInformation($"GetAllProvincesAsync from Database: {st.ElapsedMilliseconds} ms");
                    if (roleGroup != null)
                    {
                        st.Start();
                        await _redisCacheService.SetAsync<RoleGroup>(_configuration["Redis:Keys:RoleGroup"], roleGroup, TimeSpan.FromMinutes(30));
                        st.Stop();
                        _logger.LogInformation($"Time to set cache Province into Redis {st.ElapsedMilliseconds} ms");
                    }

                }
                return roleGroup;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all provinces");
                throw;
            }
        }
    }
}
