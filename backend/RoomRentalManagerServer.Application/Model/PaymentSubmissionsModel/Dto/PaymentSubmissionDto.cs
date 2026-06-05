using RoomRentalManagerServer.Domain.ModelEntities.PaymentSubmissions;

namespace RoomRentalManagerServer.Application.Model.PaymentSubmissionsModel.Dto
{
    public class PaymentSubmissionDto
    {
        public long Id { get; set; }
        public long InvoiceId { get; set; }
        public decimal DeclaredAmount { get; set; }
        public string EvidenceUrl { get; set; }
        public PaymentSubmissionStatus Status { get; set; }
        public string? RejectedReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatorUser { get; set; }
    }
}

