using RoomRentalManagerServer.Domain.ModelEntities.UtilityReadings;

namespace RoomRentalManagerServer.Application.Model.UtilityReadingsModel.Dto
{
    public class UtilityReadingDto
    {
        public long Id { get; set; }
        public long ContractId { get; set; }
        public string? RoomName { get; set; }
        public string? TenantName { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal OldElectricIndex { get; set; }
        public decimal NewElectricIndex { get; set; }
        public decimal ElectricUsage { get; set; }
        public decimal ElectricUnitPrice { get; set; }
        public UtilityReadingStatus Status { get; set; }
        public bool IsLockedByPayment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CreatorUser { get; set; } = string.Empty;
        public string UpdaterUser { get; set; } = string.Empty;
    }
}
