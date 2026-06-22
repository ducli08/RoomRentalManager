using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using RoomRentalManagerServer.Domain.ModelEntities.Contracts;
using RoomRentalManagerServer.Infrastructure.Data;
using RoomRentalManagerServer.Infrastructure.Repositories.ContractRepositories;

namespace RoomRentalManagerServer.Tests;

public class ContractRepositoryTests
{
    [Fact]
    public async Task UpdateAsync_PersistsUtilityPricingFields()
    {
        var options = new DbContextOptionsBuilder<RoomRentalManagerServerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var db = new RoomRentalManagerServerDbContext(options);

        var existing = new Contract
        {
            RoomRentalId = 1,
            TenantId = 10,
            TenantIds = new[] { 10L, 11L },
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 12, 31),
            DepositAmout = 1_000_000m,
            MonthlyRent = 2_000_000m,
            ElectricUnitPrice = 4_000m,
            WaterUnitPrice = 30_000m,
            GarbageFeePerYear = 150_000m,
            StatusContract = StatusContract.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatorUser = "seed",
            UpdaterUser = "seed"
        };
        db.Contracts.Add(existing);
        await db.SaveChangesAsync();

        var repository = new ContractRepository(db, NullLogger<ContractRepository>.Instance);

        await repository.UpdateAsync(new Contract
        {
            Id = existing.Id,
            RoomRentalId = existing.RoomRentalId,
            TenantId = 11,
            TenantIds = new[] { 11L, 12L, 13L },
            StartDate = existing.StartDate,
            EndDate = existing.EndDate,
            DepositAmout = existing.DepositAmout,
            MonthlyRent = existing.MonthlyRent,
            ElectricUnitPrice = 4_500m,
            WaterUnitPrice = 35_000m,
            GarbageFeePerYear = 180_000m,
            StatusContract = StatusContract.Active,
            UpdatedAt = DateTime.UtcNow,
            UpdaterUser = "admin"
        });

        var updated = await db.Contracts.AsNoTracking().FirstAsync(x => x.Id == existing.Id);
        Assert.Equal(4_500m, updated.ElectricUnitPrice);
        Assert.Equal(35_000m, updated.WaterUnitPrice);
        Assert.Equal(180_000m, updated.GarbageFeePerYear);
        Assert.Equal(11, updated.TenantId);
        Assert.Equal(new long[] { 11, 12, 13 }, updated.TenantIds);
    }
}
