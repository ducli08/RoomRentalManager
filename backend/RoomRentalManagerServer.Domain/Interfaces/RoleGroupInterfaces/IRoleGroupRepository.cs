using RoomRentalManagerServer.Domain.ModelEntities.RoleGroups;

namespace RoomRentalManagerServer.Domain.Interfaces.RoleGroupInterfaces
{
    public interface IRoleGroupRepository
    {
        Task AddAsync(RoleGroup roleGroup);
        Task DeleteAsync(long id);
        Task<RoleGroup?> GetRoleGroupById(long id);
        Task UpdateAsync(RoleGroup roleGroup);
        Task<IQueryable<RoleGroup>> GetAllRoleGroupAsync();
    }
}
