namespace RoomRentalManagerServer.Application.Model.Login.Dto
{
    public class RefreshResponseDto
    {
        public string AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }
}
