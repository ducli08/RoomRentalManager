using RoomRentalManagerServer.Domain.ModelEntities.UtilityReadings;

namespace RoomRentalManagerServer.Application.Model.UtilityReadingsModel.Dto
{
    public class UtilityReadingFilterDto
    {
        public int? Month { get; set; }
        public int? Year { get; set; }
        public long? ContractId { get; set; }
        public UtilityReadingStatus? Status { get; set; }
        public long? RoomRentalId { get; set; }
        public long? TenantId { get; set; }
        public string? CreatorUser { get; set; }
        public string? UpdaterUser { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
