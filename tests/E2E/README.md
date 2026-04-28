# E2E Tests with Selenium

This directory contains comprehensive end-to-end (E2E) tests using Selenium WebDriver for browser automation testing of the SmartWorkz.StarterKitMVC application.

## Overview

The E2E test suite provides automated testing of user interactions, component behavior, and complete workflows through a real browser (Chrome).

### Test Structure

- **E2ETestBase.cs** - Base class providing WebDriver setup and common utility methods
- **GridComponentE2ETests.cs** - 8 tests for grid component functionality
- **ModalComponentE2ETests.cs** - 10 tests for modal component interactions
- **NavigationAndViewE2ETests.cs** - 11 tests for page navigation and view switching
- **UserInteractionE2ETests.cs** - 10 tests for complex user workflows

## Prerequisites

1. **Application Running**: The application must be running on `localhost:5000`
2. **Chrome Browser**: Chrome browser must be installed (tests use ChromeDriver)
3. **.NET 9.0**: Project targets .NET 9.0
4. **Dependencies**: Selenium WebDriver, xUnit, and related packages (see .csproj)

## Setting Up

### 1. Start the Application

Before running tests, ensure the application is running:

```bash
cd src/SmartWorkz.StarterKitMVC.Public
dotnet run
```

The application should be accessible at `http://localhost:5000`

### 2. Install Dependencies

Dependencies are automatically managed via NuGet. Restore packages if needed:

```bash
dotnet restore tests/E2E/SmartWorkz.StarterKitMVC.Tests.E2E.csproj
```

## Running Tests

### Run All E2E Tests

```bash
cd tests/E2E
dotnet test
```

### Run Specific Test Class

```bash
# Grid component tests
dotnet test --filter "ClassName=GridComponentE2ETests"

# Modal component tests
dotnet test --filter "ClassName=ModalComponentE2ETests"

# Navigation tests
dotnet test --filter "ClassName=NavigationAndViewE2ETests"

# User interaction tests
dotnet test --filter "ClassName=UserInteractionE2ETests"
```

### Run Specific Test Method

```bash
dotnet test --filter "Name=Test_GridDataLoadsAndDisplays"
```

### Run with Verbose Output

```bash
dotnet test -v detailed
```

## Headless Mode (CI/CD)

For CI/CD environments, uncomment headless mode in `E2ETestBase.cs`:

```csharp
protected virtual void InitializeDriver()
{
    var options = new ChromeOptions();
    // ... other arguments ...
    
    // Uncomment for headless mode
    options.AddArgument("--headless");
    options.AddArgument("--disable-gpu");
    
    Driver = new ChromeDriver(options);
    // ...
}
```

## Test Coverage

### GridComponentE2ETests (8 tests)

1. **Test_GridDataLoadsAndDisplays** - Verifies grid data loads and displays
2. **Test_GridHeaderColumnsDisplay** - Verifies header columns are correct
3. **Test_GridDisplaysProductData** - Verifies product data formatting
4. **Test_GridLoadsAllProducts** - Verifies all 8 sample products load
5. **Test_GridDisplaysPricesCorrectly** - Verifies price formatting
6. **Test_GridRowCountIsCorrect** - Verifies row count matches data
7. **Test_GridRowHoverEffect** - Verifies hover effects work
8. **Test_GridDisplaysCategoriesCorrectly** - Verifies category display

### ModalComponentE2ETests (10 tests)

1. **Test_ModalIsInitiallyHidden** - Verifies modal starts hidden
2. **Test_ModalCanBeOpened** - Verifies modal can be opened
3. **Test_ModalDisplaysTitleAndMessage** - Verifies modal content
4. **Test_ModalClosesWithCloseButton** - Verifies close button works
5. **Test_ModalClosesWithCancelButton** - Verifies cancel button works
6. **Test_ConfirmButtonIsClickable** - Verifies confirm button is accessible
7. **Test_ConfirmButtonTriggersCallback** - Verifies confirm callback fires
8. **Test_ModalHasCorrectStructure** - Verifies modal DOM structure
9. **Test_ModalFooterHasCorrectButtons** - Verifies footer buttons
10. **Test_ModalMultipleOpenCloseCycles** - Verifies state management

### NavigationAndViewE2ETests (11 tests)

1. **Test_DataViewerPageLoads** - Verifies page loads with buttons
2. **Test_PageUrlIsCorrect** - Verifies navigation URL
3. **Test_GridViewDisplaysTable** - Verifies grid view renders table
4. **Test_ListViewDisplaysCards** - Verifies list view renders cards
5. **Test_SwitchFromGridToListView** - Verifies grid→list switching
6. **Test_SwitchFromListToGridView** - Verifies list→grid switching
7. **Test_ViewButtonsHaveCorrectStyling** - Verifies active/inactive styling
8. **Test_ListViewCardsDisplayProductInfo** - Verifies card content
9. **Test_NavigateToHomePageWorks** - Verifies home page navigation
10. **Test_DataViewerPageHasCorrectHeading** - Verifies page heading
11. **Test_DataViewerPageHasDescription** - Verifies page description

### UserInteractionE2ETests (10 tests)

1. **Test_ClickingGridRowIsInteractive** - Verifies grid row interaction
2. **Test_InteractWithMultipleGridRows** - Verifies multiple row interaction
3. **Test_SwitchingViewsPreservesData** - Verifies data consistency
4. **Test_AllListCardsAreInteractive** - Verifies all cards accessible
5. **Test_RapidViewSwitching** - Verifies rapid state changes
6. **Test_CardHoverEffect** - Verifies card hover effects
7. **Test_GridRowDataConsistency** - Verifies grid data order
8. **Test_ListCardDataConsistency** - Verifies list data order
9. **Test_GridScrolling** - Verifies scrolling works
10. **Test_ComplexInteractionWorkflow** - Verifies complete workflows

## E2ETestBase Utility Methods

The base class provides these utility methods for all tests:

### Navigation
- `NavigateTo(relativeUrl)` - Navigate to page relative to base URL
- `GetCurrentUrl()` - Get current page URL
- `WaitForUrlChange(expectedUrl)` - Wait for URL to change

### Element Waits
- `WaitForElement(locator)` - Wait for element visibility
- `WaitForClickable(locator)` - Wait for element to be clickable
- `WaitForPresent(locator)` - Wait for element presence
- `WaitForElements(locator)` - Wait for multiple elements
- `WaitForInvisible(locator)` - Wait for element to disappear
- `GetWait()` - Create explicit WebDriverWait instance

### Element Interaction
- `ClickElement(locator)` - Click with automatic wait
- `FillTextInput(locator, text)` - Fill text field
- `GetElementText(locator)` - Get element text content
- `ScrollAndClick(locator)` - Scroll element into view and click

### Helpers
- `IsElementVisible(locator)` - Check element visibility
- `ExecuteScript(script, args)` - Execute JavaScript
- `InitializeDriver()` - Set up WebDriver
- `Dispose()` - Clean up resources

## Configuration

### Base URL
Default: `http://localhost:5000`

Override in test class:
```csharp
public class CustomE2ETests : E2ETestBase
{
    protected override string BaseUrl { get; } = "http://custom-url:5000";
}
```

### Wait Timeout
Default: 10 seconds

Override in test class:
```csharp
protected override int DefaultWaitTimeoutSeconds { get; } = 30;
```

## Test Data

Tests use sample products from the DataViewerDemo component:

1. Laptop (Electronics) - $999.99
2. Wireless Mouse (Electronics) - $29.99
3. Office Chair (Furniture) - $199.99
4. Standing Desk (Furniture) - $399.99
5. 4K Monitor (Electronics) - $299.99
6. Mechanical Keyboard (Electronics) - $149.99
7. Desk Lamp (Furniture) - $49.99
8. USB Hub (Electronics) - $39.99

## XPath Locators

Tests use XPath locators for element selection. Common patterns:

```csharp
// Table and grid elements
"//table[@class='table table-striped table-hover']"
"//table[@class='table table-striped table-hover']//tbody//tr"
"//table[@class='table table-striped table-hover']//tbody//tr[1]"

// Modal elements
"//div[@id='confirmModal']"
"//button[@id='confirmButton']"
"//div[@class='modal-backdrop fade show']"

// Buttons
"//button[contains(text(), 'Grid View')]"
"//button[@data-bs-dismiss='modal']"

// Cards
"//div[@class='card h-100']"
```

## Troubleshooting

### "Chrome driver not found"
Ensure ChromeDriver is installed or let NuGet handle it:
```bash
dotnet test
```

### "localhost:5000 not responding"
Ensure application is running:
```bash
cd src/SmartWorkz.StarterKitMVC.Public
dotnet run
```

### "Element not found" errors
- Increase wait timeout in test configuration
- Verify XPath locators match current HTML
- Check that page has fully loaded before interaction

### "Test timeout"
- Increase `DefaultWaitTimeoutSeconds` in base class
- Verify application performance
- Check for JavaScript errors in browser console

## Best Practices

1. **Wait for Elements**: Always use wait methods instead of finding elements directly
2. **Clean Up**: Tests use `try/finally` to ensure `Dispose()` is called
3. **Single Responsibility**: Each test focuses on one user action
4. **Readable XPaths**: Use descriptive attribute selectors when possible
5. **No Test Interdependencies**: Tests should run independently in any order
6. **Meaningful Assertions**: Each test has clear, focused assertions

## Running in CI/CD

Example GitHub Actions workflow:

```yaml
- name: Start application
  run: |
    cd src/SmartWorkz.StarterKitMVC.Public
    dotnet run &
    sleep 5

- name: Run E2E tests
  run: |
    cd tests/E2E
    dotnet test --filter "ClassName=GridComponentE2ETests" -v detailed

- name: Stop application
  run: pkill -f "dotnet run"
```

## Future Enhancements

- [ ] Add screenshot capture on test failure
- [ ] Add video recording of test execution
- [ ] Implement Page Object Model for better maintainability
- [ ] Add performance benchmarking
- [ ] Expand to test additional pages and features
- [ ] Add API mocking for independent testing
- [ ] Implement data-driven testing with multiple datasets

## References

- [Selenium WebDriver Documentation](https://www.selenium.dev/documentation/webdriver/)
- [xUnit Documentation](https://xunit.net/docs/getting-started/netcore)
- [Bootstrap Modal Documentation](https://getbootstrap.com/docs/5.3/components/modal/)
