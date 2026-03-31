using System.Net;
using System.Text.Json;
using SmartWorkz.StarterKitMVC.Shared.Primitives;
using SmartWorkz.StarterKitMVC.Shared.Validation;

namespace SmartWorkz.StarterKitMVC.Web.Middleware;

/// <summary>
/// Middleware for handling exceptions globally and returning RFC 7807 Problem Details responses.
/// </summary>
public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception has occurred while executing the request.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";
        var traceId = context.TraceIdentifier;
        var instance = context.Request.Path;

        ProblemDetailsResponse problemDetails = exception switch
        {
            ValidationException validationEx =>
                CreateValidationErrorResponse(validationEx, instance, traceId),

            UnauthorizedAccessException unauthorizedEx =>
                ProblemDetailsResponse.Unauthorized(
                    unauthorizedEx.Message,
                    instance.Value,
                    traceId),

            ArgumentNullException argNullEx =>
                ProblemDetailsResponse.ValidationError(
                    $"Required parameter missing: {argNullEx.ParamName}",
                    new Dictionary<string, string[]>
                    {
                        [argNullEx.ParamName ?? "parameter"] = new[] { argNullEx.Message }
                    },
                    instance.Value,
                    traceId),

            InvalidOperationException invalidOpEx =>
                ProblemDetailsResponse.Conflict(
                    invalidOpEx.Message,
                    instance.Value,
                    traceId),

            ArgumentException argEx =>
                ProblemDetailsResponse.ValidationError(
                    argEx.Message,
                    new Dictionary<string, string[]>
                    {
                        [argEx.ParamName ?? "argument"] = new[] { argEx.Message }
                    },
                    instance.Value,
                    traceId),

            _ => ProblemDetailsResponse.InternalServerError(
                "An unexpected error occurred. Please try again later.",
                instance.Value,
                traceId)
        };

        context.Response.StatusCode = problemDetails.Status;

        var json = JsonSerializer.Serialize(
            problemDetails,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        return context.Response.WriteAsJsonAsync(problemDetails);
    }

    private static ProblemDetailsResponse CreateValidationErrorResponse(
        ValidationException ex,
        PathString instance,
        string traceId)
    {
        var errors = new Dictionary<string, string[]>();
        foreach (var failure in ex.Errors.GroupBy(x => x.PropertyName))
        {
            errors[failure.Key] = failure.Select(x => x.ErrorMessage).ToArray();
        }

        return ProblemDetailsResponse.ValidationError(
            "One or more validation errors occurred.",
            errors,
            instance.Value,
            traceId);
    }
}

/// <summary>
/// Custom validation exception for validation errors.
/// </summary>
public class ValidationException : Exception
{
    public List<ValidationFailure> Errors { get; } = new();

    public ValidationException(string message) : base(message)
    {
    }

    public ValidationException(List<ValidationFailure> errors) : base("Validation failed")
    {
        Errors = errors;
    }
}

/// <summary>
/// Represents a single validation failure.
/// </summary>
public class ValidationFailure
{
    public string PropertyName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}
