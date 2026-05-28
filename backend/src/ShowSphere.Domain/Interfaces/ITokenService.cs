using ShowSphere.Domain.Entities;

namespace ShowSphere.Domain.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    Guid? ValidateAccessToken(string token);
}
