using SmartWorkz.Core.Abstractions;

namespace SmartWorkz.Core.Services;

/// <summary>
/// Base class for all domain services
/// Provides common functionality and conventions for service implementations
/// </summary>
public abstract class ServiceBase : IService
{
    protected ServiceBase()
    {
    }
}
