# Audit API Reference

## Classes & Interfaces

### ISecurityAuditLogger

- **Namespace:** `SmartWorkz.Shared.Security.Audit.ISecurityAuditLogger`
- **Summary:** Interface for security audit logging with sensitive data redaction.

#### Methods & Properties

- **LogAuthenticationFailure** - Logs an authentication failure event.
  - Parameters:
    - `userId`: The user identifier attempting authentication
    - `reason`: The reason for authentication failure
- **LogSuspiciousInput** - Logs suspicious input patterns detected in user input.
  - Parameters:
    - `userId`: The user identifier who submitted the input
    - `patternType`: The type of suspicious pattern (SQL injection, XSS, Command injection, etc.)
    - `input`: The suspicious input (will be redacted in logs)
- **LogRateLimitViolation** - Logs rate limit violations.
  - Parameters:
    - `clientId`: The client identifier (IP address, API key, etc.)
    - `requestsCount`: The number of requests made
    - `limitCount`: The rate limit threshold
- **LogCertificatePinningFailure** - Logs certificate pinning validation failures.
  - Parameters:
    - `hostName`: The hostname that failed certificate pinning
    - `certificateThumbprint`: The thumbprint of the certificate presented
- **LogDataAccess** - Logs data access events for audit trail.
  - Parameters:
    - `userId`: The user accessing the data
    - `dataType`: The type of data being accessed
    - `recordId`: The record identifier being accessed
- **LogPrivilegeChange** - Logs privilege escalation attempts.
  - Parameters:
    - `userId`: The user attempting privilege change
    - `attemptedRole`: The role the user is attempting to assume
    - `allowed`: Whether the privilege change was allowed

### SecurityAuditLogger

- **Namespace:** `SmartWorkz.Shared.Security.Audit.SecurityAuditLogger`
- **Summary:** Implementation of security audit logging with sensitive data redaction.

#### Methods & Properties

- **LogAuthenticationFailure** - Logs an authentication failure event.
- **LogSuspiciousInput** - Logs suspicious input patterns detected in user input.
- **LogRateLimitViolation** - Logs rate limit violations.
- **LogCertificatePinningFailure** - Logs certificate pinning validation failures.
- **LogDataAccess** - Logs data access events for audit trail.
- **LogPrivilegeChange** - Logs privilege escalation attempts.
- **RedactSensitiveData** - Redacts sensitive data from strings.
            Handles emails, IDs, tokens, and other sensitive patterns.

### ISecurityAuditLogger

- **Namespace:** `SmartWorkz.Shared.Security.Audit.ISecurityAuditLogger`
- **Summary:** Interface for security audit logging with sensitive data redaction.

#### Methods & Properties

- **LogAuthenticationFailure** - Logs an authentication failure event.
  - Parameters:
    - `userId`: The user identifier attempting authentication
    - `reason`: The reason for authentication failure
- **LogSuspiciousInput** - Logs suspicious input patterns detected in user input.
  - Parameters:
    - `userId`: The user identifier who submitted the input
    - `patternType`: The type of suspicious pattern (SQL injection, XSS, Command injection, etc.)
    - `input`: The suspicious input (will be redacted in logs)
- **LogRateLimitViolation** - Logs rate limit violations.
  - Parameters:
    - `clientId`: The client identifier (IP address, API key, etc.)
    - `requestsCount`: The number of requests made
    - `limitCount`: The rate limit threshold
- **LogCertificatePinningFailure** - Logs certificate pinning validation failures.
  - Parameters:
    - `hostName`: The hostname that failed certificate pinning
    - `certificateThumbprint`: The thumbprint of the certificate presented
- **LogDataAccess** - Logs data access events for audit trail.
  - Parameters:
    - `userId`: The user accessing the data
    - `dataType`: The type of data being accessed
    - `recordId`: The record identifier being accessed
- **LogPrivilegeChange** - Logs privilege escalation attempts.
  - Parameters:
    - `userId`: The user attempting privilege change
    - `attemptedRole`: The role the user is attempting to assume
    - `allowed`: Whether the privilege change was allowed

### SecurityAuditLogger

- **Namespace:** `SmartWorkz.Shared.Security.Audit.SecurityAuditLogger`
- **Summary:** Implementation of security audit logging with sensitive data redaction.

#### Methods & Properties

- **LogAuthenticationFailure** - Logs an authentication failure event.
- **LogSuspiciousInput** - Logs suspicious input patterns detected in user input.
- **LogRateLimitViolation** - Logs rate limit violations.
- **LogCertificatePinningFailure** - Logs certificate pinning validation failures.
- **LogDataAccess** - Logs data access events for audit trail.
- **LogPrivilegeChange** - Logs privilege escalation attempts.
- **RedactSensitiveData** - Redacts sensitive data from strings.
            Handles emails, IDs, tokens, and other sensitive patterns.

### ISecurityAuditLogger

- **Namespace:** `SmartWorkz.Shared.Security.Audit.ISecurityAuditLogger`
- **Summary:** Interface for security audit logging with sensitive data redaction.

#### Methods & Properties

- **LogAuthenticationFailure** - Logs an authentication failure event.
  - Parameters:
    - `userId`: The user identifier attempting authentication
    - `reason`: The reason for authentication failure
- **LogSuspiciousInput** - Logs suspicious input patterns detected in user input.
  - Parameters:
    - `userId`: The user identifier who submitted the input
    - `patternType`: The type of suspicious pattern (SQL injection, XSS, Command injection, etc.)
    - `input`: The suspicious input (will be redacted in logs)
- **LogRateLimitViolation** - Logs rate limit violations.
  - Parameters:
    - `clientId`: The client identifier (IP address, API key, etc.)
    - `requestsCount`: The number of requests made
    - `limitCount`: The rate limit threshold
- **LogCertificatePinningFailure** - Logs certificate pinning validation failures.
  - Parameters:
    - `hostName`: The hostname that failed certificate pinning
    - `certificateThumbprint`: The thumbprint of the certificate presented
- **LogDataAccess** - Logs data access events for audit trail.
  - Parameters:
    - `userId`: The user accessing the data
    - `dataType`: The type of data being accessed
    - `recordId`: The record identifier being accessed
- **LogPrivilegeChange** - Logs privilege escalation attempts.
  - Parameters:
    - `userId`: The user attempting privilege change
    - `attemptedRole`: The role the user is attempting to assume
    - `allowed`: Whether the privilege change was allowed

### SecurityAuditLogger

- **Namespace:** `SmartWorkz.Shared.Security.Audit.SecurityAuditLogger`
- **Summary:** Implementation of security audit logging with sensitive data redaction.

#### Methods & Properties

- **LogAuthenticationFailure** - Logs an authentication failure event.
- **LogSuspiciousInput** - Logs suspicious input patterns detected in user input.
- **LogRateLimitViolation** - Logs rate limit violations.
- **LogCertificatePinningFailure** - Logs certificate pinning validation failures.
- **LogDataAccess** - Logs data access events for audit trail.
- **LogPrivilegeChange** - Logs privilege escalation attempts.
- **RedactSensitiveData** - Redacts sensitive data from strings.
            Handles emails, IDs, tokens, and other sensitive patterns.

### ISecurityAuditLogger

- **Namespace:** `SmartWorkz.Shared.Security.Audit.ISecurityAuditLogger`
- **Summary:** Interface for security audit logging with sensitive data redaction.

#### Methods & Properties

- **LogAuthenticationFailure** - Logs an authentication failure event.
  - Parameters:
    - `userId`: The user identifier attempting authentication
    - `reason`: The reason for authentication failure
- **LogSuspiciousInput** - Logs suspicious input patterns detected in user input.
  - Parameters:
    - `userId`: The user identifier who submitted the input
    - `patternType`: The type of suspicious pattern (SQL injection, XSS, Command injection, etc.)
    - `input`: The suspicious input (will be redacted in logs)
- **LogRateLimitViolation** - Logs rate limit violations.
  - Parameters:
    - `clientId`: The client identifier (IP address, API key, etc.)
    - `requestsCount`: The number of requests made
    - `limitCount`: The rate limit threshold
- **LogCertificatePinningFailure** - Logs certificate pinning validation failures.
  - Parameters:
    - `hostName`: The hostname that failed certificate pinning
    - `certificateThumbprint`: The thumbprint of the certificate presented
- **LogDataAccess** - Logs data access events for audit trail.
  - Parameters:
    - `userId`: The user accessing the data
    - `dataType`: The type of data being accessed
    - `recordId`: The record identifier being accessed
- **LogPrivilegeChange** - Logs privilege escalation attempts.
  - Parameters:
    - `userId`: The user attempting privilege change
    - `attemptedRole`: The role the user is attempting to assume
    - `allowed`: Whether the privilege change was allowed

### SecurityAuditLogger

- **Namespace:** `SmartWorkz.Shared.Security.Audit.SecurityAuditLogger`
- **Summary:** Implementation of security audit logging with sensitive data redaction.

#### Methods & Properties

- **LogAuthenticationFailure** - Logs an authentication failure event.
- **LogSuspiciousInput** - Logs suspicious input patterns detected in user input.
- **LogRateLimitViolation** - Logs rate limit violations.
- **LogCertificatePinningFailure** - Logs certificate pinning validation failures.
- **LogDataAccess** - Logs data access events for audit trail.
- **LogPrivilegeChange** - Logs privilege escalation attempts.
- **RedactSensitiveData** - Redacts sensitive data from strings.
            Handles emails, IDs, tokens, and other sensitive patterns.

### ISecurityAuditLogger

- **Namespace:** `SmartWorkz.Shared.Security.Audit.ISecurityAuditLogger`
- **Summary:** Interface for security audit logging with sensitive data redaction.

#### Methods & Properties

- **LogAuthenticationFailure** - Logs an authentication failure event.
  - Parameters:
    - `userId`: The user identifier attempting authentication
    - `reason`: The reason for authentication failure
- **LogSuspiciousInput** - Logs suspicious input patterns detected in user input.
  - Parameters:
    - `userId`: The user identifier who submitted the input
    - `patternType`: The type of suspicious pattern (SQL injection, XSS, Command injection, etc.)
    - `input`: The suspicious input (will be redacted in logs)
- **LogRateLimitViolation** - Logs rate limit violations.
  - Parameters:
    - `clientId`: The client identifier (IP address, API key, etc.)
    - `requestsCount`: The number of requests made
    - `limitCount`: The rate limit threshold
- **LogCertificatePinningFailure** - Logs certificate pinning validation failures.
  - Parameters:
    - `hostName`: The hostname that failed certificate pinning
    - `certificateThumbprint`: The thumbprint of the certificate presented
- **LogDataAccess** - Logs data access events for audit trail.
  - Parameters:
    - `userId`: The user accessing the data
    - `dataType`: The type of data being accessed
    - `recordId`: The record identifier being accessed
- **LogPrivilegeChange** - Logs privilege escalation attempts.
  - Parameters:
    - `userId`: The user attempting privilege change
    - `attemptedRole`: The role the user is attempting to assume
    - `allowed`: Whether the privilege change was allowed

### SecurityAuditLogger

- **Namespace:** `SmartWorkz.Shared.Security.Audit.SecurityAuditLogger`
- **Summary:** Implementation of security audit logging with sensitive data redaction.

#### Methods & Properties

- **LogAuthenticationFailure** - Logs an authentication failure event.
- **LogSuspiciousInput** - Logs suspicious input patterns detected in user input.
- **LogRateLimitViolation** - Logs rate limit violations.
- **LogCertificatePinningFailure** - Logs certificate pinning validation failures.
- **LogDataAccess** - Logs data access events for audit trail.
- **LogPrivilegeChange** - Logs privilege escalation attempts.
- **RedactSensitiveData** - Redacts sensitive data from strings.
            Handles emails, IDs, tokens, and other sensitive patterns.

### ISecurityAuditLogger

- **Namespace:** `SmartWorkz.Shared.Security.Audit.ISecurityAuditLogger`
- **Summary:** Interface for security audit logging with sensitive data redaction.

#### Methods & Properties

- **LogAuthenticationFailure** - Logs an authentication failure event.
  - Parameters:
    - `userId`: The user identifier attempting authentication
    - `reason`: The reason for authentication failure
- **LogSuspiciousInput** - Logs suspicious input patterns detected in user input.
  - Parameters:
    - `userId`: The user identifier who submitted the input
    - `patternType`: The type of suspicious pattern (SQL injection, XSS, Command injection, etc.)
    - `input`: The suspicious input (will be redacted in logs)
- **LogRateLimitViolation** - Logs rate limit violations.
  - Parameters:
    - `clientId`: The client identifier (IP address, API key, etc.)
    - `requestsCount`: The number of requests made
    - `limitCount`: The rate limit threshold
- **LogCertificatePinningFailure** - Logs certificate pinning validation failures.
  - Parameters:
    - `hostName`: The hostname that failed certificate pinning
    - `certificateThumbprint`: The thumbprint of the certificate presented
- **LogDataAccess** - Logs data access events for audit trail.
  - Parameters:
    - `userId`: The user accessing the data
    - `dataType`: The type of data being accessed
    - `recordId`: The record identifier being accessed
- **LogPrivilegeChange** - Logs privilege escalation attempts.
  - Parameters:
    - `userId`: The user attempting privilege change
    - `attemptedRole`: The role the user is attempting to assume
    - `allowed`: Whether the privilege change was allowed

### SecurityAuditLogger

- **Namespace:** `SmartWorkz.Shared.Security.Audit.SecurityAuditLogger`
- **Summary:** Implementation of security audit logging with sensitive data redaction.

#### Methods & Properties

- **LogAuthenticationFailure** - Logs an authentication failure event.
- **LogSuspiciousInput** - Logs suspicious input patterns detected in user input.
- **LogRateLimitViolation** - Logs rate limit violations.
- **LogCertificatePinningFailure** - Logs certificate pinning validation failures.
- **LogDataAccess** - Logs data access events for audit trail.
- **LogPrivilegeChange** - Logs privilege escalation attempts.
- **RedactSensitiveData** - Redacts sensitive data from strings.
            Handles emails, IDs, tokens, and other sensitive patterns.

### ISecurityAuditLogger

- **Namespace:** `SmartWorkz.Shared.Security.Audit.ISecurityAuditLogger`
- **Summary:** Interface for security audit logging with sensitive data redaction.

#### Methods & Properties

- **LogAuthenticationFailure** - Logs an authentication failure event.
  - Parameters:
    - `userId`: The user identifier attempting authentication
    - `reason`: The reason for authentication failure
- **LogSuspiciousInput** - Logs suspicious input patterns detected in user input.
  - Parameters:
    - `userId`: The user identifier who submitted the input
    - `patternType`: The type of suspicious pattern (SQL injection, XSS, Command injection, etc.)
    - `input`: The suspicious input (will be redacted in logs)
- **LogRateLimitViolation** - Logs rate limit violations.
  - Parameters:
    - `clientId`: The client identifier (IP address, API key, etc.)
    - `requestsCount`: The number of requests made
    - `limitCount`: The rate limit threshold
- **LogCertificatePinningFailure** - Logs certificate pinning validation failures.
  - Parameters:
    - `hostName`: The hostname that failed certificate pinning
    - `certificateThumbprint`: The thumbprint of the certificate presented
- **LogDataAccess** - Logs data access events for audit trail.
  - Parameters:
    - `userId`: The user accessing the data
    - `dataType`: The type of data being accessed
    - `recordId`: The record identifier being accessed
- **LogPrivilegeChange** - Logs privilege escalation attempts.
  - Parameters:
    - `userId`: The user attempting privilege change
    - `attemptedRole`: The role the user is attempting to assume
    - `allowed`: Whether the privilege change was allowed

### SecurityAuditLogger

- **Namespace:** `SmartWorkz.Shared.Security.Audit.SecurityAuditLogger`
- **Summary:** Implementation of security audit logging with sensitive data redaction.

#### Methods & Properties

- **LogAuthenticationFailure** - Logs an authentication failure event.
- **LogSuspiciousInput** - Logs suspicious input patterns detected in user input.
- **LogRateLimitViolation** - Logs rate limit violations.
- **LogCertificatePinningFailure** - Logs certificate pinning validation failures.
- **LogDataAccess** - Logs data access events for audit trail.
- **LogPrivilegeChange** - Logs privilege escalation attempts.
- **RedactSensitiveData** - Redacts sensitive data from strings.
            Handles emails, IDs, tokens, and other sensitive patterns.

### ISecurityAuditLogger

- **Namespace:** `SmartWorkz.Shared.Security.Audit.ISecurityAuditLogger`
- **Summary:** Interface for security audit logging with sensitive data redaction.

#### Methods & Properties

- **LogAuthenticationFailure** - Logs an authentication failure event.
  - Parameters:
    - `userId`: The user identifier attempting authentication
    - `reason`: The reason for authentication failure
- **LogSuspiciousInput** - Logs suspicious input patterns detected in user input.
  - Parameters:
    - `userId`: The user identifier who submitted the input
    - `patternType`: The type of suspicious pattern (SQL injection, XSS, Command injection, etc.)
    - `input`: The suspicious input (will be redacted in logs)
- **LogRateLimitViolation** - Logs rate limit violations.
  - Parameters:
    - `clientId`: The client identifier (IP address, API key, etc.)
    - `requestsCount`: The number of requests made
    - `limitCount`: The rate limit threshold
- **LogCertificatePinningFailure** - Logs certificate pinning validation failures.
  - Parameters:
    - `hostName`: The hostname that failed certificate pinning
    - `certificateThumbprint`: The thumbprint of the certificate presented
- **LogDataAccess** - Logs data access events for audit trail.
  - Parameters:
    - `userId`: The user accessing the data
    - `dataType`: The type of data being accessed
    - `recordId`: The record identifier being accessed
- **LogPrivilegeChange** - Logs privilege escalation attempts.
  - Parameters:
    - `userId`: The user attempting privilege change
    - `attemptedRole`: The role the user is attempting to assume
    - `allowed`: Whether the privilege change was allowed

### SecurityAuditLogger

- **Namespace:** `SmartWorkz.Shared.Security.Audit.SecurityAuditLogger`
- **Summary:** Implementation of security audit logging with sensitive data redaction.

#### Methods & Properties

- **LogAuthenticationFailure** - Logs an authentication failure event.
- **LogSuspiciousInput** - Logs suspicious input patterns detected in user input.
- **LogRateLimitViolation** - Logs rate limit violations.
- **LogCertificatePinningFailure** - Logs certificate pinning validation failures.
- **LogDataAccess** - Logs data access events for audit trail.
- **LogPrivilegeChange** - Logs privilege escalation attempts.
- **RedactSensitiveData** - Redacts sensitive data from strings.
            Handles emails, IDs, tokens, and other sensitive patterns.

