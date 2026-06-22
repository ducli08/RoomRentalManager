using RoomRentalManagerServer.Application.Common.CommonDto;
using RoomRentalManagerServer.Application.Model.UtilityReadingsModel.Dto;

namespace RoomRentalManagerServer.Application.Interfaces
{
    public interface IUtilityReadingAppService
    {
        Task<UtilityReadingPrepareDto> GetPrepareAsync(long contractId, int month, int year, long? utilityReadingId = null);
        Task<UtilityReadingDto?> GetByIdAsync(long id);
        Task<PagedResultDto<UtilityReadingDto>> GetAllAsync(PagedRequestDto<UtilityReadingFilterDto> request);
        Task<UtilityReadingDto> CreateOrEditAsync(CreateOrEditUtilityReadingDto input);
        Task<byte[]> ExportExcelAsync(UtilityReadingFilterDto filter);
    }
}
