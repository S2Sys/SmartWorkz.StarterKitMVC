namespace SmartWorkz.ECommerce.Mobile;

using Microsoft.Extensions.Logging;
using SmartWorkz.Mobile;
using SmartWorkz.ECommerce.Mobile.Repositories;
using SmartWorkz.Sample.ECommerce.Application.DTOs;

public sealed class OrdersViewModel : ViewModelBase
{
    private readonly OrderRepository _orders;
    private readonly ILogger<OrdersViewModel> _logger;

    public ObservableCollection<OrderDto> Orders { get; } = new();

    public AsyncCommand RefreshCommand { get; }

    public OrdersViewModel(
        OrderRepository orders,
        ILogger<OrdersViewModel> logger)
    {
        _orders = Guard.NotNull(orders, nameof(orders));
        _logger = Guard.NotNull(logger, nameof(logger));

        RefreshCommand = CreateCommand(LoadOrdersAsync);
    }

    public override async Task InitializeAsync() =>
        await LoadOrdersAsync();

    private async Task LoadOrdersAsync()
    {
        await RunBusyAsync(async () =>
        {
            var result = await _orders.GetMyOrdersAsync();
            if (!result.Succeeded)
            {
                ErrorMessage = result.Error?.Message ?? "Failed to load orders.";
                _logger.LogError("Failed to load orders: {Error}", result.Error?.Message);
                return;
            }

            Orders.Clear();
            var orderList = result.Data ?? Enumerable.Empty<OrderDto>();
            foreach (var order in orderList.OrderByDescending(o => o.PlacedAt))
            {
                Orders.Add(order);
            }

            _logger.LogInformation("Loaded {OrderCount} orders", Orders.Count);
        });
    }
}
