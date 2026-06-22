using AutoMapper;
using RoomRentalManagerServer.Application.Model.ContractsModel.Dto;
using RoomRentalManagerServer.Domain.ModelEntities.Contracts;

namespace RoomRentalManagerServer.Application.Model.ContractsModel.ContractProfileMapper
{
    public class ContractMappingProfile : Profile
    {
        public ContractMappingProfile()
        {
            CreateMap<Contract, ContractDto>().ReverseMap();
            CreateMap<CreateOrEditContractDto, Contract>()
                .ForMember(dest => dest.DepositAmout, opt => opt.Ignore())
                .ForMember(dest => dest.MonthlyRent, opt => opt.Ignore())
                .ForMember(dest => dest.ElectricUnitPrice, opt => opt.Ignore())
                .ForMember(dest => dest.WaterUnitPrice, opt => opt.Ignore())
                .ForMember(dest => dest.GarbageFeePerYear, opt => opt.Ignore());
        }
    }
}
