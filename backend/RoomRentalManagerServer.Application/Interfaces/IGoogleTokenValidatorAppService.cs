using RoomRentalManagerServer.Application.Model.Login.Dto;

namespace RoomRentalManagerServer.Application.Interfaces
{
    public interface IGoogleTokenValidatorAppService
    {
        Task<GoogleTokenPayload?> ValidateIdTokenAsync(string idToken);
    }
}
