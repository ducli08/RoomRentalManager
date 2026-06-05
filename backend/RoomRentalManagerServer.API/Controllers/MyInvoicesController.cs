using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomRentalManagerServer.Application.Common.CommonDto;
using RoomRentalManagerServer.Application.Interfaces;
using RoomRentalManagerServer.Application.Model.InvoicesModel.Dto;
using RoomRentalManagerServer.Application.Model.PaymentsModel.Dto;
using RoomRentalManagerServer.Application.Model.PaymentSubmissionsModel.Dto;

namespace RoomRentalManagerServer.API.Controllers
{
    [ApiController]
    [Route("api/my/invoices")]
    [Authorize]
    public class MyInvoicesController : ControllerBase
    {
        private readonly IInvoiceAppService _invoiceAppService;
        private readonly IPaymentFlowAppService _paymentFlowAppService;

        public MyInvoicesController(IInvoiceAppService invoiceAppService, IPaymentFlowAppService paymentFlowAppService)
        {
            _invoiceAppService = invoiceAppService;
            _paymentFlowAppService = paymentFlowAppService;
        }

        [HttpPost("search")]
        [ProducesResponseType(typeof(PagedResultDto<InvoiceDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Search([FromBody] PagedRequestDto<InvoiceFilterDto> requestDto)
        {
            var result = await _invoiceAppService.GetMyInvoicesAsync(requestDto);
            return Ok(result);
        }

        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById(long id)
        {
            var dto = await _invoiceAppService.GetMyInvoiceByIdAsync(id);
            if (dto == null) return NotFound(new { message = "Invoice not found" });
            return Ok(dto);
        }

        [HttpPost("{id:long}/payment-intents")]
        [ProducesResponseType(typeof(PaymentIntentDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreatePaymentIntent(long id)
        {
            var dto = await _paymentFlowAppService.CreatePaymentIntentAsync(id);
            return Ok(dto);
        }

        [HttpPost("{id:long}/payment-submissions")]
        [ProducesResponseType(typeof(PaymentSubmissionDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> SubmitEvidence(long id, [FromForm] IFormFile evidenceFile)
        {
            var dto = await _paymentFlowAppService.SubmitTransferEvidenceAsync(id, evidenceFile);
            return Ok(dto);
        }
    }
}

