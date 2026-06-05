using RoomRentalManagerServer.Domain.ModelEntities.Invoices;

namespace RoomRentalManagerServer.Application.Model.InvoicesModel.Dto
{
    public class InvoiceDto
    {
        public long Id { get; set; }
        public long ContractId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal BalanceDue { get; set; }
        public bool IsOverdue { get; set; }
        public InvoiceStatus Status { get; set; }
    }
}

