using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartWorkz.StarterKitMVC.Application.Mappings;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using SmartWorkz.StarterKitMVC.Application.Services;
using SmartWorkz.StarterKitMVC.Infrastructure.Data;

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

        return services;
    }

    /// <summary>
    /// Adds repository services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

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

        return services;
    }

    /// <summary>
    /// Adds AutoMapper with all mapping profiles
    /// </summary>
    public static IServiceCollection AddAutoMapperProfiles(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfiles));

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
        services.AddAutoMapperProfiles();

        return services;
    }
}
