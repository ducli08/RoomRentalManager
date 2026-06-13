using RoomRentalManagerServer.Domain.ModelEntities.Contracts;

namespace RoomRentalManagerServer.Application.Model.ContractsModel.Dto
{
    public class CreateOrEditContractDto
    {
        public long? Id { get; set; }
        public long RoomRentalId { get; set; }
        public long TenantId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? DepositAmout { get; set; }
        public string? MonthlyRent { get; set; }
        public StatusContract StatusContract { get; set; }
    }
}
