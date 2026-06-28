using RoomRentalManagerServer.Domain.ModelEntities.Invoices;

namespace RoomRentalManagerServer.Domain.Interfaces.InvoiceInterfaces
{
    public interface IInvoiceItemRepository
    {
        Task AddRangeAsync(IEnumerable<InvoiceItem> items);
        Task<List<InvoiceItem>> GetByInvoiceIdAsync(long invoiceId);
    }
}
