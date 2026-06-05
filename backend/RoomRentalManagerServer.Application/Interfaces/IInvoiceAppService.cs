using RoomRentalManagerServer.Application.Common.CommonDto;
using RoomRentalManagerServer.Application.Model.InvoicesModel.Dto;

namespace RoomRentalManagerServer.Application.Interfaces
{
    public interface IInvoiceAppService
    {
        Task<bool> CreateOrEditAsync(CreateOrEditInvoiceDto input);
        Task<InvoiceDto?> GetByIdAsync(long id);
        Task<InvoiceDto?> GetMyInvoiceByIdAsync(long id);
        Task<PagedResultDto<InvoiceDto>> GetAllAsync(PagedRequestDto<InvoiceFilterDto> requestDto);
        Task<bool> IssueAsync(long id);
        Task<bool> CancelAsync(long id);

        Task<PagedResultDto<InvoiceDto>> GetMyInvoicesAsync(PagedRequestDto<InvoiceFilterDto> requestDto);
    }
}

