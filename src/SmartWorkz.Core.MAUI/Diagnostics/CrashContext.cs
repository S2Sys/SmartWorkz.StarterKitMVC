namespace SmartWorkz.Mobile.Diagnostics;

/// <summary>
/// Provides contextual information about a crash or error for reporting to Sentry.
/// </summary>
public class CrashContext
{
    /// <summary>
    /// Gets the user ID associated with this crash.
    /// </summary>
    public string UserId { get; }

    /// <summary>
    /// Gets the unique session identifier.
    /// </summary>
    public string SessionId { get; }

    /// <summary>
    /// Gets the application version.
    /// </summary>
    public string AppVersion { get; }

    /// <summary>
    /// Gets the environment (Development, Staging, Production, etc.).
    /// </summary>
    public string Environment { get; }

    /// <summary>
    /// Gets custom contextual data as key-value pairs.
    /// </summary>
    public Dictionary<string, object> CustomData { get; }

    /// <summary>
    /// Gets tags for categorizing the crash.
    /// </summary>
    public Dictionary<string, string> Tags { get; }

    /// <summary>
    /// Gets extra debugging information.
    /// </summary>
    public Dictionary<string, object> Extra { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CrashContext"/> class.
    /// </summary>
    /// <param name="userId">The user ID associated with the crash.</param>
    /// <param name="sessionId">The unique session identifier.</param>
    /// <param name="appVersion">The application version.</param>
    /// <param name="environment">The environment (Development, Staging, Production).</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    /// <exception cref="ArgumentException">Thrown when any parameter is an empty string.</exception>
    public CrashContext(string userId, string sessionId, string appVersion, string environment)
    {
        UserId = ValidateParameter(userId, nameof(userId));
        SessionId = ValidateParameter(sessionId, nameof(sessionId));
        AppVersion = ValidateParameter(appVersion, nameof(appVersion));
        Environment = ValidateParameter(environment, nameof(environment));

        CustomData = new Dictionary<string, object>();
        Tags = new Dictionary<string, string>();
        Extra = new Dictionary<string, object>();
    }

    /// <summary>
    /// Validates a parameter to ensure it is not null or empty.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="parameterName">The name of the parameter being validated.</param>
    /// <returns>The validated value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    /// <exception cref="ArgumentException">Thrown when value is an empty string.</exception>
    private static string ValidateParameter(string value, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"Parameter '{parameterName}' cannot be empty.", parameterName);
        }
        return value;
    }
}
