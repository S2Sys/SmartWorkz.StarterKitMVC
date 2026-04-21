#if !WINDOWS
namespace SmartWorkz.Core.Mobile;

public class MobileApiConfig
{
    public required string BaseUrl { get; set; }
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    public int RetryCount { get; set; } = 3;
    public RetryStrategy RetryStrategy { get; set; } = RetryStrategy.Exponential;
    public string UserAgent { get; set; } = "SmartWorkz.Mobile/1.0";
    public bool EnableCompression { get; set; } = true;
}
#else
namespace SmartWorkz.Core.Mobile;

public class MobileApiConfig
{
    public required string BaseUrl { get; set; }
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    public int RetryCount { get; set; } = 3;
    public string? RetryStrategy { get; set; } = "Exponential";
    public string UserAgent { get; set; } = "SmartWorkz.Mobile/1.0";
    public bool EnableCompression { get; set; } = true;
}
#endif
