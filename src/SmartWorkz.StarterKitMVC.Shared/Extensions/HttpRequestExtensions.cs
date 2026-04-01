using Microsoft.AspNetCore.Http;

namespace SmartWorkz.StarterKitMVC.Shared.Extensions;

public static class HttpRequestExtensions
{
    /// <summary>Returns true when the request was initiated by HTMX.</summary>
    public static bool IsHtmx(this HttpRequest request)
        => request.Headers.ContainsKey("HX-Request");

    /// <summary>Returns the HTMX trigger element id, if any.</summary>
    public static string? HtmxTrigger(this HttpRequest request)
        => request.Headers.TryGetValue("HX-Trigger", out var v) ? v.ToString() : null;

    /// <summary>Returns the HTMX current URL, if any.</summary>
    public static string? HtmxCurrentUrl(this HttpRequest request)
        => request.Headers.TryGetValue("HX-Current-URL", out var v) ? v.ToString() : null;
}
