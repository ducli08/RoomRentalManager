using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RoomRentalManagerServer.Application.Common.CommonDto;
using RoomRentalManagerServer.Application.Interfaces;
using RoomRentalManagerServer.Application.Model.UsersModel.Dto;
using RoomRentalManagerServer.Domain.Interfaces.RedisCache;
using System.Diagnostics;

namespace RoomRentalManagerServer.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserAppService _userAppService;
        private readonly IProvinceAppService _provinceAppService;
        private readonly IDistrictAppService _districtAppService;
        private readonly IWardAppService _wardAppService;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IRoleGroupAppService _roleGroupAppService;
        private readonly ILogger<UserController> _logger;
        private readonly IWebHostEnvironment _env;
        public UserController(IUserAppService userAppService, IProvinceAppService provinceAppService, IDistrictAppService districtAppService,
            IWardAppService wardAppService, IRedisCacheService redisCacheService, ILogger<UserController> logger, IRoleGroupAppService roleGroupAppService,
            IWebHostEnvironment env)
        {
            _userAppService = userAppService;
            _provinceAppService = provinceAppService;
            _districtAppService = districtAppService;
            _wardAppService = wardAppService;
            _redisCacheService = redisCacheService;
            _roleGroupAppService = roleGroupAppService;
            _logger = logger;
            _env = env;
        }
        [HttpPost("createOrEditUser")]
        public async Task<ActionResult> CreateOrEditUser(CreateOrEditUserDto input)
        {
            var resCreateOrUpdate = await _userAppService.CreateOrEditUserAsync(input);
            var action = input.Id != null ? "edit" : "create";
            var res = new
            {
                code = resCreateOrUpdate ? 200 : 500,
                message = resCreateOrUpdate ? $"User {action} successfully" : $"Failed to {action} user"
            };
            return Ok(res);
        }
        [HttpPost("getAllUserAsync")]
        [ProducesResponseType(typeof(PagedResultDto<UserDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllUserAsync([FromBody] PagedRequestDto<UserFilterDto> requestDto)
        {
            var result = await _userAppService.GetAllUsersAsync(requestDto);
            return Ok(result);
        }

        [HttpPost("getAllProvince")]
        public async Task<List<SelectListItem>> GetAllProvinceAsync()
        {
            Stopwatch st = new Stopwatch();
            st.Start();
            var provinceQuery = await _provinceAppService.GetAllProvincesAsync();
            var selectList = provinceQuery.Select(p => new SelectListItem
            {
                Value = p.Code,
                Text = p.Name
            }).ToList();
            st.Stop();
            _logger.LogInformation($"Total time to get data Provinces: {st.ElapsedMilliseconds} miliseconds");
            return selectList;
        }

        [HttpPost("getAllDistrict")]
        public async Task<List<SelectListItem>> GetAllDistrictAsync(string? provinceCode)
        {
            Stopwatch st = new Stopwatch();
            st.Start();
            var districtQuery = await _districtAppService.GetAllDistrictsAsync(provinceCode);
            var selectList = districtQuery.Select(d => new SelectListItem
            {
                Value = d.Code,
                Text = d.Name
            }).ToList();
            st.Stop();
            _logger.LogInformation($"Total time to get data Districts: {st.ElapsedMilliseconds} miliseconds");
            return selectList;
        }

        [HttpPost("getAllWard")]
        public async Task<List<SelectListItem>> GetAllWardAsync(string? districtCode)
        {
            Stopwatch st = new Stopwatch();
            st.Start();
            var wardQuery = await _wardAppService.GetAllWardsAsync(districtCode);
            var selectList = wardQuery.Select(w => new SelectListItem
            {
                Value = w.Code,
                Text = w.Name
            }).ToList();
            st.Stop();
            _logger.LogInformation($"Total time to get data Wards: {st.ElapsedMilliseconds} miliseconds");
            return selectList;
        }

        [HttpDelete("deleteUser")]
        public async Task DeleteUser(long id)
        {
            await _userAppService.DeleteUserAsync(id);
        }

        [HttpPost("uploadAvatar")]
        public async Task<IActionResult> UploadAvatar([FromForm] List<IFormFile> avatar)
        {
            var boolean = HttpContext.User?.Identity?.IsAuthenticated;
            if (avatar == null)
                return BadRequest(new { message = "No files received." });

            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var (paths, publicIds, errors) = await _userAppService.UploadAvatarAsync(avatar, webRoot);

            if (errors != null && errors.Any())
            {
                if (errors.Contains("User is not authenticated."))
                    return Unauthorized(new { errors });

                return BadRequest(new { paths, publicIds, errors });
            }

            return Ok(new { paths, publicIds });
        }
    }
}
