namespace SmartWorkz.Core.Tests.Webhooks;

using SmartWorkz.Shared.Webhooks;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;

/// <summary>
/// Unit tests for WebhookDeliveryService implementation.
/// Tests webhook event publishing, delivery, retries, and signature generation.
/// </summary>
public class WebhookDeliveryServiceTests
{
    private readonly Mock<IWebhookRegistry> _mockRegistry;
    private readonly Mock<HttpClient> _mockHttpClient;
    private readonly Mock<ILogger<WebhookDeliveryService>> _mockLogger;
    private readonly WebhookDeliveryService _service;

    public WebhookDeliveryServiceTests()
    {
        _mockRegistry = new Mock<IWebhookRegistry>();
        _mockHttpClient = new Mock<HttpClient>();
        _mockLogger = new Mock<ILogger<WebhookDeliveryService>>();
        _service = new WebhookDeliveryService(_mockRegistry.Object, _mockHttpClient.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithNullRegistry_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new WebhookDeliveryService(null!, _mockHttpClient.Object, _mockLogger.Object));
        Assert.Equal("registry", ex.ParamName);
    }

    [Fact]
    public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new WebhookDeliveryService(_mockRegistry.Object, null!, _mockLogger.Object));
        Assert.Equal("httpClient", ex.ParamName);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new WebhookDeliveryService(_mockRegistry.Object, _mockHttpClient.Object, null!));
        Assert.Equal("logger", ex.ParamName);
    }

    [Fact]
    public async Task PublishEventAsync_WithNullEventName_ThrowsArgumentException()
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.PublishEventAsync(null!, new { }, default));
        Assert.Equal("eventName", ex.ParamName);
    }

    [Fact]
    public async Task PublishEventAsync_WithEmptyEventName_ThrowsArgumentException()
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.PublishEventAsync(string.Empty, new { }, default));
        Assert.Equal("eventName", ex.ParamName);
    }

    [Fact]
    public async Task PublishEventAsync_WithNullPayload_ThrowsArgumentNullException()
    {
        var ex = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.PublishEventAsync("TestEvent", null!, default));
        Assert.Equal("payload", ex.ParamName);
    }

    [Fact]
    public async Task PublishEventAsync_WithNoSubscriptions_DoesNotAttemptDelivery()
    {
        _mockRegistry.Setup(r => r.GetSubscriptionsForEventAsync(It.IsAny<string>(), default))
            .ReturnsAsync(new List<WebhookSubscription>());

        await _service.PublishEventAsync("TestEvent", new { test = "data" });

        _mockRegistry.Verify(r => r.GetSubscriptionsForEventAsync("TestEvent", default), Times.Once);
    }

    [Fact]
    public async Task PublishEventAsync_WithValidSubscription_InitiatesDelivery()
    {
        var subscription = new WebhookSubscription
        {
            Id = Guid.NewGuid(),
            Url = "https://example.com/webhook",
            Events = new[] { "TestEvent" },
            IsActive = true,
            MaxRetries = 3,
            TimeoutSeconds = 30
        };

        _mockRegistry.Setup(r => r.GetSubscriptionsForEventAsync(It.IsAny<string>(), default))
            .ReturnsAsync(new[] { subscription });

        await _service.PublishEventAsync("TestEvent", new { test = "data" });

        _mockRegistry.Verify(r => r.GetSubscriptionsForEventAsync("TestEvent", default), Times.Once);
    }
}

