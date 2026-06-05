using RoomRentalManagerServer.Domain.ModelEntities.PaymentAmount;

namespace RoomRentalManagerServer.Domain.Interfaces.PaymentInterfaces
{
    public interface IPaymentRepository
    {
        IQueryable<Payment> Query();
        Task AddAsync(Payment payment);
    }
}

