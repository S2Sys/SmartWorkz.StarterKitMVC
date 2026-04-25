using Xunit;
using SmartWorkz.Core.Shared.Webhooks.Models;
using SmartWorkz.Core.Shared.Webhooks.Security;

namespace SmartWorkz.Core.Tests.Webhooks;

/// <summary>
/// Unit tests for WebhookSignature cryptographic signing and verification.
/// </summary>
public class WebhookSignatureTests
{
    [Fact]
    public void Sign_WithValidEvent_ReturnsHexEncodedSignature()
    {
        // Arrange
        var @event = new UserCreatedEvent("user-123", "user@example.com", "John", "Doe")
        {
            TenantId = "tenant-1"
        };
        var secretKey = "super-secret-key";

        // Act
        var signature = WebhookSignature.Sign(@event, secretKey);

        // Assert
        Assert.NotEmpty(signature);
        Assert.True(signature.All(c => "0123456789abcdef".Contains(c)), "Signature should be valid hex");
    }

    [Fact]
    public void Sign_WithDifferentSecretKeys_ProducesDifferentSignatures()
    {
        // Arrange
        var @event = new UserCreatedEvent("user-123", "user@example.com", "John", "Doe")
        {
            TenantId = "tenant-1"
        };

        // Act
        var signature1 = WebhookSignature.Sign(@event, "secret-1");
        var signature2 = WebhookSignature.Sign(@event, "secret-2");

        // Assert
        Assert.NotEqual(signature1, signature2);
    }

    [Fact]
    public void Sign_WithSameEventAndSecret_ProducesSameSignature()
    {
        // Arrange
        var @event = new UserCreatedEvent("user-123", "user@example.com", "John", "Doe")
        {
            TenantId = "tenant-1"
        };
        var secretKey = "secret-key";

        // Act
        var signature1 = WebhookSignature.Sign(@event, secretKey);
        var signature2 = WebhookSignature.Sign(@event, secretKey);

        // Assert
        Assert.Equal(signature1, signature2);
    }

    [Fact]
    public void Verify_WithValidSignature_ReturnsTrue()
    {
        // Arrange
        var @event = new UserCreatedEvent("user-123", "user@example.com", "John", "Doe")
        {
            TenantId = "tenant-1"
        };
        var secretKey = "secret-key";
        var signature = WebhookSignature.Sign(@event, secretKey);

        // Act
        var result = WebhookSignature.Verify(@event, signature, secretKey);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Verify_WithInvalidSignature_ReturnsFalse()
    {
        // Arrange
        var @event = new UserCreatedEvent("user-123", "user@example.com", "John", "Doe")
        {
            TenantId = "tenant-1"
        };
        var secretKey = "secret-key";
        var invalidSignature = "0000000000000000000000000000000000000000000000000000000000000000";

        // Act
        var result = WebhookSignature.Verify(@event, invalidSignature, secretKey);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Verify_WithWrongSecretKey_ReturnsFalse()
    {
        // Arrange
        var @event = new UserCreatedEvent("user-123", "user@example.com", "John", "Doe")
        {
            TenantId = "tenant-1"
        };
        var secretKey = "secret-key";
        var signature = WebhookSignature.Sign(@event, secretKey);

        // Act
        var result = WebhookSignature.Verify(@event, signature, "wrong-secret-key");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Verify_WithSignatureCaseInsensitivity_ReturnsTrue()
    {
        // Arrange
        var @event = new UserCreatedEvent("user-123", "user@example.com", "John", "Doe")
        {
            TenantId = "tenant-1"
        };
        var secretKey = "secret-key";
        var signature = WebhookSignature.Sign(@event, secretKey);
        var uppercaseSignature = signature.ToUpper();

        // Act
        var result = WebhookSignature.Verify(@event, uppercaseSignature, secretKey);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Sign_WithJsonString_ReturnsSignature()
    {
        // Arrange
        var json = """{"id":"123","type":"user.created","timestamp":"2024-01-01T00:00:00Z"}""";
        var secretKey = "secret-key";

        // Act
        var signature = WebhookSignature.Sign(json, secretKey);

        // Assert
        Assert.NotEmpty(signature);
        Assert.True(signature.All(c => "0123456789abcdef".Contains(c)));
    }

    [Fact]
    public void Sign_WithNullEvent_ThrowsArgumentNullException()
    {
        // Arrange
        WebhookEvent nullEvent = null!;
        var secretKey = "secret-key";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => WebhookSignature.Sign(nullEvent, secretKey));
    }

    [Fact]
    public void Sign_WithNullJsonString_ThrowsArgumentNullException()
    {
        // Arrange
        string nullJson = null!;
        var secretKey = "secret-key";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => WebhookSignature.Sign(nullJson, secretKey));
    }

    [Fact]
    public void Sign_WithNullSecretKey_ThrowsArgumentNullException()
    {
        // Arrange
        var @event = new UserCreatedEvent("user-123", "user@example.com", "John", "Doe");
        string nullSecretKey = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => WebhookSignature.Sign(@event, nullSecretKey));
    }

    [Fact]
    public void Verify_WithNullEvent_ThrowsArgumentNullException()
    {
        // Arrange
        WebhookEvent nullEvent = null!;
        var signature = "somesignature";
        var secretKey = "secret-key";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => WebhookSignature.Verify(nullEvent, signature, secretKey));
    }

    [Fact]
    public void Verify_WithModifiedEventData_ReturnsFalse()
    {
        // Arrange
        var @event = new UserCreatedEvent("user-123", "user@example.com", "John", "Doe")
        {
            TenantId = "tenant-1"
        };
        var secretKey = "secret-key";
        var signature = WebhookSignature.Sign(@event, secretKey);

        // Create modified event
        var modifiedEvent = new UserCreatedEvent("user-999", "modified@example.com", "Jane", "Smith")
        {
            TenantId = "tenant-1"
        };

        // Act
        var result = WebhookSignature.Verify(modifiedEvent, signature, secretKey);

        // Assert - signature should not match modified event
        Assert.False(result);
    }
}
