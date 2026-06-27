using RoomRentalManagerServer.Domain.ModelEntities.Contracts;
using RoomRentalManagerServer.Domain.ModelEntities.UtilityReadings;

namespace RoomRentalManagerServer.Application.Common
{
    public static class InvoiceAmountCalculator
    {
        public static decimal CalculateRent(Contract contract, BillingPeriodResult period)
        {
            if (period.OccupancyDays <= 0) return 0;

            if (period.IsFullPeriod)
                return contract.MonthlyRent;

            return BillingPeriodHelper.CalculateProratedFee(contract.MonthlyRent, period.OccupancyDays);
        }

        public static decimal CalculateWaterFee(Contract contract, BillingPeriodResult period)
        {
            if (period.OccupancyDays <= 0) return 0;

            var tenantCount = BillingPeriodHelper.GetTenantCount(contract);
            var monthlyWater = contract.WaterUnitPrice * tenantCount;
            return BillingPeriodHelper.CalculateProratedFee(monthlyWater, period.OccupancyDays);
        }

        public static decimal CalculateGarbageFee(Contract contract, BillingPeriodResult period)
        {
            if (period.OccupancyDays <= 0) return 0;

            var tenantCount = BillingPeriodHelper.GetTenantCount(contract);
            var monthlyGarbage = contract.GarbageFeePerMonthPerPerson * tenantCount;
            return BillingPeriodHelper.CalculateProratedFee(monthlyGarbage, period.OccupancyDays);
        }

        public static decimal CalculateElectricFee(UtilityReading reading)
        {
            return reading.ElectricUsage * reading.ElectricUnitPrice;
        }

        public static decimal CalculateTotalAmount(
            Contract contract,
            UtilityReading reading,
            BillingPeriodResult period)
        {
            return CalculateRent(contract, period)
                   + CalculateWaterFee(contract, period)
                   + CalculateGarbageFee(contract, period)
                   + CalculateElectricFee(reading);
        }
    }
}
