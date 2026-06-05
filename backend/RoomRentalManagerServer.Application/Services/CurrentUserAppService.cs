using Microsoft.AspNetCore.Http;
using RoomRentalManagerServer.Application.Interfaces;
using System.Security.Claims;

namespace RoomRentalManagerServer.Application.Services
{
    public class CurrentUserAppService : ICurrentUserAppService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CurrentUserAppService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public long? GetUserId => long.TryParse(_httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;
        public string UserName => _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? string.Empty;
        public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    }
}
