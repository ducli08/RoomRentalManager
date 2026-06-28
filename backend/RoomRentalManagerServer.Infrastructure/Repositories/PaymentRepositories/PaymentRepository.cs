using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoomRentalManagerServer.Domain.Interfaces.PaymentInterfaces;
using RoomRentalManagerServer.Domain.ModelEntities.PaymentAmount;
using RoomRentalManagerServer.Infrastructure.Data;

namespace RoomRentalManagerServer.Infrastructure.Repositories.PaymentRepositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly RoomRentalManagerServerDbContext _context;
        private readonly ILogger<PaymentRepository> _logger;

        public PaymentRepository(RoomRentalManagerServerDbContext context, ILogger<PaymentRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IQueryable<Payment> Query() => _context.Payments.AsQueryable();

        public async Task<Payment?> GetByIdAsync(long id, bool asNoTracking = true)
        {
            try
            {
                var query = _context.Payments.Where(x => x.Id == id);
                if (asNoTracking) query = query.AsNoTracking();
                return await query.FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get payment by id {Id}", id);
                throw;
            }
        }

        public async Task AddAsync(Payment payment)
        {
            try
            {
                await _context.Payments.AddAsync(payment);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add payment");
                throw;
            }
        }

        public async Task UpdateAsync(Payment payment)
        {
            try
            {
                _context.Payments.Update(payment);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update payment {Id}", payment.Id);
                throw;
            }
        }
    }
}
