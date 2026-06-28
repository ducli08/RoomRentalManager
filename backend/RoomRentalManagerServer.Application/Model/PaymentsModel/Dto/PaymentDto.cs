using RoomRentalManagerServer.Domain.ModelEntities.PaymentAmount;

namespace RoomRentalManagerServer.Application.Model.PaymentsModel.Dto
{
    public class PaymentDto
    {
        public long Id { get; set; }
        public long InvoiceId { get; set; }
        public PaymentMethod Method { get; set; }
        public PaymentStatus Status { get; set; }
        public decimal Amount { get; set; }
        public string ReferenceCode { get; set; } = string.Empty;
        public string? EvidenceUrl { get; set; }
        public string? RejectedReason { get; set; }
        public string? CancelledReason { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? Note { get; set; }
        public DateTime? PaymentDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatorUser { get; set; } = string.Empty;
    }

    public class RejectPaymentDto
    {
        public string? Reason { get; set; }
    }

    public class CancelQrPaymentDto
    {
        public string? Reason { get; set; }
    }

    public class CreateQrPaymentResultDto
    {
        public PaymentDto Payment { get; set; } = new();
        public string QrImageUrl { get; set; } = string.Empty;
        public string BankCode { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
    }
}
