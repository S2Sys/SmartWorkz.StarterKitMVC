namespace SmartWorkz.StarterKitMVC.Tests.E2E;

/// <summary>
/// End-to-end tests for navigation and view switching functionality.
/// Tests component view switching (grid/list), navigation between pages, and URL handling.
/// </summary>
public class NavigationAndViewE2ETests : E2ETestBase
{
    /// <summary>
    /// Locators for navigation elements.
    /// </summary>
    private static class NavigationLocators
    {
        public const string GridViewButton = "//button[contains(text(), 'Grid View')]";
        public const string ListViewButton = "//button[contains(text(), 'List View')]";
        public const string GridTable = "//table[@class='table table-striped table-hover']";
        public const string ListCards = "//div[@class='card h-100']";
        public const string LoadingAlert = "//div[@class='alert alert-info']";
    }

    /// <summary>
    /// Test 1: Verifies that the data viewer page loads successfully.
    /// </summary>
    [Fact]
    public void Test_DataViewerPageLoads()
    {
        // Arrange
        InitializeDriver();

        try
        {
            // Act
            NavigateTo("/demo/data-viewer");

            // Assert - Page should load and contain view toggle buttons
            var gridViewButton = WaitForElement(By.XPath(NavigationLocators.GridViewButton));
            var listViewButton = WaitForElement(By.XPath(NavigationLocators.ListViewButton));

            Assert.NotNull(gridViewButton);
            Assert.NotNull(listViewButton);
        }
        finally
        {
            Dispose();
        }
    }

    /// <summary>
    /// Test 2: Verifies that the page URL is correct after navigation.
    /// </summary>
    [Fact]
    public void Test_PageUrlIsCorrect()
    {
        // Arrange
        InitializeDriver();

        try
        {
            // Act
            NavigateTo("/demo/data-viewer");

            // Assert
            var currentUrl = GetCurrentUrl();
            Assert.Contains("/demo/data-viewer", currentUrl);
        }
        finally
        {
            Dispose();
        }
    }

    /// <summary>
    /// Test 3: Verifies that grid view displays products in table format.
    /// </summary>
    [Fact]
    public void Test_GridViewDisplaysTable()
    {
        // Arrange
        InitializeDriver();

        try
        {
            // Act
            NavigateTo("/demo/data-viewer");

            // Ensure grid view is active
            var gridViewButton = WaitForClickable(By.XPath(NavigationLocators.GridViewButton));
            gridViewButton.Click();

            // Wait for grid table to be visible
            var gridTable = WaitForElement(By.XPath(NavigationLocators.GridTable));

            // Assert
            Assert.NotNull(gridTable);
            Assert.True(gridTable.Displayed);

            // Verify table structure
            var rows = gridTable.FindElements(By.TagName("tbody"));
            Assert.NotEmpty(rows);
        }
        finally
        {
            Dispose();
        }
    }

    /// <summary>
    /// Test 4: Verifies that list view displays products as cards.
    /// </summary>
    [Fact]
    public void Test_ListViewDisplaysCards()
    {
        // Arrange
        InitializeDriver();

        try
        {
            // Act
            NavigateTo("/demo/data-viewer");

            // Click list view button
            var listViewButton = WaitForClickable(By.XPath(NavigationLocators.ListViewButton));
            listViewButton.Click();

            // Wait for cards to be visible
            var cards = WaitForElements(By.XPath(NavigationLocators.ListCards));

            // Assert
            Assert.NotEmpty(cards);
            Assert.True(cards.Count >= 8);
        }
        finally
        {
            Dispose();
        }
    }

    /// <summary>
    /// Test 5: Verifies that switching from grid to list view works correctly.
    /// </summary>
    [Fact]
    public void Test_SwitchFromGridToListView()
    {
        // Arrange
        InitializeDriver();

        try
        {
            // Act
            NavigateTo("/demo/data-viewer");

            // Start with grid view
            var gridViewButton = WaitForClickable(By.XPath(NavigationLocators.GridViewButton));
            gridViewButton.Click();

            // Wait for grid to be visible
            WaitForElement(By.XPath(NavigationLocators.GridTable));

            // Switch to list view
            var listViewButton = WaitForClickable(By.XPath(NavigationLocators.ListViewButton));
            listViewButton.Click();

            // Wait for cards to be visible
            var cards = WaitForElements(By.XPath(NavigationLocators.ListCards));

            // Assert
            Assert.NotEmpty(cards);
            Assert.True(cards.Count >= 8);
        }
        finally
        {
            Dispose();
        }
    }

    /// <summary>
    /// Test 6: Verifies that switching from list to grid view works correctly.
    /// </summary>
    [Fact]
    public void Test_SwitchFromListToGridView()
    {
        // Arrange
        InitializeDriver();

        try
        {
            // Act
            NavigateTo("/demo/data-viewer");

            // Start with list view
            var listViewButton = WaitForClickable(By.XPath(NavigationLocators.ListViewButton));
            listViewButton.Click();

            // Wait for cards to be visible
            WaitForElements(By.XPath(NavigationLocators.ListCards));

            // Switch to grid view
            var gridViewButton = WaitForClickable(By.XPath(NavigationLocators.GridViewButton));
            gridViewButton.Click();

            // Wait for grid to be visible
            var gridTable = WaitForElement(By.XPath(NavigationLocators.GridTable));

            // Assert
            Assert.NotNull(gridTable);
            Assert.True(gridTable.Displayed);
        }
        finally
        {
            Dispose();
        }
    }

    /// <summary>
    /// Test 7: Verifies that view toggle buttons have correct active state styling.
    /// </summary>
    [Fact]
    public void Test_ViewButtonsHaveCorrectStyling()
    {
        // Arrange
        InitializeDriver();

        try
        {
            // Act
            NavigateTo("/demo/data-viewer");

            // Get buttons
            var gridViewButton = WaitForElement(By.XPath(NavigationLocators.GridViewButton));
            var listViewButton = WaitForElement(By.XPath(NavigationLocators.ListViewButton));

            // Assert - Grid view button should have btn-primary (active)
            var gridClass = gridViewButton.GetAttribute("class");
            Assert.Contains("btn-primary", gridClass);

            // List view button should have btn-outline-primary (inactive)
            var listClass = listViewButton.GetAttribute("class");
            Assert.Contains("btn-outline-primary", listClass);

            // Switch to list view
            listViewButton.Click();

            // Update references
            listViewButton = WaitForElement(By.XPath(NavigationLocators.ListViewButton));
            gridViewButton = WaitForElement(By.XPath(NavigationLocators.GridViewButton));

            // Assert - Styling should be reversed
            listClass = listViewButton.GetAttribute("class");
            Assert.Contains("btn-primary", listClass);

            gridClass = gridViewButton.GetAttribute("class");
            Assert.Contains("btn-outline-primary", gridClass);
        }
        finally
        {
            Dispose();
        }
    }

    /// <summary>
    /// Test 8: Verifies that list view cards contain product information.
    /// </summary>
    [Fact]
    public void Test_ListViewCardsDisplayProductInfo()
    {
        // Arrange
        InitializeDriver();

        try
        {
            // Act
            NavigateTo("/demo/data-viewer");

            // Switch to list view
            var listViewButton = WaitForClickable(By.XPath(NavigationLocators.ListViewButton));
            listViewButton.Click();

            // Wait for cards to be visible
            var cards = WaitForElements(By.XPath(NavigationLocators.ListCards));
            var firstCard = cards[0];

            // Assert - First card should contain Laptop info
            var cardTitle = firstCard.FindElement(By.TagName("h5"));
            Assert.Equal("Laptop", cardTitle.Text);

            // Should contain price
            var cardBody = firstCard.FindElement(By.ClassName("card-body"));
            var cardText = cardBody.Text;
            Assert.Contains("$999.99", cardText);
            Assert.Contains("Electronics", cardText);
        }
        finally
        {
            Dispose();
        }
    }

    /// <summary>
    /// Test 9: Verifies that navigating back to home page works.
    /// </summary>
    [Fact]
    public void Test_NavigateToHomePageWorks()
    {
        // Arrange
        InitializeDriver();

        try
        {
            // Act
            NavigateTo("/");

            // Assert
            var currentUrl = GetCurrentUrl();
            Assert.Contains("localhost:5000", currentUrl);
        }
        finally
        {
            Dispose();
        }
    }

    /// <summary>
    /// Test 10: Verifies that page title/heading displays correctly for data viewer.
    /// </summary>
    [Fact]
    public void Test_DataViewerPageHasCorrectHeading()
    {
        // Arrange
        InitializeDriver();

        try
        {
            // Act
            NavigateTo("/demo/data-viewer");

            // Wait for page heading
            var heading = WaitForElement(By.TagName("h1"));

            // Assert
            Assert.Equal("Multi-View Data Components Demo", heading.Text);
        }
        finally
        {
            Dispose();
        }
    }

    /// <summary>
    /// Test 11: Verifies that the page contains descriptive text about the demo.
    /// </summary>
    [Fact]
    public void Test_DataViewerPageHasDescription()
    {
        // Arrange
        InitializeDriver();

        try
        {
            // Act
            NavigateTo("/demo/data-viewer");

            // Find description text
            var description = Driver.FindElements(By.XPath("//p[@class='text-muted']"));

            // Assert
            Assert.NotEmpty(description);
            var descriptionText = description[0].Text;
            Assert.Contains("Grid and List views", descriptionText);
        }
        finally
        {
            Dispose();
        }
    }
}
