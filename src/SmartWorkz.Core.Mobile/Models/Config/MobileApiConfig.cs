namespace SmartWorkz.Mobile;

public sealed class MobileApiConfig
{
    public required string BaseUrl { get; set; }
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    public int RetryCount { get; set; } = 3;
#if !WINDOWS
    public RetryStrategy RetryStrategy { get; set; } = RetryStrategy.Exponential;
#else
    public string RetryStrategy { get; set; } = "Exponential";
#endif
    public string UserAgent { get; set; } = "SmartWorkz.Mobile/1.0";
    public bool EnableCompression { get; set; } = true;
}
