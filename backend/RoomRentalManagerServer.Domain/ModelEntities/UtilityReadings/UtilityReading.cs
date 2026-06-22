using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoomRentalManagerServer.Domain.ModelEntities.UtilityReadings
{
    [Table("utilityReading")]
    public class UtilityReading
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("contractId")]
        public long ContractId { get; set; }

        [Column("month")]
        public int Month { get; set; }

        [Column("year")]
        public int Year { get; set; }

        [Column("oldElectricIndex")]
        public decimal OldElectricIndex { get; set; }

        [Column("newElectricIndex")]
        public decimal NewElectricIndex { get; set; }

        [Column("electricUsage")]
        public decimal ElectricUsage { get; set; }

        [Column("oldWaterIndex")]
        public decimal OldWaterIndex { get; set; }

        [Column("newWaterIndex")]
        public decimal NewWaterIndex { get; set; }

        [Column("waterUsage")]
        public decimal WaterUsage { get; set; }

        [Column("electricUnitPrice")]
        public decimal ElectricUnitPrice { get; set; }

        [Column("waterUnitPrice")]
        public decimal WaterUnitPrice { get; set; }

        [Column("status")]
        public UtilityReadingStatus Status { get; set; }

        [Column("createdAt")]
        public DateTime CreatedAt { get; set; }

        [Column("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [Column("creatorUser")]
        public string CreatorUser { get; set; } = string.Empty;

        [Column("updaterUser")]
        public string UpdaterUser { get; set; } = string.Empty;
    }

    public enum UtilityReadingStatus
    {
        [Display(Name = "Đã xác nhận")]
        Confirmed = 1,

        [Display(Name = "Đã tạo hóa đơn")]
        InvoiceGenerated = 2
    }
}
