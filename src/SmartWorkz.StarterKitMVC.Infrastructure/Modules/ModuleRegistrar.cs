using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartWorkz.StarterKitMVC.Application.Abstractions;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Modules;

public static class ModuleRegistrar
{
    public static void RegisterModules(IServiceCollection services, IConfiguration configuration, params Assembly[] assemblies)
    {
        var moduleType = typeof(IModule);
        var types = assemblies.SelectMany(a => a.GetTypes())
            .Where(t => !t.IsAbstract && moduleType.IsAssignableFrom(t));

        foreach (var type in types)
        {
            if (Activator.CreateInstance(type) is IModule module)
            {
                module.Register(services, configuration);
            }
        }
    }
}
