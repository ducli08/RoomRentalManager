using RoomRentalManagerServer.Domain.ModelEntities.Invoices;

namespace RoomRentalManagerServer.Application.Model.InvoicesModel.Dto
{
    public class InvoiceDto
    {
        public long Id { get; set; }
        public long ContractId { get; set; }
        public long UtilityReadingId { get; set; }
        public string InvoiceCode { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal BalanceDue { get; set; }
        public bool IsOverdue { get; set; }
        public int DaysLate { get; set; }
        public InvoiceStatus Status { get; set; }
    }
}
