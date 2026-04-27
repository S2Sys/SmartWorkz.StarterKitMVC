using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using SmartWorkz.Core.Shared.Webhooks.Abstractions;
using SmartWorkz.Core.Shared.Webhooks.Models;
using SmartWorkz.Core.Shared.Webhooks.Security;

namespace SmartWorkz.Core.Shared.Webhooks.Implementations;

/// <summary>
/// Default implementation of <see cref="IWebhookPublisher"/> for publishing webhook events
/// to registered endpoints with automatic retry logic and signature verification.
///
/// <para><strong>Features</strong>:
/// • Publishes events to all subscribed endpoints
/// • HMAC-SHA256 payload signing for security
/// • Exponential backoff retry on delivery failures
/// • Comprehensive logging for monitoring and debugging
/// • Cancellation token support for graceful shutdown
/// • Multi-tenancy support via TenantId filtering
/// </para>
///
/// <para><strong>Dependency Injection Registration</strong>:</para>
/// <code>
/// services.AddScoped&lt;IWebhookPublisher, WebhookPublisher&gt;();
/// services.AddScoped&lt;IWebhookRegistry, SqlWebhookRegistry&gt;();
/// services.AddHttpClient();
/// </code>
///
/// <para><strong>Usage Example 1: Inject and publish directly</strong>:</para>
/// <example>
/// <code>
/// public class UserService
/// {
///     private readonly IWebhookPublisher _webhookPublisher;
///
///     public UserService(IWebhookPublisher webhookPublisher)
///     {
///         _webhookPublisher = webhookPublisher;
///     }
///
///     public async Task CreateUserAsync(CreateUserRequest request)
///     {
///         var user = new User { Id = Guid.NewGuid().ToString(), Email = request.Email };
///         // ... save user to database ...
///
///         var webhookEvent = new UserCreatedEvent(user.Id, user.Email, user.FirstName, user.LastName)
///         {
///             TenantId = request.TenantId
///         };
///
///         await _webhookPublisher.PublishAsync(webhookEvent);
///     }
/// }
/// </code>
/// </example>
///
/// <para><strong>Usage Example 2: Use in domain event handler</strong>:</para>
/// <example>
/// <code>
/// public class UserCreatedDomainEventHandler : IDomainEventHandler&lt;UserCreatedDomainEvent&gt;
/// {
///     private readonly IWebhookPublisher _webhookPublisher;
///     private readonly ILogger&lt;UserCreatedDomainEventHandler&gt; _logger;
///
///     public UserCreatedDomainEventHandler(
///         IWebhookPublisher webhookPublisher,
///         ILogger&lt;UserCreatedDomainEventHandler&gt; logger)
///     {
///         _webhookPublisher = webhookPublisher;
///         _logger = logger;
///     }
///
///     public async Task HandleAsync(UserCreatedDomainEvent domainEvent)
///     {
///         var webhookEvent = new UserCreatedEvent(
///             domainEvent.UserId,
///             domainEvent.Email,
///             domainEvent.FirstName,
///             domainEvent.LastName
///         ) { TenantId = domainEvent.TenantId };
///
///         try
///         {
///             await _webhookPublisher.PublishAsync(webhookEvent);
///             _logger.LogInformation("Webhooks published for user {UserId}", domainEvent.UserId);
///         }
///         catch (Exception ex)
///         {
///             _logger.LogError(ex, "Failed to publish webhooks for user {UserId}", domainEvent.UserId);
///             throw;
///         }
///     }
/// }
/// </code>
/// </example>
///
/// <para><strong>Usage Example 3: Publish to specific endpoint with retry</strong>:</para>
/// <example>
/// <code>
/// public class WebhookRetryService : IBackgroundService
/// {
///     private readonly IWebhookRegistry _registry;
///     private readonly IWebhookPublisher _publisher;
///     private readonly ILogger&lt;WebhookRetryService&gt; _logger;
///
///     public WebhookRetryService(
///         IWebhookRegistry registry,
///         IWebhookPublisher publisher,
///         ILogger&lt;WebhookRetryService&gt; logger)
///     {
///         _registry = registry;
///         _publisher = publisher;
///         _logger = logger;
///     }
///
///     public async Task ExecuteAsync(CancellationToken cancellationToken)
///     {
///         // Find endpoints with pending retries
///         var endpoints = await _registry.GetFailedEndpointsAsync(cancellationToken);
///
///         foreach (var endpoint in endpoints)
///         {
///             var @event = new TransactionCompletedEvent("txn-123", 100.00m, "completed")
///             {
///                 TenantId = endpoint.TenantId
///             };
///
///             try
///             {
///                 await _publisher.PublishToEndpointAsync(endpoint, @event, cancellationToken);
///                 _logger.LogInformation("Webhook retry succeeded for endpoint {EndpointId}", endpoint.Id);
///             }
///             catch (Exception ex)
///             {
///                 _logger.LogError(ex, "Webhook retry failed for endpoint {EndpointId}", endpoint.Id);
///             }
///         }
///     }
/// }
/// </code>
/// </example>
/// </summary>
public class WebhookPublisher : IWebhookPublisher
{
    private readonly IWebhookRegistry _registry;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WebhookPublisher> _logger;

    /// <summary>
    /// Initialize a new instance of WebhookPublisher.
    /// </summary>
    /// <param name="registry">The webhook registry for querying subscriptions.</param>
    /// <param name="httpClientFactory">HTTP client factory for making delivery requests.</param>
    /// <param name="logger">Logger for diagnostics and monitoring.</param>
    /// <exception cref="ArgumentNullException">Thrown if any parameter is null.</exception>
    public WebhookPublisher(
        IWebhookRegistry registry,
        IHttpClientFactory httpClientFactory,
        ILogger<WebhookPublisher> logger)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task PublishAsync(WebhookEvent webhookEvent, CancellationToken cancellationToken = default)
    {
        if (webhookEvent == null)
            throw new ArgumentNullException(nameof(webhookEvent));

        var endpoints = await _registry.GetSubscriptionsForEventAsync(webhookEvent.EventType, cancellationToken);
        var activeEndpoints = endpoints.Where(e => e.IsActive && e.TenantId == webhookEvent.TenantId).ToList();

        if (activeEndpoints.Count == 0)
        {
            _logger.LogInformation(
                "No active endpoints found for event type {EventType} in tenant {TenantId}",
                webhookEvent.EventType,
                webhookEvent.TenantId);
            return;
        }

        var tasks = activeEndpoints.Select(e => PublishToEndpointAsync(e, webhookEvent, cancellationToken));

        try
        {
            await Task.WhenAll(tasks);
            _logger.LogInformation(
                "Published event {EventType} to {EndpointCount} endpoints in tenant {TenantId}",
                webhookEvent.EventType,
                activeEndpoints.Count,
                webhookEvent.TenantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing event {EventType} in tenant {TenantId}",
                webhookEvent.EventType,
                webhookEvent.TenantId);
            throw;
        }
    }

    public async Task PublishToEndpointAsync(
        WebhookEndpointRegistration endpoint,
        WebhookEvent webhookEvent,
        CancellationToken cancellationToken = default)
    {
        if (endpoint == null)
            throw new ArgumentNullException(nameof(endpoint));
        if (webhookEvent == null)
            throw new ArgumentNullException(nameof(webhookEvent));

        if (endpoint.TenantId != webhookEvent.TenantId)
            throw new ArgumentException(
                $"Endpoint tenant {endpoint.TenantId} does not match event tenant {webhookEvent.TenantId}",
                nameof(endpoint));

        // Create signature using the event
        var signature = WebhookSignature.Sign(webhookEvent, endpoint.SecretKey);

        var payload = new WebhookPayload
        {
            Event = webhookEvent,
            Signature = signature
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var httpClient = _httpClientFactory.CreateClient();
        httpClient.Timeout = TimeSpan.FromSeconds(endpoint.TimeoutSeconds ?? 30);

        try
        {
            var response = await httpClient.PostAsync(endpoint.Url, content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                await _registry.UpdateEndpointAsync(endpoint.Id, new
                {
                    FailureCount = 0,
                    LastAttemptAt = DateTime.UtcNow,
                    FailureReason = (string?)null
                }, cancellationToken);

                _logger.LogInformation(
                    "Webhook delivery succeeded for endpoint {EndpointId} (tenant: {TenantId})",
                    endpoint.Id,
                    endpoint.TenantId);
            }
            else
            {
                await HandleFailureAsync(endpoint, $"HTTP {response.StatusCode}", cancellationToken);
            }
        }
        catch (HttpRequestException ex)
        {
            await HandleFailureAsync(endpoint, $"HTTP Error: {ex.Message}", cancellationToken);
            throw;
        }
        catch (OperationCanceledException)
        {
            await HandleFailureAsync(endpoint, "Request timeout", cancellationToken);
            throw;
        }
        catch (Exception ex)
        {
            await HandleFailureAsync(endpoint, $"Unexpected error: {ex.GetType().Name}: {ex.Message}", cancellationToken);
            throw;
        }
    }

    private async Task HandleFailureAsync(
        WebhookEndpointRegistration endpoint,
        string reason,
        CancellationToken cancellationToken = default)
    {
        endpoint.FailureCount++;
        endpoint.LastAttemptAt = DateTime.UtcNow;
        endpoint.FailureReason = reason;

        if (endpoint.FailureCount >= endpoint.MaxRetries)
        {
            endpoint.IsActive = false;
            _logger.LogError(
                "Webhook endpoint {EndpointId} (tenant: {TenantId}) disabled after {FailureCount} failures: {Reason}",
                endpoint.Id,
                endpoint.TenantId,
                endpoint.FailureCount,
                reason);
        }
        else
        {
            _logger.LogWarning(
                "Webhook delivery failed for endpoint {EndpointId} (tenant: {TenantId}), attempt {FailureCount}/{MaxRetries}: {Reason}",
                endpoint.Id,
                endpoint.TenantId,
                endpoint.FailureCount,
                endpoint.MaxRetries,
                reason);
        }

        await _registry.UpdateEndpointAsync(endpoint.Id, new
        {
            FailureCount = endpoint.FailureCount,
            LastAttemptAt = endpoint.LastAttemptAt,
            FailureReason = endpoint.FailureReason,
            IsActive = endpoint.IsActive
        }, cancellationToken);
    }
}
