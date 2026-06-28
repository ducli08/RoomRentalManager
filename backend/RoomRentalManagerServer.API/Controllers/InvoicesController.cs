using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomRentalManagerServer.Application.Common.CommonDto;
using RoomRentalManagerServer.Application.Interfaces;
using RoomRentalManagerServer.Application.Model.InvoicesModel.Dto;
using RoomRentalManagerServer.Application.Model.PaymentsModel.Dto;

namespace RoomRentalManagerServer.API.Controllers
{
    [ApiController]
    [Route("api/invoices")]
    [Authorize]
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoiceAppService _invoiceAppService;
        private readonly IPaymentAppService _paymentAppService;

        public InvoicesController(IInvoiceAppService invoiceAppService, IPaymentAppService paymentAppService)
        {
            _invoiceAppService = invoiceAppService;
            _paymentAppService = paymentAppService;
        }

        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById(long id)
        {
            var dto = await _invoiceAppService.GetByIdAsync(id);
            if (dto == null) return NotFound(new { message = "Invoice not found" });
            return Ok(dto);
        }

        [HttpGet("{id:long}/detail")]
        [ProducesResponseType(typeof(InvoiceDetailDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDetail(long id)
        {
            var dto = await _invoiceAppService.GetDetailAsync(id, InvoiceDetailViewer.Admin);
            if (dto == null) return NotFound(new { message = "Invoice not found" });
            return Ok(dto);
        }

        [HttpPost("search")]
        [ProducesResponseType(typeof(PagedResultDto<InvoiceDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Search([FromBody] PagedRequestDto<InvoiceFilterDto> requestDto)
        {
            var result = await _invoiceAppService.GetAllAsync(requestDto);
            return Ok(result);
        }

        [HttpPost("{id:long}/cancel")]
        public async Task<IActionResult> Cancel(long id)
        {
            try
            {
                var ok = await _invoiceAppService.CancelAsync(id);
                if (!ok) return NotFound(new { message = "Invoice not found" });
                return Ok(new { message = "Cancelled" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id:long}/payments/qr")]
        public async Task<IActionResult> CreateQrPayment(long id)
        {
            var dto = await _paymentAppService.CreateQrPaymentAsync(id, isAdmin: true);
            return Ok(dto);
        }

        [HttpPost("{id:long}/payments/cash")]
        public async Task<IActionResult> CreateCashPayment(long id, [FromQuery] string? note)
        {
            var dto = await _paymentAppService.CreateCashPaymentAsync(id, note, isAdmin: true);
            return Ok(dto);
        }

        [HttpPost("{id:long}/payments/{paymentId:long}/evidence")]
        public async Task<IActionResult> UploadEvidence(long id, long paymentId, [FromForm] IFormFile evidenceFile, [FromQuery] string? note)
        {
            var dto = await _paymentAppService.UploadEvidenceAsync(id, paymentId, evidenceFile, note, isAdmin: true);
            return Ok(dto);
        }

        [HttpPost("{id:long}/payments/{paymentId:long}/cancel")]
        public async Task<IActionResult> CancelQrPayment(long id, long paymentId, [FromBody] CancelQrPaymentDto? request)
        {
            var dto = await _paymentAppService.CancelQrPaymentAsync(id, paymentId, request?.Reason, isAdmin: true);
            return Ok(dto);
        }
    }
}
