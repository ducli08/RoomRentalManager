using Microsoft.AspNetCore.Http;
using RoomRentalManagerServer.Application.Model.PaymentsModel.Dto;
using RoomRentalManagerServer.Application.Model.PaymentSubmissionsModel.Dto;

namespace RoomRentalManagerServer.Application.Interfaces
{
    public interface IPaymentFlowAppService
    {
        Task<PaymentIntentDto> CreatePaymentIntentAsync(long invoiceId);
        Task<PaymentSubmissionDto> SubmitTransferEvidenceAsync(long invoiceId, IFormFile evidenceFile);
        Task<PaymentSubmissionDto> ApproveSubmissionAsync(long submissionId);
        Task<PaymentSubmissionDto> RejectSubmissionAsync(long submissionId, RejectPaymentSubmissionDto input);
        Task<PaymentSubmissionDto> MarkCashPaidAsync(long invoiceId, string? note);
        Task<List<PaymentSubmissionDto>> GetPendingSubmissionsAsync();
    }
}

