using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using RoomRentalManagerServer.Application.Interfaces;
using RoomRentalManagerServer.Application.Model.Login.Dto;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RoomRentalManagerServer.Application.Services
{
    public class GoogleTokenValidatorAppService : IGoogleTokenValidatorAppService
    {
        private readonly IConfigurationManager<OpenIdConnectConfiguration> _configuration;
        private readonly string _clientId;

        public GoogleTokenValidatorAppService(IConfiguration configuration)
        {
            var documentRetriever = new HttpDocumentRetriever
            {
                RequireHttps = true
            };
            _configuration = new ConfigurationManager<OpenIdConnectConfiguration>(
                "https://accounts.google.com/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever(),
                documentRetriever);
            _clientId = configuration["GOOGLE_CLIENT_ID"] ?? throw new Exception("Missing GOOGLE_CLIENT_ID");
        }

        public async Task<GoogleTokenPayload?> ValidateIdTokenAsync(string idToken)
        {
            var config = await _configuration.GetConfigurationAsync(CancellationToken.None);
            var validationParameters = new TokenValidationParameters
            {
                ValidIssuer = "https://accounts.google.com",
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidAudiences = new[] { _clientId },
                IssuerSigningKeys = config.SigningKeys,
                ValidateLifetime = true
            };

            var handler = new JwtSecurityTokenHandler();
            try
            {
                var principal = handler.ValidateToken(idToken, validationParameters, out var validatedToken);
                var jwt = validatedToken as JwtSecurityToken;

                // Safely access claims by type; do not index into principal.Claims (IEnumerable<Claim>).
                var payload = new GoogleTokenPayload
                {
                    Sub = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? principal.FindFirst("sub")?.Value,
                    Email = principal.FindFirst(ClaimTypes.Email)?.Value ?? principal.FindFirst("email")?.Value,
                    EmailVerified = (principal.FindFirst("email_verified")?.Value ?? principal.FindFirst("verified_email")?.Value) == "true",
                    Name = principal.FindFirst(ClaimTypes.Name)?.Value ?? principal.FindFirst("name")?.Value,
                    Picture = principal.FindFirst("picture")?.Value
                };

                return payload;
            }
            catch
            {
                return null;
            }
        }
    }
}
