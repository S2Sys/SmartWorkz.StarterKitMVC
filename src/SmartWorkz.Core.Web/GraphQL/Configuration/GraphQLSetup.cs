using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace SmartWorkz.Core.Web.GraphQL.Configuration;

/// <summary>
/// Extension methods for configuring GraphQL services and middleware.
/// Provides setup helpers for HotChocolate GraphQL endpoint with authentication and rate limiting.
/// </summary>
public static class GraphQLSetup
{
    /// <summary>
    /// Registers GraphQL infrastructure services in the DI container.
    /// The consuming application must also call services.AddGraphQLServer() from HotChocolate.AspNetCore
    /// to configure the GraphQL server with the Query type and schema types from this library.
    /// </summary>
    /// <example>
    /// In Program.cs:
    /// <code>
    /// services.AddGraphQLServices();
    /// services.AddGraphQLServer()
    ///     .AddQueryType&lt;Query&gt;()
    ///     .AddType&lt;UserType&gt;()
    ///     .AddType&lt;ProductType&gt;();
    /// </code>
    /// </example>
    public static IServiceCollection AddGraphQLServices(this IServiceCollection services)
    {
        services.AddSingleton<Middleware.GraphQLRateLimitStore>();
        return services;
    }

    /// <summary>
    /// Maps GraphQL endpoint and configures middleware.
    /// Note: The consuming application must call app.MapGraphQL("/graphql") directly
    /// since this library does not take a dependency on HotChocolate.AspNetCore.
    /// </summary>
    public static IEndpointRouteBuilder MapGraphQLEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints;
    }
}
