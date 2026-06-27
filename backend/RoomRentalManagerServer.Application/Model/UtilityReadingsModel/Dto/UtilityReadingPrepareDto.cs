namespace RoomRentalManagerServer.Application.Model.UtilityReadingsModel.Dto
{
    public class UtilityReadingPrepareDto
    {
        public long ContractId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public DateTime? PeriodStart { get; set; }
        public DateTime? PeriodEnd { get; set; }
        public int OccupancyDays { get; set; }
        public bool IsFullPeriod { get; set; }
        public string? RoomName { get; set; }
        public string? TenantName { get; set; }
        public DateTime? ContractStartDate { get; set; }
        public DateTime? ContractEndDate { get; set; }
        public decimal OldElectricIndex { get; set; }
        public decimal ElectricUnitPrice { get; set; }
        public bool CanSave { get; set; }
        public string? Message { get; set; }
    }
}
