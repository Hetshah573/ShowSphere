using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using ShowSphere.Domain.Interfaces;

namespace ShowSphere.Infrastructure.Services;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly string _clientId;

    public GoogleAuthService(IConfiguration configuration)
    {
        _clientId = configuration["Google:ClientId"]!;
    }

    public async Task<GoogleUserInfo?> VerifyIdTokenAsync(string idToken)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _clientId }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            return new GoogleUserInfo(
                payload.Email,
                payload.GivenName ?? payload.Name ?? "User",
                payload.FamilyName ?? "",
                payload.Picture);
        }
        catch
        {
            return null;
        }
    }
}
