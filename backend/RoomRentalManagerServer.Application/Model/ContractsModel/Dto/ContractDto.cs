using RoomRentalManagerServer.Domain.ModelEntities.Contracts;
using System.ComponentModel.DataAnnotations;

namespace RoomRentalManagerServer.Application.Model.ContractsModel.Dto
{
    public class ContractDto
    {
        public long Id { get; set; }

        [Display(Name = "Phòng trọ", Order = 1)]
        public long RoomRentalId { get; set; }

        [Display(Name = "Tên phòng", Order = 2)]
        public string? RoomName { get; set; }

        [Display(Name = "Người thuê", Order = 3)]
        public long TenantId { get; set; }

        [Display(Name = "Tên người thuê", Order = 4)]
        public string? TenantName { get; set; }

        [Display(Name = "Ngày bắt đầu", Order = 5)]
        public DateTime StartDate { get; set; }

        [Display(Name = "Ngày kết thúc", Order = 6)]
        public DateTime EndDate { get; set; }

        [Display(Name = "Tiền cọc", Order = 7)]
        public decimal DepositAmout { get; set; }

        [Display(Name = "Tiền thuê hàng tháng", Order = 8)]
        public decimal MonthlyRent { get; set; }

        [Display(Name = "Đơn giá điện", Order = 9)]
        public decimal ElectricUnitPrice { get; set; }

        [Display(Name = "Đơn giá nước", Order = 10)]
        public decimal WaterUnitPrice { get; set; }

        [Display(Name = "Tiền rác/năm", Order = 11)]
        public decimal GarbageFeePerYear { get; set; }

        [Display(Name = "Trạng thái", Order = 12)]
        public StatusContract StatusContract { get; set; }

        [Display(Name = "Ngày tạo", Order = 13)]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Ngày cập nhật", Order = 14)]
        public DateTime UpdatedAt { get; set; }

        [Display(Name = "Người tạo", Order = 15)]
        public string CreatorUser { get; set; }

        [Display(Name = "Người cập nhật", Order = 16)]
        public string UpdaterUser { get; set; }
    }
}
