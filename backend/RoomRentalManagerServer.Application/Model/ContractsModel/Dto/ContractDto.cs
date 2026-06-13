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

        [Display(Name = "Trạng thái", Order = 9)]
        public StatusContract StatusContract { get; set; }

        [Display(Name = "Ngày tạo", Order = 10)]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Ngày cập nhật", Order = 11)]
        public DateTime UpdatedAt { get; set; }

        [Display(Name = "Người tạo", Order = 12)]
        public string CreatorUser { get; set; }

        [Display(Name = "Người cập nhật", Order = 13)]
        public string UpdaterUser { get; set; }
    }
}
