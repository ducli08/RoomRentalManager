using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RoomRentalManagerServer.Application.Interfaces;
using RoomRentalManagerServer.Domain.Interfaces.ProvinceInterface;
using RoomRentalManagerServer.Domain.Interfaces.RedisCache;
using RoomRentalManagerServer.Domain.ModelEntities.Provinces;
using System.Diagnostics;

namespace RoomRentalManagerServer.Application.Services
{
    public class ProvinceAppService : IProvinceAppService
    {
        private readonly ILogger<ProvinceAppService> _logger;
        private readonly IMapper _mapper;
        private readonly IProvinceRepository _provinceRepository;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IConfiguration _configuration;
        public ProvinceAppService(ILogger<ProvinceAppService> logger, IProvinceRepository provinceRepository, IMapper mapper,
            IRedisCacheService redisCacheService, IConfiguration configuration)
        {
            _logger = logger;
            _mapper = mapper;
            _provinceRepository = provinceRepository;
            _redisCacheService = redisCacheService;
            _configuration = configuration;
        }

        public async Task<List<Province>> GetAllProvincesAsync()
        {
            try
            {
                Stopwatch stC = new Stopwatch();
                stC.Start();
                Stopwatch st = new Stopwatch();
                st.Start();
                var value = await _redisCacheService.GetAsync<Province>(_configuration["Redis:Keys:Province"]);
                var provinces = new List<Province>();
                if (value != null && value?.Count != 0)
                {
                    provinces = value;
                    stC.Stop();
                    _logger.LogInformation($"GetAllProvincesAsync from Redis Cache: {stC.ElapsedMilliseconds} ms");
                }
                else
                {
                    var provincesQuery = await _provinceRepository.GetAllQueryAsync();
                    provinces = await provincesQuery.ToListAsync();
                    st.Stop();
                    _logger.LogInformation($"GetAllProvincesAsync from Database: {st.ElapsedMilliseconds} ms");
                    if (provinces != null)
                    {
                        st.Start();
                        await _redisCacheService.SetAsync<Province>(_configuration["Redis:Keys:Province"], provinces, TimeSpan.FromMinutes(30));
                        st.Stop();
                        _logger.LogInformation($"Time to set cache Province into Redis {st.ElapsedMilliseconds} ms");
                    }
                    
                }
                return provinces;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all provinces");
                throw;
            }
        }
    }
}
