namespace SmartWorkz.Shared.Security.Audit;

/// <summary>
/// Interface for security audit logging with sensitive data redaction.
/// </summary>
public interface ISecurityAuditLogger
{
    /// <summary>
    /// Logs an authentication failure event.
    /// </summary>
    /// <param name="userId">The user identifier attempting authentication</param>
    /// <param name="reason">The reason for authentication failure</param>
    void LogAuthenticationFailure(string userId, string reason);

    /// <summary>
    /// Logs suspicious input patterns detected in user input.
    /// </summary>
    /// <param name="userId">The user identifier who submitted the input</param>
    /// <param name="patternType">The type of suspicious pattern (SQL injection, XSS, Command injection, etc.)</param>
    /// <param name="input">The suspicious input (will be redacted in logs)</param>
    void LogSuspiciousInput(string userId, string patternType, string input);

    /// <summary>
    /// Logs rate limit violations.
    /// </summary>
    /// <param name="clientId">The client identifier (IP address, API key, etc.)</param>
    /// <param name="requestsCount">The number of requests made</param>
    /// <param name="limitCount">The rate limit threshold</param>
    void LogRateLimitViolation(string clientId, int requestsCount, int limitCount);

    /// <summary>
    /// Logs certificate pinning validation failures.
    /// </summary>
    /// <param name="hostName">The hostname that failed certificate pinning</param>
    /// <param name="certificateThumbprint">The thumbprint of the certificate presented</param>
    void LogCertificatePinningFailure(string hostName, string certificateThumbprint);

    /// <summary>
    /// Logs data access events for audit trail.
    /// </summary>
    /// <param name="userId">The user accessing the data</param>
    /// <param name="dataType">The type of data being accessed</param>
    /// <param name="recordId">The record identifier being accessed</param>
    void LogDataAccess(string userId, string dataType, string recordId);

    /// <summary>
    /// Logs privilege escalation attempts.
    /// </summary>
    /// <param name="userId">The user attempting privilege change</param>
    /// <param name="attemptedRole">The role the user is attempting to assume</param>
    /// <param name="allowed">Whether the privilege change was allowed</param>
    void LogPrivilegeChange(string userId, string attemptedRole, bool allowed);
}
