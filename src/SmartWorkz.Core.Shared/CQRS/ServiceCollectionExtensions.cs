namespace SmartWorkz.Shared;

using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

/// <summary>
/// Extension methods for registering CQRS dispatchers and handlers in the dependency injection container.
/// </summary>
public static class CqrsServiceCollectionExtensions
{
    /// <summary>
    /// Adds CQRS dispatchers and automatically discovers and registers all command and query handlers.
    /// </summary>
    /// <param name="services">The service collection to register with.</param>
    /// <param name="assemblies">Assemblies to scan for command and query handlers. If empty, uses calling assembly.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <remarks>
    /// This method automatically discovers all types implementing ICommandHandler{T} and IQueryHandler{T,R}
    /// and registers them in the service container. Handlers can be in any assembly provided.
    /// Both ICommandDispatcher and IQueryDispatcher are registered as singletons.
    /// </remarks>
    public static IServiceCollection AddCqrs(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        if (assemblies == null || assemblies.Length == 0)
            assemblies = new[] { Assembly.GetCallingAssembly() };

        // Register dispatchers as singletons
        services.AddSingleton<ICommandDispatcher, CommandDispatcher>();
        services.AddSingleton<IQueryDispatcher, QueryDispatcher>();

        // Discover and register command handlers
        var commandHandlerType = typeof(ICommandHandler<>);
        foreach (var assembly in assemblies)
        {
            var handlers = assembly.GetTypes()
                .Where(t => t is { IsClass: true, IsAbstract: false } &&
                           t.GetInterfaces().Any(i =>
                               i.IsGenericType &&
                               i.GetGenericTypeDefinition() == commandHandlerType))
                .ToList();

            foreach (var handler in handlers)
            {
                var handlerInterfaces = handler.GetInterfaces()
                    .Where(i => i.IsGenericType &&
                               i.GetGenericTypeDefinition() == commandHandlerType)
                    .ToList();

                foreach (var @interface in handlerInterfaces)
                {
                    services.AddScoped(@interface, handler);
                }
            }
        }

        // Discover and register query handlers
        var queryHandlerType = typeof(IQueryHandler<,>);
        foreach (var assembly in assemblies)
        {
            var handlers = assembly.GetTypes()
                .Where(t => t is { IsClass: true, IsAbstract: false } &&
                           t.GetInterfaces().Any(i =>
                               i.IsGenericType &&
                               i.GetGenericTypeDefinition() == queryHandlerType))
                .ToList();

            foreach (var handler in handlers)
            {
                var handlerInterfaces = handler.GetInterfaces()
                    .Where(i => i.IsGenericType &&
                               i.GetGenericTypeDefinition() == queryHandlerType)
                    .ToList();

                foreach (var @interface in handlerInterfaces)
                {
                    services.AddScoped(@interface, handler);
                }
            }
        }

        return services;
    }
}
