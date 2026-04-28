# ToastAlertComponent Documentation

## Overview

The `ToastAlertComponent` is a Blazor component for displaying dismissible toast notifications/alerts with Bootstrap styling. It supports multiple alert types, auto-dismiss functionality, icons, and customizable content.

## Features

- **Multiple Alert Types**: Info, Success, Warning, and Danger with specific styling and icons
- **Dismissible Notifications**: Optional close button for manual dismissal
- **Auto-dismiss Timer**: Automatically dismiss alerts after a configurable timeout
- **Icon Display**: Contextual icons based on alert type
- **Title and Message**: Support for both title and message content
- **RenderFragment Support**: Rich content support via RenderFragment parameter
- **Callback Mechanism**: OnDismiss callback for handling dismissal events
- **Bootstrap Styling**: Full Bootstrap 5 alert classes and styling
- **Accessibility**: Proper ARIA labels and semantic HTML

## Parameters

### IsVisible (bool)
- **Default**: `false`
- **Description**: Controls whether the alert is visible on the page

### Type (string)
- **Default**: `"info"`
- **Valid Values**: `"info"`, `"success"`, `"warning"`, `"danger"`
- **Description**: Determines the alert styling, icon, and semantic meaning

### Title (string?)
- **Default**: `null`
- **Description**: Optional title displayed above the message. If provided, the icon appears next to the title.

### Message (RenderFragment?)
- **Default**: `null`
- **Description**: Rich content message supporting HTML and Blazor components

### MessageText (string?)
- **Default**: `null`
- **Description**: Plain text message. Used when Message RenderFragment is not provided. Can be used for simple text content.

### Dismissible (bool)
- **Default**: `true`
- **Description**: Controls whether the close button is displayed

### AutoDismissMs (int?)
- **Default**: `null`
- **Description**: Auto-dismiss timeout in milliseconds. Null or 0 disables auto-dismiss. Common values: 3000, 5000

### OnDismiss (EventCallback)
- **Default**: Empty callback
- **Description**: Callback invoked when the alert is dismissed (manually or via auto-dismiss)

## Usage Examples

### Basic Info Alert
```blazor
<ToastAlertComponent
    IsVisible="true"
    Type="info"
    MessageText="This is an information message" />
```

### Success Alert with Title
```blazor
<ToastAlertComponent
    IsVisible="isSuccess"
    Type="success"
    Title="Success!"
    MessageText="Your changes have been saved successfully"
    OnDismiss="HandleDismiss" />
```

### Warning Alert with Auto-dismiss
```blazor
<ToastAlertComponent
    IsVisible="showWarning"
    Type="warning"
    Title="Warning"
    MessageText="Please review this important information"
    AutoDismissMs="5000"
    OnDismiss="HandleDismiss" />
```

### Danger Alert with Rich Content
```blazor
<ToastAlertComponent
    IsVisible="showError"
    Type="danger"
    Title="Error"
    OnDismiss="HandleDismiss">
    <Message>
        <p>An error occurred:</p>
        <ul>
            <li>Error detail 1</li>
            <li>Error detail 2</li>
        </ul>
    </Message>
</ToastAlertComponent>
```

### Alert Without Close Button
```blazor
<ToastAlertComponent
    IsVisible="true"
    Type="info"
    MessageText="This notification cannot be manually dismissed"
    Dismissible="false"
    AutoDismissMs="3000" />
```

## Alert Types and Icons

| Type | Icon | Use Case |
|------|------|----------|
| `info` | `bi-info-circle` | Informational messages |
| `success` | `bi-check-circle` | Success/confirmation messages |
| `warning` | `bi-exclamation-triangle` | Warning/caution messages |
| `danger` | `bi-x-circle` | Error/danger messages |

## Bootstrap Classes

The component applies Bootstrap alert classes:
- Base: `alert alert-dismissible fade show`
- Type-specific: `alert-info`, `alert-success`, `alert-warning`, `alert-danger`

## Component Integration

### In a Blazor Page
```blazor
@page "/notifications"
@using SmartWorkz.Core.Web.Components.FormBuilder

<h1>Toast Notifications Demo</h1>

<button class="btn btn-success" @onclick="ShowSuccess">Show Success</button>
<button class="btn btn-danger" @onclick="ShowError">Show Error</button>

<ToastAlertComponent
    IsVisible="showAlert"
    Type="@alertType"
    Title="@alertTitle"
    MessageText="@alertMessage"
    AutoDismissMs="@autoDismissMs"
    OnDismiss="HandleDismiss" />

@code {
    private bool showAlert = false;
    private string alertType = "info";
    private string alertTitle = "";
    private string alertMessage = "";
    private int? autoDismissMs = null;

    private void ShowSuccess()
    {
        alertType = "success";
        alertTitle = "Success";
        alertMessage = "Operation completed successfully!";
        autoDismissMs = 3000;
        showAlert = true;
    }

    private void ShowError()
    {
        alertType = "danger";
        alertTitle = "Error";
        alertMessage = "An error occurred. Please try again.";
        autoDismissMs = null;
        showAlert = true;
    }

    private async Task HandleDismiss()
    {
        showAlert = false;
        await Task.CompletedTask;
    }
}
```

## Styling Customization

The component uses Bootstrap's alert styling. To customize:

1. **Override Bootstrap variables** in your CSS for alert colors
2. **Apply custom CSS classes** via the parent container
3. **Modify Bootstrap CSS** in your project

### Example Custom CSS
```css
.custom-alert {
    border-radius: 8px;
    box-shadow: 0 2px 8px rgba(0,0,0,0.1);
}
```

## Accessibility Features

- Uses semantic HTML with `role="alert"`
- Close button has proper `aria-label="Close"`
- Icons use Bootstrap Icons (bi-*) for semantic representation
- Proper contrast ratios following Bootstrap alert styling

## Testing

The component includes 25+ unit tests covering:
- Visibility rendering
- Alert type styling
- Title and message display
- Dismiss button callback
- Auto-dismiss timer
- Icon display per type
- Multiple instances
- Parameter combinations

Run tests:
```bash
dotnet test
```

## Known Limitations

1. **Auto-dismiss** uses `Task.Delay()` which continues in background even if component unmounts
2. **RenderFragment** and `MessageText` are mutually exclusive (Message takes precedence)
3. **Rapid dismissals** will queue multiple callbacks

## Future Enhancements

- Position variants (top-right, bottom-center, etc.)
- Dismiss callback with dismiss method parameter
- Custom CSS class support
- Animation customization
- Toast stacking/queue management
- Accessibility improvements (focus management)

## Browser Support

- All modern browsers (Chrome, Firefox, Safari, Edge)
- Requires Bootstrap 5+ CSS
- Requires Bootstrap Icons for icon display

## Dependencies

- Blazor 9.0+
- Bootstrap 5.0+
- Bootstrap Icons 1.0+

## Related Components

- `FormBuilderComponent` - Complete form builder with validation
- `FormFieldComponent` - Individual form field rendering
- `FileUploadComponent` - File upload with validation
