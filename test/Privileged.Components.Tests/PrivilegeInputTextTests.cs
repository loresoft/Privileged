using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Privileged.Components.Tests;

public class PrivilegeInputTextTests : TestContext
{
    [Fact]
    public void Renders_Text_Input_When_Read_And_Update_Allowed()
    {
        var model = new TestModel { Name = "John" };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", nameof(TestModel), [nameof(TestModel.Name)])
            .Allow("update", nameof(TestModel), [nameof(TestModel.Name)])
            .Build();

        var cut = RenderComponent<PrivilegeInputText>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Name)
            .Add(p => p.ValueExpression, () => model.Name)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, v => model.Name = v!))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.Name))
        );

        var input = cut.Find("input");
        // For text inputs, the type attribute may not be explicitly set since "text" is the default
        var type = input.GetAttribute("type") ?? "text";
        type.Should().Be("text");
        input.HasAttribute("readonly").Should().BeFalse();
    }

    [Fact]
    public void Sets_Readonly_When_Read_Allowed_Update_Denied()
    {
        var model = new TestModel { Name = "John" };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", nameof(TestModel), [nameof(TestModel.Name)])
            .Build(); // no update rule

        var cut = RenderComponent<PrivilegeInputText>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Name)
            .Add(p => p.ValueExpression, () => model.Name)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, v => model.Name = v!))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.Name))
        );

        var input = cut.Find("input");
        // For text inputs, the type attribute may not be explicitly set since "text" is the default
        var type = input.GetAttribute("type") ?? "text";
        type.Should().Be("text");
        input.HasAttribute("readonly").Should().BeTrue();
    }

    [Fact]
    public void Masks_When_Read_Denied_Update_Denied()
    {
        var model = new TestModel { Name = "Secret" };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Build(); // no permissions

        var cut = RenderComponent<PrivilegeInputText>(ps => ps
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
        input.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void Masks_When_Update_Allowed_But_Read_Denied()
    {
        var model = new TestModel { Name = "Secret" };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("update", nameof(TestModel), [nameof(TestModel.Name)])
            .Build(); // only update

        var cut = RenderComponent<PrivilegeInputText>(ps => ps
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
    }

    [Fact]
    public void Falls_Back_To_Model_Name_For_Subject_And_Name_For_Field_When_Not_Set()
    {
        var model = new TestModel { Name = "John" };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
    .Allow("read", nameof(TestModel))
    .Allow("update", nameof(TestModel))
    .Build();

        var cut = RenderComponent<PrivilegeInputText>(ps => ps
            .AddCascadingValue(editContext)
    .AddCascadingValue(ctx)
       .Add(p => p.Value, model.Name)
        .Add(p => p.ValueExpression, () => model.Name)
        .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, v => model.Name = v!))
        // Intentionally not setting Subject and Field - should fallback to model name and name attribute
        );

   var input = cut.Find("input");
      // For text inputs, the type attribute may not be explicitly set since "text" is the default
 var type = input.GetAttribute("type") ?? "text";
        type.Should().Be("text");
        input.HasAttribute("readonly").Should().BeFalse();

  // Verify the input has a name attribute (which would be used as the field fallback)
        input.HasAttribute("name").Should().BeTrue();
    }

    [Fact]
    public void NoPrivilegeContext_AssumeAllPrivileges_RendersNormalTextInput()
{
        // When no PrivilegeContext is provided, component should assume all privileges
   var model = new TestModel { Name = "Test" };
   var editContext = new EditContext(model);

        var cut = RenderComponent<PrivilegeInputText>(ps => ps
      .AddCascadingValue(editContext)
            .Add(p => p.Value, model.Name)
      .Add(p => p.ValueExpression, () => model.Name)
     .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, v => model.Name = v!))
      .Add(p => p.Subject, nameof(TestModel))
     .Add(p => p.Field, nameof(TestModel.Name))
        );

        var input = cut.Find("input");
        var type = input.GetAttribute("type") ?? "text";
        type.Should().Be("text");
      input.HasAttribute("readonly").Should().BeFalse();
  input.HasAttribute("disabled").Should().BeFalse();
}

    [Fact]
    public void EmptySubject_AssumeAllPrivileges_RendersNormalTextInput()
    {
        var model = new TestModel { Name = "Test" };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Build(); // No specific permissions

        var cut = RenderComponent<PrivilegeInputText>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Name)
            .Add(p => p.ValueExpression, () => model.Name)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, v => model.Name = v!))
            .Add(p => p.Subject, "") // Empty subject - should assume all privileges
            .Add(p => p.Field, nameof(TestModel.Name))
        );

        var input = cut.Find("input");
        var type = input.GetAttribute("type") ?? "text";
        type.Should().Be("text");
        input.HasAttribute("readonly").Should().BeFalse();
        input.HasAttribute("disabled").Should().BeFalse();
    }

    [Fact]
    public void NullSubject_AssumeAllPrivileges_RendersNormalTextInput()
    {
        var model = new TestModel { Name = "Test" };

        var ctx = new PrivilegeBuilder()
            .Build(); // No specific permissions

        var cut = RenderComponent<PrivilegeInputText>(ps => ps
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Name)
            .Add(p => p.ValueExpression, () => model.Name)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, v => model.Name = v!))
            .Add(p => p.Subject, (string?)null) // Null subject - should assume all privileges
            .Add(p => p.Field, nameof(TestModel.Name))
        );

        var input = cut.Find("input");
        var type = input.GetAttribute("type") ?? "text";
        type.Should().Be("text");
        input.HasAttribute("readonly").Should().BeFalse();
        input.HasAttribute("disabled").Should().BeFalse();
    }

    [Fact]
    public void WhitespaceSubject_AssumeAllPrivileges_RendersNormalTextInput()
    {
        var model = new TestModel { Name = "Test" };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Build(); // No specific permissions

        var cut = RenderComponent<PrivilegeInputText>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Name)
            .Add(p => p.ValueExpression, () => model.Name)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, v => model.Name = v!))
            .Add(p => p.Subject, "   ") // Whitespace subject - should assume all privileges
            .Add(p => p.Field, nameof(TestModel.Name))
        );

        var input = cut.Find("input");
        var type = input.GetAttribute("type") ?? "text";
        type.Should().Be("text");
        input.HasAttribute("readonly").Should().BeFalse();
        input.HasAttribute("disabled").Should().BeFalse();
    }

    [Fact]
    public void EmptySubject_WithField_StillAssumeAllPrivileges()
    {
        var model = new TestModel { Name = "Test" };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Build(); // No specific permissions

        var cut = RenderComponent<PrivilegeInputText>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Name)
            .Add(p => p.ValueExpression, () => model.Name)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, v => model.Name = v!))
            .Add(p => p.Subject, "") // Empty subject
            .Add(p => p.Field, "RestrictedField") // Even with restricted field, should work
        );

        var input = cut.Find("input");
        var type = input.GetAttribute("type") ?? "text";
        type.Should().Be("text");
        input.HasAttribute("readonly").Should().BeFalse();
        input.HasAttribute("disabled").Should().BeFalse();
    }

    [Fact]
    public void WithCustomActions_EmptySubject_AssumeAllPrivileges()
    {
        var model = new TestModel { Name = "Test" };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Build(); // No specific permissions

        var cut = RenderComponent<PrivilegeInputText>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Name)
            .Add(p => p.ValueExpression, () => model.Name)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, v => model.Name = v!))
            .Add(p => p.Subject, "") // Empty subject
            .Add(p => p.Field, nameof(TestModel.Name))
            .Add(p => p.ReadAction, "view") // Custom read action
            .Add(p => p.UpdateAction, "modify") // Custom update action
        );

        var input = cut.Find("input");
        var type = input.GetAttribute("type") ?? "text";
        type.Should().Be("text");
        input.HasAttribute("readonly").Should().BeFalse();
        input.HasAttribute("disabled").Should().BeFalse();
    }
}
