namespace SmartWorkz.Sample.ECommerce.Middleware;

using System.Net;
using System.Text.Json;
using SmartWorkz.Shared;

public class ValidationMiddleware(RequestDelegate next, ILogger<ValidationMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            logger.LogWarning(ex,
                "Validation error on {Path}: {Message}",
                context.Request.Path, ex.Message);

            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/problem+json";

            var problem = ProblemDetailsResponse.ValidationError(
                detail: ex.Message,
                instance: context.Request.Path,
                extensions: new Dictionary<string, object>
                {
                    ["errors"] = ex.Errors
                });

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(problem));
        }
    }
}
