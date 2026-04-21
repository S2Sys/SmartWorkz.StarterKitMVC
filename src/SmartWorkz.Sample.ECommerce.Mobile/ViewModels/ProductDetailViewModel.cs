namespace SmartWorkz.ECommerce.Mobile;

using Microsoft.Extensions.Logging;
using SmartWorkz.Mobile;
using SmartWorkz.ECommerce.Mobile.Repositories;

public sealed class ProductDetailViewModel : ViewModelBase
{
    private readonly ProductRepository _products;
    private readonly CartViewModel _cart;
    private readonly INavigationService _nav;
    private readonly ILogger<ProductDetailViewModel> _logger;

    private ProductDto? _product;
    private int _quantity = 1;
    private int _productId;

    public ProductDto? Product
    {
        get => _product;
        private set => SetProperty(ref _product, value);
    }

    public int Quantity
    {
        get => _quantity;
        set => SetProperty(ref _quantity, value >= 1 ? value : 1);
    }

    public int ProductId
    {
        get => _productId;
        set
        {
            if (_productId != value)
            {
                SetProperty(ref _productId, value);
                if (value > 0)
                {
                    _ = LoadAsync();
                }
            }
        }
    }

    public AsyncCommand AddToCartCommand { get; }
    public AsyncCommand GoBackCommand { get; }
    public AsyncCommand IncreaseQuantityCommand { get; }
    public AsyncCommand DecreaseQuantityCommand { get; }

    public ProductDetailViewModel(
        ProductRepository products,
        CartViewModel cart,
        INavigationService nav,
        ILogger<ProductDetailViewModel> logger)
    {
        _products = Guard.NotNull(products, nameof(products));
        _cart     = Guard.NotNull(cart,     nameof(cart));
        _nav      = Guard.NotNull(nav,      nameof(nav));
        _logger   = Guard.NotNull(logger,   nameof(logger));

        AddToCartCommand = CreateCommand(ExecuteAddToCart);
        GoBackCommand    = CreateCommand(() => _nav.GoBackAsync());
        IncreaseQuantityCommand = CreateCommand(() => { Quantity++; return Task.CompletedTask; });
        DecreaseQuantityCommand = CreateCommand(() => { Quantity--; return Task.CompletedTask; });
    }

    private async Task LoadAsync()
    {
        if (_productId <= 0) return;

        await RunBusyAsync(async () =>
        {
            var result = await _products.GetByIdAsync(_productId);
            if (!result.Succeeded)
            {
                ErrorMessage = result.Error?.Message ?? "Failed to load product.";
                _logger.LogError("Failed to load product {ProductId}: {Error}", _productId, result.Error?.Message);
                return;
            }

            Product = result.Data;
            _logger.LogInformation("Loaded product {ProductId}: {ProductName}", _productId, Product?.Name);
        });
    }

    private Task ExecuteAddToCart()
    {
        if (Product is null)
        {
            ErrorMessage = "Product not loaded yet.";
            return Task.CompletedTask;
        }

        _cart.AddToCart(Product, Quantity);
        _logger.LogInformation("Added {Quantity} of {ProductName} to cart", Quantity, Product.Name);
        return _nav.GoBackAsync();
    }
}
