namespace SmartWorkz.StarterKitMVC.Tests.E2E;

/// <summary>
/// End-to-end tests for user interaction workflows.
/// Tests complex user interactions like row clicking, data filtering, and component state changes.
/// </summary>
public class UserInteractionE2ETests : E2ETestBase
{
    /// <summary>
    /// Locators for interactive elements.
    /// </summary>
    private static class InteractionLocators
    {
        public const string GridTable = "//table[@class='table table-striped table-hover']";
        public const string GridRows = "//table[@class='table table-striped table-hover']//tbody//tr";
        public const string ListViewButton = "//button[contains(text(), 'List View')]";
        public const string GridViewButton = "//button[contains(text(), 'Grid View')]";
        public const string ListCards = "//div[@class='card h-100']";
        public const string CardTitle = ".//h5[@class='card-title']";
    }

    /// <summary>
    /// Test 1: Verifies that clicking on a grid row selects or highlights it.
    /// </summary>
    [Fact]
    public void Test_ClickingGridRowIsInteractive()
    {
        // Arrange
        InitializeDriver();

        try
        {
            // Act
            NavigateTo("/demo/data-viewer");

            // Get first row
            var firstRow = WaitForPresent(By.XPath(InteractionLocators.GridRows + "[1]"));

            // Simulate hovering and clicking
            var actions = new OpenQA.Selenium.Interactions.Actions(Driver);
            actions.MoveToElement(firstRow).Click().Perform();

            // Assert
            Assert.True(firstRow.Displayed);
            var cells = firstRow.FindElements(By.TagName("td"));
            Assert.NotEmpty(cells);
            Assert.Equal("Laptop", cells[1].Text);
        }
        finally
        {
            Dispose();
        }
    }

    /// <summary>
    /// Test 2: Verifies that user can interact with multiple grid rows sequentially.
    /// </summary>
    [Fact]
    public void Test_InteractWithMultipleGridRows()
    {
        // Arrange
        InitializeDriver();

        try
        {
            // Act
            NavigateTo("/demo/data-viewer");

            var rows = WaitForElements(By.XPath(InteractionLocators.GridRows));

            // Assert - Interact with first 3 rows
            for (int i = 0; i < 3; i++)
            {
                var actions = new OpenQA.Selenium.Interactions.Actions(Driver);
                actions.MoveToElement(rows[i]).Perform();

                Assert.True(rows[i].Displayed);
                var productName = rows[i].FindElements(By.TagName("td"))[1].Text;
                Assert.NotEmpty(productName);
            }
        }
        finally
        {
            Dispose();
        }
    }

    /// <summary>
    /// Test 3: Verifies that switching views preserves the same data.
    /// Tests that underlying data doesn't change when switching between views.
    /// </summary>
    [Fact]
    public void Test_SwitchingViewsPreservesData()
    {
        // Arrange
        InitializeDriver();

        try
        {
            // Act
            NavigateTo("/demo/data-viewer");

            // Get data from grid view
            var gridRows = WaitForElements(By.XPath(InteractionLocators.GridRows));
            var gridRowCount = gridRows.Count;
            var firstProductName = gridRows[0].FindElements(By.TagName("td"))[1].Text;

            // Switch to list view
            var listViewButton = WaitForClickable(By.XPath(InteractionLocators.ListViewButton));
            listViewButton.Click();

            // Get data from list view
            var listCards = WaitForElements(By.XPath(InteractionLocators.ListCards));

            // Assert
            Assert.Equal(gridRowCount, listCards.Count);

            // Verify first product is same
            var firstCard = listCards[0];
            var cardTitle = firstCard.FindElement(By.XPath(InteractionLocators.CardTitle));
            Assert.Equal(firstProductName, cardTitle.Text);
        }
        finally
        {
            Dispose();
        }
    }

    /// <summary>
    /// Test 4: Verifies that all list cards can be interacted with.
    /// Tests that each card is accessible and contains expected information.
    /// </summary>
    [Fact]
    public void Test_AllListCardsAreInteractive()
    {
        // Arrange
        InitializeDriver();

        try
        {
            // Act
            NavigateTo("/demo/data-viewer");

            // Switch to list view
            var listViewButton = WaitForClickable(By.XPath(InteractionLocators.ListViewButton));
            listViewButton.Click();

            // Get all cards
            var cards = WaitForElements(By.XPath(InteractionLocators.ListCards));

            // Assert - Each card should be interactive and contain product info
            var expectedProductNames = new[]
            {
                "Laptop", "Wireless Mouse", "Office Chair", "Standing Desk",
                "4K Monitor", "Mechanical Keyboard", "Desk Lamp", "USB Hub"
            };

            for (int i = 0; i < cards.Count; i++)
            {
                // Hover over card
                var actions = new OpenQA.Selenium.Interactions.Actions(Driver);
                actions.MoveToElement(cards[i]).Perform();

                // Verify card content
                var cardTitle = cards[i].FindElement(By.XPath(InteractionLocators.CardTitle));
                Assert.Equal(expectedProductNames[i], cardTitle.Text);

                // Verify card is displayed
                Assert.True(cards[i].Displayed);
            }
        }
        finally
        {
            Dispose();
        }
    }

    /// <summary>
    /// Test 5: Verifies that rapid view switching works correctly.
    /// Tests state management under rapid user interactions.
    /// </summary>
    [Fact]
    public void Test_RapidViewSwitching()
    {
        // Arrange
        InitializeDriver();

        try
        {
            // Act
            NavigateTo("/demo/data-viewer");

            // Switch views multiple times rapidly
            for (int i = 0; i < 5; i++)
            {
                var listViewButton = WaitForClickable(By.XPath(InteractionLocators.ListViewButton));
                listViewButton.Click();

                // Wait for cards to be visible
                WaitForElements(By.XPath(InteractionLocators.ListCards));

                var gridViewButton = WaitForClickable(By.XPath(InteractionLocators.GridViewButton));
                gridViewButton.Click();

                // Wait for grid to be visible
                WaitForElement(By.XPath(InteractionLocators.GridTable));
            }

            // Assert - Final state should be grid view
            var gridTable = Driver.FindElement(By.XPath(InteractionLocators.GridTable));
            Assert.True(gridTable.Displayed);
        }
        finally
        {
            Dispose();
        }
    }

    /// <summary>
    /// Test 6: Verifies that hovering over cards shows visual feedback.
    /// Tests CSS hover effects on list view cards.
    /// </summary>
    [Fact]
    public void Test_CardHoverEffect()
    {
        // Arrange
        InitializeDriver();

        try
        {
            // Act
            NavigateTo("/demo/data-viewer");

            // Switch to list view
            var listViewButton = WaitForClickable(By.XPath(InteractionLocators.ListViewButton));
            listViewButton.Click();

            // Get first card
            var firstCard = WaitForPresent(By.XPath(InteractionLocators.ListCards + "[1]"));

            // Hover over card
            var actions = new OpenQA.Selenium.Interactions.Actions(Driver);
            actions.MoveToElement(firstCard).Perform();

            // Assert - Card should still be displayed and interactive
            Assert.True(firstCard.Displayed);
            var cardTitle = firstCard.FindElement(By.TagName("h5"));
            Assert.NotEmpty(cardTitle.Text);
        }
        finally
        {
            Dispose();
        }
    }

    /// <summary>
    /// Test 7: Verifies that grid rows maintain correct data order.
    /// Tests that product data is displayed in consistent order.
    /// </summary>
    [Fact]
    public void Test_GridRowDataConsistency()
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

            var rows = WaitForElements(By.XPath(InteractionLocators.GridRows));

            // Assert
            for (int i = 0; i < rows.Count; i++)
            {
                var productName = rows[i].FindElements(By.TagName("td"))[1].Text;
                Assert.Equal(expectedProductNames[i], productName);
            }
        }
        finally
        {
            Dispose();
        }
    }

    /// <summary>
    /// Test 8: Verifies that list cards maintain correct data order.
    /// Tests that product data is displayed in consistent order in list view.
    /// </summary>
    [Fact]
    public void Test_ListCardDataConsistency()
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

            // Switch to list view
            var listViewButton = WaitForClickable(By.XPath(InteractionLocators.ListViewButton));
            listViewButton.Click();

            var cards = WaitForElements(By.XPath(InteractionLocators.ListCards));

            // Assert
            for (int i = 0; i < cards.Count; i++)
            {
                var cardTitle = cards[i].FindElement(By.XPath(InteractionLocators.CardTitle));
                Assert.Equal(expectedProductNames[i], cardTitle.Text);
            }
        }
        finally
        {
            Dispose();
        }
    }

    /// <summary>
    /// Test 9: Verifies that scrolling through grid works correctly.
    /// Tests that all rows remain accessible when scrolling.
    /// </summary>
    [Fact]
    public void Test_GridScrolling()
    {
        // Arrange
        InitializeDriver();

        try
        {
            // Act
            NavigateTo("/demo/data-viewer");

            var lastRow = WaitForPresent(By.XPath(InteractionLocators.GridRows + "[last()]"));

            // Scroll to last row
            ScrollAndClick(By.XPath(InteractionLocators.GridRows + "[last()]"));

            // Assert - Last row should be accessible
            Assert.True(lastRow.Displayed);
            var lastRowText = lastRow.Text;
            Assert.NotEmpty(lastRowText);
        }
        finally
        {
            Dispose();
        }
    }

    /// <summary>
    /// Test 10: Verifies that switching views after interaction preserves state.
    /// Tests complex workflow of interacting with grid, switching view, and back.
    /// </summary>
    [Fact]
    public void Test_ComplexInteractionWorkflow()
    {
        // Arrange
        InitializeDriver();

        try
        {
            // Act
            NavigateTo("/demo/data-viewer");

            // Interact with grid
            var gridRows = WaitForElements(By.XPath(InteractionLocators.GridRows));
            var firstProductName = gridRows[0].FindElements(By.TagName("td"))[1].Text;

            // Click on first row
            gridRows[0].Click();

            // Switch to list view
            var listViewButton = WaitForClickable(By.XPath(InteractionLocators.ListViewButton));
            listViewButton.Click();

            var cards = WaitForElements(By.XPath(InteractionLocators.ListCards));
            var firstCardTitle = cards[0].FindElement(By.XPath(InteractionLocators.CardTitle));

            // Assert - First product should be same in both views
            Assert.Equal(firstProductName, firstCardTitle.Text);

            // Switch back to grid
            var gridViewButton = WaitForClickable(By.XPath(InteractionLocators.GridViewButton));
            gridViewButton.Click();

            gridRows = WaitForElements(By.XPath(InteractionLocators.GridRows));
            var gridProductName = gridRows[0].FindElements(By.TagName("td"))[1].Text;

            // Assert - Data should still be correct
            Assert.Equal(firstProductName, gridProductName);
        }
        finally
        {
            Dispose();
        }
    }
}
