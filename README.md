# Privileged

Privileged is an authorization library for restricting resources by action, subject and qualifiers.
It's designed to be incrementally adoptable and can easily scale between a simple claim based and fully featured
subject and action based authorization. It makes it easy to manage and share permissions across UI components,
API services, and database queries.

[![Build Project](https://github.com/loresoft/Privileged/actions/workflows/dotnet.yml/badge.svg)](https://github.com/loresoft/Privileged/actions/workflows/dotnet.yml)
[![License](https://img.shields.io/github/license/loresoft/Privileged.svg)](https://github.com/loresoft/Privileged/blob/main/LICENSE)
[![Coverage Status](https://coveralls.io/repos/github/loresoft/Privileged/badge.svg?branch=main)](https://coveralls.io/github/loresoft/Privileged?branch=main)

| Package                                                                              | Version                                                                                                                           | Description                                                          |
| ------------------------------------------------------------------------------------ | --------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------- |
| [Privileged](https://www.nuget.org/packages/Privileged/)                             | [![NuGet](https://img.shields.io/nuget/v/Privileged.svg)](https://www.nuget.org/packages/Privileged/)                             | Core authorization library for rule-based permissions                |
| [Privileged.Authorization](https://www.nuget.org/packages/Privileged.Authorization/) | [![NuGet](https://img.shields.io/nuget/v/Privileged.Authorization.svg)](https://www.nuget.org/packages/Privileged.Authorization/) | ASP.NET Core authorization integration with attribute-based policies |
| [Privileged.Components](https://www.nuget.org/packages/Privileged.Components/)       | [![NuGet](https://img.shields.io/nuget/v/Privileged.Components.svg)](https://www.nuget.org/packages/Privileged.Components/)       | Blazor components for privilege-aware UI elements                    |
| [Privileged.Endpoint](https://www.nuget.org/packages/Privileged.Endpoint/)           | [![NuGet](https://img.shields.io/nuget/v/Privileged.Endpoint.svg)](https://www.nuget.org/packages/Privileged.Endpoint/)           | ASP.NET Core endpoint extensions for privilege requirements          |

## Installation

Install the core package via NuGet:

```bash
dotnet add package Privileged
```

For ASP.NET Core applications with attribute-based authorization, also install the authorization package:

```bash
dotnet add package Privileged.Authorization
```

For ASP.NET Core applications using minimal APIs with privilege requirements, also install the endpoint package:

```bash
dotnet add package Privileged.Endpoint
```

For Blazor applications, also install the components package:

```bash
dotnet add package Privileged.Components
```

## Features

- **Versatile** An incrementally adoptable and can easily scale between a simple claim based and fully featured subject and attribute based authorization.
- **Isomorphic** Can be used on front-end and back-end and complementary packages make integration with Frontend and Backend effortless
- **Declarative** Thanks to declarative rules, you can serialize and share permissions between UI and API or microservices
- **Rule-based** Support for both allow and forbid rules with rule precedence
- **Aliases** Create reusable aliases for actions, subjects, and qualifiers
- **Qualifiers** Fine-grained control with field-level permissions
- **ASP.NET Core Integration** Seamless integration with ASP.NET Core authorization using attribute-based policies
- **Blazor Integration** Ready-to-use components for conditional rendering and privilege-aware form inputs
- **Performance Optimized** Efficient rule evaluation and matching algorithms

## General

Privileged operates on rules for what a user can actually do in the application. A rule itself depends on the 3 parameters:

1. **Action** Describes what user can actually do in the app. User action is a word (usually a verb) which depends on the business logic (e.g., `update`, `read`). Very often it will be a list of words from CRUD - `create`, `read`, `update` and `delete`.
2. **Subject** The subject which you want to check user action on. Usually this is a business (or domain) entity name (e.g., `Subscription`, `Post`, `User`).
3. **Qualifiers** Can be used to restrict user action only to matched subject's qualifiers (e.g., to allow moderator to update `published` field of `Post` but not update `description` or `title`)

## Basic Usage

### Simple Rules

Using builder to create basic allow and forbid rules:

```csharp
var context = new PrivilegeBuilder()
    .Allow("read", "Post")
    .Allow("write", "User")
    .Forbid("delete", "User")
    .Build();

// Check permissions
bool canReadPost = context.Allowed("read", "Post");      // true
bool canWriteUser = context.Allowed("write", "User");    // true
bool canDeleteUser = context.Allowed("delete", "User");  // false
bool canReadUser = context.Allowed("read", "User");      // false (not explicitly allowed)
```

### Wildcard Rules

Use wildcards to allow all actions on a subject or an action on all subjects:

```csharp
var context = new PrivilegeBuilder()
    .Allow("test", PrivilegeRule.Any)     // Allow 'test' action on any subject
    .Allow(PrivilegeRule.Any, "Post")     // Allow any action on 'Post'
    .Forbid("publish", "Post")            // Forbid overrides allow
    .Build();

context.Allowed("read", "Post").Should().BeTrue();
context.Allowed("update", "Post").Should().BeTrue();
context.Allowed("archive", "Post").Should().BeTrue();
context.Allowed("read", "User").Should().BeFalse();
context.Allowed("delete", "Post").Should().BeTrue();
context.Allowed("publish", "Post").Should().BeFalse();  // Forbid takes precedence
context.Allowed("test", "User").Should().BeTrue();
context.Allowed("test", "Post").Should().BeTrue();
```

### Using Qualifiers

Qualifiers provide field-level or fine-grained permissions:

```csharp
var context = new PrivilegeBuilder()
    .Allow("read", "Post", ["title", "id"])   // Only allow reading specific fields
    .Allow("read", "User")                    // Allow reading all User fields
    .Build();

// Post permissions with qualifiers
context.Allowed("read", "Post").Should().BeTrue();           // General permission
context.Allowed("read", "Post", "id").Should().BeTrue();     // Specific field allowed
context.Allowed("read", "Post", "title").Should().BeTrue();  // Specific field allowed
context.Allowed("read", "Post", "content").Should().BeFalse(); // Field not allowed

// User permissions without qualifiers
context.Allowed("read", "User").Should().BeTrue();           // All fields allowed
context.Allowed("read", "User", "id").Should().BeTrue();     // Any field allowed
```

## Advanced Features

### Multiple Actions and Subjects

Use extension methods for bulk rule creation:

```csharp
var context = new PrivilegeBuilder()
    .Allow(["read", "update"], "Post")                    // Multiple actions, single subject
    .Allow("read", ["Post", "User"])                      // Single action, multiple subjects
    .Allow(["create", "read"], ["Post", "Comment"])       // Multiple actions and subjects
    .Build();

context.Allowed("read", "Post").Should().BeTrue();
context.Allowed("update", "Post").Should().BeTrue();
context.Allowed("read", "User").Should().BeTrue();
context.Allowed("create", "Comment").Should().BeTrue();
```

### Aliases

Create reusable aliases for common groupings:

```csharp
var context = new PrivilegeBuilder()
    .Alias("Manage", ["Create", "Update", "Delete"], PrivilegeMatch.Action)
    .Allow("Manage", "Project")                         // Allows all actions defined in the "Manage" alias
    .Allow("Read", "User")                              // Allows reading User
    .Allow("Update", "User", ["Profile", "Settings"])   // Allows updating User's Profile and Settings
    .Forbid("Delete", "User")                           // Forbids deleting User
    .Build();

bool canCreateProject = context.Allowed("Create", "Project");           // true
bool canReadUser = context.Allowed("Read", "User");                     // true
bool canUpdateProfile = context.Allowed("Update", "User", "Profile");   // true
bool canUpdatePassword = context.Allowed("Update", "User", "Password"); // false
bool canDeleteUser = context.Allowed("Delete", "User");                 // false
```

### Qualifier Aliases

Aliases can also be used for qualifiers:

```csharp
var context = new PrivilegeBuilder()
    .Alias("PublicFields", ["title", "summary", "author"], PrivilegeMatch.Qualifier)
    .Allow("read", "Post", ["PublicFields"])
    .Build();

context.Allowed("read", "Post", "title").Should().BeTrue();
context.Allowed("read", "Post", "summary").Should().BeTrue();
context.Allowed("read", "Post", "content").Should().BeFalse();
```

## Rule Evaluation

### Rule Precedence

Rules are evaluated in the order they are defined, with more specific rules taking precedence:

1. **Forbid rules** always take precedence over allow rules when both match
2. Rules are matched based on exact string comparison
3. Wildcard rules `PrivilegeRule.Any` match any value
4. Alias expansion happens during rule matching

### String Comparison

By default, rule matching uses `StringComparer.InvariantCultureIgnoreCase`. You can customize this:

```csharp
var context = new PrivilegeContext(rules, aliases, StringComparer.Ordinal);
```

## API Reference

### PrivilegeBuilder

- `Allow(string action, string subject, IEnumerable<string>? qualifiers = null)` - Add an allow rule
- `Forbid(string action, string subject, IEnumerable<string>? qualifiers = null)` - Add a forbid rule
- `Alias(string alias, IEnumerable<string> values, PrivilegeMatch type)` - Create an alias
- `Build()` - Create the PrivilegeContext

### PrivilegeContext

- `Allowed(string? action, string? subject, string? qualifier = null)` - Check if action is allowed
- `Forbidden(string? action, string? subject, string? qualifier = null)` - Check if action is forbidden
- `MatchRules(string? action, string? subject, string? qualifier = null)` - Get matching rules

### Extension Methods

- `Allow(IEnumerable<string> actions, string subject, ...)` - Allow multiple actions on single subject
- `Allow(string action, IEnumerable<string> subjects, ...)` - Allow single action on multiple subjects
- `Allow(IEnumerable<string> actions, IEnumerable<string> subjects, ...)` - Allow multiple actions on multiple subjects
- Similar `Forbid` overloads for forbid rules

## ASP.NET Core Authorization Integration

The `Privileged.Authorization` package provides seamless integration with ASP.NET Core's authorization system through attribute-based policies.

### Authorization Setup

First, configure the authorization services in your `Program.cs`:

```csharp
// Program.cs
using Privileged.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add authentication here (JWT/Cookies/etc.)
builder.Services.AddAuthentication(...);
builder.Services.AddAuthorization();

// Register privilege services + your provider
builder.Services.AddPrivilegeAuthorization();
builder.Services.AddScoped<IPrivilegeContextProvider, YourPrivilegeContextProvider>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

var app = builder.Build();
```

### Using the PrivilegeAttribute

Use the `[Privilege]` attribute on controllers and actions to declaratively specify authorization requirements:

```csharp
[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    [HttpGet]
    [Privilege("read", "Post")]
    public IActionResult GetPosts()
    {
        // Only users with "read" privilege on "Post" can access this
        return Ok();
    }

    [HttpPost]
    [Privilege("create", "Post")]
    public IActionResult CreatePost([FromBody] CreatePostRequest request)
    {
        // Only users with "create" privilege on "Post" can access this
        return Ok();
    }

    [HttpPut("{id}")]
    [Privilege("update", "Post")]
    public IActionResult UpdatePost(int id, [FromBody] UpdatePostRequest request)
    {
        // Only users with "update" privilege on "Post" can access this
        return Ok();
    }

    [HttpDelete("{id}")]
    [Privilege("delete", "Post")]
    public IActionResult DeletePost(int id)
    {
        // Only users with "delete" privilege on "Post" can access this
        return NoContent();
    }

    [HttpPut("{id}/title")]
    [Privilege("update", "Post", "title")]
    public IActionResult UpdatePostTitle(int id, [FromBody] string title)
    {
        // Only users with "update" privilege on "Post" for "title" field can access this
        return Ok();
    }
}
```

### Manual Authorization Checks

You can also perform manual authorization checks in your controllers:

```csharp
[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly IPrivilegeContextProvider _contextProvider;

    public PostsController(IPrivilegeContextProvider contextProvider)
    {
        _contextProvider = contextProvider;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPost(int id)
    {
        var context = await _contextProvider.GetContextAsync();

        if (!context.Allowed("read", "Post"))
        {
            return Forbid();
        }

        // Additional business logic...
        return Ok();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePost(int id, [FromBody] UpdatePostRequest request)
    {
        var context = await _contextProvider.GetContextAsync();

        // Check different permissions based on what's being updated
        if (!string.IsNullOrEmpty(request.Title) && !context.Allowed("update", "Post", "title"))
        {
            return Forbid("Cannot update post title");
        }

        if (!string.IsNullOrEmpty(request.Content) && !context.Allowed("update", "Post", "content"))
        {
            return Forbid("Cannot update post content");
        }

        // Perform update...
        return Ok();
    }
}
```

#### Minimal API Example

You can also use privilege-based authorization with ASP.NET Core Minimal APIs. The `[Privilege]` attribute works on route handler delegates, and the dynamic policies will be generated in exactly the same way.

For minimal APIs that need to use the `RequirePrivilege` extension method, also add the `Privileged.Endpoint` package.

```csharp
using Microsoft.AspNetCore.Authorization;
using Privileged.Authorization;
using Privileged.Endpoint;

var builder = WebApplication.CreateBuilder(args);

// Add authentication here (JWT/Cookies/etc.)
builder.Services.AddAuthentication(...);
builder.Services.AddAuthorization();

// Register privilege services + your provider
builder.Services.AddPrivilegeAuthorization<DatabasePrivilegeContextProvider>();


var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Simple collection endpoint requiring read privilege on Post
app.MapGet("/api/posts", [Privilege("read", "Post")] () => Results.Ok(new[] { new { Id = 1, Title = "Hello" } }));

// Create endpoint requiring create privilege
app.MapPost("/api/posts", [Privilege("create", "Post")] (CreatePostRequest req) =>
{
    // Business logic...
    return Results.Created($"/api/posts/{123}", req);
});

// Update with qualifier (field-level) example using RequirePrivilege extension
app.MapPut("/api/posts/{id}/title", (int id, string title) =>
{
    // Only users with update privilege on Post:title reach here
    return Results.Ok();
}).RequirePrivilege("update", "Post", "title");

// Manual check example inside a handler
app.MapPut("/api/posts/{id}", async (int id, UpdatePostRequest req, IPrivilegeContextProvider provider) =>
{
    var context = await provider.GetContextAsync();

    if (req.Title is not null && !context.Allowed("update", "Post", "title"))
        return Results.Forbid();

    if (req.Content is not null && !context.Allowed("update", "Post", "content"))
        return Results.Forbid();

    return Results.Ok();
});

app.Run();

// Example request models
public record CreatePostRequest(string Title, string Content);
public record UpdatePostRequest(string? Title, string? Content);
```

Key points:

- Apply `[Privilege]` directly to route handler delegates.
- Dynamic policy names follow the `Privilege:action:subject[:qualifier]` format automatically.
- For ad-hoc logic or multiple field checks, inject `IPrivilegeContextProvider` and perform manual `Allowed` calls.
- Combine with your existing authentication middleware (JWT, cookies, etc.).

### IPrivilegeContextProvider Implementation

The `IPrivilegeContextProvider` interface allows you to load privilege contexts asynchronously, which is useful for scenarios where permissions are loaded from external sources like APIs, databases, or authentication systems. This interface is used by both ASP.NET Core and Blazor applications.

#### ASP.NET Core: Database-Based Provider

For ASP.NET Core applications that load permissions from a database or external service.

```csharp
public class DatabasePrivilegeContextProvider : IPrivilegeContextProvider
{
    private readonly IUserPermissionService _permissionService;
    private readonly HybridCache _cache;
    private readonly ILogger<DatabasePrivilegeContextProvider> _logger;

    public DatabasePrivilegeContextProvider(
        IUserPermissionService permissionService,
        HybridCache cache,
        ILogger<DatabasePrivilegeContextProvider> logger)
    {
        _permissionService = permissionService;
        _cache = cache;
        _logger = logger;
    }

    public async ValueTask<PrivilegeContext> GetContextAsync(ClaimsPrincipal? claimsPrincipal = null)
    {
        var user = claimsPrincipal;

        // Return empty context for unauthenticated users
        if (user?.Identity?.IsAuthenticated != true)
            return PrivilegeContext.Empty;

        try
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                _logger.LogWarning("User ID not found in claims");
                return PrivilegeContext.Empty;
            }

            string cacheKey = $"privileged:permissions:{userId}";

            // Cache the privilege model to avoid repeated DB/service calls per request.
            PrivilegeModel privilegeModel = await _cache.GetOrCreateAsync(
                cacheKey,
                async ct => await _permissionService.GetUserPermissionsAsync(userId),
                options => options.SetExpiration(TimeSpan.FromMinutes(5))
            );

            return new PrivilegeContext(privilegeModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load privileges for current user");
            return PrivilegeContext.Empty;
        }
    }
}
```

#### Blazor: API-Based Provider

For Blazor applications that load permissions from an API:

```csharp
public class HttpPrivilegeContextProvider : IPrivilegeContextProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpPrivilegeContextProvider> _logger;

    public HttpPrivilegeContextProvider(
        HttpClient httpClient,
        ILogger<HttpPrivilegeContextProvider> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async ValueTask<PrivilegeContext> GetContextAsync(ClaimsPrincipal? claimsPrincipal = null)
    {
        try
        {
            // Load privilege model from API
            var privilegeModel = await _httpClient.GetFromJsonAsync<PrivilegeModel>("/api/user/privileges");
            return new PrivilegeContext(privilegeModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load user privileges");

            // Return minimal context with basic read permissions as fallback
            return new PrivilegeBuilder()
                .Allow("read", "Public")
                .Build();
        }
    }
}
```

#### Static Provider

For simpler scenarios with static permissions:

```csharp
public class StaticPrivilegeContextProvider : IPrivilegeContextProvider
{
    public ValueTask<PrivilegeContext> GetContextAsync(ClaimsPrincipal? claimsPrincipal = null)
    {
        var context = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Allow("write", "Post", new[] {"title", "content"})
            .Allow("delete", "Post")
            .Forbid("publish", "Post") // Override specific action
            .Build();

        return ValueTask.FromResult(context);
    }
}
```

#### Registration

Register your chosen provider in `Program.cs`:

```csharp
// For ASP.NET Core
builder.Services.AddScoped<IPrivilegeContextProvider, DatabasePrivilegeContextProvider>();

// For Blazor
builder.Services.AddScoped<IPrivilegeContextProvider, HttpPrivilegeContextProvider>();
// or
builder.Services.AddScoped<IPrivilegeContextProvider, StaticPrivilegeContextProvider>();
```

## Blazor Integration

The `Privileged.Components` package provides components for conditional rendering based on permissions.

### Blazor Setup

First, add the privilege context as a cascading value in your app:

```csharp
// Program.cs or similar

// Create privilege rules
var privilegeContext = new PrivilegeBuilder()
    .Allow("read", "Post")
    .Allow("edit", "Post", ["title", "content"])
    .Allow("delete", "Post")
    .Build();

// Make available as cascading parameter
builder.Services.AddCascadingValue(_ => privilegeContext);
```

### PrivilegeContextView Component

For scenarios where you need to load the privilege context asynchronously, use the `PrivilegeContextView` component:

```razor
<PrivilegeContextView>
    <Loading>
        <div class="spinner-border" role="status">
            <span class="visually-hidden">Loading permissions...</span>
        </div>
    </Loading>
    <Loaded>
        <PrivilegeView Action="read" Subject="Post">
            <p>Content loaded with permissions!</p>
        </PrivilegeView>
    </Loaded>
</PrivilegeContextView>
```

PrivilegeContextView component requires an `IPrivilegeContextProvider` service to be registered:

```csharp
// In Program.cs
builder.Services.AddScoped<IPrivilegeContextProvider, YourPrivilegeContextProvider>();
```

#### Using PrivilegeContextView in a Layout

The most common pattern is to wrap your entire layout with `PrivilegeContextView` to ensure permissions are loaded before any page content is rendered:

```razor
@* MainLayout.razor *@
@inherits LayoutComponentBase
<PrivilegeContextView>
    <div class="page">
        <div class="sidebar">
            <NavMenu />
        </div>
        <main>
            @Body
        </main>
    </div>
</PrivilegeContextView>
```

With this approach, all pages will automatically have access to the privilege context, and users will see a loading state until permissions are loaded. Your navigation menu can also use privilege checking:

### PrivilegeView Component

Use the `PrivilegeView` component to conditionally render content:

```razor
@* Basic usage with ChildContent *@
<PrivilegeView Action="read" Subject="Post">
    <p>You can read posts!</p>
</PrivilegeView>

@* With both allowed and forbidden content *@
<PrivilegeView Action="delete" Subject="Post">
    <Allowed>
        <button class="btn btn-danger">Delete Post</button>
    </Allowed>
    <Forbidden>
        <span class="text-muted">Delete not allowed</span>
    </Forbidden>
</PrivilegeView>

@* With field-level permissions *@
<PrivilegeView Action="edit" Subject="Post" Field="title">
    <input type="text" placeholder="Edit title" />
</PrivilegeView>
```

### PrivilegeLink Component

The `PrivilegeLink` component extends the standard `NavLink` component to provide privilege-aware navigation. It only renders the link when the user has the required permissions, making it perfect for building navigation menus and UI elements that should only be visible to authorized users.

The link components require a `PrivilegeContext` cascading parameter.

```razor
@* Basic navigation link that only shows if user can read posts *@
<PrivilegeLink Subject="Post" Action="read" href="/posts">
    View Posts
</PrivilegeLink>

@* Link with custom action *@
<PrivilegeLink Subject="Post" Action="edit" href="/posts/edit">
    Edit Posts
</PrivilegeLink>

@* Link with field-level permissions *@
<PrivilegeLink Subject="Post" Action="update" Qualifier="title" href="/posts/edit-title">
    Edit Post Titles
</PrivilegeLink>

@* Navigation menu example *@
<nav class="navbar">
    <PrivilegeLink Subject="Post" href="/posts" class="nav-link">
        Posts
    </PrivilegeLink>
    <PrivilegeLink Subject="User" Action="manage" href="/users" class="nav-link">
        Users
    </PrivilegeLink>
    <PrivilegeLink Subject="Settings" Action="edit" href="/settings" class="nav-link">
        Settings
    </PrivilegeLink>
</nav>

@* Using with CSS classes and additional attributes *@
<PrivilegeLink Subject="Post"
               Action="delete"
               href="/posts/delete"
               class="btn btn-danger"
               @onclick="ConfirmDelete">
    Delete Post
</PrivilegeLink>
```

### Privilege-Aware Input Components

The `Privileged.Components` package also includes privilege-aware input components that automatically handle read/write permissions:

The input components require a `PrivilegeContext` cascading parameter.

```razor
@* Text input that becomes read-only based on permissions *@
<PrivilegeInputText @bind-Value="@model.Title"
                    Subject="Post"
                    Field="title"
                    ReadAction="read"
                    UpdateAction="update" />

@* Number input with privilege checking *@
<PrivilegeInputNumber @bind-Value="@model.Views"
                      Subject="Post"
                      Field="views" />

@* Select dropdown with privilege-based enabling/disabling *@
<PrivilegeInputSelect @bind-Value="@model.Status"
                      Subject="Post"
                      Field="status">
    <option value="draft">Draft</option>
    <option value="published">Published</option>
</PrivilegeInputSelect>

@* Checkbox with privilege checking *@
<PrivilegeInputCheckbox @bind-Value="@model.IsActive"
                        Subject="Post"
                        Field="isActive" />

@* Text area with privilege checking *@
<PrivilegeInputTextArea @bind-Value="@model.Content"
                        Subject="Post"
                        Field="content"
                        rows="5" />
```

These components automatically:

- Enable/disable based on update permissions
- Show/hide based on read permissions

#### PrivilegeForm Component

The `PrivilegeForm` component extends the standard `EditForm` to provide privilege-aware form functionality. It automatically cascades privilege form state to child components, allowing you to set default privilege settings at the form level while maintaining the ability for individual components to override specific values.

```razor
@* Basic form with default privilege settings *@
<PrivilegeForm Model="@postModel" Subject="Post" ReadAction="read" UpdateAction="update">
    <DataAnnotationsValidator />
    <ValidationSummary />
    
    @* These inputs inherit the Subject, ReadAction, and UpdateAction from the form *@
    <PrivilegeInputText @bind-Value="@postModel.Title" Field="title" />
    <PrivilegeInputTextArea @bind-Value="@postModel.Content" Field="content" />
    <PrivilegeInputSelect @bind-Value="@postModel.Status" Field="status">
        <option value="draft">Draft</option>
        <option value="published">Published</option>
    </PrivilegeInputSelect>
    
    @* This input overrides the default Subject *@
    <PrivilegeInputText @bind-Value="@postModel.AuthorEmail" 
                        Subject="User" 
                        Field="email" />
    
    <button type="submit" class="btn btn-primary">Save Post</button>
</PrivilegeForm>

@* Form with different actions for different operations *@
<PrivilegeForm Model="@userModel" Subject="User" ReadAction="view" UpdateAction="edit">
    <PrivilegeInputText @bind-Value="@userModel.FirstName" Field="firstName" />
    <PrivilegeInputText @bind-Value="@userModel.LastName" Field="lastName" />
    
    @* Admin-only field with different permissions *@
    <PrivilegeInputText @bind-Value="@userModel.Role" 
                        Subject="User" 
                        Field="role"
                        ReadAction="viewRole"
                        UpdateAction="editRole" />
</PrivilegeForm>
```

Key benefits of `PrivilegeForm`:

- **Cascading Defaults**: Set privilege parameters once at the form level instead of repeating them on every input
- **Flexible Overrides**: Individual components can override any of the cascaded values when needed
- **Standard EditForm**: Maintains all functionality of the base `EditForm` component including validation
- **Clean Markup**: Reduces repetitive code and makes forms easier to maintain

### PrivilegeInputText HTML Output Example

Below is an example of the `PrivilegeInputText` component and its corresponding HTML output for various states based on the `PrivilegeContext` results:

#### PrivilegeInputText Component

```razor
<PrivilegeInputText @bind-Value="@model.Title"
                    Subject="Post"
                    Field="title" />
```

#### Corresponding HTML Output

```html
<!-- When the user has 'update' permission -->
<input type="text" id="Title" name="Title" value="Sample Title" />

<!-- When the user has only 'read' permission, make input readonly -->
<input type="text" id="Title" name="Title" value="Sample Title" readonly />

<!-- When the user has neither 'read' nor 'update' permission, use password type and disable -->
<input type="password" id="Title" name="Title" disabled />
```

This demonstrates how the `PrivilegeInputText` component dynamically adapts its output based on the user's permissions as determined by the `PrivilegeContext`.

## License

This project is licensed under the MIT License.

## References

Inspired by [CASL](https://github.com/stalniy/casl)
