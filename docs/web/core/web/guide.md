# Web Usage Guide

## Overview

Specifies response caching for MVC action methods and controllers.

## Examples

### DataContext`1

```csharp
// Initialize with data
             var context = new DataContext<Product>();
             await context.Initialize(products);
            
             // Subscribe to state changes
             context.OnStateChanged += () => Console.WriteLine("State changed!");
            
             // Filter by category - async operation
             await context.UpdateFilter("Category", "equals", "Electronics");
             // CurrentRequest.Filters now contains { "Category": "Electronics" }
             // CurrentResponse.Data reloaded asynchronously
            
             // Change sort - resets to page 1
             await context.UpdateSort("Price", isDescending: true);
            
             // Change pagination
             await context.UpdatePagination(pageNumber: 2, pageSize: 50);
            
             // Row selection - synchronous operations
             context.ToggleRowSelection(1);          // Select row with ID=1
             context.ToggleSelectAll(true);          // Select all visible rows
             var selected = context.SelectedRowIds;  // List<object> containing IDs
            
             // Check loading state
             if (context.IsLoading)
             {
                 // Show spinner
             }
            
             // Handle errors
             if (context.Error != null)
             {
                 Console.WriteLine($"Error: {context.Error}");
             }
```

### ButtonTagHelper

```csharp
<!-- Primary button (default submit button) -->
             <button type="submit" variant="primary">Submit</button>
             <!-- Generates: <button class="btn btn-primary" type="submit">Submit</button> -->
            
             <!-- Small secondary button -->
             <button variant="secondary" size="sm">Cancel</button>
             <!-- Generates: <button class="btn btn-secondary btn-sm">Cancel</button> -->
            
             <!-- Large danger button -->
             <button variant="danger" size="lg">Delete</button>
             <!-- Generates: <button class="btn btn-danger btn-lg" disabled="disabled">Delete</button> -->
            
             <!-- Success button with loading state -->
             <button variant="success" is-loading="true">Processing...</button>
             <!-- Generates: <button class="btn btn-success disabled" disabled="disabled">Processing...</button> -->
            
             <!-- Link styled as a button -->
             <a href="/dashboard" variant="info">Go to Dashboard</a>
             <!-- Generates: <a class="btn btn-info" href="/dashboard">Go to Dashboard</a> -->
            
             <!-- Warning button with custom CSS class -->
             <button variant="warning" class="mt-2">Warning Action</button>
             <!-- Generates: <button class="mt-2 btn btn-warning">Warning Action</button> -->
```

### IconTagHelper

```csharp
<!-- Standalone success icon -->
             <icon name="Success" />
             <!-- Generates: <i class="bi bi-check-circle-fill"></i> -->
            
             <!-- Small icon with custom CSS class -->
             <icon name="Info" size="sm" css-class="text-info" />
             <!-- Generates: <i class="bi bi-info-circle me-1 text-info"></i> -->
            
             <!-- Large error icon in red -->
             <icon name="Error" size="lg" css-class="text-danger" />
             <!-- Generates: <i class="bi bi-exclamation-circle fs-5 text-danger"></i> -->
            
             <!-- Search icon in a button -->
             <button type="button" class="btn btn-primary">
               <icon name="Search" size="sm" /> Search
             </button>
            
             <!-- Warning icon with emphasis -->
             <icon name="Warning" size="lg" css-class="text-warning me-2" />
             <span>Please verify your information</span>
            
             <!-- Home navigation icon -->
             <a href="/"><icon name="Home" /> Home</a>
            
             <!-- User account icon -->
             <a href="/settings"><icon name="User" size="sm" /> Settings</a>
```

### AlertTagHelper

```csharp
<!-- Simple success alert -->
             <alert type="success" message="Profile updated successfully!" />
            
             <!-- Non-dismissible danger alert -->
             <alert type="danger" message="An error occurred while saving." dismissible="false" />
            
             <!-- Warning alert with dismiss button -->
             <alert type="warning" message="This action cannot be undone." />
            
             <!-- Default info alert -->
             <alert message="Remember to save your changes regularly." />
```

### BadgeTagHelper

```csharp
<!-- Simple primary badge -->
             <badge type="primary" text="New" />
            
             <!-- Count badge -->
             <badge type="success" text="5 items" />
            
             <!-- Danger badge for inactive status -->
             <badge type="danger" text="Inactive" />
            
             <!-- Warning badge -->
             <badge type="warning" text="Pending Review" />
            
             <!-- Default secondary badge -->
             <badge text="Badge" />
            
             <!-- Pill-shaped badge -->
             <span class="badge bg-primary rounded-pill">@notificationCount</span>
```

### PaginationTagHelper

```csharp
<!-- Basic pagination with defaults (shows 5 pages max) -->
             <pagination current-page="2" total-pages="10" />
            
             <!-- Pagination with custom page URL pattern -->
             <pagination current-page="1" total-pages="5" page-url="/products?page={0}" />
            
             <!-- Pagination with more visible pages -->
             <pagination current-page="3" total-pages="15" max-visible="7" />
            
             <!-- Pagination on last page (Next is disabled) -->
             <pagination current-page="10" total-pages="10" />
            
             <!-- Pagination with single page (not rendered) -->
             <pagination current-page="1" total-pages="1" />
```

### CheckboxTagHelper

```csharp
<!-- Simple checkbox with label -->
             <checkbox-tag for="User.IsSubscribed" label="Subscribe to newsletter" />
            
             <!-- Checkbox with custom value -->
             <checkbox-tag for="User.AgreedToTerms" label="I agree to the terms" value="1" />
            
             <!-- Pre-checked checkbox -->
             <checkbox-tag for="User.IsActive" label="Active" checked="true" />
            
             <!-- In form-group wrapper for complete form control -->
             <form-group for="User.IsSubscribed" label="Newsletter Subscription" help-text="Get updates via email">
               <checkbox-tag for="User.IsSubscribed" label="Subscribe" />
             </form-group>
```

### FileInputTagHelper

```csharp
<!-- Simple file input -->
             <file-input-tag for="Model.ProfilePhoto" />
            
             <!-- File input accepting specific types -->
             <file-input-tag for="Model.Document" accept=".pdf,.docx" required="true" />
            
             <!-- Multiple file upload -->
             <file-input-tag for="Model.Attachments" accept="image/*" multiple="true" />
            
             <!-- In form-group wrapper -->
             <form-group for="Model.ProfilePhoto" label="Upload Photo" required="true" help-text="JPG or PNG, max 5MB">
               <file-input-tag for="Model.ProfilePhoto" accept="image/jpeg,image/png" />
             </form-group>
```

### FormGroupTagHelper

```csharp
<!-- Simple form-group with text input -->
             <form-group for="Model.Name" label="Full Name" required="true">
               <input-tag for="Model.Name" placeholder="Enter your full name" />
             </form-group>
            
             <!-- Form-group with help text -->
             <form-group for="Model.Email" label="Email Address" required="true" help-text="We'll never share your email">
               <input-tag for="Model.Email" type="email" placeholder="you@example.com" />
             </form-group>
            
             <!-- Form-group with select control -->
             <form-group for="Model.CountryId" label="Country" required="true" help-text="Select your country of residence">
               <select-tag for="Model.CountryId" items="@countries" />
             </form-group>
            
             <!-- Form-group with textarea -->
             <form-group for="Model.Comments" label="Comments" help-text="Optional feedback (max 500 characters)">
               <textarea-tag for="Model.Comments" rows="4" placeholder="Share your thoughts..." />
             </form-group>
            
             <!-- Form-group with checkbox -->
             <form-group for="Model.IsSubscribed" label="Newsletter">
               <checkbox-tag for="Model.IsSubscribed" label="Subscribe to newsletter" />
             </form-group>
            
             <!-- Form-group with validation error state -->
             <!-- When ModelState contains error for Model.Age, child controls show .is-invalid state -->
             <form-group for="Model.Age" label="Age" required="true" help-text="Must be 18 or older">
               <input-tag for="Model.Age" type="number" placeholder="Enter your age" />
             </form-group>
```

### FormTagHelper

```csharp
<!-- Simple form with default POST method -->
             <form-tag>
               <form-group for="Model.Name" label="Full Name" required="true">
                 <input-tag for="Model.Name" placeholder="Enter your name" />
               </form-group>
               <button type="submit" class="btn btn-primary">Submit</button>
             </form-tag>
            
             <!-- Form with custom action and GET method -->
             <form-tag method="get" action="/search">
               <input-tag for="Model.SearchTerm" placeholder="Search..." />
               <button type="submit" class="btn btn-primary">Search</button>
             </form-tag>
            
             <!-- Form with custom CSS class and validation disabled -->
             <form-tag class="login-form" novalidate="true">
               <form-group for="Model.Email" label="Email" required="true">
                 <input-tag for="Model.Email" type="email" />
               </form-group>
               <button type="submit" class="btn btn-primary">Login</button>
             </form-tag>
```

### InputTagHelper

```csharp
<!-- Simple text input -->
             <input-tag for="User.Name" placeholder="Enter your name" />
            
             <!-- Email input with validation -->
             <input-tag for="User.Email" type="email" placeholder="Enter email" required="true" />
            
             <!-- Password input -->
             <input-tag for="User.Password" type="password" placeholder="Enter password" />
            
             <!-- Number input with icon -->
             <input-tag for="Product.Price" type="number" step="0.01" icon-prefix="DollarSign" />
            
             <!-- Search input with icon -->
             <input-tag for="Model.SearchTerm" type="search" icon-suffix="Search" placeholder="Search..." />
            
             <!-- In form-group wrapper for complete form control -->
             <form-group for="User.Email" label="Email Address" required="true" help-text="We'll never share your email">
               <input-tag for="User.Email" type="email" icon-prefix="AtSign" />
             </form-group>
```

### LabelTagHelper

```csharp
<!-- Simple label -->
             <label-tag for="User.Name" text="Full Name" />
            
             <!-- Required field with asterisk -->
             <label-tag for="User.Email" text="Email Address" required="true" />
            
             <!-- Optional field without asterisk -->
             <label-tag for="User.PhoneNumber" text="Phone Number" />
            
             <!-- In form-group wrapper -->
             <form-group for="User.Email" label="Email Address" required="true" help-text="We'll never share your email">
               <label-tag for="User.Email" text="Email Address" required="true" />
               <input-tag for="User.Email" type="email" />
             </form-group>
```

### RadioButtonTagHelper

```csharp
<!-- Single radio button -->
             <radio-button-tag for="Model.Status" group-name="Status" label="Active" value="active" />
            
             <!-- Radio button group (render multiple radio-button-tags with same group-name) -->
             <div>
               <radio-button-tag for="Model.PaymentMethod" group-name="PaymentMethod" label="Credit Card" value="cc" checked="true" />
               <radio-button-tag for="Model.PaymentMethod" group-name="PaymentMethod" label="PayPal" value="paypal" />
               <radio-button-tag for="Model.PaymentMethod" group-name="PaymentMethod" label="Bank Transfer" value="bank" />
             </div>
            
             <!-- Required radio button group -->
             <div>
               <radio-button-tag for="Model.Shipping" group-name="Shipping" label="Standard (5-7 days)" value="standard" checked="true" />
               <radio-button-tag for="Model.Shipping" group-name="Shipping" label="Express (2-3 days)" value="express" />
               <radio-button-tag for="Model.Shipping" group-name="Shipping" label="Overnight" value="overnight" />
             </div>
            
             <!-- In form-group wrapper -->
             <form-group for="Model.PaymentMethod" label="Payment Method" required="true">
               <radio-button-tag for="Model.PaymentMethod" group-name="PaymentMethod" label="Credit Card" value="cc" />
               <radio-button-tag for="Model.PaymentMethod" group-name="PaymentMethod" label="Debit Card" value="dc" />
             </form-group>
```

### SelectTagHelper

```csharp
<!-- Simple select with SelectListItem collection -->
             <select-tag for="User.CountryId" items="@countries" />
            
             <!-- Select with enum binding -->
             <select-tag for="Order.Status" enum-type="typeof(OrderStatus)" />
            
             <!-- Select without blank option -->
             <select-tag for="Product.Category" items="@categories" add-blank="false" />
            
             <!-- Select with custom blank text -->
             <select-tag for="User.Department" items="@departments" blank-text="Choose a department..." />
            
             <!-- With initial selection -->
             @{ var selected = categories.First(c => c.Value == "electronics"); selected.Selected = true; }
             <select-tag for="Product.Category" items="@categories" />
            
             <!-- In form-group wrapper -->
             <form-group for="User.CountryId" label="Country" required="true" help-text="Select your country">
               <select-tag for="User.CountryId" items="@countries" />
             </form-group>
```

### TextAreaTagHelper

```csharp
<!-- Simple textarea with default 3 rows -->
             <textarea-tag for="Model.Comments" placeholder="Enter your comments..." />
            
             <!-- Textarea with custom height -->
             <textarea-tag for="Model.Description" rows="6" placeholder="Enter detailed description" />
            
             <!-- Required textarea -->
             <textarea-tag for="Model.Feedback" placeholder="Your feedback is important" required="true" />
            
             <!-- Large textarea for longer content -->
             <textarea-tag for="Model.BioOrNotes" rows="10" placeholder="Tell us about yourself..." />
            
             <!-- In form-group wrapper for complete form control -->
             <form-group for="Model.Comments" label="Comments" required="true" help-text="Please provide at least 10 characters">
               <textarea-tag for="Model.Comments" rows="5" placeholder="Share your thoughts..." />
             </form-group>
```

### ValidationMessageTagHelper

```csharp
<!-- Validation message for a field (after input element) -->
             <input-tag for="User.Email" type="email" />
             <validation-message for="User.Email" />
            
             <!-- Validation message with custom message -->
             <input-tag for="User.Age" type="number" />
             <validation-message for="User.Age" message="Age must be between 18 and 100" />
            
             <!-- Typical usage in form-group -->
             <form-group for="User.Email" label="Email" required="true">
               <input-tag for="User.Email" type="email" />
               <validation-message for="User.Email" />
             </form-group>
            
             <!-- Multiple fields with validation -->
             <form-group for="User.Password" label="Password" required="true">
               <input-tag for="User.Password" type="password" />
               <validation-message for="User.Password" />
             </form-group>
             <form-group for="User.ConfirmPassword" label="Confirm Password" required="true">
               <input-tag for="User.ConfirmPassword" type="password" />
               <validation-message for="User.ConfirmPassword" />
             </form-group>
```

### GridTagHelper

```csharp
<!-- Basic grid with data binding -->
             <grid data-source="@Model.Products" data-page-size="20">
               <grid-column property="Name" sortable="true">Product Name</grid-column>
               <grid-column property="Price" sortable="true" format="currency">Price</grid-column>
             </grid>
            
             <!-- Grid with row selection enabled -->
             <grid data-source="@Model.Orders" data-page-size="50" data-allow-selection="true">
               <grid-column property="OrderId">Order ID</grid-column>
               <grid-column property="OrderDate" sortable="true" format="date">Date</grid-column>
               <grid-column property="Status" filterable="true">Status</grid-column>
             </grid>
            
             <!-- Grid with custom CSS class and export option -->
             <grid data-source="@Model.Customers"
                   data-page-size="25"
                   data-allow-export="true"
                   data-allow-column-toggle="true"
                   data-css-class="compact-grid">
               <grid-column property="FirstName">First Name</grid-column>
               <grid-column property="Email" sortable="true">Email</grid-column>
               <grid-column property="CreatedDate" sortable="true" format="date">Created</grid-column>
             </grid>
            
             <!-- Complete example with IDataContext<Product> binding -->
             @{
               var productDataContext = new DataContext<Product>(productService);
               await productDataContext.Initialize(await productService.GetProductsAsync());
             }
             <grid data-source="@productDataContext.Items"
                   data-page-size="20"
                   data-allow-selection="true">
               <grid-column property="Name" sortable="true">Product</grid-column>
               <grid-column property="Category" filterable="true">Category</grid-column>
               <grid-column property="Price" format="currency" css-class="text-end">Price</grid-column>
               <grid-column property="Stock" sortable="true">Stock</grid-column>
             </grid>
            
             <!-- Grid with filtering and sorting -->
             <div class="mb-3">
               <label for="statusFilter">Filter by Status:</label>
               <select id="statusFilter" onchange="updateFilter(this.value)">
                 <option value="">All</option>
                 <option value="Active">Active</option>
                 <option value="Inactive">Inactive</option>
               </select>
             </div>
            
             <grid data-source="@Model.Items" data-page-size="20">
               <grid-column property="Name" sortable="true">Name</grid-column>
               <grid-column property="Status" filterable="true">Status</grid-column>
               <grid-column property="CreatedDate" sortable="true" format="date">Created</grid-column>
             </grid>
```

### BreadcrumbItem

```csharp
<!-- Create breadcrumb items for product details page -->
             @{
               var breadcrumbs = new List<BreadcrumbItem>
               {
                 new() { Label = "Home", Url = "/" },
                 new() { Label = "Products", Url = "/products" },
                 new() { Label = "Electronics", Url = "/products/electronics" },
                 new() { Label = "Laptops", Url = "/products/electronics/laptops" },
                 new() { Label = "Dell XPS 13" }  // No URL - this is the current page
               };
             }
             <breadcrumb items="breadcrumbs" />
            
             <!-- Create from dynamic data -->
             @{
               var breadcrumbs = new List<BreadcrumbItem>
               {
                 new() { Label = "Dashboard", Url = "/dashboard" }
               };
            
               foreach (var folder in Model.FolderHierarchy)
               {
                 breadcrumbs.Add(new()
                 {
                   Label = folder.Name,
                   Url = folder.Depth < Model.FolderHierarchy.Count - 1 ? folder.Url : null
                 });
               }
             }
             <breadcrumb items="breadcrumbs" />
```

### BreadcrumbTagHelper

```csharp
<!-- Basic breadcrumb navigation -->
             <breadcrumb items="new List<BreadcrumbItem> {
               new() { Label = "Home", Url = "/" },
               new() { Label = "Products", Url = "/products" },
               new() { Label = "Electronics" }
             }" />
            
             <!-- Breadcrumb from controller action -->
             <breadcrumb items="Model.Breadcrumbs" />
            
             <!-- Programmatically constructed breadcrumb in view -->
             @{
               var breadcrumbs = new List<BreadcrumbItem>
               {
                 new() { Label = "Dashboard", Url = "/dashboard" },
                 new() { Label = "Reports", Url = "/reports" },
                 new() { Label = "Monthly Summary" }
               };
             }
             <breadcrumb items="breadcrumbs" />
```

### IfAuthorizedTagHelper

```csharp
<!-- Simple logout button, shown only if authenticated -->
             <if-authorized>
               <button class="btn btn-danger">Logout</button>
             </if-authorized>
            
             <!-- User profile section in header -->
             <if-authorized>
               <div class="user-profile">
                 <span>Welcome, @Model.User.Name!</span>
                 <a href="/profile">View Profile</a>
               </div>
             </if-authorized>
            
             <!-- Multiple buttons and links for authenticated users -->
             <if-authorized>
               <div class="authenticated-menu">
                 <a href="/dashboard" class="btn btn-primary">Dashboard</a>
                 <a href="/orders" class="btn btn-info">My Orders</a>
                 <button onclick="logout()" class="btn btn-danger">Logout</button>
               </div>
             </if-authorized>
            
             <!-- Unauthenticated content shown separately -->
             <div>
               <if-authorized>
                 <p>You are logged in.</p>
               </if-authorized>
               <!-- Note: For not-authenticated content, use negation in IfAuthorizedTagHelper
                    or implement a separate IfNotAuthorizedTagHelper -->
             </div>
```

### IfClaimTagHelper

```csharp
<!-- Simple claim check: show if user has "role" claim with value "Admin" -->
             <if-claim type="role" value="Admin">
               <button class="btn btn-danger">Delete User</button>
             </if-claim>
            
             <!-- Multiple values (OR logic): show if user is Admin or Manager -->
             <if-claim type="role" value="Admin,Manager">
               <a href="/admin" class="btn btn-primary">Admin Panel</a>
             </if-claim>
            
             <!-- Department-based access: show if in IT or Finance department -->
             <if-claim type="department" value="IT,Finance">
               <div class="reports-section">
                 <h3>Financial Reports</h3>
                 <a href="/reports/finance">View Reports</a>
               </div>
             </if-claim>
            
             <!-- Subscription level: show premium features if subscription is Premium or Enterprise -->
             <if-claim type="subscription_level" value="Premium,Enterprise">
               <div class="premium-features">
                 <h3>Advanced Analytics</h3>
                 <p>You have access to advanced reporting features.</p>
               </div>
             </if-claim>
            
             <!-- Claim existence without value check: show if user has ANY custom-permission claim -->
             <if-claim type="custom-permission">
               <p>You have special permissions.</p>
             </if-claim>
            
             <!-- Combined with other markup: admin toolbar with multiple restrictions -->
             <div class="admin-toolbar">
               <if-claim type="role" value="Admin">
                 <button class="btn btn-danger" onclick="deleteAll()">Delete All</button>
               </if-claim>
               <if-claim type="role" value="Admin,Moderator">
                 <button class="btn btn-warning" onclick="moderate()">Moderate</button>
               </if-claim>
             </div>
```

### IfRoleTagHelper

```csharp
<!-- Simple role check: show delete button if user is Admin -->
             <if-role role="Admin">
               <button class="btn btn-danger" onclick="deleteItem()">Delete</button>
             </if-role>
            
             <!-- Multiple roles (OR logic): show if user is Admin or Manager -->
             <if-role role="Admin,Manager">
               <a href="/admin" class="btn btn-primary">Admin Panel</a>
             </if-role>
            
             <!-- Editor dashboard with edit/delete options -->
             <if-role role="Editor">
               <div class="editor-toolbar">
                 <button class="btn btn-info" onclick="editItem()">Edit</button>
                 <button class="btn btn-warning" onclick="publishItem()">Publish</button>
               </div>
             </if-role>
            
             <!-- Multiple role sections with different features -->
             <div class="dashboard">
               <if-role role="Admin">
                 <div class="admin-section">
                   <h3>System Administration</h3>
                   <a href="/admin/users">Manage Users</a>
                   <a href="/admin/settings">System Settings</a>
                 </div>
               </if-role>
               <if-role role="Manager,Editor">
                 <div class="content-section">
                   <h3>Content Management</h3>
                   <a href="/content">View Content</a>
                 </div>
               </if-role>
             </div>
            
             <!-- Nested role checks in a complex menu -->
             <nav class="sidebar">
               <ul>
                 <li><a href="/home">Home</a></li>
                 <if-role role="User,Admin,Manager">
                   <li><a href="/dashboard">Dashboard</a></li>
                   <li><a href="/profile">My Profile</a></li>
                 </if-role>
                 <if-role role="Admin">
                   <li><a href="/admin">Administration</a></li>
                 </if-role>
               </ul>
             </nav>
            
             <!-- Contributor role with limited permissions -->
             <if-role role="Contributor,Editor,Admin">
               <div class="publish-section">
                 <button class="btn btn-success" onclick="submit()">Submit for Review</button>
               </div>
             </if-role>
```

### DataContext`1

```csharp
// Initialize with data
             var context = new DataContext<Product>();
             await context.Initialize(products);
            
             // Subscribe to state changes
             context.OnStateChanged += () => Console.WriteLine("State changed!");
            
             // Filter by category - async operation
             await context.UpdateFilter("Category", "equals", "Electronics");
             // CurrentRequest.Filters now contains { "Category": "Electronics" }
             // CurrentResponse.Data reloaded asynchronously
            
             // Change sort - resets to page 1
             await context.UpdateSort("Price", isDescending: true);
            
             // Change pagination
             await context.UpdatePagination(pageNumber: 2, pageSize: 50);
            
             // Row selection - synchronous operations
             context.ToggleRowSelection(1);          // Select row with ID=1
             context.ToggleSelectAll(true);          // Select all visible rows
             var selected = context.SelectedRowIds;  // List<object> containing IDs
            
             // Check loading state
             if (context.IsLoading)
             {
                 // Show spinner
             }
            
             // Handle errors
             if (context.Error != null)
             {
                 Console.WriteLine($"Error: {context.Error}");
             }
```

### ButtonTagHelper

```csharp
<!-- Primary button (default submit button) -->
             <button type="submit" variant="primary">Submit</button>
             <!-- Generates: <button class="btn btn-primary" type="submit">Submit</button> -->
            
             <!-- Small secondary button -->
             <button variant="secondary" size="sm">Cancel</button>
             <!-- Generates: <button class="btn btn-secondary btn-sm">Cancel</button> -->
            
             <!-- Large danger button -->
             <button variant="danger" size="lg">Delete</button>
             <!-- Generates: <button class="btn btn-danger btn-lg" disabled="disabled">Delete</button> -->
            
             <!-- Success button with loading state -->
             <button variant="success" is-loading="true">Processing...</button>
             <!-- Generates: <button class="btn btn-success disabled" disabled="disabled">Processing...</button> -->
            
             <!-- Link styled as a button -->
             <a href="/dashboard" variant="info">Go to Dashboard</a>
             <!-- Generates: <a class="btn btn-info" href="/dashboard">Go to Dashboard</a> -->
            
             <!-- Warning button with custom CSS class -->
             <button variant="warning" class="mt-2">Warning Action</button>
             <!-- Generates: <button class="mt-2 btn btn-warning">Warning Action</button> -->
```

### IconTagHelper

```csharp
<!-- Standalone success icon -->
             <icon name="Success" />
             <!-- Generates: <i class="bi bi-check-circle-fill"></i> -->
            
             <!-- Small icon with custom CSS class -->
             <icon name="Info" size="sm" css-class="text-info" />
             <!-- Generates: <i class="bi bi-info-circle me-1 text-info"></i> -->
            
             <!-- Large error icon in red -->
             <icon name="Error" size="lg" css-class="text-danger" />
             <!-- Generates: <i class="bi bi-exclamation-circle fs-5 text-danger"></i> -->
            
             <!-- Search icon in a button -->
             <button type="button" class="btn btn-primary">
               <icon name="Search" size="sm" /> Search
             </button>
            
             <!-- Warning icon with emphasis -->
             <icon name="Warning" size="lg" css-class="text-warning me-2" />
             <span>Please verify your information</span>
            
             <!-- Home navigation icon -->
             <a href="/"><icon name="Home" /> Home</a>
            
             <!-- User account icon -->
             <a href="/settings"><icon name="User" size="sm" /> Settings</a>
```

### AlertTagHelper

```csharp
<!-- Simple success alert -->
             <alert type="success" message="Profile updated successfully!" />
            
             <!-- Non-dismissible danger alert -->
             <alert type="danger" message="An error occurred while saving." dismissible="false" />
            
             <!-- Warning alert with dismiss button -->
             <alert type="warning" message="This action cannot be undone." />
            
             <!-- Default info alert -->
             <alert message="Remember to save your changes regularly." />
```

### BadgeTagHelper

```csharp
<!-- Simple primary badge -->
             <badge type="primary" text="New" />
            
             <!-- Count badge -->
             <badge type="success" text="5 items" />
            
             <!-- Danger badge for inactive status -->
             <badge type="danger" text="Inactive" />
            
             <!-- Warning badge -->
             <badge type="warning" text="Pending Review" />
            
             <!-- Default secondary badge -->
             <badge text="Badge" />
            
             <!-- Pill-shaped badge -->
             <span class="badge bg-primary rounded-pill">@notificationCount</span>
```

### PaginationTagHelper

```csharp
<!-- Basic pagination with defaults (shows 5 pages max) -->
             <pagination current-page="2" total-pages="10" />
            
             <!-- Pagination with custom page URL pattern -->
             <pagination current-page="1" total-pages="5" page-url="/products?page={0}" />
            
             <!-- Pagination with more visible pages -->
             <pagination current-page="3" total-pages="15" max-visible="7" />
            
             <!-- Pagination on last page (Next is disabled) -->
             <pagination current-page="10" total-pages="10" />
            
             <!-- Pagination with single page (not rendered) -->
             <pagination current-page="1" total-pages="1" />
```

### CheckboxTagHelper

```csharp
<!-- Simple checkbox with label -->
             <checkbox-tag for="User.IsSubscribed" label="Subscribe to newsletter" />
            
             <!-- Checkbox with custom value -->
             <checkbox-tag for="User.AgreedToTerms" label="I agree to the terms" value="1" />
            
             <!-- Pre-checked checkbox -->
             <checkbox-tag for="User.IsActive" label="Active" checked="true" />
            
             <!-- In form-group wrapper for complete form control -->
             <form-group for="User.IsSubscribed" label="Newsletter Subscription" help-text="Get updates via email">
               <checkbox-tag for="User.IsSubscribed" label="Subscribe" />
             </form-group>
```

### FileInputTagHelper

```csharp
<!-- Simple file input -->
             <file-input-tag for="Model.ProfilePhoto" />
            
             <!-- File input accepting specific types -->
             <file-input-tag for="Model.Document" accept=".pdf,.docx" required="true" />
            
             <!-- Multiple file upload -->
             <file-input-tag for="Model.Attachments" accept="image/*" multiple="true" />
            
             <!-- In form-group wrapper -->
             <form-group for="Model.ProfilePhoto" label="Upload Photo" required="true" help-text="JPG or PNG, max 5MB">
               <file-input-tag for="Model.ProfilePhoto" accept="image/jpeg,image/png" />
             </form-group>
```

### FormGroupTagHelper

```csharp
<!-- Simple form-group with text input -->
             <form-group for="Model.Name" label="Full Name" required="true">
               <input-tag for="Model.Name" placeholder="Enter your full name" />
             </form-group>
            
             <!-- Form-group with help text -->
             <form-group for="Model.Email" label="Email Address" required="true" help-text="We'll never share your email">
               <input-tag for="Model.Email" type="email" placeholder="you@example.com" />
             </form-group>
            
             <!-- Form-group with select control -->
             <form-group for="Model.CountryId" label="Country" required="true" help-text="Select your country of residence">
               <select-tag for="Model.CountryId" items="@countries" />
             </form-group>
            
             <!-- Form-group with textarea -->
             <form-group for="Model.Comments" label="Comments" help-text="Optional feedback (max 500 characters)">
               <textarea-tag for="Model.Comments" rows="4" placeholder="Share your thoughts..." />
             </form-group>
            
             <!-- Form-group with checkbox -->
             <form-group for="Model.IsSubscribed" label="Newsletter">
               <checkbox-tag for="Model.IsSubscribed" label="Subscribe to newsletter" />
             </form-group>
            
             <!-- Form-group with validation error state -->
             <!-- When ModelState contains error for Model.Age, child controls show .is-invalid state -->
             <form-group for="Model.Age" label="Age" required="true" help-text="Must be 18 or older">
               <input-tag for="Model.Age" type="number" placeholder="Enter your age" />
             </form-group>
```

### FormTagHelper

```csharp
<!-- Simple form with default POST method -->
             <form-tag>
               <form-group for="Model.Name" label="Full Name" required="true">
                 <input-tag for="Model.Name" placeholder="Enter your name" />
               </form-group>
               <button type="submit" class="btn btn-primary">Submit</button>
             </form-tag>
            
             <!-- Form with custom action and GET method -->
             <form-tag method="get" action="/search">
               <input-tag for="Model.SearchTerm" placeholder="Search..." />
               <button type="submit" class="btn btn-primary">Search</button>
             </form-tag>
            
             <!-- Form with custom CSS class and validation disabled -->
             <form-tag class="login-form" novalidate="true">
               <form-group for="Model.Email" label="Email" required="true">
                 <input-tag for="Model.Email" type="email" />
               </form-group>
               <button type="submit" class="btn btn-primary">Login</button>
             </form-tag>
```

### InputTagHelper

```csharp
<!-- Simple text input -->
             <input-tag for="User.Name" placeholder="Enter your name" />
            
             <!-- Email input with validation -->
             <input-tag for="User.Email" type="email" placeholder="Enter email" required="true" />
            
             <!-- Password input -->
             <input-tag for="User.Password" type="password" placeholder="Enter password" />
            
             <!-- Number input with icon -->
             <input-tag for="Product.Price" type="number" step="0.01" icon-prefix="DollarSign" />
            
             <!-- Search input with icon -->
             <input-tag for="Model.SearchTerm" type="search" icon-suffix="Search" placeholder="Search..." />
            
             <!-- In form-group wrapper for complete form control -->
             <form-group for="User.Email" label="Email Address" required="true" help-text="We'll never share your email">
               <input-tag for="User.Email" type="email" icon-prefix="AtSign" />
             </form-group>
```

### LabelTagHelper

```csharp
<!-- Simple label -->
             <label-tag for="User.Name" text="Full Name" />
            
             <!-- Required field with asterisk -->
             <label-tag for="User.Email" text="Email Address" required="true" />
            
             <!-- Optional field without asterisk -->
             <label-tag for="User.PhoneNumber" text="Phone Number" />
            
             <!-- In form-group wrapper -->
             <form-group for="User.Email" label="Email Address" required="true" help-text="We'll never share your email">
               <label-tag for="User.Email" text="Email Address" required="true" />
               <input-tag for="User.Email" type="email" />
             </form-group>
```

### RadioButtonTagHelper

```csharp
<!-- Single radio button -->
             <radio-button-tag for="Model.Status" group-name="Status" label="Active" value="active" />
            
             <!-- Radio button group (render multiple radio-button-tags with same group-name) -->
             <div>
               <radio-button-tag for="Model.PaymentMethod" group-name="PaymentMethod" label="Credit Card" value="cc" checked="true" />
               <radio-button-tag for="Model.PaymentMethod" group-name="PaymentMethod" label="PayPal" value="paypal" />
               <radio-button-tag for="Model.PaymentMethod" group-name="PaymentMethod" label="Bank Transfer" value="bank" />
             </div>
            
             <!-- Required radio button group -->
             <div>
               <radio-button-tag for="Model.Shipping" group-name="Shipping" label="Standard (5-7 days)" value="standard" checked="true" />
               <radio-button-tag for="Model.Shipping" group-name="Shipping" label="Express (2-3 days)" value="express" />
               <radio-button-tag for="Model.Shipping" group-name="Shipping" label="Overnight" value="overnight" />
             </div>
            
             <!-- In form-group wrapper -->
             <form-group for="Model.PaymentMethod" label="Payment Method" required="true">
               <radio-button-tag for="Model.PaymentMethod" group-name="PaymentMethod" label="Credit Card" value="cc" />
               <radio-button-tag for="Model.PaymentMethod" group-name="PaymentMethod" label="Debit Card" value="dc" />
             </form-group>
```

### SelectTagHelper

```csharp
<!-- Simple select with SelectListItem collection -->
             <select-tag for="User.CountryId" items="@countries" />
            
             <!-- Select with enum binding -->
             <select-tag for="Order.Status" enum-type="typeof(OrderStatus)" />
            
             <!-- Select without blank option -->
             <select-tag for="Product.Category" items="@categories" add-blank="false" />
            
             <!-- Select with custom blank text -->
             <select-tag for="User.Department" items="@departments" blank-text="Choose a department..." />
            
             <!-- With initial selection -->
             @{ var selected = categories.First(c => c.Value == "electronics"); selected.Selected = true; }
             <select-tag for="Product.Category" items="@categories" />
            
             <!-- In form-group wrapper -->
             <form-group for="User.CountryId" label="Country" required="true" help-text="Select your country">
               <select-tag for="User.CountryId" items="@countries" />
             </form-group>
```

### TextAreaTagHelper

```csharp
<!-- Simple textarea with default 3 rows -->
             <textarea-tag for="Model.Comments" placeholder="Enter your comments..." />
            
             <!-- Textarea with custom height -->
             <textarea-tag for="Model.Description" rows="6" placeholder="Enter detailed description" />
            
             <!-- Required textarea -->
             <textarea-tag for="Model.Feedback" placeholder="Your feedback is important" required="true" />
            
             <!-- Large textarea for longer content -->
             <textarea-tag for="Model.BioOrNotes" rows="10" placeholder="Tell us about yourself..." />
            
             <!-- In form-group wrapper for complete form control -->
             <form-group for="Model.Comments" label="Comments" required="true" help-text="Please provide at least 10 characters">
               <textarea-tag for="Model.Comments" rows="5" placeholder="Share your thoughts..." />
             </form-group>
```

### ValidationMessageTagHelper

```csharp
<!-- Validation message for a field (after input element) -->
             <input-tag for="User.Email" type="email" />
             <validation-message for="User.Email" />
            
             <!-- Validation message with custom message -->
             <input-tag for="User.Age" type="number" />
             <validation-message for="User.Age" message="Age must be between 18 and 100" />
            
             <!-- Typical usage in form-group -->
             <form-group for="User.Email" label="Email" required="true">
               <input-tag for="User.Email" type="email" />
               <validation-message for="User.Email" />
             </form-group>
            
             <!-- Multiple fields with validation -->
             <form-group for="User.Password" label="Password" required="true">
               <input-tag for="User.Password" type="password" />
               <validation-message for="User.Password" />
             </form-group>
             <form-group for="User.ConfirmPassword" label="Confirm Password" required="true">
               <input-tag for="User.ConfirmPassword" type="password" />
               <validation-message for="User.ConfirmPassword" />
             </form-group>
```

### GridTagHelper

```csharp
<!-- Basic grid with data binding -->
             <grid data-source="@Model.Products" data-page-size="20">
               <grid-column property="Name" sortable="true">Product Name</grid-column>
               <grid-column property="Price" sortable="true" format="currency">Price</grid-column>
             </grid>
            
             <!-- Grid with row selection enabled -->
             <grid data-source="@Model.Orders" data-page-size="50" data-allow-selection="true">
               <grid-column property="OrderId">Order ID</grid-column>
               <grid-column property="OrderDate" sortable="true" format="date">Date</grid-column>
               <grid-column property="Status" filterable="true">Status</grid-column>
             </grid>
            
             <!-- Grid with custom CSS class and export option -->
             <grid data-source="@Model.Customers"
                   data-page-size="25"
                   data-allow-export="true"
                   data-allow-column-toggle="true"
                   data-css-class="compact-grid">
               <grid-column property="FirstName">First Name</grid-column>
               <grid-column property="Email" sortable="true">Email</grid-column>
               <grid-column property="CreatedDate" sortable="true" format="date">Created</grid-column>
             </grid>
            
             <!-- Complete example with IDataContext<Product> binding -->
             @{
               var productDataContext = new DataContext<Product>(productService);
               await productDataContext.Initialize(await productService.GetProductsAsync());
             }
             <grid data-source="@productDataContext.Items"
                   data-page-size="20"
                   data-allow-selection="true">
               <grid-column property="Name" sortable="true">Product</grid-column>
               <grid-column property="Category" filterable="true">Category</grid-column>
               <grid-column property="Price" format="currency" css-class="text-end">Price</grid-column>
               <grid-column property="Stock" sortable="true">Stock</grid-column>
             </grid>
            
             <!-- Grid with filtering and sorting -->
             <div class="mb-3">
               <label for="statusFilter">Filter by Status:</label>
               <select id="statusFilter" onchange="updateFilter(this.value)">
                 <option value="">All</option>
                 <option value="Active">Active</option>
                 <option value="Inactive">Inactive</option>
               </select>
             </div>
            
             <grid data-source="@Model.Items" data-page-size="20">
               <grid-column property="Name" sortable="true">Name</grid-column>
               <grid-column property="Status" filterable="true">Status</grid-column>
               <grid-column property="CreatedDate" sortable="true" format="date">Created</grid-column>
             </grid>
```

### BreadcrumbItem

```csharp
<!-- Create breadcrumb items for product details page -->
             @{
               var breadcrumbs = new List<BreadcrumbItem>
               {
                 new() { Label = "Home", Url = "/" },
                 new() { Label = "Products", Url = "/products" },
                 new() { Label = "Electronics", Url = "/products/electronics" },
                 new() { Label = "Laptops", Url = "/products/electronics/laptops" },
                 new() { Label = "Dell XPS 13" }  // No URL - this is the current page
               };
             }
             <breadcrumb items="breadcrumbs" />
            
             <!-- Create from dynamic data -->
             @{
               var breadcrumbs = new List<BreadcrumbItem>
               {
                 new() { Label = "Dashboard", Url = "/dashboard" }
               };
            
               foreach (var folder in Model.FolderHierarchy)
               {
                 breadcrumbs.Add(new()
                 {
                   Label = folder.Name,
                   Url = folder.Depth < Model.FolderHierarchy.Count - 1 ? folder.Url : null
                 });
               }
             }
             <breadcrumb items="breadcrumbs" />
```

### BreadcrumbTagHelper

```csharp
<!-- Basic breadcrumb navigation -->
             <breadcrumb items="new List<BreadcrumbItem> {
               new() { Label = "Home", Url = "/" },
               new() { Label = "Products", Url = "/products" },
               new() { Label = "Electronics" }
             }" />
            
             <!-- Breadcrumb from controller action -->
             <breadcrumb items="Model.Breadcrumbs" />
            
             <!-- Programmatically constructed breadcrumb in view -->
             @{
               var breadcrumbs = new List<BreadcrumbItem>
               {
                 new() { Label = "Dashboard", Url = "/dashboard" },
                 new() { Label = "Reports", Url = "/reports" },
                 new() { Label = "Monthly Summary" }
               };
             }
             <breadcrumb items="breadcrumbs" />
```

### IfAuthorizedTagHelper

```csharp
<!-- Simple logout button, shown only if authenticated -->
             <if-authorized>
               <button class="btn btn-danger">Logout</button>
             </if-authorized>
            
             <!-- User profile section in header -->
             <if-authorized>
               <div class="user-profile">
                 <span>Welcome, @Model.User.Name!</span>
                 <a href="/profile">View Profile</a>
               </div>
             </if-authorized>
            
             <!-- Multiple buttons and links for authenticated users -->
             <if-authorized>
               <div class="authenticated-menu">
                 <a href="/dashboard" class="btn btn-primary">Dashboard</a>
                 <a href="/orders" class="btn btn-info">My Orders</a>
                 <button onclick="logout()" class="btn btn-danger">Logout</button>
               </div>
             </if-authorized>
            
             <!-- Unauthenticated content shown separately -->
             <div>
               <if-authorized>
                 <p>You are logged in.</p>
               </if-authorized>
               <!-- Note: For not-authenticated content, use negation in IfAuthorizedTagHelper
                    or implement a separate IfNotAuthorizedTagHelper -->
             </div>
```

### IfClaimTagHelper

```csharp
<!-- Simple claim check: show if user has "role" claim with value "Admin" -->
             <if-claim type="role" value="Admin">
               <button class="btn btn-danger">Delete User</button>
             </if-claim>
            
             <!-- Multiple values (OR logic): show if user is Admin or Manager -->
             <if-claim type="role" value="Admin,Manager">
               <a href="/admin" class="btn btn-primary">Admin Panel</a>
             </if-claim>
            
             <!-- Department-based access: show if in IT or Finance department -->
             <if-claim type="department" value="IT,Finance">
               <div class="reports-section">
                 <h3>Financial Reports</h3>
                 <a href="/reports/finance">View Reports</a>
               </div>
             </if-claim>
            
             <!-- Subscription level: show premium features if subscription is Premium or Enterprise -->
             <if-claim type="subscription_level" value="Premium,Enterprise">
               <div class="premium-features">
                 <h3>Advanced Analytics</h3>
                 <p>You have access to advanced reporting features.</p>
               </div>
             </if-claim>
            
             <!-- Claim existence without value check: show if user has ANY custom-permission claim -->
             <if-claim type="custom-permission">
               <p>You have special permissions.</p>
             </if-claim>
            
             <!-- Combined with other markup: admin toolbar with multiple restrictions -->
             <div class="admin-toolbar">
               <if-claim type="role" value="Admin">
                 <button class="btn btn-danger" onclick="deleteAll()">Delete All</button>
               </if-claim>
               <if-claim type="role" value="Admin,Moderator">
                 <button class="btn btn-warning" onclick="moderate()">Moderate</button>
               </if-claim>
             </div>
```

### IfRoleTagHelper

```csharp
<!-- Simple role check: show delete button if user is Admin -->
             <if-role role="Admin">
               <button class="btn btn-danger" onclick="deleteItem()">Delete</button>
             </if-role>
            
             <!-- Multiple roles (OR logic): show if user is Admin or Manager -->
             <if-role role="Admin,Manager">
               <a href="/admin" class="btn btn-primary">Admin Panel</a>
             </if-role>
            
             <!-- Editor dashboard with edit/delete options -->
             <if-role role="Editor">
               <div class="editor-toolbar">
                 <button class="btn btn-info" onclick="editItem()">Edit</button>
                 <button class="btn btn-warning" onclick="publishItem()">Publish</button>
               </div>
             </if-role>
            
             <!-- Multiple role sections with different features -->
             <div class="dashboard">
               <if-role role="Admin">
                 <div class="admin-section">
                   <h3>System Administration</h3>
                   <a href="/admin/users">Manage Users</a>
                   <a href="/admin/settings">System Settings</a>
                 </div>
               </if-role>
               <if-role role="Manager,Editor">
                 <div class="content-section">
                   <h3>Content Management</h3>
                   <a href="/content">View Content</a>
                 </div>
               </if-role>
             </div>
            
             <!-- Nested role checks in a complex menu -->
             <nav class="sidebar">
               <ul>
                 <li><a href="/home">Home</a></li>
                 <if-role role="User,Admin,Manager">
                   <li><a href="/dashboard">Dashboard</a></li>
                   <li><a href="/profile">My Profile</a></li>
                 </if-role>
                 <if-role role="Admin">
                   <li><a href="/admin">Administration</a></li>
                 </if-role>
               </ul>
             </nav>
            
             <!-- Contributor role with limited permissions -->
             <if-role role="Contributor,Editor,Admin">
               <div class="publish-section">
                 <button class="btn btn-success" onclick="submit()">Submit for Review</button>
               </div>
             </if-role>
```

### DataContext`1

```csharp
// Initialize with data
             var context = new DataContext<Product>();
             await context.Initialize(products);
            
             // Subscribe to state changes
             context.OnStateChanged += () => Console.WriteLine("State changed!");
            
             // Filter by category - async operation
             await context.UpdateFilter("Category", "equals", "Electronics");
             // CurrentRequest.Filters now contains { "Category": "Electronics" }
             // CurrentResponse.Data reloaded asynchronously
            
             // Change sort - resets to page 1
             await context.UpdateSort("Price", isDescending: true);
            
             // Change pagination
             await context.UpdatePagination(pageNumber: 2, pageSize: 50);
            
             // Row selection - synchronous operations
             context.ToggleRowSelection(1);          // Select row with ID=1
             context.ToggleSelectAll(true);          // Select all visible rows
             var selected = context.SelectedRowIds;  // List<object> containing IDs
            
             // Check loading state
             if (context.IsLoading)
             {
                 // Show spinner
             }
            
             // Handle errors
             if (context.Error != null)
             {
                 Console.WriteLine($"Error: {context.Error}");
             }
```

### ButtonTagHelper

```csharp
<!-- Primary button (default submit button) -->
             <button type="submit" variant="primary">Submit</button>
             <!-- Generates: <button class="btn btn-primary" type="submit">Submit</button> -->
            
             <!-- Small secondary button -->
             <button variant="secondary" size="sm">Cancel</button>
             <!-- Generates: <button class="btn btn-secondary btn-sm">Cancel</button> -->
            
             <!-- Large danger button -->
             <button variant="danger" size="lg">Delete</button>
             <!-- Generates: <button class="btn btn-danger btn-lg" disabled="disabled">Delete</button> -->
            
             <!-- Success button with loading state -->
             <button variant="success" is-loading="true">Processing...</button>
             <!-- Generates: <button class="btn btn-success disabled" disabled="disabled">Processing...</button> -->
            
             <!-- Link styled as a button -->
             <a href="/dashboard" variant="info">Go to Dashboard</a>
             <!-- Generates: <a class="btn btn-info" href="/dashboard">Go to Dashboard</a> -->
            
             <!-- Warning button with custom CSS class -->
             <button variant="warning" class="mt-2">Warning Action</button>
             <!-- Generates: <button class="mt-2 btn btn-warning">Warning Action</button> -->
```

### IconTagHelper

```csharp
<!-- Standalone success icon -->
             <icon name="Success" />
             <!-- Generates: <i class="bi bi-check-circle-fill"></i> -->
            
             <!-- Small icon with custom CSS class -->
             <icon name="Info" size="sm" css-class="text-info" />
             <!-- Generates: <i class="bi bi-info-circle me-1 text-info"></i> -->
            
             <!-- Large error icon in red -->
             <icon name="Error" size="lg" css-class="text-danger" />
             <!-- Generates: <i class="bi bi-exclamation-circle fs-5 text-danger"></i> -->
            
             <!-- Search icon in a button -->
             <button type="button" class="btn btn-primary">
               <icon name="Search" size="sm" /> Search
             </button>
            
             <!-- Warning icon with emphasis -->
             <icon name="Warning" size="lg" css-class="text-warning me-2" />
             <span>Please verify your information</span>
            
             <!-- Home navigation icon -->
             <a href="/"><icon name="Home" /> Home</a>
            
             <!-- User account icon -->
             <a href="/settings"><icon name="User" size="sm" /> Settings</a>
```

### AlertTagHelper

```csharp
<!-- Simple success alert -->
             <alert type="success" message="Profile updated successfully!" />
            
             <!-- Non-dismissible danger alert -->
             <alert type="danger" message="An error occurred while saving." dismissible="false" />
            
             <!-- Warning alert with dismiss button -->
             <alert type="warning" message="This action cannot be undone." />
            
             <!-- Default info alert -->
             <alert message="Remember to save your changes regularly." />
```

### BadgeTagHelper

```csharp
<!-- Simple primary badge -->
             <badge type="primary" text="New" />
            
             <!-- Count badge -->
             <badge type="success" text="5 items" />
            
             <!-- Danger badge for inactive status -->
             <badge type="danger" text="Inactive" />
            
             <!-- Warning badge -->
             <badge type="warning" text="Pending Review" />
            
             <!-- Default secondary badge -->
             <badge text="Badge" />
            
             <!-- Pill-shaped badge -->
             <span class="badge bg-primary rounded-pill">@notificationCount</span>
```

### PaginationTagHelper

```csharp
<!-- Basic pagination with defaults (shows 5 pages max) -->
             <pagination current-page="2" total-pages="10" />
            
             <!-- Pagination with custom page URL pattern -->
             <pagination current-page="1" total-pages="5" page-url="/products?page={0}" />
            
             <!-- Pagination with more visible pages -->
             <pagination current-page="3" total-pages="15" max-visible="7" />
            
             <!-- Pagination on last page (Next is disabled) -->
             <pagination current-page="10" total-pages="10" />
            
             <!-- Pagination with single page (not rendered) -->
             <pagination current-page="1" total-pages="1" />
```

### CheckboxTagHelper

```csharp
<!-- Simple checkbox with label -->
             <checkbox-tag for="User.IsSubscribed" label="Subscribe to newsletter" />
            
             <!-- Checkbox with custom value -->
             <checkbox-tag for="User.AgreedToTerms" label="I agree to the terms" value="1" />
            
             <!-- Pre-checked checkbox -->
             <checkbox-tag for="User.IsActive" label="Active" checked="true" />
            
             <!-- In form-group wrapper for complete form control -->
             <form-group for="User.IsSubscribed" label="Newsletter Subscription" help-text="Get updates via email">
               <checkbox-tag for="User.IsSubscribed" label="Subscribe" />
             </form-group>
```

### FileInputTagHelper

```csharp
<!-- Simple file input -->
             <file-input-tag for="Model.ProfilePhoto" />
            
             <!-- File input accepting specific types -->
             <file-input-tag for="Model.Document" accept=".pdf,.docx" required="true" />
            
             <!-- Multiple file upload -->
             <file-input-tag for="Model.Attachments" accept="image/*" multiple="true" />
            
             <!-- In form-group wrapper -->
             <form-group for="Model.ProfilePhoto" label="Upload Photo" required="true" help-text="JPG or PNG, max 5MB">
               <file-input-tag for="Model.ProfilePhoto" accept="image/jpeg,image/png" />
             </form-group>
```

### FormGroupTagHelper

```csharp
<!-- Simple form-group with text input -->
             <form-group for="Model.Name" label="Full Name" required="true">
               <input-tag for="Model.Name" placeholder="Enter your full name" />
             </form-group>
            
             <!-- Form-group with help text -->
             <form-group for="Model.Email" label="Email Address" required="true" help-text="We'll never share your email">
               <input-tag for="Model.Email" type="email" placeholder="you@example.com" />
             </form-group>
            
             <!-- Form-group with select control -->
             <form-group for="Model.CountryId" label="Country" required="true" help-text="Select your country of residence">
               <select-tag for="Model.CountryId" items="@countries" />
             </form-group>
            
             <!-- Form-group with textarea -->
             <form-group for="Model.Comments" label="Comments" help-text="Optional feedback (max 500 characters)">
               <textarea-tag for="Model.Comments" rows="4" placeholder="Share your thoughts..." />
             </form-group>
            
             <!-- Form-group with checkbox -->
             <form-group for="Model.IsSubscribed" label="Newsletter">
               <checkbox-tag for="Model.IsSubscribed" label="Subscribe to newsletter" />
             </form-group>
            
             <!-- Form-group with validation error state -->
             <!-- When ModelState contains error for Model.Age, child controls show .is-invalid state -->
             <form-group for="Model.Age" label="Age" required="true" help-text="Must be 18 or older">
               <input-tag for="Model.Age" type="number" placeholder="Enter your age" />
             </form-group>
```

### FormTagHelper

```csharp
<!-- Simple form with default POST method -->
             <form-tag>
               <form-group for="Model.Name" label="Full Name" required="true">
                 <input-tag for="Model.Name" placeholder="Enter your name" />
               </form-group>
               <button type="submit" class="btn btn-primary">Submit</button>
             </form-tag>
            
             <!-- Form with custom action and GET method -->
             <form-tag method="get" action="/search">
               <input-tag for="Model.SearchTerm" placeholder="Search..." />
               <button type="submit" class="btn btn-primary">Search</button>
             </form-tag>
            
             <!-- Form with custom CSS class and validation disabled -->
             <form-tag class="login-form" novalidate="true">
               <form-group for="Model.Email" label="Email" required="true">
                 <input-tag for="Model.Email" type="email" />
               </form-group>
               <button type="submit" class="btn btn-primary">Login</button>
             </form-tag>
```

### InputTagHelper

```csharp
<!-- Simple text input -->
             <input-tag for="User.Name" placeholder="Enter your name" />
            
             <!-- Email input with validation -->
             <input-tag for="User.Email" type="email" placeholder="Enter email" required="true" />
            
             <!-- Password input -->
             <input-tag for="User.Password" type="password" placeholder="Enter password" />
            
             <!-- Number input with icon -->
             <input-tag for="Product.Price" type="number" step="0.01" icon-prefix="DollarSign" />
            
             <!-- Search input with icon -->
             <input-tag for="Model.SearchTerm" type="search" icon-suffix="Search" placeholder="Search..." />
            
             <!-- In form-group wrapper for complete form control -->
             <form-group for="User.Email" label="Email Address" required="true" help-text="We'll never share your email">
               <input-tag for="User.Email" type="email" icon-prefix="AtSign" />
             </form-group>
```

### LabelTagHelper

```csharp
<!-- Simple label -->
             <label-tag for="User.Name" text="Full Name" />
            
             <!-- Required field with asterisk -->
             <label-tag for="User.Email" text="Email Address" required="true" />
            
             <!-- Optional field without asterisk -->
             <label-tag for="User.PhoneNumber" text="Phone Number" />
            
             <!-- In form-group wrapper -->
             <form-group for="User.Email" label="Email Address" required="true" help-text="We'll never share your email">
               <label-tag for="User.Email" text="Email Address" required="true" />
               <input-tag for="User.Email" type="email" />
             </form-group>
```

### RadioButtonTagHelper

```csharp
<!-- Single radio button -->
             <radio-button-tag for="Model.Status" group-name="Status" label="Active" value="active" />
            
             <!-- Radio button group (render multiple radio-button-tags with same group-name) -->
             <div>
               <radio-button-tag for="Model.PaymentMethod" group-name="PaymentMethod" label="Credit Card" value="cc" checked="true" />
               <radio-button-tag for="Model.PaymentMethod" group-name="PaymentMethod" label="PayPal" value="paypal" />
               <radio-button-tag for="Model.PaymentMethod" group-name="PaymentMethod" label="Bank Transfer" value="bank" />
             </div>
            
             <!-- Required radio button group -->
             <div>
               <radio-button-tag for="Model.Shipping" group-name="Shipping" label="Standard (5-7 days)" value="standard" checked="true" />
               <radio-button-tag for="Model.Shipping" group-name="Shipping" label="Express (2-3 days)" value="express" />
               <radio-button-tag for="Model.Shipping" group-name="Shipping" label="Overnight" value="overnight" />
             </div>
            
             <!-- In form-group wrapper -->
             <form-group for="Model.PaymentMethod" label="Payment Method" required="true">
               <radio-button-tag for="Model.PaymentMethod" group-name="PaymentMethod" label="Credit Card" value="cc" />
               <radio-button-tag for="Model.PaymentMethod" group-name="PaymentMethod" label="Debit Card" value="dc" />
             </form-group>
```

### SelectTagHelper

```csharp
<!-- Simple select with SelectListItem collection -->
             <select-tag for="User.CountryId" items="@countries" />
            
             <!-- Select with enum binding -->
             <select-tag for="Order.Status" enum-type="typeof(OrderStatus)" />
            
             <!-- Select without blank option -->
             <select-tag for="Product.Category" items="@categories" add-blank="false" />
            
             <!-- Select with custom blank text -->
             <select-tag for="User.Department" items="@departments" blank-text="Choose a department..." />
            
             <!-- With initial selection -->
             @{ var selected = categories.First(c => c.Value == "electronics"); selected.Selected = true; }
             <select-tag for="Product.Category" items="@categories" />
            
             <!-- In form-group wrapper -->
             <form-group for="User.CountryId" label="Country" required="true" help-text="Select your country">
               <select-tag for="User.CountryId" items="@countries" />
             </form-group>
```

### TextAreaTagHelper

```csharp
<!-- Simple textarea with default 3 rows -->
             <textarea-tag for="Model.Comments" placeholder="Enter your comments..." />
            
             <!-- Textarea with custom height -->
             <textarea-tag for="Model.Description" rows="6" placeholder="Enter detailed description" />
            
             <!-- Required textarea -->
             <textarea-tag for="Model.Feedback" placeholder="Your feedback is important" required="true" />
            
             <!-- Large textarea for longer content -->
             <textarea-tag for="Model.BioOrNotes" rows="10" placeholder="Tell us about yourself..." />
            
             <!-- In form-group wrapper for complete form control -->
             <form-group for="Model.Comments" label="Comments" required="true" help-text="Please provide at least 10 characters">
               <textarea-tag for="Model.Comments" rows="5" placeholder="Share your thoughts..." />
             </form-group>
```

### ValidationMessageTagHelper

```csharp
<!-- Validation message for a field (after input element) -->
             <input-tag for="User.Email" type="email" />
             <validation-message for="User.Email" />
            
             <!-- Validation message with custom message -->
             <input-tag for="User.Age" type="number" />
             <validation-message for="User.Age" message="Age must be between 18 and 100" />
            
             <!-- Typical usage in form-group -->
             <form-group for="User.Email" label="Email" required="true">
               <input-tag for="User.Email" type="email" />
               <validation-message for="User.Email" />
             </form-group>
            
             <!-- Multiple fields with validation -->
             <form-group for="User.Password" label="Password" required="true">
               <input-tag for="User.Password" type="password" />
               <validation-message for="User.Password" />
             </form-group>
             <form-group for="User.ConfirmPassword" label="Confirm Password" required="true">
               <input-tag for="User.ConfirmPassword" type="password" />
               <validation-message for="User.ConfirmPassword" />
             </form-group>
```

### GridTagHelper

```csharp
<!-- Basic grid with data binding -->
             <grid data-source="@Model.Products" data-page-size="20">
               <grid-column property="Name" sortable="true">Product Name</grid-column>
               <grid-column property="Price" sortable="true" format="currency">Price</grid-column>
             </grid>
            
             <!-- Grid with row selection enabled -->
             <grid data-source="@Model.Orders" data-page-size="50" data-allow-selection="true">
               <grid-column property="OrderId">Order ID</grid-column>
               <grid-column property="OrderDate" sortable="true" format="date">Date</grid-column>
               <grid-column property="Status" filterable="true">Status</grid-column>
             </grid>
            
             <!-- Grid with custom CSS class and export option -->
             <grid data-source="@Model.Customers"
                   data-page-size="25"
                   data-allow-export="true"
                   data-allow-column-toggle="true"
                   data-css-class="compact-grid">
               <grid-column property="FirstName">First Name</grid-column>
               <grid-column property="Email" sortable="true">Email</grid-column>
               <grid-column property="CreatedDate" sortable="true" format="date">Created</grid-column>
             </grid>
            
             <!-- Complete example with IDataContext<Product> binding -->
             @{
               var productDataContext = new DataContext<Product>(productService);
               await productDataContext.Initialize(await productService.GetProductsAsync());
             }
             <grid data-source="@productDataContext.Items"
                   data-page-size="20"
                   data-allow-selection="true">
               <grid-column property="Name" sortable="true">Product</grid-column>
               <grid-column property="Category" filterable="true">Category</grid-column>
               <grid-column property="Price" format="currency" css-class="text-end">Price</grid-column>
               <grid-column property="Stock" sortable="true">Stock</grid-column>
             </grid>
            
             <!-- Grid with filtering and sorting -->
             <div class="mb-3">
               <label for="statusFilter">Filter by Status:</label>
               <select id="statusFilter" onchange="updateFilter(this.value)">
                 <option value="">All</option>
                 <option value="Active">Active</option>
                 <option value="Inactive">Inactive</option>
               </select>
             </div>
            
             <grid data-source="@Model.Items" data-page-size="20">
               <grid-column property="Name" sortable="true">Name</grid-column>
               <grid-column property="Status" filterable="true">Status</grid-column>
               <grid-column property="CreatedDate" sortable="true" format="date">Created</grid-column>
             </grid>
```

### BreadcrumbItem

```csharp
<!-- Create breadcrumb items for product details page -->
             @{
               var breadcrumbs = new List<BreadcrumbItem>
               {
                 new() { Label = "Home", Url = "/" },
                 new() { Label = "Products", Url = "/products" },
                 new() { Label = "Electronics", Url = "/products/electronics" },
                 new() { Label = "Laptops", Url = "/products/electronics/laptops" },
                 new() { Label = "Dell XPS 13" }  // No URL - this is the current page
               };
             }
             <breadcrumb items="breadcrumbs" />
            
             <!-- Create from dynamic data -->
             @{
               var breadcrumbs = new List<BreadcrumbItem>
               {
                 new() { Label = "Dashboard", Url = "/dashboard" }
               };
            
               foreach (var folder in Model.FolderHierarchy)
               {
                 breadcrumbs.Add(new()
                 {
                   Label = folder.Name,
                   Url = folder.Depth < Model.FolderHierarchy.Count - 1 ? folder.Url : null
                 });
               }
             }
             <breadcrumb items="breadcrumbs" />
```

### BreadcrumbTagHelper

```csharp
<!-- Basic breadcrumb navigation -->
             <breadcrumb items="new List<BreadcrumbItem> {
               new() { Label = "Home", Url = "/" },
               new() { Label = "Products", Url = "/products" },
               new() { Label = "Electronics" }
             }" />
            
             <!-- Breadcrumb from controller action -->
             <breadcrumb items="Model.Breadcrumbs" />
            
             <!-- Programmatically constructed breadcrumb in view -->
             @{
               var breadcrumbs = new List<BreadcrumbItem>
               {
                 new() { Label = "Dashboard", Url = "/dashboard" },
                 new() { Label = "Reports", Url = "/reports" },
                 new() { Label = "Monthly Summary" }
               };
             }
             <breadcrumb items="breadcrumbs" />
```

### IfAuthorizedTagHelper

```csharp
<!-- Simple logout button, shown only if authenticated -->
             <if-authorized>
               <button class="btn btn-danger">Logout</button>
             </if-authorized>
            
             <!-- User profile section in header -->
             <if-authorized>
               <div class="user-profile">
                 <span>Welcome, @Model.User.Name!</span>
                 <a href="/profile">View Profile</a>
               </div>
             </if-authorized>
            
             <!-- Multiple buttons and links for authenticated users -->
             <if-authorized>
               <div class="authenticated-menu">
                 <a href="/dashboard" class="btn btn-primary">Dashboard</a>
                 <a href="/orders" class="btn btn-info">My Orders</a>
                 <button onclick="logout()" class="btn btn-danger">Logout</button>
               </div>
             </if-authorized>
            
             <!-- Unauthenticated content shown separately -->
             <div>
               <if-authorized>
                 <p>You are logged in.</p>
               </if-authorized>
               <!-- Note: For not-authenticated content, use negation in IfAuthorizedTagHelper
                    or implement a separate IfNotAuthorizedTagHelper -->
             </div>
```

### IfClaimTagHelper

```csharp
<!-- Simple claim check: show if user has "role" claim with value "Admin" -->
             <if-claim type="role" value="Admin">
               <button class="btn btn-danger">Delete User</button>
             </if-claim>
            
             <!-- Multiple values (OR logic): show if user is Admin or Manager -->
             <if-claim type="role" value="Admin,Manager">
               <a href="/admin" class="btn btn-primary">Admin Panel</a>
             </if-claim>
            
             <!-- Department-based access: show if in IT or Finance department -->
             <if-claim type="department" value="IT,Finance">
               <div class="reports-section">
                 <h3>Financial Reports</h3>
                 <a href="/reports/finance">View Reports</a>
               </div>
             </if-claim>
            
             <!-- Subscription level: show premium features if subscription is Premium or Enterprise -->
             <if-claim type="subscription_level" value="Premium,Enterprise">
               <div class="premium-features">
                 <h3>Advanced Analytics</h3>
                 <p>You have access to advanced reporting features.</p>
               </div>
             </if-claim>
            
             <!-- Claim existence without value check: show if user has ANY custom-permission claim -->
             <if-claim type="custom-permission">
               <p>You have special permissions.</p>
             </if-claim>
            
             <!-- Combined with other markup: admin toolbar with multiple restrictions -->
             <div class="admin-toolbar">
               <if-claim type="role" value="Admin">
                 <button class="btn btn-danger" onclick="deleteAll()">Delete All</button>
               </if-claim>
               <if-claim type="role" value="Admin,Moderator">
                 <button class="btn btn-warning" onclick="moderate()">Moderate</button>
               </if-claim>
             </div>
```

### IfRoleTagHelper

```csharp
<!-- Simple role check: show delete button if user is Admin -->
             <if-role role="Admin">
               <button class="btn btn-danger" onclick="deleteItem()">Delete</button>
             </if-role>
            
             <!-- Multiple roles (OR logic): show if user is Admin or Manager -->
             <if-role role="Admin,Manager">
               <a href="/admin" class="btn btn-primary">Admin Panel</a>
             </if-role>
            
             <!-- Editor dashboard with edit/delete options -->
             <if-role role="Editor">
               <div class="editor-toolbar">
                 <button class="btn btn-info" onclick="editItem()">Edit</button>
                 <button class="btn btn-warning" onclick="publishItem()">Publish</button>
               </div>
             </if-role>
            
             <!-- Multiple role sections with different features -->
             <div class="dashboard">
               <if-role role="Admin">
                 <div class="admin-section">
                   <h3>System Administration</h3>
                   <a href="/admin/users">Manage Users</a>
                   <a href="/admin/settings">System Settings</a>
                 </div>
               </if-role>
               <if-role role="Manager,Editor">
                 <div class="content-section">
                   <h3>Content Management</h3>
                   <a href="/content">View Content</a>
                 </div>
               </if-role>
             </div>
            
             <!-- Nested role checks in a complex menu -->
             <nav class="sidebar">
               <ul>
                 <li><a href="/home">Home</a></li>
                 <if-role role="User,Admin,Manager">
                   <li><a href="/dashboard">Dashboard</a></li>
                   <li><a href="/profile">My Profile</a></li>
                 </if-role>
                 <if-role role="Admin">
                   <li><a href="/admin">Administration</a></li>
                 </if-role>
               </ul>
             </nav>
            
             <!-- Contributor role with limited permissions -->
             <if-role role="Contributor,Editor,Admin">
               <div class="publish-section">
                 <button class="btn btn-success" onclick="submit()">Submit for Review</button>
               </div>
             </if-role>
```

## API Reference

See [API Reference](./api-reference.md) for complete documentation.

