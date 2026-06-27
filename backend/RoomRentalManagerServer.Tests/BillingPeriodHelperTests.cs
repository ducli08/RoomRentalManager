using RoomRentalManagerServer.Application.Common;
using RoomRentalManagerServer.Domain.ModelEntities.Contracts;

namespace RoomRentalManagerServer.Tests;

public class BillingPeriodHelperTests
{
    [Fact]
    public void GetAnchorDate_Day31InFebruary_ClampsToLastDay()
    {
        var date = BillingPeriodHelper.GetAnchorDate(2025, 2, 31);
        Assert.Equal(new DateTime(2025, 2, 28), date);
    }

    [Fact]
    public void GetBillingPeriod_Anchor15_MarchPeriod()
    {
        var contract = CreateContract(new DateTime(2025, 1, 15), new DateTime(2025, 12, 31));
        var period = BillingPeriodHelper.GetBillingPeriod(contract, 3, 2025);

        Assert.Equal(new DateTime(2025, 3, 15), period.PeriodStart);
        Assert.Equal(new DateTime(2025, 4, 14), period.PeriodEnd);
        Assert.True(period.IsFullPeriod);
        Assert.Equal(31, period.OccupancyDays);
    }

    [Fact]
    public void GetBillingPeriod_Anchor31_JanuaryToFebruary()
    {
        var contract = CreateContract(new DateTime(2025, 1, 31), new DateTime(2025, 12, 31));
        var period = BillingPeriodHelper.GetBillingPeriod(contract, 1, 2025);

        Assert.Equal(new DateTime(2025, 1, 31), period.PeriodStart);
        Assert.Equal(new DateTime(2025, 2, 27), period.PeriodEnd);
    }

    [Fact]
    public void GetBillingPeriod_LastPeriodPartialRent()
    {
        var contract = CreateContract(new DateTime(2025, 1, 1), new DateTime(2025, 6, 20));
        var period = BillingPeriodHelper.GetBillingPeriod(contract, 6, 2025);

        Assert.Equal(new DateTime(2025, 6, 1), period.PeriodStart);
        Assert.Equal(new DateTime(2025, 6, 30), period.PeriodEnd);
        Assert.Equal(new DateTime(2025, 6, 20), period.EffectiveEnd);
        Assert.False(period.IsFullPeriod);
        Assert.Equal(20, period.OccupancyDays);
    }

    [Fact]
    public void GetBillingPeriod_LastPeriodCutByEndDate()
    {
        var contract = CreateContract(new DateTime(2025, 1, 1), new DateTime(2025, 6, 20));
        var period = BillingPeriodHelper.GetBillingPeriod(contract, 6, 2025);

        Assert.Equal(new DateTime(2025, 6, 20), period.EffectiveEnd);
        Assert.Equal(new DateTime(2025, 6, 30), period.PeriodEnd);
        Assert.False(period.IsFullPeriod);
    }

    [Fact]
    public void CalculateTotalAmount_FullPeriod_NoRounding()
    {
        var contract = CreateContract(new DateTime(2025, 1, 1), new DateTime(2025, 12, 31));
        contract.MonthlyRent = 2_000_000m;
        contract.WaterUnitPrice = 30_000m;
        contract.GarbageFeePerMonthPerPerson = 12_500m;

        var period = BillingPeriodHelper.GetBillingPeriod(contract, 1, 2025);
        var reading = new Domain.ModelEntities.UtilityReadings.UtilityReading
        {
            ElectricUsage = 100,
            ElectricUnitPrice = 4000m
        };

        var total = InvoiceAmountCalculator.CalculateTotalAmount(contract, reading, period);
        const decimal expected = 2_000_000m + (100 * 4000m) + (30_000m / 30 * 31) + (12_500m / 30 * 31);
        Assert.Equal(expected, total);
    }

    [Fact]
    public void CalculateRent_PartialPeriod_UsesProrata()
    {
        var contract = CreateContract(new DateTime(2025, 1, 1), new DateTime(2025, 6, 20));
        contract.MonthlyRent = 3_000_000m;

        var period = BillingPeriodHelper.GetBillingPeriod(contract, 6, 2025);
        var rent = InvoiceAmountCalculator.CalculateRent(contract, period);

        Assert.Equal(3_000_000m / 30 * 20, rent);
    }

    private static Contract CreateContract(DateTime start, DateTime end)
    {
        return new Contract
        {
            StartDate = start,
            EndDate = end,
            TenantId = 1,
            TenantIds = new[] { 1L },
            MonthlyRent = 2_000_000m,
            WaterUnitPrice = 30_000m,
            GarbageFeePerMonthPerPerson = 12_500m,
            ElectricUnitPrice = 4000m
        };
    }
}
