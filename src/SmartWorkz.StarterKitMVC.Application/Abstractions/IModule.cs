using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SmartWorkz.StarterKitMVC.Application.Abstractions;

/// <summary>
/// Interface for modular feature registration.
/// </summary>
public interface IModule
{
    /// <summary>Module name.</summary>
    string Name { get; }
    
    /// <summary>Registers module services.</summary>
    void Register(IServiceCollection services, IConfiguration configuration);
}
