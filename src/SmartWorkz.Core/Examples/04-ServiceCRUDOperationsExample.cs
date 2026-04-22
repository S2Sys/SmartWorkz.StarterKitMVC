namespace SmartWorkz.Core.Examples;

using SmartWorkz.Shared;

/// <summary>
/// Demonstrates IService<TEntity, TDto> CRUD operations for business logic.
/// Services encapsulate domain operations, validation, and orchestration
/// while returning Result<T> for explicit error handling.
/// </summary>
public class ServiceCRUDOperationsExample
{
    public class Order : AuditableEntity
    {
        public int CustomerId { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; } = "Pending";

        public void Confirm()
        {
            if (Status == "Confirmed")
                throw new InvalidOperationException("Order already confirmed");
            Status = "Confirmed";
        }
    }

    public record CreateOrderDto(int CustomerId, decimal Total);
    public record OrderDto(int Id, int CustomerId, decimal Total, string Status);

    /// <summary>
    /// Service implementing IService<Order, OrderDto> interface.
    /// In real usage, repositories and unit of work would be injected.
    /// </summary>
    public class OrderService
    {
        /// <summary>
        /// CREATE: Insert order and return created DTO.
        /// Maps DTO -> Entity -> Persist -> Map back to DTO.
        /// </summary>
        public Result<OrderDto> CreateOrder(CreateOrderDto dto)
        {
            // Validate input
            if (dto.CustomerId <= 0)
                return Result.Fail<OrderDto>("INVALID_CUSTOMER", "Customer ID must be positive");
            if (dto.Total < 0)
                return Result.Fail<OrderDto>("INVALID_TOTAL", "Total must be non-negative");

            // Map DTO to entity
            var order = new Order
            {
                CustomerId = dto.CustomerId,
                Total = dto.Total,
                Status = "Pending"
            };

            // In real usage: await repository.AddAsync(order); await unitOfWork.SaveAsync();

            // Map entity back to DTO
            return Result.Ok(new OrderDto(order.Id, order.CustomerId, order.Total, order.Status));
        }

        /// <summary>
        /// READ: Retrieve order and return as DTO.
        /// </summary>
        public Result<OrderDto> GetOrderById(int id)
        {
            // In real usage: var order = await repository.GetByIdAsync(id);
            if (id <= 0)
                return Result.Fail<OrderDto>("NOT_FOUND", "Order not found");

            // Mock order
            var order = new Order { Id = id, CustomerId = 1, Total = 100m, Status = "Pending" };

            return Result.Ok(new OrderDto(order.Id, order.CustomerId, order.Total, order.Status));
        }

        /// <summary>
        /// UPDATE: Modify order and return updated DTO.
        /// </summary>
        public Result<OrderDto> UpdateOrder(int id, OrderDto dto)
        {
            // Fetch existing order
            var orderResult = GetOrderById(id);
            if (!orderResult.Succeeded)
                return Result.Fail<OrderDto>("NOT_FOUND", "Order not found");

            var order = new Order { Id = id, CustomerId = dto.CustomerId, Total = dto.Total, Status = dto.Status };

            // Apply updates
            // In real usage: await repository.UpdateAsync(order); await unitOfWork.SaveAsync();

            return Result.Ok(new OrderDto(order.Id, order.CustomerId, order.Total, order.Status));
        }

        /// <summary>
        /// DELETE: Remove order.
        /// For soft delete, use order.Delete(userId) and UpdateAsync.
        /// </summary>
        public Result<bool> DeleteOrder(int id)
        {
            if (id <= 0)
                return Result.Fail<bool>("NOT_FOUND", "Order not found");

            // In real usage: await repository.DeleteAsync(id); await unitOfWork.SaveAsync();
            return Result.Ok(true);
        }

        /// <summary>
        /// LIST: Retrieve all orders as DTOs.
        /// </summary>
        public Result<IReadOnlyCollection<OrderDto>> GetAllOrders()
        {
            // In real usage: var orders = await repository.GetAllAsync();
            var orders = new List<OrderDto> { };

            return Result.Ok<IReadOnlyCollection<OrderDto>>(orders);
        }
    }

    /// <summary>
    /// Demonstrates service usage in ASP.NET controller context.
    /// </summary>
    public class OrderController
    {
        private readonly OrderService _service;

        public OrderController(OrderService service)
        {
            _service = service;
        }

        /// <summary>
        /// POST /orders - Create order from DTO.
        /// </summary>
        public object CreateOrder(CreateOrderDto dto)
        {
            var result = _service.CreateOrder(dto);
            if (!result.Succeeded)
                return new { success = false, error = result.Error?.Code };
            return new { success = true, data = result.Data };
        }

        /// <summary>
        /// GET /orders/{id} - Get order by ID.
        /// </summary>
        public object GetOrder(int id)
        {
            var result = _service.GetOrderById(id);
            if (!result.Succeeded)
                return new { success = false, error = result.Error?.Code };
            return new { success = true, data = result.Data };
        }

        /// <summary>
        /// PUT /orders/{id} - Update order.
        /// </summary>
        public object UpdateOrder(int id, OrderDto dto)
        {
            var result = _service.UpdateOrder(id, dto);
            if (!result.Succeeded)
                return new { success = false, error = result.Error?.Code };
            return new { success = true, data = result.Data };
        }

        /// <summary>
        /// DELETE /orders/{id} - Delete order.
        /// </summary>
        public object DeleteOrder(int id)
        {
            var result = _service.DeleteOrder(id);
            if (!result.Succeeded)
                return new { success = false, error = result.Error?.Code };
            return new { success = true, message = "Order deleted" };
        }

        /// <summary>
        /// GET /orders - List all orders.
        /// </summary>
        public object GetAllOrders()
        {
            var result = _service.GetAllOrders();
            if (!result.Succeeded)
                return new { success = false, error = result.Error?.Code };
            return new { success = true, data = result.Data };
        }
    }
}
