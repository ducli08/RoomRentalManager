using RoomRentalManagerServer.Domain.ModelEntities.Contracts;

namespace RoomRentalManagerServer.Application.Common
{
    public class BillingPeriodResult
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public DateTime EffectiveStart { get; set; }
        public DateTime EffectiveEnd { get; set; }
        public int OccupancyDays { get; set; }
        public bool IsFullPeriod { get; set; }
    }

    public static class BillingPeriodHelper
    {
        private const int DaysPerMonth = 30;

        public static DateTime GetAnchorDate(int year, int month, int anchorDay)
        {
            var daysInMonth = DateTime.DaysInMonth(year, month);
            var day = Math.Min(anchorDay, daysInMonth);
            return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Unspecified);
        }

        public static BillingPeriodResult GetBillingPeriod(Contract contract, int month, int year)
        {
            var anchorDay = contract.StartDate.Day;
            var periodStart = GetAnchorDate(year, month, anchorDay);
            var nextPeriodStart = GetAnchorDate(
                month == 12 ? year + 1 : year,
                month == 12 ? 1 : month + 1,
                anchorDay);
            var periodEnd = nextPeriodStart.AddDays(-1);

            var effectiveStart = periodStart.Date >= contract.StartDate.Date
                ? periodStart.Date
                : contract.StartDate.Date;
            var effectiveEnd = periodEnd.Date <= contract.EndDate.Date
                ? periodEnd.Date
                : contract.EndDate.Date;

            var occupancyDays = effectiveEnd >= effectiveStart
                ? (effectiveEnd - effectiveStart).Days + 1
                : 0;

            var isFullPeriod = effectiveStart == periodStart.Date && effectiveEnd == periodEnd.Date;

            return new BillingPeriodResult
            {
                PeriodStart = periodStart.Date,
                PeriodEnd = periodEnd.Date,
                EffectiveStart = effectiveStart,
                EffectiveEnd = effectiveEnd,
                OccupancyDays = occupancyDays,
                IsFullPeriod = isFullPeriod
            };
        }

        public static bool IsPeriodWithinContract(Contract contract, int month, int year)
        {
            var period = GetBillingPeriod(contract, month, year);
            return period.OccupancyDays > 0;
        }

        public static (int Month, int Year) GetPreviousPeriodMonthYear(int month, int year)
        {
            if (month == 1) return (12, year - 1);
            return (month - 1, year);
        }

        public static int GetTenantCount(Contract contract)
        {
            if (contract.TenantIds is { Length: > 0 })
                return contract.TenantIds.Where(x => x > 0).Distinct().Count();

            return contract.TenantId > 0 ? 1 : 0;
        }

        public static decimal CalculateProratedFee(decimal monthlyAmount, int occupancyDays)
        {
            return monthlyAmount / DaysPerMonth * occupancyDays;
        }
    }
}
