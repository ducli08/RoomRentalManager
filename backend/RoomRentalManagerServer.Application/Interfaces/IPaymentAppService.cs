using Microsoft.AspNetCore.Http;
using RoomRentalManagerServer.Application.Model.InvoicesModel.Dto;
using RoomRentalManagerServer.Application.Model.PaymentsModel.Dto;

namespace RoomRentalManagerServer.Application.Interfaces
{
    public interface IPaymentAppService
    {
        Task<CreateQrPaymentResultDto> CreateQrPaymentAsync(long invoiceId, bool isAdmin);
        Task<PaymentDto> CreateCashPaymentAsync(long invoiceId, string? note, bool isAdmin);
        Task<PaymentDto> UploadEvidenceAsync(long invoiceId, long paymentId, IFormFile evidenceFile, string? note, bool isAdmin);
        Task<PaymentDto> CancelQrPaymentAsync(long invoiceId, long paymentId, string? reason, bool isAdmin);
        Task<PaymentDto> ApprovePaymentAsync(long paymentId);
        Task<PaymentDto> RejectPaymentAsync(long paymentId, RejectPaymentDto input);
        Task<List<PaymentDto>> GetPendingPaymentsAsync();
    }
}
