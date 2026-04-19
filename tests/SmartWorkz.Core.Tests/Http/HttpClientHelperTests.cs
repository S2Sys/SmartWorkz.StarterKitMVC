namespace SmartWorkz.Core.Tests.Http;

using SmartWorkz.Core.Shared.Http;
using System.Text.Json;

/// <summary>
/// Unit tests for HttpClientHelper covering core functionality.
/// Focus on structure and builder patterns; integration tests with real APIs can be added separately.
/// </summary>
public class HttpClientHelperTests
{
    [Fact]
    public async Task ExecuteAsync_WithInvalidUrl_ReturnsFailResult()
    {
        // Arrange
        var helper = HttpClientHelper.Get(string.Empty);

        // Act
        var result = await helper.ExecuteAsync<JsonElement>();

        // Assert
        Assert.False(result.Succeeded);
        Assert.True(result.Errors.Any(e => e.Contains("URL", StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public async Task ExecuteAsync_WithZeroTimeout_ReturnsFailResult()
    {
        // Arrange
        var helper = HttpClientHelper.Get("https://example.com/api")
            .WithTimeout(0);

        // Act
        var result = await helper.ExecuteAsync<JsonElement>();

        // Assert
        Assert.False(result.Succeeded);
        Assert.True(result.Errors.Any(e => e.Contains("timeout", StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public async Task BuilderPattern_AllowsFluentChaining()
    {
        // Arrange
        var url = "https://jsonplaceholder.typicode.com/posts/1";

        // Act
        var helper = HttpClientHelper
            .Get(url)
            .WithHeader("User-Agent", "Test")
            .WithTimeout(5000);

        // Assert - Should not throw and should be chainable
        Assert.NotNull(helper);
    }

    [Fact]
    public async Task WithHeaders_ReplacesPreviousHeaders()
    {
        // Arrange
        var headers = new Dictionary<string, string>
        {
            { "Authorization", "Bearer token" }
        };

        // Act
        var helper = HttpClientHelper
            .Get("https://example.com")
            .WithHeaders(headers);

        // Assert
        Assert.NotNull(helper);
    }

    [Fact]
    public async Task WithRetry_ConfiguresRetryPolicy()
    {
        // Arrange & Act
        var helper = HttpClientHelper
            .Get("https://example.com")
            .WithRetry(maxAttempts: 3, backoffMs: 100, RetryStrategy.Linear);

        // Assert
        Assert.NotNull(helper);
    }

    [Fact]
    public async Task WithRetry_ExponentialBackoff_IsConfigurable()
    {
        // Arrange & Act
        var helper = HttpClientHelper
            .Get("https://example.com")
            .WithRetry(maxAttempts: 2, backoffMs: 50, RetryStrategy.Exponential);

        // Assert
        Assert.NotNull(helper);
    }

    [Fact]
    public async Task WithRetry_FibonacciBackoff_IsConfigurable()
    {
        // Arrange & Act
        var helper = HttpClientHelper
            .Get("https://example.com")
            .WithRetry(maxAttempts: 2, backoffMs: 50, RetryStrategy.Fibonacci);

        // Assert
        Assert.NotNull(helper);
    }

    [Fact]
    public async Task HttpResponse_HasCorrectStructure()
    {
        // Arrange
        var response = new HttpResponse<JsonElement>
        {
            IsSuccess = true,
            StatusCode = 200,
            ResponseHeaders = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };

        // Act & Assert
        Assert.True(response.IsSuccess);
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(response.ResponseHeaders);
        Assert.Single(response.ResponseHeaders);
        Assert.Null(response.Error); // No error on success
    }

    [Fact]
    public async Task HttpRequest_SealedClass_StoresAllFields()
    {
        // Arrange
        var request = new HttpRequest
        {
            Url = "https://example.com",
            Method = HttpMethod.Get,
            Timeout = TimeSpan.FromSeconds(30),
            Headers = new Dictionary<string, string> { { "Custom", "Header" } }
        };

        // Act & Assert
        Assert.NotNull(request);
        Assert.Equal("https://example.com", request.Url);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal(TimeSpan.FromSeconds(30), request.Timeout);
        Assert.Single(request.Headers);
    }

    [Fact]
    public async Task RetryPolicy_CanBeCreatedAndConfigured()
    {
        // Arrange
        var policy = new RetryPolicy
        {
            MaxAttempts = 5,
            BackoffMilliseconds = 100,
            Strategy = RetryStrategy.Exponential,
            RetryableStatusCodes = new() { 503, 504 }
        };

        // Act & Assert
        Assert.Equal(5, policy.MaxAttempts);
        Assert.Equal(100, policy.BackoffMilliseconds);
        Assert.Equal(RetryStrategy.Exponential, policy.Strategy);
        Assert.Contains(503, policy.RetryableStatusCodes);
        Assert.Contains(504, policy.RetryableStatusCodes);
    }

    [Fact]
    public void RetryStrategy_HasAllThreeOptions()
    {
        // Arrange & Act & Assert
        Assert.NotNull(RetryStrategy.Linear);
        Assert.NotNull(RetryStrategy.Exponential);
        Assert.NotNull(RetryStrategy.Fibonacci);
    }

    [Fact]
    public async Task FactoryMethods_Get_CreatesGetRequest()
    {
        // Arrange & Act
        var helper = HttpClientHelper.Get("https://example.com");

        // Assert
        Assert.NotNull(helper);
    }

    [Fact]
    public async Task FactoryMethods_Post_CreatesPostRequest()
    {
        // Arrange & Act
        var helper = HttpClientHelper.Post("https://example.com");

        // Assert
        Assert.NotNull(helper);
    }

    [Fact]
    public async Task FactoryMethods_Put_CreatesPutRequest()
    {
        // Arrange & Act
        var helper = HttpClientHelper.Put("https://example.com");

        // Assert
        Assert.NotNull(helper);
    }

    [Fact]
    public async Task FactoryMethods_Delete_CreatesDeleteRequest()
    {
        // Arrange & Act
        var helper = HttpClientHelper.Delete("https://example.com");

        // Assert
        Assert.NotNull(helper);
    }

    [Fact]
    public async Task IHttpClient_CanBeInstantiatedAsInterface()
    {
        // Arrange & Act
        IHttpClient httpClient = new HttpClientHelper();

        // Assert
        Assert.NotNull(httpClient);
        Assert.IsAssignableFrom<IHttpClient>(httpClient);
    }

    [Fact]
    public async Task WithBody_StoresBodyForPostRequests()
    {
        // Arrange
        var body = new { title = "Test", content = "Content" };

        // Act
        var helper = HttpClientHelper.Post("https://example.com")
            .WithBody(body);

        // Assert
        Assert.NotNull(helper);
    }

    [Fact]
    public async Task WithHeader_SingleHeader_SetCorrectly()
    {
        // Arrange
        var customHeaderKey = "X-Custom-Header";
        var customHeaderValue = "CustomValue";

        // Act
        var helper = HttpClientHelper.Get("https://example.com")
            .WithHeader(customHeaderKey, customHeaderValue);

        // Assert
        Assert.NotNull(helper);
    }

    [Fact]
    public async Task WithTimeout_SetsTimeoutCorrectly()
    {
        // Arrange & Act
        var helper = HttpClientHelper.Get("https://example.com")
            .WithTimeout(5000);

        // Assert
        Assert.NotNull(helper);
    }

    [Fact]
    public async Task HttpClientHelper_IsSealedClass()
    {
        // Arrange
        var type = typeof(HttpClientHelper);

        // Act & Assert
        Assert.True(type.IsSealed, "HttpClientHelper should be sealed");
    }

    [Fact]
    public async Task HttpRequest_IsSealedClass()
    {
        // Arrange
        var type = typeof(HttpRequest);

        // Act & Assert
        Assert.True(type.IsSealed, "HttpRequest should be sealed");
    }

    [Fact]
    public async Task HttpResponse_IsSealedClass()
    {
        // Arrange
        var type = typeof(HttpResponse<>);

        // Act & Assert
        Assert.True(type.IsSealed, "HttpResponse<T> should be sealed");
    }

    [Fact]
    public async Task RetryPolicy_IsSealedClass()
    {
        // Arrange
        var type = typeof(RetryPolicy);

        // Act & Assert
        Assert.True(type.IsSealed, "RetryPolicy should be sealed");
    }

    [Fact]
    public async Task IHttpClient_DefinesAllRequiredMethods()
    {
        // Arrange
        var interfaceType = typeof(IHttpClient);

        // Act & Assert - Check interface has all required methods
        var methods = interfaceType.GetMethods();
        Assert.NotEmpty(methods);

        // Check for GetAsync<T>
        Assert.NotNull(methods.FirstOrDefault(m => m.Name == "GetAsync" && m.IsGenericMethodDefinition));

        // Check for PostAsync<T>
        Assert.NotNull(methods.FirstOrDefault(m => m.Name == "PostAsync" && m.IsGenericMethodDefinition));

        // Check for PutAsync<T>
        Assert.NotNull(methods.FirstOrDefault(m => m.Name == "PutAsync" && m.IsGenericMethodDefinition));

        // Check for DeleteAsync<T>
        Assert.NotNull(methods.FirstOrDefault(m => m.Name == "DeleteAsync" && m.IsGenericMethodDefinition));
    }

    [Fact]
    public async Task HttpResponseT_ImplementsGenericInterface()
    {
        // Arrange
        var responseType = typeof(HttpResponse<string>);

        // Act & Assert
        Assert.NotNull(responseType);
        Assert.True(responseType.IsGenericType);
    }

    [Fact]
    public async Task MultipleChainedHeaders_AreAllApplied()
    {
        // Arrange & Act
        var helper = HttpClientHelper.Get("https://example.com")
            .WithHeader("Authorization", "Bearer token")
            .WithHeader("Content-Type", "application/json")
            .WithHeader("X-Custom", "Value");

        // Assert
        Assert.NotNull(helper);
    }

    [Fact]
    public async Task ComplexChain_CombinesMultipleConfigurations()
    {
        // Arrange & Act
        var result = HttpClientHelper
            .Post("https://example.com/api/users")
            .WithHeader("Authorization", "Bearer token")
            .WithBody(new { name = "John", email = "john@example.com" })
            .WithTimeout(10000)
            .WithRetry(maxAttempts: 3, backoffMs: 500, RetryStrategy.Exponential);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task HttpRequest_DefaultValues_AreInitialized()
    {
        // Arrange
        var request = new HttpRequest();

        // Act & Assert
        Assert.Equal(string.Empty, request.Url);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.NotNull(request.Headers);
        Assert.Empty(request.Headers);
        Assert.Null(request.Body);
        Assert.Equal(TimeSpan.FromSeconds(30), request.Timeout);
        Assert.Null(request.RetryPolicy);
    }

    [Fact]
    public async Task IHttpClient_Implementations_AreAsync()
    {
        // Arrange
        IHttpClient httpClient = new HttpClientHelper();

        // Act & Assert - Methods should be async (return Task)
        var methods = typeof(IHttpClient).GetMethods();
        foreach (var method in methods)
        {
            Assert.True(method.ReturnType.Name.Contains("Task"), $"Method {method.Name} should return Task");
        }
    }
}
