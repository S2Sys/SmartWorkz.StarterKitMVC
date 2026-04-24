namespace SmartWorkz.Core.Web.Tests.Middleware;

using Microsoft.AspNetCore.Http;
using SmartWorkz.Core.Web.Middleware;
using Xunit;

public class SecurityHeadersMiddlewareTests
{
    /// <summary>
    /// Creates a test HTTP context with a simple next middleware.
    /// </summary>
    private static HttpContext CreateTestContext()
    {
        var context = new DefaultHttpContext();
        return context;
    }

    /// <summary>
    /// Test: Verify Strict-Transport-Security header is present with correct value.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_AddsStrictTransportSecurityHeader()
    {
        // Arrange
        var context = CreateTestContext();
        RequestDelegate next = async (ctx) => await Task.CompletedTask;
        var middleware = new SecurityHeadersMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(context.Response.Headers.ContainsKey("Strict-Transport-Security"));
        Assert.Equal("max-age=31536000; includeSubDomains", context.Response.Headers["Strict-Transport-Security"].ToString());
    }

    /// <summary>
    /// Test: Verify Content-Security-Policy header is present with correct value.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_AddsContentSecurityPolicyHeader()
    {
        // Arrange
        var context = CreateTestContext();
        RequestDelegate next = async (ctx) => await Task.CompletedTask;
        var middleware = new SecurityHeadersMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(context.Response.Headers.ContainsKey("Content-Security-Policy"));
        var cspValue = context.Response.Headers["Content-Security-Policy"].ToString();
        Assert.Contains("default-src 'self'", cspValue);
        Assert.Contains("script-src 'self' 'unsafe-inline'", cspValue);
    }

    /// <summary>
    /// Test: Verify X-Content-Type-Options header is present with correct value.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_AddsXContentTypeOptionsHeader()
    {
        // Arrange
        var context = CreateTestContext();
        RequestDelegate next = async (ctx) => await Task.CompletedTask;
        var middleware = new SecurityHeadersMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(context.Response.Headers.ContainsKey("X-Content-Type-Options"));
        Assert.Equal("nosniff", context.Response.Headers["X-Content-Type-Options"].ToString());
    }

    /// <summary>
    /// Test: Verify X-Frame-Options header is present with correct value.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_AddsXFrameOptionsHeader()
    {
        // Arrange
        var context = CreateTestContext();
        RequestDelegate next = async (ctx) => await Task.CompletedTask;
        var middleware = new SecurityHeadersMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(context.Response.Headers.ContainsKey("X-Frame-Options"));
        Assert.Equal("DENY", context.Response.Headers["X-Frame-Options"].ToString());
    }

    /// <summary>
    /// Test: Verify X-XSS-Protection header is present with correct value.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_AddsXXSSProtectionHeader()
    {
        // Arrange
        var context = CreateTestContext();
        RequestDelegate next = async (ctx) => await Task.CompletedTask;
        var middleware = new SecurityHeadersMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(context.Response.Headers.ContainsKey("X-XSS-Protection"));
        Assert.Equal("1; mode=block", context.Response.Headers["X-XSS-Protection"].ToString());
    }

    /// <summary>
    /// Test: Verify Referrer-Policy header is present with correct value.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_AddsReferrerPolicyHeader()
    {
        // Arrange
        var context = CreateTestContext();
        RequestDelegate next = async (ctx) => await Task.CompletedTask;
        var middleware = new SecurityHeadersMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(context.Response.Headers.ContainsKey("Referrer-Policy"));
        Assert.Equal("strict-origin-when-cross-origin", context.Response.Headers["Referrer-Policy"].ToString());
    }

    /// <summary>
    /// Test: Verify Permissions-Policy header is present and restricts browser APIs.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_AddsPermissionsPolicyHeader()
    {
        // Arrange
        var context = CreateTestContext();
        RequestDelegate next = async (ctx) => await Task.CompletedTask;
        var middleware = new SecurityHeadersMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(context.Response.Headers.ContainsKey("Permissions-Policy"));
        var ppValue = context.Response.Headers["Permissions-Policy"].ToString();
        Assert.Contains("geolocation=()", ppValue);
        Assert.Contains("microphone=()", ppValue);
        Assert.Contains("camera=()", ppValue);
    }

    /// <summary>
    /// Test: Verify all security headers are added together in one request.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_AddsAllSecurityHeadersTogether()
    {
        // Arrange
        var context = CreateTestContext();
        RequestDelegate next = async (ctx) => await Task.CompletedTask;
        var middleware = new SecurityHeadersMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var headers = new[]
        {
            "Strict-Transport-Security",
            "Content-Security-Policy",
            "X-Content-Type-Options",
            "X-Frame-Options",
            "X-XSS-Protection",
            "Referrer-Policy",
            "Permissions-Policy"
        };

        foreach (var header in headers)
        {
            Assert.True(context.Response.Headers.ContainsKey(header),
                $"Expected header '{header}' not found in response");
        }
    }

    /// <summary>
    /// Test: Verify middleware calls next middleware in pipeline.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_CallsNextMiddleware()
    {
        // Arrange
        var context = CreateTestContext();
        var nextCalled = false;
        RequestDelegate next = async (ctx) =>
        {
            nextCalled = true;
            await Task.CompletedTask;
        };
        var middleware = new SecurityHeadersMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled, "Next middleware was not called");
    }
}
