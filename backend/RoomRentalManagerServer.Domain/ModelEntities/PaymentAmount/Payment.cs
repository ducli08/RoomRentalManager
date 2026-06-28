using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoomRentalManagerServer.Domain.ModelEntities.PaymentAmount
{
    [Table("payment")]
    public class Payment
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("invoiceId")]
        public long InvoiceId { get; set; }

        [Column("paymentMethod")]
        public PaymentMethod Method { get; set; }

        [Column("status")]
        public PaymentStatus Status { get; set; }

        [Column("amount")]
        public decimal Amount { get; set; }

        [Column("referenceCode")]
        public string ReferenceCode { get; set; } = string.Empty;

        [Column("evidenceUrl")]
        public string? EvidenceUrl { get; set; }

        [Column("evidencePublicId")]
        public string? EvidencePublicId { get; set; }

        [Column("rejectedReason")]
        public string? RejectedReason { get; set; }

        [Column("cancelledReason")]
        public string? CancelledReason { get; set; }

        [Column("cancelledAt")]
        public DateTime? CancelledAt { get; set; }

        [Column("note")]
        public string? Note { get; set; }

        [Column("paymentDate")]
        public DateTime? PaymentDate { get; set; }

        [Column("createdAt")]
        public DateTime CreatedAt { get; set; }

        [Column("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [Column("creatorUser")]
        public string CreatorUser { get; set; } = string.Empty;

        [Column("lastUpdateUser")]
        public string LastUpdateUser { get; set; } = string.Empty;
    }

    public enum PaymentMethod
    {
        [Display(Name = "Tiền mặt")]
        Cash = 1,
        [Display(Name = "Chuyển khoản")]
        BankTransfer = 2,
        [Display(Name = "Thẻ")]
        Card = 3
    }

    public enum PaymentStatus
    {
        PendingTransfer = 1,
        WaitingForApproval = 2,
        Approved = 3,
        Rejected = 4,
        Cancelled = 5
    }
}
