using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoomRentalManagerServer.Domain.Interfaces.InvoiceInterfaces;
using RoomRentalManagerServer.Domain.ModelEntities.Invoices;
using RoomRentalManagerServer.Infrastructure.Data;

namespace RoomRentalManagerServer.Infrastructure.Repositories.InvoiceRepositories
{
    public class InvoiceItemRepository : IInvoiceItemRepository
    {
        private readonly RoomRentalManagerServerDbContext _context;
        private readonly ILogger<InvoiceItemRepository> _logger;

        public InvoiceItemRepository(RoomRentalManagerServerDbContext context, ILogger<InvoiceItemRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task AddRangeAsync(IEnumerable<InvoiceItem> items)
        {
            try
            {
                await _context.InvoiceItems.AddRangeAsync(items);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add invoice items");
                throw;
            }
        }

        public async Task<List<InvoiceItem>> GetByInvoiceIdAsync(long invoiceId)
        {
            return await _context.InvoiceItems
                .AsNoTracking()
                .Where(x => x.InvoiceId == invoiceId)
                .OrderBy(x => x.SortOrder)
                .ToListAsync();
        }
    }
}
