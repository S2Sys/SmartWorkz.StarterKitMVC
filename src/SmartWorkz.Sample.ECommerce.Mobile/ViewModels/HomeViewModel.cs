namespace SmartWorkz.ECommerce.Mobile;

using Microsoft.Extensions.Logging;
using SmartWorkz.Mobile;
using SmartWorkz.ECommerce.Mobile.Repositories;

public sealed class HomeViewModel : ViewModelBase
{
    private readonly ProductRepository _products;
    private readonly INavigationService _nav;
    private readonly IResponsiveService _responsive;
    private readonly ILogger<HomeViewModel> _logger;

    public ObservableCollection<ProductDto> Products { get; } = new();

    public int ColumnCount => _responsive.GetProfile().ColumnCount;

    public AsyncCommand RefreshCommand { get; }
    public AsyncCommand SelectProductCommand { get; }

    public HomeViewModel(
        ProductRepository products,
        INavigationService nav,
        IResponsiveService responsive,
        ILogger<HomeViewModel> logger)
    {
        _products   = Guard.NotNull(products,   nameof(products));
        _nav        = Guard.NotNull(nav,        nameof(nav));
        _responsive = Guard.NotNull(responsive, nameof(responsive));
        _logger     = Guard.NotNull(logger,     nameof(logger));

        RefreshCommand = CreateCommand(LoadProductsAsync);
        SelectProductCommand = new AsyncCommand(ExecuteSelectProduct);
    }

    public override async Task InitializeAsync() =>
        await LoadProductsAsync();

    private async Task LoadProductsAsync()
    {
        await RunBusyAsync(async () =>
        {
            var result = await _products.GetAllAsync();
            if (!result.Succeeded)
            {
                ErrorMessage = result.Error?.Message ?? "Failed to load products.";
                _logger.LogError("Failed to load products: {Error}", result.Error?.Message);
                return;
            }

            Products.Clear();
            foreach (var product in result.Data ?? Enumerable.Empty<ProductDto>())
            {
                Products.Add(product);
            }

            _logger.LogInformation("Loaded {ProductCount} products", Products.Count);
        });
    }

    private Task ExecuteSelectProduct(object? parameter)
    {
        if (parameter is not ProductDto product)
            return Task.CompletedTask;

        var parameters = new NavigationParameters { ["productId"] = product.Id };
        return _nav.NavigateToAsync("product-detail", parameters);
    }
}
