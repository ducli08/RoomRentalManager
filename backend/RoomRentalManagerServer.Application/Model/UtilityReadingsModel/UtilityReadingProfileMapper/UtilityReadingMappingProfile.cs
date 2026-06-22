using AutoMapper;
using RoomRentalManagerServer.Application.Model.UtilityReadingsModel.Dto;
using RoomRentalManagerServer.Domain.ModelEntities.UtilityReadings;

namespace RoomRentalManagerServer.Application.Model.UtilityReadingsModel.UtilityReadingProfileMapper
{
    public class UtilityReadingMappingProfile : Profile
    {
        public UtilityReadingMappingProfile()
        {
            CreateMap<UtilityReading, UtilityReadingDto>();
            CreateMap<CreateOrEditUtilityReadingDto, UtilityReading>()
                .ForMember(dest => dest.OldElectricIndex, opt => opt.Ignore())
                .ForMember(dest => dest.OldWaterIndex, opt => opt.Ignore())
                .ForMember(dest => dest.ElectricUsage, opt => opt.Ignore())
                .ForMember(dest => dest.WaterUsage, opt => opt.Ignore())
                .ForMember(dest => dest.ElectricUnitPrice, opt => opt.Ignore())
                .ForMember(dest => dest.WaterUnitPrice, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore());
        }
    }
}
