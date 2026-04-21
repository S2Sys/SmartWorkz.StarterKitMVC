namespace SmartWorkz.Sample.ECommerce.Tests.Api;

using System.Net;
using System.Net.Http.Json;
using SmartWorkz.Sample.ECommerce.Application.DTOs;

public class OrdersApiTests : IClassFixture<ECommerceWebApplicationFactory>
{
    private readonly HttpClient _client;

    public OrdersApiTests(ECommerceWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GET_api_orders_without_auth_returns_401()
    {
        var response = await _client.GetAsync("/api/orders");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task POST_api_orders_without_auth_returns_401()
    {
        var checkout = new CheckoutDto("123 Main", "NYC", "NY", "10001", "USA");
        var response = await _client.PostAsJsonAsync("/api/orders", checkout);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GET_api_orders_by_id_without_auth_returns_401()
    {
        var response = await _client.GetAsync("/api/orders/1");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GET_api_categories_nonexistent_returns_404()
    {
        var response = await _client.GetAsync("/api/categories/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DELETE_api_products_nonexistent_returns_404()
    {
        var response = await _client.DeleteAsync("/api/products/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
