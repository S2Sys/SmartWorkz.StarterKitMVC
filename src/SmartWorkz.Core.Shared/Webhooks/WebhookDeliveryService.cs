namespace SmartWorkz.Shared;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using SmartWorkz.Core.Shared.Guards;
using Microsoft.Extensions.Logging;

/// <summary>
/// Service for publishing domain events to registered webhook endpoints.
/// Implements exponential backoff retry logic, HMAC signature verification, and failure tracking.
/// </summary>
public class WebhookDeliveryService
{
    private readonly IWebhookRegistry _registry;
    private readonly HttpClient _httpClient;
    private readonly ILogger<WebhookDeliveryService> _logger;

    public WebhookDeliveryService(IWebhookRegistry registry, HttpClient httpClient, ILogger<WebhookDeliveryService> logger)
    {
        _registry = Guard.NotNull(registry, nameof(registry));
        _httpClient = Guard.NotNull(httpClient, nameof(httpClient));
        _logger = Guard.NotNull(logger, nameof(logger));
    }

    /// <summary>
    /// Publish an event to all subscribed webhook endpoints.
    /// </summary>
    /// <param name="eventName">The name of the event being published.</param>
    /// <param name="payload">The event payload to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task PublishEventAsync(string eventName, object payload, CancellationToken cancellationToken = default)
    {
        Guard.NotEmpty(eventName, nameof(eventName));
        Guard.NotNull(payload, nameof(payload));

        var subscriptions = await _registry.GetSubscriptionsForEventAsync(eventName, cancellationToken);

        if (!subscriptions.Any())
        {
            _logger.LogInformation("No webhook subscriptions for event {EventName}", eventName);
            return;
        }

        var tasks = subscriptions.Select(sub => DeliverAsync(sub, eventName, payload, cancellationToken));
        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Deliver an event to a single webhook endpoint with exponential backoff retry logic.
    /// </summary>
    private async Task DeliverAsync(WebhookSubscription subscription, string eventName, object payload, CancellationToken cancellationToken)
    {
        var maxRetries = subscription.MaxRetries ?? 3;
        var timeout = subscription.TimeoutSeconds ?? 30;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var json = JsonSerializer.Serialize(new { @event = eventName, data = payload });
                var signature = GenerateSignature(json, subscription.Secret);

                using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                {
                    cts.CancelAfter(TimeSpan.FromSeconds(timeout));

                    var request = new HttpRequestMessage(HttpMethod.Post, subscription.Url)
                    {
                        Content = new StringContent(json, Encoding.UTF8, "application/json")
                    };

                    if (!string.IsNullOrEmpty(signature))
                        request.Headers.Add("X-Webhook-Signature", signature);

                    request.Headers.Add("X-Webhook-Event", eventName);
                    request.Headers.Add("X-Webhook-Id", subscription.Id.ToString());

                    var response = await _httpClient.SendAsync(request, cts.Token);

                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Webhook delivered successfully to {Url} for event {EventName}", subscription.Url, eventName);
                        await _registry.UpdateSubscriptionStatusAsync(subscription.Id, true, 0, null, cancellationToken);
                        return;
                    }
                    else
                    {
                        _logger.LogWarning("Webhook delivery failed with status {Status} to {Url}", response.StatusCode, subscription.Url);

                        if (attempt == maxRetries)
                        {
                            await _registry.UpdateSubscriptionStatusAsync(subscription.Id, false, maxRetries, $"HTTP {response.StatusCode}", cancellationToken);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Webhook delivery timed out to {Url}", subscription.Url);

                if (attempt == maxRetries)
                {
                    await _registry.UpdateSubscriptionStatusAsync(subscription.Id, false, maxRetries, "Timeout", cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error delivering webhook to {Url}", subscription.Url);

                if (attempt == maxRetries)
                {
                    await _registry.UpdateSubscriptionStatusAsync(subscription.Id, false, maxRetries, ex.Message, cancellationToken);
                }
            }

            if (attempt < maxRetries)
            {
                var delayMs = (int)Math.Pow(2, attempt) * 1000;
                await Task.Delay(delayMs, cancellationToken);
            }
        }
    }

    /// <summary>
    /// Generate HMAC-SHA256 signature for webhook payload verification.
    /// </summary>
    private static string? GenerateSignature(string payload, string? secret)
    {
        if (string.IsNullOrEmpty(secret))
            return null;

        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
        {
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return Convert.ToHexString(hash);
        }
    }
}
