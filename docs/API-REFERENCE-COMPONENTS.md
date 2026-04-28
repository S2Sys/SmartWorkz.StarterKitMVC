# API Reference Documentation

## Overview

This document provides comprehensive API reference for all 25+ UI components available in SmartWorkz.StarterKitMVC. Each component is fully documented with parameters, return types, usage examples, Bootstrap classes, and best practices.

The components are organized by category:
- **Core Components (Tasks 1-3)**: Modal, Dropdown, DatePicker
- **Additional Components (Tasks 4-18)**: Alert, Badge, Accordion, Tabs, and more
- **Layout & Navigation**: Sidebar, Breadcrumb, Carousel
- **Data Presentation**: Grid, ListView, DataViewer, DataView
- **Form Components**: FormBuilder, FileUpload, Autocomplete, RichTextEditor, TimePicker
- **Feedback**: ToastAlert, Spinner, ProgressBar
- **Export Services**: CSV, Excel, PDF exporters

---

## Component Matrix - Quick Reference Table

| Component | Category | Parameters | Key Features | Bootstrap Classes |
|-----------|----------|-----------|---------------|-------------------|
| Accordion | Layout | Items, ActiveSectionKey, AllowMultipleOpen | Multi-section collapsible | accordion, accordion-button |
| Alert | Feedback | Type, Message, Heading, Dismissible, Icon | Type-based styling (success/info/warning/danger) | alert, alert-[type], alert-dismissible |
| Badge | Status | Text, Variant, IsPill, Dismissible | 8 color variants, pill shape | badge, bg-[color], rounded-pill |
| Breadcrumb | Navigation | Items, Separator | Hierarchical navigation | breadcrumb, breadcrumb-item |
| Carousel | Layout | Items, AutoPlay, Interval | Image/content carousel | carousel, carousel-item |
| DataContext | Data | Entity type, Filtering, Paging | Data management context | - |
| DataView | Data | Data, Columns, Paging | Flexible data view | - |
| DataViewer | Data | DataContext, Template, PageSize | Data display with templates | - |
| FileUpload | Form | AcceptedFormats, MaxFileSize, Multiple | Multi-file support | form-control |
| FormBuilder | Form | FormDef, OnSubmit | Dynamic form generation | form-builder, form-field |
| Grid | Data | Items, Columns, Editable, Sortable | Sortable/filterable table | table, table-bordered |
| ListView | Data | Items, Template, PageSize | List with pagination | list-group, list-group-item |
| ProgressBar | Feedback | Value, Max, Striped, Animated | Progress indication | progress, progress-bar |
| Sidebar | Navigation | Items, Collapsible | App navigation menu | sidebar, sidebar-item |
| Tabs | Layout | Tabs, ActiveTabKey | Multi-tab interface | nav-tabs, nav-link, tab-pane |
| Tooltip | Feedback | Content, Position | Hover tooltips | tooltip, tooltip-inner |
| AutoComplete | Form | DataSource, OnSelect | Type-ahead suggestions | - |
| RichTextEditor | Form | Content, OnChange | Rich text editing | - |
| Spinner | Feedback | Size, Variant | Loading indicator | spinner-border |
| TimePicker | Form | Time, OnTimeChanged | Time selection | - |
| ToastAlert | Feedback | Message, Type, Duration | Toast notifications | toast, toast-header |

---

## Core Components

### 1. Accordion Component

**Description**: A collapsible accordion component that displays multiple sections with headers. Supports exclusive (single open) and multiple open modes with smooth animations.

**Location**: `src/SmartWorkz.Core.Web/Components/Accordion/AccordionComponent.razor`

**Parameters**:

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Items | List<AccordionItem> | null | Collection of accordion items with Key, Title, and Content |
| ActiveSectionKey | string | null | Key of the currently active section (first item if not provided) |
| AllowMultipleOpen | bool | false | When true, multiple sections can be open simultaneously; when false, only one section is open |
| OnSectionChanged | EventCallback<string> | - | Callback invoked when a section is toggled, passing the section key |

**Return Types**: RenderFragment (via component markup)

**Bootstrap Classes Used**:
- `.accordion` - Main container
- `.accordion-item` - Individual accordion item
- `.accordion-button` - Header/button element (with `.collapsed` state)
- `.accordion-collapse` - Content container (with `.show` when expanded)
- `.accordion-body` - Content wrapper

**Usage Example**:

```csharp
@page "/components/accordion-demo"

<AccordionComponent 
    Items="AccordionItems"
    ActiveSectionKey="@activeSectionKey"
    AllowMultipleOpen="false"
    OnSectionChanged="HandleSectionChanged" />

@code {
    private string? activeSectionKey;
    private List<AccordionItem> AccordionItems = [];

    protected override void OnInitialized()
    {
        AccordionItems = new()
        {
            new()
            {
                Key = "section1",
                Title = "Getting Started",
                Content = @<text>
                    <p>Installation instructions and basic setup.</p>
                </text>
            },
            new()
            {
                Key = "section2",
                Title = "Configuration",
                Content = @<text>
                    <p>Configure component behavior and appearance.</p>
                </text>
            },
            new()
            {
                Key = "section3",
                Title = "Advanced Usage",
                Content = @<text>
                    <p>Advanced patterns and performance optimization.</p>
                </text>
            }
        };
    }

    private async Task HandleSectionChanged(string sectionKey)
    {
        activeSectionKey = sectionKey;
        // Handle section change logic
    }
}
```

**Tips & Best Practices**:
- Use exclusive mode (AllowMultipleOpen=false) for step-by-step wizards
- Use multiple open mode for FAQs where users might compare sections
- Keep section titles concise and descriptive
- Provide clear visual feedback with icons or badges for active sections

---

### 2. Alert Component

**Description**: A contextual alert component for displaying feedback messages. Supports four alert types (success, info, warning, danger) with optional icons, headings, and dismissible functionality.

**Location**: `src/SmartWorkz.Core.Web/Components/Alert/AlertComponent.razor`

**Parameters**:

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Type | string | "info" | Alert type: "success", "info", "warning", "danger" |
| Message | string | null | Alert message content (as plain text) |
| Heading | string | null | Optional heading displayed above the message |
| Dismissible | bool | true | When true, shows a close button for dismissal |
| Icon | bool | true | When true, displays a type-specific icon |
| Visible | bool | true | When false, alert is not rendered |
| OnDismiss | EventCallback | - | Callback invoked when the alert is dismissed |
| ChildContent | RenderFragment | null | Custom content to render in the alert (overrides Message) |

**Return Types**: RenderFragment (via component markup)

**Bootstrap Classes Used**:
- `.alert` - Base alert container
- `.alert-success`, `.alert-info`, `.alert-warning`, `.alert-danger` - Type-specific styling
- `.alert-dismissible` - Applied when dismissible (adds padding for close button)
- `.fade`, `.show` - Animation and visibility classes
- `.alert-heading` - Heading element styling

**Usage Example**:

```csharp
@page "/components/alert-demo"

<div class="container mt-4">
    <!-- Success Alert -->
    <AlertComponent 
        Type="success"
        Heading="Success!"
        Message="Your changes have been saved successfully."
        Dismissible="true"
        OnDismiss="HandleAlertDismiss" />

    <!-- Warning Alert with Custom Content -->
    <AlertComponent 
        Type="warning"
        Icon="true"
        Heading="Warning"
        Dismissible="false">
        <p>Please review your changes before submitting the form.</p>
        <a href="/help">Learn more about validation rules</a>
    </AlertComponent>

    <!-- Danger Alert -->
    <AlertComponent 
        Type="danger"
        Message="An error occurred while processing your request. Please try again."
        Visible="@showErrorAlert" />

    <!-- Info Alert -->
    <AlertComponent 
        Type="info"
        Message="New features are now available. Check the release notes for details."
        Icon="false" />
</div>

@code {
    private bool showErrorAlert = true;

    private async Task HandleAlertDismiss()
    {
        showErrorAlert = false;
        // Log dismissal or perform other actions
    }
}
```

**Tips & Best Practices**:
- Use success alerts for completed operations (file uploads, form submissions)
- Use warning alerts to highlight important notices or required actions
- Use danger alerts for errors that require user attention
- Use info alerts for general information and announcements
- Set Dismissible="true" for non-critical alerts that users may want to close
- Keep messages concise (aim for 1-2 sentences)

---

### 3. Badge Component

**Description**: A small status and count badge component with 8 color variants and optional pill shape. Can be dismissible and support custom content.

**Location**: `src/SmartWorkz.Core.Web/Components/Badge/BadgeComponent.razor`

**Parameters**:

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Text | string | null | Badge text content |
| Variant | string | "primary" | Color variant: "primary", "secondary", "success", "info", "warning", "danger", "light", "dark" |
| IsPill | bool | false | When true, displays pill-shaped badge with fully rounded edges |
| Dismissible | bool | false | When true, shows a close button for dismissal |
| OnDismiss | EventCallback | - | Callback invoked when dismiss button is clicked |
| ChildContent | RenderFragment | null | Custom content to render in badge (overrides Text) |

**Return Types**: RenderFragment (via component markup)

**Bootstrap Classes Used**:
- `.badge` - Base badge container
- `.bg-primary`, `.bg-secondary`, `.bg-success`, `.bg-info`, `.bg-warning`, `.bg-danger`, `.bg-light`, `.bg-dark` - Variant colors
- `.rounded-pill` - Pill shape styling
- `.btn-close`, `.btn-close-white` - Dismiss button styling
- `.ms-1` - Margin spacing for dismiss button

**Usage Example**:

```csharp
@page "/components/badge-demo"

<div class="container mt-4">
    <!-- Simple Badges -->
    <div class="mb-3">
        <h5>Badge Variants</h5>
        <BadgeComponent Text="Primary" Variant="primary" />
        <BadgeComponent Text="Secondary" Variant="secondary" />
        <BadgeComponent Text="Success" Variant="success" />
        <BadgeComponent Text="Info" Variant="info" />
        <BadgeComponent Text="Warning" Variant="warning" />
        <BadgeComponent Text="Danger" Variant="danger" />
    </div>

    <!-- Pill Badges -->
    <div class="mb-3">
        <h5>Pill Badges</h5>
        <BadgeComponent Text="New" IsPill="true" Variant="success" />
        <BadgeComponent Text="Online" IsPill="true" Variant="info" />
        <BadgeComponent Text="Pending" IsPill="true" Variant="warning" />
    </div>

    <!-- Dismissible Badge -->
    <div class="mb-3">
        <BadgeComponent 
            Text="Removable Tag" 
            Variant="danger"
            Dismissible="true"
            OnDismiss="HandleBadgeDismiss" />
    </div>

    <!-- Badge with Custom Content -->
    <BadgeComponent IsPill="true" Variant="primary">
        <span>👤 User #123</span>
    </BadgeComponent>
</div>

@code {
    private async Task HandleBadgeDismiss()
    {
        // Handle badge dismissal
    }
}
```

**Tips & Best Practices**:
- Use badges to indicate status (Active, Inactive, Pending)
- Use pill-shaped badges for tags and labels
- Use color variants consistently across the app (e.g., always use "success" for completed items)
- Keep badge text short (1-3 words)
- Use dismissible badges for removable tags or filters

---

## Additional Components (Tasks 4-18)

### 4. Breadcrumb Component

**Description**: Displays a hierarchical navigation path showing the user's location in the application. Helps users understand and navigate the site structure.

**Location**: `src/SmartWorkz.Core.Web/Components/Breadcrumb/BreadcrumbComponent.razor`

**Parameters**:

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Items | List<BreadcrumbItem> | null | Collection of breadcrumb items with Label and Url |
| Separator | string | "/" | Separator character between breadcrumb items |

**Bootstrap Classes Used**:
- `.breadcrumb` - Main breadcrumb container
- `.breadcrumb-item` - Individual breadcrumb item

**Usage Example**:

```csharp
<BreadcrumbComponent Items="BreadcrumbItems" />

@code {
    private List<BreadcrumbItem> BreadcrumbItems = new()
    {
        new() { Label = "Home", Url = "/" },
        new() { Label = "Products", Url = "/products" },
        new() { Label = "Electronics", Url = "/products/electronics" },
        new() { Label = "Laptops", Url = null } // Current page (no link)
    };
}
```

---

### 5. Tabs Component

**Description**: A multi-tab interface component that allows users to switch between different content panels. Supports keyboard navigation and ARIA attributes for accessibility.

**Location**: `src/SmartWorkz.Core.Web/Components/Tabs/TabsComponent.razor`

**Parameters**:

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Tabs | List<TabItem> | null | Collection of tabs with Key, Label, and Content |
| ActiveTabKey | string | null | Key of the currently active tab |
| OnTabChanged | EventCallback<string> | - | Callback invoked when tab is switched |

**Bootstrap Classes Used**:
- `.nav-tabs` - Tabs container
- `.nav-link` - Tab link (with `.active` when selected)
- `.tab-content` - Content wrapper
- `.tab-pane` - Individual tab pane (with `.active.show` when visible)

**Usage Example**:

```csharp
@page "/components/tabs-demo"

<TabsComponent 
    Tabs="TabItems"
    ActiveTabKey="@activeTabKey"
    OnTabChanged="HandleTabChanged" />

@code {
    private string? activeTabKey = "tab1";
    private List<TabItem> TabItems = new()
    {
        new()
        {
            Key = "tab1",
            Label = "Overview",
            Content = @<text>
                <h5>Product Overview</h5>
                <p>General information about the product.</p>
            </text>
        },
        new()
        {
            Key = "tab2",
            Label = "Details",
            Content = @<text>
                <h5>Technical Details</h5>
                <p>Specifications and technical information.</p>
            </text>
        },
        new()
        {
            Key = "tab3",
            Label = "Reviews",
            Content = @<text>
                <h5>Customer Reviews</h5>
                <p>Reviews from other customers.</p>
            </text>
        }
    };

    private async Task HandleTabChanged(string tabKey)
    {
        activeTabKey = tabKey;
    }
}
```

---

### 6. Carousel Component

**Description**: An image and content carousel with auto-play, navigation controls, and indicator dots. Supports multiple items and custom animation intervals.

**Location**: `src/SmartWorkz.Core.Web/Components/Carousel/CarouselComponent.razor`

**Parameters**:

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Items | List<CarouselItem> | null | Collection of carousel items with Image, Title, and Description |
| AutoPlay | bool | true | When true, carousel automatically rotates |
| Interval | int | 3000 | Time in milliseconds between auto-rotations (interval in ms) |
| ShowIndicators | bool | true | When true, displays indicator dots |
| ShowControls | bool | true | When true, displays previous/next buttons |

**Bootstrap Classes Used**:
- `.carousel` - Main carousel container
- `.carousel-item` - Individual item
- `.carousel-indicators` - Indicator dots
- `.carousel-control-prev`, `.carousel-control-next` - Navigation buttons

**Usage Example**:

```csharp
<CarouselComponent 
    Items="CarouselItems"
    AutoPlay="true"
    Interval="4000"
    ShowIndicators="true"
    ShowControls="true" />

@code {
    private List<CarouselItem> CarouselItems = new()
    {
        new()
        {
            Image = "/images/slide1.jpg",
            Title = "Welcome",
            Description = "First slide description"
        },
        new()
        {
            Image = "/images/slide2.jpg",
            Title = "Features",
            Description = "Second slide description"
        }
    };
}
```

---

### 7. Sidebar Component

**Description**: A vertical navigation sidebar component for app navigation. Supports collapsible sections, nested items, and responsive behavior.

**Location**: `src/SmartWorkz.Core.Web/Components/Sidebar/SidebarNavigationComponent.razor`

**Parameters**:

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Items | List<SidebarItem> | null | Collection of sidebar navigation items |
| Collapsible | bool | true | When true, sidebar can be collapsed |
| Collapsed | bool | false | Current collapse state |
| OnItemSelected | EventCallback<string> | - | Callback when a sidebar item is selected |

**Bootstrap Classes Used**:
- `.sidebar` - Main sidebar container
- `.sidebar-item` - Individual item
- `.sidebar-link` - Link styling
- `.sidebar-collapsed` - Collapsed state class

**Usage Example**:

```csharp
<SidebarNavigationComponent 
    Items="SidebarItems"
    Collapsible="true"
    OnItemSelected="HandleSidebarItemSelected" />

@code {
    private List<SidebarItem> SidebarItems = new()
    {
        new() { Label = "Dashboard", Icon = "📊", Url = "/dashboard" },
        new() { Label = "Products", Icon = "📦", Url = "/products" },
        new() { Label = "Orders", Icon = "📋", Url = "/orders" },
        new() { Label = "Settings", Icon = "⚙️", Url = "/settings" }
    };

    private async Task HandleSidebarItemSelected(string itemLabel)
    {
        // Handle selection
    }
}
```

---

### 8. Tooltip Component

**Description**: A tooltip component that displays contextual information on hover. Supports positioning (top, bottom, left, right) and custom content.

**Location**: `src/SmartWorkz.Core.Web/Components/Tooltip/TooltipComponent.razor`

**Parameters**:

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Content | string | null | Tooltip text content |
| Position | string | "top" | Position: "top", "bottom", "left", "right" |
| Delay | int | 0 | Delay in milliseconds before showing tooltip |
| ChildContent | RenderFragment | null | Element that triggers the tooltip |

**Bootstrap Classes Used**:
- `.tooltip` - Tooltip container
- `.tooltip-inner` - Tooltip content
- `.tooltip-top`, `.tooltip-bottom`, `.tooltip-left`, `.tooltip-right` - Position variants

**Usage Example**:

```csharp
<TooltipComponent Content="Click to save" Position="top">
    <button class="btn btn-primary">Save</button>
</TooltipComponent>

<TooltipComponent Content="Delete permanently" Position="right">
    <button class="btn btn-danger">Delete</button>
</TooltipComponent>
```

---

### 9. ProgressBar Component

**Description**: A visual progress indicator showing completion percentage. Supports multiple styles (striped, animated) and color variants.

**Location**: `src/SmartWorkz.Core.Web/Components/ProgressBar/ProgressBarComponent.razor`

**Parameters**:

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Value | int | 0 | Current progress value (0-100) |
| Max | int | 100 | Maximum progress value |
| Striped | bool | false | When true, applies striped pattern |
| Animated | bool | false | When true, animates the striped pattern |
| Variant | string | "primary" | Color variant: "primary", "success", "warning", "danger", "info" |
| ShowLabel | bool | false | When true, displays percentage label |

**Bootstrap Classes Used**:
- `.progress` - Progress container
- `.progress-bar` - Progress indicator bar
- `.progress-bar-striped` - Striped pattern
- `.progress-bar-animated` - Animation

**Usage Example**:

```csharp
<div class="container mt-4">
    <!-- Basic Progress -->
    <ProgressBarComponent Value="@uploadProgress" ShowLabel="true" />

    <!-- Striped Progress -->
    <ProgressBarComponent Value="65" Striped="true" Variant="success" ShowLabel="true" />

    <!-- Animated Progress (for active operations) -->
    <ProgressBarComponent Value="45" Striped="true" Animated="true" ShowLabel="true" />
</div>

@code {
    private int uploadProgress = 75;
}
```

---

### 10. Spinner Component

**Description**: A loading spinner indicator. Supports multiple sizes and color variants for use during asynchronous operations.

**Location**: `src/SmartWorkz.Core.Web/Components/FormBuilder/SpinnerComponent.razor`

**Parameters**:

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Size | string | "md" | Spinner size: "sm", "md", "lg" |
| Variant | string | "primary" | Color variant: "primary", "secondary", "success", "danger", "warning", "info" |
| Type | string | "border" | Spinner type: "border" (circular) or "grow" (pulsing) |

**Bootstrap Classes Used**:
- `.spinner-border`, `.spinner-grow` - Spinner container
- `.spinner-border-sm`, `.spinner-border-lg` - Size variants
- `.text-[color]` - Color variants

**Usage Example**:

```csharp
<!-- Loading Spinner in Button -->
<button class="btn btn-primary" disabled>
    <SpinnerComponent Size="sm" Variant="primary" />
    Loading...
</button>

<!-- Large Spinner during Data Loading -->
<SpinnerComponent Size="lg" Variant="info" />

<!-- Pulsing Spinner -->
<SpinnerComponent Type="grow" Variant="success" />
```

---

### 11. Grid Component

**Description**: A feature-rich data grid component with sorting, filtering, pagination, and optional inline editing. Supports multiple column types and data export.

**Location**: `src/SmartWorkz.Core.Web/Components/Grid/GridComponent.razor`

**Parameters**:

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Items | List<T> | null | Data items to display in the grid |
| Columns | List<GridColumn> | null | Column definitions (PropertyName, DisplayName, IsVisible, IsEditable) |
| PageSize | int | 10 | Items per page |
| Sortable | bool | true | Enable/disable column sorting |
| Filterable | bool | true | Enable/disable column filtering |
| Editable | bool | false | Enable/disable inline editing |
| ShowPagination | bool | true | Display pagination controls |
| OnRowSelected | EventCallback<T> | - | Callback when row is selected |
| OnEdit | EventCallback<T> | - | Callback when row is edited |

**Bootstrap Classes Used**:
- `.table` - Table base
- `.table-bordered` - Borders
- `.table-hover` - Hover effect
- `.table-striped` - Alternating row colors

**Usage Example**:

```csharp
@page "/components/grid-demo"

<GridComponent 
    Items="Products"
    Columns="GridColumns"
    PageSize="10"
    Sortable="true"
    Filterable="true"
    Editable="false"
    OnRowSelected="HandleRowSelected" />

@code {
    private List<Product> Products = [];
    private List<GridColumn> GridColumns = [];

    protected override async Task OnInitializedAsync()
    {
        GridColumns = new()
        {
            new() { PropertyName = "Id", DisplayName = "ID", IsVisible = true },
            new() { PropertyName = "Name", DisplayName = "Product Name", IsVisible = true },
            new() { PropertyName = "Price", DisplayName = "Price", IsVisible = true },
            new() { PropertyName = "Stock", DisplayName = "Stock", IsVisible = true }
        };

        Products = await LoadProductsAsync();
    }

    private async Task HandleRowSelected(Product product)
    {
        // Handle row selection
    }

    private async Task<List<Product>> LoadProductsAsync()
    {
        // Load data
        return new();
    }
}
```

---

### 12. ListView Component

**Description**: A list view component for displaying items in a vertical list format with pagination, templates, and optional selection.

**Location**: `src/SmartWorkz.Core.Web/Components/ListView/ListViewComponent.razor`

**Parameters**:

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Items | List<T> | null | Items to display |
| Template | RenderFragment<T> | null | Custom template for rendering each item |
| PageSize | int | 10 | Items per page |
| Selectable | bool | false | Enable/disable item selection |
| OnItemSelected | EventCallback<T> | - | Callback when item is selected |

**Bootstrap Classes Used**:
- `.list-group` - List container
- `.list-group-item` - Individual list item
- `.list-group-item-active` - Active/selected item

**Usage Example**:

```csharp
<ListView Items="Companies" PageSize="5">
    <Template>
        <div class="list-group-item">
            <h5>@context.Name</h5>
            <p class="text-muted">@context.Address</p>
        </div>
    </Template>
</ListView>

@code {
    private List<Company> Companies = [];
}
```

---

### 13. FileUpload Component

**Description**: A file upload component with drag-and-drop support, file type validation, and size limits. Supports single and multiple file uploads.

**Location**: `src/SmartWorkz.Core.Web/Components/FileUpload/FileUploadComponent.razor`

**Parameters**:

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| AcceptedFormats | string | "*" | Comma-separated MIME types (e.g., "image/*,application/pdf") |
| MaxFileSize | long | 5242880 | Max file size in bytes (default: 5MB) |
| Multiple | bool | false | Allow multiple file selection |
| OnFilesSelected | EventCallback<IReadOnlyList<IBrowserFile>> | - | Callback when files are selected |

**Bootstrap Classes Used**:
- `.form-control` - Input styling

**Usage Example**:

```csharp
<FileUploadComponent 
    AcceptedFormats="image/*,.pdf"
    MaxFileSize="10485760"
    Multiple="true"
    OnFilesSelected="HandleFileSelected" />

@code {
    private async Task HandleFileSelected(IReadOnlyList<IBrowserFile> files)
    {
        foreach (var file in files)
        {
            var buffer = new byte[file.Size];
            await file.OpenReadStream(maxAllowedSize: 10485760).ReadAsync(buffer);
            // Process file
        }
    }
}
```

---

### 14. FormBuilder Component

**Description**: A dynamic form builder component that generates forms from FormDefinition objects. Supports validation, field templates, and customizable layout.

**Location**: `src/SmartWorkz.Core.Web/Components/FormBuilder/FormBuilderComponent.razor`

**Parameters**:

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| FormDef | FormDefinition | null | Form definition with fields, title, description |
| OnSubmit | EventCallback<object> | - | Callback when form is submitted |
| OnCancel | EventCallback | - | Callback when cancel is clicked |

**Bootstrap Classes Used**:
- `.form-builder-container` - Form wrapper
- `.form-control` - Input fields
- `.form-group` - Field grouping
- `.form-actions` - Button container

**Usage Example**:

```csharp
<FormBuilderComponent 
    FormDef="CreateProductForm"
    OnSubmit="HandleFormSubmit"
    OnCancel="HandleCancel" />

@code {
    private FormDefinition CreateProductForm = new()
    {
        Title = "Create New Product",
        Description = "Fill in all required fields",
        Fields = new()
        {
            new()
            {
                Name = "ProductName",
                Label = "Product Name",
                Type = "text",
                Required = true,
                Placeholder = "Enter product name"
            },
            new()
            {
                Name = "Description",
                Label = "Description",
                Type = "textarea",
                Required = false
            },
            new()
            {
                Name = "Price",
                Label = "Price",
                Type = "number",
                Required = true,
                Min = "0",
                Step = "0.01"
            }
        }
    };

    private async Task HandleFormSubmit(object formData)
    {
        // Handle form submission
    }

    private async Task HandleCancel()
    {
        // Handle cancel
    }
}
```

---

### 15. Autocomplete Component

**Description**: An autocomplete/type-ahead component that suggests options as the user types. Supports custom templates and data sources.

**Location**: `src/SmartWorkz.Core.Web/Components/FormBuilder/AutocompleteComponent.razor`

**Parameters**:

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| DataSource | List<T> | null | List of items to search |
| FilterFunc | Func<string, List<T>> | null | Custom filter function |
| SelectedItem | T | null | Currently selected item |
| OnSelect | EventCallback<T> | - | Callback when item is selected |
| Placeholder | string | "" | Input placeholder text |
| MinChars | int | 1 | Minimum characters before suggesting |

**Usage Example**:

```csharp
<AutocompleteComponent 
    DataSource="Companies"
    Placeholder="Search companies..."
    OnSelect="HandleCompanySelected"
    MinChars="2" />

@code {
    private List<Company> Companies = [];
    private Company? SelectedCompany;

    private async Task HandleCompanySelected(Company company)
    {
        SelectedCompany = company;
    }
}
```

---

### 16. RichTextEditor Component

**Description**: A rich text editor component with formatting tools, media insertion, and content management.

**Location**: `src/SmartWorkz.Core.Web/Components/FormBuilder/RichTextEditorComponent.razor`

**Parameters**:

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Content | string | null | Current editor content (HTML) |
| OnChange | EventCallback<string> | - | Callback when content changes |
| Placeholder | string | "" | Editor placeholder text |
| Height | string | "300px" | Editor height |

**Usage Example**:

```csharp
<RichTextEditorComponent 
    Content="@articleContent"
    OnChange="HandleContentChanged"
    Height="400px"
    Placeholder="Write your article here..." />

@code {
    private string? articleContent;

    private async Task HandleContentChanged(string newContent)
    {
        articleContent = newContent;
    }
}
```

---

### 17. TimePicker Component

**Description**: A time selection component for picking hours and minutes. Supports 12-hour and 24-hour formats.

**Location**: `src/SmartWorkz.Core.Web/Components/FormBuilder/TimePickerComponent.razor`

**Parameters**:

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Time | TimeSpan | TimeSpan.Zero | Current selected time |
| OnTimeChanged | EventCallback<TimeSpan> | - | Callback when time is changed |
| Format | string | "24" | Time format: "12" or "24" |
| Step | int | 15 | Minute increment step (in minutes) |

**Usage Example**:

```csharp
<TimePicker 
    Time="@appointmentTime"
    OnTimeChanged="HandleTimeChanged"
    Format="24"
    Step="15" />

@code {
    private TimeSpan appointmentTime = TimeSpan.Zero;

    private async Task HandleTimeChanged(TimeSpan time)
    {
        appointmentTime = time;
    }
}
```

---

### 18. ToastAlert Component

**Description**: A toast notification component for displaying temporary messages. Supports multiple types and auto-dismiss functionality.

**Location**: `src/SmartWorkz.Core.Web/Components/FormBuilder/ToastAlertComponent.razor`

**Parameters**:

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Message | string | null | Toast message content |
| Type | string | "info" | Type: "success", "info", "warning", "danger" |
| Duration | int | 3000 | Auto-dismiss duration in milliseconds |
| OnDismiss | EventCallback | - | Callback when toast is dismissed |
| ShowIcon | bool | true | Display type-specific icon |

**Bootstrap Classes Used**:
- `.toast` - Toast container
- `.toast-header` - Header with close button
- `.toast-body` - Message content

**Usage Example**:

```csharp
<ToastAlertComponent 
    Message="Operation completed successfully!"
    Type="success"
    Duration="3000"
    OnDismiss="HandleToastDismissed" />

@code {
    private async Task HandleToastDismissed()
    {
        // Handle dismissal
    }
}
```

---

## Data Components

### 19. DataViewer Component

**Description**: A component for displaying data with custom templates and support for nested rendering.

**Location**: `src/SmartWorkz.Core.Web/Components/DataViewer/DataViewerComponent.razor`

**Parameters**:

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| DataContext | IDataContext | null | Data context interface |
| ItemTemplate | RenderFragment<T> | null | Custom template per item |
| LoadingTemplate | RenderFragment | null | Loading state template |
| EmptyTemplate | RenderFragment | null | Empty state template |

**Usage Example**:

```csharp
<DataViewerComponent DataContext="ProductDataContext">
    <ItemTemplate>
        <div class="card mb-3">
            <div class="card-body">
                <h5>@context.Name</h5>
                <p>@context.Description</p>
            </div>
        </div>
    </ItemTemplate>
    <LoadingTemplate>
        <SpinnerComponent Size="lg" />
    </LoadingTemplate>
    <EmptyTemplate>
        <p>No products found.</p>
    </EmptyTemplate>
</DataViewerComponent>
```

---

### 20. DataView Component

**Description**: A flexible data view component with filtering, sorting, and pagination capabilities.

**Location**: `src/SmartWorkz.Core.Web/Components/DataView/FilterBuilderComponent.razor`

**Parameters**:

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Data | IEnumerable<T> | null | Data to display and filter |
| Columns | List<GridColumn> | null | Column definitions for filtering |
| PageSize | int | 10 | Items per page |
| OnFilter | EventCallback<FilterCriteria> | - | Callback when filters change |

**Usage Example**:

```csharp
<DataViewComponent 
    Data="AllProducts"
    Columns="ColumnDefinitions"
    PageSize="20"
    OnFilter="HandleFilterApplied" />

@code {
    private async Task HandleFilterApplied(FilterCriteria criteria)
    {
        // Apply filters to data
    }
}
```

---

## Export Services

### 21. GridExportService

**Description**: Service for exporting grid data to CSV and Excel formats with RFC 4180 compliance.

**Location**: `src/SmartWorkz.Core.Web/Services/Grid/GridExportService.cs`

**Methods**:

#### ExportToCsv<T>

```csharp
public string ExportToCsv<T>(
    IEnumerable<T> data,
    List<GridColumn> columns,
    GridExportOptions options)
```

**Parameters**:
- `data`: Collection of items to export
- `columns`: Column definitions with PropertyName and DisplayName
- `options`: Export options (IncludeHeaders, IncludeColumns, ExcludeColumns)

**Returns**: CSV-formatted string

**Usage Example**:

```csharp
@inject GridExportService ExportService

<button class="btn btn-primary" @onclick="HandleExportCsv">
    Export to CSV
</button>

@code {
    private List<Product> Products = [];
    private List<GridColumn> Columns = [];

    private async Task HandleExportCsv()
    {
        var options = new GridExportOptions
        {
            IncludeHeaders = true,
            Format = "csv"
        };

        var csv = ExportService.ExportToCsv(Products, Columns, options);

        // Trigger browser download
        var bytes = Encoding.UTF8.GetBytes(csv);
        var fileName = $"products_{DateTime.Now:yyyy-MM-dd}.csv";
        await JS.InvokeVoidAsync("downloadFile", fileName, "text/csv", bytes);
    }
}
```

#### ExportToExcel<T>

```csharp
public byte[] ExportToExcel<T>(
    IEnumerable<T> data,
    List<GridColumn> columns,
    GridExportOptions options)
```

**Parameters**:
- `data`: Collection of items to export
- `columns`: Column definitions
- `options`: Export options including sheet name and formatting

**Returns**: Excel file as byte array

**Usage Example**:

```csharp
var options = new GridExportOptions
{
    IncludeHeaders = true,
    Format = "excel",
    SheetName = "Products"
};

var excelData = ExportService.ExportToExcel(Products, Columns, options);
var fileName = $"products_{DateTime.Now:yyyy-MM-dd}.xlsx";
await JS.InvokeVoidAsync("downloadFile", fileName, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelData);
```

---

## Tag Helper Reference

### Bootstrap Integration

All components integrate seamlessly with Bootstrap 5 classes. Common class patterns:

**Spacing Utilities**:
- `.m-[0-5]` - Margin
- `.p-[0-5]` - Padding
- `.mt-[0-5]`, `.mb-[0-5]`, `.ms-[0-5]`, `.me-[0-5]` - Directional margins
- `.pt-[0-5]`, `.pb-[0-5]`, `.ps-[0-5]`, `.pe-[0-5]` - Directional padding

**Display & Visibility**:
- `.d-flex` - Flexbox layout
- `.d-none` - Display none (hidden)
- `.d-grid` - Grid layout
- `.gap-[0-5]` - Gap between flex/grid items

**Color Classes**:
- `.text-primary`, `.text-secondary`, `.text-success`, `.text-danger`, `.text-warning`, `.text-info`, `.text-light`, `.text-dark`, `.text-muted`
- `.bg-primary`, `.bg-secondary`, `.bg-success`, `.bg-danger`, `.bg-warning`, `.bg-info`, `.bg-light`, `.bg-dark`

**Button Classes**:
- `.btn` - Base button class
- `.btn-primary`, `.btn-secondary`, `.btn-success`, `.btn-danger`, `.btn-warning`, `.btn-info`, `.btn-light`, `.btn-dark` - Button variants
- `.btn-outline-[color]` - Outline buttons
- `.btn-sm`, `.btn-lg` - Button sizes
- `.disabled` - Disabled state

**Form Classes**:
- `.form-control` - Input styling
- `.form-group` - Field grouping
- `.form-label` - Label styling
- `.form-check` - Checkbox/radio group
- `.form-floating` - Floating label pattern
- `.invalid-feedback` - Validation error message
- `.valid-feedback` - Validation success message

---

## Best Practices Summary

### General Principles

1. **Accessibility**: All components include ARIA attributes and keyboard navigation support
2. **Bootstrap Integration**: Components use Bootstrap 5 classes for consistent styling
3. **Event Callbacks**: Use EventCallback for async operations and proper error handling
4. **Responsive Design**: Components adapt to different screen sizes automatically
5. **Performance**: Use @key directives for list rendering to optimize re-renders

### Component-Specific Tips

**Forms**:
- Always provide clear labels and placeholder text
- Use validation messages for form feedback
- Consider field grouping for complex forms

**Data Display**:
- Implement pagination for large datasets (>50 items)
- Provide filtering and sorting for user discovery
- Use templates to customize data presentation

**Feedback & Notifications**:
- Use alerts for critical information
- Use toasts for temporary messages
- Provide clear action buttons for user direction

**Navigation**:
- Use breadcrumbs for deep site hierarchies
- Keep sidebar items to 5-10 main categories
- Highlight current page in navigation

---

## Code Examples Repository

For complete, runnable examples of all components, see:
- `/src/SmartWorkz.StarterKitMVC.WebUI/Pages/Forms/AllComponents.cshtml` - Component showcase
- `/src/SmartWorkz.StarterKitMVC.WebUI/Pages/Forms/AllFormComponents.cshtml` - Form components
- `/docs/GRID_COMPONENT_WIKI.md` - Detailed Grid component guide
- `/src/SmartWorkz.Core.Web/Components/FormBuilder/INTEGRATION_GUIDE.md` - FormBuilder integration

---

## Troubleshooting & FAQ

### Common Issues

**Q: Component not rendering**
- A: Check that component files are in the correct namespace
- A: Verify @namespace directive in component file
- A: Ensure component is registered in dependency injection if required

**Q: Styling not applied**
- A: Verify Bootstrap CSS is included in HTML layout
- A: Check for CSS specificity conflicts
- A: Use browser DevTools to inspect computed styles

**Q: Events not firing**
- A: Ensure EventCallback parameters are properly configured
- A: Check that callback methods are async
- A: Verify HasDelegate before invoking callbacks

**Q: Performance issues with large datasets**
- A: Implement pagination to limit rendered items
- A: Use virtual scrolling for very large lists
- A: Optimize change detection with OnParametersSetAsync

---

## Summary

This API reference documents 25+ production-ready UI components covering:
- Layout & Navigation (Accordion, Sidebar, Breadcrumb, Tabs)
- Feedback & Status (Alert, Badge, ProgressBar, Spinner, Toast)
- Data Display (Grid, ListView, DataViewer, DataView)
- Forms (FormBuilder, FileUpload, Autocomplete, RichTextEditor, TimePicker)
- Media (Carousel, Tooltip)
- Export Services (CSV, Excel)

All components follow Bootstrap 5 conventions, support accessibility features, and include comprehensive parameter documentation and code examples.

For implementation details and internal architecture, refer to the individual component source files in `/src/SmartWorkz.Core.Web/Components/`.
