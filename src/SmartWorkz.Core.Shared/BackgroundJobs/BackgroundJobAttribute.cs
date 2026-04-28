namespace SmartWorkz.Shared;

/// <summary>
/// Marks a method as a background job that should be executed asynchronously with retry and timeout policies.
/// Used with background job processing frameworks (e.g., Hangfire) to configure job execution behavior.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class BackgroundJobAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the maximum number of retry attempts if the job fails.
    /// Default is 3 retries.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Gets or sets the job execution timeout in seconds.
    /// Default is 3600 seconds (1 hour). If the job exceeds this duration, it will be terminated.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 3600; // 1 hour default

    /// <summary>
    /// Initializes a new instance of the <see cref="BackgroundJobAttribute"/> class with a description.
    /// </summary>
    /// <param name="description">Human-readable description of the background job's purpose.</param>
    public BackgroundJobAttribute(string description = "")
    {
        Description = description;
    }

    /// <summary>
    /// Gets or sets the description of the background job explaining its purpose and functionality.
    /// </summary>
    public string Description { get; set; }
}
