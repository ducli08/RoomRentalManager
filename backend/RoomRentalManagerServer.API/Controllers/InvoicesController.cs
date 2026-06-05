using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomRentalManagerServer.Application.Common.CommonDto;
using RoomRentalManagerServer.Application.Interfaces;
using RoomRentalManagerServer.Application.Model.InvoicesModel.Dto;

namespace RoomRentalManagerServer.API.Controllers
{
    [ApiController]
    [Route("api/invoices")]
    [Authorize]
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoiceAppService _invoiceAppService;
        private readonly IPaymentFlowAppService _paymentFlowAppService;

        public InvoicesController(IInvoiceAppService invoiceAppService, IPaymentFlowAppService paymentFlowAppService)
        {
            _invoiceAppService = invoiceAppService;
            _paymentFlowAppService = paymentFlowAppService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrEditInvoiceDto input)
        {
            var ok = await _invoiceAppService.CreateOrEditAsync(input);
            if (!ok) return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to create invoice" });
            return StatusCode(StatusCodes.Status201Created, new { message = "Invoice created" });
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(long id, [FromBody] CreateOrEditInvoiceDto input)
        {
            input.Id = id;
            var ok = await _invoiceAppService.CreateOrEditAsync(input);
            if (!ok) return NotFound(new { message = "Invoice not found" });
            return Ok(new { message = "Invoice updated" });
        }

        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById(long id)
        {
            var dto = await _invoiceAppService.GetByIdAsync(id);
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

        [HttpPost("{id:long}/issue")]
        public async Task<IActionResult> Issue(long id)
        {
            var ok = await _invoiceAppService.IssueAsync(id);
            if (!ok) return NotFound(new { message = "Invoice not found" });
            return Ok(new { message = "Issued" });
        }

        [HttpPost("{id:long}/cancel")]
        public async Task<IActionResult> Cancel(long id)
        {
            var ok = await _invoiceAppService.CancelAsync(id);
            if (!ok) return NotFound(new { message = "Invoice not found" });
            return Ok(new { message = "Cancelled" });
        }

        [HttpPost("{id:long}/payments/cash")]
        public async Task<IActionResult> MarkCashPaid(long id, [FromQuery] string? note)
        {
            var dto = await _paymentFlowAppService.MarkCashPaidAsync(id, note);
            return Ok(dto);
        }
    }
}

