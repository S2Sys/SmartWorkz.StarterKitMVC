namespace SmartWorkz.Shared;

/// <summary>
/// Exception thrown when configuration validation fails, indicating that a required
/// configuration key is missing, empty, or cannot be converted to the requested type.
/// </summary>
public sealed class ConfigurationValidationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the ConfigurationValidationException class with a specified message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public ConfigurationValidationException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the ConfigurationValidationException class with a specified message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public ConfigurationValidationException(string message, Exception innerException)
        : base(message, innerException) { }
}
