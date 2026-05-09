namespace SmartWorkz.Mobile;

/// <summary>
/// Extends IResponseInterceptor to provide token refresh capability.
/// Automatically refreshes JWT tokens when 401 Unauthorized responses are encountered.
/// </summary>
public interface ITokenRefreshInterceptor : IResponseInterceptor
{
}
