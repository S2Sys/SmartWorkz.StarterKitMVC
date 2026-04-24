namespace SmartWorkz.Core.Web.Middleware;

using Microsoft.AspNetCore.Http;

/// <summary>
/// ASP.NET Core middleware that adds security headers to all HTTP responses.
/// Implements OWASP-recommended security headers to protect against common attacks.
/// </summary>
public class SecurityHeadersMiddleware
{
    /// <summary>
    /// The next middleware in the pipeline.
    /// </summary>
    private readonly RequestDelegate _next;

    /// <summary>
    /// Initializes a new instance of the SecurityHeadersMiddleware class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    /// <summary>
    /// Processes the HTTP request and adds security headers to the response.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        // Add Strict-Transport-Security header (HSTS)
        // Forces HTTPS for 1 year and includes subdomains
        context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";

        // Add Content-Security-Policy header
        // Restricts resource loading to self and allows unsafe-inline for scripts
        context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; font-src 'self'; connect-src 'self'";

        // Add X-Content-Type-Options header
        // Prevents MIME type sniffing attacks
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";

        // Add X-Frame-Options header
        // Prevents clickjacking attacks by denying frame embedding
        context.Response.Headers["X-Frame-Options"] = "DENY";

        // Add X-XSS-Protection header
        // Legacy header for older browsers to enable XSS protection
        context.Response.Headers["X-XSS-Protection"] = "1; mode=block";

        // Add Referrer-Policy header
        // Controls how much referrer information is shared
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // Add Permissions-Policy header (formerly Feature-Policy)
        // Restricts access to browser features and APIs
        context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=(), payment=(), usb=(), magnetometer=(), gyroscope=(), accelerometer=()";

        // Call next middleware
        await _next(context);
    }
}
