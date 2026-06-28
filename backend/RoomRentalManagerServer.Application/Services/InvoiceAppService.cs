using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoomRentalManagerServer.Application.Common;
using RoomRentalManagerServer.Application.Common.CommonDto;
using RoomRentalManagerServer.Application.Interfaces;
using RoomRentalManagerServer.Application.Model.InvoicesModel.Dto;
using RoomRentalManagerServer.Application.Model.PaymentsModel.Dto;
using RoomRentalManagerServer.Domain.Interfaces.BankAccountInterfaces;
using RoomRentalManagerServer.Domain.Interfaces.ContractInterfaces;
using RoomRentalManagerServer.Domain.Interfaces.InvoiceInterfaces;
using RoomRentalManagerServer.Domain.Interfaces.PaymentInterfaces;
using RoomRentalManagerServer.Domain.Interfaces.RoomRentalInterfaces;
using RoomRentalManagerServer.Domain.Interfaces.UserInterfaces;
using RoomRentalManagerServer.Domain.ModelEntities.Contracts;
using RoomRentalManagerServer.Domain.ModelEntities.Invoices;
using RoomRentalManagerServer.Domain.ModelEntities.PaymentAmount;
using RoomRentalManagerServer.Domain.ModelEntities.UtilityReadings;

namespace RoomRentalManagerServer.Application.Services
{
    public class InvoiceAppService : IInvoiceAppService
    {
        private readonly ILogger<InvoiceAppService> _logger;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IInvoiceItemRepository _invoiceItemRepository;
        private readonly IContractRepository _contractRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IRoomRentalRepository _roomRentalRepository;
        private readonly IUserRepository _userRepository;
        private readonly IBankAccountRepository _bankAccountRepository;
        private readonly ICurrentUserAppService _currentUser;

        public InvoiceAppService(
            ILogger<InvoiceAppService> logger,
            IInvoiceRepository invoiceRepository,
            IInvoiceItemRepository invoiceItemRepository,
            IContractRepository contractRepository,
            IPaymentRepository paymentRepository,
            IRoomRentalRepository roomRentalRepository,
            IUserRepository userRepository,
            IBankAccountRepository bankAccountRepository,
            ICurrentUserAppService currentUser)
        {
            _logger = logger;
            _invoiceRepository = invoiceRepository;
            _invoiceItemRepository = invoiceItemRepository;
            _contractRepository = contractRepository;
            _paymentRepository = paymentRepository;
            _roomRentalRepository = roomRentalRepository;
            _userRepository = userRepository;
            _bankAccountRepository = bankAccountRepository;
            _currentUser = currentUser;
        }

        public async Task<InvoiceDto?> GetByIdAsync(long id)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(id, asNoTracking: true);
            return invoice == null ? null : MapToDto(invoice);
        }

        public async Task<InvoiceDto?> GetMyInvoiceByIdAsync(long id)
        {
            if (!_currentUser.IsAuthenticated || _currentUser.GetUserId is null)
                throw new UnauthorizedAccessException("User is not authenticated.");

            var invoice = await _invoiceRepository.GetByIdAsync(id, asNoTracking: true);
            if (invoice == null) return null;

            var contract = await _contractRepository.GetByIdAsync(invoice.ContractId, asNoTracking: true);
            if (contract == null) return null;
            if (contract.StatusContract != StatusContract.Active) return null;
            if (!ContractContainsTenant(contract, _currentUser.GetUserId.Value)) return null;

            return MapToDto(invoice);
        }

        public Task<InvoiceDetailDto?> GetDetailAsync(long id, InvoiceDetailViewer viewer)
            => BuildDetailAsync(id, viewer);

        public async Task<InvoiceDetailDto?> GetMyDetailAsync(long id)
        {
            if (!_currentUser.IsAuthenticated || _currentUser.GetUserId is null)
                throw new UnauthorizedAccessException("User is not authenticated.");

            var invoice = await _invoiceRepository.GetByIdAsync(id, asNoTracking: true);
            if (invoice == null) return null;

            var contract = await _contractRepository.GetByIdAsync(invoice.ContractId, asNoTracking: true);
            if (contract == null) return null;
            if (contract.StatusContract != StatusContract.Active) return null;
            if (!ContractContainsTenant(contract, _currentUser.GetUserId.Value)) return null;

            return await BuildDetailAsync(id, InvoiceDetailViewer.Tenant);
        }

        public async Task<PagedResultDto<InvoiceDto>> GetAllAsync(PagedRequestDto<InvoiceFilterDto> requestDto)
        {
            var query = _invoiceRepository.Query().AsNoTracking();

            if (requestDto?.Filter?.ContractId is not null)
                query = query.Where(x => x.ContractId == requestDto.Filter.ContractId.Value);
            if (requestDto?.Filter?.Status is not null)
                query = query.Where(x => x.Status == requestDto.Filter.Status.Value);

            if (requestDto?.Filter?.IsOverdue is not null)
            {
                if (requestDto.Filter.IsOverdue.Value)
                    query = query.Where(x =>
                        x.Status != InvoiceStatus.Paid
                        && x.Status != InvoiceStatus.Cancelled
                        && (x.TotalAmount - x.PaidAmount) > 0
                        && x.DueDate.Date < VnDateHelper.GetVnToday());
                else
                    query = query.Where(x =>
                        x.Status == InvoiceStatus.Paid
                        || x.Status == InvoiceStatus.Cancelled
                        || (x.TotalAmount - x.PaidAmount) <= 0
                        || x.DueDate.Date >= VnDateHelper.GetVnToday());
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

        public async Task<bool> CancelAsync(long id)
        {
            if (!_currentUser.IsAuthenticated)
                throw new UnauthorizedAccessException("User is not authenticated.");

            var invoice = await _invoiceRepository.GetByIdAsync(id, asNoTracking: false)
                ?? throw new KeyNotFoundException("Invoice not found.");

            if (invoice.Status != InvoiceStatus.Unpaid)
                throw new InvalidOperationException("Chỉ có thể hủy hóa đơn ở trạng thái Chưa thanh toán.");

            if (await HasActivePaymentAsync(invoice.Id))
                throw new InvalidOperationException("Không thể hủy hóa đơn đang có giao dịch thanh toán đang hoạt động.");

            var now = DateTime.UtcNow;
            invoice.Status = InvoiceStatus.Cancelled;
            invoice.CancelledAt = now;
            invoice.UpdatedAt = now;
            invoice.LastUpdateUser = _currentUser.UserName ?? "system";
            await _invoiceRepository.UpdateAsync(invoice);
            return true;
        }

        public async Task<PagedResultDto<InvoiceDto>> GetMyInvoicesAsync(PagedRequestDto<InvoiceFilterDto> requestDto)
        {
            if (!_currentUser.IsAuthenticated || _currentUser.GetUserId is null)
                throw new UnauthorizedAccessException("User is not authenticated.");

            var tenantId = _currentUser.GetUserId.Value;
            var activeContractIds = _contractRepository.Query()
                .AsNoTracking()
                .Where(x => x.StatusContract == StatusContract.Active
                    && (x.TenantId == tenantId || x.TenantIds.Contains(tenantId)))
                .Select(x => x.Id);

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

        public async Task EnsureUtilityReadingInvoiceEditableAsync(long? utilityReadingId)
        {
            if (!utilityReadingId.HasValue) return;

            if (await IsUtilityReadingInvoiceLockedByPaymentAsync(utilityReadingId))
                throw new InvalidOperationException("Không thể sửa chỉ số vì hóa đơn đã có thanh toán hoặc giao dịch đang hoạt động.");
        }

        public async Task<bool> IsUtilityReadingInvoiceLockedByPaymentAsync(long? utilityReadingId)
        {
            if (!utilityReadingId.HasValue) return false;

            var invoice = await GetActiveInvoiceByUtilityReadingIdAsync(utilityReadingId.Value);
            if (invoice == null) return false;

            if (invoice.Status is InvoiceStatus.Paid or InvoiceStatus.WaitingTransfer or InvoiceStatus.WaitingApproval)
                return true;

            if (invoice.PaidAmount > 0)
                return true;

            return await HasActivePaymentAsync(invoice.Id);
        }

        public async Task<long> GenerateOrRegenerateFromUtilityReadingAsync(UtilityReading reading, Contract contract)
        {
            if (!_currentUser.IsAuthenticated)
                throw new UnauthorizedAccessException("User is not authenticated.");

            await EnsureUtilityReadingInvoiceEditableAsync(reading.Id);

            var existing = await GetActiveInvoiceByUtilityReadingIdAsync(reading.Id);
            if (existing != null)
            {
                if (existing.Status != InvoiceStatus.Unpaid)
                    throw new InvalidOperationException("Không thể tạo lại hóa đơn khi hóa đơn không ở trạng thái Chưa thanh toán.");

                if (await HasActivePaymentAsync(existing.Id))
                    throw new InvalidOperationException("Không thể tạo lại hóa đơn khi có giao dịch thanh toán đang hoạt động.");

                await CancelInvoiceInternalAsync(existing);
            }

            var period = BillingPeriodHelper.GetBillingPeriod(contract, reading.Month, reading.Year);
            var totalAmount = InvoiceAmountCalculator.CalculateTotalAmount(contract, reading, period);
            var roomName = await ResolveRoomNameAsync(contract.RoomRentalId);
            var tenantName = await ResolveTenantNameAsync(contract);
            var now = DateTime.UtcNow;

            var invoice = new Invoice
            {
                ContractId = contract.Id,
                UtilityReadingId = reading.Id,
                Month = reading.Month,
                Year = reading.Year,
                RoomNameSnapshot = roomName,
                TenantNameSnapshot = tenantName,
                OccupancyDaysSnapshot = period.OccupancyDays,
                PeriodStartSnapshot = period.PeriodStart,
                PeriodEndSnapshot = period.PeriodEnd,
                InvoiceDate = now,
                DueDate = period.PeriodEnd.Date.AddDays(5),
                TotalAmount = totalAmount,
                PaidAmount = 0,
                Status = InvoiceStatus.Unpaid,
                CreatedAt = now,
                UpdatedAt = now,
                CreatorUser = _currentUser.UserName ?? "system",
                LastUpdateUser = _currentUser.UserName ?? "system"
            };

            await _invoiceRepository.AddAsync(invoice);

            invoice.InvoiceCode = InvoiceCodeHelper.Build(invoice.Id, invoice.Month, invoice.Year);
            await _invoiceRepository.UpdateAsync(invoice);

            var items = InvoiceSnapshotBuilder.BuildItems(contract, reading, period);
            foreach (var item in items)
                item.InvoiceId = invoice.Id;
            await _invoiceItemRepository.AddRangeAsync(items);

            return invoice.Id;
        }

        private async Task<InvoiceDetailDto?> BuildDetailAsync(long id, InvoiceDetailViewer viewer)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(id, asNoTracking: true);
            if (invoice == null) return null;

            var items = await _invoiceItemRepository.GetByInvoiceIdAsync(id);
            var payments = await _paymentRepository.Query()
                .AsNoTracking()
                .Where(x => x.InvoiceId == id)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            var activePayment = payments.FirstOrDefault(p =>
                p.Status is PaymentStatus.PendingTransfer or PaymentStatus.WaitingForApproval);
            var lastRejected = payments.FirstOrDefault(p => p.Status == PaymentStatus.Rejected);

            var balanceDue = Math.Max(0, invoice.TotalAmount - invoice.PaidAmount);
            var daysLate = VnDateHelper.CalcDaysLate(invoice.DueDate);
            var isOverdue = VnDateHelper.IsOverdue(invoice.Status, balanceDue, invoice.DueDate);

            string? qrImageUrl = null;
            if (activePayment?.Status == PaymentStatus.PendingTransfer)
            {
                var bankAccount = await _bankAccountRepository.Query()
                    .AsNoTracking()
                    .Where(x => x.IsActive)
                    .OrderByDescending(x => x.UpdatedAt)
                    .FirstOrDefaultAsync();
                if (bankAccount != null)
                {
                    qrImageUrl = VietQrHelper.BuildImageUrl(
                        bankAccount.BankCode,
                        bankAccount.AccountNumber,
                        bankAccount.AccountName,
                        activePayment.Amount,
                        activePayment.ReferenceCode);
                }
            }

            var dto = new InvoiceDetailDto
            {
                Id = invoice.Id,
                InvoiceCode = invoice.InvoiceCode,
                RoomName = invoice.RoomNameSnapshot,
                TenantName = invoice.TenantNameSnapshot,
                Month = invoice.Month,
                Year = invoice.Year,
                PeriodStart = invoice.PeriodStartSnapshot,
                PeriodEnd = invoice.PeriodEndSnapshot,
                OccupancyDays = invoice.OccupancyDaysSnapshot,
                InvoiceDate = invoice.InvoiceDate,
                DueDate = invoice.DueDate,
                Items = items.Select(i => new InvoiceItemDto
                {
                    ItemType = i.ItemType,
                    Description = i.Description,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Amount = i.Amount,
                    SortOrder = i.SortOrder
                }).ToList(),
                TotalAmount = invoice.TotalAmount,
                PaidAmount = invoice.PaidAmount,
                BalanceDue = balanceDue,
                Status = invoice.Status,
                DisplayStatus = GetDisplayStatus(invoice.Status),
                IsOverdue = isOverdue,
                DaysLate = daysLate,
                ActivePayment = activePayment == null ? null : MapPayment(activePayment),
                LastRejectedPayment = lastRejected == null ? null : MapPayment(lastRejected),
                QrImageUrl = qrImageUrl,
                Actions = BuildActions(invoice, activePayment, viewer)
            };

            if (viewer == InvoiceDetailViewer.Admin)
            {
                dto.ContractId = invoice.ContractId;
                dto.UtilityReadingId = invoice.UtilityReadingId;
                dto.PaymentHistory = payments.Select(MapPayment).ToList();
            }

            return dto;
        }

        private static InvoiceDetailActionsDto BuildActions(Invoice invoice, Payment? activePayment, InvoiceDetailViewer viewer)
        {
            var actions = new InvoiceDetailActionsDto();

            switch (invoice.Status)
            {
                case InvoiceStatus.Unpaid:
                    actions.CanPayQr = true;
                    actions.CanPayCash = true;
                    if (viewer == InvoiceDetailViewer.Admin)
                        actions.CanCancelInvoice = true;
                    break;
                case InvoiceStatus.WaitingTransfer:
                    actions.CanViewQr = true;
                    actions.CanUploadEvidence = activePayment?.Status == PaymentStatus.PendingTransfer;
                    actions.CanCancelQr = activePayment?.Status == PaymentStatus.PendingTransfer;
                    break;
                case InvoiceStatus.WaitingApproval:
                    actions.CanViewEvidence = activePayment != null;
                    if (viewer == InvoiceDetailViewer.Admin)
                    {
                        actions.CanApprove = true;
                        actions.CanReject = true;
                    }
                    break;
            }

            return actions;
        }

        private static string GetDisplayStatus(InvoiceStatus status) => status switch
        {
            InvoiceStatus.Unpaid => "Chưa thanh toán",
            InvoiceStatus.WaitingTransfer => "Chờ thanh toán (QR)",
            InvoiceStatus.WaitingApproval => "Đang chờ xác nhận",
            InvoiceStatus.Paid => "Đã thanh toán",
            InvoiceStatus.Cancelled => "Đã hủy",
            _ => "Không rõ"
        };

        private static PaymentDto MapPayment(Payment payment) => new()
        {
            Id = payment.Id,
            InvoiceId = payment.InvoiceId,
            Method = payment.Method,
            Status = payment.Status,
            Amount = payment.Amount,
            ReferenceCode = payment.ReferenceCode,
            EvidenceUrl = payment.EvidenceUrl,
            RejectedReason = payment.RejectedReason,
            CancelledReason = payment.CancelledReason,
            CancelledAt = payment.CancelledAt,
            Note = payment.Note,
            PaymentDate = payment.PaymentDate,
            CreatedAt = payment.CreatedAt,
            CreatorUser = payment.CreatorUser
        };

        private static InvoiceDto MapToDto(Invoice invoice)
        {
            var balanceDue = Math.Max(0, invoice.TotalAmount - invoice.PaidAmount);
            return new InvoiceDto
            {
                Id = invoice.Id,
                ContractId = invoice.ContractId,
                UtilityReadingId = invoice.UtilityReadingId,
                InvoiceCode = invoice.InvoiceCode,
                Month = invoice.Month,
                Year = invoice.Year,
                InvoiceDate = invoice.InvoiceDate,
                DueDate = invoice.DueDate,
                TotalAmount = invoice.TotalAmount,
                PaidAmount = invoice.PaidAmount,
                BalanceDue = balanceDue,
                IsOverdue = VnDateHelper.IsOverdue(invoice.Status, balanceDue, invoice.DueDate),
                DaysLate = VnDateHelper.CalcDaysLate(invoice.DueDate),
                Status = invoice.Status
            };
        }

        private async Task<bool> HasActivePaymentAsync(long invoiceId)
        {
            return await _paymentRepository.Query()
                .AsNoTracking()
                .AnyAsync(x => x.InvoiceId == invoiceId
                    && (x.Status == PaymentStatus.PendingTransfer || x.Status == PaymentStatus.WaitingForApproval));
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

        private async Task CancelInvoiceInternalAsync(Invoice invoice)
        {
            var now = DateTime.UtcNow;
            invoice.Status = InvoiceStatus.Cancelled;
            invoice.CancelledAt = now;
            invoice.UpdatedAt = now;
            invoice.LastUpdateUser = _currentUser.UserName ?? "system";
            await _invoiceRepository.UpdateAsync(invoice);
        }

        private async Task<string> ResolveRoomNameAsync(long roomRentalId)
        {
            var room = await _roomRentalRepository.GetRoomRetalById(roomRentalId);
            return room?.RoomNumber.ToString() ?? string.Empty;
        }

        private async Task<string> ResolveTenantNameAsync(Contract contract)
        {
            var tenantIds = GetTenantIds(contract);
            var users = await _userRepository.GetAllQueryAsync();
            var names = await users
                .Where(u => tenantIds.Contains(u.Id))
                .Select(u => u.Name)
                .ToListAsync();
            return string.Join(", ", names);
        }

        private static IEnumerable<long> GetTenantIds(Contract contract)
        {
            if (contract.TenantIds is { Length: > 0 })
                return contract.TenantIds.Where(x => x > 0).Distinct();
            return contract.TenantId > 0 ? new[] { contract.TenantId } : Array.Empty<long>();
        }
    }
}
