using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Privileged.Components.Tests;

public class PrivilegeInputPasswordTests : BunitContext
{
    [Fact]
    public void Renders_Password_Input_When_Read_And_Update_Allowed()
    {
        var model = new TestModel { Name = "SecretPassword" };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", nameof(TestModel), [nameof(TestModel.Name)])
            .Allow("update", nameof(TestModel), [nameof(TestModel.Name)])
            .Build();

        var cut = Render<PrivilegeInputPassword>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Name)
            .Add(p => p.ValueExpression, () => model.Name)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, v => model.Name = v!))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.Name))
        );

        var input = cut.Find("input");
        input.GetAttribute("type").Should().Be("password");
        input.HasAttribute("readonly").Should().BeFalse();
        input.HasAttribute("disabled").Should().BeFalse();
    }

    [Fact]
    public void Sets_Readonly_When_Read_Allowed_Update_Denied()
    {
        var model = new TestModel { Name = "SecretPassword" };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", nameof(TestModel), [nameof(TestModel.Name)])
            .Build(); // no update rule

        var cut = Render<PrivilegeInputPassword>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Name)
            .Add(p => p.ValueExpression, () => model.Name)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, v => model.Name = v!))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.Name))
        );

        var input = cut.Find("input");
        input.GetAttribute("type").Should().Be("password");
        input.HasAttribute("readonly").Should().BeTrue();
    }

    [Fact]
    public void Disables_When_Read_Denied_Update_Denied()
    {
        var model = new TestModel { Name = "SecretPassword" };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder().Build(); // No permissions

        var cut = Render<PrivilegeInputPassword>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Name)
            .Add(p => p.ValueExpression, () => model.Name)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, v => model.Name = v!))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.Name))
        );

        var input = cut.Find("input[type=password]");
        input.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void EmptySubject_AssumeAllPrivileges_RendersEditableInput()
    {
        var model = new TestModel { Name = "SecretPassword" };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Build(); // No specific permissions

        var cut = Render<PrivilegeInputPassword>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Name)
            .Add(p => p.ValueExpression, () => model.Name)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, v => model.Name = v!))
            .Add(p => p.Subject, "") // Empty subject
            .Add(p => p.Field, nameof(TestModel.Name))
        );

        var input = cut.Find("input");
        input.GetAttribute("type").Should().Be("password");
        input.HasAttribute("readonly").Should().BeFalse();
    }

    [Fact]
    public void NullSubject_AssumeAllPrivileges_RendersEditableInput()
    {
        var model = new TestModel { Name = "SecretPassword" };

        var ctx = new PrivilegeBuilder()
            .Build(); // No specific permissions

        var cut = Render<PrivilegeInputPassword>(ps => ps
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Name)
            .Add(p => p.ValueExpression, () => model.Name)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, v => model.Name = v!))
            .Add(p => p.Subject, (string?)null) // Null subject
            .Add(p => p.Field, nameof(TestModel.Name))
        );

        var input = cut.Find("input");
        input.GetAttribute("type").Should().Be("password");
        input.HasAttribute("readonly").Should().BeFalse();
    }

    [Fact]
    public void CustomActions_UsesCustomActionNames()
    {
        var model = new TestModel { Name = "SecretPassword" };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("view", nameof(TestModel), [nameof(TestModel.Name)])
            .Allow("edit", nameof(TestModel), [nameof(TestModel.Name)])
            .Build();

        var cut = Render<PrivilegeInputPassword>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Name)
            .Add(p => p.ValueExpression, () => model.Name)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, v => model.Name = v!))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.Name))
            .Add(p => p.ReadAction, "view")
            .Add(p => p.UpdateAction, "edit")
        );

        var input = cut.Find("input");
        input.GetAttribute("type").Should().Be("password");
        input.HasAttribute("readonly").Should().BeFalse();
    }

    [Fact]
    public void CustomActions_ReadDenied_RendersDisabledInput()
    {
        var model = new TestModel { Name = "SecretPassword" };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("edit", nameof(TestModel), [nameof(TestModel.Name)])
            .Build(); // view action not allowed

        var cut = Render<PrivilegeInputPassword>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Name)
            .Add(p => p.ValueExpression, () => model.Name)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, v => model.Name = v!))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.Name))
            .Add(p => p.ReadAction, "view")
            .Add(p => p.UpdateAction, "edit")
        );

        var input = cut.Find("input[type=password]");
        input.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void CustomActions_UpdateDenied_RendersReadonlyInput()
    {
        var model = new TestModel { Name = "SecretPassword" };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("view", nameof(TestModel), [nameof(TestModel.Name)])
            .Build(); // edit action not allowed

        var cut = Render<PrivilegeInputPassword>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Name)
            .Add(p => p.ValueExpression, () => model.Name)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, v => model.Name = v!))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.Name))
            .Add(p => p.ReadAction, "view")
            .Add(p => p.UpdateAction, "edit")
        );

        var input = cut.Find("input");
        input.GetAttribute("type").Should().Be("password");
        input.HasAttribute("readonly").Should().BeTrue();
    }

    [Fact]
    public void NoPrivilegeContext_AssumeAllPrivileges_RendersEditableInput()
    {
        // When no PrivilegeContext is provided, component should assume all privileges
        var model = new TestModel { Name = "SecretPassword" };
        var editContext = new EditContext(model);

        var cut = Render<PrivilegeInputPassword>(ps => ps
            .AddCascadingValue(editContext)
            // No PrivilegeContext provided
            .Add(p => p.Value, model.Name)
            .Add(p => p.ValueExpression, () => model.Name)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, v => model.Name = v!))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.Name))
        );

        var input = cut.Find("input");
        input.GetAttribute("type").Should().Be("password");
        input.HasAttribute("readonly").Should().BeFalse();
        input.HasAttribute("disabled").Should().BeFalse();
    }

    [Fact]
    public void Togglable_False_DoesNotRenderToggleButton()
    {
        var model = new TestModel { Name = "SecretPassword" };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", nameof(TestModel), [nameof(TestModel.Name)])
            .Allow("update", nameof(TestModel), [nameof(TestModel.Name)])
            .Build();

        var cut = Render<PrivilegeInputPassword>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Name)
            .Add(p => p.ValueExpression, () => model.Name)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, v => model.Name = v!))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.Name))
            .Add(p => p.Togglable, false)
        );

        var buttons = cut.FindAll("button");
        buttons.Should().BeEmpty();
    }

    [Fact]
    public void Togglable_True_RendersToggleButton()
    {
        var model = new TestModel { Name = "SecretPassword" };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", nameof(TestModel), [nameof(TestModel.Name)])
            .Allow("update", nameof(TestModel), [nameof(TestModel.Name)])
            .Build();

        var cut = Render<PrivilegeInputPassword>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Name)
            .Add(p => p.ValueExpression, () => model.Name)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, v => model.Name = v!))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.Name))
            .Add(p => p.Togglable, true)
        );

        var button = cut.Find("button");
        button.Should().NotBeNull();
        button.GetAttribute("type").Should().Be("button");
        button.GetAttribute("title").Should().Be("Toggle Visibility");
    }

    [Fact]
    public void Togglable_True_WrapsInContainer()
    {
        var model = new TestModel { Name = "SecretPassword" };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", nameof(TestModel), [nameof(TestModel.Name)])
            .Allow("update", nameof(TestModel), [nameof(TestModel.Name)])
            .Build();

        var cut = Render<PrivilegeInputPassword>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Name)
            .Add(p => p.ValueExpression, () => model.Name)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, v => model.Name = v!))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.Name))
            .Add(p => p.Togglable, true)
        );

        var div = cut.Find("div");
        div.Should().NotBeNull();
        div.GetAttribute("style").Should().Contain("position: relative");
    }

    [Fact]
    public void ToggleButton_Click_ChangesInputType()
    {
        var model = new TestModel { Name = "SecretPassword" };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", nameof(TestModel), [nameof(TestModel.Name)])
            .Allow("update", nameof(TestModel), [nameof(TestModel.Name)])
            .Build();

        var cut = Render<PrivilegeInputPassword>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Name)
            .Add(p => p.ValueExpression, () => model.Name)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, v => model.Name = v!))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.Name))
            .Add(p => p.Togglable, true)
        );

        var input = cut.Find("input");
        input.GetAttribute("type").Should().Be("password");

        var button = cut.Find("button");
        button.Click();

        input = cut.Find("input");
        input.GetAttribute("type").Should().Be("text");

        // Click again to toggle back
        button = cut.Find("button");
        button.Click();

        input = cut.Find("input");
        input.GetAttribute("type").Should().Be("password");
    }

    [Fact]
    public void Togglable_NoReadPermission_DoesNotRenderToggleButton()
    {
        var model = new TestModel { Name = "SecretPassword" };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Build(); // No permissions

        var cut = Render<PrivilegeInputPassword>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Name)
            .Add(p => p.ValueExpression, () => model.Name)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, v => model.Name = v!))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.Name))
            .Add(p => p.Togglable, true)
        );

        var buttons = cut.FindAll("button");
        buttons.Should().BeEmpty();
    }

    [Fact]
    public void ContainerClass_AppliedToContainer()
    {
        var model = new TestModel { Name = "SecretPassword" };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", nameof(TestModel), [nameof(TestModel.Name)])
            .Allow("update", nameof(TestModel), [nameof(TestModel.Name)])
            .Build();

        var cut = Render<PrivilegeInputPassword>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Name)
            .Add(p => p.ValueExpression, () => model.Name)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, v => model.Name = v!))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.Name))
            .Add(p => p.Togglable, true)
            .Add(p => p.ContainerClass, "custom-container-class")
        );

        var div = cut.Find("div");
        div.GetAttribute("class").Should().Be("custom-container-class");
    }

    [Fact]
    public void ToggleClass_AppliedToToggleButton()
    {
        var model = new TestModel { Name = "SecretPassword" };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", nameof(TestModel), [nameof(TestModel.Name)])
            .Allow("update", nameof(TestModel), [nameof(TestModel.Name)])
            .Build();

        var cut = Render<PrivilegeInputPassword>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Name)
            .Add(p => p.ValueExpression, () => model.Name)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, v => model.Name = v!))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.Name))
            .Add(p => p.Togglable, true)
            .Add(p => p.ToggleClass, "custom-toggle-class")
        );

        var button = cut.Find("button");
        button.GetAttribute("class").Should().Be("custom-toggle-class");
    }

    [Fact]
    public void InputHasPaddingRight_WhenTogglable()
    {
        var model = new TestModel { Name = "SecretPassword" };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", nameof(TestModel), [nameof(TestModel.Name)])
            .Allow("update", nameof(TestModel), [nameof(TestModel.Name)])
            .Build();

        var cut = Render<PrivilegeInputPassword>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Name)
            .Add(p => p.ValueExpression, () => model.Name)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, v => model.Name = v!))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.Name))
            .Add(p => p.Togglable, true)
        );

        var input = cut.Find("input");
        input.GetAttribute("style").Should().Contain("padding-right: 2rem");
    }

    [Fact]
    public void AdditionalAttributes_ArePreserved()
    {
        var model = new TestModel { Name = "SecretPassword" };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", nameof(TestModel), [nameof(TestModel.Name)])
            .Allow("update", nameof(TestModel), [nameof(TestModel.Name)])
            .Build();

        var additionalAttributes = new Dictionary<string, object>
        {
            ["data-testid"] = "password-input",
            ["placeholder"] = "Enter password"
        };

        var cut = Render<PrivilegeInputPassword>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Name)
            .Add(p => p.ValueExpression, () => model.Name)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, v => model.Name = v!))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.Name))
            .Add(p => p.AdditionalAttributes, additionalAttributes)
        );

        var input = cut.Find("input");
        input.GetAttribute("data-testid").Should().Be("password-input");
        input.GetAttribute("placeholder").Should().Be("Enter password");
    }

    [Fact]
    public void AdditionalAttributes_PreservedWithReadonly()
    {
        var model = new TestModel { Name = "SecretPassword" };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", nameof(TestModel), [nameof(TestModel.Name)])
            .Build(); // no update permission

        var additionalAttributes = new Dictionary<string, object>
        {
            ["data-testid"] = "password-input",
            ["class"] = "custom-class"
        };

        var cut = Render<PrivilegeInputPassword>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Name)
            .Add(p => p.ValueExpression, () => model.Name)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, v => model.Name = v!))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.Name))
            .Add(p => p.AdditionalAttributes, additionalAttributes)
        );

        var input = cut.Find("input");
        input.GetAttribute("data-testid").Should().Be("password-input");
        input.GetAttribute("class").Should().Contain("custom-class");
        input.HasAttribute("readonly").Should().BeTrue();
    }

    [Fact]
    public void AdditionalAttributes_PreservedWhenDisabled()
    {
        var model = new TestModel { Name = "SecretPassword" };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder().Build(); // No permissions

        var additionalAttributes = new Dictionary<string, object>
        {
            ["data-testid"] = "password-input",
            ["aria-label"] = "User Password"
        };

        var cut = Render<PrivilegeInputPassword>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Name)
            .Add(p => p.ValueExpression, () => model.Name)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, v => model.Name = v!))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.Name))
            .Add(p => p.AdditionalAttributes, additionalAttributes)
        );

        var input = cut.Find("input[type=password]");
        input.GetAttribute("data-testid").Should().Be("password-input");
        input.GetAttribute("aria-label").Should().Be("User Password");
    }

    [Fact]
    public void UsesFormStateForDefaultSubject()
    {
        var model = new TestModel { Name = "SecretPassword" };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", "FormSubject", [nameof(TestModel.Name)])
            .Allow("update", "FormSubject", [nameof(TestModel.Name)])
            .Build();

        var formState = new PrivilegeFormState(
            Subject: "FormSubject",
            ReadAction: "read",
            UpdateAction: "update"
        );

        var cut = Render<PrivilegeInputPassword>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .AddCascadingValue(formState)
            .Add(p => p.Value, model.Name)
            .Add(p => p.ValueExpression, () => model.Name)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, v => model.Name = v!))
            // Not setting Subject - should use FormState.Subject
            .Add(p => p.Field, nameof(TestModel.Name))
        );

        var input = cut.Find("input");
        input.GetAttribute("type").Should().Be("password");
        input.HasAttribute("readonly").Should().BeFalse();
    }

    [Fact]
    public void UsesFormStateForDefaultActions()
    {
        var model = new TestModel { Name = "SecretPassword" };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("view", nameof(TestModel), [nameof(TestModel.Name)])
            .Allow("modify", nameof(TestModel), [nameof(TestModel.Name)])
            .Build();

        var formState = new PrivilegeFormState(
            Subject: nameof(TestModel),
            ReadAction: "view",
            UpdateAction: "modify"
        );

        var cut = Render<PrivilegeInputPassword>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .AddCascadingValue(formState)
            .Add(p => p.Value, model.Name)
            .Add(p => p.ValueExpression, () => model.Name)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, v => model.Name = v!))
            // Not setting ReadAction/UpdateAction - should use FormState values
            .Add(p => p.Field, nameof(TestModel.Name))
        );

        var input = cut.Find("input");
        input.GetAttribute("type").Should().Be("password");
        input.HasAttribute("readonly").Should().BeFalse();
    }

    [Fact]
    public void ComponentParametersOverrideFormState()
    {
        var model = new TestModel { Name = "SecretPassword" };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("componentRead", "ComponentSubject", [nameof(TestModel.Name)])
            .Allow("componentUpdate", "ComponentSubject", [nameof(TestModel.Name)])
            .Build();

        var formState = new PrivilegeFormState(
            Subject: "FormSubject",
            ReadAction: "formRead",
            UpdateAction: "formUpdate"
        );

        var cut = Render<PrivilegeInputPassword>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .AddCascadingValue(formState)
            .Add(p => p.Value, model.Name)
            .Add(p => p.ValueExpression, () => model.Name)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, v => model.Name = v!))
            // Component parameters override FormState
            .Add(p => p.Subject, "ComponentSubject")
            .Add(p => p.ReadAction, "componentRead")
            .Add(p => p.UpdateAction, "componentUpdate")
            .Add(p => p.Field, nameof(TestModel.Name))
        );

        var input = cut.Find("input");
        input.GetAttribute("type").Should().Be("password");
        input.HasAttribute("readonly").Should().BeFalse();
    }

    [Fact]
    public void RendersCorrectly_WhenValueChanges()
    {
        var model = new TestModel { Name = "OldPassword" };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", nameof(TestModel), [nameof(TestModel.Name)])
            .Allow("update", nameof(TestModel), [nameof(TestModel.Name)])
            .Build();

        var cut = Render<PrivilegeInputPassword>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Name)
            .Add(p => p.ValueExpression, () => model.Name)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, v => model.Name = v!))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.Name))
        );

        var input = cut.Find("input");
        input.GetAttribute("type").Should().Be("password");

        // Update value
        var newPassword = "NewPassword";
        cut.Render(ps => ps.Add(p => p.Value, newPassword));

        input = cut.Find("input");
        input.GetAttribute("type").Should().Be("password");
    }

    [Fact]
    public void ToggleButton_DisplaysCorrectIcon()
    {
        var model = new TestModel { Name = "SecretPassword" };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", nameof(TestModel), [nameof(TestModel.Name)])
            .Allow("update", nameof(TestModel), [nameof(TestModel.Name)])
            .Build();

        var cut = Render<PrivilegeInputPassword>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Name)
            .Add(p => p.ValueExpression, () => model.Name)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, v => model.Name = v!))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.Name))
            .Add(p => p.Togglable, true)
        );

        var button = cut.Find("button");
        // Initially shows the "show" icon (eye)
        button.InnerHtml.Should().Contain("svg");

        // Click to show password
        button.Click();

        button = cut.Find("button");
        // Now shows the "hide" icon (eye with slash)
        button.InnerHtml.Should().Contain("svg");
    }

    [Fact]
    public void ToggleButton_MultipleClicks_TogglesCorrectly()
    {
        var model = new TestModel { Name = "SecretPassword" };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", nameof(TestModel), [nameof(TestModel.Name)])
            .Allow("update", nameof(TestModel), [nameof(TestModel.Name)])
            .Build();

        var cut = Render<PrivilegeInputPassword>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Name)
            .Add(p => p.ValueExpression, () => model.Name)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, v => model.Name = v!))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.Name))
            .Add(p => p.Togglable, true)
        );

        var input = cut.Find("input");
        input.GetAttribute("type").Should().Be("password");

        var button = cut.Find("button");
        
        // First click - show password
        button.Click();
        input = cut.Find("input");
        input.GetAttribute("type").Should().Be("text");

        // Second click - hide password
        button = cut.Find("button");
        button.Click();
        input = cut.Find("input");
        input.GetAttribute("type").Should().Be("password");

        // Third click - show password again
        button = cut.Find("button");
        button.Click();
        input = cut.Find("input");
        input.GetAttribute("type").Should().Be("text");
    }

    [Fact]
    public void Readonly_WithToggle_AllowsToggling()
    {
        var model = new TestModel { Name = "SecretPassword" };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", nameof(TestModel), [nameof(TestModel.Name)])
            .Build(); // no update permission

        var cut = Render<PrivilegeInputPassword>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Name)
            .Add(p => p.ValueExpression, () => model.Name)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, v => model.Name = v!))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.Name))
            .Add(p => p.Togglable, true)
        );

        var input = cut.Find("input");
        input.HasAttribute("readonly").Should().BeTrue();
        input.GetAttribute("type").Should().Be("password");

        var button = cut.Find("button");
        button.Click();

        input = cut.Find("input");
        input.HasAttribute("readonly").Should().BeTrue();
        input.GetAttribute("type").Should().Be("text");
    }
}
