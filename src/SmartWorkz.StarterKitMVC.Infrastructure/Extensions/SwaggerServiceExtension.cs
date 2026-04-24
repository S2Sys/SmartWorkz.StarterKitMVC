using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Extensions;

/// <summary>
/// Extension methods for configuring Swagger/OpenAPI documentation
/// </summary>
public static class SwaggerServiceExtension
{
    /// <summary>
    /// Adds Swagger/OpenAPI documentation services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddSwaggerDocumentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var swaggerOptions = configuration
            .GetSection("Features:Swagger")
            .Get<SwaggerFeatureOptions>()
            ?? new SwaggerFeatureOptions();

        if (!swaggerOptions.Enabled)
            return services;

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = swaggerOptions.Title ?? "SmartWorkz API",
                Version = swaggerOptions.Version ?? "v1",
                Description = swaggerOptions.Description ?? "Core API for SmartWorkz products",
                Contact = new OpenApiContact
                {
                    Name = swaggerOptions.ContactName,
                    Email = swaggerOptions.ContactEmail
                }
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using Bearer scheme",
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] { }
                }
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
                options.IncludeXmlComments(xmlPath);

            options.DocumentFilter<HiddenEndpointsFilter>();
        });

        return services;
    }

    /// <summary>
    /// Adds Swagger/OpenAPI middleware to the application pipeline
    /// </summary>
    public static WebApplication UseSwaggerDocumentation(
        this WebApplication app,
        IConfiguration configuration)
    {
        var swaggerOptions = configuration
            .GetSection("Features:Swagger")
            .Get<SwaggerFeatureOptions>();

        if (swaggerOptions?.Enabled != true)
            return app;

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartWorkz API v1");
            options.RoutePrefix = string.Empty;
            options.DocumentTitle = "SmartWorkz API Documentation";
        });

        return app;
    }
}

/// <summary>
/// Configuration options for Swagger/OpenAPI documentation
/// </summary>
public class SwaggerFeatureOptions
{
    public bool Enabled { get; set; } = true;
    public string Title { get; set; } = "SmartWorkz API";
    public string Version { get; set; } = "v1";
    public string Description { get; set; } = "Core API for SmartWorkz products";
    public string ContactName { get; set; } = "SmartWorkz Support";
    public string ContactEmail { get; set; } = "support@smartworkz.com";
}

/// <summary>
/// Document filter to hide internal and health check endpoints from Swagger
/// </summary>
public class HiddenEndpointsFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var pathsToRemove = swaggerDoc.Paths
            .Where(p => p.Key.Contains("/internal/") || p.Key.Contains("/health"))
            .ToList();

        foreach (var path in pathsToRemove)
            swaggerDoc.Paths.Remove(path.Key);
    }
}
