using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomRentalManagerServer.Application.Interfaces
{
    public interface ICurrentUserAppService
    {
        long? GetUserId { get; }
        string UserName { get; }
        bool IsAuthenticated { get; }
    }
}
