using OlloLifestyleAPI.Core.Entities;
using System.Security.Claims;

namespace OlloLifestyleAPI.Core.Interfaces;

public interface ITokenService
{
    string GenerateJwtToken(User user, IList<string> roles, IList<Claim> claims);
    string GenerateRefreshToken();
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}