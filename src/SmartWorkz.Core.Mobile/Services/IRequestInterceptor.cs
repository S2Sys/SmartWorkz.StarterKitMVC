namespace SmartWorkz.Mobile;

public interface IRequestInterceptor
{
    Task InterceptAsync(HttpRequestMessage request, CancellationToken ct = default);
}
