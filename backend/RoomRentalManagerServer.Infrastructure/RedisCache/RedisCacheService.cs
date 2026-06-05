using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RoomRentalManagerServer.Domain.Interfaces.RedisCache;
using StackExchange.Redis;
using System.Diagnostics;
using System.Net;

namespace RoomRentalManagerServer.Infrastructure.RedisCache
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IDatabase _database;
        private readonly ILogger<RedisCacheService> _logger;

        public RedisCacheService(IConfiguration configuration, IConnectionMultiplexer redis, ILogger<RedisCacheService> logger)
        {
            _database = redis.GetDatabase();
            _logger = logger;
        }

        public async Task SetAsync<T>(string key, List<T> value, TimeSpan? expiry = null)
        {
            Stopwatch st = new Stopwatch();
            st.Start();
            var json = JsonConvert.SerializeObject(value);
            await _database.StringSetAsync(key, json, expiry);
            st.Stop();
            _logger.LogInformation($"Time to set cache into RedisCacheService: {st.ElapsedMilliseconds} ms");
        }

        public async Task<List<T>> GetAsync<T>(string key)
        {
            Stopwatch st = new Stopwatch();
            st.Start();
            var json = await _database.StringGetAsync(key);
            if (string.IsNullOrEmpty(json))
            {
                st.Stop();
                _logger.LogInformation($"Time to get cache from RedisCacheService: {st.ElapsedMilliseconds} ms");
                return new List<T>();
            }
            else
            {
                st.Stop();
                _logger.LogInformation($"Time to get cache from RedisCacheService: {st.ElapsedMilliseconds} ms");
                return JsonConvert.DeserializeObject<List<T>>(json) ?? new List<T>();
            }
            
        }

        public async Task RemoveAsync(string key)
        {
            Stopwatch st = new Stopwatch();
            st.Start();
            await _database.KeyDeleteAsync(key);
            st.Stop();
            _logger.LogInformation($"Time to remove cache from RedisCacheService: {st.ElapsedMilliseconds} ms");
        }
    }
}
