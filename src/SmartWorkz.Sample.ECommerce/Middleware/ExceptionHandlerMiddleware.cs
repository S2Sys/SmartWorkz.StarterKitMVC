namespace SmartWorkz.Sample.ECommerce.Middleware;

using System.Net;
using System.Text.Json;
using SmartWorkz.Shared;

public class ExceptionHandlerMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlerMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            await WriteResponse(context, HttpStatusCode.BadRequest,
                ProblemDetailsResponse.ValidationError(
                    ex.Message, context.Request.Path,
                    new Dictionary<string, object> { ["errors"] = ex.Errors }));
        }
        catch (NotFoundException ex)
        {
            logger.LogWarning(ex, "Not found: {Message}", ex.Message);
            await WriteResponse(context, HttpStatusCode.NotFound,
                ProblemDetailsResponse.NotFound(ex.Message, context.Request.Path));
        }
        catch (UnauthorizedException ex)
        {
            logger.LogWarning(ex, "Unauthorized: {Message}", ex.Message);
            await WriteResponse(context, HttpStatusCode.Unauthorized,
                ProblemDetailsResponse.Unauthorized(ex.Message, context.Request.Path));
        }
        catch (BusinessException ex)
        {
            logger.LogWarning(ex, "Business rule violation: {Code} {Message}",
                ex.ErrorCode, ex.Message);
            await WriteResponse(context, HttpStatusCode.UnprocessableEntity,
                ProblemDetailsResponse.Custom(
                    type: null,
                    title: "Business Rule Violation",
                    status: 422,
                    detail: ex.Message,
                    instance: context.Request.Path));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception on {Path}", context.Request.Path);
            await WriteResponse(context, HttpStatusCode.InternalServerError,
                ProblemDetailsResponse.InternalServerError(
                    detail: "An unexpected error occurred.",
                    instance: context.Request.Path));
        }
    }

    private static async Task WriteResponse(
        HttpContext context,
        HttpStatusCode status,
        ProblemDetailsResponse problem)
    {
        // Only intercept API paths to avoid hijacking MVC error pages
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = (int)status;
            return;
        }
        context.Response.StatusCode = (int)status;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}
