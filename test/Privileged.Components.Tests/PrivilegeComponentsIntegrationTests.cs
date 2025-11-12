using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Privileged.Components.Tests;

public class PrivilegeComponentsIntegrationTests : BunitContext
{
    [Fact]
    public void Components_Use_Default_Actions_When_Not_Specified()
    {
        var model = new TestModel { Name = "Test" };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", nameof(TestModel))  // Default read action
            .Allow("update", nameof(TestModel)) // Default update action
            .Build();

        var cut = Render<PrivilegeInputText>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Name)
            .Add(p => p.ValueExpression, () => model.Name)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, v => model.Name = v!))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.Name))
        );

        // Should render normally since both read and update are allowed
        var input = cut.Find("input");
        input.HasAttribute("readonly").Should().BeFalse();
        var type = input.GetAttribute("type") ?? "text";
        type.Should().Be("text");
    }

    [Fact]
    public void Components_Fall_Back_To_Model_Type_When_Subject_Not_Specified()
    {
        var model = new TestModel { Name = "Test" };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", nameof(TestModel))
            .Allow("update", nameof(TestModel))
            .Build();

        var cut = Render<PrivilegeInputText>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Name)
            .Add(p => p.ValueExpression, () => model.Name)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, v => model.Name = v!))
            // Intentionally not setting Subject - should fall back to model type
            .Add(p => p.Field, nameof(TestModel.Name))
        );

        // Should work since it falls back to model type name (TestModel)
        var input = cut.Find("input");
        input.HasAttribute("readonly").Should().BeFalse();
    }

    [Fact]
    public void Mixed_Permissions_Scenario()
    {
        var model = new TestModel { Name = "John", Age = 30, IsActive = true };
        var editContext = new EditContext(model);

        // Complex permission scenario: some fields editable, some readonly, some denied
        var ctx = new PrivilegeBuilder()
            .Allow("read", nameof(TestModel)) // Can read all fields
            .Allow("update", nameof(TestModel), [nameof(TestModel.Name)]) // Can only update Name
            .Build();

        // Test Text input (should be editable)
        var textCut = Render<PrivilegeInputText>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Name)
            .Add(p => p.ValueExpression, () => model.Name)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, v => model.Name = v!))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.Name))
        );

        var textInput = textCut.Find("input");
        textInput.HasAttribute("readonly").Should().BeFalse();

        // Test Number input (should be readonly)
        var numberCut = Render<PrivilegeInputNumber<int>>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Age)
            .Add(p => p.ValueExpression, () => model.Age)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<int>(this, v => model.Age = v))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.Age))
        );

        var numberInput = numberCut.Find("input");
        numberInput.HasAttribute("readonly").Should().BeTrue();

        // Test Checkbox (should be disabled)
        var checkboxCut = Render<PrivilegeInputCheckbox>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.IsActive)
            .Add(p => p.ValueExpression, () => model.IsActive)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<bool>(this, v => model.IsActive = v))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.IsActive))
        );

        var checkbox = checkboxCut.Find("input[type=checkbox]");
        checkbox.HasAttribute("disabled").Should().BeTrue();
    }
}
