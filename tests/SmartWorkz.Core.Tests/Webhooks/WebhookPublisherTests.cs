using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using SmartWorkz.Core.Shared.Webhooks.Models;
using SmartWorkz.Core.Shared.Webhooks.Implementations;
using SmartWorkz.Core.Shared.Webhooks.Abstractions;

namespace SmartWorkz.Core.Tests.Webhooks;

/// <summary>
/// Unit tests for WebhookPublisher implementation.
/// </summary>
public class WebhookPublisherTests
{
    private readonly Mock<IWebhookRegistry> _mockRegistry;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<ILogger<WebhookPublisher>> _mockLogger;
    private readonly WebhookPublisher _publisher;

    public WebhookPublisherTests()
    {
        _mockRegistry = new Mock<IWebhookRegistry>();
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockLogger = new Mock<ILogger<WebhookPublisher>>();
        _publisher = new WebhookPublisher(_mockRegistry.Object, _mockHttpClientFactory.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithNullRegistry_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new WebhookPublisher(null!, _mockHttpClientFactory.Object, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullHttpClientFactory_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new WebhookPublisher(_mockRegistry.Object, null!, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new WebhookPublisher(_mockRegistry.Object, _mockHttpClientFactory.Object, null!));
    }

    [Fact]
    public async Task PublishAsync_WithNullEvent_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _publisher.PublishAsync(null!));
    }

    [Fact]
    public async Task PublishAsync_WithNoSubscriptions_DoesNotAttemptDelivery()
    {
        // Arrange
        var @event = new UserCreatedEvent("user-123", "user@example.com", "John", "Doe")
        {
            TenantId = "tenant-1"
        };
        _mockRegistry.Setup(r => r.GetSubscriptionsForEventAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<WebhookEndpointRegistration>());

        // Act
        await _publisher.PublishAsync(@event);

        // Assert
        _mockRegistry.Verify(r => r.GetSubscriptionsForEventAsync("user.created", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_WithInactiveEndpoints_SkipsDelivery()
    {
        // Arrange
        var @event = new UserCreatedEvent("user-123", "user@example.com", "John", "Doe")
        {
            TenantId = "tenant-1"
        };
        var inactiveEndpoint = new WebhookEndpointRegistration
        {
            Id = "endpoint-1",
            TenantId = "tenant-1",
            Url = "https://example.com/webhook",
            SecretKey = "secret",
            EventTypes = new[] { "user.created" },
            IsActive = false
        };
        _mockRegistry.Setup(r => r.GetSubscriptionsForEventAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { inactiveEndpoint });

        // Act
        await _publisher.PublishAsync(@event);

        // Assert
        // Should not attempt delivery to inactive endpoint
        _mockHttpClientFactory.Verify(f => f.CreateClient(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task PublishAsync_WithWrongTenant_SkipsDelivery()
    {
        // Arrange
        var @event = new UserCreatedEvent("user-123", "user@example.com", "John", "Doe")
        {
            TenantId = "tenant-1"
        };
        var otherTenantEndpoint = new WebhookEndpointRegistration
        {
            Id = "endpoint-1",
            TenantId = "tenant-2",  // Different tenant
            Url = "https://example.com/webhook",
            SecretKey = "secret",
            EventTypes = new[] { "user.created" },
            IsActive = true
        };
        _mockRegistry.Setup(r => r.GetSubscriptionsForEventAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { otherTenantEndpoint });

        // Act
        await _publisher.PublishAsync(@event);

        // Assert
        // Should not attempt delivery to other tenant's endpoint
        _mockHttpClientFactory.Verify(f => f.CreateClient(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task PublishToEndpointAsync_WithNullEndpoint_ThrowsArgumentNullException()
    {
        // Arrange
        var @event = new UserCreatedEvent("user-123", "user@example.com", "John", "Doe");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _publisher.PublishToEndpointAsync(null!, @event));
    }

    [Fact]
    public async Task PublishToEndpointAsync_WithNullEvent_ThrowsArgumentNullException()
    {
        // Arrange
        var endpoint = new WebhookEndpointRegistration
        {
            Id = "endpoint-1",
            TenantId = "tenant-1",
            Url = "https://example.com/webhook",
            SecretKey = "secret"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _publisher.PublishToEndpointAsync(endpoint, null!));
    }

    [Fact]
    public async Task PublishToEndpointAsync_WithMismatchedTenant_ThrowsArgumentException()
    {
        // Arrange
        var endpoint = new WebhookEndpointRegistration
        {
            Id = "endpoint-1",
            TenantId = "tenant-1",
            Url = "https://example.com/webhook",
            SecretKey = "secret"
        };
        var @event = new UserCreatedEvent("user-123", "user@example.com", "John", "Doe")
        {
            TenantId = "tenant-2"  // Different tenant
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _publisher.PublishToEndpointAsync(endpoint, @event));
    }

    [Fact]
    public async Task PublishToEndpointAsync_WithValidEndpointAndEvent_ConfiguresHttpClient()
    {
        // Arrange
        var mockHttpClient = new Mock<HttpClient>();
        var mockResponse = new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.OK };

        _mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(mockHttpClient.Object);

        mockHttpClient.Setup(c => c.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse);

        var endpoint = new WebhookEndpointRegistration
        {
            Id = "endpoint-1",
            TenantId = "tenant-1",
            Url = "https://example.com/webhook",
            SecretKey = "secret",
            TimeoutSeconds = 15
        };

        var @event = new UserCreatedEvent("user-123", "user@example.com", "John", "Doe")
        {
            TenantId = "tenant-1"
        };

        _mockRegistry.Setup(r => r.UpdateEndpointAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(endpoint);

        // Act
        await _publisher.PublishToEndpointAsync(endpoint, @event);

        // Assert
        _mockHttpClientFactory.Verify(f => f.CreateClient(It.IsAny<string>()), Times.Once);
        Assert.Equal(TimeSpan.FromSeconds(15), mockHttpClient.Object.Timeout);
    }

    [Fact]
    public async Task PublishToEndpointAsync_WithSuccessfulDelivery_ResetsFailureCount()
    {
        // Arrange
        var mockHttpClient = new Mock<HttpClient>();
        var mockResponse = new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.OK };

        _mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(mockHttpClient.Object);

        mockHttpClient.Setup(c => c.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse);

        var endpoint = new WebhookEndpointRegistration
        {
            Id = "endpoint-1",
            TenantId = "tenant-1",
            Url = "https://example.com/webhook",
            SecretKey = "secret",
            FailureCount = 3  // Had previous failures
        };

        var @event = new UserCreatedEvent("user-123", "user@example.com", "John", "Doe")
        {
            TenantId = "tenant-1"
        };

        _mockRegistry.Setup(r => r.UpdateEndpointAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(endpoint);

        // Act
        await _publisher.PublishToEndpointAsync(endpoint, @event);

        // Assert
        _mockRegistry.Verify(
            r => r.UpdateEndpointAsync(
                endpoint.Id,
                It.Is<object>(o => o.ToString()!.Contains("FailureCount") && o.ToString()!.Contains("0")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PublishToEndpointAsync_WithFailedDelivery_IncrementsFailureCount()
    {
        // Arrange
        var mockHttpClient = new Mock<HttpClient>();
        var mockResponse = new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.InternalServerError };

        _mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(mockHttpClient.Object);

        mockHttpClient.Setup(c => c.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse);

        var endpoint = new WebhookEndpointRegistration
        {
            Id = "endpoint-1",
            TenantId = "tenant-1",
            Url = "https://example.com/webhook",
            SecretKey = "secret",
            FailureCount = 0,
            MaxRetries = 5
        };

        var @event = new UserCreatedEvent("user-123", "user@example.com", "John", "Doe")
        {
            TenantId = "tenant-1"
        };

        _mockRegistry.Setup(r => r.UpdateEndpointAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(endpoint);

        // Act
        await _publisher.PublishToEndpointAsync(endpoint, @event);

        // Assert
        _mockRegistry.Verify(
            r => r.UpdateEndpointAsync(
                endpoint.Id,
                It.Is<object>(o => o.ToString()!.Contains("FailureCount") && o.ToString()!.Contains("1")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PublishToEndpointAsync_ExceedingMaxRetries_DisablesEndpoint()
    {
        // Arrange
        var mockHttpClient = new Mock<HttpClient>();
        var mockResponse = new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.InternalServerError };

        _mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(mockHttpClient.Object);

        mockHttpClient.Setup(c => c.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse);

        var endpoint = new WebhookEndpointRegistration
        {
            Id = "endpoint-1",
            TenantId = "tenant-1",
            Url = "https://example.com/webhook",
            SecretKey = "secret",
            FailureCount = 5,  // Already at max retries
            MaxRetries = 5,
            IsActive = true
        };

        var @event = new UserCreatedEvent("user-123", "user@example.com", "John", "Doe")
        {
            TenantId = "tenant-1"
        };

        _mockRegistry.Setup(r => r.UpdateEndpointAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(endpoint);

        // Act
        await _publisher.PublishToEndpointAsync(endpoint, @event);

        // Assert
        _mockRegistry.Verify(
            r => r.UpdateEndpointAsync(
                endpoint.Id,
                It.Is<object>(o => o.ToString()!.Contains("IsActive") && o.ToString()!.Contains("False")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
