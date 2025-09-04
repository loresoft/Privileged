using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Privileged.Components.Tests;

public class PrivilegeInputCheckboxTests : TestContext
{
    [Fact]
    public void Renders_Checkbox_When_Read_And_Update_Allowed()
    {
        var model = new TestModel { IsActive = true };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", nameof(TestModel), [nameof(TestModel.IsActive)])
            .Allow("update", nameof(TestModel), [nameof(TestModel.IsActive)])
            .Build();

        var cut = RenderComponent<PrivilegeInputCheckbox>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.IsActive)
            .Add(p => p.ValueExpression, () => model.IsActive)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<bool>(this, v => model.IsActive = v))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.IsActive))
        );

        var input = cut.Find("input[type=checkbox]");
        input.HasAttribute("disabled").Should().BeFalse();
    }

    [Fact]
    public void Sets_Disabled_When_Update_Denied()
    {
        var model = new TestModel { IsActive = true };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", nameof(TestModel), [nameof(TestModel.IsActive)])
            .Build(); // no update rule

        var cut = RenderComponent<PrivilegeInputCheckbox>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.IsActive)
            .Add(p => p.ValueExpression, () => model.IsActive)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<bool>(this, v => model.IsActive = v))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.IsActive))
        );

        var input = cut.Find("input[type=checkbox]");
        input.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void EmptySubject_AssumeAllPrivileges_RendersEnabledCheckbox()
    {
        var model = new TestModel { IsActive = true };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Build(); // No specific permissions

        var cut = RenderComponent<PrivilegeInputCheckbox>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.IsActive)
            .Add(p => p.ValueExpression, () => model.IsActive)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<bool>(this, v => model.IsActive = v))
            .Add(p => p.Subject, "") // Empty subject - should assume all privileges
            .Add(p => p.Field, nameof(TestModel.IsActive))
        );

        var input = cut.Find("input[type=checkbox]");
        input.HasAttribute("disabled").Should().BeFalse();
    }

    [Fact]
    public void NullSubject_AssumeAllPrivileges_RendersEnabledCheckbox()
    {
        var model = new TestModel { IsActive = true };

        var ctx = new PrivilegeBuilder()
            .Build(); // No specific permissions

        var cut = RenderComponent<PrivilegeInputCheckbox>(ps => ps
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.IsActive)
            .Add(p => p.ValueExpression, () => model.IsActive)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<bool>(this, v => model.IsActive = v))
            .Add(p => p.Subject, (string?)null) // Null subject - should assume all privileges
            .Add(p => p.Field, nameof(TestModel.IsActive))
        );

        var input = cut.Find("input[type=checkbox]");
        input.HasAttribute("disabled").Should().BeFalse();
    }

    [Fact]
    public void WhitespaceSubject_AssumeAllPrivileges_RendersEnabledCheckbox()
    {
        var model = new TestModel { IsActive = true };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Build(); // No specific permissions

        var cut = RenderComponent<PrivilegeInputCheckbox>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.IsActive)
            .Add(p => p.ValueExpression, () => model.IsActive)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<bool>(this, v => model.IsActive = v))
            .Add(p => p.Subject, "   ") // Whitespace subject - should assume all privileges
            .Add(p => p.Field, nameof(TestModel.IsActive))
        );

        var input = cut.Find("input[type=checkbox]");
        input.HasAttribute("disabled").Should().BeFalse();
    }

    [Fact]
    public void EmptySubject_WithField_StillAssumeAllPrivileges()
    {
        var model = new TestModel { IsActive = true };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Build(); // No specific permissions

        var cut = RenderComponent<PrivilegeInputCheckbox>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.IsActive)
            .Add(p => p.ValueExpression, () => model.IsActive)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<bool>(this, v => model.IsActive = v))
            .Add(p => p.Subject, "") // Empty subject
            .Add(p => p.Field, "RestrictedField") // Even with restricted field, should work
        );

        var input = cut.Find("input[type=checkbox]");
        input.HasAttribute("disabled").Should().BeFalse();
    }

    [Fact]
    public void Throws_When_No_PrivilegeContext_Provided()
    {
        var model = new TestModel { IsActive = true };
        var editContext = new EditContext(model);

        var exception = Assert.Throws<System.InvalidOperationException>(() =>
        {
            RenderComponent<PrivilegeInputCheckbox>(ps => ps
                .AddCascadingValue(editContext)
                .Add(p => p.Value, model.IsActive)
                .Add(p => p.ValueExpression, () => model.IsActive)
                .Add(p => p.ValueChanged, EventCallback.Factory.Create<bool>(this, v => model.IsActive = v))
                .Add(p => p.Subject, nameof(TestModel))
                .Add(p => p.Field, nameof(TestModel.IsActive))
            );
        });

        exception.Message.Should().Contain("PrivilegeContext");
    }

    [Fact]
    public void WithCustomActions_EmptySubject_AssumeAllPrivileges()
    {
        var model = new TestModel { IsActive = true };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Build(); // No specific permissions

        var cut = RenderComponent<PrivilegeInputCheckbox>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.IsActive)
            .Add(p => p.ValueExpression, () => model.IsActive)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<bool>(this, v => model.IsActive = v))
            .Add(p => p.Subject, "") // Empty subject
            .Add(p => p.Field, nameof(TestModel.IsActive))
            .Add(p => p.ReadAction, "view") // Custom read action
            .Add(p => p.UpdateAction, "modify") // Custom update action
        );

        var input = cut.Find("input[type=checkbox]");
        input.HasAttribute("disabled").Should().BeFalse();
    }
}
