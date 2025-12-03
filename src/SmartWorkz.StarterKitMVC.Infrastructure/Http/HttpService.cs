using System.Net.Http.Json;
using SmartWorkz.StarterKitMVC.Application.Abstractions;
using SmartWorkz.StarterKitMVC.Shared.Primitives;

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

    public async Task<ApiResponse<T>> SendAsync<T>(ApiRequest request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(request.Method, request.Path);

        if (request.Body is not null)
        {
            message.Content = JsonContent.Create(request.Body);
        }

        if (request.Headers is not null)
        {
            foreach (var (key, value) in request.Headers)
            {
                message.Headers.TryAddWithoutValidation(key, value);
            }
        }

        using var response = await _client.SendAsync(message, cancellationToken).ConfigureAwait(false);

        var apiResponse = new ApiResponse<T>
        {
            StatusCode = response.StatusCode,
            IsSuccess = response.IsSuccessStatusCode
        };

        if (response.IsSuccessStatusCode)
        {
            apiResponse = apiResponse with { Data = await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken) };
        }
        else
        {
            var error = await response.Content.ReadFromJsonAsync<ApiError>(cancellationToken: cancellationToken);
            apiResponse = apiResponse with { Error = error };
        }

        return apiResponse;
    }
}
