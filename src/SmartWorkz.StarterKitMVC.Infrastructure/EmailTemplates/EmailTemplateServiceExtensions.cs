using Microsoft.Extensions.DependencyInjection;
using SmartWorkz.StarterKitMVC.Application.EmailTemplates;

namespace SmartWorkz.StarterKitMVC.Infrastructure.EmailTemplates;

/// <summary>
/// Extension methods for registering email template services.
/// </summary>
public static class EmailTemplateServiceExtensions
{
    /// <summary>
    /// Adds email template services to the service collection.
    /// Supports both SQL Server (database-backed) and JSON file storage.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="useSqlRepository">If true, uses SQL Server repository; otherwise uses JSON file storage.</param>
    /// <param name="storagePath">Optional custom storage path for JSON templates (ignored if useSqlRepository=true).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddEmailTemplates(
        this IServiceCollection services,
        bool useSqlRepository = true,
        string? storagePath = null)
    {
        if (useSqlRepository)
        {
            // Use SQL Server repository (Dapper-based, reads from Master.ContentTemplates)
            services.AddScoped<IEmailTemplateRepository, DapperContentTemplateRepository>();
        }
        else
        {
            // Use JSON file repository (legacy fallback for development)
            services.AddSingleton<IEmailTemplateRepository>(sp =>
                new JsonEmailTemplateRepository(storagePath));
        }

        services.AddScoped<IEmailTemplateService, EmailTemplateService>();
        services.AddScoped<ITemplatedEmailSender, TemplatedEmailSender>();

        return services;
    }

    /// <summary>
    /// Seeds default email templates and sections if they don't exist.
    /// Call this during application startup.
    /// </summary>
    public static async Task SeedDefaultEmailTemplatesAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IEmailTemplateRepository>();

        // Seed default sections
        foreach (var section in DefaultEmailTemplates.AllSections)
        {
            var existing = await repository.GetSectionByIdAsync(section.Id);
            if (existing == null)
            {
                await repository.SaveSectionAsync(section);
            }
        }

        // Seed default templates
        foreach (var template in DefaultEmailTemplates.AllTemplates)
        {
            var existing = await repository.GetTemplateByIdAsync(template.Id);
            if (existing == null)
            {
                await repository.SaveTemplateAsync(template);
            }
        }
    }
}
