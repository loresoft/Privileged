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
}
