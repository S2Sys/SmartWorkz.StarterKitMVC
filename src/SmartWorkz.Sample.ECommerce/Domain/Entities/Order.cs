using SmartWorkz.Core;
using SmartWorkz.Shared;
using SmartWorkz.Sample.ECommerce.Domain.Enums;
using SmartWorkz.Sample.ECommerce.Domain.Events;

namespace SmartWorkz.Sample.ECommerce.Domain.Entities;

public class Order : AggregateRoot<int>
{
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;
    public Address ShippingAddress { get; set; } = null!;
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public Money? Total { get; set; }
    public DateTime PlacedAt { get; set; }

    public void Place()
    {
        PlacedAt = DateTime.UtcNow;
        if (Total != null)
        {
            RaiseDomainEvent(new OrderPlacedEvent(Id, CustomerId, Total.Amount));
        }
    }

    public void ChangeStatus(OrderStatus newStatus)
    {
        var old = Status;
        Status = newStatus;
        RaiseDomainEvent(new OrderStatusChangedEvent(Id, old, newStatus));
    }
}
