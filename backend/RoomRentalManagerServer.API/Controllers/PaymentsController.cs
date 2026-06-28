using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomRentalManagerServer.Application.Interfaces;
using RoomRentalManagerServer.Application.Model.PaymentsModel.Dto;

namespace RoomRentalManagerServer.API.Controllers
{
    [ApiController]
    [Route("api/payments")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentAppService _paymentAppService;

        public PaymentsController(IPaymentAppService paymentAppService)
        {
            _paymentAppService = paymentAppService;
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPending()
        {
            var result = await _paymentAppService.GetPendingPaymentsAsync();
            return Ok(result);
        }

        [HttpPost("{id:long}/approve")]
        public async Task<IActionResult> Approve(long id)
        {
            var dto = await _paymentAppService.ApprovePaymentAsync(id);
            return Ok(dto);
        }

        [HttpPost("{id:long}/reject")]
        public async Task<IActionResult> Reject(long id, [FromBody] RejectPaymentDto input)
        {
            var dto = await _paymentAppService.RejectPaymentAsync(id, input);
            return Ok(dto);
        }
    }
}
