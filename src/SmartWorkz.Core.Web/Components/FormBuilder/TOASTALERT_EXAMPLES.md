# ToastAlertComponent - Practical Examples

## Example 1: Simple Notification Queue

```blazor
@page "/notifications"
@using SmartWorkz.Core.Web.Components.FormBuilder

<h1>Notification Manager</h1>

<div class="mb-3">
    <button class="btn btn-info" @onclick="() => AddNotification('info', 'Info', 'This is an informational message')">Info</button>
    <button class="btn btn-success" @onclick="() => AddNotification('success', 'Success', 'Operation completed!')">Success</button>
    <button class="btn btn-warning" @onclick="() => AddNotification('warning', 'Warning', 'Please check this important information')">Warning</button>
    <button class="btn btn-danger" @onclick="() => AddNotification('danger', 'Error', 'An error occurred!')">Error</button>
</div>

<div class="notification-container">
    @foreach (var notification in Notifications)
    {
        <ToastAlertComponent
            IsVisible="true"
            Type="@notification.Type"
            Title="@notification.Title"
            MessageText="@notification.Message"
            AutoDismissMs="@notification.AutoDismissMs"
            OnDismiss="() => RemoveNotification(notification.Id)" />
    }
</div>

@code {
    private List<Notification> Notifications = new();
    private int NotificationCounter = 0;

    private class Notification
    {
        public int Id { get; set; }
        public string Type { get; set; } = "info";
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public int? AutoDismissMs { get; set; } = 3000;
    }

    private void AddNotification(string type, string title, string message)
    {
        Notifications.Add(new Notification
        {
            Id = NotificationCounter++,
            Type = type,
            Title = title,
            Message = message,
            AutoDismissMs = type == "danger" ? null : 3000
        });
    }

    private void RemoveNotification(int id)
    {
        Notifications.RemoveAll(n => n.Id == id);
    }
}
```

## Example 2: Form Submission Feedback

```blazor
@page "/contact"
@using SmartWorkz.Core.Web.Components.FormBuilder

<h1>Contact Form</h1>

<ToastAlertComponent
    IsVisible="showAlert"
    Type="@alertType"
    Title="@alertTitle"
    MessageText="@alertMessage"
    AutoDismissMs="5000"
    OnDismiss="ClearAlert" />

<form @onsubmit="HandleSubmit">
    <div class="mb-3">
        <label class="form-label">Name</label>
        <input type="text" class="form-control" @bind="FormData.Name" required />
    </div>

    <div class="mb-3">
        <label class="form-label">Email</label>
        <input type="email" class="form-control" @bind="FormData.Email" required />
    </div>

    <div class="mb-3">
        <label class="form-label">Message</label>
        <textarea class="form-control" @bind="FormData.Message" rows="5" required></textarea>
    </div>

    <button type="submit" class="btn btn-primary">Send</button>
</form>

@code {
    private bool showAlert = false;
    private string alertType = "info";
    private string alertTitle = "";
    private string alertMessage = "";

    private class ContactForm
    {
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Message { get; set; } = "";
    }

    private ContactForm FormData = new();

    private async Task HandleSubmit()
    {
        try
        {
            // Simulate API call
            await Task.Delay(1000);

            // Validate
            if (string.IsNullOrWhiteSpace(FormData.Name))
            {
                ShowAlert("warning", "Validation Error", "Please enter your name");
                return;
            }

            // Submit
            // await ApiService.SendContact(FormData);

            ShowAlert("success", "Success!", "Your message has been sent successfully. We'll be in touch soon.");
            FormData = new();
        }
        catch (Exception ex)
        {
            ShowAlert("danger", "Error", $"Failed to send message: {ex.Message}");
        }
    }

    private void ShowAlert(string type, string title, string message)
    {
        alertType = type;
        alertTitle = title;
        alertMessage = message;
        showAlert = true;
    }

    private void ClearAlert()
    {
        showAlert = false;
    }
}
```

## Example 3: File Upload with Progress Notifications

```blazor
@page "/upload"
@using SmartWorkz.Core.Web.Components.FormBuilder

<h1>File Upload with Notifications</h1>

<ToastAlertComponent
    IsVisible="showUploadStatus"
    Type="@uploadStatusType"
    Title="@uploadStatusTitle"
    MessageText="@uploadStatusMessage"
    Dismissible="@uploadStatusDismissible"
    AutoDismissMs="@uploadAutoDissmis"
    OnDismiss="ClearUploadStatus" />

<div class="mb-3">
    <label class="form-label">Select File</label>
    <input type="file" class="form-control" @ref="fileInput" />
</div>

<button class="btn btn-primary" @onclick="UploadFile">Upload</button>

@code {
    private InputFile fileInput;
    private bool showUploadStatus = false;
    private string uploadStatusType = "info";
    private string uploadStatusTitle = "";
    private string uploadStatusMessage = "";
    private bool uploadStatusDismissible = true;
    private int? uploadAutoDissmis = null;

    private async Task UploadFile()
    {
        try
        {
            if (fileInput?.Files?.Count == 0)
            {
                ShowStatus("warning", "No File", "Please select a file to upload", 3000);
                return;
            }

            var file = fileInput.Files[0];

            // Validate file size (5MB max)
            if (file.Size > 5 * 1024 * 1024)
            {
                ShowStatus("danger", "File Too Large", "Maximum file size is 5MB", null);
                return;
            }

            ShowStatus("info", "Uploading", "Your file is being uploaded...", null);

            // Simulate upload
            await Task.Delay(2000);

            ShowStatus("success", "Upload Complete", $"'{file.Name}' uploaded successfully!", 5000);
        }
        catch (Exception ex)
        {
            ShowStatus("danger", "Upload Error", $"Error uploading file: {ex.Message}", null);
        }
    }

    private void ShowStatus(string type, string title, string message, int? autoDismiss)
    {
        uploadStatusType = type;
        uploadStatusTitle = title;
        uploadStatusMessage = message;
        uploadAutoDissmis = autoDismiss;
        uploadStatusDismissible = type != "info"; // Don't allow manual dismiss while uploading
        showUploadStatus = true;
    }

    private void ClearUploadStatus()
    {
        showUploadStatus = false;
    }
}
```

## Example 4: Custom Rich Content Alert

```blazor
@page "/advanced"
@using SmartWorkz.Core.Web.Components.FormBuilder

<h1>Advanced Alert Example</h1>

<button class="btn btn-primary" @onclick="ShowDetailedError">Show Detailed Error</button>

<ToastAlertComponent
    IsVisible="showAlert"
    Type="danger"
    Title="Validation Errors"
    Dismissible="true"
    OnDismiss="() => showAlert = false">
    <Message>
        <p>The following errors occurred:</p>
        <ul class="mb-0">
            <li>Email address is invalid</li>
            <li>Password must be at least 8 characters</li>
            <li>Terms and conditions must be accepted</li>
        </ul>
        <hr class="my-2">
        <small class="text-muted">Please correct these issues and try again.</small>
    </Message>
</ToastAlertComponent>

@code {
    private bool showAlert = false;

    private void ShowDetailedError()
    {
        showAlert = true;
    }
}
```

## Example 5: Persistent Alert with Rich Formatting

```blazor
@page "/important"
@using SmartWorkz.Core.Web.Components.FormBuilder

<h1>Important Announcement</h1>

<ToastAlertComponent
    IsVisible="true"
    Type="warning"
    Title="System Maintenance Notice"
    Dismissible="false"
    Message="CreateMaintenanceMessage()">
</ToastAlertComponent>

@code {
    private RenderFragment CreateMaintenanceMessage() => @<div>
        <p>We will be performing scheduled maintenance on <strong>Sunday, May 5th</strong> from 2:00 AM to 4:00 AM EST.</p>
        <p>During this time, the service may be unavailable. We apologize for any inconvenience.</p>
        <p class="mb-0">Thank you for your patience.</p>
    </div>;
}
```

## Example 6: API Error Handling

```blazor
@page "/data"
@using SmartWorkz.Core.Web.Components.FormBuilder
@inject HttpClient Http

<h1>Data Management</h1>

<ToastAlertComponent
    IsVisible="showAlert"
    Type="@alertType"
    Title="@alertTitle"
    MessageText="@alertMessage"
    AutoDismissMs="@alertAutoDissmis"
    OnDismiss="ClearAlert" />

<button class="btn btn-primary" @onclick="LoadData">Load Data</button>

@code {
    private bool showAlert = false;
    private string alertType = "info";
    private string alertTitle = "";
    private string alertMessage = "";
    private int? alertAutoDissmis = null;

    private async Task LoadData()
    {
        try
        {
            ShowAlert("info", "Loading", "Fetching data...", null);

            // Simulate API call
            await Task.Delay(2000);

            // Check for errors (simulate API response)
            var hasError = new Random().NextDouble() > 0.7;

            if (hasError)
            {
                throw new Exception("API returned error code 500");
            }

            ShowAlert("success", "Success", "Data loaded successfully!", 3000);
        }
        catch (HttpRequestException ex)
        {
            ShowAlert("danger", "Network Error", 
                $"Failed to connect to server: {ex.Message}", null);
        }
        catch (Exception ex)
        {
            ShowAlert("danger", "Error", 
                $"An error occurred: {ex.Message}", null);
        }
    }

    private void ShowAlert(string type, string title, string message, int? autoDissmis)
    {
        alertType = type;
        alertTitle = title;
        alertMessage = message;
        alertAutoDissmis = autoDissmis;
        showAlert = true;
    }

    private void ClearAlert()
    {
        showAlert = false;
    }
}
```

## Styling Tips

### CSS Classes for Container Styling
```css
.notification-container {
    position: fixed;
    top: 20px;
    right: 20px;
    z-index: 1050;
    max-width: 400px;
}

.notification-container .alert {
    margin-bottom: 12px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
    border-radius: 6px;
}
```

### Animated Appearance
```css
.alert.fade {
    animation: slideIn 0.3s ease-out;
}

@keyframes slideIn {
    from {
        transform: translateX(400px);
        opacity: 0;
    }
    to {
        transform: translateX(0);
        opacity: 1;
    }
}
```

## Common Patterns

### Pattern: Form Validation
Use "warning" type for validation errors that need user attention but won't auto-dismiss.

### Pattern: Success Confirmation
Use "success" type with 3-5 second auto-dismiss for operation confirmations.

### Pattern: Error Handling
Use "danger" type without auto-dismiss so users can read and act on errors.

### Pattern: Loading State
Use "info" type with non-dismissible setting while operations are in progress.
