using Microsoft.Extensions.DependencyInjection;
using SmartWorkz.Core.Shared.Templates;

namespace SmartWorkz.Core.Shared.Extensions;

/// <summary>
/// Extension methods for IServiceCollection to register Core.Shared services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Core.Shared services including TemplateEngine for template rendering.
    /// </summary>
    public static IServiceCollection AddCoreSharedServices(this IServiceCollection services)
    {
        services.AddScoped<ITemplateEngine, TemplateEngine>();
        return services;
    }
}
