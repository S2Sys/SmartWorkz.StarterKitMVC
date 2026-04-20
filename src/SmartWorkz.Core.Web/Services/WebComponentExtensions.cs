namespace SmartWorkz.Core.Web.Services;

using Microsoft.Extensions.DependencyInjection;
using SmartWorkz.Core.Web.Services.Components;
using SmartWorkz.Core.Web.Services.DataView;

/// <summary>
/// Extension methods for registering SmartWorkz.Core.Web services with dependency injection.
/// </summary>
public static class WebComponentExtensions
{
    /// <summary>
    /// Register all SmartWorkz.Core.Web services and TagHelpers with the dependency injection container.
    /// Registers component services as singletons for optimal performance.
    /// Note: TagHelpers are auto-discovered by ASP.NET Core and do not require explicit registration.
    /// </summary>
    /// <param name="services">The IServiceCollection to register services with.</param>
    /// <returns>The IServiceCollection for method chaining.</returns>
    public static IServiceCollection AddSmartWorkzCoreWeb(this IServiceCollection services)
    {
        // Register component services as singletons (stateless services)
        services.AddSingleton<IIconProvider, IconProvider>();
        services.AddSingleton<IValidationMessageProvider, ValidationMessageProvider>();
        services.AddSingleton<IFormComponentProvider, FormComponentProvider>();
        services.AddSingleton<IAccessibilityService, AccessibilityService>();

        // TagHelpers are auto-discovered by ASP.NET Core and do not require explicit registration
        return services;
    }

    /// <summary>
    /// Register all SmartWorkz.Core.Web data view components and services.
    /// </summary>
    /// <param name="services">The IServiceCollection to register services with.</param>
    /// <returns>The IServiceCollection for method chaining.</returns>
    public static IServiceCollection AddSmartWorkzWebComponents(this IServiceCollection services)
    {
        // Register formatters
        services.AddScoped<IListViewFormatter, ListViewFormatter>();

        // ViewConfiguration can be added per-use or registered as singleton default
        services.AddScoped(_ => new ViewConfiguration
        {
            VisibleColumns = [],
            ItemsPerRow = 2,
            ShowHeaders = true,
            AllowRowSelection = true,
            DefaultPageSize = 20
        });

        return services;
    }
}
