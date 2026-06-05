using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoomRentalManagerServer.Application.Interfaces;
using RoomRentalManagerServer.Application.Model.PaymentSubmissionsModel.Dto;
using RoomRentalManagerServer.Application.Model.PaymentsModel.Dto;
using RoomRentalManagerServer.Domain.Interfaces;
using RoomRentalManagerServer.Domain.Interfaces.BankAccountInterfaces;
using RoomRentalManagerServer.Domain.Interfaces.ContractInterfaces;
using RoomRentalManagerServer.Domain.Interfaces.InvoiceInterfaces;
using RoomRentalManagerServer.Domain.Interfaces.PaymentInterfaces;
using RoomRentalManagerServer.Domain.Interfaces.PaymentSubmissionInterfaces;
using RoomRentalManagerServer.Domain.ModelEntities.Contracts;
using RoomRentalManagerServer.Domain.ModelEntities.Invoices;
using RoomRentalManagerServer.Domain.ModelEntities.PaymentAmount;
using RoomRentalManagerServer.Domain.ModelEntities.PaymentSubmissions;

namespace RoomRentalManagerServer.Application.Services
{
    public class PaymentFlowAppService : IPaymentFlowAppService
    {
        private const int MAX_EVIDENCE_SIZE_BYTES = 5 * 1024 * 1024;
        private static readonly HashSet<string> ALLOWED_EVIDENCE_CONTENT_TYPES = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

        private readonly ILogger<PaymentFlowAppService> _logger;
        private readonly ICurrentUserAppService _currentUser;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IContractRepository _contractRepository;
        private readonly IPaymentSubmissionRepository _submissionRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IBankAccountRepository _bankAccountRepository;
        private readonly ICloudinaryService _cloudinaryService;

        public PaymentFlowAppService(
            ILogger<PaymentFlowAppService> logger,
            ICurrentUserAppService currentUser,
            IInvoiceRepository invoiceRepository,
            IContractRepository contractRepository,
            IPaymentSubmissionRepository submissionRepository,
            IPaymentRepository paymentRepository,
            IBankAccountRepository bankAccountRepository,
            ICloudinaryService cloudinaryService)
        {
            _logger = logger;
            _currentUser = currentUser;
            _invoiceRepository = invoiceRepository;
            _contractRepository = contractRepository;
            _submissionRepository = submissionRepository;
            _paymentRepository = paymentRepository;
            _bankAccountRepository = bankAccountRepository;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<PaymentIntentDto> CreatePaymentIntentAsync(long invoiceId)
        {
            if (!_currentUser.IsAuthenticated || _currentUser.GetUserId is null)
                throw new UnauthorizedAccessException("User is not authenticated.");

            var invoice = await _invoiceRepository.GetByIdAsync(invoiceId, asNoTracking: true)
                ?? throw new KeyNotFoundException("Invoice not found.");

            // Tenant ownership check via active contract
            await EnsureTenantCanAccessInvoiceAsync(invoice);

            if (invoice.Status != InvoiceStatus.Issued)
                throw new InvalidOperationException("Invoice is not in Issued status.");

            var amount = Math.Max(0, invoice.TotalAmount - invoice.AmountPaid);
            if (amount <= 0)
                throw new InvalidOperationException("Invoice is already paid.");

            var bankAccount = await _bankAccountRepository.Query()
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.UpdatedAt)
                .FirstOrDefaultAsync()
                ?? throw new InvalidOperationException("No active bank account configured.");

            var transferContent = $"INV-{invoice.Id}";
            var qrUrl = BuildVietQrImageUrl(bankAccount.BankCode, bankAccount.AccountNumber, bankAccount.AccountName, amount, transferContent);

            return new PaymentIntentDto
            {
                InvoiceId = invoice.Id,
                Amount = amount,
                TransferContent = transferContent,
                BankCode = bankAccount.BankCode,
                AccountNumber = bankAccount.AccountNumber,
                AccountName = bankAccount.AccountName,
                QrImageUrl = qrUrl
            };
        }

        public async Task<PaymentSubmissionDto> SubmitTransferEvidenceAsync(long invoiceId, IFormFile evidenceFile)
        {
            if (!_currentUser.IsAuthenticated || _currentUser.GetUserId is null)
                throw new UnauthorizedAccessException("User is not authenticated.");

            if (evidenceFile == null)
                throw new ArgumentNullException(nameof(evidenceFile));

            ValidateEvidenceFile(evidenceFile);

            var invoice = await _invoiceRepository.GetByIdAsync(invoiceId, asNoTracking: true)
                ?? throw new KeyNotFoundException("Invoice not found.");

            await EnsureTenantCanAccessInvoiceAsync(invoice);

            if (invoice.Status != InvoiceStatus.Issued)
                throw new InvalidOperationException("Invoice is not in Issued status.");

            var balanceDue = Math.Max(0, invoice.TotalAmount - invoice.AmountPaid);
            if (balanceDue <= 0)
                throw new InvalidOperationException("Invoice is already paid.");

            var hasPending = await _submissionRepository.Query()
                .AsNoTracking()
                .AnyAsync(x => x.InvoiceId == invoice.Id && x.Status == PaymentSubmissionStatus.Pending);
            if (hasPending)
                throw new InvalidOperationException("There is already a pending submission for this invoice.");

            var (url, publicId) = await _cloudinaryService.UploadImageAsync(evidenceFile, "invoice-payments");
            if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(publicId))
                throw new InvalidOperationException("Failed to upload evidence image.");

            var now = DateTime.UtcNow;
            var submission = new PaymentSubmission
            {
                InvoiceId = invoice.Id,
                DeclaredAmount = balanceDue,
                EvidenceUrl = url,
                EvidencePublicId = publicId,
                Status = PaymentSubmissionStatus.Pending,
                RejectedReason = null,
                CreatedAt = now,
                UpdatedAt = now,
                CreatorUser = _currentUser.UserName,
                LastUpdateUser = _currentUser.UserName
            };

            await _submissionRepository.AddAsync(submission);
            return MapSubmission(submission);
        }

        public async Task<List<PaymentSubmissionDto>> GetPendingSubmissionsAsync()
        {
            if (!_currentUser.IsAuthenticated)
                throw new UnauthorizedAccessException("User is not authenticated.");

            var items = await _submissionRepository.Query()
                .AsNoTracking()
                .Where(x => x.Status == PaymentSubmissionStatus.Pending)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();

            return items.Select(MapSubmission).ToList();
        }

        public async Task<PaymentSubmissionDto> ApproveSubmissionAsync(long submissionId)
        {
            if (!_currentUser.IsAuthenticated)
                throw new UnauthorizedAccessException("User is not authenticated.");

            var submission = await _submissionRepository.GetByIdAsync(submissionId, asNoTracking: false)
                ?? throw new KeyNotFoundException("Submission not found.");

            if (submission.Status != PaymentSubmissionStatus.Pending)
                throw new InvalidOperationException("Only Pending submissions can be approved.");

            var invoice = await _invoiceRepository.GetByIdAsync(submission.InvoiceId, asNoTracking: false)
                ?? throw new KeyNotFoundException("Invoice not found.");

            if (invoice.Status != InvoiceStatus.Issued)
                throw new InvalidOperationException("Invoice is not in Issued status.");

            var balanceDue = Math.Max(0, invoice.TotalAmount - invoice.AmountPaid);
            if (balanceDue <= 0)
                throw new InvalidOperationException("Invoice is already paid.");

            // v1 no-partial: declared amount must match full remaining balance
            if (submission.DeclaredAmount != balanceDue)
                throw new InvalidOperationException("Declared amount does not match invoice balance due.");

            var now = DateTime.UtcNow;

            // Create ledger payment
            var payment = new Payment
            {
                InvoiceId = invoice.Id,
                AmountPaid = submission.DeclaredAmount,
                PaymentDate = now,
                PaymentMethod = PaymentMethod.BankTransfer,
                Note = null,
                CreatedAt = now,
                UpdatedAt = now,
                CreatorUser = _currentUser.UserName,
                LastUpdateUser = _currentUser.UserName
            };
            await _paymentRepository.AddAsync(payment);

            // Update invoice
            invoice.AmountPaid += submission.DeclaredAmount;
            invoice.Status = InvoiceStatus.Paid;
            invoice.UpdatedAt = now;
            invoice.LastUpdateUser = _currentUser.UserName;
            await _invoiceRepository.UpdateAsync(invoice);

            submission.Status = PaymentSubmissionStatus.Approved;
            submission.UpdatedAt = now;
            submission.LastUpdateUser = _currentUser.UserName;
            await _submissionRepository.UpdateAsync(submission);

            return MapSubmission(submission);
        }

        public async Task<PaymentSubmissionDto> RejectSubmissionAsync(long submissionId, RejectPaymentSubmissionDto input)
        {
            if (!_currentUser.IsAuthenticated)
                throw new UnauthorizedAccessException("User is not authenticated.");

            var submission = await _submissionRepository.GetByIdAsync(submissionId, asNoTracking: false)
                ?? throw new KeyNotFoundException("Submission not found.");

            if (submission.Status != PaymentSubmissionStatus.Pending)
                throw new InvalidOperationException("Only Pending submissions can be rejected.");

            var now = DateTime.UtcNow;
            submission.Status = PaymentSubmissionStatus.Rejected;
            submission.RejectedReason = input?.Reason;
            submission.UpdatedAt = now;
            submission.LastUpdateUser = _currentUser.UserName;
            await _submissionRepository.UpdateAsync(submission);
            return MapSubmission(submission);
        }

        public async Task<PaymentSubmissionDto> MarkCashPaidAsync(long invoiceId, string? note)
        {
            if (!_currentUser.IsAuthenticated)
                throw new UnauthorizedAccessException("User is not authenticated.");

            var invoice = await _invoiceRepository.GetByIdAsync(invoiceId, asNoTracking: false)
                ?? throw new KeyNotFoundException("Invoice not found.");

            if (invoice.Status != InvoiceStatus.Issued)
                throw new InvalidOperationException("Invoice is not in Issued status.");

            var balanceDue = Math.Max(0, invoice.TotalAmount - invoice.AmountPaid);
            if (balanceDue <= 0)
                throw new InvalidOperationException("Invoice is already paid.");

            var now = DateTime.UtcNow;
            var payment = new Payment
            {
                InvoiceId = invoice.Id,
                AmountPaid = balanceDue,
                PaymentDate = now,
                PaymentMethod = PaymentMethod.Cash,
                Note = note,
                CreatedAt = now,
                UpdatedAt = now,
                CreatorUser = _currentUser.UserName,
                LastUpdateUser = _currentUser.UserName
            };
            await _paymentRepository.AddAsync(payment);

            invoice.AmountPaid += balanceDue;
            invoice.Status = InvoiceStatus.Paid;
            invoice.UpdatedAt = now;
            invoice.LastUpdateUser = _currentUser.UserName;
            await _invoiceRepository.UpdateAsync(invoice);

            // Represent cash payment as an Approved submission-like dto for UI convenience (without evidence)
            var pseudo = new PaymentSubmissionDto
            {
                Id = 0,
                InvoiceId = invoice.Id,
                DeclaredAmount = balanceDue,
                EvidenceUrl = string.Empty,
                Status = PaymentSubmissionStatus.Approved,
                RejectedReason = null,
                CreatedAt = now,
                CreatorUser = _currentUser.UserName
            };
            return pseudo;
        }

        private static PaymentSubmissionDto MapSubmission(PaymentSubmission s) => new()
        {
            Id = s.Id,
            InvoiceId = s.InvoiceId,
            DeclaredAmount = s.DeclaredAmount,
            EvidenceUrl = s.EvidenceUrl,
            Status = s.Status,
            RejectedReason = s.RejectedReason,
            CreatedAt = s.CreatedAt,
            CreatorUser = s.CreatorUser
        };

        private async Task EnsureTenantCanAccessInvoiceAsync(Invoice invoice)
        {
            var userId = _currentUser.GetUserId;
            if (userId is null)
                throw new UnauthorizedAccessException("User is not authenticated.");

            var contract = await _contractRepository.GetByIdAsync(invoice.ContractId, asNoTracking: true);
            if (contract == null)
                throw new InvalidOperationException("Invoice contract not found.");

            if (contract.StatusContract != StatusContract.Active)
                throw new UnauthorizedAccessException("Contract is not active.");

            if (contract.TenantId != userId.Value)
                throw new UnauthorizedAccessException("You do not have access to this invoice.");
        }

        private static void ValidateEvidenceFile(IFormFile file)
        {
            if (file.Length <= 0)
                throw new InvalidOperationException("Evidence file is empty.");
            if (file.Length > MAX_EVIDENCE_SIZE_BYTES)
                throw new InvalidOperationException("Evidence file is too large.");
            if (!ALLOWED_EVIDENCE_CONTENT_TYPES.Contains(file.ContentType))
                throw new InvalidOperationException("Unsupported evidence file type.");
        }

        private static string BuildVietQrImageUrl(string bankCode, string accountNumber, string accountName, decimal amount, string addInfo)
        {
            // Free public rendering service. This is NOT a paid API; it returns an image based on query params.
            // Template "compact2" is a common choice; can be changed by client later.
            var encodedAddInfo = Uri.EscapeDataString(addInfo);
            var encodedName = Uri.EscapeDataString(accountName);
            return $"https://img.vietqr.io/image/{bankCode}-{accountNumber}-compact2.png?amount={amount}&addInfo={encodedAddInfo}&accountName={encodedName}";
        }
    }
}

