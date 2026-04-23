namespace SmartWorkz.Mobile.Services.Implementations;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmartWorkz.Mobile.Models;
using SmartWorkz.Shared;

/// <summary>
/// Routes incoming real-time messages to registered handlers.
/// Supports case-insensitive method matching and asynchronous handler execution.
/// </summary>
public class RealtimeMessageHandler
{
    private readonly Dictionary<string, Func<RealtimeMessage, Task>> _handlers =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly ILogger<RealtimeMessageHandler>? _logger;

    public RealtimeMessageHandler(ILogger<RealtimeMessageHandler>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Register a handler for a specific message method.
    /// Replaces any existing handler for the same method.
    /// </summary>
    /// <param name="method">The message method name to handle (case-insensitive)</param>
    /// <param name="handler">The async handler function to execute</param>
    /// <exception cref="ArgumentException">Thrown when method is null or empty</exception>
    /// <exception cref="ArgumentNullException">Thrown when handler is null</exception>
    public void RegisterHandler(string method, Func<RealtimeMessage, Task> handler)
    {
        Guard.NotEmpty(method, nameof(method));
        Guard.NotNull(handler, nameof(handler));

        _handlers[method] = handler;
        _logger?.LogDebug("Registered handler for method {Method}", method);
    }

    /// <summary>
    /// Route incoming message to the appropriate registered handler.
    /// </summary>
    /// <param name="message">The real-time message to handle</param>
    /// <returns>True if message was handled successfully, false if no handler found or handler threw exception</returns>
    /// <exception cref="ArgumentNullException">Thrown when message is null</exception>
    public async Task<bool> HandleAsync(RealtimeMessage message)
    {
        Guard.NotNull(message, nameof(message));

        if (!_handlers.TryGetValue(message.Method, out var handler))
        {
            _logger?.LogWarning("No handler registered for method {Method}", message.Method);
            return false;
        }

        try
        {
            await handler(message);
            _logger?.LogDebug(
                "Successfully handled message {Method} with correlation {CorrelationId}",
                message.Method, message.CorrelationId);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error handling message {Method} with correlation {CorrelationId}",
                message.Method, message.CorrelationId);
            return false;
        }
    }

    /// <summary>
    /// Clear all registered handlers.
    /// </summary>
    public void ClearHandlers()
    {
        _handlers.Clear();
        _logger?.LogDebug("Cleared all message handlers");
    }

    /// <summary>
    /// Get the count of currently registered handlers.
    /// </summary>
    public int HandlerCount => _handlers.Count;
}
