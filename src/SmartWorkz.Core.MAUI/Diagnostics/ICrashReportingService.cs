namespace SmartWorkz.Mobile.Diagnostics;

/// <summary>
/// Defines the contract for crash reporting and error tracking services.
/// </summary>
public interface ICrashReportingService
{
    /// <summary>
    /// Gets a value indicating whether the crash reporting service has been initialized.
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Initializes the crash reporting service asynchronously.
    /// </summary>
    /// <param name="dsn">The Data Source Name (DSN) for the crash reporting backend (e.g., Sentry).</param>
    /// <param name="environment">The environment name (Development, Staging, Production).</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous initialization operation.</returns>
    Task InitializeAsync(string dsn, string environment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the current user context for crash reporting asynchronously.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="email">Optional email address of the user.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SetUserAsync(string userId, string? email = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the contextual information for crash reporting asynchronously.
    /// </summary>
    /// <param name="context">The crash context containing session and application information.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SetContextAsync(CrashContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a breadcrumb to the event trail asynchronously.
    /// </summary>
    /// <param name="message">The breadcrumb message describing the event.</param>
    /// <param name="category">The category of the breadcrumb (e.g., "ui.click", "navigation", "network").</param>
    /// <param name="level">The severity level of the breadcrumb (debug, info, warning, error).</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddBreadcrumbAsync(string message, string category, string level, CancellationToken cancellationToken = default);

    /// <summary>
    /// Captures an exception and reports it asynchronously.
    /// </summary>
    /// <param name="exception">The exception to capture.</param>
    /// <param name="context">Optional crash context to include with the exception.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CaptureExceptionAsync(Exception exception, CrashContext? context = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Captures a message and reports it asynchronously.
    /// </summary>
    /// <param name="message">The message to capture.</param>
    /// <param name="level">The severity level of the message (debug, info, warning, error).</param>
    /// <param name="context">Optional crash context to include with the message.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CaptureMessageAsync(string message, string level, CrashContext? context = null, CancellationToken cancellationToken = default);
}
