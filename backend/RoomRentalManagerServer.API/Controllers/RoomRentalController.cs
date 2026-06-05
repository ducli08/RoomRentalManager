using Microsoft.AspNetCore.Mvc;
using RoomRentalManagerServer.Application.Common.CommonDto;
using RoomRentalManagerServer.Application.Interfaces;
using RoomRentalManagerServer.Application.Model.RoomRentalsModel.Dto;

namespace RoomRentalManagerServer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomRentalController : ControllerBase
    {
        private readonly IRoomRentalAppService _roomRentalAppService;
        private readonly IWebHostEnvironment _env;

        public RoomRentalController(IRoomRentalAppService roomRentalAppService, IWebHostEnvironment env)
        {
            _roomRentalAppService = roomRentalAppService;
            _env = env;
        }

        [HttpPost("getAllRoomRentalAsync")]
        [ProducesResponseType(typeof(PagedResultDto<RoomRentalDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllRoomRentalAsync([FromBody] PagedRequestDto<RoomRentalFilterDto> requestDto)
        {
            if (requestDto == null)
                return BadRequest(new { message = "Request body is required." });

            var roomRentals = await _roomRentalAppService.GetAllRoomRentalAsync(requestDto);
            return Ok(roomRentals);
        }

        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(RoomRentalDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(long id)
        {
            var dto = await _roomRentalAppService.GetRoomRentalByIdAsync(id);
            if (dto == null)
                return NotFound(new { message = "Room rental not found." });
            return Ok(dto);
        }

        [HttpPost("createOrEdit")]
        public async Task<IActionResult> CreateOrEditRoomRental([FromBody] CreateOrEditRoomRentalDto input)
        {
            if (input == null)
                return BadRequest(new { message = "Request body is required." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var succeeded = await _roomRentalAppService.CreateOrEditRoomRentalAsync(input);

            if (!succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while saving the room rental." });
            }

            // We cannot return the created resource location because service returns only bool.
            if (input.Id == null)
                return StatusCode(StatusCodes.Status201Created, new { message = "Room rental created successfully." });

            return Ok(new { message = "Room rental updated successfully." });
        }

        [HttpPost("uploadImageDescription")]
        public async Task<IActionResult> UploadImageDescription([FromForm] List<IFormFile> uploadImages)
        {
            if (uploadImages == null || !uploadImages.Any())
                return BadRequest(new { message = "No files received." });

            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var (paths, errors) = await _roomRentalAppService.UploadImageDescriptionAsync(uploadImages, webRoot);

            if (errors != null && errors.Any())
            {
                if (errors.Contains("User is not authenticated."))
                    return Unauthorized(new { errors });

                return BadRequest(new { paths, errors });
            }

            return Ok(new { paths });
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> DeleteRoomRental(long id)
        {
            var existing = await _roomRentalAppService.GetRoomRentalByIdAsync(id);
            if (existing == null)
                return NotFound(new { message = "Room rental not found." });
            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            await _roomRentalAppService.DeleteRoomRentalAsync(id, webRoot);
            return NoContent();
        }
    }
}
