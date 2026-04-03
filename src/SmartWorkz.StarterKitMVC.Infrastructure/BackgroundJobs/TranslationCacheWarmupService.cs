using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartWorkz.StarterKitMVC.Application.Localization;

namespace SmartWorkz.StarterKitMVC.Infrastructure.BackgroundJobs;

/// <summary>
/// Warms translation cache at application startup for all known tenants and locales.
/// Prevents first-request DB hit and ensures translations are available immediately.
/// </summary>
public sealed class TranslationCacheWarmupService(
    ITranslationService translationService,
    IConfiguration configuration,
    ILogger<TranslationCacheWarmupService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken ct)
    {
        try
        {
            // Read supported locales from config
            var locales = configuration
                .GetSection("Features:Localization:SupportedCultures")
                .Get<string[]>() ?? new[] { "en-US" };

            // Known seeded tenants
            var tenants = new[] { "DEFAULT", "DEMO" };

            logger.LogInformation("Starting translation cache warm-up for {TenantCount} tenants × {LocaleCount} locales",
                tenants.Length, locales.Length);

            foreach (var tenant in tenants)
            {
                foreach (var locale in locales)
                {
                    try
                    {
                        await translationService.WarmCacheAsync(tenant, locale);
                        logger.LogInformation("✓ Translations warmed: {Tenant}/{Locale}", tenant, locale);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "⚠ Translation warm-up skipped: {Tenant}/{Locale}", tenant, locale);
                        // Continue warming other locales even if one fails
                    }
                }
            }

            logger.LogInformation("✓ Translation cache warm-up complete");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "✗ Translation cache warm-up failed");
            // Don't throw — allow app to start even if translations are unavailable
        }
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
