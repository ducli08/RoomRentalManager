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
using RoomRentalManagerServer.Domain.ModelEntities.UtilityReadings;

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
                if (existing.UtilityReadingId.HasValue)
                    throw new InvalidOperationException("Hóa đơn được tạo từ chỉ số điện nước chỉ có thể sửa qua module Chỉ số điện nước.");
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
                if (!ContractContainsTenant(contract, _currentUser.GetUserId.Value)) return null;

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
                    .Where(x => x.StatusContract == StatusContract.Active
                        && (x.TenantId == tenantId || x.TenantIds.Contains(tenantId)));

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
                UtilityReadingId = invoice.UtilityReadingId,
                Month = invoice.Month,
                Year = invoice.Year,
                InvoiceDate = invoice.InvoiceDate,
                DueDate = invoice.DueDate,
                TotalAmount = invoice.TotalAmount,
                AmountPaid = invoice.AmountPaid,
                BalanceDue = balanceDue,
                IsOverdue = isOverdue,
                Status = invoice.Status
            };
        }

        public async Task EnsureUtilityReadingInvoiceEditableAsync(long? utilityReadingId)
        {
            if (!utilityReadingId.HasValue) return;

            var locked = await IsUtilityReadingInvoiceLockedByPaymentAsync(utilityReadingId);
            if (locked)
            {
                throw new InvalidOperationException("Không thể sửa chỉ số vì hóa đơn đã có thanh toán.");
            }
        }

        public async Task<bool> IsUtilityReadingInvoiceLockedByPaymentAsync(long? utilityReadingId)
        {
            if (utilityReadingId.HasValue)
            {
                var invoice = await GetActiveInvoiceByUtilityReadingIdAsync(utilityReadingId.Value);
                if (invoice != null && (invoice.AmountPaid > 0 || invoice.Status == InvoiceStatus.Paid))
                {
                    return true;
                }
            }

            return false;
        }

        public async Task<long> GenerateOrRegenerateFromUtilityReadingAsync(UtilityReading reading, Contract contract)
        {
            if (!_currentUser.IsAuthenticated)
                throw new UnauthorizedAccessException("User is not authenticated.");

            await EnsureUtilityReadingInvoiceEditableAsync(reading.Id);

            var existing = await GetActiveInvoiceByUtilityReadingIdAsync(reading.Id);
            if (existing != null)
            {
                if (existing.Status == InvoiceStatus.Issued && existing.AmountPaid == 0)
                {
                    await CancelInvoiceInternalAsync(existing);
                }
                else if (existing.Status == InvoiceStatus.Draft)
                {
                    await CancelInvoiceInternalAsync(existing, allowDraft: true);
                }
            }

            var totalAmount = CalculateTotalAmount(reading, contract);
            var invoiceDate = new DateTime(reading.Year, reading.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var now = DateTime.UtcNow;

            var invoice = new Invoice
            {
                ContractId = contract.Id,
                UtilityReadingId = reading.Id,
                Month = reading.Month,
                Year = reading.Year,
                InvoiceDate = invoiceDate,
                DueDate = invoiceDate.AddDays(7),
                TotalAmount = totalAmount,
                AmountPaid = 0,
                Status = InvoiceStatus.Issued,
                IssuedAt = now,
                CreatedAt = now,
                UpdatedAt = now,
                CreatorUser = _currentUser.UserName ?? "system",
                LastUpdateUser = _currentUser.UserName ?? "system"
            };

            await _invoiceRepository.AddAsync(invoice);
            return invoice.Id;
        }

        private static decimal CalculateTotalAmount(UtilityReading reading, Contract contract)
        {
            var garbage = reading.Month == 1 ? contract.GarbageFeePerYear : 0;
            return contract.MonthlyRent
                   + reading.ElectricUsage * reading.ElectricUnitPrice
                   + reading.WaterUsage * reading.WaterUnitPrice
                   + garbage;
        }

        private static bool ContractContainsTenant(Contract contract, long tenantId)
        {
            return contract.TenantId == tenantId || contract.TenantIds.Contains(tenantId);
        }

        private async Task<Invoice?> GetActiveInvoiceByUtilityReadingIdAsync(long utilityReadingId)
        {
            return await _invoiceRepository.Query()
                .Where(x => x.UtilityReadingId == utilityReadingId && x.Status != InvoiceStatus.Cancelled)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();
        }

        private async Task CancelInvoiceInternalAsync(Invoice invoice, bool allowDraft = false)
        {
            if (invoice.Status == InvoiceStatus.Issued)
            {
                if (invoice.AmountPaid > 0)
                    throw new InvalidOperationException("Cannot cancel an invoice that has collected any payment.");

                invoice.Status = InvoiceStatus.Cancelled;
            }
            else if (invoice.Status == InvoiceStatus.Draft && allowDraft)
            {
                invoice.Status = InvoiceStatus.Cancelled;
            }
            else
            {
                throw new InvalidOperationException("Only Issued or Draft invoices can be cancelled for regeneration.");
            }

            var now = DateTime.UtcNow;
            invoice.CancelledAt = now;
            invoice.UpdatedAt = now;
            invoice.LastUpdateUser = _currentUser.UserName ?? "system";
            await _invoiceRepository.UpdateAsync(invoice);
        }
    }
}

