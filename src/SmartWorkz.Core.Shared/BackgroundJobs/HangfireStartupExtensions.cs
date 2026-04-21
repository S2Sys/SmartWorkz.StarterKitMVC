namespace SmartWorkz.Core.Shared.BackgroundJobs;

using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

public static class HangfireStartupExtensions
{
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

internal class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        try
        {
            var httpContext = context.GetHttpContext();
            return httpContext?.User?.Identity?.IsAuthenticated ?? false;
        }
        catch
        {
            return false;
        }
    }
}
