using RoomRentalManagerServer.Domain.ModelEntities.Contracts;

namespace RoomRentalManagerServer.Application.Model.ContractsModel.Dto
{
    public class CreateOrEditContractDto
    {
        public long? Id { get; set; }
        public long RoomRentalId { get; set; }
        public long TenantId { get; set; }
        public long[] TenantIds { get; set; } = Array.Empty<long>();
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? DepositAmout { get; set; }
        public string? MonthlyRent { get; set; }
        public string? ElectricUnitPrice { get; set; }
        public string? WaterUnitPrice { get; set; }
        public string? GarbageFeePerYear { get; set; }
        public StatusContract StatusContract { get; set; }
    }
}
