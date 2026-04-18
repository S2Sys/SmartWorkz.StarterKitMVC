using System.Data;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.SqlServer;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using SmartWorkz.StarterKitMVC.Application.Abstractions;
using SmartWorkz.StarterKitMVC.Application.Authorization;
using SmartWorkz.StarterKitMVC.Application.Localization;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using SmartWorkz.StarterKitMVC.Application.Services;
using SmartWorkz.StarterKitMVC.Infrastructure.Authorization;
using SmartWorkz.StarterKitMVC.Infrastructure.Data;
using SmartWorkz.StarterKitMVC.Infrastructure.EmailTemplates;
using SmartWorkz.StarterKitMVC.Infrastructure.Repositories;
using SmartWorkz.StarterKitMVC.Infrastructure.BackgroundJobs;
using SmartWorkz.StarterKitMVC.Infrastructure.Services;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds infrastructure services including DbContexts, repositories, and application services
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext registration
        services.AddDbContext<MasterDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddDbContext<SharedDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddDbContext<TransactionDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddDbContext<ReportDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddDbContext<AuthDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Register IDbConnection for Dapper repositories
        services.AddScoped<IDbConnection>(sp =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not configured.");
            return new SqlConnection(connectionString);
        });

        return services;
    }

    /// <summary>
    /// Adds repository services to the dependency injection container
    /// Uses Dapper for authentication and user data access
    /// </summary>
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEmailQueueRepository, EmailQueueRepository>();

        return services;
    }

    /// <summary>
    /// Adds domain services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IMenuService, MenuService>();
        services.AddScoped<ISeoMetaService, SeoMetaService>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<SmartWorkz.StarterKitMVC.Application.Authorization.IPermissionService, SmartWorkz.StarterKitMVC.Infrastructure.Authorization.PermissionService>();
        services.AddScoped<IClaimService, ClaimService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        // Translation service (DB-backed, memory cached)
        services.AddMemoryCache();
        services.AddScoped<ITranslationRepository, TranslationRepository>();
        services.AddSingleton<ITranslationService, TranslationService>();

        // Email templates (DB-backed, replacing JSON file storage)
        services.AddEmailTemplates(useSqlRepository: true);

        return services;
    }

    /// <summary>
    /// Adds distributed cache services.
    /// L1: IMemoryCache (in-process, 2 min TTL).
    /// L2: IDistributedCache (Redis if configured, else SQL Server fallback).
    /// </summary>
    private static IServiceCollection AddCacheServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // L1: IMemoryCache already registered by AddMemoryCache() above
        // L2: Configure distributed cache (Redis primary, SQL Server fallback)
        var redisConn = configuration.GetConnectionString("Redis");

        if (!string.IsNullOrEmpty(redisConn))
        {
            // Redis as L2 distributed cache
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConn;
                options.InstanceName = "StarterKit_";
            });
        }
        else
        {
            // SQL Server fallback for L2 distributed cache
            // NOTE: AddSqlServerCache requires Microsoft.Extensions.Caching.SqlServer package
            // and uses the Master.CacheEntries table managed by the SQL migration
            // TODO: Enable when package dependencies are fully resolved
            // services.AddSqlServerCache(options =>
            // {
            //     options.ConnectionString = configuration.GetConnectionString("DefaultConnection");
            //     options.SchemaName = "Master";
            //     options.TableName = "CacheEntries";
            // });

            // Fallback: use memory cache as L2 (same as L1 for now)
            services.AddDistributedMemoryCache();
        }

        services.AddSingleton<ICacheService, HybridCacheService>();
        return services;
    }

    /// <summary>
    /// Adds JWT Bearer authentication
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var secret = configuration["Features:Authentication:Jwt:Secret"];
        var issuer = configuration["Features:Authentication:Jwt:Issuer"];
        var audience = configuration["Features:Authentication:Jwt:Audience"];

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        return services;
    }

    /// <summary>
    /// Adds all infrastructure and application services at once
    /// </summary>
    public static IServiceCollection AddApplicationStack(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddInfrastructureServices(configuration);
        services.AddRepositories();
        services.AddApplicationServices();
        services.AddCacheServices(configuration);
        services.AddJwtAuthentication(configuration);

        // Translation cache warm-up at startup
        services.AddHostedService<TranslationCacheWarmupService>();

        return services;
    }
}
