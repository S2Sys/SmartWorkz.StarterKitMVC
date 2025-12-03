namespace SmartWorkz.StarterKitMVC.Application.Abstractions;

public interface IModule
{
    string Name { get; }
    void Register(IServiceCollection services, IConfiguration configuration);
}
