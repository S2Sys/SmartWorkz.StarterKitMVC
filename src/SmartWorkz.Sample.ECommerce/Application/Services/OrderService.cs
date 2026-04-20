using SmartWorkz.Core;
using SmartWorkz.Core.Abstractions;
using SmartWorkz.Core.Shared.Events;
using SmartWorkz.Core.Shared.Guards;
using SmartWorkz.Core.ValueObjects;
using SmartWorkz.Sample.ECommerce.Application.DTOs;
using SmartWorkz.Sample.ECommerce.Application.Validators;
using SmartWorkz.Sample.ECommerce.Domain.Entities;
using SmartWorkz.Sample.ECommerce.Domain.Enums;

namespace SmartWorkz.Sample.ECommerce.Application.Services;

public class OrderService(
    IRepository<Order, int> orderRepo,
    CheckoutValidator checkoutValidator)
{
    public async Task<Result<int>> PlaceOrderAsync(int customerId, CartDto cart, CheckoutDto checkout)
    {
        Guard.NotNull(cart, nameof(cart));
        Guard.NotNull(checkout, nameof(checkout));

        var validation = await checkoutValidator.ValidateAsync(checkout);
        if (!validation.IsValid)
            return Result<int>.Failure(new Error("Validation.Failed", validation.Failures.First().Message));

        if (!cart.Items.Any())
            return Result<int>.Failure(new Error("Order.CartEmpty", "Cart is empty"));

        var addressResult = Address.Create(checkout.Street, checkout.City, checkout.State, checkout.PostalCode, checkout.Country);
        if (!addressResult.Succeeded)
            return Result<int>.Failure(addressResult.Error!);

        var totalResult = Money.Create(cart.Total, "USD");
        if (!totalResult.Succeeded)
            return Result<int>.Failure(totalResult.Error!);

        var order = new Order
        {
            CustomerId = customerId,
            ShippingAddress = addressResult.Data!,
            Total = totalResult.Data!
        };

        foreach (var item in cart.Items)
        {
            var itemPriceResult = Money.Create(item.UnitPrice, "USD");
            if (!itemPriceResult.Succeeded)
                return Result<int>.Failure(itemPriceResult.Error!);

            order.Items.Add(new OrderItem
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                UnitPrice = itemPriceResult.Data!
            });
        }

        order.Place();
        await orderRepo.AddAsync(order);

        // Domain events are typically published after persistence in a real scenario
        // Clear events after handling
        order.ClearDomainEvents();

        return Result<int>.Success(order.Id);
    }
}
