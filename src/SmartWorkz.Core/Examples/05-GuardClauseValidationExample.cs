namespace SmartWorkz.Core.Examples;

using SmartWorkz.Shared;

/// <summary>
/// Demonstrates Guard utility class for precondition validation.
/// Guards enforce invariants at domain boundaries (constructors, service methods).
/// </summary>
public class GuardClauseValidationExample
{
    public class Order
    {
        public Guid OrderId { get; }
        public string CustomerName { get; }
        public List<OrderItem> Items { get; }
        public decimal Total { get; }

        /// <summary>
        /// Constructor with comprehensive guard clauses.
        /// </summary>
        public Order(Guid orderId, string customerName, List<OrderItem> items, decimal total)
        {
            // Guard against invalid Guid (Guid.Empty)
            OrderId = Guard.NotDefault(orderId, nameof(orderId));

            // Guard against empty/null string
            CustomerName = Guard.NotEmpty(customerName, nameof(customerName));

            // Guard against null or empty collection
            Items = Guard.NotEmpty(items, nameof(items)).ToList();

            // Guard amount is non-negative
            Guard.InRange(total, 0, decimal.MaxValue, nameof(total));
            Total = total;
        }
    }

    public class OrderItem
    {
        public int ProductId { get; }
        public int Quantity { get; }
        public decimal Price { get; }

        public OrderItem(int productId, int quantity, decimal price)
        {
            // Guard ID is non-zero
            ProductId = Guard.NotDefault(productId, nameof(productId));

            // Guard quantity is positive
            Quantity = Guard.InRange(quantity, 1, int.MaxValue, nameof(quantity));

            // Guard price is non-negative
            Guard.InRange(price, 0, decimal.MaxValue, nameof(price));
            Price = price;
        }
    }

    /// <summary>
    /// Guard methods overview.
    /// </summary>
    public class GuardMethods
    {
        public void Example_NotNull()
        {
            // Validates reference type is not null
            var customer = new object();
            Guard.NotNull(customer, nameof(customer));
            System.Console.WriteLine("Reference type validated");
        }

        public void Example_NotEmpty_String()
        {
            // Validates string is not null/empty/whitespace
            var name = "John Doe";
            Guard.NotEmpty(name, nameof(name));
            System.Console.WriteLine("String validated");
        }

        public void Example_NotEmpty_Collection()
        {
            // Validates collection is not null/empty
            var items = new List<int> { 1, 2, 3 };
            Guard.NotEmpty(items, nameof(items));
            System.Console.WriteLine("Collection validated");
        }

        public void Example_NotDefault()
        {
            // Validates value is not default (not 0, not Guid.Empty)
            var id = 123;
            Guard.NotDefault(id, nameof(id));
            System.Console.WriteLine("ID validated");
        }

        public void Example_InRange()
        {
            // Validates value falls within [min, max] range
            var pageSize = 20;
            Guard.InRange(pageSize, 1, 100, nameof(pageSize));
            System.Console.WriteLine("Page size validated");
        }

        public void Example_Requires()
        {
            // Validates custom condition
            var startDate = System.DateTime.UtcNow;
            var endDate = startDate.AddDays(7);
            Guard.Requires(startDate < endDate, nameof(startDate),
                "Start date must be before end date");
            System.Console.WriteLine("Date range validated");
        }
    }

    /// <summary>
    /// Guard clauses in service methods.
    /// </summary>
    public class ServiceValidation
    {
        public Result<Order> CreateOrder(Guid orderId, string customerName, List<OrderItem> items)
        {
            // Validate all inputs at method entry
            try
            {
                var order = new Order(orderId, customerName, items, items.Sum(i => i.Quantity * i.Price));
                return Result.Ok(order);
            }
            catch (ArgumentException ex)
            {
                return Result.Fail<Order>("VALIDATION_ERROR", ex.Message);
            }
        }
    }
}
