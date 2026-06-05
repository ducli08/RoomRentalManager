namespace RoomRentalManagerServer.Application.Model.Login.Dto
{
    public class GoogleLoginRequestDto
    {
        public string IdToken { get; set; }
        public bool RememberMe { get; set; }
    }
}

