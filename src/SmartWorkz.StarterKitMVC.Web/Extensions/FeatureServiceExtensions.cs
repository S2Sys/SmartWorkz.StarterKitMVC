using SmartWorkz.StarterKitMVC.Application.Configuration;

namespace SmartWorkz.StarterKitMVC.Web.Extensions;

/// <summary>
/// Extension methods for registering plug-and-play features based on configuration.
/// </summary>
public static class FeatureServiceExtensions
{
    /// <summary>
    /// Adds all enabled features from configuration.
    /// </summary>
    public static IServiceCollection AddFeatures(this IServiceCollection services, IConfiguration configuration)
    {
        var features = configuration.GetSection(FeatureOptions.SectionName).Get<FeatureOptions>() ?? new FeatureOptions();
        
        services.Configure<FeatureOptions>(configuration.GetSection(FeatureOptions.SectionName));

        // Identity
        if (features.Identity.Enabled)
        {
            services.AddIdentityFeature(features.Identity);
        }

        // Authentication (JWT, OAuth)
        if (features.Authentication.Jwt.Enabled || features.Authentication.OAuth.Google.Enabled ||
            features.Authentication.OAuth.Microsoft.Enabled || features.Authentication.OAuth.GitHub.Enabled)
        {
            services.AddAuthenticationFeature(features.Authentication, configuration);
        }

        // Multi-Tenancy
        if (features.MultiTenancy.Enabled)
        {
            services.AddMultiTenancyFeature(features.MultiTenancy);
        }

        // Caching
        if (features.Caching.Enabled)
        {
            services.AddCachingFeature(features.Caching, configuration);
        }

        // Background Jobs
        if (features.BackgroundJobs.Enabled)
        {
            services.AddBackgroundJobsFeature(features.BackgroundJobs, configuration);
        }

        // Event Bus
        if (features.EventBus.Enabled)
        {
            services.AddEventBusFeature(features.EventBus, configuration);
        }

        // Notifications
        if (features.Notifications.Enabled)
        {
            services.AddNotificationsFeature(features.Notifications);
        }

        // Storage
        if (features.Storage.Enabled)
        {
            services.AddStorageFeature(features.Storage);
        }

        // AI
        if (features.AI.Enabled)
        {
            services.AddAiFeature(features.AI);
        }

        // Rate Limiting
        if (features.RateLimiting.Enabled)
        {
            services.AddRateLimitingFeature(features.RateLimiting);
        }

        // Health Checks
        if (features.HealthChecks.Enabled)
        {
            services.AddHealthChecksFeature(features.HealthChecks);
        }

        // Swagger
        if (features.Swagger.Enabled)
        {
            services.AddSwaggerFeature(features.Swagger);
        }

        // Localization
        if (features.Localization.Enabled)
        {
            services.AddLocalizationFeature(features.Localization);
        }

        // Compression
        if (features.Compression.Enabled)
        {
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = features.Compression.EnableForHttps;
            });
        }

        // CORS
        if (features.Cors.Enabled)
        {
            services.AddCorsFeature(features.Cors);
        }

        return services;
    }

    /// <summary>
    /// Configures the middleware pipeline for enabled features.
    /// </summary>
    public static IApplicationBuilder UseFeatures(this IApplicationBuilder app, IConfiguration configuration)
    {
        var features = configuration.GetSection(FeatureOptions.SectionName).Get<FeatureOptions>() ?? new FeatureOptions();

        // Security
        if (features.Security.Https.Enabled)
        {
            if (features.Security.Https.UseHsts)
            {
                app.UseHsts();
            }
            if (features.Security.Https.RedirectHttps)
            {
                app.UseHttpsRedirection();
            }
        }

        // Compression
        if (features.Compression.Enabled)
        {
            app.UseResponseCompression();
        }

        // CORS
        if (features.Cors.Enabled)
        {
            app.UseCors(features.Cors.PolicyName);
        }

        // Localization
        if (features.Localization.Enabled)
        {
            var supportedCultures = features.Localization.SupportedCultures;
            app.UseRequestLocalization(options =>
            {
                options.SetDefaultCulture(features.Localization.DefaultCulture)
                       .AddSupportedCultures(supportedCultures)
                       .AddSupportedUICultures(supportedCultures);
            });
        }

        // Rate Limiting
        if (features.RateLimiting.Enabled)
        {
            app.UseRateLimiter();
        }

        // Health Checks
        if (features.HealthChecks.Enabled)
        {
            app.UseHealthChecks(features.HealthChecks.Path);
        }

        // Swagger
        if (features.Swagger.Enabled)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", features.Swagger.Title);
            });
        }

        return app;
    }

    #region Private Feature Registration Methods

    private static void AddIdentityFeature(this IServiceCollection services, IdentityFeatureOptions options)
    {
        // Register identity services based on provider
        // Actual implementation would configure ASP.NET Identity here
        Console.WriteLine($"[Feature] Identity enabled with provider: {options.Provider}");
    }

    private static void AddAuthenticationFeature(this IServiceCollection services, AuthenticationFeatureOptions options, IConfiguration configuration)
    {
        var authBuilder = services.AddAuthentication();

        // JWT
        if (options.Jwt.Enabled)
        {
            Console.WriteLine("[Feature] JWT Authentication enabled");
            // Configure JWT Bearer authentication
        }

        // Google OAuth
        if (options.OAuth.Google.Enabled)
        {
            Console.WriteLine("[Feature] Google OAuth enabled");
            // authBuilder.AddGoogle(...)
        }

        // Microsoft OAuth
        if (options.OAuth.Microsoft.Enabled)
        {
            Console.WriteLine("[Feature] Microsoft OAuth enabled");
            // authBuilder.AddMicrosoftAccount(...)
        }

        // GitHub OAuth
        if (options.OAuth.GitHub.Enabled)
        {
            Console.WriteLine("[Feature] GitHub OAuth enabled");
            // authBuilder.AddGitHub(...)
        }

        // Facebook OAuth
        if (options.OAuth.Facebook.Enabled)
        {
            Console.WriteLine("[Feature] Facebook OAuth enabled");
            // authBuilder.AddFacebook(...)
        }
    }

    private static void AddMultiTenancyFeature(this IServiceCollection services, MultiTenancyFeatureOptions options)
    {
        Console.WriteLine($"[Feature] Multi-Tenancy enabled with strategy: {options.Strategy}");
        // Register tenant resolver, context, etc.
    }

    private static void AddCachingFeature(this IServiceCollection services, CachingFeatureOptions options, IConfiguration configuration)
    {
        if (options.Provider == "Redis" && options.Redis.Enabled)
        {
            var redisConnection = configuration.GetConnectionString("Redis");
            if (!string.IsNullOrEmpty(redisConnection))
            {
                services.AddStackExchangeRedisCache(opt =>
                {
                    opt.Configuration = redisConnection;
                    opt.InstanceName = options.Redis.InstanceName;
                });
                Console.WriteLine("[Feature] Redis caching enabled");
            }
        }
        else
        {
            services.AddMemoryCache();
            services.AddDistributedMemoryCache();
            Console.WriteLine("[Feature] In-Memory caching enabled");
        }
    }

    private static void AddBackgroundJobsFeature(this IServiceCollection services, BackgroundJobsFeatureOptions options, IConfiguration configuration)
    {
        if (options.Hangfire.Enabled)
        {
            Console.WriteLine("[Feature] Hangfire background jobs enabled");
            // services.AddHangfire(...)
        }
        else if (options.Quartz.Enabled)
        {
            Console.WriteLine("[Feature] Quartz background jobs enabled");
            // services.AddQuartz(...)
        }
        else
        {
            Console.WriteLine("[Feature] In-Memory background jobs enabled");
            // Use built-in IHostedService
        }
    }

    private static void AddEventBusFeature(this IServiceCollection services, EventBusFeatureOptions options, IConfiguration configuration)
    {
        Console.WriteLine($"[Feature] Event Bus enabled with provider: {options.Provider}");
        
        if (options.RabbitMQ.Enabled)
        {
            // Configure RabbitMQ
        }
        else if (options.AzureServiceBus.Enabled)
        {
            // Configure Azure Service Bus
        }
        else if (options.Kafka.Enabled)
        {
            // Configure Kafka
        }
        // Default: InMemory event bus
    }

    private static void AddNotificationsFeature(this IServiceCollection services, NotificationsFeatureOptions options)
    {
        if (options.Email.Enabled)
        {
            Console.WriteLine($"[Feature] Email notifications enabled with provider: {options.Email.Provider}");
        }

        if (options.Sms.Enabled)
        {
            Console.WriteLine($"[Feature] SMS notifications enabled with provider: {options.Sms.Provider}");
        }

        if (options.Push.Enabled)
        {
            Console.WriteLine($"[Feature] Push notifications enabled with provider: {options.Push.Provider}");
        }

        if (options.SignalR.Enabled)
        {
            services.AddSignalR();
            Console.WriteLine("[Feature] SignalR real-time notifications enabled");
        }
    }

    private static void AddStorageFeature(this IServiceCollection services, StorageFeatureOptions options)
    {
        Console.WriteLine($"[Feature] Storage enabled with provider: {options.Provider}");
        
        if (options.Azure.Enabled)
        {
            // Configure Azure Blob Storage
        }
        else if (options.S3.Enabled)
        {
            // Configure AWS S3
        }
        // Default: Local file storage
    }

    private static void AddAiFeature(this IServiceCollection services, AiFeatureOptions options)
    {
        Console.WriteLine($"[Feature] AI enabled with provider: {options.Provider}");
        
        if (options.AzureOpenAI.Enabled)
        {
            // Configure Azure OpenAI
        }
        else
        {
            // Configure OpenAI
        }
    }

    private static void AddRateLimitingFeature(this IServiceCollection services, RateLimitingFeatureOptions options)
    {
        services.AddRateLimiter(limiter =>
        {
            limiter.AddFixedWindowLimiter("fixed", opt =>
            {
                opt.PermitLimit = options.PermitLimit;
                opt.Window = TimeSpan.FromSeconds(options.WindowSeconds);
                opt.QueueLimit = options.QueueLimit;
            });
        });
        Console.WriteLine($"[Feature] Rate limiting enabled: {options.PermitLimit} requests per {options.WindowSeconds}s");
    }

    private static void AddHealthChecksFeature(this IServiceCollection services, HealthChecksFeatureOptions options)
    {
        services.AddHealthChecks();
        Console.WriteLine("[Feature] Health checks enabled");
    }

    private static void AddSwaggerFeature(this IServiceCollection services, SwaggerFeatureOptions options)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc(options.Version, new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = options.Title,
                Version = options.Version,
                Description = options.Description
            });
        });
        Console.WriteLine("[Feature] Swagger API documentation enabled");
    }

    private static void AddLocalizationFeature(this IServiceCollection services, LocalizationFeatureOptions options)
    {
        services.AddLocalization(opt => opt.ResourcesPath = "Resources");
        Console.WriteLine($"[Feature] Localization enabled with default culture: {options.DefaultCulture}");
    }

    private static void AddCorsFeature(this IServiceCollection services, CorsFeatureOptions options)
    {
        services.AddCors(cors =>
        {
            cors.AddPolicy(options.PolicyName, policy =>
            {
                if (options.AllowedOrigins.Contains("*"))
                {
                    policy.AllowAnyOrigin();
                }
                else
                {
                    policy.WithOrigins(options.AllowedOrigins);
                }

                if (options.AllowedMethods.Contains("*"))
                {
                    policy.AllowAnyMethod();
                }
                else
                {
                    policy.WithMethods(options.AllowedMethods);
                }

                if (options.AllowedHeaders.Contains("*"))
                {
                    policy.AllowAnyHeader();
                }
                else
                {
                    policy.WithHeaders(options.AllowedHeaders);
                }

                if (options.AllowCredentials)
                {
                    policy.AllowCredentials();
                }
            });
        });
        Console.WriteLine("[Feature] CORS enabled");
    }

    #endregion
}
