using Microsoft.AspNetCore.Mvc;
using RoomRentalManagerServer.Application.Common.CommonDto;
using RoomRentalManagerServer.Application.Interfaces;
using RoomRentalManagerServer.Application.Model.RoleGroupsModel.Dto;
using RoomRentalManagerServer.Application.Model.Roles.Dto;

namespace RoomRentalManagerServer.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleGroupController : ControllerBase
    {
        private readonly IRoleGroupAppService _roleGroupAppService;
        private readonly ILogger<RoleGroupController> _logger;
        private readonly IRoleAppService _roleAppService;
        private readonly IRoleGroupPermissionAppService _roleGroupPermissionAppService;

        public RoleGroupController(IRoleGroupAppService roleGroupAppService, ILogger<RoleGroupController> logger, IRoleAppService roleAppService,
            IRoleGroupPermissionAppService roleGroupPermissionAppService)
        {
            _roleGroupAppService = roleGroupAppService;
            _logger = logger;
            _roleAppService = roleAppService;
            _roleGroupPermissionAppService = roleGroupPermissionAppService;
        }

        [HttpPost("getAllRoleGroupsAsync")]
        [ProducesResponseType(typeof(PagedResultDto<RoleGroupDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllRoleGroupsAsync([FromBody] PagedRequestDto<RoleGroupFilterDto> requestDto)
        {
            if (requestDto == null)
                return BadRequest(new { message = "Request body is required." });

            var roleGroups = await _roleGroupAppService.GetAllRoleGroupsAsync(requestDto);
            return Ok(roleGroups);
        }

        [HttpPost("createOrEditRoleGroup")]
        public async Task<IActionResult> CreateOrEditRoleGroup([FromBody] CreateOrEditRoleGroupDto input)
        {
            if (input == null)
                return BadRequest(new { message = "Request body is required." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var succeeded = await _roleGroupAppService.CreateOrEditRoleGroupAsync(input);

            if (!succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while saving the room rental." });
            }

            // We cannot return the created resource location because service returns only bool.
            if (input.Id == null)
                return StatusCode(StatusCodes.Status201Created, new { message = "Room rental created successfully." });

            return Ok(new { message = "Room rental updated successfully." });
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> DeleteRoleGroup(long id)
        {
            var existing = await _roleGroupAppService.GetRoleGroupByIdAsync(id);
            if (existing == null)
                return NotFound(new { message = "Room rental not found." });
            await _roleGroupAppService.DeleteRoleGroupAsync(id);
            await _roleGroupPermissionAppService.DeleteActivePermissionByRoleGroupIdAsync(id);
            return NoContent();
        }

        [HttpGet("getAllRole")]
        public async Task<ActionResult<List<RoleDto>>> GetAll()
        {
            try
            {
                var roles = await _roleAppService.GetAllRoleAsync();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving roles.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving roles." });
            }
        }
        [HttpGet("getActivePermission")]
        public async Task<List<long>> GetActivePermission(long roleGroupId)
        {
            try
            {
                var activePermission = await _roleGroupPermissionAppService.GetActivePermissionByRoleGroupIdAsync(roleGroupId);
                return activePermission != null ? activePermission : new List<long>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving active permissions.");
                throw;
            }
        }
        
    }
}
