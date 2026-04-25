namespace SmartWorkz.Mobile.Notifications;

/// <summary>
/// Routes incoming push messages to appropriate handlers based on message category.
/// Supports multiple handlers per category, default fallback handler, and error recovery.
/// </summary>
public class PushMessageRouter
{
    private readonly Dictionary<string, List<IPushNotificationHandler>> _categoryHandlers = new();
    private IPushNotificationHandler? _defaultHandler;
    private readonly object _lockObject = new();

    /// <summary>
    /// Registers a handler for a specific message category.
    /// Multiple handlers can be registered for the same category.
    /// </summary>
    /// <param name="category">The message category to handle (e.g., "order", "alert").</param>
    /// <param name="handler">The handler instance to register.</param>
    /// <exception cref="ArgumentException">Thrown when category is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when handler is null.</exception>
    public void RegisterHandler(string category, IPushNotificationHandler handler)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(category, nameof(category));
        ArgumentNullException.ThrowIfNull(handler);

        lock (_lockObject)
        {
            if (!_categoryHandlers.TryGetValue(category, out var handlers))
            {
                handlers = new();
                _categoryHandlers[category] = handlers;
            }

            if (!handlers.Contains(handler))
            {
                handlers.Add(handler);
            }
        }
    }

    /// <summary>
    /// Registers a default handler that processes messages with unregistered categories.
    /// </summary>
    /// <param name="handler">The default handler instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when handler is null.</exception>
    public void RegisterDefaultHandler(IPushNotificationHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        lock (_lockObject)
        {
            _defaultHandler = handler;
        }
    }

    /// <summary>
    /// Gets the currently registered default handler.
    /// </summary>
    /// <returns>The default handler, or null if none is registered.</returns>
    public IPushNotificationHandler? GetDefaultHandler()
    {
        lock (_lockObject)
        {
            return _defaultHandler;
        }
    }

    /// <summary>
    /// Unregisters a handler for a specific category.
    /// </summary>
    /// <param name="category">The message category.</param>
    /// <param name="handler">The handler to unregister.</param>
    public void UnregisterHandler(string category, IPushNotificationHandler handler)
    {
        lock (_lockObject)
        {
            if (_categoryHandlers.TryGetValue(category, out var handlers))
            {
                handlers.Remove(handler);
                if (handlers.Count == 0)
                {
                    _categoryHandlers.Remove(category);
                }
            }
        }
    }

    /// <summary>
    /// Unregisters all handlers for a specific category.
    /// </summary>
    /// <param name="category">The message category to clear.</param>
    public void UnregisterAllForCategory(string category)
    {
        lock (_lockObject)
        {
            _categoryHandlers.Remove(category);
        }
    }

    /// <summary>
    /// Checks if a handler is registered for a category.
    /// </summary>
    /// <param name="category">The message category.</param>
    /// <param name="handler">The handler to check.</param>
    /// <returns>True if the handler is registered for the category; false otherwise.</returns>
    public bool IsHandlerRegistered(string category, IPushNotificationHandler handler)
    {
        lock (_lockObject)
        {
            return _categoryHandlers.TryGetValue(category, out var handlers) && handlers.Contains(handler);
        }
    }

    /// <summary>
    /// Gets the total number of handlers registered (excluding default handler).
    /// </summary>
    public int GetHandlerCount()
    {
        lock (_lockObject)
        {
            return _categoryHandlers.Values.Sum(h => h.Count);
        }
    }

    /// <summary>
    /// Clears all registered handlers and the default handler.
    /// </summary>
    public void Clear()
    {
        lock (_lockObject)
        {
            _categoryHandlers.Clear();
            _defaultHandler = null;
        }
    }

    /// <summary>
    /// Routes an incoming message to appropriate handlers based on its category.
    /// If no handlers are registered for the category, uses the default handler.
    /// Skips expired messages.
    /// </summary>
    /// <param name="message">The message to route.</param>
    /// <param name="continueOnError">If true, continues routing to other handlers even if one throws; otherwise propagates the exception.</param>
    /// <param name="cancellationToken">Token to cancel the routing operation.</param>
    /// <exception cref="ArgumentNullException">Thrown when message is null.</exception>
    public async Task RouteMessageAsync(
        PushMessage message,
        bool continueOnError = true,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        cancellationToken.ThrowIfCancellationRequested();

        // Skip expired messages
        if (message.IsExpired())
        {
            return;
        }

        List<IPushNotificationHandler> handlersToCall = new();

        lock (_lockObject)
        {
            // Get category-specific handlers
            if (!string.IsNullOrWhiteSpace(message.Category) &&
                _categoryHandlers.TryGetValue(message.Category, out var categoryHandlers))
            {
                handlersToCall.AddRange(categoryHandlers);
            }
            else if (_defaultHandler != null)
            {
                // Use default handler if no category handlers found
                handlersToCall.Add(_defaultHandler);
            }
        }

        if (handlersToCall.Count == 0)
        {
            return; // No handlers, skip silently
        }

        if (continueOnError)
        {
            foreach (var handler in handlersToCall)
            {
                try
                {
                    await handler.HandleMessageAsync(message, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception)
                {
                    // Continue to next handler
                }
            }
        }
        else
        {
            foreach (var handler in handlersToCall)
            {
                await handler.HandleMessageAsync(message, cancellationToken);
            }
        }
    }
}
