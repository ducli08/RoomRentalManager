namespace RoomRentalManagerServer.Application.Model.UtilityReadingsModel.Dto
{
    public class CreateOrEditUtilityReadingDto
    {
        public long? Id { get; set; }
        public long ContractId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal NewElectricIndex { get; set; }
        public decimal NewWaterIndex { get; set; }
    }
}
