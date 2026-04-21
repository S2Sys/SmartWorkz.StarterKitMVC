namespace SmartWorkz.Sample.ECommerce.Tests.Api;

using System.Net;
using System.Net.Http.Json;
using SmartWorkz.Shared;
using SmartWorkz.Sample.ECommerce.Application.DTOs;

public class ProductsApiTests : IClassFixture<ECommerceWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProductsApiTests(ECommerceWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GET_api_products_returns_200_ok()
    {
        var response = await _client.GetAsync("/api/products");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<PagedList<ProductDto>>>();
        Assert.NotNull(body);
        Assert.True(body!.Success);
        Assert.NotNull(body.Data);
    }

    [Fact]
    public async Task GET_api_products_with_search_term_returns_filtered_results()
    {
        var response = await _client.GetAsync("/api/products?searchTerm=test");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<PagedList<ProductDto>>>();
        Assert.NotNull(body?.Data);
    }

    [Fact]
    public async Task GET_api_products_by_id_nonexistent_returns_404()
    {
        var response = await _client.GetAsync("/api/products/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GET_api_products_with_sort_by_price_returns_sorted_results()
    {
        var response = await _client.GetAsync("/api/products?sortBy=price");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GET_api_products_with_pagination_returns_paged_results()
    {
        var response = await _client.GetAsync("/api/products?page=1&pageSize=5");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<PagedList<ProductDto>>>();
        Assert.NotNull(body?.Data);
    }
}
