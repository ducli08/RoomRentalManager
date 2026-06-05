using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoomRentalManagerServer.Domain.ModelEntities.PaymentSubmissions
{
    [Table("payment_submission")]
    public class PaymentSubmission
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("invoiceId")]
        public long InvoiceId { get; set; }

        [Column("declaredAmount")]
        public decimal DeclaredAmount { get; set; }

        [Column("evidenceUrl")]
        public string EvidenceUrl { get; set; }

        [Column("evidencePublicId")]
        public string EvidencePublicId { get; set; }

        [Column("status")]
        public PaymentSubmissionStatus Status { get; set; }

        [Column("rejectedReason")]
        public string? RejectedReason { get; set; }

        [Column("createdAt")]
        public DateTime CreatedAt { get; set; }

        [Column("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [Column("creatorUser")]
        public string CreatorUser { get; set; }

        [Column("lastUpdateUser")]
        public string LastUpdateUser { get; set; }
    }

    public enum PaymentSubmissionStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3
    }
}

