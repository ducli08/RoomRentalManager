using RoomRentalManagerServer.Domain.ModelEntities.Invoices;

namespace RoomRentalManagerServer.Application.Model.InvoicesModel.Dto
{
    public class InvoiceFilterDto
    {
        public long? ContractId { get; set; }
        public InvoiceStatus? Status { get; set; }
        public bool? IsOverdue { get; set; }
    }
}

