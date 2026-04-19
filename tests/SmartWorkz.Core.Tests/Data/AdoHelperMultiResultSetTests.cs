namespace SmartWorkz.Core.Tests.Data;

using System.Data;
using Moq;
using SmartWorkz.Core.Shared.Data;
using SmartWorkz.Core.Shared.Results;

public class AdoHelperMultiResultSetTests
{
    // Test models
    private sealed class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    private sealed class Order
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }

    private sealed class Product
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    private sealed class Inventory
    {
        public int InventoryId { get; set; }
        public int Quantity { get; set; }
        public string Location { get; set; } = string.Empty;
    }

    // Mapper functions
    private static User MapUser(IDataReader reader)
    {
        return new User
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Email = reader.GetString(2)
        };
    }

    private static Order MapOrder(IDataReader reader)
    {
        return new Order
        {
            OrderId = reader.GetInt32(0),
            OrderNumber = reader.GetString(1),
            Total = reader.GetDecimal(2)
        };
    }

    private static Product MapProduct(IDataReader reader)
    {
        return new Product
        {
            ProductId = reader.GetInt32(0),
            ProductName = reader.GetString(1),
            Price = reader.GetDecimal(2)
        };
    }

    private static Inventory MapInventory(IDataReader reader)
    {
        return new Inventory
        {
            InventoryId = reader.GetInt32(0),
            Quantity = reader.GetInt32(1),
            Location = reader.GetString(2)
        };
    }

    // Tests for ExecuteQueryMultipleAsync<T1, T2>

    [Fact]
    public async Task ExecuteQueryMultipleAsync_TwoResultSets_ReturnsCorrectTuples()
    {
        // Arrange
        var mockReader = new Mock<IDataReader>();
        var mockCommand = new Mock<IDbCommand>();
        var mockConnection = new Mock<IDbConnection>();
        var mockProvider = new Mock<IDbProvider>();

        // Setup connection
        mockConnection.Setup(c => c.State).Returns(ConnectionState.Open);
        mockConnection.Setup(c => c.CreateCommand()).Returns(mockCommand.Object);

        // Setup reader
        mockReader
            .SetupSequence(r => r.Read())
            .Returns(true)   // User 1
            .Returns(true)   // User 2
            .Returns(false)  // End of first set
            .Returns(true)   // Order 1
            .Returns(true)   // Order 2
            .Returns(false); // End of second set

        mockReader
            .SetupSequence(r => r.NextResult())
            .Returns(true)   // Move to second set
            .Returns(false); // No more sets

        var readIndex = 0;
        mockReader
            .Setup(r => r.GetInt32(It.IsAny<int>()))
            .Returns(() => ++readIndex);

        mockReader
            .Setup(r => r.GetString(It.IsAny<int>()))
            .Returns(() => $"Value{readIndex}");

        mockReader
            .Setup(r => r.GetDecimal(It.IsAny<int>()))
            .Returns(() => readIndex * 10.0m);

        mockCommand.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(mockReader.Object);
        mockProvider.Setup(p => p.GetParameterPrefix()).Returns("@");

        // Act
        var result = await AdoHelper.ExecuteQueryMultipleAsync<User, Order>(
            mockConnection.Object,
            "EXEC sp_GetUsersAndOrders",
            mockProvider.Object,
            MapUser,
            MapOrder);

        // Assert
        Assert.True(result.Succeeded);
        var (returnedUsers, returnedOrders) = result.Data;
        Assert.Equal(2, returnedUsers.Count);
        Assert.Equal(2, returnedOrders.Count);
    }

    [Fact]
    public async Task ExecuteQueryMultipleAsync_NullMapper1_ReturnsFail()
    {
        // Arrange
        var mockConnection = new Mock<IDbConnection>();
        var mockProvider = new Mock<IDbProvider>();

        mockConnection.Setup(c => c.State).Returns(ConnectionState.Open);

        // Act
        var result = await AdoHelper.ExecuteQueryMultipleAsync<User, Order>(
            mockConnection.Object,
            "EXEC sp_Test",
            mockProvider.Object,
            null!,
            MapOrder);

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        Assert.Equal("ADO.MAPPER_NULL", result.Error.Code);
    }

    [Fact]
    public async Task ExecuteQueryMultipleAsync_NullMapper2_ReturnsFail()
    {
        // Arrange
        var mockConnection = new Mock<IDbConnection>();
        var mockProvider = new Mock<IDbProvider>();

        mockConnection.Setup(c => c.State).Returns(ConnectionState.Open);

        // Act
        var result = await AdoHelper.ExecuteQueryMultipleAsync<User, Order>(
            mockConnection.Object,
            "EXEC sp_Test",
            mockProvider.Object,
            MapUser,
            null!);

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        Assert.Equal("ADO.MAPPER_NULL", result.Error.Code);
    }

    [Fact]
    public async Task ExecuteQueryMultipleAsync_TwoSetsException_ReturnsFail()
    {
        // Arrange
        var mockConnection = new Mock<IDbConnection>();
        var mockCommand = new Mock<IDbCommand>();
        var mockProvider = new Mock<IDbProvider>();

        mockConnection.Setup(c => c.State).Returns(ConnectionState.Open);
        mockConnection.Setup(c => c.CreateCommand()).Returns(mockCommand.Object);
        mockCommand.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>()))
            .Throws(new InvalidOperationException("Connection error"));
        mockProvider.Setup(p => p.GetParameterPrefix()).Returns("@");

        // Act
        var result = await AdoHelper.ExecuteQueryMultipleAsync<User, Order>(
            mockConnection.Object,
            "EXEC sp_Test",
            mockProvider.Object,
            MapUser,
            MapOrder);

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        Assert.Equal("ADO.QUERY_MULTIPLE_FAILED", result.Error.Code);
    }

    // Tests for ExecuteQueryMultipleAsync<T1, T2, T3>

    [Fact]
    public async Task ExecuteQueryMultipleAsync_ThreeResultSets_ReturnsCorrectTuples()
    {
        // Arrange
        var users = new List<User>
        {
            new() { Id = 1, Name = "Alice", Email = "alice@example.com" }
        };

        var orders = new List<Order>
        {
            new() { OrderId = 101, OrderNumber = "ORD001", Total = 99.99m }
        };

        var products = new List<Product>
        {
            new() { ProductId = 201, ProductName = "Widget", Price = 29.99m }
        };

        var mockReader = new Mock<IDataReader>();
        var mockCommand = new Mock<IDbCommand>();
        var mockConnection = new Mock<IDbConnection>();
        var mockProvider = new Mock<IDbProvider>();

        mockConnection.Setup(c => c.State).Returns(ConnectionState.Open);
        mockConnection.Setup(c => c.CreateCommand()).Returns(mockCommand.Object);

        mockReader
            .SetupSequence(r => r.Read())
            .Returns(true)   // User 1
            .Returns(false)  // End of first set
            .Returns(true)   // Order 1
            .Returns(false)  // End of second set
            .Returns(true)   // Product 1
            .Returns(false); // End of third set

        mockReader
            .SetupSequence(r => r.NextResult())
            .Returns(true)   // Move to second set
            .Returns(true)   // Move to third set
            .Returns(false); // No more sets

        mockCommand.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(mockReader.Object);
        mockProvider.Setup(p => p.GetParameterPrefix()).Returns("@");

        // Act
        var result = await AdoHelper.ExecuteQueryMultipleAsync<User, Order, Product>(
            mockConnection.Object,
            "EXEC sp_GetMultiple",
            mockProvider.Object,
            MapUser,
            MapOrder,
            MapProduct);

        // Assert
        Assert.True(result.Succeeded);
        var (returnedUsers, returnedOrders, returnedProducts) = result.Data;
        Assert.NotEmpty(returnedUsers);
        Assert.NotEmpty(returnedOrders);
        Assert.NotEmpty(returnedProducts);
    }

    [Fact]
    public async Task ExecuteQueryMultipleAsync_ThreeSetsMissingMapper_ReturnsFail()
    {
        // Arrange
        var mockConnection = new Mock<IDbConnection>();
        var mockProvider = new Mock<IDbProvider>();

        mockConnection.Setup(c => c.State).Returns(ConnectionState.Open);

        // Act
        var result = await AdoHelper.ExecuteQueryMultipleAsync<User, Order, Product>(
            mockConnection.Object,
            "EXEC sp_Test",
            mockProvider.Object,
            MapUser,
            MapOrder,
            null!);

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        Assert.Equal("ADO.MAPPER_NULL", result.Error.Code);
    }

    [Fact]
    public async Task ExecuteQueryMultipleAsync_ThreeSetsException_ReturnsFail()
    {
        // Arrange
        var mockConnection = new Mock<IDbConnection>();
        var mockCommand = new Mock<IDbCommand>();
        var mockProvider = new Mock<IDbProvider>();

        mockConnection.Setup(c => c.State).Returns(ConnectionState.Open);
        mockConnection.Setup(c => c.CreateCommand()).Returns(mockCommand.Object);
        mockCommand.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>()))
            .Throws(new InvalidOperationException("Database error"));
        mockProvider.Setup(p => p.GetParameterPrefix()).Returns("@");

        // Act
        var result = await AdoHelper.ExecuteQueryMultipleAsync<User, Order, Product>(
            mockConnection.Object,
            "EXEC sp_Test",
            mockProvider.Object,
            MapUser,
            MapOrder,
            MapProduct);

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        Assert.Equal("ADO.QUERY_MULTIPLE_FAILED", result.Error.Code);
    }

    // Tests for ExecuteQueryMultipleAsync<T1, T2, T3, T4>

    [Fact]
    public async Task ExecuteQueryMultipleAsync_FourResultSets_ReturnsCorrectTuples()
    {
        // Arrange
        var users = new List<User>
        {
            new() { Id = 1, Name = "Alice", Email = "alice@example.com" }
        };

        var orders = new List<Order>
        {
            new() { OrderId = 101, OrderNumber = "ORD001", Total = 99.99m }
        };

        var products = new List<Product>
        {
            new() { ProductId = 201, ProductName = "Widget", Price = 29.99m }
        };

        var inventories = new List<Inventory>
        {
            new() { InventoryId = 301, Quantity = 50, Location = "Warehouse A" }
        };

        var mockReader = new Mock<IDataReader>();
        var mockCommand = new Mock<IDbCommand>();
        var mockConnection = new Mock<IDbConnection>();
        var mockProvider = new Mock<IDbProvider>();

        mockConnection.Setup(c => c.State).Returns(ConnectionState.Open);
        mockConnection.Setup(c => c.CreateCommand()).Returns(mockCommand.Object);

        mockReader
            .SetupSequence(r => r.Read())
            .Returns(true)   // User 1
            .Returns(false)  // End of first set
            .Returns(true)   // Order 1
            .Returns(false)  // End of second set
            .Returns(true)   // Product 1
            .Returns(false)  // End of third set
            .Returns(true)   // Inventory 1
            .Returns(false); // End of fourth set

        mockReader
            .SetupSequence(r => r.NextResult())
            .Returns(true)   // Move to second set
            .Returns(true)   // Move to third set
            .Returns(true)   // Move to fourth set
            .Returns(false); // No more sets

        mockCommand.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(mockReader.Object);
        mockProvider.Setup(p => p.GetParameterPrefix()).Returns("@");

        // Act
        var result = await AdoHelper.ExecuteQueryMultipleAsync<User, Order, Product, Inventory>(
            mockConnection.Object,
            "EXEC sp_GetComplex",
            mockProvider.Object,
            MapUser,
            MapOrder,
            MapProduct,
            MapInventory);

        // Assert
        Assert.True(result.Succeeded);
        var (returnedUsers, returnedOrders, returnedProducts, returnedInventories) = result.Data;
        Assert.NotEmpty(returnedUsers);
        Assert.NotEmpty(returnedOrders);
        Assert.NotEmpty(returnedProducts);
        Assert.NotEmpty(returnedInventories);
    }

    [Fact]
    public async Task ExecuteQueryMultipleAsync_FourSetsNullMapperSecond_ReturnsFail()
    {
        // Arrange
        var mockConnection = new Mock<IDbConnection>();
        var mockProvider = new Mock<IDbProvider>();

        mockConnection.Setup(c => c.State).Returns(ConnectionState.Open);

        // Act
        var result = await AdoHelper.ExecuteQueryMultipleAsync<User, Order, Product, Inventory>(
            mockConnection.Object,
            "EXEC sp_Test",
            mockProvider.Object,
            MapUser,
            null!,
            MapProduct,
            MapInventory);

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        Assert.Equal("ADO.MAPPER_NULL", result.Error.Code);
    }

    [Fact]
    public async Task ExecuteQueryMultipleAsync_FourSetsNullMapperFourth_ReturnsFail()
    {
        // Arrange
        var mockConnection = new Mock<IDbConnection>();
        var mockProvider = new Mock<IDbProvider>();

        mockConnection.Setup(c => c.State).Returns(ConnectionState.Open);

        // Act
        var result = await AdoHelper.ExecuteQueryMultipleAsync<User, Order, Product, Inventory>(
            mockConnection.Object,
            "EXEC sp_Test",
            mockProvider.Object,
            MapUser,
            MapOrder,
            MapProduct,
            null!);

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        Assert.Equal("ADO.MAPPER_NULL", result.Error.Code);
    }

    [Fact]
    public async Task ExecuteQueryMultipleAsync_FourSetsException_ReturnsFail()
    {
        // Arrange
        var mockConnection = new Mock<IDbConnection>();
        var mockCommand = new Mock<IDbCommand>();
        var mockProvider = new Mock<IDbProvider>();

        mockConnection.Setup(c => c.State).Returns(ConnectionState.Open);
        mockConnection.Setup(c => c.CreateCommand()).Returns(mockCommand.Object);
        mockCommand.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>()))
            .Throws(new TimeoutException("Query timeout"));
        mockProvider.Setup(p => p.GetParameterPrefix()).Returns("@");

        // Act
        var result = await AdoHelper.ExecuteQueryMultipleAsync<User, Order, Product, Inventory>(
            mockConnection.Object,
            "EXEC sp_Test",
            mockProvider.Object,
            MapUser,
            MapOrder,
            MapProduct,
            MapInventory);

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        Assert.Equal("ADO.QUERY_MULTIPLE_FAILED", result.Error.Code);
    }

    [Fact]
    public async Task ExecuteQueryMultipleAsync_EmptyResultSets_ReturnsEmptyLists()
    {
        // Arrange
        var mockReader = new Mock<IDataReader>();
        var mockCommand = new Mock<IDbCommand>();
        var mockConnection = new Mock<IDbConnection>();
        var mockProvider = new Mock<IDbProvider>();

        mockConnection.Setup(c => c.State).Returns(ConnectionState.Open);
        mockConnection.Setup(c => c.CreateCommand()).Returns(mockCommand.Object);

        mockReader
            .SetupSequence(r => r.Read())
            .Returns(false) // No users
            .Returns(false) // No orders
            .Returns(false);

        mockReader
            .SetupSequence(r => r.NextResult())
            .Returns(true)  // Move to second set
            .Returns(false); // No more sets

        mockCommand.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(mockReader.Object);
        mockProvider.Setup(p => p.GetParameterPrefix()).Returns("@");

        // Act
        var result = await AdoHelper.ExecuteQueryMultipleAsync<User, Order>(
            mockConnection.Object,
            "EXEC sp_Empty",
            mockProvider.Object,
            MapUser,
            MapOrder);

        // Assert
        Assert.True(result.Succeeded);
        var (users, orders) = result.Data;
        Assert.Empty(users);
        Assert.Empty(orders);
    }

    [Fact]
    public async Task ExecuteQueryMultipleAsync_NoSecondResultSet_ReturnsOnlyFirstSet()
    {
        // Arrange
        var mockReader = new Mock<IDataReader>();
        var mockCommand = new Mock<IDbCommand>();
        var mockConnection = new Mock<IDbConnection>();
        var mockProvider = new Mock<IDbProvider>();

        mockConnection.Setup(c => c.State).Returns(ConnectionState.Open);
        mockConnection.Setup(c => c.CreateCommand()).Returns(mockCommand.Object);

        mockReader
            .SetupSequence(r => r.Read())
            .Returns(true)   // User 1
            .Returns(false); // End of first set

        mockReader
            .Setup(r => r.NextResult())
            .Returns(false); // No second set

        mockCommand.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(mockReader.Object);
        mockProvider.Setup(p => p.GetParameterPrefix()).Returns("@");

        // Act
        var result = await AdoHelper.ExecuteQueryMultipleAsync<User, Order>(
            mockConnection.Object,
            "EXEC sp_Partial",
            mockProvider.Object,
            MapUser,
            MapOrder);

        // Assert
        Assert.True(result.Succeeded);
        var (users, orders) = result.Data;
        Assert.NotEmpty(users);
        Assert.Empty(orders);
    }

    [Fact]
    public async Task ExecuteQueryMultipleAsync_CommandBehaviorSequentialAccess_IsUsed()
    {
        // Arrange
        var mockReader = new Mock<IDataReader>();
        var mockCommand = new Mock<IDbCommand>();
        var mockConnection = new Mock<IDbConnection>();
        var mockProvider = new Mock<IDbProvider>();

        mockConnection.Setup(c => c.State).Returns(ConnectionState.Open);
        mockConnection.Setup(c => c.CreateCommand()).Returns(mockCommand.Object);

        mockReader
            .SetupSequence(r => r.Read())
            .Returns(false)
            .Returns(false);

        mockReader
            .Setup(r => r.NextResult())
            .Returns(false);

        mockCommand.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(mockReader.Object);
        mockProvider.Setup(p => p.GetParameterPrefix()).Returns("@");

        // Act
        await AdoHelper.ExecuteQueryMultipleAsync<User, Order>(
            mockConnection.Object,
            "EXEC sp_Test",
            mockProvider.Object,
            MapUser,
            MapOrder);

        // Assert
        mockCommand.Verify(
            c => c.ExecuteReader(CommandBehavior.SequentialAccess),
            Times.Once);
    }
}
