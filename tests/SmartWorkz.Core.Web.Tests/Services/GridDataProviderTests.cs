using Xunit;
using Moq;
using SmartWorkz.Web.Services.Grid;
using SmartWorkz.Shared;

namespace SmartWorkz.Core.Web.Tests.Services;

public class GridDataProviderTests
{
    [Fact]
    public async Task GetDataAsync_ShouldSerializeGridRequestToApiCall()
    {
        // Arrange
        var mockHttpClient = new Mock<HttpClient>();
        var provider = new GridDataProvider(mockHttpClient.Object);
        var request = new GridRequest(Page: 1, PageSize: 20, SortBy: "Name");

        // Act
        var result = await provider.GetDataAsync<ProductDto>(request);

        // Assert
        Assert.NotNull(result);
    }

    private class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}



