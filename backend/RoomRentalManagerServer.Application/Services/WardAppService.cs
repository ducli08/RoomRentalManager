using AutoMapper;
using Microsoft.Extensions.Logging;
using RoomRentalManagerServer.Application.Interfaces;
using RoomRentalManagerServer.Domain.Interfaces.WardInterfaces;
using RoomRentalManagerServer.Domain.ModelEntities.Wards;

namespace RoomRentalManagerServer.Application.Services
{
    public class WardAppService : IWardAppService
    {
        private readonly ILogger<WardAppService> _logger;
        private readonly IMapper _mapper;
        private readonly IWardRepository _wardRepository;
        public WardAppService(ILogger<WardAppService> logger, IWardRepository wardRepository, IMapper mapper)
        {
            _logger = logger;
            _mapper = mapper;
            _wardRepository = wardRepository;
        }

        public async Task<List<Ward>> GetAllWardsAsync(string? districtCode)
        {
            try
            {
                var wardQuery = await _wardRepository.GetAllQueryAsync();
                if (!string.IsNullOrEmpty(districtCode))
                {
                    wardQuery = wardQuery.Where(d => d.DistrictCode == districtCode);
                }
                return wardQuery.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all districts");
                throw;
            }
        }
    }
}
