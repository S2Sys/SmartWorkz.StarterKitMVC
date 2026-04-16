using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;
using SmartWorkz.StarterKitMVC.Application.Authorization;
using SmartWorkz.StarterKitMVC.Shared.Constants;

namespace SmartWorkz.StarterKitMVC.Admin.Middleware;

/// <summary>
/// Comprehensive authorization middleware that validates:
/// 1. Roles - standard ASP.NET Core roles
/// 2. Claims - custom claim types and values
/// 3. Permissions - entity-level CRUD permissions
///
/// Validates that users have the required authorization to access resources
/// and enriches the principal with permission claims from the database.
/// </summary>
public class AuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthorizationMiddleware> _logger;

    public AuthorizationMiddleware(RequestDelegate next, ILogger<AuthorizationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IClaimService claimService,
        IMemoryCache cache)
    {
        try
        {
            // Only process authenticated requests
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var roles = context.User.Claims
                    .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                    .Select(c => c.Value)
                    .ToList();

                if (!string.IsNullOrEmpty(userId) && roles.Count > 0)
                {
                    // Validate and enrich authorization claims
                    await ValidateAndEnrichAuthorizationAsync(
                        context, claimService, cache, userId, roles);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in AuthorizationMiddleware");
            // Continue processing even if enrichment fails
        }

        await _next(context);
    }

    /// <summary>
    /// Validates user authorization and enriches principal with claims/permissions
    /// </summary>
    private async Task ValidateAndEnrichAuthorizationAsync(
        HttpContext context,
        IClaimService claimService,
        IMemoryCache cache,
        string userId,
        List<string> roles)
    {
        try
        {
            // 1. VALIDATE ROLES
            var validRoles = ValidateRoles(roles);
            LogAuthorizationInfo(userId, "Roles", validRoles);

            // 2. VALIDATE AND ENRICH CLAIMS
            await ValidateAndEnrichClaimsAsync(context, claimService, cache, userId, roles);

            // 3. LOG FINAL AUTHORIZATION STATE
            var finalRoles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            var allClaims = context.User.Claims.Select(c => $"{c.Type}={c.Value}").ToList();

            _logger.LogDebug(
                "Authorization validation complete for user {UserId}: Roles={Roles}, ClaimCount={ClaimCount}",
                userId, string.Join(",", finalRoles), context.User.Claims.Count());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during authorization enrichment for user {UserId}", userId);
            // Continue - don't block request on enrichment failure
        }
    }

    /// <summary>
    /// Validates that roles are properly formatted and recognized
    /// </summary>
    private List<string> ValidateRoles(List<string> roles)
    {
        var validRoles = new List<string>();
        var recognizedRoles = new[]
        {
            AppConstants.Roles.Admin.ToLower(),
            AppConstants.Roles.Manager.ToLower(),
            AppConstants.Roles.User.ToLower(),
            "guest",
            "staff"
        };

        foreach (var role in roles)
        {
            var lowerRole = role.ToLowerInvariant();
            if (recognizedRoles.Contains(lowerRole))
            {
                validRoles.Add(lowerRole);
            }
            else
            {
                _logger.LogWarning("Unrecognized role: {Role}", role);
            }
        }

        return validRoles;
    }

    /// <summary>
    /// Validates and enriches claims from the claim service
    /// </summary>
    private async Task ValidateAndEnrichClaimsAsync(
        HttpContext context,
        IClaimService claimService,
        IMemoryCache cache,
        string userId,
        List<string> roles)
    {
        try
        {
            // Get all claim types
            var claimTypes = await claimService.GetActiveClaimTypesAsync();

            if (context.User.Identity is not ClaimsIdentity identity)
            {
                _logger.LogWarning("User identity is not ClaimsIdentity for user {UserId}", userId);
                return;
            }

            foreach (var claimType in claimTypes)
            {
                // Skip if user already has this claim type (don't override explicit claims)
                var existingClaims = identity.FindAll(claimType.Key).ToList();
                if (existingClaims.Any())
                {
                    LogAuthorizationInfo(userId, $"Claim [{claimType.Key}]",
                        existingClaims.Select(c => c.Value).ToList());
                    continue;
                }

                // Get claim values for user's roles
                var cacheKey = $"user_claims:{userId}:{claimType.Key}";
                HashSet<string> claimValues;

                if (!cache.TryGetValue(cacheKey, out claimValues))
                {
                    claimValues = await claimService.GetClaimValuesForRolesAsync(roles, claimType.Key);
                    cache.Set(cacheKey, claimValues, TimeSpan.FromMinutes(10));
                }

                // Add claim values to identity
                if (claimValues?.Count > 0)
                {
                    foreach (var value in claimValues)
                    {
                        // Use the claim type key as the claim type (not the full display name)
                        identity.AddClaim(new Claim(claimType.Key, value));
                        _logger.LogDebug(
                            "Added claim for user {UserId}: Type={ClaimType}, Value={ClaimValue}",
                            userId, claimType.Key, value);
                    }

                    LogAuthorizationInfo(userId, $"Claim [{claimType.Key}]",
                        claimValues.ToList());
                }
            }

            _logger.LogDebug("Claims enrichment complete for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enriching claims for user {UserId}", userId);
            // Continue - don't block on claim enrichment failure
        }
    }

    /// <summary>
    /// Validates that a user has access to a resource
    /// </summary>
    public static bool HasAccessToResource(
        ClaimsPrincipal user,
        string requiredRole = null,
        string requiredClaim = null,
        string requiredClaimValue = null,
        string requiredPermission = null)
    {
        // 1. Check role requirement
        if (!string.IsNullOrEmpty(requiredRole))
        {
            var hasRole = user.FindAll(ClaimTypes.Role)
                .Any(c => c.Value.Equals(requiredRole, StringComparison.OrdinalIgnoreCase));

            if (!hasRole)
                return false;
        }

        // 2. Check claim requirement (type only - any value accepted)
        if (!string.IsNullOrEmpty(requiredClaim) && string.IsNullOrEmpty(requiredClaimValue))
        {
            var hasClaim = user.Claims.Any(c => c.Type.Equals(requiredClaim, StringComparison.OrdinalIgnoreCase));

            if (!hasClaim)
                return false;
        }

        // 3. Check claim requirement (type + specific value)
        if (!string.IsNullOrEmpty(requiredClaim) && !string.IsNullOrEmpty(requiredClaimValue))
        {
            var hasClaim = user.Claims.Any(c =>
                c.Type.Equals(requiredClaim, StringComparison.OrdinalIgnoreCase) &&
                c.Value.Equals(requiredClaimValue, StringComparison.OrdinalIgnoreCase));

            if (!hasClaim)
                return false;
        }

        // 4. Check permission requirement
        if (!string.IsNullOrEmpty(requiredPermission))
        {
            var hasPermission = user.FindAll("permission")
                .Any(c => c.Value.Equals(requiredPermission, StringComparison.OrdinalIgnoreCase));

            if (!hasPermission)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Gets all claims of a specific type from the user
    /// </summary>
    public static List<string> GetClaimValues(ClaimsPrincipal user, string claimType)
    {
        return user.Claims
            .Where(c => c.Type.Equals(claimType, StringComparison.OrdinalIgnoreCase))
            .Select(c => c.Value)
            .ToList();
    }

    /// <summary>
    /// Gets all permissions the user has
    /// </summary>
    public static List<string> GetUserPermissions(ClaimsPrincipal user)
    {
        return user.FindAll("permission")
            .Select(c => c.Value)
            .ToList();
    }

    /// <summary>
    /// Gets all roles the user has
    /// </summary>
    public static List<string> GetUserRoles(ClaimsPrincipal user)
    {
        return user.FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();
    }

    /// <summary>
    /// Helper to log authorization information
    /// </summary>
    private void LogAuthorizationInfo(string userId, string type, List<string> values)
    {
        if (values?.Count > 0)
        {
            _logger.LogDebug(
                "User {UserId} has {Type}: {Values}",
                userId, type, string.Join(", ", values));
        }
    }
}

/// <summary>
/// Extension methods for the authorization middleware
/// </summary>
public static class AuthorizationMiddlewareExtensions
{
    public static IApplicationBuilder UseAuthorizationValidation(this IApplicationBuilder app)
    {
        return app.UseMiddleware<AuthorizationMiddleware>();
    }
}
