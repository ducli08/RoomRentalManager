using AutoMapper;
using RoomRentalManagerServer.Application.Model.RoleGroupsModel.Dto;
using RoomRentalManagerServer.Domain.ModelEntities.RoleGroups;

namespace RoomRentalManagerServer.Application.Model.RoleGroupsModel.RoleGroupMapper
{
    public class RoleGroupMappingProfile : Profile
    {
        public RoleGroupMappingProfile()
        {
            CreateMap<RoleGroup, CreateOrEditRoleGroupDto>().ReverseMap();
            CreateMap<RoleGroup, RoleGroupDto>().ReverseMap();
        }
    }
}
