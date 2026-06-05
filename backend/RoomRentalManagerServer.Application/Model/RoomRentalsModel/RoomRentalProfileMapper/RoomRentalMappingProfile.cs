using AutoMapper;
using RoomRentalManagerServer.Application.Model.RoomRentalsModel.Dto;
using RoomRentalManagerServer.Domain.ModelEntities.RoomRentals;

namespace RoomRentalManagerServer.Application.Model.RoomRentalsModel.RoomRentalProfileMapper
{
    public class RoomRentalMappingProfile : Profile
    {
        public RoomRentalMappingProfile()
        {
            CreateMap<RoomRental, RoomRentalDto>().ReverseMap();
            CreateMap<CreateOrEditRoomRentalDto, RoomRental>().ReverseMap();
        }
    }
}
