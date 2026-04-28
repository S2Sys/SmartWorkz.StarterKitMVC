namespace SmartWorkz.Shared;

using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for configuring Hangfire background job processing in the dependency injection container and application pipeline.
/// </summary>
public static class HangfireStartupExtensions
{
    /// <summary>
    /// Adds Hangfire background job processing to the dependency injection container with SQL Server storage and job service registration.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="connectionString">SQL Server connection string for Hangfire job storage.</param>
    /// <param name="workerCount">Number of background job workers. Default is 20.</param>
    /// <returns>The configured service collection for method chaining.</returns>
    public static IServiceCollection AddHangfireBackgroundJobs(
        this IServiceCollection services,
        string connectionString,
        int workerCount = 20)
    {
        // Configure Hangfire with SQL Server storage
        services.AddHangfire(config =>
        {
            config.UseSqlServerStorage(connectionString, new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true
            });
        });

        // Add Hangfire server
        services.AddHangfireServer(options =>
        {
            options.WorkerCount = workerCount;
            options.ServerName = $"{Environment.MachineName}-{Guid.NewGuid()}";
        });

        // Register the background job service
        services.AddScoped<IBackgroundJobService, HangfireJobService>();
        return services;
    }

    /// <summary>
    /// Configures the Hangfire dashboard middleware in the application pipeline at the "/admin/jobs" route.
    /// Includes authentication-based authorization using HangfireAuthorizationFilter.
    /// </summary>
    /// <param name="app">The application builder for configuring the request pipeline.</param>
    /// <returns>The configured application builder for method chaining.</returns>
    public static IApplicationBuilder UseHangfireDashboard(this IApplicationBuilder app)
    {
        // Use Hangfire dashboard
        app.UseHangfireDashboard("/admin/jobs", new DashboardOptions
        {
            Authorization = new[] { new HangfireAuthorizationFilter() }
        });
        return app;
    }
}

/// <summary>
/// Authorization filter for Hangfire dashboard that requires authenticated users.
/// Checks the HttpContext's User identity to determine dashboard access permission.
/// </summary>
internal class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    /// <summary>
    /// Authorizes dashboard access by verifying user authentication status.
    /// </summary>
    /// <param name="context">The dashboard context containing HTTP context and user information.</param>
    /// <returns>True if the user is authenticated and authorized; otherwise, false.</returns>
    public bool Authorize(DashboardContext context)
    {
        try
        {
            var httpContext = context.GetHttpContext();
            var isAuthenticated = httpContext?.User?.Identity?.IsAuthenticated ?? false;

            if (!isAuthenticated)
            {
                // Unauthenticated access attempt
                System.Diagnostics.Debug.WriteLine("Unauthenticated access to Hangfire dashboard");
            }

            return isAuthenticated;
        }
        catch (Exception ex)
        {
            // Authorization check failed - log and deny access
            System.Diagnostics.Debug.WriteLine($"Failed to authorize dashboard access: {ex.Message}");
            return false;
        }
    }
}
