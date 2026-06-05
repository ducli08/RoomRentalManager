using AutoMapper;
using RoomRentalManagerServer.Application.Model.ImageDescriptionsModel.Dto;
using RoomRentalManagerServer.Domain.ModelEntities.ImageDescriptions;

namespace RoomRentalManagerServer.Application.Model.ImageDescriptionsModel.ImageDescriptionProfileMapper
{
    public class ImageDescriptionMappingProfile : Profile
    {
        public ImageDescriptionMappingProfile()
        {
            CreateMap<ImagesDescription, ImageDescriptionDto>().ReverseMap();
            CreateMap<CreateOrEditImageDescriptionDto, ImagesDescription>().ReverseMap();
        }
    }
}
