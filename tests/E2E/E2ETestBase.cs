namespace SmartWorkz.StarterKitMVC.Tests.E2E;

/// <summary>
/// Base class for end-to-end (E2E) tests using Selenium WebDriver.
/// Provides common setup, teardown, and utility methods for browser automation testing.
/// </summary>
public abstract class E2ETestBase : IDisposable
{
    /// <summary>
    /// The Selenium WebDriver instance used for browser automation.
    /// </summary>
    protected IWebDriver Driver { get; private set; } = null!;

    /// <summary>
    /// The base URL of the application being tested.
    /// Defaults to localhost:5000 for local testing.
    /// </summary>
    protected virtual string BaseUrl { get; } = "http://localhost:5000";

    /// <summary>
    /// Default timeout for WebDriver waits (in seconds).
    /// Used for waiting for elements to appear, be clickable, etc.
    /// </summary>
    protected virtual int DefaultWaitTimeoutSeconds { get; } = 10;

    /// <summary>
    /// Initializes the WebDriver and sets up the browser for testing.
    /// Called before each test method execution.
    /// </summary>
    public virtual void InitializeDriver()
    {
        var options = new ChromeOptions();
        options.AddArgument("--start-maximized");
        options.AddArgument("--disable-notifications");

        // Uncomment for headless mode in CI/CD environments
        // options.AddArgument("--headless");
        // options.AddArgument("--disable-gpu");

        Driver = new ChromeDriver(options);
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(DefaultWaitTimeoutSeconds);
    }

    /// <summary>
    /// Creates a WebDriverWait instance with the default timeout.
    /// Useful for explicit waits on specific elements or conditions.
    /// </summary>
    /// <returns>A configured WebDriverWait instance.</returns>
    protected WebDriverWait GetWait()
    {
        return new WebDriverWait(Driver, TimeSpan.FromSeconds(DefaultWaitTimeoutSeconds));
    }

    /// <summary>
    /// Navigates to the specified URL relative to BaseUrl.
    /// </summary>
    /// <param name="relativeUrl">The relative URL path (e.g., "/demo/data-viewer").</param>
    protected void NavigateTo(string relativeUrl)
    {
        Driver.Navigate().GoToUrl($"{BaseUrl}{relativeUrl}");
    }

    /// <summary>
    /// Waits for an element to be visible and returns it.
    /// </summary>
    /// <param name="locator">The By locator for finding the element.</param>
    /// <returns>The found IWebElement.</returns>
    protected IWebElement WaitForElement(By locator)
    {
        return GetWait().Until(ExpectedConditions.ElementIsVisible(locator));
    }

    /// <summary>
    /// Waits for an element to be clickable and returns it.
    /// </summary>
    /// <param name="locator">The By locator for finding the element.</param>
    /// <returns>The found IWebElement.</returns>
    protected IWebElement WaitForClickable(By locator)
    {
        return GetWait().Until(ExpectedConditions.ElementToBeClickable(locator));
    }

    /// <summary>
    /// Waits for an element to be present in the DOM.
    /// </summary>
    /// <param name="locator">The By locator for finding the element.</param>
    /// <returns>The found IWebElement.</returns>
    protected IWebElement WaitForPresent(By locator)
    {
        return GetWait().Until(ExpectedConditions.PresenceOfElementLocated(locator));
    }

    /// <summary>
    /// Waits for multiple elements to be visible.
    /// </summary>
    /// <param name="locator">The By locator for finding the elements.</param>
    /// <returns>A list of visible IWebElements.</returns>
    protected IList<IWebElement> WaitForElements(By locator)
    {
        return GetWait().Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(locator));
    }

    /// <summary>
    /// Clicks an element after waiting for it to be clickable.
    /// </summary>
    /// <param name="locator">The By locator for the element to click.</param>
    protected void ClickElement(By locator)
    {
        var element = WaitForClickable(locator);
        element.Click();
    }

    /// <summary>
    /// Fills a text input field with the specified text.
    /// Clears the field before entering text.
    /// </summary>
    /// <param name="locator">The By locator for the input element.</param>
    /// <param name="text">The text to enter.</param>
    protected void FillTextInput(By locator, string text)
    {
        var element = WaitForElement(locator);
        element.Clear();
        element.SendKeys(text);
    }

    /// <summary>
    /// Gets the text content of an element after waiting for it to be visible.
    /// </summary>
    /// <param name="locator">The By locator for the element.</param>
    /// <returns>The text content of the element.</returns>
    protected string GetElementText(By locator)
    {
        var element = WaitForElement(locator);
        return element.Text;
    }

    /// <summary>
    /// Checks if an element is currently visible on the page.
    /// </summary>
    /// <param name="locator">The By locator for the element.</param>
    /// <returns>True if the element is visible; false otherwise.</returns>
    protected bool IsElementVisible(By locator)
    {
        try
        {
            var element = Driver.FindElement(locator);
            return element.Displayed;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }

    /// <summary>
    /// Waits for an element to be invisible or removed from the DOM.
    /// </summary>
    /// <param name="locator">The By locator for the element.</param>
    protected void WaitForInvisible(By locator)
    {
        GetWait().Until(ExpectedConditions.InvisibilityOfElementLocated(locator));
    }

    /// <summary>
    /// Scrolls the element into view and clicks it.
    /// Useful for elements that might be off-screen.
    /// </summary>
    /// <param name="locator">The By locator for the element to click.</param>
    protected void ScrollAndClick(By locator)
    {
        var element = WaitForPresent(locator);
        ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView(true);", element);
        element.Click();
    }

    /// <summary>
    /// Executes arbitrary JavaScript on the current page.
    /// </summary>
    /// <param name="script">The JavaScript code to execute.</param>
    /// <param name="args">Optional arguments to pass to the script.</param>
    /// <returns>The result of the script execution.</returns>
    protected object ExecuteScript(string script, params object[] args)
    {
        return ((IJavaScriptExecutor)Driver).ExecuteScript(script, args);
    }

    /// <summary>
    /// Gets the current page URL.
    /// </summary>
    /// <returns>The current page URL.</returns>
    protected string GetCurrentUrl()
    {
        return Driver.Url;
    }

    /// <summary>
    /// Waits for the page URL to change to the expected URL.
    /// </summary>
    /// <param name="expectedUrl">The expected URL (can be partial).</param>
    protected void WaitForUrlChange(string expectedUrl)
    {
        GetWait().Until(driver => driver.Url.Contains(expectedUrl));
    }

    /// <summary>
    /// Cleans up resources by closing the WebDriver.
    /// Called after each test method execution.
    /// </summary>
    public virtual void Dispose()
    {
        Driver?.Quit();
        Driver?.Dispose();
    }
}
