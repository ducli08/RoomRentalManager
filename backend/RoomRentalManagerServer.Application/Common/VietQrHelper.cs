namespace RoomRentalManagerServer.Application.Common
{
    public static class VietQrHelper
    {
        public static string BuildImageUrl(string bankCode, string accountNumber, string accountName, decimal amount, string addInfo)
        {
            var encodedAddInfo = Uri.EscapeDataString(addInfo);
            var encodedName = Uri.EscapeDataString(accountName);
            return $"https://img.vietqr.io/image/{bankCode}-{accountNumber}-compact2.png?amount={amount}&addInfo={encodedAddInfo}&accountName={encodedName}";
        }
    }
}
