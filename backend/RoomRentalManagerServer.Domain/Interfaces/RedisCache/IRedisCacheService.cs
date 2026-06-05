using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomRentalManagerServer.Domain.Interfaces.RedisCache
{
    public interface IRedisCacheService
    {
        Task SetAsync<T>(string key, List<T> value, TimeSpan? expiry = null);
        Task<List<T>> GetAsync<T>(string key);
        Task RemoveAsync(string key);
    }
}
