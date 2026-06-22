namespace RoomRentalManagerServer.Application.Model.UtilityReadingsModel.Dto
{
    public class UtilityReadingPrepareDto
    {
        public long ContractId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string? RoomName { get; set; }
        public string? TenantName { get; set; }
        public DateTime? ContractStartDate { get; set; }
        public DateTime? ContractEndDate { get; set; }
        public decimal OldElectricIndex { get; set; }
        public decimal OldWaterIndex { get; set; }
        public decimal ElectricUnitPrice { get; set; }
        public decimal WaterUnitPrice { get; set; }
        public bool CanSave { get; set; }
        public string? Message { get; set; }
    }
}
