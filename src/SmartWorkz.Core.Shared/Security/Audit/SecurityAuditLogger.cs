namespace SmartWorkz.Shared.Security.Audit;

using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

/// <summary>
/// Implementation of security audit logging with sensitive data redaction.
/// </summary>
public class SecurityAuditLogger : ISecurityAuditLogger
{
    private readonly ILogger<SecurityAuditLogger> _logger;

    public SecurityAuditLogger(ILogger<SecurityAuditLogger> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Logs an authentication failure event.
    /// </summary>
    public void LogAuthenticationFailure(string userId, string reason)
    {
        var redactedUserId = RedactSensitiveData(userId);

        _logger.LogWarning(
            "Authentication failure for user {UserId}: {Reason}",
            redactedUserId,
            reason);
    }

    /// <summary>
    /// Logs suspicious input patterns detected in user input.
    /// </summary>
    public void LogSuspiciousInput(string userId, string patternType, string input)
    {
        var redactedUserId = RedactSensitiveData(userId);
        var redactedInput = RedactSensitiveData(input);

        _logger.LogWarning(
            "Suspicious input detected for user {UserId} - Pattern: {PatternType}, Input: {Input}",
            redactedUserId,
            patternType,
            redactedInput);
    }

    /// <summary>
    /// Logs rate limit violations.
    /// </summary>
    public void LogRateLimitViolation(string clientId, int requestsCount, int limitCount)
    {
        var redactedClientId = RedactSensitiveData(clientId);

        _logger.LogWarning(
            "Rate limit violation for client {ClientId}: {RequestsCount} requests exceeds limit of {LimitCount}",
            redactedClientId,
            requestsCount,
            limitCount);
    }

    /// <summary>
    /// Logs certificate pinning validation failures.
    /// </summary>
    public void LogCertificatePinningFailure(string hostName, string certificateThumbprint)
    {
        var redactedThumbprint = RedactSensitiveData(certificateThumbprint);

        _logger.LogCritical(
            "Certificate pinning validation failed for host {HostName}. Certificate thumbprint: {CertificateThumbprint}",
            hostName,
            redactedThumbprint);
    }

    /// <summary>
    /// Logs data access events for audit trail.
    /// </summary>
    public void LogDataAccess(string userId, string dataType, string recordId)
    {
        var redactedUserId = RedactSensitiveData(userId);
        var redactedRecordId = RedactSensitiveData(recordId);

        _logger.LogInformation(
            "Data access by user {UserId}: type {DataType}, record {RecordId}",
            redactedUserId,
            dataType,
            redactedRecordId);
    }

    /// <summary>
    /// Logs privilege escalation attempts.
    /// </summary>
    public void LogPrivilegeChange(string userId, string attemptedRole, bool allowed)
    {
        var redactedUserId = RedactSensitiveData(userId);
        var logLevel = allowed ? LogLevel.Information : LogLevel.Warning;

        if (allowed)
        {
            _logger.Log(
                logLevel,
                "Privilege escalation granted for user {UserId}: role {AttemptedRole}",
                redactedUserId,
                attemptedRole);
        }
        else
        {
            _logger.Log(
                logLevel,
                "Privilege escalation denied for user {UserId}: attempted role {AttemptedRole}",
                redactedUserId,
                attemptedRole);
        }
    }

    /// <summary>
    /// Redacts sensitive data from strings.
    /// Handles emails, IDs, tokens, and other sensitive patterns.
    /// </summary>
    private static string RedactSensitiveData(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var redacted = input;

        // Redact email addresses
        redacted = Regex.Replace(
            redacted,
            @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}",
            "[REDACTED_EMAIL]",
            RegexOptions.IgnoreCase);

        // Redact UUID/GUID patterns
        redacted = Regex.Replace(
            redacted,
            @"\b[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}\b",
            "[REDACTED_ID]");

        // Redact numeric IDs (sequences of 8+ digits)
        redacted = Regex.Replace(
            redacted,
            @"\b\d{8,}\b",
            "[REDACTED_ID]");

        // Redact JWT tokens (long alphanumeric sequences with dots)
        redacted = Regex.Replace(
            redacted,
            @"\b[A-Za-z0-9_-]{20,}\.[A-Za-z0-9_-]{20,}\.[A-Za-z0-9_-]{20,}\b",
            "[REDACTED_TOKEN]");

        // Redact common token patterns
        redacted = Regex.Replace(
            redacted,
            @"(token|key|secret|password|auth|bearer)\s*[:=]\s*[^\s]+",
            "[REDACTED_TOKEN]",
            RegexOptions.IgnoreCase);

        // Redact API keys (common patterns)
        redacted = Regex.Replace(
            redacted,
            @"(api[_-]?key|apikey)\s*[:=]\s*[^\s]+",
            "[REDACTED_API_KEY]",
            RegexOptions.IgnoreCase);

        // Redact IP addresses (if longer than simple patterns)
        redacted = Regex.Replace(
            redacted,
            @"\b(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b",
            "[REDACTED_IP]");

        return redacted;
    }
}
