using RoomRentalManagerServer.Application.Common.CommonDto;

namespace RoomRentalManagerServer.Application.Common.CommonAppService
{
    public interface ICommonAppService
    {
        Task<List<SelectListItemDto>> GetCustomSelectListItem(string typeSelect, string cascadeValue);
        List<SelectListItemDto> GetEnumSelectListItem(string enumTypeName);
    }
}
