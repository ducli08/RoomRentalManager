using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomRentalManagerServer.Application.Interfaces;
using RoomRentalManagerServer.Application.Model.PaymentSubmissionsModel.Dto;

namespace RoomRentalManagerServer.API.Controllers
{
    [ApiController]
    [Route("api/payment-submissions")]
    [Authorize]
    public class PaymentSubmissionsController : ControllerBase
    {
        private readonly IPaymentFlowAppService _paymentFlowAppService;

        public PaymentSubmissionsController(IPaymentFlowAppService paymentFlowAppService)
        {
            _paymentFlowAppService = paymentFlowAppService;
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPending()
        {
            var result = await _paymentFlowAppService.GetPendingSubmissionsAsync();
            return Ok(result);
        }

        [HttpPost("{id:long}/approve")]
        public async Task<IActionResult> Approve(long id)
        {
            var dto = await _paymentFlowAppService.ApproveSubmissionAsync(id);
            return Ok(dto);
        }

        [HttpPost("{id:long}/reject")]
        public async Task<IActionResult> Reject(long id, [FromBody] RejectPaymentSubmissionDto input)
        {
            var dto = await _paymentFlowAppService.RejectSubmissionAsync(id, input);
            return Ok(dto);
        }
    }
}

