namespace SmartWorkz.Mobile.Tests.Services.Interceptors;

using Moq;
using System.Net;
using Microsoft.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

public class RequestLoggingInterceptorTests
{
    private readonly Mock<ILogger<RequestLoggingInterceptor>> _logger = new();

    [Fact]
    public async Task InterceptAsync_LogsRequestDetails()
    {
        // Arrange
        var interceptor = new RequestLoggingInterceptor(_logger.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/products");

        // Act
        await interceptor.InterceptAsync(request);

        // Assert
        _logger.Verify(
            l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("GET") &&
                    v.ToString()!.Contains("api.example.com/products")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task OnResponseAsync_LogsResponseStatusAndTiming()
    {
        // Arrange
        var interceptor = new RequestLoggingInterceptor(_logger.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test");

        // Act - log request first (starts timing)
        await interceptor.InterceptAsync(request);

        // Small delay to ensure measurable time passes
        await Task.Delay(5);

        // Now log response
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            RequestMessage = request
        };
        var result = await interceptor.OnResponseAsync(response);

        // Assert
        _logger.Verify(
            l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("200")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);

        Assert.False(result);  // Should not retry
    }

    [Fact]
    public async Task OnResponseAsync_BodyLoggingDisabledByDefault()
    {
        // Arrange
        var interceptor = new RequestLoggingInterceptor(_logger.Object, enableBodyLogging: false);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test");

        await interceptor.InterceptAsync(request);

        var responseBody = @"{""status"":""ok""}";
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseBody),
            RequestMessage = request
        };

        // Act
        await interceptor.OnResponseAsync(response);

        // Assert - body should NOT be logged
        _logger.Verify(
            l => l.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    !v.ToString()!.Contains(@"""status"":""ok""")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task OnResponseAsync_5xxResponseLoggedAsError()
    {
        // Arrange
        var interceptor = new RequestLoggingInterceptor(_logger.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test");

        await interceptor.InterceptAsync(request);

        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            RequestMessage = request
        };

        // Act
        var result = await interceptor.OnResponseAsync(response);

        // Assert
        _logger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("500")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        Assert.False(result);
    }

    [Fact]
    public async Task OnResponseAsync_WithBodyLoggingEnabled_LogsResponseBody()
    {
        // Arrange
        var interceptor = new RequestLoggingInterceptor(_logger.Object, enableBodyLogging: true);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test");

        await interceptor.InterceptAsync(request);

        var responseBody = @"{""data"":""value""}";
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseBody),
            RequestMessage = request
        };

        // Act
        await interceptor.OnResponseAsync(response);

        // Assert - body should be logged
        _logger.Verify(
            l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains(@"""data"":""value""")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
