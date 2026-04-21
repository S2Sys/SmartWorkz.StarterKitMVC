using SmartWorkz.Core;
using SmartWorkz.Shared;
using SmartWorkz.Sample.ECommerce.Domain.Enums;

namespace SmartWorkz.Sample.ECommerce.Domain.Events;

public class OrderStatusChangedEvent : DomainEvent
{
    public int OrderId { get; set; }
    public OrderStatus OldStatus { get; set; }
    public OrderStatus NewStatus { get; set; }

    public OrderStatusChangedEvent(int orderId, OrderStatus oldStatus, OrderStatus newStatus)
    {
        OrderId = orderId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
    }
}
