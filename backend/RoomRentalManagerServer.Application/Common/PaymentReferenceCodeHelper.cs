namespace RoomRentalManagerServer.Application.Common
{
    public static class PaymentReferenceCodeHelper
    {
        public static string Build(string invoiceCode, long paymentId)
        {
            return $"{invoiceCode}-P{paymentId}";
        }
    }
}
