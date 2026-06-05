using RoomRentalManagerServer.Application.Common.CommonDto;
using RoomRentalManagerServer.Application.Model.RoleGroupsModel.Dto;
using RoomRentalManagerServer.Domain.ModelEntities.RoleGroups;

namespace RoomRentalManagerServer.Application.Interfaces
{
    public interface IRoleGroupAppService
    {
        Task<PagedResultDto<RoleGroupDto>> GetAllRoleGroupsAsync(PagedRequestDto<RoleGroupFilterDto> pagedRequestRoleGroupDto);
        Task<RoleGroupDto?> GetRoleGroupByIdAsync(long id);
        Task<bool> CreateOrEditRoleGroupAsync(CreateOrEditRoleGroupDto createOrEditRoleGroupDto);
        Task UpdateRoleGroupAsync(RoleGroup roleGroup);
        Task<RoleGroupDto> AddRoleGroupAsync(RoleGroup roleGroup);
        Task DeleteRoleGroupAsync(long id);
        Task<List<RoleGroup>> GetAllRoleGroupAsync();
    }
}
