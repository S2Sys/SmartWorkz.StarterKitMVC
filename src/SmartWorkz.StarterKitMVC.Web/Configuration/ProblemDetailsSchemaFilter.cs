using Microsoft.OpenApi.Models;
using SmartWorkz.StarterKitMVC.Shared.Primitives;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SmartWorkz.StarterKitMVC.Web.Configuration;

/// <summary>
/// Swagger schema filter to properly document ProblemDetails responses.
/// </summary>
public class ProblemDetailsSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type != typeof(ProblemDetailsResponse))
            return;

        schema.Title = "Problem Details";
        schema.Description = "RFC 7807 Problem Details response format";
        schema.Type = "object";
        schema.Properties = new Dictionary<string, OpenApiSchema>
        {
            ["type"] = new OpenApiSchema
            {
                Type = "string",
                Description = "A URI reference that identifies the problem type"
            },
            ["title"] = new OpenApiSchema
            {
                Type = "string",
                Description = "A short, human-readable summary of the problem type"
            },
            ["status"] = new OpenApiSchema
            {
                Type = "integer",
                Format = "int32",
                Description = "The HTTP status code"
            },
            ["detail"] = new OpenApiSchema
            {
                Type = "string",
                Description = "A human-readable explanation specific to this occurrence of the problem"
            },
            ["instance"] = new OpenApiSchema
            {
                Type = "string",
                Description = "A URI reference that identifies the specific occurrence of the problem"
            },
            ["timestamp"] = new OpenApiSchema
            {
                Type = "string",
                Format = "date-time",
                Description = "When the error occurred"
            },
            ["traceId"] = new OpenApiSchema
            {
                Type = "string",
                Description = "Correlation/trace ID for debugging"
            },
            ["errors"] = new OpenApiSchema
            {
                Type = "object",
                Description = "Detailed validation errors by field name",
                AdditionalProperties = new OpenApiSchema
                {
                    Type = "array",
                    Items = new OpenApiSchema { Type = "string" }
                }
            }
        };

        schema.Required = new HashSet<string> { "type", "title", "status", "detail" };
    }
}
