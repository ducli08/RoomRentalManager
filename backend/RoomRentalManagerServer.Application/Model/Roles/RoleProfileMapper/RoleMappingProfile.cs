using AutoMapper;
using RoomRentalManagerServer.Application.Model.Roles.Dto;
using RoomRentalManagerServer.Application.Model.RoomRentalsModel.Dto;
using RoomRentalManagerServer.Domain.ModelEntities.Roles;
using RoomRentalManagerServer.Domain.ModelEntities.RoomRentals;

namespace RoomRentalManagerServer.Application.Model.Roles.RoleProfileMapper
{
    public class RoleMappingProfile : Profile
    {
        public RoleMappingProfile()
        {
            CreateMap<Role, RoleDto>().ReverseMap();
        }
    }
}
