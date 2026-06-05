namespace RoomRentalManagerServer.Application.Model.PaymentsModel.Dto
{
    public class PaymentIntentDto
    {
        public long InvoiceId { get; set; }
        public decimal Amount { get; set; }
        public string TransferContent { get; set; }
        public string BankCode { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }

        // A convenient URL for rendering QR on client (based on vietqr.io)
        public string QrImageUrl { get; set; }
    }
}

