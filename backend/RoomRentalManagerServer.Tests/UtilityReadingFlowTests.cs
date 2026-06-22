using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using RoomRentalManagerServer.Application.Interfaces;
using RoomRentalManagerServer.Application.Model.UtilityReadingsModel.Dto;
using RoomRentalManagerServer.Application.Model.UtilityReadingsModel.UtilityReadingProfileMapper;
using RoomRentalManagerServer.Application.Services;
using RoomRentalManagerServer.Domain.ModelEntities.Contracts;
using RoomRentalManagerServer.Domain.ModelEntities.Invoices;
using RoomRentalManagerServer.Domain.ModelEntities.UtilityReadings;
using RoomRentalManagerServer.Infrastructure.Data;
using RoomRentalManagerServer.Infrastructure.Repositories.ContractRepositories;
using RoomRentalManagerServer.Infrastructure.Repositories.InvoiceRepositories;
using RoomRentalManagerServer.Infrastructure.Repositories.RoomRentalRepositories;
using RoomRentalManagerServer.Infrastructure.Repositories.UserRepository;
using RoomRentalManagerServer.Infrastructure.Repositories.UtilityReadingRepositories;

namespace RoomRentalManagerServer.Tests;

public class UtilityReadingFlowTests
{
    [Fact]
    public async Task Prepare_FirstMonthWithinContract_OldIndexIsZero()
    {
        var (utilityApp, contract, _) = CreateServices();
        var prepare = await utilityApp.GetPrepareAsync(contract.Id, 1, 2025);

        Assert.True(prepare.CanSave);
        Assert.Equal(0, prepare.OldElectricIndex);
        Assert.Equal(0, prepare.OldWaterIndex);
    }

    [Fact]
    public async Task Prepare_SkipsPreviousMonth_CannotSave()
    {
        var (utilityApp, contract, db) = CreateServices();
        db.UtilityReadings.Add(new UtilityReading
        {
            ContractId = contract.Id,
            Month = 1,
            Year = 2025,
            OldElectricIndex = 0,
            OldWaterIndex = 0,
            NewElectricIndex = 100,
            NewWaterIndex = 10,
            ElectricUsage = 100,
            WaterUsage = 10,
            ElectricUnitPrice = 4000,
            WaterUnitPrice = 30000,
            Status = UtilityReadingStatus.InvoiceGenerated,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatorUser = "test",
            UpdaterUser = "test"
        });
        await db.SaveChangesAsync();

        var prepare = await utilityApp.GetPrepareAsync(contract.Id, 3, 2025);

        Assert.False(prepare.CanSave);
        Assert.Contains("tháng 2/2025", prepare.Message ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Create_January_IncludesGarbageFeeInInvoice()
    {
        var (utilityApp, contract, db) = CreateServices();

        var result = await utilityApp.CreateOrEditAsync(new CreateOrEditUtilityReadingDto
        {
            ContractId = contract.Id,
            Month = 1,
            Year = 2025,
            NewElectricIndex = 100,
            NewWaterIndex = 10
        });

        var invoice = await db.Invoices.AsNoTracking()
            .FirstAsync(x => x.UtilityReadingId == result.Id);

        const decimal expected = 2_000_000m + (100 * 4000m) + (10 * 30_000m) + 150_000m;
        Assert.Equal(expected, invoice.TotalAmount);
        Assert.Equal(InvoiceStatus.Issued, invoice.Status);
        Assert.Equal(UtilityReadingStatus.InvoiceGenerated, result.Status);
    }

    [Fact]
    public async Task Create_February_DoesNotIncludeGarbageFee()
    {
        var (utilityApp, contract, db) = CreateServices();

        await utilityApp.CreateOrEditAsync(new CreateOrEditUtilityReadingDto
        {
            ContractId = contract.Id,
            Month = 1,
            Year = 2025,
            NewElectricIndex = 100,
            NewWaterIndex = 10
        });

        var feb = await utilityApp.CreateOrEditAsync(new CreateOrEditUtilityReadingDto
        {
            ContractId = contract.Id,
            Month = 2,
            Year = 2025,
            NewElectricIndex = 150,
            NewWaterIndex = 15
        });

        var invoice = await db.Invoices.AsNoTracking()
            .FirstAsync(x => x.UtilityReadingId == feb.Id);

        const decimal expected = 2_000_000m + (50 * 4000m) + (5 * 30_000m);
        Assert.Equal(expected, invoice.TotalAmount);
    }

    [Fact]
    public async Task Edit_CascadesOldIndexToNextMonth()
    {
        var (utilityApp, contract, db) = CreateServices();

        var jan = await utilityApp.CreateOrEditAsync(new CreateOrEditUtilityReadingDto
        {
            ContractId = contract.Id,
            Month = 1,
            Year = 2025,
            NewElectricIndex = 100,
            NewWaterIndex = 10
        });

        var feb = await utilityApp.CreateOrEditAsync(new CreateOrEditUtilityReadingDto
        {
            ContractId = contract.Id,
            Month = 2,
            Year = 2025,
            NewElectricIndex = 150,
            NewWaterIndex = 15
        });

        await utilityApp.CreateOrEditAsync(new CreateOrEditUtilityReadingDto
        {
            Id = jan.Id,
            ContractId = contract.Id,
            Month = 1,
            Year = 2025,
            NewElectricIndex = 120,
            NewWaterIndex = 12
        });

        var febEntity = await db.UtilityReadings.AsNoTracking().FirstAsync(x => x.Id == feb.Id);
        Assert.Equal(120, febEntity.OldElectricIndex);
        Assert.Equal(12, febEntity.OldWaterIndex);
        Assert.Equal(30, febEntity.ElectricUsage);
        Assert.Equal(3, febEntity.WaterUsage);
    }

    [Fact]
    public async Task Edit_WhenInvoiceHasPayment_Throws()
    {
        var (utilityApp, contract, db) = CreateServices();

        var reading = await utilityApp.CreateOrEditAsync(new CreateOrEditUtilityReadingDto
        {
            ContractId = contract.Id,
            Month = 1,
            Year = 2025,
            NewElectricIndex = 100,
            NewWaterIndex = 10
        });

        var invoice = await db.Invoices.FirstAsync(x => x.UtilityReadingId == reading.Id);
        invoice.AmountPaid = 1;
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() => utilityApp.CreateOrEditAsync(new CreateOrEditUtilityReadingDto
        {
            Id = reading.Id,
            ContractId = contract.Id,
            Month = 1,
            Year = 2025,
            NewElectricIndex = 110,
            NewWaterIndex = 11
        }));
    }

    [Fact]
    public async Task Export_ReturnsNonEmptyExcel()
    {
        var (utilityApp, contract, _) = CreateServices();
        await utilityApp.CreateOrEditAsync(new CreateOrEditUtilityReadingDto
        {
            ContractId = contract.Id,
            Month = 1,
            Year = 2025,
            NewElectricIndex = 50,
            NewWaterIndex = 5
        });

        var bytes = await utilityApp.ExportExcelAsync(new UtilityReadingFilterDto { Year = 2025 });
        Assert.NotEmpty(bytes);
        Assert.Equal(0x50, bytes[0]); // PK zip header
    }

    private static (IUtilityReadingAppService UtilityApp, Contract Contract, RoomRentalManagerServerDbContext Db) CreateServices()
    {
        var options = new DbContextOptionsBuilder<RoomRentalManagerServerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        var db = new RoomRentalManagerServerDbContext(options);

        var contract = new Contract
        {
            RoomRentalId = 1,
            TenantId = 10,
            TenantIds = new[] { 10L },
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 12, 31),
            DepositAmout = 0,
            MonthlyRent = 2_000_000m,
            ElectricUnitPrice = 4000m,
            WaterUnitPrice = 30_000m,
            GarbageFeePerYear = 150_000m,
            StatusContract = StatusContract.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatorUser = "seed",
            UpdaterUser = "seed"
        };
        db.Contracts.Add(contract);
        db.SaveChanges();

        var contractRepo = new ContractRepository(db, NullLogger<ContractRepository>.Instance);
        var utilityRepo = new UtilityReadingRepository(db, NullLogger<UtilityReadingRepository>.Instance);
        var invoiceRepo = new InvoiceRepository(db, NullLogger<InvoiceRepository>.Instance);
        var roomRepo = new RoomRentalRepository(db, NullLogger<RoomRentalRepository>.Instance);
        var userRepo = new UserRepository(db, NullLogger<UserRepository>.Instance);
        var currentUser = new FakeCurrentUserAppService(isAuthenticated: true, userId: 1, userName: "admin");

        var mapper = new MapperConfiguration(cfg => cfg.AddProfile<UtilityReadingMappingProfile>()).CreateMapper();

        IInvoiceAppService invoiceApp = new InvoiceAppService(
            NullLogger<InvoiceAppService>.Instance,
            invoiceRepo,
            contractRepo,
            currentUser,
            mapper);

        IUtilityReadingAppService utilityApp = new UtilityReadingAppService(
            NullLogger<UtilityReadingAppService>.Instance,
            mapper,
            utilityRepo,
            contractRepo,
            roomRepo,
            userRepo,
            invoiceApp,
            currentUser,
            db);

        return (utilityApp, contract, db);
    }
}
