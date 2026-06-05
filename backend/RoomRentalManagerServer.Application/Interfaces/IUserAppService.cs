using Microsoft.AspNetCore.Http;
using RoomRentalManagerServer.Application.Common.CommonDto;
using RoomRentalManagerServer.Application.Model.Login.Dto;
using RoomRentalManagerServer.Application.Model.UsersModel.Dto;
using RoomRentalManagerServer.Domain.ModelEntities.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomRentalManagerServer.Application.Interfaces
{
    public interface IUserAppService
    {
        Task<(List<string> Paths, List<string> PublicIds, List<string> Errors)> UploadAvatarAsync(List<IFormFile> avatar, string webRoot);
        Task<PagedResultDto<UserDto>> GetAllUsersAsync(PagedRequestDto<UserFilterDto> pagedRequestDto);
        Task<UserDto> GetUserByIdAsync(long id);
        Task<bool> CreateOrEditUserAsync(CreateOrEditUserDto input);
        Task DeleteUserAsync(long id);
        Task<UserDto> Authentication(string username, string password);
        Task<List<Users>> GetAllUserForSelectListItem();
        Task<UserDto> FindOrCreateGoogleUserAsync(GoogleTokenPayload googlePayload, string webRoot);
    }
}
