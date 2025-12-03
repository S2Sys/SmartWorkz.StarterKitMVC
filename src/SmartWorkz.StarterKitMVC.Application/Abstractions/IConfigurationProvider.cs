namespace SmartWorkz.StarterKitMVC.Application.Abstractions;

public interface IConfigurationProvider
{
    string? Get(string key);
    T? Get<T>(string key);
}
