using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RoomRentalManagerServer.Application.Interfaces;
using RoomRentalManagerServer.Domain.Interfaces.DistrictInterfaces;
using RoomRentalManagerServer.Domain.Interfaces.RedisCache;
using RoomRentalManagerServer.Domain.ModelEntities.Districts;
using System.Diagnostics;

namespace RoomRentalManagerServer.Application.Services
{
    public class DistrictAppService : IDistrictAppService
    {
        private readonly ILogger<DistrictAppService> _logger;
        private readonly IMapper _mapper;
        private readonly IDistrictRepository _districtRepository;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IConfiguration _configuration;
        public DistrictAppService(ILogger<DistrictAppService> logger, IDistrictRepository districtRepository, IMapper mapper,
            IRedisCacheService redisCacheService, IConfiguration configuration)
        {
            _logger = logger;
            _mapper = mapper;
            _districtRepository = districtRepository;
            _redisCacheService = redisCacheService;
            _configuration = configuration;
        }

        public async Task<List<District>> GetAllDistrictsAsync(string? provinceCode)
        {
            try
            {
                Stopwatch stC = new Stopwatch();
                stC.Start();
                Stopwatch st = new Stopwatch();
                st.Start();
                var key = _configuration["Redis:Keys:District"];
                if (!string.IsNullOrEmpty(key))
                {
                    var value = await _redisCacheService.GetAsync<District>(key);
                    var districts = new List<District>();
                    if (value != null && value?.Count != 0)
                    {
                        districts = value;
                        stC.Stop();
                        _logger.LogInformation($"GetAllDistrictsAsync from Redis Cache: {stC.ElapsedMilliseconds} ms");
                    }
                    else
                    {
                        var districtsQuery = await _districtRepository.GetAllQueryAsync();
                        districts = await districtsQuery.ToListAsync();
                        st.Stop();
                        _logger.LogInformation($"GetAllDistrictsAsync from Database: {st.ElapsedMilliseconds} ms");
                        if (districts != null)
                        {
                            st.Start();
                            await _redisCacheService.SetAsync<District>(key, districts, TimeSpan.FromMinutes(30));
                            st.Stop();
                            _logger.LogInformation($"Time to set cache District into Redis: {st.ElapsedMilliseconds} ms");
                        }
                        
                    }
                    if (!string.IsNullOrEmpty(provinceCode))
                    {
                        districts = districts?.FindAll(x => x.ProvinceCode == provinceCode);
                    }
                    return districts;
                }
                else
                {
                    _logger.LogError("Redis key for District is not configured.");
                    return new List<District>();
                }    
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all districts");
                throw;
            }
        }
    }
}
