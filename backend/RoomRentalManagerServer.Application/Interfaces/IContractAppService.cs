using RoomRentalManagerServer.Application.Common.CommonDto;
using RoomRentalManagerServer.Application.Model.ContractsModel.Dto;

namespace RoomRentalManagerServer.Application.Interfaces
{
    public interface IContractAppService
    {
        Task<PagedResultDto<ContractDto>> GetAllContractAsync(PagedRequestDto<ContractFilterDto> pagedRequestDto);
        Task<ContractDto?> GetContractByIdAsync(long id);
        Task<bool> CreateOrEditContractAsync(CreateOrEditContractDto input);
        Task DeleteContractAsync(long id);
        Task<List<SelectListItemDto>> GetActiveContractsForSelectListItemAsync();
    }
}
