using Microsoft.Extensions.Configuration;
using SmartWorkz.StarterKitMVC.Application.Abstractions;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Configuration;

/// <summary>
/// Implementation of application configuration provider.
/// </summary>
public sealed class AppConfigurationProvider : Application.Abstractions.IConfigurationProvider
{
    private readonly IConfiguration _configuration;

    public AppConfigurationProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string? Get(string key) => _configuration[key];

    public T? Get<T>(string key) => _configuration.GetValue<T>(key);
}
