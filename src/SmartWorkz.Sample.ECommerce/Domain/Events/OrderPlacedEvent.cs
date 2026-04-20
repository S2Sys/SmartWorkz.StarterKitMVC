using SmartWorkz.Core.Shared.Base_Classes;

namespace SmartWorkz.Sample.ECommerce.Domain.Events;

public class OrderPlacedEvent : DomainEvent
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public decimal Total { get; set; }

    public OrderPlacedEvent(int orderId, int customerId, decimal total)
    {
        OrderId = orderId;
        CustomerId = customerId;
        Total = total;
    }
}
