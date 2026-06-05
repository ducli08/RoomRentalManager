using RoomRentalManagerServer.Domain.ModelEntities.Invoices;

namespace RoomRentalManagerServer.Domain.Interfaces.InvoiceInterfaces
{
    public interface IInvoiceRepository
    {
        IQueryable<Invoice> Query();
        Task<Invoice?> GetByIdAsync(long id, bool asNoTracking = true);
        Task AddAsync(Invoice invoice);
        Task UpdateAsync(Invoice invoice);
    }
}

