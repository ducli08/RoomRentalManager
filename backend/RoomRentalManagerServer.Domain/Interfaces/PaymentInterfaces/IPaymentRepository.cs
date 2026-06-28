using RoomRentalManagerServer.Domain.ModelEntities.PaymentAmount;

namespace RoomRentalManagerServer.Domain.Interfaces.PaymentInterfaces
{
    public interface IPaymentRepository
    {
        IQueryable<Payment> Query();
        Task<Payment?> GetByIdAsync(long id, bool asNoTracking = true);
        Task AddAsync(Payment payment);
        Task UpdateAsync(Payment payment);
    }
}
