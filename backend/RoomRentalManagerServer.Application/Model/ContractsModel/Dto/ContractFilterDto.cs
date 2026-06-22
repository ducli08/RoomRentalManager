using RoomRentalManagerServer.Domain.ModelEntities.Contracts;

namespace RoomRentalManagerServer.Application.Model.ContractsModel.Dto
{
    public class ContractFilterDto
    {
        public long RoomRentalId { get; set; }
        public long TenantId { get; set; }
        public StatusContract StatusContract { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public DateTime? EndDateFrom { get; set; }
        public DateTime? EndDateTo { get; set; }
        public string? CreatorUser { get; set; }
    }
}
