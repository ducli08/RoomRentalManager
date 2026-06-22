using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RoomRentalManagerServer.Application.Common.CommonAppService;
using RoomRentalManagerServer.Application.Common.CommonDto;
using RoomRentalManagerServer.Application.Interfaces;
using RoomRentalManagerServer.Application.Model.RoomRentalsModel.Dto;

namespace RoomRentalManagerServer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommonController : Controller
    {
        public readonly ICommonAppService _commonAppService;
        public CommonController(ICommonAppService commonAppService)
        {
            _commonAppService = commonAppService;
        }
        [HttpPost("getSelectListItem")]
        public async Task<ActionResult<List<SelectListItemDto>>> GetSelectListItem(string type, string? cascadeValue)
        {
            try
            {
                var selectListItemDtos = await _commonAppService.GetCustomSelectListItem(type, cascadeValue ?? string.Empty);
                return Ok(selectListItemDtos);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("getEnumSelectListItem")]
        public Task<ActionResult<List<SelectListItemDto>>> GetEnumSelectListItem([FromQuery]string? enumType)
        {
            if (string.IsNullOrEmpty(enumType))
            {
                return Task.FromResult<ActionResult<List<SelectListItemDto>>>(BadRequest("Enum type is null!"));
            }
            var selectListEnum = _commonAppService.GetEnumSelectListItem(enumType);
            return Task.FromResult<ActionResult<List<SelectListItemDto>>>(Ok(selectListEnum));
        }
    }
}
