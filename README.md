# Privileged

Privileged is an authorization library for restricting resources by action, subject and qualifiers.
It's designed to be incrementally adoptable and can easily scale between a simple claim based and fully featured
subject and action based authorization. It makes it easy to manage and share permissions across UI components,
API services, and database queries.

[![Build Project](https://github.com/loresoft/Privileged/actions/workflows/dotnet.yml/badge.svg)](https://github.com/loresoft/Privileged/actions/workflows/dotnet.yml)
[![License](https://img.shields.io/github/license/loresoft/Privileged.svg)](https://github.com/loresoft/Privileged/blob/main/LICENSE)
[![Coverage Status](https://coveralls.io/repos/github/loresoft/Privileged/badge.svg?branch=main)](https://coveralls.io/github/loresoft/Privileged?branch=main)

| Package                                                                        | Version                                                                                                                     | Description                                           |
| ------------------------------------------------------------------------------ | --------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------- |
| [Privileged](https://www.nuget.org/packages/Privileged/)                       | [![NuGet](https://img.shields.io/nuget/v/Privileged.svg)](https://www.nuget.org/packages/Privileged/)                       | Core authorization library for rule-based permissions |
| [Privileged.Components](https://www.nuget.org/packages/Privileged.Components/) | [![NuGet](https://img.shields.io/nuget/v/Privileged.Components.svg)](https://www.nuget.org/packages/Privileged.Components/) | Blazor components for privilege-aware UI elements     |

## Installation

Install the core package via NuGet:

```bash
dotnet add package Privileged
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
    .Allow("test", PrivilegeSubjects.All)    // Allow 'test' action on any subject
    .Allow(PrivilegeActions.All, "Post")     // Allow any action on 'Post'
    .Forbid("publish", "Post")               // Forbid overrides allow
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
    .Allow("read", "User")                     // Allow reading all User fields
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

## Blazor Integration

The `Privileged.Components` package provides components for conditional rendering based on permissions.

### Setup

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
        <PrivilegedView Action="read" Subject="Post">
            <p>Content loaded with permissions!</p>
        </PrivilegedView>
    </Loaded>
</PrivilegeContextView>
```

This component requires an `IPrivilegeContextProvider` service to be registered:

```csharp
// In Program.cs
builder.Services.AddScoped<IPrivilegeContextProvider, YourPrivilegeContextProvider>();
```

### PrivilegedView Component

Use the `PrivilegedView` component to conditionally render content:

```razor
@* Basic usage with ChildContent *@
<PrivilegedView Action="read" Subject="Post">
    <p>You can read posts!</p>
</PrivilegedView>

@* With both allowed and forbidden content *@
<PrivilegedView Action="delete" Subject="Post"
               Allowed="@allowedContent"
               Forbidden="@forbiddenContent" />

@* With field-level permissions *@
<PrivilegedView Action="edit" Subject="Post" Field="title">
    <input type="text" placeholder="Edit title" />
</PrivilegedView>

@code {
    private RenderFragment<PrivilegeContext> allowedContent = (context) =>
        @<button class="btn btn-danger">Delete Post</button>;

    private RenderFragment<PrivilegeContext> forbiddenContent = (context) =>
        @<span class="text-muted">Delete not allowed</span>;
}
```

### Cascading Parameters

The component requires a `PrivilegeContext` cascading parameter:

```razor
<CascadingValue Value="@privilegeContext">
    <PrivilegedView Action="read" Subject="Post">
        <p>Protected content here</p>
    </PrivilegedView>
</CascadingValue>
```

### Privilege-Aware Input Components

The `Privileged.Components` package also includes privilege-aware input components that automatically handle read/write permissions:

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

## Rule Evaluation

### Rule Precedence

Rules are evaluated in the order they are defined, with more specific rules taking precedence:

1. **Forbid rules** always take precedence over allow rules when both match
2. Rules are matched based on exact string comparison (case-insensitive by default)
3. Wildcard rules (`PrivilegeActions.All`, `PrivilegeSubjects.All`) match any value
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

## License

This project is licensed under the MIT License.

## References

Inspired by [CASL](https://github.com/stalniy/casl)
