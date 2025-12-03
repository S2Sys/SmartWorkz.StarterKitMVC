using Microsoft.Extensions.Configuration;
using SmartWorkz.StarterKitMVC.Application.Abstractions;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Configuration;

public sealed class ConfigurationProvider : IConfigurationProvider
{
    private readonly IConfiguration _configuration;

    public ConfigurationProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string? Get(string key) => _configuration[key];

    public T? Get<T>(string key) => _configuration.GetValue<T>(key);
}
