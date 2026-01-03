using Microsoft.Extensions.DependencyInjection;
using SmartWorkz.StarterKitMVC.Application.Localization;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Localization;

/// <summary>
/// Extension methods for resource service initialization.
/// </summary>
public static class ResourceServiceExtensions
{
    /// <summary>
    /// Synchronizes default localization resources with existing data.
    /// Adds any new resources that don't exist yet without overwriting existing ones.
    /// Call this during application startup to ensure all resources are available.
    /// </summary>
    public static async Task SyncDefaultResourcesAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var resourceService = scope.ServiceProvider.GetRequiredService<IResourceService>();
        await resourceService.SyncDefaultResourcesAsync();
    }
}
