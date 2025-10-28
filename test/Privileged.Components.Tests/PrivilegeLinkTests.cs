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

        // Assert - Should render as span when permission denied and HideForbidden is false (default)
        cut.Find("span").Should().NotBeNull();
        cut.Find("span").TextContent.Should().Be("View Posts");
        cut.FindAll("a").Should().BeEmpty();
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

        // Assert - Should render as span when permission denied and HideForbidden is false (default)
        cut.Find("span").Should().NotBeNull();
        cut.Find("span").TextContent.Should().Be("Edit Author");
        cut.FindAll("a").Should().BeEmpty();
    }

    [Fact]
    public void EmptySubject_AssumeAllPrivileges_RendersLink()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Build(); // No specific permissions

        // Act
        var cut = RenderComponent<PrivilegeLink>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Subject, "") // Empty subject - should assume all privileges
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
    public void NullSubject_AssumeAllPrivileges_RendersLink()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Build(); // No specific permissions

        // Act
        var cut = RenderComponent<PrivilegeLink>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Subject, (string?)null) // Null subject - should assume all privileges
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
    public void WhitespaceSubject_AssumeAllPrivileges_RendersLink()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Build(); // No specific permissions

        // Act
        var cut = RenderComponent<PrivilegeLink>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Subject, "   ") // Whitespace subject - should assume all privileges
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
    public void EmptySubject_WithQualifier_StillAssumeAllPrivileges()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Build(); // No specific permissions

        // Act
        var cut = RenderComponent<PrivilegeLink>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Subject, "") // Empty subject
            .Add(p => p.Action, "edit")
            .Add(p => p.Qualifier, "title") // Even with qualifier, should work
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "href", "/posts/edit" } })
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "Edit Title"))
        );

        // Assert
        cut.Find("a").Should().NotBeNull();
        cut.Find("a").GetAttribute("href").Should().Be("/posts/edit");
        cut.Find("a").TextContent.Should().Be("Edit Title");
    }

    [Fact]
    public void NoPrivilegeContext_AssumeAllPrivileges_RendersLink()
    {
        // When no PrivilegeContext is provided, component should assume all privileges
        var cut = RenderComponent<PrivilegeLink>(parameters => parameters
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
    public void NoPrivilegeContext_WithHideForbidden_DoesNotHide()
    {
        // When no PrivilegeContext is provided, all privileges are assumed, so link should not hide
        var cut = RenderComponent<PrivilegeLink>(parameters => parameters
            .Add(p => p.Subject, "Post")
            .Add(p => p.Action, "read")
            .Add(p => p.HideForbidden, true)
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "href", "/posts" } })
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "View Posts"))
        );

        // Assert
        cut.Find("a").Should().NotBeNull();
        cut.Find("a").GetAttribute("href").Should().Be("/posts");
        cut.Find("a").TextContent.Should().Be("View Posts");
        cut.FindAll("span").Should().BeEmpty();
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
        cut2.Find("span").Should().NotBeNull(); // Should render as span when no permission
        cut2.Find("span").TextContent.Should().Be("View Draft Posts");
    }

    [Fact]
    public void RendersSpanWhenNoPermission()
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
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object>
            {
                { "href", "/posts" },
                { "class", "nav-link" }
            })
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "View Posts"))
        );

        // Assert
        var span = cut.Find("span");
        span.Should().NotBeNull();
        span.GetAttribute("class").Should().Contain("nav-link");
        span.TextContent.Should().Be("View Posts");
        cut.FindAll("a").Should().BeEmpty();
    }

    [Fact]
    public void HiddenWhenPermissionDeniedAndHideForbiddenTrue()
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
            .Add(p => p.HideForbidden, true)
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "href", "/posts" } })
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "View Posts"))
        );

        // Assert
        cut.Markup.Should().BeEmpty();
        cut.FindAll("a").Should().BeEmpty();
        cut.FindAll("span").Should().BeEmpty();
    }

    [Fact]
    public void RendersLinkWhenPermissionGrantedAndHideForbiddenTrue()
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
            .Add(p => p.HideForbidden, true)
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "href", "/posts" } })
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "View Posts"))
        );

        // Assert
        cut.Find("a").Should().NotBeNull();
        cut.Find("a").GetAttribute("href").Should().Be("/posts");
        cut.Find("a").TextContent.Should().Be("View Posts");
        cut.FindAll("span").Should().BeEmpty();
    }

    [Fact]
    public void EmptySubjectWithHideForbiddenDoesNotHide()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Build(); // No specific permissions

        // Act
        var cut = RenderComponent<PrivilegeLink>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Subject, "") // Empty subject - should assume all privileges
            .Add(p => p.Action, "read")
            .Add(p => p.HideForbidden, true) // Should not hide because all privileges are assumed
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "href", "/posts" } })
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "View Posts"))
        );

        // Assert
        cut.Find("a").Should().NotBeNull();
        cut.Find("a").GetAttribute("href").Should().Be("/posts");
        cut.Find("a").TextContent.Should().Be("View Posts");
        cut.FindAll("span").Should().BeEmpty();
    }

    [Fact]
    public void NullSubjectWithHideForbiddenDoesNotHide()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Build(); // No specific permissions

        // Act
        var cut = RenderComponent<PrivilegeLink>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Subject, (string?)null) // Null subject - should assume all privileges
            .Add(p => p.Action, "read")
            .Add(p => p.HideForbidden, true) // Should not hide because all privileges are assumed
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "href", "/posts" } })
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "View Posts"))
        );

        // Assert
        cut.Find("a").Should().NotBeNull();
        cut.Find("a").GetAttribute("href").Should().Be("/posts");
        cut.Find("a").TextContent.Should().Be("View Posts");
        cut.FindAll("span").Should().BeEmpty();
    }

    [Fact]
    public void WhitespaceSubjectWithHideForbiddenDoesNotHide()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Build(); // No specific permissions

        // Act
        var cut = RenderComponent<PrivilegeLink>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Subject, "   ") // Whitespace subject - should assume all privileges
            .Add(p => p.Action, "read")
            .Add(p => p.HideForbidden, true) // Should not hide because all privileges are assumed
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "href", "/posts" } })
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "View Posts"))
        );

        // Assert
        cut.Find("a").Should().NotBeNull();
        cut.Find("a").GetAttribute("href").Should().Be("/posts");
        cut.Find("a").TextContent.Should().Be("View Posts");
        cut.FindAll("span").Should().BeEmpty();
    }

    [Fact]
    public void HideForbiddenWorksWithQualifiers()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("edit", "Post", ["title", "content"])
            .Build();

        // Act - Should render with allowed qualifier
        var cut1 = RenderComponent<PrivilegeLink>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Subject, "Post")
            .Add(p => p.Action, "edit")
            .Add(p => p.Qualifier, "title")
            .Add(p => p.HideForbidden, true)
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "href", "/posts/edit" } })
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "Edit Title"))
        );

        // Act - Should NOT render with forbidden qualifier
        var cut2 = RenderComponent<PrivilegeLink>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Subject, "Post")
            .Add(p => p.Action, "edit")
            .Add(p => p.Qualifier, "author") // not allowed
            .Add(p => p.HideForbidden, true)
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "href", "/posts/edit" } })
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "Edit Author"))
        );

        // Assert
        cut1.Find("a").Should().NotBeNull();
        cut1.Find("a").TextContent.Should().Be("Edit Title");

        cut2.Markup.Should().BeEmpty();
        cut2.FindAll("a").Should().BeEmpty();
        cut2.FindAll("span").Should().BeEmpty();
    }

    [Fact]
    public void HideForbiddenWorksWithForbidRules()
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
            .Add(p => p.HideForbidden, true)
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "href", "/posts" } })
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "View Posts"))
        );

        // Act - Should be hidden for forbidden qualifier
        var cut2 = RenderComponent<PrivilegeLink>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Subject, "Post")
            .Add(p => p.Action, "read")
            .Add(p => p.Qualifier, "draft")
            .Add(p => p.HideForbidden, true)
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "href", "/posts/draft" } })
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "View Draft Posts"))
        );

        // Assert
        cut1.Find("a").Should().NotBeNull();
        cut1.Find("a").TextContent.Should().Be("View Posts");

        cut2.Markup.Should().BeEmpty();
        cut2.FindAll("a").Should().BeEmpty();
        cut2.FindAll("span").Should().BeEmpty();
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
            .Add(p => p.Subjects, ["Post", "User", "Comment"])
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "href", "/posts" } })
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "View Content"))
        );

        // Assert - Should render as span when permission denied and HideForbidden is false (default)
        cut.Find("span").Should().NotBeNull();
        cut.Find("span").TextContent.Should().Be("View Content");
        cut.FindAll("a").Should().BeEmpty();
    }

    [Fact]
    public void SubjectsParameterWithHideForbiddenHidesWhenNoPermission()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("read", "Admin")
            .Build();

        // Act
        var cut = RenderComponent<PrivilegeLink>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "read")
            .Add(p => p.Subjects, ["Post", "User", "Comment"])
            .Add(p => p.HideForbidden, true)
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "href", "/posts" } })
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "View Content"))
        );

        // Assert
        cut.Markup.Should().BeEmpty();
        cut.FindAll("a").Should().BeEmpty();
        cut.FindAll("span").Should().BeEmpty();
    }

    [Fact]
    public void SubjectsParameterWithHideForbiddenRendersWhenPermissionGranted()
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
            .Add(p => p.Subjects, ["Post", "User", "Comment"])
            .Add(p => p.HideForbidden, true)
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "href", "/posts" } })
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "View Content"))
        );

        // Assert
        cut.Find("a").Should().NotBeNull();
        cut.Find("a").GetAttribute("href").Should().Be("/posts");
        cut.Find("a").TextContent.Should().Be("View Content");
        cut.FindAll("span").Should().BeEmpty();
    }

    [Fact]
    public void HideForbiddenDefaultValueIsFalse()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("read", "User")
            .Build();

        // Act - Not setting HideForbidden (should default to false)
        var cut = RenderComponent<PrivilegeLink>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Subject, "Post")
            .Add(p => p.Action, "read")
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "href", "/posts" } })
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "View Posts"))
        );

        // Assert - Should render as span, not be hidden
        cut.Find("span").Should().NotBeNull();
        cut.Find("span").TextContent.Should().Be("View Posts");
        cut.FindAll("a").Should().BeEmpty();
        cut.Instance.HideForbidden.Should().BeFalse();
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

        // Assert - Component should render as span when permission is denied and HideForbidden is false (default)
        cut.Find("span").Should().NotBeNull();
        cut.Find("span").TextContent.Should().Be("View Posts");
        cut.FindAll("a").Should().BeEmpty();
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
            .Add(p => p.Subjects, ["Post", "User"])
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "href", "/posts" } })
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "View Posts"))
        );

        // Assert - Component should render as span when permission is denied and HideForbidden is false (default)
        cut.Find("span").Should().NotBeNull();
        cut.Find("span").TextContent.Should().Be("View Posts");
        cut.FindAll("a").Should().BeEmpty();
    }

    [Fact]
    public void SubjectsParameter_WithForbidRules_RespectsProhibitions()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Allow("read", "Comment")
            .Forbid("read", "Post", ["draft"])
            .Build();

        // Act - Should render for general read permission on Post/Comment subjects
        var cut1 = RenderComponent<PrivilegeLink>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "read")
            .Add(p => p.Subjects, ["Post", "Comment"])
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "href", "/content" } })
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "View Content"))
        );

        // Act - Should render because when using Subjects, the qualifier is ignored
        // and Post is generally allowed for "read" action (forbid only applies to specific qualifier)
        var cut2 = RenderComponent<PrivilegeLink>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "read")
            .Add(p => p.Subjects, ["Post"])
            .Add(p => p.Qualifier, "draft") // This qualifier is ignored when using Subjects
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "href", "/posts/draft" } })
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "View Draft Posts"))
        );

        // Act - Using Subject (not Subjects) should respect the qualifier and render as span
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
        cut3.Find("span").Should().NotBeNull(); // Subject respects qualifier, Post+read+draft is forbidden, renders as span
        cut3.Find("span").TextContent.Should().Be("View Draft Posts");
        cut3.FindAll("a").Should().BeEmpty();
    }
}
