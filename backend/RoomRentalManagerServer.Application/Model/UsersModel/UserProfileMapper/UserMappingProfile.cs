using AutoMapper;
using RoomRentalManagerServer.Application.Model.UsersModel.Dto;
using RoomRentalManagerServer.Domain.ModelEntities.User;
namespace RoomRentalManagerServer.Application.Model.UsersModel.UserProfileMapper
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            CreateMap<Users, CreateOrEditUserDto>().ReverseMap();
            CreateMap<Users, UserDto>().ReverseMap();
            CreateMap<Users, Users>();
        }
    }
}
