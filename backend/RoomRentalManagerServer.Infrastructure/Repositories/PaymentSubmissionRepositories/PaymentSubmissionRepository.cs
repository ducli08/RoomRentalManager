using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoomRentalManagerServer.Domain.Interfaces.PaymentSubmissionInterfaces;
using RoomRentalManagerServer.Domain.ModelEntities.PaymentSubmissions;
using RoomRentalManagerServer.Infrastructure.Data;

namespace RoomRentalManagerServer.Infrastructure.Repositories.PaymentSubmissionRepositories
{
    public class PaymentSubmissionRepository : IPaymentSubmissionRepository
    {
        private readonly RoomRentalManagerServerDbContext _context;
        private readonly ILogger<PaymentSubmissionRepository> _logger;

        public PaymentSubmissionRepository(RoomRentalManagerServerDbContext context, ILogger<PaymentSubmissionRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IQueryable<PaymentSubmission> Query() => _context.PaymentSubmissions.AsQueryable();

        public async Task<PaymentSubmission?> GetByIdAsync(long id, bool asNoTracking = true)
        {
            try
            {
                var query = _context.PaymentSubmissions.Where(x => x.Id == id);
                if (asNoTracking) query = query.AsNoTracking();
                return await query.FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get payment submission by id {Id}", id);
                throw;
            }
        }

        public async Task AddAsync(PaymentSubmission submission)
        {
            try
            {
                await _context.PaymentSubmissions.AddAsync(submission);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add payment submission");
                throw;
            }
        }

        public async Task UpdateAsync(PaymentSubmission submission)
        {
            try
            {
                _context.PaymentSubmissions.Update(submission);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update payment submission {Id}", submission.Id);
                throw;
            }
        }
    }
}

