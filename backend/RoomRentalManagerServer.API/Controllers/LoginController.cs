using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomRentalManagerServer.Application.Interfaces;
using RoomRentalManagerServer.Application.Model.Login.Dto;
using RoomRentalManagerServer.Application.Model.Roles.Dto;
using RoomRentalManagerServer.Application.Model.UsersModel.Dto;
using RoomRentalManagerServer.Domain.Interfaces.RedisCache;
using System.Security;
using System.Security.Claims;

namespace RoomRentalManagerServer.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IUserAppService _userAppService;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IRoleGroupPermissionAppService _roleGroupPermissionAppService;
        private readonly IRoleAppService _roleAppService;
        private readonly ILogger<UserController> _logger;
        private readonly IJwtTokenAppService _jwtTokenAppService;
        private readonly IConfiguration _configuration;
        private readonly IGoogleTokenValidatorAppService _googleTokenValidatorAppService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public LoginController(IUserAppService userAppService, IRedisCacheService redisCacheService, ILogger<UserController> logger,
            IRoleGroupAppService roleGroupAppService, IJwtTokenAppService jwtTokenAppService, IConfiguration configuration,
            IRoleGroupPermissionAppService roleGroupPermissionAppService, IRoleAppService roleAppService,
            IGoogleTokenValidatorAppService googleTokenValidatorAppService, IWebHostEnvironment webHostEnvironment)
        {
            _userAppService = userAppService;
            _redisCacheService = redisCacheService;
            _logger = logger;
            _jwtTokenAppService = jwtTokenAppService;
            _configuration = configuration;
            _roleGroupPermissionAppService = roleGroupPermissionAppService;
            _roleAppService = roleAppService;
            _googleTokenValidatorAppService = googleTokenValidatorAppService;
            _webHostEnvironment = webHostEnvironment;
        }
        [HttpPost]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Login(string username, string password, bool rememberMe)
        {
            var expiresInMinutes = _configuration.GetSection("Jwt")["ExpiresInMinutes"];
            var expires = DateTime.UtcNow.AddMinutes(int.Parse(expiresInMinutes ?? "30"));
            var lstStringPermission = new List<string>();
            var user = await _userAppService.Authentication(username, password);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }
            if (!string.IsNullOrEmpty(user.RoleGroupId))
            {
                long.TryParse(user.RoleGroupId, out long roleGroupId);
                var lstPermissionId = await _roleGroupPermissionAppService.GetActivePermissionByRoleGroupIdAsync(roleGroupId);
                var rolePermissions = await _roleGroupPermissionAppService.GetActiveRolePermissionByPermissionId(lstPermissionId);
                var roles = await _roleAppService.GetAllRoleAsync();
                var rolesDic = roles.ToDictionary(x => x.Id);
                foreach(var item in rolePermissions)
                {
                    rolesDic.TryGetValue(item.RoleId, out var role);
                    var permissionName = role?.Permissions.FirstOrDefault(x => x.Id == item.PermissionId)?.Name;
                    var rolePermissionName = role?.Name + "." + permissionName;
                    lstStringPermission.Add(rolePermissionName);
                }
            }
            var accessToken = _jwtTokenAppService.GenerateToken(user.Id, user.Name, expires);

            string? refreshToken = null;
            DateTime? refreshExpiresAt = null;
            int? refreshExpiresIn = null;
            if (rememberMe)
            {
                refreshToken = _jwtTokenAppService.GenerateRefreshToken();
                var expiry = TimeSpan.FromDays(7);
                await _redisCacheService.SetAsync<string>($"refresh_{user.Id}", new List<string> { refreshToken }, expiry);
                refreshExpiresAt = DateTime.UtcNow.Add(expiry);
                refreshExpiresIn = (int)expiry.TotalSeconds;
            }

            // Optionally cache user info
            var userCacheExpiry = rememberMe ? TimeSpan.FromDays(7) : TimeSpan.FromHours(1);
            await _redisCache_service_SetUserAsync(user.Id.ToString(), user, userCacheExpiry);

            var loginResponseDto = new LoginResponseDto
            {
               Message = "Login successful",
               AccessToken = accessToken,
               RefreshToken = refreshToken,
               User = user,
               ExpiresAt = expires,
               ExpiresIn = (int)(expires - DateTime.UtcNow).TotalSeconds,
               RefreshExpiresAt = refreshExpiresAt,
               RefreshExpiresIn = refreshExpiresIn,
               RoleGroupPermissions = lstStringPermission
            };

            // if refresh provided, include optionally as additional headers? For now attach to response body via dynamic
            return Ok(loginResponseDto);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto request)
        {
            if (request == null || string.IsNullOrEmpty(request.RefreshToken))
                return BadRequest(new { message = "Invalid refresh request" });

            var key = $"refresh_{request.UserId}";
            var stored = await _redisCacheService.GetAsync<string>(key);
            if (stored == null || stored.Count == 0)
                return Unauthorized(new { message = "Refresh token not found" });

            if (!stored.Contains(request.RefreshToken))
                return Unauthorized(new { message = "Invalid refresh token" });

            // generate new access token (short lived)
            var newAccessExpiry = DateTime.UtcNow.AddHours(1);
            // get username from cached user if available
            var cachedUserList = await _redisCacheService.GetAsync<UserDto>(request.UserId.ToString());
            var cachedUser = cachedUserList?.FirstOrDefault();
            var userName = cachedUser?.Name ?? string.Empty;
            var newAccessToken = _jwtTokenAppService.GenerateToken(request.UserId, userName, newAccessExpiry);

            string? newRefreshToken = null;
            DateTime? newRefreshExpiresAt = null;
            int? newRefreshExpiresIn = null;
            if (request.RememberMe)
            {
                // rotate refresh token
                newRefreshToken = _jwtTokenAppService.GenerateRefreshToken();
                var expiry = TimeSpan.FromDays(7);
                await _redisCacheService.SetAsync<string>(key, new List<string> { newRefreshToken }, expiry);
                newRefreshExpiresAt = DateTime.UtcNow.Add(expiry);
                newRefreshExpiresIn = (int)expiry.TotalSeconds;
            }

            var response = new RefreshResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
            return Ok(response);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutDto request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(!long.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { message = "Invalid user" });
            var refreshKey = $"refresh_{userId}";
            if(request != null && !string.IsNullOrEmpty(request.RefreshToken))
            {
                var tokens = await _redisCacheService.GetAsync<string>(refreshKey);
                if(tokens != null && tokens.Count > 0)
                {
                    tokens.RemoveAll(t => t == request.RefreshToken);
                    if(tokens.Count > 0)
                    {
                        await _redisCacheService.SetAsync(refreshKey, tokens, TimeSpan.FromDays(7));
                    }
                    else
                    {
                        await _redisCacheService.RemoveAsync(refreshKey);
                    }    
                }
            }
            else
            {
                await _redisCacheService.RemoveAsync(refreshKey);
            }    
            await _redisCacheService.RemoveAsync(userId.ToString());
            return Ok(new { message = "Logout successful" });
        }

        // wrapper helper to set cached user as list<UserDto> to match existing Redis API
        private async Task _redisCache_service_SetUserAsync(string key, UserDto user, TimeSpan? expiry)
        {
            await _redisCacheService.SetAsync(key, new List<UserDto> { user }, expiry);
        }

        [HttpPost("google")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> LoginWithGoogle([FromBody] GoogleLoginRequestDto request)
        {
            if (request == null || string.IsNullOrEmpty(request.IdToken))
            {
                return BadRequest(new { message = "Invalid request. IdToken is required." });
            }

            try
            {
                // Validate Google ID token
                var googlePayload = await _googleTokenValidatorAppService.ValidateIdTokenAsync(request.IdToken);
                if (googlePayload == null)
                {
                    return Unauthorized(new { message = "Invalid Google token" });
                }

                // Check if email is verified
                if (!googlePayload.EmailVerified)
                {
                    return BadRequest(new { message = "Email not verified by Google" });
                }

                // Find or create user
                var user = await _userAppService.FindOrCreateGoogleUserAsync(googlePayload, _webHostEnvironment.WebRootPath);
                if (user == null)
                {
                    return Unauthorized(new { message = "Failed to authenticate user" });
                }

                // Generate JWT tokens (same flow as regular login)
                var expiresInMinutes = _configuration.GetSection("Jwt")["ExpiresInMinutes"];
                var expires = DateTime.UtcNow.AddMinutes(int.Parse(expiresInMinutes ?? "30"));
                var lstStringPermission = new List<string>();

                // Get permissions if user has role group
                if (!string.IsNullOrEmpty(user.RoleGroupId))
                {
                    long.TryParse(user.RoleGroupId, out long roleGroupId);
                    var lstPermissionId = await _roleGroupPermissionAppService.GetActivePermissionByRoleGroupIdAsync(roleGroupId);
                    var rolePermissions = await _roleGroupPermissionAppService.GetActiveRolePermissionByPermissionId(lstPermissionId);
                    var roles = await _roleAppService.GetAllRoleAsync();
                    var rolesDic = roles.ToDictionary(x => x.Id);
                    foreach (var item in rolePermissions)
                    {
                        rolesDic.TryGetValue(item.RoleId, out var role);
                        var permissionName = role?.Permissions.FirstOrDefault(x => x.Id == item.PermissionId)?.Name;
                        var rolePermissionName = role?.Name + "." + permissionName;
                        lstStringPermission.Add(rolePermissionName);
                    }
                }

                var accessToken = _jwtTokenAppService.GenerateToken(user.Id, user.Name, expires);

                string? refreshToken = null;
                DateTime? refreshExpiresAt = null;
                int? refreshExpiresIn = null;
                if (request.RememberMe)
                {
                    refreshToken = _jwtTokenAppService.GenerateRefreshToken();
                    var expiry = TimeSpan.FromDays(7);
                    await _redisCacheService.SetAsync<string>($"refresh_{user.Id}", new List<string> { refreshToken }, expiry);
                    refreshExpiresAt = DateTime.UtcNow.Add(expiry);
                    refreshExpiresIn = (int)expiry.TotalSeconds;
                }

                // Cache user info
                var userCacheExpiry = request.RememberMe ? TimeSpan.FromDays(7) : TimeSpan.FromHours(1);
                await _redisCache_service_SetUserAsync(user.Id.ToString(), user, userCacheExpiry);

                var loginResponseDto = new LoginResponseDto
                {
                    Message = "Google login successful",
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    User = user,
                    ExpiresAt = expires,
                    ExpiresIn = (int)(expires - DateTime.UtcNow).TotalSeconds,
                    RefreshExpiresAt = refreshExpiresAt,
                    RefreshExpiresIn = refreshExpiresIn,
                    RoleGroupPermissions = lstStringPermission
                };

                return Ok(loginResponseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Google login error: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred during Google login" });
            }
        }
    }
}
