using System.Security.Claims;
using SmartWorkz.StarterKitMVC.Domain.Entities.Auth;

namespace SmartWorkz.StarterKitMVC.Application.Services;

public interface ITokenService
{
    string GenerateAccessToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions);
    string GenerateRefreshToken();
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}
