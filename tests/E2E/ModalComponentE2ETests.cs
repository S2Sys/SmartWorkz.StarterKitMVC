namespace SmartWorkz.StarterKitMVC.Tests.E2E;

/// <summary>
/// End-to-end tests for modal component functionality.
/// Tests modal open, close, confirm actions, and user interactions with modal dialogs.
/// Uses the confirm modal component present in the application shared layout.
/// </summary>
public class ModalComponentE2ETests : E2ETestBase
{
    /// <summary>
    /// Locators for modal component elements.
    /// </summary>
    private static class ModalLocators
    {
        public const string ConfirmModal = "//div[@id='confirmModal']";
        public const string ConfirmModalBackdrop = "//div[@class='modal-backdrop fade show']";
        public const string ConfirmModalTitle = "//h5[@id='confirmModalLabel']";
        public const string ConfirmModalMessage = "//div[@id='confirmMessage']";
        public const string ConfirmButton = "//button[@id='confirmButton']";
        public const string CloseButton = "//button[@data-bs-dismiss='modal']";
        public const string CancelButton = "//button[@class='btn btn-secondary' and @data-bs-dismiss='modal']";
        public const string ModalDialog = "//div[@class='modal-dialog']";
    }

    /// <summary>
    /// Test 1: Verifies that the confirm modal component is present on the page but initially hidden.
    /// </summary>
    [Fact]
    public void Test_ModalIsInitiallyHidden()
    {
        // Arrange
        InitializeDriver();

        try
        {
            // Act
            NavigateTo("/");

            // Find the modal element
            var modal = Driver.FindElement(By.XPath(ModalLocators.ConfirmModal));

            // Assert
            Assert.NotNull(modal);
            // Modal should have aria-hidden="true" when initially hidden
            var ariaHidden = modal.GetAttribute("aria-hidden");
            Assert.Equal("true", ariaHidden);
        }
        finally
        {
            Dispose();
        }
    }

    /// <summary>
    /// Test 2: Verifies that the modal can be opened via JavaScript and displays correctly.
    /// </summary>
    [Fact]
    public void Test_ModalCanBeOpened()
    {
        // Arrange
        InitializeDriver();

        try
        {
            // Act
            NavigateTo("/");

            // Open the modal using Bootstrap JavaScript
            ExecuteScript("var modal = new bootstrap.Modal(document.getElementById('confirmModal')); modal.show();");

            // Wait for modal to be visible
            var modal = WaitForElement(By.XPath(ModalLocators.ConfirmModal));

            // Assert
            Assert.NotNull(modal);
            Assert.True(modal.Displayed);

            // Verify modal is marked as shown
            var ariaHidden = modal.GetAttribute("aria-hidden");
            Assert.Equal("false", ariaHidden);
        }
        finally
        {
            Dispose();
        }
    }

    /// <summary>
    /// Test 3: Verifies that the modal title and message display correctly.
    /// </summary>
    [Fact]
    public void Test_ModalDisplaysTitleAndMessage()
    {
        // Arrange
        InitializeDriver();

        try
        {
            // Act
            NavigateTo("/");

            // Set custom message using JavaScript
            ExecuteScript("document.getElementById('confirmMessage').textContent = 'Are you sure you want to delete this item?';");

            // Open the modal
            ExecuteScript("var modal = new bootstrap.Modal(document.getElementById('confirmModal')); modal.show();");

            // Get modal title and message
            var title = WaitForElement(By.XPath(ModalLocators.ConfirmModalTitle));
            var message = WaitForElement(By.XPath(ModalLocators.ConfirmModalMessage));

            // Assert
            Assert.Equal("Confirm Action", title.Text);
            Assert.Equal("Are you sure you want to delete this item?", message.Text);
        }
        finally
        {
            Dispose();
        }
    }

    /// <summary>
    /// Test 4: Verifies that the modal can be closed by clicking the close button.
    /// </summary>
    [Fact]
    public void Test_ModalClosesWithCloseButton()
    {
        // Arrange
        InitializeDriver();

        try
        {
            // Act
            NavigateTo("/");

            // Open the modal
            ExecuteScript("var modal = new bootstrap.Modal(document.getElementById('confirmModal')); modal.show();");

            // Wait for modal to be visible
            WaitForElement(By.XPath(ModalLocators.ConfirmModal));

            // Click the close button
            var closeButton = WaitForClickable(By.XPath("//button[@class='btn-close'][@data-bs-dismiss='modal']"));
            closeButton.Click();

            // Wait for modal to be hidden
            WaitForInvisible(By.XPath("//div[@class='modal-backdrop fade show']"));

            // Assert - Modal should be hidden
            var modal = Driver.FindElement(By.XPath(ModalLocators.ConfirmModal));
            var ariaHidden = modal.GetAttribute("aria-hidden");
            Assert.Equal("true", ariaHidden);
        }
        finally
        {
            Dispose();
        }
    }

    /// <summary>
    /// Test 5: Verifies that the modal can be closed by clicking the cancel button.
    /// </summary>
    [Fact]
    public void Test_ModalClosesWithCancelButton()
    {
        // Arrange
        InitializeDriver();

        try
        {
            // Act
            NavigateTo("/");

            // Open the modal
            ExecuteScript("var modal = new bootstrap.Modal(document.getElementById('confirmModal')); modal.show();");

            // Wait for modal to be visible
            WaitForElement(By.XPath(ModalLocators.ConfirmModal));

            // Click the cancel button
            var cancelButton = WaitForClickable(By.XPath("//button[@class='btn btn-secondary'][@data-bs-dismiss='modal']"));
            cancelButton.Click();

            // Wait for modal to be hidden
            WaitForInvisible(By.XPath("//div[@class='modal-backdrop fade show']"));

            // Assert
            var modal = Driver.FindElement(By.XPath(ModalLocators.ConfirmModal));
            var ariaHidden = modal.GetAttribute("aria-hidden");
            Assert.Equal("true", ariaHidden);
        }
        finally
        {
            Dispose();
        }
    }

    /// <summary>
    /// Test 6: Verifies that the confirm button is clickable when modal is open.
    /// </summary>
    [Fact]
    public void Test_ConfirmButtonIsClickable()
    {
        // Arrange
        InitializeDriver();

        try
        {
            // Act
            NavigateTo("/");

            // Open the modal
            ExecuteScript("var modal = new bootstrap.Modal(document.getElementById('confirmModal')); modal.show();");

            // Wait for confirm button to be clickable
            var confirmButton = WaitForClickable(By.XPath(ModalLocators.ConfirmButton));

            // Assert
            Assert.NotNull(confirmButton);
            Assert.True(confirmButton.Enabled);
            Assert.True(confirmButton.Displayed);
            Assert.Equal("Confirm", confirmButton.Text);
        }
        finally
        {
            Dispose();
        }
    }

    /// <summary>
    /// Test 7: Verifies that clicking confirm button fires callback and closes modal.
    /// Tests the complete confirm workflow.
    /// </summary>
    [Fact]
    public void Test_ConfirmButtonTriggersCallback()
    {
        // Arrange
        InitializeDriver();

        try
        {
            // Act
            NavigateTo("/");

            // Set up a JavaScript flag to track if confirm callback was triggered
            ExecuteScript("window.confirmClicked = false;");
            ExecuteScript("document.getElementById('confirmButton').addEventListener('click', function() { window.confirmClicked = true; });");

            // Open the modal
            ExecuteScript("var modal = new bootstrap.Modal(document.getElementById('confirmModal')); modal.show();");

            // Wait for modal to be visible
            WaitForElement(By.XPath(ModalLocators.ConfirmModal));

            // Click the confirm button
            var confirmButton = WaitForClickable(By.XPath(ModalLocators.ConfirmButton));
            confirmButton.Click();

            // Assert - Callback should have been triggered
            var callbackTriggered = (bool)ExecuteScript("return window.confirmClicked;");
            Assert.True(callbackTriggered);
        }
        finally
        {
            Dispose();
        }
    }

    /// <summary>
    /// Test 8: Verifies that modal has correct structure with header, body, and footer.
    /// </summary>
    [Fact]
    public void Test_ModalHasCorrectStructure()
    {
        // Arrange
        InitializeDriver();

        try
        {
            // Act
            NavigateTo("/");

            // Open the modal
            ExecuteScript("var modal = new bootstrap.Modal(document.getElementById('confirmModal')); modal.show();");

            // Wait for modal elements
            var modalHeader = WaitForElement(By.XPath("//div[@class='modal-header']"));
            var modalBody = WaitForElement(By.XPath("//div[@class='modal-body']"));
            var modalFooter = WaitForElement(By.XPath("//div[@class='modal-footer']"));

            // Assert
            Assert.NotNull(modalHeader);
            Assert.NotNull(modalBody);
            Assert.NotNull(modalFooter);
            Assert.True(modalHeader.Displayed);
            Assert.True(modalBody.Displayed);
            Assert.True(modalFooter.Displayed);
        }
        finally
        {
            Dispose();
        }
    }

    /// <summary>
    /// Test 9: Verifies that modal footer contains the expected buttons.
    /// </summary>
    [Fact]
    public void Test_ModalFooterHasCorrectButtons()
    {
        // Arrange
        InitializeDriver();

        try
        {
            // Act
            NavigateTo("/");

            // Open the modal
            ExecuteScript("var modal = new bootstrap.Modal(document.getElementById('confirmModal')); modal.show();");

            // Get modal footer buttons
            var modalFooter = WaitForElement(By.XPath("//div[@class='modal-footer']"));
            var buttons = modalFooter.FindElements(By.TagName("button"));

            // Assert
            Assert.Equal(2, buttons.Count); // Cancel and Confirm buttons
            Assert.Contains("Cancel", buttons[0].Text);
            Assert.Contains("Confirm", buttons[1].Text);
        }
        finally
        {
            Dispose();
        }
    }

    /// <summary>
    /// Test 10: Verifies that multiple modal open/close cycles work correctly.
    /// Tests modal state management across multiple interactions.
    /// </summary>
    [Fact]
    public void Test_ModalMultipleOpenCloseCycles()
    {
        // Arrange
        InitializeDriver();

        try
        {
            // Act & Assert
            NavigateTo("/");

            for (int cycle = 0; cycle < 3; cycle++)
            {
                // Open the modal
                ExecuteScript("var modal = new bootstrap.Modal(document.getElementById('confirmModal')); modal.show();");

                // Wait for modal to be visible
                WaitForElement(By.XPath(ModalLocators.ConfirmModal));

                // Verify it's open
                var modal = Driver.FindElement(By.XPath(ModalLocators.ConfirmModal));
                Assert.Equal("false", modal.GetAttribute("aria-hidden"));

                // Close the modal
                var closeButton = WaitForClickable(By.XPath("//button[@class='btn-close'][@data-bs-dismiss='modal']"));
                closeButton.Click();

                // Wait for modal to be hidden
                WaitForInvisible(By.XPath("//div[@class='modal-backdrop fade show']"));

                // Verify it's closed
                var ariaHidden = modal.GetAttribute("aria-hidden");
                Assert.Equal("true", ariaHidden);
            }
        }
        finally
        {
            Dispose();
        }
    }
}
