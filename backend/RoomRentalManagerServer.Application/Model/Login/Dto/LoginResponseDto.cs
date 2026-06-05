using RoomRentalManagerServer.Application.Model.UsersModel.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomRentalManagerServer.Application.Model.Login.Dto
{
    public class LoginResponseDto
    {
        public string Message { get; set; }
        public string AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public UserDto User { get; set; }
        // UTC expiry timestamp for access token
        public DateTime ExpiresAt { get; set; }
        // Number of seconds until access token expires
        public int ExpiresIn { get; set; }

        // Optional: refresh token expiry info (present only when refresh token issued)
        public DateTime? RefreshExpiresAt { get; set; }
        public int? RefreshExpiresIn { get; set; }
        public List<string> RoleGroupPermissions { get; set; }

    }
}
