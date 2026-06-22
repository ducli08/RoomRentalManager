using RoomRentalManagerServer.Domain.ModelEntities.UtilityReadings;

namespace RoomRentalManagerServer.Application.Interfaces
{
    public interface IPermissionChecker
    {
        Task<bool> HasPermissionAsync(string permission);
        Task<bool> HasAnyPermissionAsync(params string[] permissions);
    }
}
