using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomRentalManagerServer.Domain.ModelEntities.Contracts
{
    [Table("contract")]
    public class Contract
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("roomRentalId")]
        public long RoomRentalId { get; set; }

        [Column("tenantId")]
        public long TenantId { get; set; }

        [Column("startDate")]
        public DateTime StartDate { get; set; }

        [Column("endDate")]
        public DateTime EndDate { get; set; }

        [Column("depositAmout")]
        public decimal DepositAmout { get; set; }

        [Column("monthlyRent")]
        public decimal MonthlyRent { get; set; }

        [Column("statusContract")]
        public StatusContract StatusContract { get; set; }

        [Column("createdAt")]
        public DateTime CreatedAt { get; set; }

        [Column("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [Column("creatorUser")]
        public string CreatorUser { get; set; }

        [Column("updaterUser")] 
        public string UpdaterUser { get; set; }
    }

    public enum StatusContract
    {
        [Display(Name = "Đang kích hoạt")]
        Active = 1,
        [Display(Name = "Ngưng kích hoạt")]
        Inactive = 2,
        [Display(Name = "Đã hủy")]
        Canceled = 3
    }
}
