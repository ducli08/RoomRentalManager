using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoomRentalManagerServer.Application.Common;
using RoomRentalManagerServer.Application.Interfaces;
using RoomRentalManagerServer.Application.Model.PaymentsModel.Dto;
using RoomRentalManagerServer.Domain.Interfaces;
using RoomRentalManagerServer.Domain.Interfaces.BankAccountInterfaces;
using RoomRentalManagerServer.Domain.Interfaces.ContractInterfaces;
using RoomRentalManagerServer.Domain.Interfaces.InvoiceInterfaces;
using RoomRentalManagerServer.Domain.Interfaces.PaymentInterfaces;
using RoomRentalManagerServer.Domain.ModelEntities.Contracts;
using RoomRentalManagerServer.Domain.ModelEntities.Invoices;
using RoomRentalManagerServer.Domain.ModelEntities.PaymentAmount;

namespace RoomRentalManagerServer.Application.Services
{
    public class PaymentAppService : IPaymentAppService
    {
        private const int MaxEvidenceSizeBytes = 5 * 1024 * 1024;
        private static readonly HashSet<string> AllowedEvidenceContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

        private readonly ILogger<PaymentAppService> _logger;
        private readonly ICurrentUserAppService _currentUser;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IContractRepository _contractRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IBankAccountRepository _bankAccountRepository;
        private readonly ICloudinaryService _cloudinaryService;

        public PaymentAppService(
            ILogger<PaymentAppService> logger,
            ICurrentUserAppService currentUser,
            IInvoiceRepository invoiceRepository,
            IContractRepository contractRepository,
            IPaymentRepository paymentRepository,
            IBankAccountRepository bankAccountRepository,
            ICloudinaryService cloudinaryService)
        {
            _logger = logger;
            _currentUser = currentUser;
            _invoiceRepository = invoiceRepository;
            _contractRepository = contractRepository;
            _paymentRepository = paymentRepository;
            _bankAccountRepository = bankAccountRepository;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<CreateQrPaymentResultDto> CreateQrPaymentAsync(long invoiceId, bool isAdmin)
        {
            EnsureAuthenticated();

            var invoice = await _invoiceRepository.GetByIdAsync(invoiceId, asNoTracking: false)
                ?? throw new KeyNotFoundException("Invoice not found.");

            if (!isAdmin)
                await EnsureTenantCanAccessInvoiceAsync(invoice);

            await EnsureCanStartPaymentAsync(invoice);

            var bankAccount = await _bankAccountRepository.Query()
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.UpdatedAt)
                .FirstOrDefaultAsync()
                ?? throw new InvalidOperationException("No active bank account configured.");

            var now = DateTime.UtcNow;
            var payment = new Payment
            {
                InvoiceId = invoice.Id,
                Method = PaymentMethod.BankTransfer,
                Status = PaymentStatus.PendingTransfer,
                Amount = invoice.TotalAmount,
                ReferenceCode = string.Empty,
                CreatedAt = now,
                UpdatedAt = now,
                CreatorUser = _currentUser.UserName ?? "system",
                LastUpdateUser = _currentUser.UserName ?? "system"
            };

            await _paymentRepository.AddAsync(payment);

            payment.ReferenceCode = PaymentReferenceCodeHelper.Build(invoice.InvoiceCode, payment.Id);
            payment.UpdatedAt = DateTime.UtcNow;
            await _paymentRepository.UpdateAsync(payment);

            invoice.Status = InvoiceStatus.WaitingTransfer;
            invoice.UpdatedAt = now;
            invoice.LastUpdateUser = _currentUser.UserName ?? "system";
            await _invoiceRepository.UpdateAsync(invoice);

            var qrUrl = VietQrHelper.BuildImageUrl(
                bankAccount.BankCode,
                bankAccount.AccountNumber,
                bankAccount.AccountName,
                payment.Amount,
                payment.ReferenceCode);

            return new CreateQrPaymentResultDto
            {
                Payment = MapPayment(payment),
                QrImageUrl = qrUrl,
                BankCode = bankAccount.BankCode,
                AccountNumber = bankAccount.AccountNumber,
                AccountName = bankAccount.AccountName
            };
        }

        public async Task<PaymentDto> CreateCashPaymentAsync(long invoiceId, string? note, bool isAdmin)
        {
            EnsureAuthenticated();

            var invoice = await _invoiceRepository.GetByIdAsync(invoiceId, asNoTracking: false)
                ?? throw new KeyNotFoundException("Invoice not found.");

            if (!isAdmin)
                await EnsureTenantCanAccessInvoiceAsync(invoice);

            await EnsureCanStartPaymentAsync(invoice);

            var now = DateTime.UtcNow;
            var payment = new Payment
            {
                InvoiceId = invoice.Id,
                Method = PaymentMethod.Cash,
                Status = isAdmin ? PaymentStatus.Approved : PaymentStatus.WaitingForApproval,
                Amount = invoice.TotalAmount,
                ReferenceCode = string.Empty,
                Note = note,
                CreatedAt = now,
                UpdatedAt = now,
                CreatorUser = _currentUser.UserName ?? "system",
                LastUpdateUser = _currentUser.UserName ?? "system"
            };

            await _paymentRepository.AddAsync(payment);

            payment.ReferenceCode = PaymentReferenceCodeHelper.Build(invoice.InvoiceCode, payment.Id);

            if (isAdmin)
            {
                payment.PaymentDate = now;
                payment.UpdatedAt = now;
                await _paymentRepository.UpdateAsync(payment);

                invoice.PaidAmount = invoice.TotalAmount;
                invoice.Status = InvoiceStatus.Paid;
                invoice.UpdatedAt = now;
                invoice.LastUpdateUser = _currentUser.UserName ?? "system";
                await _invoiceRepository.UpdateAsync(invoice);
            }
            else
            {
                payment.UpdatedAt = now;
                await _paymentRepository.UpdateAsync(payment);

                invoice.Status = InvoiceStatus.WaitingApproval;
                invoice.UpdatedAt = now;
                invoice.LastUpdateUser = _currentUser.UserName ?? "system";
                await _invoiceRepository.UpdateAsync(invoice);
            }

            return MapPayment(payment);
        }

        public async Task<PaymentDto> UploadEvidenceAsync(long invoiceId, long paymentId, IFormFile evidenceFile, string? note, bool isAdmin)
        {
            EnsureAuthenticated();

            if (evidenceFile == null)
                throw new ArgumentNullException(nameof(evidenceFile));

            ValidateEvidenceFile(evidenceFile);

            var payment = await _paymentRepository.GetByIdAsync(paymentId, asNoTracking: false)
                ?? throw new KeyNotFoundException("Payment not found.");

            if (payment.InvoiceId != invoiceId)
                throw new InvalidOperationException("Payment does not belong to this invoice.");

            var invoice = await _invoiceRepository.GetByIdAsync(invoiceId, asNoTracking: false)
                ?? throw new KeyNotFoundException("Invoice not found.");

            if (!isAdmin)
                await EnsureTenantCanAccessInvoiceAsync(invoice);

            if (payment.Status is not (PaymentStatus.PendingTransfer or PaymentStatus.WaitingForApproval))
                throw new InvalidOperationException("Payment is not awaiting evidence upload.");

            if (!string.IsNullOrWhiteSpace(payment.EvidencePublicId))
            {
                try
                {
                    await _cloudinaryService.DeleteImageAsync(payment.EvidencePublicId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete old evidence image {PublicId}", payment.EvidencePublicId);
                }
            }

            var (url, publicId) = await _cloudinaryService.UploadImageAsync(evidenceFile, "invoice-payments");
            if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(publicId))
                throw new InvalidOperationException("Failed to upload evidence image.");

            var now = DateTime.UtcNow;
            payment.EvidenceUrl = url;
            payment.EvidencePublicId = publicId;
            payment.Note = note ?? payment.Note;
            payment.Status = PaymentStatus.WaitingForApproval;
            payment.UpdatedAt = now;
            payment.LastUpdateUser = _currentUser.UserName ?? "system";
            await _paymentRepository.UpdateAsync(payment);

            invoice.Status = InvoiceStatus.WaitingApproval;
            invoice.UpdatedAt = now;
            invoice.LastUpdateUser = _currentUser.UserName ?? "system";
            await _invoiceRepository.UpdateAsync(invoice);

            return MapPayment(payment);
        }

        public async Task<PaymentDto> CancelQrPaymentAsync(long invoiceId, long paymentId, string? reason, bool isAdmin)
        {
            EnsureAuthenticated();

            var payment = await _paymentRepository.GetByIdAsync(paymentId, asNoTracking: false)
                ?? throw new KeyNotFoundException("Payment not found.");

            if (payment.InvoiceId != invoiceId)
                throw new InvalidOperationException("Payment does not belong to this invoice.");

            if (payment.Status != PaymentStatus.PendingTransfer)
                throw new InvalidOperationException("Only pending transfer payments can be cancelled.");

            var invoice = await _invoiceRepository.GetByIdAsync(invoiceId, asNoTracking: false)
                ?? throw new KeyNotFoundException("Invoice not found.");

            if (!isAdmin)
                await EnsureTenantCanAccessInvoiceAsync(invoice);

            var now = DateTime.UtcNow;
            payment.Status = PaymentStatus.Cancelled;
            payment.CancelledReason = reason ?? "User cancelled";
            payment.CancelledAt = now;
            payment.UpdatedAt = now;
            payment.LastUpdateUser = _currentUser.UserName ?? "system";
            await _paymentRepository.UpdateAsync(payment);

            invoice.Status = InvoiceStatus.Unpaid;
            invoice.UpdatedAt = now;
            invoice.LastUpdateUser = _currentUser.UserName ?? "system";
            await _invoiceRepository.UpdateAsync(invoice);

            return MapPayment(payment);
        }

        public async Task<PaymentDto> ApprovePaymentAsync(long paymentId)
        {
            EnsureAuthenticated();

            var payment = await _paymentRepository.GetByIdAsync(paymentId, asNoTracking: false)
                ?? throw new KeyNotFoundException("Payment not found.");

            if (payment.Status != PaymentStatus.WaitingForApproval)
                throw new InvalidOperationException("Only payments waiting for approval can be approved.");

            var invoice = await _invoiceRepository.GetByIdAsync(payment.InvoiceId, asNoTracking: false)
                ?? throw new KeyNotFoundException("Invoice not found.");

            if (payment.Amount != invoice.TotalAmount)
                throw new InvalidOperationException("Payment amount does not match invoice total.");

            if (invoice.PaidAmount > 0)
                throw new InvalidOperationException("Invoice already has payments recorded.");

            var now = DateTime.UtcNow;
            payment.Status = PaymentStatus.Approved;
            payment.PaymentDate = now;
            payment.UpdatedAt = now;
            payment.LastUpdateUser = _currentUser.UserName ?? "system";
            await _paymentRepository.UpdateAsync(payment);

            invoice.PaidAmount = payment.Amount;
            invoice.Status = InvoiceStatus.Paid;
            invoice.UpdatedAt = now;
            invoice.LastUpdateUser = _currentUser.UserName ?? "system";
            await _invoiceRepository.UpdateAsync(invoice);

            return MapPayment(payment);
        }

        public async Task<PaymentDto> RejectPaymentAsync(long paymentId, RejectPaymentDto input)
        {
            EnsureAuthenticated();

            var payment = await _paymentRepository.GetByIdAsync(paymentId, asNoTracking: false)
                ?? throw new KeyNotFoundException("Payment not found.");

            if (payment.Status != PaymentStatus.WaitingForApproval)
                throw new InvalidOperationException("Only payments waiting for approval can be rejected.");

            var invoice = await _invoiceRepository.GetByIdAsync(payment.InvoiceId, asNoTracking: false)
                ?? throw new KeyNotFoundException("Invoice not found.");

            var now = DateTime.UtcNow;
            payment.Status = PaymentStatus.Rejected;
            payment.RejectedReason = input?.Reason;
            payment.UpdatedAt = now;
            payment.LastUpdateUser = _currentUser.UserName ?? "system";
            await _paymentRepository.UpdateAsync(payment);

            invoice.Status = InvoiceStatus.Unpaid;
            invoice.UpdatedAt = now;
            invoice.LastUpdateUser = _currentUser.UserName ?? "system";
            await _invoiceRepository.UpdateAsync(invoice);

            return MapPayment(payment);
        }

        public async Task<List<PaymentDto>> GetPendingPaymentsAsync()
        {
            EnsureAuthenticated();

            var items = await _paymentRepository.Query()
                .AsNoTracking()
                .Where(x => x.Status == PaymentStatus.WaitingForApproval)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();

            return items.Select(MapPayment).ToList();
        }

        private async Task EnsureCanStartPaymentAsync(Invoice invoice)
        {
            if (invoice.Status != InvoiceStatus.Unpaid)
                throw new InvalidOperationException("Invoice is not in Unpaid status.");

            if (invoice.PaidAmount > 0)
                throw new InvalidOperationException("Invoice is already paid.");

            var hasActive = await _paymentRepository.Query()
                .AsNoTracking()
                .AnyAsync(x => x.InvoiceId == invoice.Id
                    && (x.Status == PaymentStatus.PendingTransfer || x.Status == PaymentStatus.WaitingForApproval));

            if (hasActive)
                throw new InvalidOperationException("There is already an active payment for this invoice.");
        }

        private void EnsureAuthenticated()
        {
            if (!_currentUser.IsAuthenticated)
                throw new UnauthorizedAccessException("User is not authenticated.");
        }

        private async Task EnsureTenantCanAccessInvoiceAsync(Invoice invoice)
        {
            var userId = _currentUser.GetUserId
                ?? throw new UnauthorizedAccessException("User is not authenticated.");

            var contract = await _contractRepository.GetByIdAsync(invoice.ContractId, asNoTracking: true)
                ?? throw new InvalidOperationException("Invoice contract not found.");

            if (contract.StatusContract != StatusContract.Active)
                throw new UnauthorizedAccessException("Contract is not active.");

            if (contract.TenantId != userId && !contract.TenantIds.Contains(userId))
                throw new UnauthorizedAccessException("You do not have access to this invoice.");
        }

        private static void ValidateEvidenceFile(IFormFile file)
        {
            if (file.Length <= 0)
                throw new InvalidOperationException("Evidence file is empty.");
            if (file.Length > MaxEvidenceSizeBytes)
                throw new InvalidOperationException("Evidence file is too large.");
            if (!AllowedEvidenceContentTypes.Contains(file.ContentType))
                throw new InvalidOperationException("Unsupported evidence file type.");
        }

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
    }
}
