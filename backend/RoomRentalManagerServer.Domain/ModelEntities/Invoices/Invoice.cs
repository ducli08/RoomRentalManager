using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoomRentalManagerServer.Domain.ModelEntities.Invoices
{
    [Table("invoice")]
    public class Invoice
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("contractId")]
        public long ContractId { get; set; }

        [Column("utilityReadingId")]
        public long UtilityReadingId { get; set; }

        [Column("month")]
        public int Month { get; set; }

        [Column("year")]
        public int Year { get; set; }

        [Column("invoiceCode")]
        public string InvoiceCode { get; set; } = string.Empty;

        [Column("roomNameSnapshot")]
        public string RoomNameSnapshot { get; set; } = string.Empty;

        [Column("tenantNameSnapshot")]
        public string TenantNameSnapshot { get; set; } = string.Empty;

        [Column("occupancyDaysSnapshot")]
        public int OccupancyDaysSnapshot { get; set; }

        [Column("periodStartSnapshot")]
        public DateTime PeriodStartSnapshot { get; set; }

        [Column("periodEndSnapshot")]
        public DateTime PeriodEndSnapshot { get; set; }

        [Column("invoiceDate")]
        public DateTime InvoiceDate { get; set; }

        [Column("dueDate")]
        public DateTime DueDate { get; set; }

        [Column("totalAmount")]
        public decimal TotalAmount { get; set; }

        [Column("paidAmount")]
        public decimal PaidAmount { get; set; }

        [Column("invoiceStatus")]
        public InvoiceStatus Status { get; set; }

        [Column("cancelledAt")]
        public DateTime? CancelledAt { get; set; }

        [Column("createdAt")]
        public DateTime CreatedAt { get; set; }

        [Column("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [Column("creatorUser")]
        public string CreatorUser { get; set; } = string.Empty;

        [Column("lastUpdateUser")]
        public string LastUpdateUser { get; set; } = string.Empty;
    }

    public enum InvoiceStatus
    {
        [Display(Name = "Chưa thanh toán")]
        Unpaid = 1,
        [Display(Name = "Chờ chuyển khoản")]
        WaitingTransfer = 2,
        [Display(Name = "Đang chờ xác nhận")]
        WaitingApproval = 3,
        [Display(Name = "Đã thanh toán")]
        Paid = 4,
        [Display(Name = "Đã hủy")]
        Cancelled = 5
    }
}
