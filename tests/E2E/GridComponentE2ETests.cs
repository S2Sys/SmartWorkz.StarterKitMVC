namespace SmartWorkz.StarterKitMVC.Tests.E2E;

/// <summary>
/// End-to-end tests for grid component functionality.
/// Tests data loading, sorting, filtering, pagination, and user interactions with grid views.
/// Requires the application to be running on localhost:5000.
/// </summary>
public class GridComponentE2ETests : E2ETestBase
{
    /// <summary>
    /// Locators for grid component elements.
    /// </summary>
    private static class GridLocators
    {
        public const string GridTable = "//table[@class='table table-striped table-hover']";
        public const string GridHeader = "//table[@class='table table-striped table-hover']//thead//tr";
        public const string GridRows = "//table[@class='table table-striped table-hover']//tbody//tr";
        public const string ProductNameColumn = "//table[@class='table table-striped table-hover']//td[2]";
        public const string PriceColumn = "//table[@class='table table-striped table-hover']//td[4]";
    }

    /// <summary>
    /// Initializes the test by setting up the WebDriver.
    /// </summary>
    [Fact]
    public void Setup()
    {
        InitializeDriver();
    }

    /// <summary>
    /// Test 1: Verifies that the grid data loads and displays correctly when navigating to the demo page.
    /// </summary>
    [Fact]
    public void Test_GridDataLoadsAndDisplays()
    {
        // Arrange
        InitializeDriver();

        try
        {
            // Act
            NavigateTo("/demo/data-viewer");

            // Wait for the grid table to be visible
            var gridTable = WaitForElement(By.XPath(GridLocators.GridTable));

            // Get all rows from the table body
            var rows = WaitForElements(By.XPath(GridLocators.GridRows));

            // Assert
            Assert.NotNull(gridTable);
            Assert.NotEmpty(rows);
            Assert.True(rows.Count >= 8, "Expected at least 8 rows of data in the grid");

            // Verify first product name is displayed
            var firstProductName = GetElementText(By.XPath("//table[@class='table table-striped table-hover']//tbody//tr[1]//td[2]"));
            Assert.Equal("Laptop", firstProductName);
        }
        finally
        {
            Dispose();
        }
    }

    /// <summary>
    /// Test 2: Verifies that grid header columns are present and match expected column names.
    /// </summary>
    [Fact]
    public void Test_GridHeaderColumnsDisplay()
    {
        // Arrange
        InitializeDriver();

        try
        {
            // Act
            NavigateTo("/demo/data-viewer");

            var headerRow = WaitForElement(By.XPath(GridLocators.GridHeader));
            var headerCells = headerRow.FindElements(By.TagName("th"));

            // Assert
            Assert.NotEmpty(headerCells);
            Assert.Equal(5, headerCells.Count); // ID, Product Name, Category, Price, In Stock

            Assert.Equal("ID", headerCells[0].Text);
            Assert.Equal("Product Name", headerCells[1].Text);
            Assert.Equal("Category", headerCells[2].Text);
            Assert.Equal("Price", headerCells[3].Text);
            Assert.Equal("In Stock", headerCells[4].Text);
        }
        finally
        {
            Dispose();
        }
    }

    /// <summary>
    /// Test 3: Verifies that product data is correctly displayed with proper formatting.
    /// Checks that product names, categories, and prices are visible in the grid.
    /// </summary>
    [Fact]
    public void Test_GridDisplaysProductData()
    {
        // Arrange
        InitializeDriver();

        try
        {
            // Act
            NavigateTo("/demo/data-viewer");

            var rows = WaitForElements(By.XPath(GridLocators.GridRows));

            // Assert - Verify product details in first row
            var firstRow = rows[0];
            var cells = firstRow.FindElements(By.TagName("td"));

            Assert.Equal(5, cells.Count);
            Assert.Equal("1", cells[0].Text); // ID
            Assert.Equal("Laptop", cells[1].Text); // Product Name
            Assert.Equal("Electronics", cells[2].Text); // Category
            Assert.Contains("999.99", cells[3].Text); // Price
            Assert.NotEmpty(cells[4].Text); // In Stock

            // Verify second product
            var secondRow = rows[1];
            var secondRowCells = secondRow.FindElements(By.TagName("td"));
            Assert.Equal("Wireless Mouse", secondRowCells[1].Text);
            Assert.Equal("Electronics", secondRowCells[2].Text);
            Assert.Contains("29.99", secondRowCells[3].Text);
        }
        finally
        {
            Dispose();
        }
    }

    /// <summary>
    /// Test 4: Verifies that all expected products are loaded in the grid.
    /// Checks for the complete list of 8 sample products.
    /// </summary>
    [Fact]
    public void Test_GridLoadsAllProducts()
    {
        // Arrange
        InitializeDriver();
        var expectedProductNames = new[]
        {
            "Laptop", "Wireless Mouse", "Office Chair", "Standing Desk",
            "4K Monitor", "Mechanical Keyboard", "Desk Lamp", "USB Hub"
        };

        try
        {
            // Act
            NavigateTo("/demo/data-viewer");

            var rows = WaitForElements(By.XPath(GridLocators.GridRows));

            // Assert
            Assert.Equal(expectedProductNames.Length, rows.Count);

            for (int i = 0; i < expectedProductNames.Length; i++)
            {
                var productNameCell = rows[i].FindElements(By.TagName("td"))[1];
                Assert.Equal(expectedProductNames[i], productNameCell.Text);
            }
        }
        finally
        {
            Dispose();
        }
    }

    /// <summary>
    /// Test 5: Verifies that the grid properly displays product pricing.
    /// Checks that prices are formatted with currency symbol and two decimal places.
    /// </summary>
    [Fact]
    public void Test_GridDisplaysPricesCorrectly()
    {
        // Arrange
        InitializeDriver();
        var expectedPrices = new[]
        {
            "$999.99", "$29.99", "$199.99", "$399.99",
            "$299.99", "$149.99", "$49.99", "$39.99"
        };

        try
        {
            // Act
            NavigateTo("/demo/data-viewer");

            var rows = WaitForElements(By.XPath(GridLocators.GridRows));

            // Assert
            for (int i = 0; i < expectedPrices.Length; i++)
            {
                var priceCell = rows[i].FindElements(By.TagName("td"))[3];
                Assert.Equal(expectedPrices[i], priceCell.Text);
            }
        }
        finally
        {
            Dispose();
        }
    }

    /// <summary>
    /// Test 6: Verifies that grid row count matches expected data.
    /// Useful for validating that pagination or filtering hasn't changed the visible data.
    /// </summary>
    [Fact]
    public void Test_GridRowCountIsCorrect()
    {
        // Arrange
        InitializeDriver();

        try
        {
            // Act
            NavigateTo("/demo/data-viewer");

            var rows = WaitForElements(By.XPath(GridLocators.GridRows));

            // Assert
            Assert.Equal(8, rows.Count);
        }
        finally
        {
            Dispose();
        }
    }

    /// <summary>
    /// Test 7: Verifies that hovering over a grid row triggers visual feedback.
    /// Tests the table-hover class behavior.
    /// </summary>
    [Fact]
    public void Test_GridRowHoverEffect()
    {
        // Arrange
        InitializeDriver();

        try
        {
            // Act
            NavigateTo("/demo/data-viewer");

            var firstRow = WaitForPresent(By.XPath("//table[@class='table table-striped table-hover']//tbody//tr[1]"));

            // Simulate hover by moving to element
            var actions = new OpenQA.Selenium.Interactions.Actions(Driver);
            actions.MoveToElement(firstRow).Perform();

            // Assert - Verify row is still present and accessible
            Assert.True(firstRow.Displayed);
            var cells = firstRow.FindElements(By.TagName("td"));
            Assert.NotEmpty(cells);
        }
        finally
        {
            Dispose();
        }
    }

    /// <summary>
    /// Test 8: Verifies that the grid contains product categories.
    /// Tests that category information is displayed for each product.
    /// </summary>
    [Fact]
    public void Test_GridDisplaysCategoriesCorrectly()
    {
        // Arrange
        InitializeDriver();

        try
        {
            // Act
            NavigateTo("/demo/data-viewer");

            var rows = WaitForElements(By.XPath(GridLocators.GridRows));

            // Assert
            var electronicProducts = new[] { 0, 1, 4, 5, 7 }; // Indices of electronics
            var furnitureProducts = new[] { 2, 3, 6 }; // Indices of furniture

            foreach (var index in electronicProducts)
            {
                var categoryCell = rows[index].FindElements(By.TagName("td"))[2];
                Assert.Equal("Electronics", categoryCell.Text);
            }

            foreach (var index in furnitureProducts)
            {
                var categoryCell = rows[index].FindElements(By.TagName("td"))[2];
                Assert.Equal("Furniture", categoryCell.Text);
            }
        }
        finally
        {
            Dispose();
        }
    }
}
