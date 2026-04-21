namespace SmartWorkz.Core.Shared.Logging;

using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// Enriched logger wrapper around ILogger that provides structured logging methods
/// for domain events, commands, sagas, file operations, and background jobs.
/// Uses structured properties instead of string interpolation for better queryability.
/// </summary>
public class EnrichedLogger
{
    private readonly Microsoft.Extensions.Logging.ILogger? _innerLogger;
    private readonly string _categoryName;

    /// <summary>
    /// Creates a new instance of EnrichedLogger.
    /// </summary>
    /// <param name="logger">The underlying ILogger instance</param>
    public EnrichedLogger(Microsoft.Extensions.Logging.ILogger? logger)
    {
        _innerLogger = logger;
        _categoryName = logger?.GetType().Name ?? "EnrichedLogger";
    }

    #region Command Logging

    /// <summary>
    /// Logs command execution with duration and other metrics.
    /// </summary>
    /// <param name="commandType">The type of command being executed</param>
    /// <param name="duration">How long the command took to execute</param>
    public void LogCommandExecuted(string commandType, TimeSpan duration)
    {
        _innerLogger?.LogInformation(
            "Command executed: {CommandType} in {DurationMs}ms",
            commandType,
            duration.TotalMilliseconds);
    }

    /// <summary>
    /// Logs a command execution error with exception details.
    /// </summary>
    /// <param name="commandType">The type of command that failed</param>
    /// <param name="exception">The exception that occurred</param>
    public void LogCommandExecutionError(string commandType, Exception exception)
    {
        _innerLogger?.LogError(
            exception,
            "Command execution failed: {CommandType}",
            commandType);
    }

    /// <summary>
    /// Logs a command with validation errors.
    /// </summary>
    /// <param name="commandType">The type of command</param>
    /// <param name="errors">Dictionary of validation errors</param>
    public void LogCommandValidationError(string commandType, IDictionary<string, string> errors)
    {
        _innerLogger?.LogWarning(
            "Command validation failed: {CommandType} with {ErrorCount} errors",
            commandType,
            errors.Count);
    }

    #endregion

    #region Event Logging

    /// <summary>
    /// Logs an event publication with metadata.
    /// </summary>
    /// <param name="eventType">The type of event being published</param>
    /// <param name="eventId">The unique identifier of the event</param>
    public void LogEventPublished(string eventType, Guid eventId)
    {
        _innerLogger?.LogInformation(
            "Event published: {EventType} with EventId {EventId}",
            eventType,
            eventId);
    }

    /// <summary>
    /// Logs an event with additional context properties.
    /// </summary>
    /// <param name="eventType">The type of event</param>
    /// <param name="eventId">The event identifier</param>
    /// <param name="context">Additional context data</param>
    public void LogEventPublishedWithContext(string eventType, Guid eventId, IDictionary<string, object> context)
    {
        var contextString = string.Join(", ", context.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        _innerLogger?.LogInformation(
            "Event published with context: {EventType}, EventId: {EventId}, Context: {Context}",
            eventType,
            eventId,
            contextString);
    }

    /// <summary>
    /// Logs an event subscription.
    /// </summary>
    /// <param name="eventType">The type of event being subscribed to</param>
    /// <param name="subscriberType">The subscriber type</param>
    public void LogEventSubscribed(string eventType, string subscriberType)
    {
        _innerLogger?.LogInformation(
            "Event subscriber registered: {EventType} -> {SubscriberType}",
            eventType,
            subscriberType);
    }

    #endregion

    #region Saga Logging

    /// <summary>
    /// Logs the start of a saga with its initial state.
    /// </summary>
    /// <param name="sagaId">The unique saga identifier</param>
    /// <param name="state">The initial saga state</param>
    public void LogSagaStarted(Guid sagaId, string state)
    {
        _innerLogger?.LogInformation(
            "Saga started: {SagaId} with initial state {State}",
            sagaId,
            state);
    }

    /// <summary>
    /// Logs a saga state transition.
    /// </summary>
    /// <param name="sagaId">The saga identifier</param>
    /// <param name="fromState">The previous state</param>
    /// <param name="toState">The new state</param>
    public void LogSagaStateTransition(Guid sagaId, string fromState, string toState)
    {
        _innerLogger?.LogInformation(
            "Saga state transition: {SagaId} from {FromState} to {ToState}",
            sagaId,
            fromState,
            toState);
    }

    /// <summary>
    /// Logs the completion of a saga.
    /// </summary>
    /// <param name="sagaId">The saga identifier</param>
    /// <param name="duration">How long the saga took to complete</param>
    public void LogSagaCompleted(Guid sagaId, TimeSpan duration)
    {
        _innerLogger?.LogInformation(
            "Saga completed: {SagaId} in {DurationSeconds}s",
            sagaId,
            duration.TotalSeconds);
    }

    /// <summary>
    /// Logs a saga failure.
    /// </summary>
    /// <param name="sagaId">The saga identifier</param>
    /// <param name="exception">The exception that caused the failure</param>
    public void LogSagaFailed(Guid sagaId, Exception exception)
    {
        _innerLogger?.LogError(
            exception,
            "Saga failed: {SagaId}",
            sagaId);
    }

    #endregion

    #region File Operation Logging

    /// <summary>
    /// Logs file operations such as upload, download, delete.
    /// </summary>
    /// <param name="operation">The type of operation (Upload, Download, Delete, etc.)</param>
    /// <param name="filePath">The file path or URI</param>
    public void LogFileOperation(string operation, string filePath)
    {
        _innerLogger?.LogInformation(
            "File operation: {Operation} on {FilePath}",
            operation,
            filePath);
    }

    /// <summary>
    /// Logs a file operation with size information.
    /// </summary>
    /// <param name="operation">The type of operation</param>
    /// <param name="filePath">The file path</param>
    /// <param name="sizeBytes">The file size in bytes</param>
    public void LogFileOperationWithSize(string operation, string filePath, long sizeBytes)
    {
        _innerLogger?.LogInformation(
            "File operation: {Operation} on {FilePath} ({SizeBytes} bytes)",
            operation,
            filePath,
            sizeBytes);
    }

    /// <summary>
    /// Logs a file operation error.
    /// </summary>
    /// <param name="operation">The operation that failed</param>
    /// <param name="filePath">The file path</param>
    /// <param name="exception">The exception that occurred</param>
    public void LogFileOperationError(string operation, string filePath, Exception exception)
    {
        _innerLogger?.LogError(
            exception,
            "File operation failed: {Operation} on {FilePath}",
            operation,
            filePath);
    }

    #endregion

    #region Background Job Logging

    /// <summary>
    /// Logs when a background job is queued.
    /// </summary>
    /// <param name="jobId">The unique job identifier</param>
    /// <param name="jobType">The type of job being queued</param>
    public void LogJobQueued(string jobId, string jobType)
    {
        _innerLogger?.LogInformation(
            "Job queued: {JobId} of type {JobType}",
            jobId,
            jobType);
    }

    /// <summary>
    /// Logs when a background job starts processing.
    /// </summary>
    /// <param name="jobId">The job identifier</param>
    /// <param name="jobType">The job type</param>
    public void LogJobStarted(string jobId, string jobType)
    {
        _innerLogger?.LogInformation(
            "Job started: {JobId} of type {JobType}",
            jobId,
            jobType);
    }

    /// <summary>
    /// Logs successful job completion.
    /// </summary>
    /// <param name="jobId">The job identifier</param>
    /// <param name="duration">How long the job took to complete</param>
    public void LogJobCompleted(string jobId, TimeSpan duration)
    {
        _innerLogger?.LogInformation(
            "Job completed: {JobId} in {DurationMinutes}m",
            jobId,
            duration.TotalMinutes);
    }

    /// <summary>
    /// Logs a job failure.
    /// </summary>
    /// <param name="jobId">The job identifier</param>
    /// <param name="exception">The exception that caused the failure</param>
    public void LogJobFailed(string jobId, Exception exception)
    {
        _innerLogger?.LogError(
            exception,
            "Job failed: {JobId}",
            jobId);
    }

    /// <summary>
    /// Logs job retry attempt.
    /// </summary>
    /// <param name="jobId">The job identifier</param>
    /// <param name="attemptNumber">The current attempt number</param>
    /// <param name="maxRetries">The maximum number of retries</param>
    public void LogJobRetry(string jobId, int attemptNumber, int maxRetries)
    {
        _innerLogger?.LogWarning(
            "Job retry: {JobId} attempt {AttemptNumber} of {MaxRetries}",
            jobId,
            attemptNumber,
            maxRetries);
    }

    #endregion

    #region General Structured Logging

    /// <summary>
    /// Logs a message with structured context properties.
    /// </summary>
    /// <param name="operationName">The name of the operation</param>
    /// <param name="context">Dictionary of contextual properties</param>
    public void LogWithContext(string operationName, IDictionary<string, object> context)
    {
        var contextString = FormatContext(context);
        _innerLogger?.LogInformation(
            "Operation: {OperationName} | Context: {Context}",
            operationName,
            contextString);
    }

    /// <summary>
    /// Logs performance metrics for an operation.
    /// </summary>
    /// <param name="operationName">The operation name</param>
    /// <param name="duration">The operation duration</param>
    /// <param name="resultStatus">The result status (Success, Failure, etc.)</param>
    public void LogPerformanceMetrics(string operationName, TimeSpan duration, string resultStatus)
    {
        _innerLogger?.LogInformation(
            "Performance metric: {OperationName} completed in {DurationMs}ms with status {Status}",
            operationName,
            duration.TotalMilliseconds,
            resultStatus);
    }

    /// <summary>
    /// Logs a correlation ID for request tracing.
    /// </summary>
    /// <param name="correlationId">The correlation identifier</param>
    /// <param name="userId">Optional user identifier</param>
    /// <param name="requestPath">Optional request path</param>
    public void LogCorrelation(string correlationId, string? userId = null, string? requestPath = null)
    {
        var contextParts = new List<string> { $"CorrelationId={correlationId}" };

        if (!string.IsNullOrEmpty(userId))
            contextParts.Add($"UserId={userId}");

        if (!string.IsNullOrEmpty(requestPath))
            contextParts.Add($"RequestPath={requestPath}");

        var contextString = string.Join(" | ", contextParts);

        _innerLogger?.LogInformation(
            "Request context: {Context}",
            contextString);
    }

    /// <summary>
    /// Logs unhandled exceptions as critical errors.
    /// </summary>
    /// <param name="exception">The exception that occurred</param>
    /// <param name="operationName">The operation that failed</param>
    public void LogUnhandledException(Exception exception, string operationName)
    {
        _innerLogger?.LogCritical(
            exception,
            "Unhandled exception in {OperationName}",
            operationName);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Formats a context dictionary into a readable string.
    /// </summary>
    private static string FormatContext(IDictionary<string, object> context)
    {
        if (context == null || context.Count == 0)
            return "No context";

        return string.Join(" | ", context.Select(kvp => $"{kvp.Key}={kvp.Value}"));
    }

    #endregion
}
