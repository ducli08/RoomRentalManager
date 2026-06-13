using Microsoft.AspNetCore.Mvc;
using RoomRentalManagerServer.Application.Common.CommonDto;
using RoomRentalManagerServer.Application.Interfaces;
using RoomRentalManagerServer.Application.Model.ContractsModel.Dto;

namespace RoomRentalManagerServer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContractController : ControllerBase
    {
        private readonly IContractAppService _contractAppService;

        public ContractController(IContractAppService contractAppService)
        {
            _contractAppService = contractAppService;
        }

        [HttpPost("getAllContractAsync")]
        [ProducesResponseType(typeof(PagedResultDto<ContractDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllContractAsync([FromBody] PagedRequestDto<ContractFilterDto> requestDto)
        {
            if (requestDto == null)
            {
                return BadRequest(new { message = "Request body is required." });
            }

            var result = await _contractAppService.GetAllContractAsync(requestDto);
            return Ok(result);
        }

        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(ContractDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(long id)
        {
            var dto = await _contractAppService.GetContractByIdAsync(id);
            if (dto == null)
            {
                return NotFound(new { message = "Contract not found." });
            }

            return Ok(dto);
        }

        [HttpPost("createOrEdit")]
        public async Task<IActionResult> CreateOrEditContract([FromBody] CreateOrEditContractDto input)
        {
            if (input == null)
            {
                return BadRequest(new { message = "Request body is required." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var succeeded = await _contractAppService.CreateOrEditContractAsync(input);

                if (!succeeded)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while saving the contract." });
                }

                if (input.Id == null || input.Id == 0)
                {
                    return StatusCode(StatusCodes.Status201Created, new { message = "Contract created successfully." });
                }

                return Ok(new { message = "Contract updated successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> DeleteContract(long id)
        {
            var existing = await _contractAppService.GetContractByIdAsync(id);
            if (existing == null)
            {
                return NotFound(new { message = "Contract not found." });
            }

            try
            {
                await _contractAppService.DeleteContractAsync(id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
