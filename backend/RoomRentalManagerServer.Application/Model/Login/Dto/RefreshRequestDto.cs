namespace RoomRentalManagerServer.Application.Model.Login.Dto
{
    public class RefreshRequestDto
    {
        public long UserId { get; set; }
        public string RefreshToken { get; set; }
        public bool RememberMe { get; set; }
    }
}
