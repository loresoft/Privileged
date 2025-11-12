using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Privileged.Components.Tests;

public class PrivilegeInputBooleanTests : BunitContext
{
    [Fact]
    public void Renders_Boolean_Select_When_Read_And_Update_Allowed()
    {
        var model = new TestModel { IsActive = true };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", nameof(TestModel), [nameof(TestModel.IsActive)])
            .Allow("update", nameof(TestModel), [nameof(TestModel.IsActive)])
            .Build();

        var cut = Render<PrivilegeInputBoolean<bool>>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.IsActive)
            .Add(p => p.ValueExpression, () => model.IsActive)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<bool>(this, v => model.IsActive = v))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.IsActive))
        );

        var select = cut.Find("select");
        select.HasAttribute("disabled").Should().BeFalse();

        var options = cut.FindAll("option");
        options.Count.Should().Be(2); // Only True and False for bool
        options[0].GetAttribute("value").Should().Be("true");
        options[0].TextContent.Should().Be("True");
        options[1].GetAttribute("value").Should().Be("false");
        options[1].TextContent.Should().Be("False");
    }

    [Fact]
    public void Renders_Nullable_Boolean_Select_When_Read_And_Update_Allowed()
    {
        var model = new TestModel { IsOptional = true };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", nameof(TestModel), [nameof(TestModel.IsOptional)])
            .Allow("update", nameof(TestModel), [nameof(TestModel.IsOptional)])
            .Build();

        var cut = Render<PrivilegeInputBoolean<bool?>>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.IsOptional)
            .Add(p => p.ValueExpression, () => model.IsOptional)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<bool?>(this, v => model.IsOptional = v))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.IsOptional))
        );

        var select = cut.Find("select");
        select.HasAttribute("disabled").Should().BeFalse();

        var options = cut.FindAll("option");
        options.Count.Should().Be(3); // Null, True, and False for bool?
        options[0].GetAttribute("value").Should().Be("");
        options[0].TextContent.Should().Be("- select -");
        options[1].GetAttribute("value").Should().Be("true");
        options[1].TextContent.Should().Be("True");
        options[2].GetAttribute("value").Should().Be("false");
        options[2].TextContent.Should().Be("False");
    }

    [Fact]
    public void Select_Disabled_When_Update_Denied_Bool()
    {
        var model = new TestModel { IsActive = false };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", nameof(TestModel), [nameof(TestModel.IsActive)])
            .Build(); // no update rule

        var cut = Render<PrivilegeInputBoolean<bool>>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.IsActive)
            .Add(p => p.ValueExpression, () => model.IsActive)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<bool>(this, v => model.IsActive = v))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.IsActive))
        );

        var select = cut.Find("select");
        select.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void Select_Disabled_When_Update_Denied_Nullable_Bool()
    {
        var model = new TestModel { IsOptional = null };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", nameof(TestModel), [nameof(TestModel.IsOptional)])
            .Build(); // no update rule

        var cut = Render<PrivilegeInputBoolean<bool?>>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.IsOptional)
            .Add(p => p.ValueExpression, () => model.IsOptional)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<bool?>(this, v => model.IsOptional = v))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.IsOptional))
        );

        var select = cut.Find("select");
        select.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void Masks_When_Read_Denied_Update_Denied_Bool()
    {
        var model = new TestModel { IsActive = true };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder().Build(); // no permissions

        var cut = Render<PrivilegeInputBoolean<bool>>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.IsActive)
            .Add(p => p.ValueExpression, () => model.IsActive)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<bool>(this, v => model.IsActive = v))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.IsActive))
        );

        // should render fallback password input
        var input = cut.Find("input[type=password]");
        input.Should().NotBeNull();
    }

    [Fact]
    public void Masks_When_Read_Denied_Update_Denied_Nullable_Bool()
    {
        var model = new TestModel { IsOptional = false };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder().Build(); // no permissions

        var cut = Render<PrivilegeInputBoolean<bool?>>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.IsOptional)
            .Add(p => p.ValueExpression, () => model.IsOptional)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<bool?>(this, v => model.IsOptional = v))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.IsOptional))
        );

        // should render fallback password input
        var input = cut.Find("input[type=password]");
        input.Should().NotBeNull();
    }

    [Fact]
    public void Uses_Custom_Labels_For_Bool()
    {
        var model = new TestModel { IsActive = true };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", nameof(TestModel), [nameof(TestModel.IsActive)])
            .Allow("update", nameof(TestModel), [nameof(TestModel.IsActive)])
            .Build();

        var cut = Render<PrivilegeInputBoolean<bool>>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.IsActive)
            .Add(p => p.ValueExpression, () => model.IsActive)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<bool>(this, v => model.IsActive = v))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.IsActive))
            .Add(p => p.TrueLabel, "Yes")
            .Add(p => p.FalseLabel, "No")
        );

        var options = cut.FindAll("option");
        options.Count.Should().Be(2);
        options[0].GetAttribute("value").Should().Be("true");
        options[0].TextContent.Should().Be("Yes");
        options[1].GetAttribute("value").Should().Be("false");
        options[1].TextContent.Should().Be("No");
    }

    [Fact]
    public void Uses_Custom_Labels_For_Nullable_Bool()
    {
        var model = new TestModel { IsOptional = null };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", nameof(TestModel), [nameof(TestModel.IsOptional)])
            .Allow("update", nameof(TestModel), [nameof(TestModel.IsOptional)])
            .Build();

        var cut = Render<PrivilegeInputBoolean<bool?>>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.IsOptional)
            .Add(p => p.ValueExpression, () => model.IsOptional)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<bool?>(this, v => model.IsOptional = v))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.IsOptional))
            .Add(p => p.NullLabel, "Not Set")
            .Add(p => p.TrueLabel, "Enabled")
            .Add(p => p.FalseLabel, "Disabled")
        );

        var options = cut.FindAll("option");
        options.Count.Should().Be(3);
        options[0].GetAttribute("value").Should().Be("");
        options[0].TextContent.Should().Be("Not Set");
        options[1].GetAttribute("value").Should().Be("true");
        options[1].TextContent.Should().Be("Enabled");
        options[2].GetAttribute("value").Should().Be("false");
        options[2].TextContent.Should().Be("Disabled");
    }

    [Fact]
    public void Throws_Exception_For_Invalid_Type()
    {
        var model = new TestModel { Name = "Test" };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", nameof(TestModel))
            .Allow("update", nameof(TestModel))
            .Build();

        // This should throw because string is not bool or bool?
        Action action = () => Render<PrivilegeInputBoolean<string>>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Name)
            .Add(p => p.ValueExpression, () => model.Name)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Name = v))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.Name))
        );

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Component only supports bool or bool? types.");
    }

    [Fact]
    public void Formats_Bool_Values_Correctly()
    {
        var model = new TestModel { IsActive = true };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", nameof(TestModel), [nameof(TestModel.IsActive)])
            .Allow("update", nameof(TestModel), [nameof(TestModel.IsActive)])
            .Build();

        var cut = Render<PrivilegeInputBoolean<bool>>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, true)
            .Add(p => p.ValueExpression, () => model.IsActive)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<bool>(this, v => model.IsActive = v))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.IsActive))
        );

        var select = cut.Find("select");
        select.GetAttribute("value").Should().Be("true");
    }

    [Fact]
    public void Works_Without_Explicit_Subject()
    {
        var model = new TestModel { IsActive = false };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", nameof(TestModel))
            .Allow("update", nameof(TestModel))
            .Build();

        var cut = Render<PrivilegeInputBoolean<bool>>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.IsActive)
            .Add(p => p.ValueExpression, () => model.IsActive)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<bool>(this, v => model.IsActive = v))
            // Intentionally not setting Subject - should fall back to model type
            .Add(p => p.Field, nameof(TestModel.IsActive))
        );

        // Should work since it falls back to model type name (TestModel)
        var select = cut.Find("select");
        select.HasAttribute("disabled").Should().BeFalse();
    }

    [Fact]
    public void Inherits_Select_Attributes()
    {
        var model = new TestModel { IsActive = true };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", nameof(TestModel), [nameof(TestModel.IsActive)])
            .Allow("update", nameof(TestModel), [nameof(TestModel.IsActive)])
            .Build();

        var cut = Render<PrivilegeInputBoolean<bool>>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.IsActive)
            .Add(p => p.ValueExpression, () => model.IsActive)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<bool>(this, v => model.IsActive = v))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.IsActive))
            .AddUnmatched("class", "custom-class")
            .AddUnmatched("data-testid", "boolean-select")
        );

        var select = cut.Find("select");
        select.GetAttribute("class").Should().Contain("custom-class");
        select.GetAttribute("data-testid").Should().Be("boolean-select");
    }
}
