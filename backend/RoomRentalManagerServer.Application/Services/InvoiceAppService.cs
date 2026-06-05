using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoomRentalManagerServer.Application.Common.CommonDto;
using RoomRentalManagerServer.Application.Interfaces;
using RoomRentalManagerServer.Application.Model.InvoicesModel.Dto;
using RoomRentalManagerServer.Domain.Interfaces.ContractInterfaces;
using RoomRentalManagerServer.Domain.Interfaces.InvoiceInterfaces;
using RoomRentalManagerServer.Domain.ModelEntities.Contracts;
using RoomRentalManagerServer.Domain.ModelEntities.Invoices;

namespace RoomRentalManagerServer.Application.Services
{
    public class InvoiceAppService : IInvoiceAppService
    {
        private readonly ILogger<InvoiceAppService> _logger;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IContractRepository _contractRepository;
        private readonly ICurrentUserAppService _currentUser;
        private readonly IMapper _mapper;

        public InvoiceAppService(
            ILogger<InvoiceAppService> logger,
            IInvoiceRepository invoiceRepository,
            IContractRepository contractRepository,
            ICurrentUserAppService currentUser,
            IMapper mapper)
        {
            _logger = logger;
            _invoiceRepository = invoiceRepository;
            _contractRepository = contractRepository;
            _currentUser = currentUser;
            _mapper = mapper;
        }

        public async Task<bool> CreateOrEditAsync(CreateOrEditInvoiceDto input)
        {
            if (!_currentUser.IsAuthenticated)
                throw new UnauthorizedAccessException("User is not authenticated.");

            if (input == null) return false;

            try
            {
                var now = DateTime.UtcNow;
                if (input.Id is null)
                {
                    var invoice = new Invoice
                    {
                        ContractId = input.ContractId,
                        InvoiceDate = input.InvoiceDate,
                        DueDate = input.DueDate,
                        TotalAmount = input.TotalAmount,
                        AmountPaid = 0,
                        Status = InvoiceStatus.Draft,
                        CreatedAt = now,
                        UpdatedAt = now,
                        CreatorUser = _currentUser.UserName,
                        LastUpdateUser = _currentUser.UserName
                    };
                    await _invoiceRepository.AddAsync(invoice);
                    return true;
                }

                var existing = await _invoiceRepository.GetByIdAsync(input.Id.Value, asNoTracking: false);
                if (existing == null) return false;
                if (existing.Status != InvoiceStatus.Draft)
                    throw new InvalidOperationException("Only Draft invoices can be edited.");

                existing.ContractId = input.ContractId;
                existing.InvoiceDate = input.InvoiceDate;
                existing.DueDate = input.DueDate;
                existing.TotalAmount = input.TotalAmount;
                existing.UpdatedAt = now;
                existing.LastUpdateUser = _currentUser.UserName;

                await _invoiceRepository.UpdateAsync(existing);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create or edit invoice");
                throw;
            }
        }

        public async Task<InvoiceDto?> GetByIdAsync(long id)
        {
            try
            {
                var invoice = await _invoiceRepository.GetByIdAsync(id, asNoTracking: true);
                if (invoice == null) return null;
                return MapToDto(invoice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get invoice by id {Id}", id);
                throw;
            }
        }

        public async Task<InvoiceDto?> GetMyInvoiceByIdAsync(long id)
        {
            if (!_currentUser.IsAuthenticated || _currentUser.GetUserId is null)
                throw new UnauthorizedAccessException("User is not authenticated.");

            try
            {
                var invoice = await _invoiceRepository.GetByIdAsync(id, asNoTracking: true);
                if (invoice == null) return null;

                var contract = await _contractRepository.GetByIdAsync(invoice.ContractId, asNoTracking: true);
                if (contract == null) return null;
                if (contract.StatusContract != StatusContract.Active) return null;
                if (contract.TenantId != _currentUser.GetUserId.Value) return null;

                return MapToDto(invoice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get my invoice by id {Id}", id);
                throw;
            }
        }

        public async Task<PagedResultDto<InvoiceDto>> GetAllAsync(PagedRequestDto<InvoiceFilterDto> requestDto)
        {
            try
            {
                var query = _invoiceRepository.Query().AsNoTracking();

                if (requestDto?.Filter?.ContractId is not null)
                    query = query.Where(x => x.ContractId == requestDto.Filter.ContractId.Value);
                if (requestDto?.Filter?.Status is not null)
                    query = query.Where(x => x.Status == requestDto.Filter.Status.Value);

                // Overdue is computed (Issued & balanceDue>0 & dueDate < now)
                if (requestDto?.Filter?.IsOverdue is not null)
                {
                    var now = DateTime.UtcNow;
                    if (requestDto.Filter.IsOverdue.Value)
                        query = query.Where(x => x.Status == InvoiceStatus.Issued && (x.TotalAmount - x.AmountPaid) > 0 && x.DueDate < now);
                    else
                        query = query.Where(x => !(x.Status == InvoiceStatus.Issued && (x.TotalAmount - x.AmountPaid) > 0 && x.DueDate < now));
                }

                var total = await query.CountAsync();
                var page = requestDto?.Page ?? 1;
                var pageSize = requestDto?.PageSize ?? 10;

                var items = await query
                    .OrderByDescending(x => x.UpdatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PagedResultDto<InvoiceDto>(items.Select(MapToDto).ToList(), total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to list invoices");
                throw;
            }
        }

        public async Task<bool> IssueAsync(long id)
        {
            if (!_currentUser.IsAuthenticated)
                throw new UnauthorizedAccessException("User is not authenticated.");

            try
            {
                var invoice = await _invoiceRepository.GetByIdAsync(id, asNoTracking: false);
                if (invoice == null) return false;
                if (invoice.Status != InvoiceStatus.Draft)
                    throw new InvalidOperationException("Only Draft invoices can be issued.");

                var now = DateTime.UtcNow;
                invoice.Status = InvoiceStatus.Issued;
                invoice.IssuedAt = now;
                invoice.UpdatedAt = now;
                invoice.LastUpdateUser = _currentUser.UserName;
                await _invoiceRepository.UpdateAsync(invoice);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to issue invoice {Id}", id);
                throw;
            }
        }

        public async Task<bool> CancelAsync(long id)
        {
            if (!_currentUser.IsAuthenticated)
                throw new UnauthorizedAccessException("User is not authenticated.");

            try
            {
                var invoice = await _invoiceRepository.GetByIdAsync(id, asNoTracking: false);
                if (invoice == null) return false;

                if (invoice.Status != InvoiceStatus.Issued)
                    throw new InvalidOperationException("Only Issued invoices can be cancelled.");

                if (invoice.AmountPaid > 0)
                    throw new InvalidOperationException("Cannot cancel an invoice that has collected any payment.");

                var now = DateTime.UtcNow;
                invoice.Status = InvoiceStatus.Cancelled;
                invoice.CancelledAt = now;
                invoice.UpdatedAt = now;
                invoice.LastUpdateUser = _currentUser.UserName;
                await _invoiceRepository.UpdateAsync(invoice);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cancel invoice {Id}", id);
                throw;
            }
        }

        public async Task<PagedResultDto<InvoiceDto>> GetMyInvoicesAsync(PagedRequestDto<InvoiceFilterDto> requestDto)
        {
            if (!_currentUser.IsAuthenticated || _currentUser.GetUserId is null)
                throw new UnauthorizedAccessException("User is not authenticated.");

            try
            {
                var tenantId = _currentUser.GetUserId.Value;
                var activeContractsQuery = _contractRepository.Query()
                    .AsNoTracking()
                    .Where(x => x.TenantId == tenantId && x.StatusContract == StatusContract.Active);

                var activeContractIds = activeContractsQuery.Select(x => x.Id);

                var query = _invoiceRepository.Query()
                    .AsNoTracking()
                    .Where(x => activeContractIds.Contains(x.ContractId));

                if (requestDto?.Filter?.Status is not null)
                    query = query.Where(x => x.Status == requestDto.Filter.Status.Value);

                var total = await query.CountAsync();
                var page = requestDto?.Page ?? 1;
                var pageSize = requestDto?.PageSize ?? 10;

                var items = await query
                    .OrderByDescending(x => x.DueDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PagedResultDto<InvoiceDto>(items.Select(MapToDto).ToList(), total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to list my invoices");
                throw;
            }
        }

        private static InvoiceDto MapToDto(Invoice invoice)
        {
            var balanceDue = Math.Max(0, invoice.TotalAmount - invoice.AmountPaid);
            var isOverdue = invoice.Status == InvoiceStatus.Issued && balanceDue > 0 && invoice.DueDate < DateTime.UtcNow;
            return new InvoiceDto
            {
                Id = invoice.Id,
                ContractId = invoice.ContractId,
                InvoiceDate = invoice.InvoiceDate,
                DueDate = invoice.DueDate,
                TotalAmount = invoice.TotalAmount,
                AmountPaid = invoice.AmountPaid,
                BalanceDue = balanceDue,
                IsOverdue = isOverdue,
                Status = invoice.Status
            };
        }
    }
}

