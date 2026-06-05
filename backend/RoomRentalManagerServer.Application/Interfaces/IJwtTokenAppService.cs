using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomRentalManagerServer.Application.Interfaces
{
    public interface IJwtTokenAppService
    {
        string GenerateToken(long userId, string userName, DateTime expiresTime);
        string GenerateRefreshToken();
    }
}
