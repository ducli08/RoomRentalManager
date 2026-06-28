namespace RoomRentalManagerServer.Application.Common
{
    public static class InvoiceCodeHelper
    {
        public static string Build(long invoiceId, int month, int year)
        {
            return $"INV-{year:D4}{month:D2}-{invoiceId:D3}";
        }
    }
}
