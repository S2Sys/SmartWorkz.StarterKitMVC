using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SmartWorkz.StarterKitMVC.Application.Abstractions;
using SmartWorkz.StarterKitMVC.Application.MultiTenancy;
using SmartWorkz.StarterKitMVC.Domain.Entities.Auth;
using SmartWorkz.StarterKitMVC.Infrastructure.Data;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Auditing;

/// <summary>
/// Logs audit trail entries to the AuditTrail table in AuthDbContext.
/// Captures action, user, entity type, IP address, and user agent for GDPR/SOC2 compliance.
/// </summary>
public sealed class AuditLogger : IAuditLogger
{
    private readonly AuthDbContext _authDbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditLogger> _logger;
    private readonly ITenantContext _tenantContext;

    public AuditLogger(
        AuthDbContext authDbContext,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuditLogger> logger,
        ITenantContext tenantContext)
    {
        _authDbContext = authDbContext ?? throw new ArgumentNullException(nameof(authDbContext));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    public async Task LogAsync(
        string action,
        string? subjectId = null,
        string? subjectType = null,
        object? metadata = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(action))
            throw new ArgumentException("Action cannot be null or empty.", nameof(action));

        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var ipAddress = GetClientIpAddress(httpContext) ?? "unknown";
            var userAgent = httpContext?.Request.Headers["User-Agent"].ToString() ?? "unknown";
            var userId = httpContext?.User?.FindFirst("sub")?.Value ?? "anonymous";
            var tenantId = _tenantContext.TenantId ?? "unknown";
            var changes = metadata != null ? JsonSerializer.Serialize(metadata) : null;

            var auditEntry = new AuditTrail
            {
                UserId = userId,
                Action = action,
                EntityType = subjectType ?? "Unknown",
                Changes = changes,
                IPAddress = ipAddress,
                UserAgent = userAgent,
                TenantId = tenantId,
                CreatedAt = DateTime.UtcNow
            };

            _authDbContext.AuditTrails.Add(auditEntry);
            await _authDbContext.SaveChangesAsync(cancellationToken);

            _logger.LogDebug(
                "Audit logged: Action={Action}, UserId={UserId}, EntityType={EntityType}, TenantId={TenantId}",
                action, userId, subjectType, tenantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log audit trail: {Action}", action);
            // Don't throw — audit failures should not break application flow
        }
    }

    private static string? GetClientIpAddress(HttpContext? httpContext)
    {
        if (httpContext == null)
            return null;

        // Try X-Forwarded-For first (for proxy scenarios)
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            var ips = forwardedFor.Split(',');
            return ips.FirstOrDefault()?.Trim();
        }

        // Fall back to RemoteIpAddress
        return httpContext.Connection.RemoteIpAddress?.ToString();
    }
}
