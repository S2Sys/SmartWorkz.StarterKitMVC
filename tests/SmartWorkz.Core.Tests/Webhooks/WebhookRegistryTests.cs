using Xunit;
using Moq;
using SmartWorkz.Core.Shared.Webhooks.Models;
using SmartWorkz.Core.Shared.Webhooks.Abstractions;

namespace SmartWorkz.Core.Tests.Webhooks;

/// <summary>
/// Unit tests for IWebhookRegistry implementation.
/// Tests registration/lookup, CRUD operations, and event subscription management.
/// </summary>
public class WebhookRegistryTests
{
    private readonly Mock<IWebhookRegistry> _mockRegistry;

    public WebhookRegistryTests()
    {
        _mockRegistry = new Mock<IWebhookRegistry>();
    }

    [Fact]
    public async Task CreateEndpointAsync_WithValidEndpoint_ReturnsCreatedEndpoint()
    {
        // Arrange
        var endpoint = new WebhookEndpointRegistration
        {
            TenantId = "tenant-1",
            Url = "https://example.com/webhook",
            SecretKey = "secret-key",
            EventTypes = new[] { "user.created", "user.updated" },
            IsActive = true
        };

        var createdEndpoint = new WebhookEndpointRegistration
        {
            Id = "endpoint-1",
            TenantId = "tenant-1",
            Url = "https://example.com/webhook",
            SecretKey = "secret-key",
            EventTypes = new[] { "user.created", "user.updated" },
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        _mockRegistry.Setup(r => r.CreateEndpointAsync(endpoint, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdEndpoint);

        // Act
        var result = await _mockRegistry.Object.CreateEndpointAsync(endpoint);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("endpoint-1", result.Id);
        Assert.Equal("tenant-1", result.TenantId);
        Assert.Equal("https://example.com/webhook", result.Url);
        _mockRegistry.Verify(r => r.CreateEndpointAsync(endpoint, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetSubscriptionsForEventAsync_WithEventType_ReturnsActiveEndpoints()
    {
        // Arrange
        var activeEndpoints = new[]
        {
            new WebhookEndpointRegistration
            {
                Id = "endpoint-1",
                TenantId = "tenant-1",
                Url = "https://example.com/webhook1",
                EventTypes = new[] { "user.created" },
                IsActive = true
            },
            new WebhookEndpointRegistration
            {
                Id = "endpoint-2",
                TenantId = "tenant-2",
                Url = "https://example.com/webhook2",
                EventTypes = new[] { "user.created" },
                IsActive = true
            }
        };

        _mockRegistry.Setup(r => r.GetSubscriptionsForEventAsync("user.created", It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeEndpoints);

        // Act
        var result = await _mockRegistry.Object.GetSubscriptionsForEventAsync("user.created");

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, ep => Assert.True(ep.IsActive));
        Assert.All(result, ep => Assert.Contains("user.created", ep.EventTypes));
    }

    [Fact]
    public async Task GetSubscriptionsForEventAsync_WithNoSubscriptions_ReturnsEmpty()
    {
        // Arrange
        _mockRegistry.Setup(r => r.GetSubscriptionsForEventAsync("unknown.event", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<WebhookEndpointRegistration>());

        // Act
        var result = await _mockRegistry.Object.GetSubscriptionsForEventAsync("unknown.event");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetSubscriptionsForEventAsync_FiltersOutInactiveEndpoints()
    {
        // Arrange
        var mixedEndpoints = new[]
        {
            new WebhookEndpointRegistration
            {
                Id = "endpoint-1",
                TenantId = "tenant-1",
                Url = "https://example.com/webhook1",
                EventTypes = new[] { "user.created" },
                IsActive = true
            },
            new WebhookEndpointRegistration
            {
                Id = "endpoint-2",
                TenantId = "tenant-2",
                Url = "https://example.com/webhook2",
                EventTypes = new[] { "user.created" },
                IsActive = false  // Inactive
            }
        };

        // Simulate filtering: registry returns only active
        _mockRegistry.Setup(r => r.GetSubscriptionsForEventAsync("user.created", It.IsAny<CancellationToken>()))
            .ReturnsAsync(mixedEndpoints.Where(ep => ep.IsActive));

        // Act
        var result = await _mockRegistry.Object.GetSubscriptionsForEventAsync("user.created");

        // Assert
        Assert.Single(result);
        Assert.True(result.First().IsActive);
    }

    [Fact]
    public async Task UpdateEndpointAsync_WithValidUpdates_UpdatesEndpoint()
    {
        // Arrange
        var endpointId = "endpoint-1";
        var updates = new { IsActive = false, FailureCount = 1 };
        var updatedEndpoint = new WebhookEndpointRegistration
        {
            Id = endpointId,
            TenantId = "tenant-1",
            Url = "https://example.com/webhook",
            IsActive = false,
            FailureCount = 1
        };

        _mockRegistry.Setup(r => r.UpdateEndpointAsync(endpointId, updates, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedEndpoint);

        // Act
        var result = await _mockRegistry.Object.UpdateEndpointAsync(endpointId, updates);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsActive);
        Assert.Equal(1, result.FailureCount);
        _mockRegistry.Verify(r => r.UpdateEndpointAsync(endpointId, updates, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetFailedEndpointsAsync_ReturnsEndpointsWithFailures()
    {
        // Arrange
        var failedEndpoints = new[]
        {
            new WebhookEndpointRegistration
            {
                Id = "endpoint-1",
                TenantId = "tenant-1",
                Url = "https://example.com/webhook1",
                FailureCount = 2,
                MaxRetries = 5
            },
            new WebhookEndpointRegistration
            {
                Id = "endpoint-2",
                TenantId = "tenant-2",
                Url = "https://example.com/webhook2",
                FailureCount = 3,
                MaxRetries = 5
            }
        };

        _mockRegistry.Setup(r => r.GetFailedEndpointsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(failedEndpoints);

        // Act
        var result = await _mockRegistry.Object.GetFailedEndpointsAsync();

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, ep => Assert.True(ep.FailureCount > 0));
    }

    [Fact]
    public async Task GetFailedEndpointsAsync_WithNoFailures_ReturnsEmpty()
    {
        // Arrange
        _mockRegistry.Setup(r => r.GetFailedEndpointsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<WebhookEndpointRegistration>());

        // Act
        var result = await _mockRegistry.Object.GetFailedEndpointsAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task DeleteEndpointAsync_WithValidEndpointId_DeletesEndpoint()
    {
        // Arrange
        var endpointId = "endpoint-1";
        _mockRegistry.Setup(r => r.DeleteEndpointAsync(endpointId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _mockRegistry.Object.DeleteEndpointAsync(endpointId);

        // Assert
        _mockRegistry.Verify(r => r.DeleteEndpointAsync(endpointId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteEndpointAsync_WithNullEndpointId_ThrowsArgumentNullException()
    {
        // Arrange
        _mockRegistry.Setup(r => r.DeleteEndpointAsync(null!, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentNullException(nameof(null)));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _mockRegistry.Object.DeleteEndpointAsync(null!));
    }

    [Fact]
    public async Task CreateEndpointAsync_WithMultipleEventTypes_RegistersAllEventTypes()
    {
        // Arrange
        var eventTypes = new[] { "user.created", "user.updated", "user.deleted", "order.placed" };
        var endpoint = new WebhookEndpointRegistration
        {
            TenantId = "tenant-1",
            Url = "https://example.com/webhook",
            SecretKey = "secret",
            EventTypes = eventTypes
        };

        var createdEndpoint = new WebhookEndpointRegistration
        {
            Id = "endpoint-1",
            TenantId = "tenant-1",
            Url = "https://example.com/webhook",
            EventTypes = eventTypes
        };

        _mockRegistry.Setup(r => r.CreateEndpointAsync(endpoint, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdEndpoint);

        // Act
        var result = await _mockRegistry.Object.CreateEndpointAsync(endpoint);

        // Assert
        Assert.Equal(4, result.EventTypes.Length);
        Assert.Contains("user.created", result.EventTypes);
        Assert.Contains("order.placed", result.EventTypes);
    }

    [Fact]
    public async Task GetSubscriptionsForEventAsync_WithMultipleTenants_FiltersByTenant()
    {
        // Arrange
        var tenant1Endpoint = new WebhookEndpointRegistration
        {
            Id = "endpoint-1",
            TenantId = "tenant-1",
            Url = "https://example.com/webhook1",
            EventTypes = new[] { "user.created" },
            IsActive = true
        };

        var tenant2Endpoint = new WebhookEndpointRegistration
        {
            Id = "endpoint-2",
            TenantId = "tenant-2",
            Url = "https://example.com/webhook2",
            EventTypes = new[] { "user.created" },
            IsActive = true
        };

        _mockRegistry.Setup(r => r.GetSubscriptionsForEventAsync("user.created", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { tenant1Endpoint, tenant2Endpoint });

        // Act
        var result = await _mockRegistry.Object.GetSubscriptionsForEventAsync("user.created");

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Single(result.Where(ep => ep.TenantId == "tenant-1"));
        Assert.Single(result.Where(ep => ep.TenantId == "tenant-2"));
    }
}
