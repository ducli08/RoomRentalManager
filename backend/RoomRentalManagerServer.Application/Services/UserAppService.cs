using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RoomRentalManagerServer.Application.Common.CommonDto;
using RoomRentalManagerServer.Application.Interfaces;
using RoomRentalManagerServer.Application.Model.Login.Dto;
using RoomRentalManagerServer.Application.Model.UsersModel.Dto;
using RoomRentalManagerServer.Domain.Interfaces.UserInterfaces;
using RoomRentalManagerServer.Domain.ModelEntities.User;
using System.IO;
using RoomRentalManagerServer.Domain.Interfaces.RoleGroupInterfaces;

namespace RoomRentalManagerServer.Application.Services
{
    public class UserAppService : IUserAppService
    {
        private readonly ILogger<UserAppService> _logger;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUserAppService _currentUserAppService;
        private readonly ILocalFileStorageAppService _localFileStorageAppService;
        private readonly IConfiguration _configuration;
        private readonly IWebHost _webHostEnvironment;
        private readonly RoomRentalManagerServer.Domain.Interfaces.ICloudinaryService _cloudinaryService;
        private readonly IRoleGroupRepository _roleGroupRepository;
        
        public UserAppService(ILogger<UserAppService> logger, IUserRepository userRepository, IMapper mapper, ICurrentUserAppService currentUserAppService, ILocalFileStorageAppService localFileStorageAppService, IConfiguration configuration, RoomRentalManagerServer.Domain.Interfaces.ICloudinaryService cloudinaryService, IRoleGroupRepository roleGroupRepository)
        {
            _logger = logger;
            _mapper = mapper;
            _userRepository = userRepository;
            _currentUserAppService = currentUserAppService;
            _localFileStorageAppService = localFileStorageAppService;
            _configuration = configuration;
            _cloudinaryService = cloudinaryService;
            _roleGroupRepository = roleGroupRepository;
        }

        public async Task<(List<string> Paths, List<string> PublicIds, List<string> Errors)> UploadAvatarAsync(List<IFormFile> avatar, string webRoot)
        {
            if (!_currentUserAppService.IsAuthenticated)
            {
                _logger.LogWarning("Unauthenticated user attempted to upload images");
                return (new List<string>(), new List<string>(), new List<string> { "User is not authenticated." });
            }

            var paths = new List<string>();
            var publicIds = new List<string>();
            var errors = new List<string>();

            foreach (var file in avatar)
            {
                if (file.Length > 0)
                {
                    try
                    {
                        var result = await _cloudinaryService.UploadImageAsync(file, "avatars");
                        if (!string.IsNullOrEmpty(result.Url))
                        {
                            paths.Add(result.Url);
                            publicIds.Add(result.PublicId);
                        }
                        else
                        {
                            errors.Add($"Failed to upload {file.FileName} to Cloudinary.");
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Error uploading {file.FileName}: {ex.Message}");
                    }
                }
            }

            return (paths, publicIds, errors);
        }

        public async Task<bool> CreateOrEditUserAsync(CreateOrEditUserDto input)
        {
            var action = input.Id != null ? "Edit" : "Create";
            var res = true;
            try
            {
                var user = _mapper.Map<Users>(input);
                if (input.Id != null && input.Id != 0)
                {
                    await UpdateAsync(user);
                }
                else
                {
                    await AddAsync(user);
                }
                return res;
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Failed to {action}: {ex.Message}");
                throw;
            }

        }

        public async Task DeleteUserAsync(long id)
        {
            await _userRepository.DeleteAsync(id);
        }

        public async Task<PagedResultDto<UserDto>> GetAllUsersAsync(PagedRequestDto<UserFilterDto> pagedRequestDto)
        {
            try
            {
                var queryUser = await _userRepository.GetAllQueryAsync();
                if (!string.IsNullOrEmpty(pagedRequestDto.Filter.NameFilter))
                {
                    queryUser = queryUser.Where(x =>
                        EF.Functions.ILike(EF.Functions.Unaccent(x.Name), EF.Functions.Unaccent($"%{pagedRequestDto.Filter.NameFilter}%")) ||
                        EF.Functions.ILike(EF.Functions.Unaccent(x.Name), EF.Functions.Unaccent($"{pagedRequestDto.Filter.NameFilter}%")) ||
                        EF.Functions.ILike(EF.Functions.Unaccent(x.Name), EF.Functions.Unaccent($"{pagedRequestDto.Filter.NameFilter}%"))
                    );
                }
                if (!string.IsNullOrEmpty(pagedRequestDto.Filter.ProvinceCodeFilter))
                {
                    queryUser = queryUser.Where(x => x.ProvinceCode.Equals(pagedRequestDto.Filter.ProvinceCodeFilter));
                }
                if (!string.IsNullOrEmpty(pagedRequestDto.Filter.DistrictCodeFilter))
                {
                    queryUser = queryUser.Where(x => x.DistrictCode.Equals(pagedRequestDto.Filter.DistrictCodeFilter));
                }
                if (!string.IsNullOrEmpty(pagedRequestDto.Filter.WardCodeFilter))
                {
                    queryUser = queryUser.Where(x => x.WardCode.Equals(pagedRequestDto.Filter.WardCodeFilter));
                }
                if (!string.IsNullOrEmpty(pagedRequestDto.Filter.AddressFilter))
                {
                    queryUser = queryUser.Where(x =>
                        EF.Functions.ILike(EF.Functions.Unaccent(x.Address), EF.Functions.Unaccent($"%{pagedRequestDto.Filter.AddressFilter}%")) ||
                        EF.Functions.ILike(EF.Functions.Unaccent(x.Address), EF.Functions.Unaccent($"{pagedRequestDto.Filter.AddressFilter}%")) ||
                        EF.Functions.ILike(EF.Functions.Unaccent(x.Address), EF.Functions.Unaccent($"%{pagedRequestDto.Filter.AddressFilter}"))
                    );
                }
                if (!string.IsNullOrEmpty(pagedRequestDto.Filter.EmailFilter))
                {
                    queryUser = queryUser.Where(x => x.Email.Equals(pagedRequestDto.Filter.EmailFilter));
                }
                if (!string.IsNullOrEmpty(pagedRequestDto.Filter.IDCardFilter))
                {
                    queryUser = queryUser.Where(x => x.IDCard.Equals(pagedRequestDto.Filter.IDCardFilter));
                }
                if (pagedRequestDto.Filter.DateOfBirth != null)
                {
                    queryUser = queryUser.Where(x => x.DateOfBirth == pagedRequestDto.Filter.DateOfBirth);
                }
                if (!string.IsNullOrEmpty(pagedRequestDto.SortOrder) && !string.IsNullOrEmpty(pagedRequestDto.SortBy))
                {
                    queryUser = pagedRequestDto.SortOrder == "desc"
                    ? queryUser.OrderByDescending(x => EF.Property<object>(x, pagedRequestDto.SortBy))
                    : queryUser.OrderBy(x => EF.Property<object>(x, pagedRequestDto.SortBy));
                }
                var total = queryUser.Count();
                var lstUser = await queryUser.Skip((pagedRequestDto.Page - 1) * pagedRequestDto.PageSize).Take(pagedRequestDto.PageSize).ToListAsync();
                var lstUserDto = lstUser.Select(user => _mapper.Map<UserDto>(user)).ToList();
                return new PagedResultDto<UserDto>(lstUserDto, total);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get all users: {ex.Message}");
                throw;
            }
        }

        public async Task<UserDto> GetUserByIdAsync(long id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> AddAsync(Users user)
        {
            try
            {
                var hasher = new PasswordHasher<Users>();
                user.Password = hasher.HashPassword(user, user.Password); // Hash the password before saving
                user.CreatedDate = user.UpdatedDate = DateTime.UtcNow;
                user.CreatorUser = user.LastUpdateUser = _currentUserAppService.UserName;
                await _userRepository.AddAsync(user);
                return _mapper.Map<UserDto>(user);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to add user: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateAsync(Users user)
        {
            try
            {
                user.LastUpdateUser = _currentUserAppService.UserName;

                var existingUser = await _userRepository.GetByIdAsync(user.Id);
                if (existingUser != null && !string.IsNullOrEmpty(existingUser.AvatarPublicId) && existingUser.AvatarPublicId != user.AvatarPublicId)
                {
                    // If the user changed their avatar, delete the old one from Cloudinary 
                    await _cloudinaryService.DeleteImageAsync(existingUser.AvatarPublicId);
                }

                await _userRepository.UpdateAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to update user: {ex.Message}");
                throw;
            }
        }

        public async Task<UserDto> Authentication(string username, string password)
        {
            try
            {
                var user = await _userRepository.GetUserByEmail(username);
                if (user != null)
                {
                    var hasher = new PasswordHasher<Users>();
                    var result = hasher.VerifyHashedPassword(user, user.Password, password);
                    if (result == PasswordVerificationResult.Success)
                    {
                        return _mapper.Map<UserDto>(user);
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task<List<Users>> GetAllUserForSelectListItem()
        {
            try
            {
                var queryGetAllUser = await _userRepository.GetAllQueryAsync();
                return queryGetAllUser.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get all user for selectListItem: {ex.Message}");
                throw;
            }
        }

        private async Task<(string? Url, string? PublicId)> UploadGoogleAvatarToCloudinaryAsync(string? googlePictureUrl)
        {
            if (string.IsNullOrEmpty(googlePictureUrl))
                return (null, null);

            try
            {
                var result = await _cloudinaryService.UploadImageFromUrlAsync(googlePictureUrl, "avatars");
                if (!string.IsNullOrEmpty(result.Url))
                {
                    return (result.Url, result.PublicId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to upload Google avatar from {googlePictureUrl} to Cloudinary: {ex.Message}");
            }

            return (null, null);
        }

        private async Task DeleteOldAvatarIfExistsAsync(string? oldAvatarPath, string? oldAvatarPublicId, string webRoot)
        {
            if (!string.IsNullOrEmpty(oldAvatarPublicId))
            {
                // Delete from Cloudinary
                try
                {
                    await _cloudinaryService.DeleteImageAsync(oldAvatarPublicId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Failed to delete old avatar from Cloudinary: {ex.Message}");
                }
                return;
            }

            if (string.IsNullOrWhiteSpace(oldAvatarPath))
                return;

            // Only delete if it's a local path (not a URL)
            if (oldAvatarPath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
                oldAvatarPath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return; // It's a URL, not a local file
            }

            webRoot = webRoot ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            await _localFileStorageAppService.DeleteFileAsync(oldAvatarPath, webRoot);
        }

        public async Task<UserDto> FindOrCreateGoogleUserAsync(GoogleTokenPayload googlePayload, string webRoot)
        {
            try
            {
                if (googlePayload == null || string.IsNullOrEmpty(googlePayload.Sub) || string.IsNullOrEmpty(googlePayload.Email))
                {
                    _logger.LogWarning("Invalid Google token payload");
                    throw new ArgumentException("Invalid Google token payload");
                }

                // First, try to find user by provider and providerId (Google sub)
                var user = await _userRepository.GetUserByProviderAsync("Google", googlePayload.Sub);
                
                if (user != null)
                {
                    // User exists with this Google account, sync avatar if needed
                    _logger.LogInformation($"Found existing Google user: {user.Email}");
                    
                    if (!string.IsNullOrEmpty(googlePayload.Picture))
                    {
                        // Check if avatar needs to be synced
                        bool needsSync = string.IsNullOrEmpty(user.Avatar) || 
                                       user.Avatar.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
                                       user.Avatar.StartsWith("https://", StringComparison.OrdinalIgnoreCase);

                        if (needsSync)
                        {
                            var oldAvatarPath = user.Avatar;
                            var oldAvatarPublicId = user.AvatarPublicId;
                            var (newAvatarUrl, newAvatarPublicId) = await UploadGoogleAvatarToCloudinaryAsync(googlePayload.Picture);
                            
                            if (!string.IsNullOrEmpty(newAvatarUrl))
                            {
                                // Delete old avatar
                                await DeleteOldAvatarIfExistsAsync(oldAvatarPath, oldAvatarPublicId, webRoot);
                                user.Avatar = newAvatarUrl;
                                user.AvatarPublicId = newAvatarPublicId;
                                user.UpdatedDate = DateTime.UtcNow;
                                user.LastUpdateUser = "Google";
                                await _userRepository.UpdateAsync(user);
                                _logger.LogInformation($"Updated avatar for user {user.Email}");
                            }
                        }
                    }
                    
                    return _mapper.Map<UserDto>(user);
                }

                // If not found by provider, check by email (for auto-linking)
                var userByEmail = await _userRepository.GetUserByEmail(googlePayload.Email);
                if (userByEmail != null)
                {
                    // User exists with this email but not linked to Google, link it
                    _logger.LogInformation($"Linking existing user {userByEmail.Email} to Google account");
                    userByEmail.Provider = "Google";
                    userByEmail.ProviderId = googlePayload.Sub;
                    
                    if (!string.IsNullOrEmpty(googlePayload.Picture))
                    {
                        var oldAvatarPath = userByEmail.Avatar;
                        var oldAvatarPublicId = userByEmail.AvatarPublicId;
                        var (newAvatarUrl, newAvatarPublicId) = await UploadGoogleAvatarToCloudinaryAsync(googlePayload.Picture);
                        
                        if (!string.IsNullOrEmpty(newAvatarUrl))
                        {
                            // Delete old avatar
                            await DeleteOldAvatarIfExistsAsync(oldAvatarPath, oldAvatarPublicId, webRoot);
                            userByEmail.Avatar = newAvatarUrl;
                            userByEmail.AvatarPublicId = newAvatarPublicId;
                        }
                        else
                        {
                            // If upload fails, keep old avatar or set to Google URL as fallback
                            if (string.IsNullOrEmpty(userByEmail.Avatar))
                            {
                                userByEmail.Avatar = googlePayload.Picture;
                            }
                        }
                    }
                    
                    if (!string.IsNullOrEmpty(googlePayload.Name) && string.IsNullOrEmpty(userByEmail.Name))
                    {
                        userByEmail.Name = googlePayload.Name;
                    }
                    userByEmail.UpdatedDate = DateTime.UtcNow;
                    userByEmail.LastUpdateUser = "Google";
                    await _userRepository.UpdateAsync(userByEmail);
                    return _mapper.Map<UserDto>(userByEmail);
                }

                // User doesn't exist, create new user
                _logger.LogInformation($"Creating new Google user: {googlePayload.Email}");
                int roleGroupId = 0;

                // Try to assign "Manager" or "Quản lý" role group by default
                try
                {
                    var allRolesQuery = await _roleGroupRepository.GetAllRoleGroupAsync();
                    var managerRole = allRolesQuery.FirstOrDefault(x => x.Name.ToLower() == "manager" || x.Name.ToLower() == "quản lý");
                    if (managerRole != null)
                    {
                        roleGroupId = (int)managerRole.Id;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Failed to query Manager role group: {ex.Message}");
                }

                // Fallback to appsettings.json if no Manager role found
                if (roleGroupId == 0)
                {
                    var defaultRoleGroupId = _configuration["Google:DefaultRoleGroupId"];
                    if (!string.IsNullOrEmpty(defaultRoleGroupId) && int.TryParse(defaultRoleGroupId, out int parsedRoleId))
                    {
                        roleGroupId = parsedRoleId;
                    }
                }

                string? avatarPath = null;
                string? avatarPublicId = null;
                if (!string.IsNullOrEmpty(googlePayload.Picture))
                {
                    var result = await UploadGoogleAvatarToCloudinaryAsync(googlePayload.Picture);
                    avatarPath = result.Url;
                    avatarPublicId = result.PublicId;

                    if (string.IsNullOrEmpty(avatarPath))
                    {
                        // If upload fails, use Google URL as fallback
                        avatarPath = googlePayload.Picture;
                        _logger.LogWarning($"Failed to upload avatar to Cloudinary for new user {googlePayload.Email}, using Google URL as fallback");
                    }
                }

                var newUser = new Users
                {
                    Email = googlePayload.Email,
                    Name = googlePayload.Name ?? googlePayload.Email,
                    Provider = "Google",
                    ProviderId = googlePayload.Sub,
                    Avatar = avatarPath ?? string.Empty,
                    AvatarPublicId = avatarPublicId,
                    RoleGroupId = roleGroupId,
                    Password = Guid.NewGuid().ToString(), // Placeholder password for Google users (never used)
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    CreatorUser = "Google",
                    LastUpdateUser = "Google"

                };

                // For Google users, we don't hash the password since it's just a placeholder
                // We'll add directly to repository without going through AddAsync which hashes
                newUser.CreatedDate = newUser.UpdatedDate = DateTime.UtcNow;
                await _userRepository.AddAsync(newUser);
                return _mapper.Map<UserDto>(newUser);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to find or create Google user: {ex.Message}");
                throw;
            }
        }
    }
}
