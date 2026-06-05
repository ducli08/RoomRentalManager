using RoomRentalManagerServer.Domain.ModelEntities;

namespace RoomRentalManagerServer.Domain.Interfaces
{
    public interface IPermissionRepository
    {
        Task<IQueryable<Permission>> GetAllQueryAsync();
        Task<List<Permission>> GetAllPermissionByListIdAsync(List<long> listPermissionId);
    }
}
