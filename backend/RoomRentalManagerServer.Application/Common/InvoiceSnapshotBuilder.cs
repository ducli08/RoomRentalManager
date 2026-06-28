using RoomRentalManagerServer.Domain.ModelEntities.Contracts;
using RoomRentalManagerServer.Domain.ModelEntities.Invoices;
using RoomRentalManagerServer.Domain.ModelEntities.UtilityReadings;

namespace RoomRentalManagerServer.Application.Common
{
    public static class InvoiceSnapshotBuilder
    {
        public static List<InvoiceItem> BuildItems(
            Contract contract,
            UtilityReading reading,
            BillingPeriodResult period)
        {
            var rent = InvoiceAmountCalculator.CalculateRent(contract, period);
            var electric = InvoiceAmountCalculator.CalculateElectricFee(reading);
            var water = InvoiceAmountCalculator.CalculateWaterFee(contract, period);
            var garbage = InvoiceAmountCalculator.CalculateGarbageFee(contract, period);
            var tenantCount = BillingPeriodHelper.GetTenantCount(contract);

            return new List<InvoiceItem>
            {
                new()
                {
                    ItemType = InvoiceItemType.Rent,
                    Description = "Tiền phòng",
                    Quantity = 1,
                    UnitPrice = period.IsFullPeriod ? contract.MonthlyRent : rent,
                    Amount = rent,
                    SortOrder = 1
                },
                new()
                {
                    ItemType = InvoiceItemType.Electric,
                    Description = $"Điện ({reading.ElectricUsage:N0} kWh × {reading.ElectricUnitPrice:N0})",
                    Quantity = reading.ElectricUsage,
                    UnitPrice = reading.ElectricUnitPrice,
                    Amount = electric,
                    SortOrder = 2
                },
                new()
                {
                    ItemType = InvoiceItemType.Water,
                    Description = "Nước",
                    Quantity = tenantCount,
                    UnitPrice = contract.WaterUnitPrice,
                    Amount = water,
                    SortOrder = 3
                },
                new()
                {
                    ItemType = InvoiceItemType.Garbage,
                    Description = "Phí rác",
                    Quantity = tenantCount,
                    UnitPrice = contract.GarbageFeePerMonthPerPerson,
                    Amount = garbage,
                    SortOrder = 4
                }
            };
        }
    }
}
