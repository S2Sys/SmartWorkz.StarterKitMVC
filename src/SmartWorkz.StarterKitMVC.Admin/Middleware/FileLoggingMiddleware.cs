namespace SmartWorkz.StarterKitMVC.Admin.Middleware;

/// <summary>
/// Middleware that logs all HTTP requests and responses to a local file.
/// Useful for debugging without needing to monitor console output.
/// </summary>
public class FileLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _logFilePath;

    public FileLoggingMiddleware(RequestDelegate next)
    {
        _next = next;

        // Create Logs directory in the app root
        var logsDirectory = Path.Combine(AppContext.BaseDirectory, "Logs");
        if (!Directory.Exists(logsDirectory))
            Directory.CreateDirectory(logsDirectory);

        // Create log file with timestamp
        _logFilePath = Path.Combine(logsDirectory, $"admin-{DateTime.UtcNow:yyyy-MM-dd}.log");
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var method = context.Request.Method;
        var path = context.Request.Path;
        var queryString = context.Request.QueryString;
        var userAgent = context.Request.Headers["User-Agent"].ToString();

        // Log request
        var requestLog = $"[{timestamp}] REQUEST: {method} {path}{queryString} | User-Agent: {userAgent}";
        LogToFile(requestLog);

        // Capture the original response body stream
        var originalBodyStream = context.Response.Body;

        try
        {
            await _next(context);

            // Log response
            var responseLog = $"[{timestamp}] RESPONSE: {method} {path}{queryString} | Status: {context.Response.StatusCode}";
            LogToFile(responseLog);
        }
        catch (Exception ex)
        {
            var errorLog = $"[{timestamp}] ERROR: {method} {path}{queryString} | Exception: {ex.Message} | StackTrace: {ex.StackTrace}";
            LogToFile(errorLog);
            throw;
        }
    }

    private void LogToFile(string message)
    {
        try
        {
            lock (this)
            {
                File.AppendAllText(_logFilePath, message + Environment.NewLine);
            }
        }
        catch
        {
            // Silently fail if we can't write to file
        }
    }
}

/// <summary>
/// Extension method to add file logging middleware to the pipeline
/// </summary>
public static class FileLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseFileLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<FileLoggingMiddleware>();
    }
}
