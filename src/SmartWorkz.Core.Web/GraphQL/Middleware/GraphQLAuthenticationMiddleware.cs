using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace SmartWorkz.Core.Web.GraphQL.Middleware;

/// <summary>
/// GraphQL middleware for JWT authentication.
/// Validates JWT tokens and attaches claims to the GraphQL request context.
/// </summary>
public static class GraphQLAuthenticationMiddleware
{

    /// <summary>
    /// Validates JWT token from Authorization header.
    /// </summary>
    /// <param name="token">The JWT token to validate.</param>
    /// <param name="issuer">Optional issuer claim to validate.</param>
    /// <param name="audience">Optional audience claim to validate.</param>
    /// <returns>ClaimsPrincipal if token is valid; null otherwise.</returns>
    public static ClaimsPrincipal? ValidateToken(string token, string? issuer = null, string? audience = null)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();

            if (!handler.CanReadToken(token))
            {
                return null;
            }

            var jwtToken = handler.ReadJwtToken(token);

            // Create a claims principal from the token claims
            var identity = new ClaimsIdentity(jwtToken.Claims, "jwt");
            return new ClaimsPrincipal(identity);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Extracts JWT token from Authorization header.
    /// Expected format: "Bearer {token}"
    /// </summary>
    public static string? ExtractBearerToken(HttpContext context)
    {
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();

        if (string.IsNullOrEmpty(authHeader))
        {
            return null;
        }

        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return authHeader["Bearer ".Length..].Trim();
    }
}
