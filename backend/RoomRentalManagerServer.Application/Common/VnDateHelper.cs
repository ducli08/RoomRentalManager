using RoomRentalManagerServer.Domain.ModelEntities.Invoices;

namespace RoomRentalManagerServer.Application.Common
{
    public static class VnDateHelper
    {
        private static readonly TimeZoneInfo VnTimeZone = ResolveVnTimeZone();

        private static TimeZoneInfo ResolveVnTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }
        }

        public static DateTime GetVnToday()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VnTimeZone).Date;
        }

        public static int CalcDaysLate(DateTime dueDate)
        {
            var today = GetVnToday();
            var due = dueDate.Date;
            return today > due ? (today - due).Days : 0;
        }

        public static bool IsOverdue(InvoiceStatus status, decimal balanceDue, DateTime dueDate)
        {
            if (status is InvoiceStatus.Paid or InvoiceStatus.Cancelled)
                return false;
            if (balanceDue <= 0)
                return false;
            return CalcDaysLate(dueDate) > 0;
        }
    }
}
