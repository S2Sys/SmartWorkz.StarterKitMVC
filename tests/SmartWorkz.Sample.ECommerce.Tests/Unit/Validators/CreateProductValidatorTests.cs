namespace SmartWorkz.Sample.ECommerce.Tests.Unit.Validators;

using SmartWorkz.Sample.ECommerce.Application.Requests;
using SmartWorkz.Sample.ECommerce.Application.Validators;

public class CreateProductValidatorTests
{
    private readonly CreateProductValidator _validator = new();

    [Fact]
    public async Task Valid_request_passes_validation()
    {
        var request = new CreateProductRequest(
            "Widget Pro", "widget-pro", "A great widget", 29.99m, "USD", 10, true, 1);
        var result = await _validator.ValidateAsync(request);
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Empty_name_fails_validation()
    {
        var request = new CreateProductRequest(
            "", "widget-pro", null, 29.99m, "USD", 10, true, 1);
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Failures, f => f.PropertyName == "Name");
    }

    [Fact]
    public async Task Empty_slug_fails_validation()
    {
        var request = new CreateProductRequest(
            "Widget", "", null, 29.99m, "USD", 10, true, 1);
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Failures, f => f.PropertyName == "Slug");
    }

    [Fact]
    public async Task Negative_price_fails_validation()
    {
        var request = new CreateProductRequest(
            "Widget", "widget", null, -1m, "USD", 10, true, 1);
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Failures, f => f.PropertyName == "Price");
    }

    [Fact]
    public async Task Zero_category_id_fails_validation()
    {
        var request = new CreateProductRequest(
            "Widget", "widget", null, 10m, "USD", 10, true, 0);
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Failures, f => f.PropertyName == "CategoryId");
    }
}
