using System.Net.Http.Json;
using SmartWorkz.StarterKitMVC.Application.Abstractions;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Http;

/// <summary>
/// Default implementation of <see cref="IHttpService"/> using HttpClient.
/// </summary>
public sealed class HttpService : IHttpService
{
    private readonly HttpClient _client;

    public HttpService(HttpClient client)
    {
        _client = client;
    }

    public async Task<HttpResult<T>> GetAsync<T>(string path, CancellationToken ct = default)
    {
        try
        {
            var response = await _client.GetAsync(path, ct);
            return await CreateResultAsync<T>(response, ct);
        }
        catch (Exception ex)
        {
            return new HttpResult<T>(false, default, ex.Message, 0);
        }
    }

    public async Task<HttpResult<T>> PostAsync<T>(string path, object? body = null, CancellationToken ct = default)
    {
        try
        {
            var response = await _client.PostAsJsonAsync(path, body, ct);
            return await CreateResultAsync<T>(response, ct);
        }
        catch (Exception ex)
        {
            return new HttpResult<T>(false, default, ex.Message, 0);
        }
    }

    public async Task<HttpResult<T>> PutAsync<T>(string path, object? body = null, CancellationToken ct = default)
    {
        try
        {
            var response = await _client.PutAsJsonAsync(path, body, ct);
            return await CreateResultAsync<T>(response, ct);
        }
        catch (Exception ex)
        {
            return new HttpResult<T>(false, default, ex.Message, 0);
        }
    }

    public async Task<HttpResult<T>> DeleteAsync<T>(string path, CancellationToken ct = default)
    {
        try
        {
            var response = await _client.DeleteAsync(path, ct);
            return await CreateResultAsync<T>(response, ct);
        }
        catch (Exception ex)
        {
            return new HttpResult<T>(false, default, ex.Message, 0);
        }
    }

    private static async Task<HttpResult<T>> CreateResultAsync<T>(HttpResponseMessage response, CancellationToken ct)
    {
        var statusCode = (int)response.StatusCode;
        
        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadFromJsonAsync<T>(cancellationToken: ct);
            return new HttpResult<T>(true, data, null, statusCode);
        }
        
        var error = await response.Content.ReadAsStringAsync(ct);
        return new HttpResult<T>(false, default, error, statusCode);
    }
}
