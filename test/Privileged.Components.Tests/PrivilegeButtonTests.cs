using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;

namespace Privileged.Components.Tests;

public class PrivilegeButtonTests : TestContext
{
    [Fact]
    public void Renders_Button_When_Permission_Allowed()
    {
        var ctx = new PrivilegeBuilder()
            .Allow("create", "Post")
            .Build();

        var cut = RenderComponent<PrivilegeButton>(ps => ps
            .AddCascadingValue(ctx)
            .Add(p => p.Action, "create")
            .Add(p => p.Subject, "Post")
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "Create Post"))
        );

        var button = cut.Find("button");
        button.Should().NotBeNull();
        button.HasAttribute("disabled").Should().BeFalse();
        button.TextContent.Should().Be("Create Post");
    }

    [Fact]
    public void Button_Disabled_When_Permission_Denied()
    {
        var ctx = new PrivilegeBuilder()
            .Build(); // no permissions

        var cut = RenderComponent<PrivilegeButton>(ps => ps
            .AddCascadingValue(ctx)
            .Add(p => p.Action, "create")
            .Add(p => p.Subject, "Post")
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "Create Post"))
        );

        var button = cut.Find("button");
        button.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void Button_Hidden_When_Permission_Denied_And_HideForbidden_True()
    {
        var ctx = new PrivilegeBuilder()
            .Build(); // no permissions

        var cut = RenderComponent<PrivilegeButton>(ps => ps
            .AddCascadingValue(ctx)
            .Add(p => p.Action, "create")
            .Add(p => p.Subject, "Post")
            .Add(p => p.HideForbidden, true)
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "Create Post"))
        );

        cut.FindAll("button").Should().BeEmpty();
    }

    [Fact]
    public void Button_Disabled_When_Disabled_Property_True()
    {
        var ctx = new PrivilegeBuilder()
            .Allow("create", "Post")
            .Build();

        var cut = RenderComponent<PrivilegeButton>(ps => ps
            .AddCascadingValue(ctx)
            .Add(p => p.Action, "create")
            .Add(p => p.Subject, "Post")
            .Add(p => p.Disabled, true)
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "Create Post"))
        );

        var button = cut.Find("button");
        button.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void Button_Disabled_When_Busy_True()
    {
        var ctx = new PrivilegeBuilder()
            .Allow("create", "Post")
            .Build();

        var cut = RenderComponent<PrivilegeButton>(ps => ps
            .AddCascadingValue(ctx)
            .Add(p => p.Action, "create")
            .Add(p => p.Subject, "Post")
            .Add(p => p.Busy, true)
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "Create Post"))
        );

        var button = cut.Find("button");
        button.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void Button_Shows_Default_Busy_Text_When_Busy()
    {
        var ctx = new PrivilegeBuilder()
            .Allow("create", "Post")
            .Build();

        var cut = RenderComponent<PrivilegeButton>(ps => ps
            .AddCascadingValue(ctx)
            .Add(p => p.Action, "create")
            .Add(p => p.Subject, "Post")
            .Add(p => p.Busy, true)
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "Create Post"))
        );

        var button = cut.Find("button");
        button.TextContent.Should().Be("Processing...");
    }

    [Fact]
    public void Button_Shows_Custom_Busy_Text_When_Busy()
    {
        var ctx = new PrivilegeBuilder()
            .Allow("create", "Post")
            .Build();

        var cut = RenderComponent<PrivilegeButton>(ps => ps
            .AddCascadingValue(ctx)
            .Add(p => p.Action, "create")
            .Add(p => p.Subject, "Post")
            .Add(p => p.Busy, true)
            .Add(p => p.BusyText, "Creating...")
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "Create Post"))
        );

        var button = cut.Find("button");
        button.TextContent.Should().Be("Creating...");
    }

    [Fact]
    public void Button_Shows_Custom_Busy_Template_When_Busy()
    {
        var ctx = new PrivilegeBuilder()
            .Allow("create", "Post")
            .Build();

        var cut = RenderComponent<PrivilegeButton>(ps => ps
            .AddCascadingValue(ctx)
            .Add(p => p.Action, "create")
            .Add(p => p.Subject, "Post")
            .Add(p => p.Busy, true)
            .Add(p => p.BusyTemplate, builder => builder.AddMarkupContent(0, "<span>Working...</span>"))
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "Create Post"))
        );

        var button = cut.Find("button");
        button.InnerHtml.Should().Contain("<span>Working...</span>");
    }

    [Fact]
    public void Button_Has_Type_Attribute_When_Trigger_Provided()
    {
        var ctx = new PrivilegeBuilder()
            .Allow("create", "Post")
            .Build();

        var callback = EventCallback.Factory.Create(this, () => { });

        var cut = RenderComponent<PrivilegeButton>(ps => ps
            .AddCascadingValue(ctx)
            .Add(p => p.Action, "create")
            .Add(p => p.Subject, "Post")
            .Add(p => p.Trigger, callback)
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "Create Post"))
        );

        var button = cut.Find("button");
        button.GetAttribute("type").Should().Be("button");
    }

    [Fact]
    public void Button_Click_Invokes_Trigger()
    {
        var ctx = new PrivilegeBuilder()
            .Allow("create", "Post")
            .Build();

        var triggered = false;
        var callback = EventCallback.Factory.Create(this, () => triggered = true);

        var cut = RenderComponent<PrivilegeButton>(ps => ps
            .AddCascadingValue(ctx)
            .Add(p => p.Action, "create")
            .Add(p => p.Subject, "Post")
            .Add(p => p.Trigger, callback)
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "Create Post"))
        );

        var button = cut.Find("button");
        button.Click();

        triggered.Should().BeTrue();
    }

    [Fact]
    public async Task Button_Shows_Busy_State_During_Async_Trigger_Execution()
    {
        var ctx = new PrivilegeBuilder()
            .Allow("create", "Post")
            .Build();

        var tcs = new TaskCompletionSource();
        var callback = EventCallback.Factory.Create(this, () => tcs.Task);

        var cut = RenderComponent<PrivilegeButton>(ps => ps
            .AddCascadingValue(ctx)
            .Add(p => p.Action, "create")
            .Add(p => p.Subject, "Post")
            .Add(p => p.Trigger, callback)
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "Create Post"))
        );

        var button = cut.Find("button");

        // Before click - should show normal content
        button.TextContent.Should().Be("Create Post");
        button.HasAttribute("disabled").Should().BeFalse();

        // Click the button to start async operation
        button.Click();

        // During execution - should show busy state
        button = cut.Find("button");
        button.TextContent.Should().Be("Processing...");
        button.HasAttribute("disabled").Should().BeTrue();

        // Complete the async operation
        tcs.SetResult();
        await cut.InvokeAsync(() => { }); // Allow component to re-render

        // After completion - should return to normal state
        button = cut.Find("button");
        button.TextContent.Should().Be("Create Post");
        button.HasAttribute("disabled").Should().BeFalse();
    }

    [Fact]
    public void Button_Works_With_Qualifier_Parameter()
    {
        var ctx = new PrivilegeBuilder()
            .Allow("update", "Post", ["title"])
            .Build();

        var cut = RenderComponent<PrivilegeButton>(ps => ps
            .AddCascadingValue(ctx)
            .Add(p => p.Action, "update")
            .Add(p => p.Subject, "Post")
            .Add(p => p.Qualifier, "title")
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "Update Title"))
        );

        var button = cut.Find("button");
        button.HasAttribute("disabled").Should().BeFalse();
    }

    [Fact]
    public void Button_Disabled_When_Qualifier_Not_Allowed()
    {
        var ctx = new PrivilegeBuilder()
            .Allow("update", "Post", ["title"])
            .Build();

        var cut = RenderComponent<PrivilegeButton>(ps => ps
            .AddCascadingValue(ctx)
            .Add(p => p.Action, "update")
            .Add(p => p.Subject, "Post")
            .Add(p => p.Qualifier, "content") // not allowed
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "Update Content"))
        );

        var button = cut.Find("button");
        button.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void Button_Applies_Additional_Attributes()
    {
        var ctx = new PrivilegeBuilder()
            .Allow("create", "Post")
            .Build();

        var cut = RenderComponent<PrivilegeButton>(ps => ps
            .AddCascadingValue(ctx)
            .Add(p => p.Action, "create")
            .Add(p => p.Subject, "Post")
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "Create Post"))
            .AddUnmatched("class", "btn btn-primary")
            .AddUnmatched("data-testid", "create-button")
        );

        var button = cut.Find("button");
        button.GetAttribute("class").Should().Be("btn btn-primary");
        button.GetAttribute("data-testid").Should().Be("create-button");
    }

    [Fact]
    public void Throws_When_No_PrivilegeContext_Provided()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            RenderComponent<PrivilegeButton>(ps => ps
                .Add(p => p.Action, "create")
                .Add(p => p.Subject, "Post")
                .Add(p => p.ChildContent, builder => builder.AddContent(0, "Create Post"))
            );
        });

        exception.Message.Should().Contain("PrivilegeContext");
    }

    [Fact]
    public void Button_Without_Trigger_Does_Not_Have_Type_Attribute()
    {
        var ctx = new PrivilegeBuilder()
            .Allow("create", "Post")
            .Build();

        var cut = RenderComponent<PrivilegeButton>(ps => ps
            .AddCascadingValue(ctx)
            .Add(p => p.Action, "create")
            .Add(p => p.Subject, "Post")
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "Create Post"))
        );

        var button = cut.Find("button");
        button.HasAttribute("type").Should().BeFalse();
    }

    [Fact]
    public void Button_Multiple_Disabled_Conditions_All_Apply()
    {
        var ctx = new PrivilegeBuilder()
            .Build(); // no permissions

        var cut = RenderComponent<PrivilegeButton>(ps => ps
            .AddCascadingValue(ctx)
            .Add(p => p.Action, "create")
            .Add(p => p.Subject, "Post")
            .Add(p => p.Disabled, true)
            .Add(p => p.Busy, true)
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "Create Post"))
        );

        // Should be disabled due to multiple conditions:
        // 1. No permission
        // 2. Disabled = true
        // 3. Busy = true
        var button = cut.Find("button");
        button.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void Button_Shows_BusyTemplate_Over_BusyText_When_Both_Provided()
    {
        var ctx = new PrivilegeBuilder()
            .Allow("create", "Post")
            .Build();

        var cut = RenderComponent<PrivilegeButton>(ps => ps
            .AddCascadingValue(ctx)
            .Add(p => p.Action, "create")
            .Add(p => p.Subject, "Post")
            .Add(p => p.Busy, true)
            .Add(p => p.BusyText, "Should not see this")
            .Add(p => p.BusyTemplate, builder => builder.AddMarkupContent(0, "<em>Custom Template</em>"))
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "Create Post"))
        );

        var button = cut.Find("button");
        button.InnerHtml.Should().Contain("<em>Custom Template</em>");
        button.TextContent.Should().NotContain("Should not see this");
    }

    [Fact]
    public async Task Button_Handles_Exception_In_Trigger_Gracefully()
    {
        var ctx = new PrivilegeBuilder()
            .Allow("create", "Post")
            .Build();

        var callback = EventCallback.Factory.Create(this, () => throw new InvalidOperationException("Test exception"));

        var cut = RenderComponent<PrivilegeButton>(ps => ps
            .AddCascadingValue(ctx)
            .Add(p => p.Action, "create")
            .Add(p => p.Subject, "Post")
            .Add(p => p.Trigger, callback)
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "Create Post"))
        );

        var button = cut.Find("button");

        // Click should not crash the component even if trigger throws
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            button.Click();
            await cut.InvokeAsync(() => { }); // Process the exception
        });

        exception.Message.Should().Be("Test exception");

        // Button should return to normal state after exception
        button = cut.Find("button");
        button.HasAttribute("disabled").Should().BeFalse();
        button.TextContent.Should().Be("Create Post");
    }

    [Fact]
    public void Button_Empty_ChildContent_Renders_Empty_Button()
    {
        var ctx = new PrivilegeBuilder()
            .Allow("create", "Post")
            .Build();

        var cut = RenderComponent<PrivilegeButton>(ps => ps
            .AddCascadingValue(ctx)
            .Add(p => p.Action, "create")
            .Add(p => p.Subject, "Post")
        );

        var button = cut.Find("button");
        button.TextContent.Should().BeEmpty();
    }
}
