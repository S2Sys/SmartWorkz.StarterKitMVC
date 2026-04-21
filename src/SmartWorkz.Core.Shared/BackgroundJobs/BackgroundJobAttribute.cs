namespace SmartWorkz.Core.Services.BackgroundJobs;

[AttributeUsage(AttributeTargets.Method)]
public class BackgroundJobAttribute : Attribute
{
    public int MaxRetries { get; set; } = 3;
    public int TimeoutSeconds { get; set; } = 3600; // 1 hour default

    public BackgroundJobAttribute(string description = "")
    {
        Description = description;
    }

    public string Description { get; set; }
}
