using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoomRentalManagerServer.Domain.Interfaces.InvoiceInterfaces;
using RoomRentalManagerServer.Domain.ModelEntities.Invoices;
using RoomRentalManagerServer.Infrastructure.Data;

namespace RoomRentalManagerServer.Infrastructure.Repositories.InvoiceRepositories
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly RoomRentalManagerServerDbContext _context;
        private readonly ILogger<InvoiceRepository> _logger;

        public InvoiceRepository(RoomRentalManagerServerDbContext context, ILogger<InvoiceRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IQueryable<Invoice> Query() => _context.Invoices.AsQueryable();

        public async Task<Invoice?> GetByIdAsync(long id, bool asNoTracking = true)
        {
            try
            {
                var query = _context.Invoices.Where(x => x.Id == id);
                if (asNoTracking) query = query.AsNoTracking();
                return await query.FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get invoice by id {Id}", id);
                throw;
            }
        }

        public async Task AddAsync(Invoice invoice)
        {
            try
            {
                await _context.Invoices.AddAsync(invoice);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add invoice");
                throw;
            }
        }

        public async Task UpdateAsync(Invoice invoice)
        {
            try
            {
                _context.Invoices.Update(invoice);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update invoice {Id}", invoice.Id);
                throw;
            }
        }
    }
}

