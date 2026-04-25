using System;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using SmartWorkz.Shared.Security.Audit;

namespace SmartWorkz.Core.Tests.Security.Audit
{
    public class SecurityAuditLoggerTests
    {
        private readonly Mock<ILogger<SecurityAuditLogger>> _mockLogger;
        private readonly SecurityAuditLogger _sut;

        public SecurityAuditLoggerTests()
        {
            _mockLogger = new Mock<ILogger<SecurityAuditLogger>>();
            _sut = new SecurityAuditLogger(_mockLogger.Object);
        }

        [Fact]
        public void LogAuthenticationFailure_CapturesEvent()
        {
            // Arrange
            var userId = "user123";
            var reason = "Invalid password";

            // Act
            _sut.LogAuthenticationFailure(userId, reason);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Authentication failure")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogSuspiciousInput_RedactsSensitiveData()
        {
            // Arrange
            var userId = "user@example.com";
            var patternType = "SQL_INJECTION";
            var suspiciousInput = "'; DROP TABLE users; --";

            // Act
            _sut.LogSuspiciousInput(userId, patternType, suspiciousInput);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString().Contains("[REDACTED_EMAIL]") &&
                        v.ToString().Contains("[REDACTED") &&
                        !v.ToString().Contains(userId) &&
                        !v.ToString().Contains(suspiciousInput)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogRateLimitViolation_CapturesViolation()
        {
            // Arrange
            var clientId = "192.168.1.100";
            var requestsCount = 150;
            var limitCount = 100;

            // Act
            _sut.LogRateLimitViolation(clientId, requestsCount, limitCount);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString().Contains("Rate limit violation") &&
                        v.ToString().Contains("150") &&
                        v.ToString().Contains("100")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogCertificatePinningFailure_AlertsSecurityTeam()
        {
            // Arrange
            var hostName = "api.example.com";
            var certificateThumbprint = "3F2504E0-4F89-41D3-9A0C-0305E8EA9698";

            // Act
            _sut.LogCertificatePinningFailure(hostName, certificateThumbprint);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Critical,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString().Contains("Certificate pinning validation failed") &&
                        v.ToString().Contains("api.example.com") &&
                        v.ToString().Contains("[REDACTED")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogDataAccess_TracksAccess()
        {
            // Arrange
            var userId = "user456";
            var dataType = "CustomerProfile";
            var recordId = "cust_12345678";

            // Act
            _sut.LogDataAccess(userId, dataType, recordId);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString().Contains("Data access") &&
                        v.ToString().Contains("CustomerProfile") &&
                        !v.ToString().Contains(recordId)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogPrivilegeChange_TracksEscalation()
        {
            // Arrange
            var userId = "user789";
            var attemptedRole = "Admin";
            var allowed = true;

            // Act
            _sut.LogPrivilegeChange(userId, attemptedRole, allowed);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString().Contains("Privilege escalation granted") &&
                        v.ToString().Contains("Admin") &&
                        !v.ToString().Contains(userId)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogPrivilegeChange_WarnsOnDeniedEscalation()
        {
            // Arrange
            var userId = "user999";
            var attemptedRole = "SuperAdmin";
            var allowed = false;

            // Act
            _sut.LogPrivilegeChange(userId, attemptedRole, allowed);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString().Contains("Privilege escalation denied") &&
                        v.ToString().Contains("SuperAdmin")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Theory]
        [InlineData("user@example.com", "[REDACTED_EMAIL]")]
        [InlineData("3F2504E0-4F89-41D3-9A0C-0305E8EA9698", "[REDACTED_ID]")]
        [InlineData("token123456789abcdefghijklmnop.token123456789abcdefghijklmnop.token123456789abcdefghijklmnop", "[REDACTED_TOKEN]")]
        [InlineData("api_key=sk_test_abcdefghijklmnopqrstuvwxyz", "[REDACTED]")]
        public void RedactSensitiveData_HandlesVariousPatterns(string input, string expectedRedaction)
        {
            // Arrange
            var userId = input;
            var reason = "Test";

            // Act
            _sut.LogAuthenticationFailure(userId, reason);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString().Contains(expectedRedaction) &&
                        !v.ToString().Contains(input)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
