using System;
using System.Collections.Generic;

namespace Privileged.Components.Tests;

public class PrivilegeLinkTests : TestContext
{
    [Fact]
    public void RendersWhenUserHasPermission()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Build();

        // Act
        var cut = RenderComponent<PrivilegeLink>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Subject, "Post")
            .Add(p => p.Action, "read")
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "href", "/posts" } })
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "View Posts"))
        );

        // Assert
        cut.Find("a").Should().NotBeNull();
        cut.Find("a").GetAttribute("href").Should().Be("/posts");
        cut.Find("a").TextContent.Should().Be("View Posts");
    }

    [Fact]
    public void DoesNotRenderWhenUserLacksPermission()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("read", "User")
            .Build();

        // Act
        var cut = RenderComponent<PrivilegeLink>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Subject, "Post")
            .Add(p => p.Action, "read")
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "href", "/posts" } })
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "View Posts"))
        );

        // Assert
        cut.Markup.Should().BeEmpty();
    }

    [Fact]
    public void DefaultActionIsRead()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Build();

        // Act
        var cut = RenderComponent<PrivilegeLink>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Subject, "Post")
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "href", "/posts" } })
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "View Posts"))
        );

        // Assert
        cut.Instance.Action.Should().Be("read");
        cut.Find("a").Should().NotBeNull();
    }

    [Fact]
    public void WorksWithCustomAction()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("edit", "Post")
            .Build();

        // Act
        var cut = RenderComponent<PrivilegeLink>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Subject, "Post")
            .Add(p => p.Action, "edit")
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "href", "/posts/edit" } })
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "Edit Posts"))
        );

        // Assert
        cut.Find("a").Should().NotBeNull();
        cut.Find("a").GetAttribute("href").Should().Be("/posts/edit");
        cut.Find("a").TextContent.Should().Be("Edit Posts");
    }

    [Fact]
    public void WorksWithQualifiers()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("edit", "Post", ["title", "content"])
            .Build();

        // Act
        var cut = RenderComponent<PrivilegeLink>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Subject, "Post")
            .Add(p => p.Action, "edit")
            .Add(p => p.Qualifier, "title")
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "href", "/posts/edit" } })
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "Edit Title"))
        );

        // Assert
        cut.Find("a").Should().NotBeNull();
        cut.Find("a").TextContent.Should().Be("Edit Title");
    }

    [Fact]
    public void DoesNotRenderWithInvalidQualifier()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("edit", "Post", ["title", "content"])
            .Build();

        // Act
        var cut = RenderComponent<PrivilegeLink>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Subject, "Post")
            .Add(p => p.Action, "edit")
            .Add(p => p.Qualifier, "author")
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "href", "/posts/edit" } })
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "Edit Author"))
        );

        // Assert
        cut.Markup.Should().BeEmpty();
    }

    [Fact]
    public void ThrowsExceptionWhenNoPrivilegeContext()
    {
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            RenderComponent<PrivilegeLink>(parameters => parameters
                .Add(p => p.Subject, "Post")
                .Add(p => p.Action, "read")
                .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "href", "/posts" } })
                .Add(p => p.ChildContent, builder => builder.AddContent(0, "View Posts"))
            )
        );

        exception.Message.Should().Be("Component requires a cascading parameter of type PrivilegeContext.");
    }

    [Fact]
    public void InheritsNavLinkBehavior()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Build();

        // Act
        var cut = RenderComponent<PrivilegeLink>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Subject, "Post")
            .Add(p => p.Action, "read")
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object>
            {
                { "href", "/posts" },
                { "class", "nav-link" }
            })
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "View Posts"))
        );

        // Assert
        var link = cut.Find("a");
        link.GetAttribute("class").Should().Contain("nav-link");
        link.GetAttribute("href").Should().Be("/posts");
    }

    [Fact]
    public void WorksWithForbidRules()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Forbid("read", "Post", ["draft"])
            .Build();

        // Act - Should render for general read permission
        var cut1 = RenderComponent<PrivilegeLink>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Subject, "Post")
            .Add(p => p.Action, "read")
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "href", "/posts" } })
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "View Posts"))
        );

        // Act - Should NOT render for forbidden qualifier
        var cut2 = RenderComponent<PrivilegeLink>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Subject, "Post")
            .Add(p => p.Action, "read")
            .Add(p => p.Qualifier, "draft")
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "href", "/posts/draft" } })
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "View Draft Posts"))
        );

        // Assert
        cut1.Find("a").Should().NotBeNull();
        cut2.Markup.Should().BeEmpty();
    }

    [Fact]
    public void PermissionCheckUpdatesCorrectly()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Build();

        // Act
        var cut = RenderComponent<PrivilegeLink>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Subject, "Post")
            .Add(p => p.Action, "read")
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "href", "/posts" } })
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "View Posts"))
        );

        // Assert - Component should render when permission is granted
        cut.Find("a").Should().NotBeNull();
    }

    [Fact]
    public void PermissionCheckReturnsFalseWhenNoAccess()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("read", "User")
            .Build();

        // Act
        var cut = RenderComponent<PrivilegeLink>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Subject, "Post")
            .Add(p => p.Action, "read")
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "href", "/posts" } })
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "View Posts"))
        );

        // Assert - Component should not render when permission is denied
        cut.Markup.Should().BeEmpty();
    }

    [Fact]
    public void SupportsMatchParameter()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Build();

        // Act
        var cut = RenderComponent<PrivilegeLink>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Subject, "Post")
            .Add(p => p.Action, "read")
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "href", "/posts" } })
            .Add(p => p.Match, Microsoft.AspNetCore.Components.Routing.NavLinkMatch.All)
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "View Posts"))
        );

        // Assert
        cut.Find("a").Should().NotBeNull();
        cut.Find("a").TextContent.Should().Be("View Posts");
    }

    // Tests for Subjects parameter
    [Fact]
    public void SubjectsParameter_WithAllowedSubject_RendersLink()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Allow("read", "Comment")
            .Build();

        // Act
        var cut = RenderComponent<PrivilegeLink>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "read")
            .Add(p => p.Subjects, new[] { "Post", "User", "Comment" })
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "href", "/posts" } })
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "View Content"))
        );

        // Assert
        cut.Find("a").Should().NotBeNull();
        cut.Find("a").GetAttribute("href").Should().Be("/posts");
        cut.Find("a").TextContent.Should().Be("View Content");
    }

    [Fact]
    public void SubjectsParameter_WithNoAllowedSubjects_DoesNotRender()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("read", "Admin")
            .Build();

        // Act
        var cut = RenderComponent<PrivilegeLink>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "read")
            .Add(p => p.Subjects, new[] { "Post", "User", "Comment" })
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "href", "/posts" } })
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "View Content"))
        );

        // Assert
        cut.Markup.Should().BeEmpty();
    }

    [Fact]
    public void SubjectsParameter_TakesPrecedenceOverSubject()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Forbid("read", "User")
            .Build();

        // Act
        var cut = RenderComponent<PrivilegeLink>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "read")
            .Add(p => p.Subject, "User") // This should be ignored
            .Add(p => p.Subjects, new[] { "Post" }) // This takes precedence
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "href", "/posts" } })
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "View Posts"))
        );

        // Assert
        cut.Find("a").Should().NotBeNull();
        cut.Find("a").TextContent.Should().Be("View Posts");
    }

    [Fact]
    public void SubjectsParameter_WithEmptyCollection_UsesSubjectInstead()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("read", "User")
            .Build();

        // Act
        var cut = RenderComponent<PrivilegeLink>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "read")
            .Add(p => p.Subject, "User")
            .Add(p => p.Subjects, new string[0])
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "href", "/users" } })
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "View Users"))
        );

        // Assert
        cut.Find("a").Should().NotBeNull();
        cut.Find("a").GetAttribute("href").Should().Be("/users");
        cut.Find("a").TextContent.Should().Be("View Users");
    }

    [Fact]
    public void SubjectsParameter_WithSingleAllowedSubject_RendersLink()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Forbid("update", "Post")
            .Forbid("update", "User")
            .Allow("update", "Comment")
            .Build();

        // Act
        var cut = RenderComponent<PrivilegeLink>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "update")
            .Add(p => p.Subjects, new[] { "Post", "User", "Comment" })
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "href", "/comments/edit" } })
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "Edit Comments"))
        );

        // Assert
        cut.Find("a").Should().NotBeNull();
        cut.Find("a").GetAttribute("href").Should().Be("/comments/edit");
        cut.Find("a").TextContent.Should().Be("Edit Comments");
    }

    [Fact]
    public void SubjectsParameter_WithQualifier_ChecksWithQualifier()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("read", "Post", new[] { "title" })
            .Forbid("read", "Post", new[] { "content" })
            .Build();

        // Note: When using Subjects parameter, the qualifier is ignored 
        // because the Any extension method doesn't support qualifiers.
        // This test demonstrates that behavior.

        // Act - Should NOT render because Post subject with "read" action is not generally allowed
        // (only allowed with "title" qualifier, but qualifier is ignored when using Subjects)
        var cut1 = RenderComponent<PrivilegeLink>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "read")
            .Add(p => p.Subjects, new[] { "Post" })
            .Add(p => p.Qualifier, "title") // This qualifier is ignored when using Subjects
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "href", "/posts/title" } })
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "View Title"))
        );

        // Act - Using Subject (not Subjects) should respect the qualifier
        var cut2 = RenderComponent<PrivilegeLink>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "read")
            .Add(p => p.Subject, "Post") // Using Subject instead of Subjects
            .Add(p => p.Qualifier, "title")
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "href", "/posts/title" } })
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "View Title"))
        );

        // Assert
        cut1.Markup.Should().BeEmpty(); // Subjects ignores qualifier, Post+read not generally allowed
        cut2.Find("a").Should().NotBeNull(); // Subject respects qualifier, Post+read+title is allowed
        cut2.Find("a").TextContent.Should().Be("View Title");
    }

    [Fact]
    public void SubjectsParameter_WithMultipleSubjectsAndMixedPermissions_UsesAnyLogic()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Forbid("read", "User")
            .Forbid("read", "Admin")
            .Build();

        // Act
        var cut = RenderComponent<PrivilegeLink>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "read")
            .Add(p => p.Subjects, new[] { "User", "Admin", "Post" }) // Post is allowed, others are not
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "href", "/dashboard" } })
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "Dashboard"))
        );

        // Assert
        cut.Find("a").Should().NotBeNull();
        cut.Find("a").GetAttribute("href").Should().Be("/dashboard");
        cut.Find("a").TextContent.Should().Be("Dashboard");
    }

    [Fact]
    public void SubjectsParameter_WithWildcardPermissions_WorksCorrectly()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("read", PrivilegeSubjects.All) // Allow read on all subjects
            .Build();

        // Act
        var cut = RenderComponent<PrivilegeLink>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "read")
            .Add(p => p.Subjects, new[] { "Post", "User", "Comment", "Admin" })
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "href", "/everything" } })
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "View Everything"))
        );

        // Assert
        cut.Find("a").Should().NotBeNull();
        cut.Find("a").GetAttribute("href").Should().Be("/everything");
        cut.Find("a").TextContent.Should().Be("View Everything");
    }

    [Fact]
    public void SubjectsParameter_WithCustomAction_RendersWhenAllowed()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("edit", "Post")
            .Allow("delete", "Comment")
            .Build();

        // Act
        var cut = RenderComponent<PrivilegeLink>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "edit")
            .Add(p => p.Subjects, new[] { "Post", "User", "Comment" })
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "href", "/posts/edit" } })
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "Edit Content"))
        );

        // Assert
        cut.Find("a").Should().NotBeNull();
        cut.Find("a").GetAttribute("href").Should().Be("/posts/edit");
        cut.Find("a").TextContent.Should().Be("Edit Content");
    }

    [Fact]
    public void SubjectsParameter_WithDefaultReadAction_WorksCorrectly()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Build();

        // Act
        var cut = RenderComponent<PrivilegeLink>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Subjects, new[] { "Post", "User" })
            // Not explicitly setting Action, should default to "read"
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "href", "/posts" } })
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "View Posts"))
        );

        // Assert
        cut.Instance.Action.Should().Be("read");
        cut.Find("a").Should().NotBeNull();
        cut.Find("a").TextContent.Should().Be("View Posts");
    }

    [Fact]
    public void SubjectsParameter_InheritsNavLinkBehavior()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Build();

        // Act
        var cut = RenderComponent<PrivilegeLink>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "read")
            .Add(p => p.Subjects, new[] { "Post" })
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object>
            {
                { "href", "/posts" },
                { "class", "nav-link" }
            })
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "View Posts"))
        );

        // Assert
        var link = cut.Find("a");
        link.GetAttribute("class").Should().Contain("nav-link");
        link.GetAttribute("href").Should().Be("/posts");
        link.TextContent.Should().Be("View Posts");
    }

    [Fact]
    public void SubjectsParameter_WithForbidRules_RespectsProhibitions()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Allow("read", "Comment")
            .Forbid("read", "Post", new[] { "draft" })
            .Build();

        // Act - Should render for general read permission on Post/Comment subjects
        var cut1 = RenderComponent<PrivilegeLink>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "read")
            .Add(p => p.Subjects, new[] { "Post", "Comment" })
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "href", "/content" } })
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "View Content"))
        );

        // Act - Should render because when using Subjects, the qualifier is ignored
        // and Post is generally allowed for "read" action (forbid only applies to specific qualifier)
        var cut2 = RenderComponent<PrivilegeLink>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "read")
            .Add(p => p.Subjects, new[] { "Post" })
            .Add(p => p.Qualifier, "draft") // This qualifier is ignored when using Subjects
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "href", "/posts/draft" } })
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "View Draft Posts"))
        );

        // Act - Using Subject (not Subjects) should respect the qualifier and NOT render
        var cut3 = RenderComponent<PrivilegeLink>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "read")
            .Add(p => p.Subject, "Post") // Using Subject instead of Subjects
            .Add(p => p.Qualifier, "draft")
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "href", "/posts/draft" } })
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "View Draft Posts"))
        );

        // Assert
        cut1.Find("a").Should().NotBeNull();
        cut2.Find("a").Should().NotBeNull(); // Subjects ignores qualifier, so general Post+read is allowed
        cut3.Markup.Should().BeEmpty(); // Subject respects qualifier, Post+read+draft is forbidden
    }

    [Fact]
    public void SubjectsParameter_HasPermissionPropertyUpdatesCorrectly()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Build();

        // Act
        var cut = RenderComponent<PrivilegeLink>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "read")
            .Add(p => p.Subjects, new[] { "Post", "User" })
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "href", "/posts" } })
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "View Posts"))
        );

        // Assert - Component should render when permission is granted
        cut.Find("a").Should().NotBeNull();
    }

    [Fact]
    public void SubjectsParameter_HasPermissionPropertyReturnsFalseWhenNoAccess()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("read", "Admin")
            .Build();

        // Act
        var cut = RenderComponent<PrivilegeLink>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "read")
            .Add(p => p.Subjects, new[] { "Post", "User" })
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "href", "/posts" } })
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "View Posts"))
        );

        // Assert - Component should not render when permission is denied
        cut.Markup.Should().BeEmpty();
    }
}
