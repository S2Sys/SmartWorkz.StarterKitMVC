using Microsoft.Extensions.Logging;
using Sentry;

namespace SmartWorkz.Mobile.Diagnostics;

/// <summary>
/// Implements crash reporting using the Sentry service.
/// </summary>
public class SentryCrashReporter : ICrashReportingService, IDisposable
{
    private bool _isInitialized;
    private bool _disposedValue;
    private IDisposable? _sentryInitialization;
    private readonly BreadcrumbLogger _breadcrumbLogger;
    private readonly Microsoft.Extensions.Logging.ILogger? _logger;

    /// <summary>
    /// Gets a value indicating whether the crash reporting service has been initialized.
    /// </summary>
    public bool IsInitialized => _isInitialized;

    /// <summary>
    /// Initializes a new instance of the <see cref="SentryCrashReporter"/> class.
    /// </summary>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    public SentryCrashReporter(Microsoft.Extensions.Logging.ILogger? logger = null)
    {
        _logger = logger;
        _breadcrumbLogger = new BreadcrumbLogger();
    }

    /// <summary>
    /// Initializes the Sentry crash reporter asynchronously.
    /// </summary>
    /// <param name="dsn">The Data Source Name (DSN) for Sentry.</param>
    /// <param name="environment">The environment name (Development, Staging, Production).</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous initialization operation.</returns>
    public Task InitializeAsync(string dsn, string environment, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ArgumentException.ThrowIfNullOrWhiteSpace(dsn, nameof(dsn));
        ArgumentException.ThrowIfNullOrWhiteSpace(environment, nameof(environment));

        try
        {
            var options = new SentryOptions
            {
                Dsn = dsn,
                Environment = environment,
                Debug = environment == "Development",
                TracesSampleRate = environment == "Production" ? 0.1 : 1.0,
                IsEnvironmentUser = false,
                SendDefaultPii = false,
                AutoSessionTracking = true,
            };

            // Add handlers for integrations
            options.AddIntegration(new AppDomainUnhandledExceptionIntegration());
            options.AddIntegration(new UnobservedTaskExceptionIntegration());

            _sentryInitialization = SentrySdk.Init(options);
            _isInitialized = true;

            _logger?.LogInformation("Sentry crash reporter initialized successfully for environment: {Environment}", environment);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to initialize Sentry crash reporter");
            _isInitialized = false;
            throw;
        }
    }

    /// <summary>
    /// Sets the current user context for crash reporting asynchronously.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="email">Optional email address of the user.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task SetUserAsync(string userId, string? email = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ArgumentException.ThrowIfNullOrWhiteSpace(userId, nameof(userId));

        if (!_isInitialized)
        {
            _logger?.LogWarning("Attempting to set user context before Sentry initialization");
            return Task.CompletedTask;
        }

        try
        {
            SentrySdk.ConfigureScope(scope =>
            {
                scope.User = new User
                {
                    Id = userId,
                    Email = email
                };
            });

            _logger?.LogDebug("User context set for Sentry: {UserId}", userId);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error setting user context in Sentry");
            throw;
        }
    }

    /// <summary>
    /// Sets the contextual information for crash reporting asynchronously.
    /// </summary>
    /// <param name="context">The crash context containing session and application information.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task SetContextAsync(CrashContext context, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ArgumentNullException.ThrowIfNull(context, nameof(context));

        if (!_isInitialized)
        {
            _logger?.LogWarning("Attempting to set crash context before Sentry initialization");
            return Task.CompletedTask;
        }

        try
        {
            // Validate no overlapping keys between CustomData and Extra
            if (context.CustomData != null && context.Extra != null)
            {
                var overlappingKeys = context.CustomData.Keys.Intersect(context.Extra.Keys).ToList();
                if (overlappingKeys.Count > 0)
                {
                    _logger?.LogWarning(
                        "CrashContext has overlapping keys between CustomData and Extra: {Keys}. Extra values will override CustomData.",
                        string.Join(", ", overlappingKeys)
                    );
                }
            }

            SentrySdk.ConfigureScope(scope =>
            {
                // Set session context
                scope.SetContext("session", new Dictionary<string, object>
                {
                    { "session_id", context.SessionId },
                    { "app_version", context.AppVersion },
                    { "environment", context.Environment }
                });

                // Add custom data
                foreach (var kvp in context.CustomData)
                {
                    scope.SetExtra(kvp.Key, kvp.Value);
                }

                // Add tags
                foreach (var kvp in context.Tags)
                {
                    scope.SetTag(kvp.Key, kvp.Value);
                }

                // Add extra debugging information
                foreach (var kvp in context.Extra)
                {
                    scope.SetExtra(kvp.Key, kvp.Value);
                }
            });

            _logger?.LogDebug("Crash context set for Sentry: {SessionId}", context.SessionId);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error setting crash context in Sentry");
            throw;
        }
    }

    /// <summary>
    /// Adds a breadcrumb to the event trail asynchronously.
    /// </summary>
    /// <param name="message">The breadcrumb message describing the event.</param>
    /// <param name="category">The category of the breadcrumb (e.g., "ui.click", "navigation", "network").</param>
    /// <param name="level">The severity level of the breadcrumb (debug, info, warning, error).</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task AddBreadcrumbAsync(string message, string category, string level, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ArgumentException.ThrowIfNullOrWhiteSpace(message, nameof(message));
        ArgumentException.ThrowIfNullOrWhiteSpace(category, nameof(category));
        ArgumentException.ThrowIfNullOrWhiteSpace(level, nameof(level));

        try
        {
            // Add to local logger
            _breadcrumbLogger.AddBreadcrumb(message, category, level);

            // Add to Sentry if initialized
            if (_isInitialized)
            {
                var breadcrumbLevel = ConvertLevelToSentryLevel(level);
                SentrySdk.AddBreadcrumb(message, category, level: breadcrumbLevel);
            }

            _logger?.LogDebug("Breadcrumb added: {Message} [{Category}]", message, category);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error adding breadcrumb to Sentry");
            throw;
        }
    }

    /// <summary>
    /// Captures an exception and reports it asynchronously.
    /// </summary>
    /// <param name="exception">The exception to capture.</param>
    /// <param name="context">Optional crash context to include with the exception.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task CaptureExceptionAsync(Exception exception, CrashContext? context = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ArgumentNullException.ThrowIfNull(exception, nameof(exception));

        try
        {
            if (!_isInitialized)
            {
                _logger?.LogWarning("Attempting to capture exception before Sentry initialization");
                return Task.CompletedTask;
            }

            // Set context if provided
            if (context != null)
            {
                SentrySdk.ConfigureScope(scope =>
                {
                    scope.SetContext("crash", new Dictionary<string, object>
                    {
                        { "user_id", context.UserId },
                        { "session_id", context.SessionId },
                        { "app_version", context.AppVersion }
                    });

                    scope.User = new User { Id = context.UserId };
                });
            }

            // Capture the exception
            SentrySdk.CaptureException(exception);

            _logger?.LogError(exception, "Exception captured by Sentry");

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error capturing exception in Sentry");
            throw;
        }
    }

    /// <summary>
    /// Captures a message and reports it asynchronously.
    /// </summary>
    /// <param name="message">The message to capture.</param>
    /// <param name="level">The severity level of the message (debug, info, warning, error).</param>
    /// <param name="context">Optional crash context to include with the message.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task CaptureMessageAsync(string message, string level, CrashContext? context = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ArgumentException.ThrowIfNullOrWhiteSpace(message, nameof(message));
        ArgumentException.ThrowIfNullOrWhiteSpace(level, nameof(level));

        try
        {
            if (!_isInitialized)
            {
                _logger?.LogWarning("Attempting to capture message before Sentry initialization");
                return Task.CompletedTask;
            }

            // Set context if provided
            if (context != null)
            {
                SentrySdk.ConfigureScope(scope =>
                {
                    scope.SetContext("message", new Dictionary<string, object>
                    {
                        { "user_id", context.UserId },
                        { "session_id", context.SessionId }
                    });
                });
            }

            var sentryLevel = ConvertLevelToSentryLevel(level);
            SentrySdk.CaptureMessage(message, sentryLevel);

            _logger?.LogInformation("Message captured by Sentry: {Message} [{Level}]", message, level);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error capturing message in Sentry");
            throw;
        }
    }

    /// <summary>
    /// Converts a string level representation to Sentry's SentryLevel.
    /// </summary>
    /// <param name="level">The level string (debug, info, warning, error).</param>
    /// <returns>The corresponding Sentry level.</returns>
    private static SentryLevel ConvertLevelToSentryLevel(string level)
    {
        return level.ToLowerInvariant() switch
        {
            "debug" => SentryLevel.Debug,
            "info" => SentryLevel.Info,
            "warning" => SentryLevel.Warning,
            "error" => SentryLevel.Error,
            "fatal" => SentryLevel.Fatal,
            _ => SentryLevel.Info
        };
    }

    /// <summary>
    /// Disposes the Sentry initialization.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the managed resources.
    /// </summary>
    /// <param name="disposing">Whether to dispose managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _sentryInitialization?.Dispose();
                _sentryInitialization = null;
            }

            _disposedValue = true;
        }
    }
}
