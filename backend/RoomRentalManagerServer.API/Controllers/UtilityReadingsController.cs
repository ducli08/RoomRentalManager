using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomRentalManagerServer.API.Authorization;
using RoomRentalManagerServer.Application.Common.CommonDto;
using RoomRentalManagerServer.Application.Interfaces;
using RoomRentalManagerServer.Application.Model.UtilityReadingsModel.Dto;

namespace RoomRentalManagerServer.API.Controllers
{
    [ApiController]
    [Route("api/utility-readings")]
    [Authorize]
    public class UtilityReadingsController : ControllerBase
    {
        private readonly IUtilityReadingAppService _utilityReadingAppService;

        public UtilityReadingsController(IUtilityReadingAppService utilityReadingAppService)
        {
            _utilityReadingAppService = utilityReadingAppService;
        }

        [HttpGet("{id:long}")]
        [RequirePermission(AnyOf = new[] { "UtilityReading.Thêm", "UtilityReading.Sửa", "UtilityReading.Xóa" })]
        [ProducesResponseType(typeof(UtilityReadingDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById(long id)
        {
            var dto = await _utilityReadingAppService.GetByIdAsync(id);
            if (dto == null) return NotFound(new { message = "Utility reading not found." });
            return Ok(dto);
        }

        [HttpPost]
        [RequirePermission(Permission = "UtilityReading.Thêm")]
        public async Task<IActionResult> Create([FromBody] CreateOrEditUtilityReadingDto input)
        {
            if (input == null) return BadRequest(new { message = "Request body is required." });

            try
            {
                var result = await _utilityReadingAppService.CreateOrEditAsync(input);
                return StatusCode(StatusCodes.Status201Created, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:long}")]
        [RequirePermission(Permission = "UtilityReading.Sửa")]
        public async Task<IActionResult> Update(long id, [FromBody] CreateOrEditUtilityReadingDto input)
        {
            if (input == null) return BadRequest(new { message = "Request body is required." });

            input.Id = id;
            try
            {
                var result = await _utilityReadingAppService.CreateOrEditAsync(input);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Utility reading not found." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("search")]
        [RequirePermission(AnyOf = new[] { "UtilityReading.Thêm", "UtilityReading.Sửa", "UtilityReading.Xóa" })]
        [ProducesResponseType(typeof(PagedResultDto<UtilityReadingDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Search([FromBody] PagedRequestDto<UtilityReadingFilterDto> requestDto)
        {
            if (requestDto == null) return BadRequest(new { message = "Request body is required." });
            var result = await _utilityReadingAppService.GetAllAsync(requestDto);
            return Ok(result);
        }

        [HttpGet("prepare")]
        [RequirePermission(AnyOf = new[] { "UtilityReading.Thêm", "UtilityReading.Sửa", "UtilityReading.Xóa" })]
        [ProducesResponseType(typeof(UtilityReadingPrepareDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Prepare([FromQuery] long contractId, [FromQuery] int month, [FromQuery] int year, [FromQuery] long? utilityReadingId)
        {
            var result = await _utilityReadingAppService.GetPrepareAsync(contractId, month, year, utilityReadingId);
            return Ok(result);
        }

        [HttpPost("export")]
        [RequirePermission(AnyOf = new[] { "UtilityReading.Thêm", "UtilityReading.Sửa", "UtilityReading.Xóa" })]
        public async Task<IActionResult> Export([FromBody] UtilityReadingFilterDto filter)
        {
            var bytes = await _utilityReadingAppService.ExportExcelAsync(filter ?? new UtilityReadingFilterDto());
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"utility-readings-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx");
        }
    }
}
