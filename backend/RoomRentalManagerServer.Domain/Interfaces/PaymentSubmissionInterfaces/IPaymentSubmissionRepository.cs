using RoomRentalManagerServer.Domain.ModelEntities.PaymentSubmissions;

namespace RoomRentalManagerServer.Domain.Interfaces.PaymentSubmissionInterfaces
{
    public interface IPaymentSubmissionRepository
    {
        IQueryable<PaymentSubmission> Query();
        Task<PaymentSubmission?> GetByIdAsync(long id, bool asNoTracking = true);
        Task AddAsync(PaymentSubmission submission);
        Task UpdateAsync(PaymentSubmission submission);
    }
}

