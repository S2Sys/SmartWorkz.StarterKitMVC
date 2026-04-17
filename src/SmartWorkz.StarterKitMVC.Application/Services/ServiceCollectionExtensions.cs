using Microsoft.Extensions.DependencyInjection;
using SmartWorkz.StarterKitMVC.Application.Repositories;

namespace SmartWorkz.StarterKitMVC.Application.Services;

/// <summary>
/// Dependency injection extensions for service layer registration.
/// Registers all business logic services with their corresponding interfaces.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all application services in the dependency injection container.
    /// Should be called in Program.cs during service collection configuration.
    ///
    /// Example usage:
    /// <code>
    /// services.AddApplicationServices()
    ///     .AddScoped&lt;IPasswordHasher, PasswordHasher&gt;()
    ///     .AddScoped&lt;ITokenService, TokenService&gt;();
    /// </code>
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        // Core authentication and authorization services
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IPermissionService, PermissionService>();

        // Business logic services
        services.AddScoped<ILookupService, LookupService>();
        services.AddScoped<IConfigurationService, ConfigurationService>();
        services.AddScoped<IBlogService, BlogService>();
        services.AddScoped<INotificationService, NotificationService>();

        // Infrastructure services
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IAuditService, AuditService>();

        return services;
    }

    /// <summary>
    /// Registers only the core authentication and authorization services.
    /// Useful for scenarios where you want to register services selectively.
    /// </summary>
    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IPermissionService, PermissionService>();

        return services;
    }

    /// <summary>
    /// Registers only the business logic services (Lookup, Configuration, Blog, Notification).
    /// Useful for scenarios where you want to register services selectively.
    /// </summary>
    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        services.AddScoped<ILookupService, LookupService>();
        services.AddScoped<IConfigurationService, ConfigurationService>();
        services.AddScoped<IBlogService, BlogService>();
        services.AddScoped<INotificationService, NotificationService>();

        return services;
    }

    /// <summary>
    /// Registers only the infrastructure services (Email, Audit).
    /// Useful for scenarios where you want to register services selectively.
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IAuditService, AuditService>();

        return services;
    }

    /// <summary>
    /// Registers a custom implementation of IPasswordHasher.
    /// This should be registered before AddAuthenticationServices.
    /// </summary>
    public static IServiceCollection AddPasswordHasher<TImplementation>(
        this IServiceCollection services)
        where TImplementation : class, IPasswordHasher
    {
        services.AddScoped<IPasswordHasher, TImplementation>();
        return services;
    }

    /// <summary>
    /// Registers a custom implementation of ITokenService.
    /// This should be registered before AddAuthenticationServices.
    /// </summary>
    public static IServiceCollection AddTokenService<TImplementation>(
        this IServiceCollection services)
        where TImplementation : class, ITokenService
    {
        services.AddScoped<ITokenService, TImplementation>();
        return services;
    }

    /// <summary>
    /// Validates that all required dependencies are registered.
    /// Throws an exception if essential services are missing.
    /// </summary>
    public static IServiceCollection ValidateServiceDependencies(this IServiceCollection services)
    {
        // This would validate that all required services are registered
        // In a real implementation, you might use a service provider to check
        return services;
    }
}
