using RoomRentalManagerServer.Application.Model.Roles.Dto;

namespace RoomRentalManagerServer.Application.Interfaces
{
    public interface IRoleAppService
    {
        Task<List<RoleDto>> GetAllRoleAsync();
    }
}
