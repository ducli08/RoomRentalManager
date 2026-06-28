using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using RoomRentalManagerServer.Application.Interfaces;
using RoomRentalManagerServer.Application.Model.PaymentsModel.Dto;
using RoomRentalManagerServer.Application.Services;
using RoomRentalManagerServer.Domain.Interfaces;
using RoomRentalManagerServer.Domain.ModelEntities.BankAccounts;
using RoomRentalManagerServer.Domain.ModelEntities.Contracts;
using RoomRentalManagerServer.Domain.ModelEntities.Invoices;
using RoomRentalManagerServer.Domain.ModelEntities.PaymentAmount;
using RoomRentalManagerServer.Infrastructure.Data;
using RoomRentalManagerServer.Infrastructure.Repositories.BankAccountRepositories;
using RoomRentalManagerServer.Infrastructure.Repositories.ContractRepositories;
using RoomRentalManagerServer.Infrastructure.Repositories.InvoiceRepositories;
using RoomRentalManagerServer.Infrastructure.Repositories.PaymentRepositories;
using RoomRentalManagerServer.Infrastructure.Repositories.RoomRentalRepositories;
using RoomRentalManagerServer.Infrastructure.Repositories.UserRepository;

namespace RoomRentalManagerServer.Tests;

public class InvoicePaymentFlowTests
{
    [Fact]
    public async Task QrFlow_TenantUpload_AdminApprove_MarksInvoicePaid()
    {
        var (db, _, paymentTenant, paymentAdmin) = CreateServices();
        await SeedBankAccountAsync(db);
        var invoice = await SeedUnpaidInvoiceAsync(db, totalAmount: 1_000_000m);

        var qr = await paymentTenant.CreateQrPaymentAsync(invoice.Id, isAdmin: false);
        Assert.Contains("vietqr.io", qr.QrImageUrl);

        invoice = await db.Invoices.AsNoTracking().FirstAsync(x => x.Id == invoice.Id);
        Assert.Equal(InvoiceStatus.WaitingTransfer, invoice.Status);

        var evidence = FakeFormFile.CreatePng("evidence.png");
        var payment = await paymentTenant.UploadEvidenceAsync(invoice.Id, qr.Payment.Id, evidence, note: null, isAdmin: false);
        Assert.Equal(PaymentStatus.WaitingForApproval, payment.Status);

        invoice = await db.Invoices.AsNoTracking().FirstAsync(x => x.Id == invoice.Id);
        Assert.Equal(InvoiceStatus.WaitingApproval, invoice.Status);

        var approved = await paymentAdmin.ApprovePaymentAsync(payment.Id);
        Assert.Equal(PaymentStatus.Approved, approved.Status);

        invoice = await db.Invoices.AsNoTracking().FirstAsync(x => x.Id == invoice.Id);
        Assert.Equal(InvoiceStatus.Paid, invoice.Status);
        Assert.Equal(1_000_000m, invoice.PaidAmount);
    }

    [Fact]
    public async Task CashFlow_Tenant_AdminApprove_MarksInvoicePaid()
    {
        var (db, _, paymentTenant, paymentAdmin) = CreateServices();
        var invoice = await SeedUnpaidInvoiceAsync(db, totalAmount: 500_000m);

        var payment = await paymentTenant.CreateCashPaymentAsync(invoice.Id, note: "cash", isAdmin: false);
        Assert.Equal(PaymentStatus.WaitingForApproval, payment.Status);

        await paymentAdmin.ApprovePaymentAsync(payment.Id);

        invoice = await db.Invoices.AsNoTracking().FirstAsync(x => x.Id == invoice.Id);
        Assert.Equal(InvoiceStatus.Paid, invoice.Status);
    }

    [Fact]
    public async Task AdminCash_AutoApprove_MarksInvoicePaid()
    {
        var (db, _, _, paymentAdmin) = CreateServices();
        var invoice = await SeedUnpaidInvoiceAsync(db, totalAmount: 300_000m);

        var payment = await paymentAdmin.CreateCashPaymentAsync(invoice.Id, note: null, isAdmin: true);
        Assert.Equal(PaymentStatus.Approved, payment.Status);

        invoice = await db.Invoices.AsNoTracking().FirstAsync(x => x.Id == invoice.Id);
        Assert.Equal(InvoiceStatus.Paid, invoice.Status);
    }

    [Fact]
    public async Task Reject_AllowsNewPayment()
    {
        var (db, _, paymentTenant, paymentAdmin) = CreateServices();
        await SeedBankAccountAsync(db);
        var invoice = await SeedUnpaidInvoiceAsync(db, totalAmount: 100_000m);

        var cash = await paymentTenant.CreateCashPaymentAsync(invoice.Id, null, isAdmin: false);
        await paymentAdmin.RejectPaymentAsync(cash.Id, new RejectPaymentDto { Reason = "Sai" });

        invoice = await db.Invoices.AsNoTracking().FirstAsync(x => x.Id == invoice.Id);
        Assert.Equal(InvoiceStatus.Unpaid, invoice.Status);

        var qr = await paymentTenant.CreateQrPaymentAsync(invoice.Id, isAdmin: false);
        Assert.NotEqual(cash.Id, qr.Payment.Id);
    }

    [Fact]
    public async Task CancelQr_ReturnsUnpaid()
    {
        var (db, invoiceApp, paymentTenant, _) = CreateServices();
        await SeedBankAccountAsync(db);
        var invoice = await SeedUnpaidInvoiceAsync(db, totalAmount: 100_000m);

        var qr = await paymentTenant.CreateQrPaymentAsync(invoice.Id, isAdmin: false);
        await paymentTenant.CancelQrPaymentAsync(invoice.Id, qr.Payment.Id, reason: "changed mind", isAdmin: false);

        invoice = await db.Invoices.AsNoTracking().FirstAsync(x => x.Id == invoice.Id);
        Assert.Equal(InvoiceStatus.Unpaid, invoice.Status);

        var cancelled = await db.Payments.AsNoTracking().FirstAsync(x => x.Id == qr.Payment.Id);
        Assert.Equal(PaymentStatus.Cancelled, cancelled.Status);
    }

    [Fact]
    public async Task CancelInvoice_BlockedWhenActivePayment()
    {
        var (db, invoiceApp, paymentTenant, _) = CreateServices();
        await SeedBankAccountAsync(db);
        var invoice = await SeedUnpaidInvoiceAsync(db, totalAmount: 100_000m);
        await paymentTenant.CreateQrPaymentAsync(invoice.Id, isAdmin: false);

        await Assert.ThrowsAsync<InvalidOperationException>(() => invoiceApp.CancelAsync(invoice.Id));
    }

    [Fact]
    public async Task CancelInvoice_OnlyUnpaid()
    {
        var (db, invoiceApp, _, _) = CreateServices();
        var invoice = await SeedUnpaidInvoiceAsync(db, totalAmount: 100_000m);

        var ok = await invoiceApp.CancelAsync(invoice.Id);
        Assert.True(ok);

        invoice = await db.Invoices.AsNoTracking().FirstAsync(x => x.Id == invoice.Id);
        Assert.Equal(InvoiceStatus.Cancelled, invoice.Status);
    }

    [Fact]
    public async Task TenantCannotAccessInvoiceNotBelongingToContract()
    {
        var (db, _, paymentTenant, _) = CreateServices();
        await SeedBankAccountAsync(db);

        var contract = new Contract
        {
            RoomRentalId = 1,
            TenantId = 999,
            TenantIds = new[] { 999L },
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddMonths(6),
            DepositAmout = 0,
            MonthlyRent = 0,
            StatusContract = StatusContract.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatorUser = "seed",
            UpdaterUser = "seed"
        };
        db.Contracts.Add(contract);
        await db.SaveChangesAsync();

        var invoice = await SeedUnpaidInvoiceAsync(db, contract, totalAmount: 100m);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            paymentTenant.CreateQrPaymentAsync(invoice.Id, isAdmin: false));
    }

    [Fact]
    public async Task SecondaryTenantInTenantIds_CanAccessInvoice()
    {
        var (db, _, paymentTenant, _) = CreateServices();
        await SeedBankAccountAsync(db);

        var contract = new Contract
        {
            RoomRentalId = 1,
            TenantId = 999,
            TenantIds = new[] { 999L, 10L },
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddMonths(6),
            DepositAmout = 0,
            MonthlyRent = 0,
            StatusContract = StatusContract.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatorUser = "seed",
            UpdaterUser = "seed"
        };
        db.Contracts.Add(contract);
        await db.SaveChangesAsync();

        var invoice = await SeedUnpaidInvoiceAsync(db, contract, totalAmount: 100m);
        var qr = await paymentTenant.CreateQrPaymentAsync(invoice.Id, isAdmin: false);
        Assert.Equal(invoice.Id, qr.Payment.InvoiceId);
    }

    private static async Task SeedBankAccountAsync(RoomRentalManagerServerDbContext db)
    {
        db.BankAccounts.Add(new BankAccount
        {
            BankCode = "VCB",
            AccountNumber = "0123456789",
            AccountName = "TEST",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatorUser = "seed",
            LastUpdateUser = "seed"
        });
        await db.SaveChangesAsync();
    }

    private static async Task<Invoice> SeedUnpaidInvoiceAsync(
        RoomRentalManagerServerDbContext db,
        decimal totalAmount = 1_000_000m)
    {
        var contract = await db.Contracts.FirstAsync();
        return await SeedUnpaidInvoiceAsync(db, contract, totalAmount);
    }

    private static async Task<Invoice> SeedUnpaidInvoiceAsync(
        RoomRentalManagerServerDbContext db,
        Contract contract,
        decimal totalAmount)
    {
        var now = DateTime.UtcNow;
        var invoice = new Invoice
        {
            ContractId = contract.Id,
            UtilityReadingId = 1,
            Month = 1,
            Year = 2025,
            InvoiceCode = "INV-202501-001",
            RoomNameSnapshot = "P101",
            TenantNameSnapshot = "Tenant",
            OccupancyDaysSnapshot = 30,
            PeriodStartSnapshot = new DateTime(2025, 1, 1),
            PeriodEndSnapshot = new DateTime(2025, 1, 31),
            InvoiceDate = now,
            DueDate = now.AddDays(5),
            TotalAmount = totalAmount,
            PaidAmount = 0,
            Status = InvoiceStatus.Unpaid,
            CreatedAt = now,
            UpdatedAt = now,
            CreatorUser = "seed",
            LastUpdateUser = "seed"
        };
        db.Invoices.Add(invoice);
        await db.SaveChangesAsync();
        return invoice;
    }

    private static (RoomRentalManagerServerDbContext Db, IInvoiceAppService InvoiceApp, IPaymentAppService PaymentTenant, IPaymentAppService PaymentAdmin) CreateServices()
    {
        var options = new DbContextOptionsBuilder<RoomRentalManagerServerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new RoomRentalManagerServerDbContext(options);

        var invoiceRepo = new InvoiceRepository(db, NullLogger<InvoiceRepository>.Instance);
        var invoiceItemRepo = new InvoiceItemRepository(db, NullLogger<InvoiceItemRepository>.Instance);
        var contractRepo = new ContractRepository(db, NullLogger<ContractRepository>.Instance);
        var paymentRepo = new PaymentRepository(db, NullLogger<PaymentRepository>.Instance);
        var bankRepo = new BankAccountRepository(db, NullLogger<BankAccountRepository>.Instance);
        var roomRepo = new RoomRentalRepository(db, NullLogger<RoomRentalRepository>.Instance);
        var userRepo = new UserRepository(db, NullLogger<UserRepository>.Instance);

        var currentTenant = new FakeCurrentUserAppService(isAuthenticated: true, userId: 10, userName: "tenant");
        var currentAdmin = new FakeCurrentUserAppService(isAuthenticated: true, userId: 1, userName: "admin");
        var cloudinary = new FakeCloudinaryService();

        IInvoiceAppService invoiceApp = new InvoiceAppService(
            NullLogger<InvoiceAppService>.Instance,
            invoiceRepo,
            invoiceItemRepo,
            contractRepo,
            paymentRepo,
            roomRepo,
            userRepo,
            bankRepo,
            currentAdmin);

        IPaymentAppService paymentTenant = new PaymentAppService(
            NullLogger<PaymentAppService>.Instance,
            currentTenant,
            invoiceRepo,
            contractRepo,
            paymentRepo,
            bankRepo,
            cloudinary);

        IPaymentAppService paymentAdmin = new PaymentAppService(
            NullLogger<PaymentAppService>.Instance,
            currentAdmin,
            invoiceRepo,
            contractRepo,
            paymentRepo,
            bankRepo,
            cloudinary);

        var contract = new Contract
        {
            RoomRentalId = 1,
            TenantId = 10,
            TenantIds = new[] { 10L },
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddMonths(6),
            DepositAmout = 0,
            MonthlyRent = 0,
            StatusContract = StatusContract.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatorUser = "seed",
            UpdaterUser = "seed"
        };
        db.Contracts.Add(contract);
        db.SaveChanges();

        return (db, invoiceApp, paymentTenant, paymentAdmin);
    }
}

internal sealed class FakeCurrentUserAppService : ICurrentUserAppService
{
    public FakeCurrentUserAppService(bool isAuthenticated, long? userId, string userName)
    {
        IsAuthenticated = isAuthenticated;
        GetUserId = userId;
        UserName = userName;
    }

    public long? GetUserId { get; }
    public string UserName { get; }
    public bool IsAuthenticated { get; }
}

internal sealed class FakeCloudinaryService : ICloudinaryService
{
    public Task<bool> DeleteImageAsync(string publicId) => Task.FromResult(true);

    public Task<(string Url, string PublicId)> UploadImageAsync(Microsoft.AspNetCore.Http.IFormFile file, string folder)
    {
        return Task.FromResult<(string, string)>(($"https://cdn.test/{folder}/{file.FileName}", $"public/{folder}/{file.FileName}"));
    }

    public Task<(string Url, string PublicId)> UploadImageFromUrlAsync(string imageUrl, string folderName)
    {
        var fileName = Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri) ? Path.GetFileName(uri.LocalPath) : "image";
        return Task.FromResult<(string, string)>(($"https://cdn.test/{folderName}/{fileName}", $"public/{folderName}/{fileName}"));
    }
}

internal static class FakeFormFile
{
    public static Microsoft.AspNetCore.Http.IFormFile CreatePng(string fileName)
    {
        var bytes = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMB/erR1ZkAAAAASUVORK5CYII=");
        var stream = new MemoryStream(bytes);
        return new Microsoft.AspNetCore.Http.FormFile(stream, 0, bytes.Length, "evidenceFile", fileName)
        {
            Headers = new Microsoft.AspNetCore.Http.HeaderDictionary(),
            ContentType = "image/png"
        };
    }
}
