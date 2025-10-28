using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Privileged.Components.Tests;

public class PrivilegeInputSelectTests : TestContext
{
    [Fact]
    public void Renders_Select_When_Read_And_Update_Allowed()
    {
        var model = new TestModel { Option = "B" };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", nameof(TestModel), [nameof(TestModel.Option)])
            .Allow("update", nameof(TestModel), [nameof(TestModel.Option)])
            .Build();

        var cut = RenderComponent<PrivilegeInputSelect<string>>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Option)
            .Add(p => p.ValueExpression, () => model.Option)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Option = v))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.Option))
            .Add(p => p.ChildContent, (RenderFragment)(builder =>
            {
                builder.OpenElement(0, "option");
                builder.AddAttribute(1, "value", "A");
                builder.AddContent(2, "Option A");
                builder.CloseElement();
                builder.OpenElement(3, "option");
                builder.AddAttribute(4, "value", "B");
                builder.AddContent(5, "Option B");
                builder.CloseElement();
            }))
        );

        cut.Find("select").HasAttribute("disabled").Should().BeFalse();
    }

    [Fact]
    public void Select_Disabled_When_Update_Denied()
    {
        var model = new TestModel { Option = "B" };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", nameof(TestModel), [nameof(TestModel.Option)])
            .Build();

        var cut = RenderComponent<PrivilegeInputSelect<string>>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Option)
            .Add(p => p.ValueExpression, () => model.Option)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Option = v))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.Option))
            .Add(p => p.ChildContent, (RenderFragment)(builder =>
            {
                builder.OpenElement(0, "option");
                builder.AddAttribute(1, "value", "A");
                builder.AddContent(2, "Option A");
                builder.CloseElement();
            }))
        );

        cut.Find("select").HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void Select_Masked_When_Read_Denied()
    {
        var model = new TestModel { Option = "B" };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder().Build(); // no permissions

        var cut = RenderComponent<PrivilegeInputSelect<string>>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Option)
            .Add(p => p.ValueExpression, () => model.Option)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Option = v))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.Option))
            .Add(p => p.ChildContent, (RenderFragment)(builder =>
            {
                builder.OpenElement(0, "option");
                builder.AddAttribute(1, "value", "A");
                builder.AddContent(2, "Option A");
                builder.CloseElement();
            }))
        );

        // should render fallback password input
        var input = cut.Find("input[type=password]");
        input.Should().NotBeNull();
    }

    [Fact]
    public void EmptySubject_AssumeAllPrivileges_RendersNormalSelect()
    {
        var model = new TestModel { Option = "B" };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Build(); // No specific permissions

        var cut = RenderComponent<PrivilegeInputSelect<string>>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Option)
            .Add(p => p.ValueExpression, () => model.Option)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Option = v))
            .Add(p => p.Subject, "") // Empty subject - should assume all privileges
            .Add(p => p.Field, nameof(TestModel.Option))
            .Add(p => p.ChildContent, (RenderFragment)(builder =>
            {
                builder.OpenElement(0, "option");
                builder.AddAttribute(1, "value", "A");
                builder.AddContent(2, "Option A");
                builder.CloseElement();
                builder.OpenElement(3, "option");
                builder.AddAttribute(4, "value", "B");
                builder.AddContent(5, "Option B");
                builder.CloseElement();
            }))
        );

        cut.Find("select").HasAttribute("disabled").Should().BeFalse();
    }

    [Fact]
    public void NullSubject_AssumeAllPrivileges_RendersNormalSelect()
    {
        var model = new TestModel { Option = "B" };

        var ctx = new PrivilegeBuilder()
            .Build(); // No specific permissions

        var cut = RenderComponent<PrivilegeInputSelect<string>>(ps => ps
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Option)
            .Add(p => p.ValueExpression, () => model.Option)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Option = v))
            .Add(p => p.Subject, (string?)null) // Null subject - should assume all privileges
            .Add(p => p.Field, nameof(TestModel.Option))
            .Add(p => p.ChildContent, (RenderFragment)(builder =>
            {
                builder.OpenElement(0, "option");
                builder.AddAttribute(1, "value", "A");
                builder.AddContent(2, "Option A");
                builder.CloseElement();
                builder.OpenElement(3, "option");
                builder.AddAttribute(4, "value", "B");
                builder.AddContent(5, "Option B");
                builder.CloseElement();
            }))
        );

        cut.Find("select").HasAttribute("disabled").Should().BeFalse();
    }

    [Fact]
    public void WhitespaceSubject_AssumeAllPrivileges_RendersNormalSelect()
    {
        var model = new TestModel { Option = "B" };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Build(); // No specific permissions

        var cut = RenderComponent<PrivilegeInputSelect<string>>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Option)
            .Add(p => p.ValueExpression, () => model.Option)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Option = v))
            .Add(p => p.Subject, "   ") // Whitespace subject - should assume all privileges
            .Add(p => p.Field, nameof(TestModel.Option))
            .Add(p => p.ChildContent, (RenderFragment)(builder =>
            {
                builder.OpenElement(0, "option");
                builder.AddAttribute(1, "value", "A");
                builder.AddContent(2, "Option A");
                builder.CloseElement();
                builder.OpenElement(3, "option");
                builder.AddAttribute(4, "value", "B");
                builder.AddContent(5, "Option B");
                builder.CloseElement();
            }))
        );

        cut.Find("select").HasAttribute("disabled").Should().BeFalse();
    }

    [Fact]
    public void EmptySubject_WithField_StillAssumeAllPrivileges()
    {
        var model = new TestModel { Option = "B" };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Build(); // No specific permissions

        var cut = RenderComponent<PrivilegeInputSelect<string>>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Option)
            .Add(p => p.ValueExpression, () => model.Option)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Option = v))
            .Add(p => p.Subject, "") // Empty subject
            .Add(p => p.Field, "RestrictedField") // Even with restricted field, should work
            .Add(p => p.ChildContent, (RenderFragment)(builder =>
            {
                builder.OpenElement(0, "option");
                builder.AddAttribute(1, "value", "A");
                builder.AddContent(2, "Option A");
                builder.CloseElement();
            }))
        );

        cut.Find("select").HasAttribute("disabled").Should().BeFalse();
    }

    [Fact]
    public void NoPrivilegeContext_AssumeAllPrivileges_RendersNormalSelect()
    {
        // When no PrivilegeContext is provided, component should assume all privileges
        var model = new TestModel { Option = "B" };
        var editContext = new EditContext(model);

        var cut = RenderComponent<PrivilegeInputSelect<string>>(ps => ps
            .AddCascadingValue(editContext)
            .Add(p => p.Value, model.Option)
            .Add(p => p.ValueExpression, () => model.Option)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Option = v))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.Option))
            .Add(p => p.ChildContent, (RenderFragment)(builder =>
            {
                builder.OpenElement(0, "option");
                builder.AddAttribute(1, "value", "A");
                builder.AddContent(2, "Option A");
                builder.CloseElement();
                builder.OpenElement(3, "option");
                builder.AddAttribute(4, "value", "B");
                builder.AddContent(5, "Option B");
                builder.CloseElement();
            }))
        );

        cut.Find("select").HasAttribute("disabled").Should().BeFalse();
    }

    [Fact]
    public void WithCustomActions_EmptySubject_AssumeAllPrivileges()
    {
        var model = new TestModel { Option = "B" };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Build(); // No specific permissions

        var cut = RenderComponent<PrivilegeInputSelect<string>>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Option)
            .Add(p => p.ValueExpression, () => model.Option)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Option = v))
            .Add(p => p.Subject, "") // Empty subject
            .Add(p => p.Field, nameof(TestModel.Option))
            .Add(p => p.ReadAction, "view") // Custom read action
            .Add(p => p.UpdateAction, "modify") // Custom update action
            .Add(p => p.ChildContent, (RenderFragment)(builder =>
            {
                builder.OpenElement(0, "option");
                builder.AddAttribute(1, "value", "A");
                builder.AddContent(2, "Option A");
                builder.CloseElement();
            }))
        );

        cut.Find("select").HasAttribute("disabled").Should().BeFalse();
    }
}
