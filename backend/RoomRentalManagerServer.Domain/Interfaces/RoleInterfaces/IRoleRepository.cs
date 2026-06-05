using RoomRentalManagerServer.Domain.ModelEntities.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomRentalManagerServer.Domain.Interfaces.RoleInterfaces
{
    public interface IRoleRepository
    {
        Task<Role> AddAsync(Role role);
        Task<bool> DeleteAsync(long id);
        Task<IQueryable<Role>> GetAllQueryAsync();
        Task<Role?> GetByIdAsync(long id);
        Task<bool> UpdateAsync(Role role);
    }
}
