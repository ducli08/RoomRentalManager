using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using RoomRentalManagerServer.Application.Common.CommonDto;
using RoomRentalManagerServer.Application.Interfaces;
using RoomRentalManagerServer.Application.Model.InvoicesModel.Dto;
using RoomRentalManagerServer.Application.Model.PaymentSubmissionsModel.Dto;
using RoomRentalManagerServer.Application.Services;
using RoomRentalManagerServer.Domain.Interfaces;
using RoomRentalManagerServer.Domain.ModelEntities.BankAccounts;
using RoomRentalManagerServer.Domain.ModelEntities.Contracts;
using RoomRentalManagerServer.Domain.ModelEntities.Invoices;
using RoomRentalManagerServer.Domain.ModelEntities.PaymentSubmissions;
using RoomRentalManagerServer.Infrastructure.Data;
using RoomRentalManagerServer.Infrastructure.Repositories.BankAccountRepositories;
using RoomRentalManagerServer.Infrastructure.Repositories.ContractRepositories;
using RoomRentalManagerServer.Infrastructure.Repositories.InvoiceRepositories;
using RoomRentalManagerServer.Infrastructure.Repositories.PaymentRepositories;
using RoomRentalManagerServer.Infrastructure.Repositories.PaymentSubmissionRepositories;

namespace RoomRentalManagerServer.Tests;

public class InvoicePaymentFlowTests
{
    [Fact]
    public async Task DraftIssue_ThenTenantIntent_Submit_ThenAdminApprove_MarksInvoicePaid()
    {
        var (db, invoiceApp, paymentFlowTenant, paymentFlowAdmin) = CreateServices();

        // Seed: active contract for tenant 10
        var contract = new Contract
        {
            RoomRentalId = 1,
            TenantId = 10,
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

        // Seed: active bank account
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

        // Admin creates draft invoice
        var createOk = await invoiceApp.CreateOrEditAsync(new CreateOrEditInvoiceDto
        {
            ContractId = contract.Id,
            InvoiceDate = DateTime.UtcNow.Date,
            DueDate = DateTime.UtcNow.AddDays(7),
            TotalAmount = 1000000m
        });
        Assert.True(createOk);

        var invoice = await db.Invoices.AsNoTracking().OrderByDescending(x => x.Id).FirstAsync();
        Assert.Equal(InvoiceStatus.Draft, invoice.Status);

        // Issue
        var issueOk = await invoiceApp.IssueAsync(invoice.Id);
        Assert.True(issueOk);
        invoice = await db.Invoices.AsNoTracking().FirstAsync(x => x.Id == invoice.Id);
        Assert.Equal(InvoiceStatus.Issued, invoice.Status);

        // Tenant creates payment intent
        var intent = await paymentFlowTenant.CreatePaymentIntentAsync(invoice.Id);
        Assert.Equal(invoice.Id, intent.InvoiceId);
        Assert.Equal(1000000m, intent.Amount);
        Assert.Contains("INV-", intent.TransferContent);
        Assert.Contains("vietqr.io", intent.QrImageUrl);

        // Tenant submits evidence
        var evidence = FakeFormFile.CreatePng("evidence.png");
        var submission = await paymentFlowTenant.SubmitTransferEvidenceAsync(invoice.Id, evidence);
        Assert.Equal(PaymentSubmissionStatus.Pending, submission.Status);
        Assert.Equal(1000000m, submission.DeclaredAmount);

        // Admin approves
        var approved = await paymentFlowAdmin.ApproveSubmissionAsync(submission.Id);
        Assert.Equal(PaymentSubmissionStatus.Approved, approved.Status);

        invoice = await db.Invoices.AsNoTracking().FirstAsync(x => x.Id == invoice.Id);
        Assert.Equal(InvoiceStatus.Paid, invoice.Status);
        Assert.Equal(1000000m, invoice.AmountPaid);
    }

    [Fact]
    public async Task TenantCannotAccessInvoiceNotBelongingToContract()
    {
        var (db, invoiceApp, paymentFlowTenant, _) = CreateServices();

        var contract = new Contract
        {
            RoomRentalId = 1,
            TenantId = 999,
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

        await invoiceApp.CreateOrEditAsync(new CreateOrEditInvoiceDto
        {
            ContractId = contract.Id,
            InvoiceDate = DateTime.UtcNow.Date,
            DueDate = DateTime.UtcNow.AddDays(7),
            TotalAmount = 100m
        });
        var invoice = await db.Invoices.AsNoTracking().OrderByDescending(x => x.Id).FirstAsync();
        await invoiceApp.IssueAsync(invoice.Id);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => paymentFlowTenant.CreatePaymentIntentAsync(invoice.Id));
    }

    private static (RoomRentalManagerServerDbContext Db, IInvoiceAppService InvoiceApp, IPaymentFlowAppService PaymentFlowTenant, IPaymentFlowAppService PaymentFlowAdmin) CreateServices()
    {
        var options = new DbContextOptionsBuilder<RoomRentalManagerServerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new RoomRentalManagerServerDbContext(options);

        var invoiceRepo = new InvoiceRepository(db, NullLogger<InvoiceRepository>.Instance);
        var contractRepo = new ContractRepository(db, NullLogger<ContractRepository>.Instance);
        var paymentRepo = new PaymentRepository(db, NullLogger<PaymentRepository>.Instance);
        var submissionRepo = new PaymentSubmissionRepository(db, NullLogger<PaymentSubmissionRepository>.Instance);
        var bankRepo = new BankAccountRepository(db, NullLogger<BankAccountRepository>.Instance);

        // Current users: tenant 10 and admin 1
        var currentTenant = new FakeCurrentUserAppService(isAuthenticated: true, userId: 10, userName: "tenant");
        var currentAdmin = new FakeCurrentUserAppService(isAuthenticated: true, userId: 1, userName: "admin");

        // Invoice app uses admin for create/issue in tests
        IInvoiceAppService invoiceApp = new InvoiceAppService(
            NullLogger<InvoiceAppService>.Instance,
            invoiceRepo,
            contractRepo,
            currentAdmin,
            mapper: null! // mapper not used in InvoiceAppService currently
        );

        var cloudinary = new FakeCloudinaryService();

        IPaymentFlowAppService paymentFlowTenant = new PaymentFlowAppService(
            NullLogger<PaymentFlowAppService>.Instance,
            currentTenant,
            invoiceRepo,
            contractRepo,
            submissionRepo,
            paymentRepo,
            bankRepo,
            cloudinary);

        IPaymentFlowAppService paymentFlowAdmin = new PaymentFlowAppService(
            NullLogger<PaymentFlowAppService>.Instance,
            currentAdmin,
            invoiceRepo,
            contractRepo,
            submissionRepo,
            paymentRepo,
            bankRepo,
            cloudinary);

        return (db, invoiceApp, paymentFlowTenant, paymentFlowAdmin);
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
        // 1x1 transparent PNG
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
