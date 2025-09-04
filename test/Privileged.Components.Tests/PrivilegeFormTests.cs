using System;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Privileged.Components.Tests;

public class PrivilegeFormTests : TestContext
{
    [Fact]
    public void Renders_Form_With_Basic_Parameters()
    {
        var model = new TestModel { Name = "John", Age = 30 };
        var ctx = new PrivilegeBuilder()
            .Allow("read", "TestModel")
            .Allow("update", "TestModel")
            .Build();

        var cut = RenderComponent<PrivilegeForm>(ps => ps
            .AddCascadingValue(ctx)
            .Add(p => p.Model, model)
            .Add(p => p.Subject, "User")
            .Add(p => p.ReadAction, "view")
            .Add(p => p.UpdateAction, "edit")
            .Add(p => p.ChildContent, (EditContext context) => builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddContent(1, "Form Content");
                builder.CloseElement();
            })
        );

        var form = cut.Find("form");
        form.Should().NotBeNull();
        form.InnerHtml.Should().Contain("Form Content");
    }

    [Fact]
    public void Cascades_PrivilegeFormState_To_Child_Components()
    {
        var model = new TestModel { Name = "John" };
        var ctx = new PrivilegeBuilder()
            .Allow("view", "User", ["Name"])
            .Allow("edit", "User", ["Name"])
            .Build();

        var cut = RenderComponent<PrivilegeForm>(ps => ps
            .AddCascadingValue(ctx)
            .Add(p => p.Model, model)
            .Add(p => p.Subject, "User")
            .Add(p => p.ReadAction, "view")
            .Add(p => p.UpdateAction, "edit")
            .Add(p => p.ChildContent, (EditContext context) => builder =>
            {
                builder.OpenComponent<PrivilegeInputText>(0);
                builder.AddAttribute(1, "Value", model.Name);
                builder.AddAttribute(2, "ValueExpression", (Expression<Func<string>>)(() => model.Name));
                builder.AddAttribute(3, "ValueChanged", EventCallback.Factory.Create<string?>(this, v => model.Name = v!));
                builder.AddAttribute(4, "Field", "Name");
                builder.CloseComponent();
            })
        );

        // The input should inherit the Subject, ReadAction, and UpdateAction from the form
        var input = cut.Find("input");
        input.Should().NotBeNull();
        var type = input.GetAttribute("type") ?? "text";
        type.Should().Be("text");
        input.HasAttribute("readonly").Should().BeFalse(); // Should have both read and update permissions
    }

    [Fact]
    public void Child_Components_Can_Override_Cascaded_Subject()
    {
        var model = new TestModel { Name = "John" };
        var ctx = new PrivilegeBuilder()
            .Allow("read", "Post", ["Name"])
            .Allow("update", "Post", ["Name"])
            .Build();

        var cut = RenderComponent<PrivilegeForm>(ps => ps
            .AddCascadingValue(ctx)
            .Add(p => p.Model, model)
            .Add(p => p.Subject, "User") // Form has User subject
            .Add(p => p.ReadAction, "read")
            .Add(p => p.UpdateAction, "update")
            .Add(p => p.ChildContent, (EditContext context) => builder =>
            {
                builder.OpenComponent<PrivilegeInputText>(0);
                builder.AddAttribute(1, "Value", model.Name);
                builder.AddAttribute(2, "ValueExpression", (Expression<Func<string>>)(() => model.Name));
                builder.AddAttribute(3, "ValueChanged", EventCallback.Factory.Create<string?>(this, v => model.Name = v!));
                builder.AddAttribute(4, "Subject", "Post"); // Child overrides with Post subject
                builder.AddAttribute(5, "Field", "Name");
                builder.CloseComponent();
            })
        );

        // The input should work with Post permissions, not User permissions
        var input = cut.Find("input");
        input.Should().NotBeNull();
        var type = input.GetAttribute("type") ?? "text";
        type.Should().Be("text");
        input.HasAttribute("readonly").Should().BeFalse();
    }

    [Fact]
    public void Child_Components_Can_Override_Cascaded_Actions()
    {
        var model = new TestModel { Name = "John" };
        var ctx = new PrivilegeBuilder()
            .Allow("view", "User", ["Name"]) // Child will use 'view' instead of 'read'
            .Allow("modify", "User", ["Name"]) // Child will use 'modify' instead of 'update'
            .Build();

        var cut = RenderComponent<PrivilegeForm>(ps => ps
            .AddCascadingValue(ctx)
            .Add(p => p.Model, model)
            .Add(p => p.Subject, "User")
            .Add(p => p.ReadAction, "read") // Form default
            .Add(p => p.UpdateAction, "update") // Form default
            .Add(p => p.ChildContent, (EditContext context) => builder =>
            {
                builder.OpenComponent<PrivilegeInputText>(0);
                builder.AddAttribute(1, "Value", model.Name);
                builder.AddAttribute(2, "ValueExpression", (Expression<Func<string>>)(() => model.Name));
                builder.AddAttribute(3, "ValueChanged", EventCallback.Factory.Create<string?>(this, v => model.Name = v!));
                builder.AddAttribute(4, "Field", "Name");
                builder.AddAttribute(5, "ReadAction", "view"); // Override read action
                builder.AddAttribute(6, "UpdateAction", "modify"); // Override update action
                builder.CloseComponent();
            })
        );

        // The input should work with overridden actions
        var input = cut.Find("input");
        input.Should().NotBeNull();
        var type = input.GetAttribute("type") ?? "text";
        type.Should().Be("text");
        input.HasAttribute("readonly").Should().BeFalse();
    }

    [Fact]
    public void Multiple_Child_Components_With_Mixed_Overrides()
    {
        var model = new TestModel { Name = "John", Age = 30 };
        var ctx = new PrivilegeBuilder()
            .Allow("read", "User", ["Name"])
            .Allow("update", "User", ["Name"])
            .Allow("view", "Profile", ["Age"])
            .Build(); // Note: no update permission for Profile Age

        var cut = RenderComponent<PrivilegeForm>(ps => ps
            .AddCascadingValue(ctx)
            .Add(p => p.Model, model)
            .Add(p => p.Subject, "User")
            .Add(p => p.ReadAction, "read")
            .Add(p => p.UpdateAction, "update")
            .Add(p => p.ChildContent, (EditContext context) => builder =>
            {
                // First input inherits form settings
                builder.OpenComponent<PrivilegeInputText>(0);
                builder.AddAttribute(1, "Value", model.Name);
                builder.AddAttribute(2, "ValueExpression", (Expression<Func<string>>)(() => model.Name));
                builder.AddAttribute(3, "ValueChanged", EventCallback.Factory.Create<string?>(this, v => model.Name = v!));
                builder.AddAttribute(4, "Field", "Name");
                builder.CloseComponent();

                // Second input overrides subject and read action
                builder.OpenComponent<PrivilegeInputNumber<int>>(5);
                builder.AddAttribute(6, "Value", model.Age);
                builder.AddAttribute(7, "ValueExpression", (Expression<Func<int>>)(() => model.Age));
                builder.AddAttribute(8, "ValueChanged", EventCallback.Factory.Create<int>(this, v => model.Age = v));
                builder.AddAttribute(9, "Subject", "Profile");
                builder.AddAttribute(10, "ReadAction", "view");
                builder.AddAttribute(11, "Field", "Age");
                builder.CloseComponent();
            })
        );

        var inputs = cut.FindAll("input");
        inputs.Count.Should().Be(2);

        // First input (Name) should be editable
        var nameInput = inputs[0];
        var nameType = nameInput.GetAttribute("type") ?? "text";
        nameType.Should().Be("text");
        nameInput.HasAttribute("readonly").Should().BeFalse();

        // Second input (Age) should be readonly (read permission but no update)
        var ageInput = inputs[1];
        ageInput.GetAttribute("type").Should().Be("number");
        ageInput.HasAttribute("readonly").Should().BeTrue();
    }

    [Fact]
    public void Form_Parameters_Are_Null_By_Default()
    {
        var model = new TestModel();
        var ctx = new PrivilegeBuilder().Build();

        var cut = RenderComponent<PrivilegeForm>(ps => ps
            .AddCascadingValue(ctx)
            .Add(p => p.Model, model)
            .Add(p => p.ChildContent, (EditContext context) => builder =>
            {
                builder.AddContent(0, "Content");
            })
        );

        // Should render without issues even with null parameters
        var form = cut.Find("form");
        form.Should().NotBeNull();
        form.InnerHtml.Should().Contain("Content");
    }

    [Fact]
    public void Child_Components_Use_Model_Type_When_No_Subject_Provided()
    {
        var model = new TestModel { Name = "John" };
        var ctx = new PrivilegeBuilder()
            .Allow("read", "TestModel", ["Name"])
            .Allow("update", "TestModel", ["Name"])
            .Build();

        var cut = RenderComponent<PrivilegeForm>(ps => ps
            .AddCascadingValue(ctx)
            .Add(p => p.Model, model)
            // No Subject parameter provided
            .Add(p => p.ChildContent, (EditContext context) => builder =>
            {
                builder.OpenComponent<PrivilegeInputText>(0);
                builder.AddAttribute(1, "Value", model.Name);
                builder.AddAttribute(2, "ValueExpression", (Expression<Func<string>>)(() => model.Name));
                builder.AddAttribute(3, "ValueChanged", EventCallback.Factory.Create<string?>(this, v => model.Name = v!));
                builder.AddAttribute(4, "Field", "Name");
                builder.CloseComponent();
            })
        );

        // Should work because child component falls back to model type name
        var input = cut.Find("input");
        input.Should().NotBeNull();
        var type = input.GetAttribute("type") ?? "text";
        type.Should().Be("text");
        input.HasAttribute("readonly").Should().BeFalse();
    }

    [Fact]
    public void Supports_OnValidSubmit_And_OnInvalidSubmit()
    {
        var model = new ValidationTestModel { Name = "Valid Name" };
        var ctx = new PrivilegeBuilder()
            .Allow("read", "ValidationTestModel")
            .Allow("update", "ValidationTestModel")
            .Build();

        var validSubmitCalled = false;
        var invalidSubmitCalled = false;

        var cut = RenderComponent<PrivilegeForm>(ps => ps
            .AddCascadingValue(ctx)
            .Add(p => p.Model, model)
            .Add(p => p.Subject, "ValidationTestModel")
            .Add(p => p.OnValidSubmit, EventCallback.Factory.Create<EditContext>(this, (EditContext context) => validSubmitCalled = true))
            .Add(p => p.OnInvalidSubmit, EventCallback.Factory.Create<EditContext>(this, (EditContext context) => invalidSubmitCalled = true))
            .Add(p => p.ChildContent, (EditContext context) => builder =>
            {
                builder.OpenComponent<DataAnnotationsValidator>(0);
                builder.CloseComponent();

                builder.OpenElement(1, "button");
                builder.AddAttribute(2, "type", "submit");
                builder.AddContent(3, "Submit");
                builder.CloseElement();
            })
        );

        var submitButton = cut.Find("button[type=submit]");
        submitButton.Click();

        // Valid submit should be called
        validSubmitCalled.Should().BeTrue();
        invalidSubmitCalled.Should().BeFalse();
    }

    [Fact]
    public void Form_State_Updates_When_Parameters_Change()
    {
        var model = new TestModel { Name = "John" };
        var ctx = new PrivilegeBuilder()
            .Allow("read", "User")
            .Allow("update", "User")
            .Allow("view", "NewSubject")
            .Allow("modify", "NewSubject")
            .Build();

        var cut = RenderComponent<PrivilegeForm>(ps => ps
            .AddCascadingValue(ctx)
            .Add(p => p.Model, model)
            .Add(p => p.Subject, "User")
            .Add(p => p.ReadAction, "read")
            .Add(p => p.UpdateAction, "update")
            .Add(p => p.ChildContent, (EditContext context) => builder =>
            {
                builder.OpenComponent<PrivilegeInputText>(0);
                builder.AddAttribute(1, "Value", model.Name);
                builder.AddAttribute(2, "ValueExpression", (Expression<Func<string>>)(() => model.Name));
                builder.AddAttribute(3, "ValueChanged", EventCallback.Factory.Create<string?>(this, v => model.Name = v!));
                builder.AddAttribute(4, "Field", "Name");
                builder.CloseComponent();
            })
        );

        // Initial state - should work
        var input = cut.Find("input");
        var type = input.GetAttribute("type") ?? "text";
        type.Should().Be("text");

        // Update form parameters
        cut.SetParametersAndRender(ps => ps
            .Add(p => p.Subject, "NewSubject")
            .Add(p => p.ReadAction, "view")
            .Add(p => p.UpdateAction, "modify")
        );

        // Should still work with new parameters
        input = cut.Find("input");
        type = input.GetAttribute("type") ?? "text";
        type.Should().Be("text");
        input.HasAttribute("readonly").Should().BeFalse();
    }

    [Fact]
    public void Empty_Subject_Allows_All_Privileges_In_Child_Components()
    {
        var model = new TestModel { Name = "John" };
        var ctx = new PrivilegeBuilder()
            .Build(); // No permissions defined

        var cut = RenderComponent<PrivilegeForm>(ps => ps
            .AddCascadingValue(ctx)
            .Add(p => p.Model, model)
            .Add(p => p.Subject, "") // Empty subject
            .Add(p => p.ReadAction, "read")
            .Add(p => p.UpdateAction, "update")
            .Add(p => p.ChildContent, (EditContext context) => builder =>
            {
                builder.OpenComponent<PrivilegeInputText>(0);
                builder.AddAttribute(1, "Value", model.Name);
                builder.AddAttribute(2, "ValueExpression", (Expression<Func<string>>)(() => model.Name));
                builder.AddAttribute(3, "ValueChanged", EventCallback.Factory.Create<string?>(this, v => model.Name = v!));
                builder.AddAttribute(4, "Field", "Name");
                builder.CloseComponent();
            })
        );

        // Should work because empty subject assumes all privileges
        var input = cut.Find("input");
        var type = input.GetAttribute("type") ?? "text";
        type.Should().Be("text");
        input.HasAttribute("readonly").Should().BeFalse();
    }

    [Fact]
    public void PrivilegeFormState_Is_Created_Correctly()
    {
        var model = new TestModel { Name = "John" };
        var ctx = new PrivilegeBuilder().Build();

        var capturedFormState = (PrivilegeFormState?)null;

        var cut = RenderComponent<PrivilegeForm>(ps => ps
            .AddCascadingValue(ctx)
            .Add(p => p.Model, model)
            .Add(p => p.Subject, "TestSubject")
            .Add(p => p.ReadAction, "TestRead")
            .Add(p => p.UpdateAction, "TestUpdate")
            .Add(p => p.ChildContent, (EditContext context) => builder =>
            {
                // Create a component that captures the cascaded form state
                builder.OpenComponent<TestFormStateCapture>(0);
                builder.AddAttribute(1, "OnFormStateCapture", EventCallback.Factory.Create<PrivilegeFormState?>(this, state => capturedFormState = state));
                builder.CloseComponent();
            })
        );

        // Verify that the form state was cascaded correctly
        capturedFormState.Should().NotBeNull();
        capturedFormState!.Subject.Should().Be("TestSubject");
        capturedFormState.ReadAction.Should().Be("TestRead");
        capturedFormState.UpdateAction.Should().Be("TestUpdate");
    }

    [Fact]
    public void Null_Parameters_Create_Null_FormState_Properties()
    {
        var model = new TestModel { Name = "John" };
        var ctx = new PrivilegeBuilder().Build();

        var capturedFormState = (PrivilegeFormState?)null;

        var cut = RenderComponent<PrivilegeForm>(ps => ps
            .AddCascadingValue(ctx)
            .Add(p => p.Model, model)
            // Not setting Subject, ReadAction, or UpdateAction - should be null
            .Add(p => p.ChildContent, (EditContext context) => builder =>
            {
                builder.OpenComponent<TestFormStateCapture>(0);
                builder.AddAttribute(1, "OnFormStateCapture", EventCallback.Factory.Create<PrivilegeFormState?>(this, state => capturedFormState = state));
                builder.CloseComponent();
            })
        );

        capturedFormState.Should().NotBeNull();
        capturedFormState!.Subject.Should().BeNull();
        capturedFormState.ReadAction.Should().BeNull();
        capturedFormState.UpdateAction.Should().BeNull();
    }

    // Helper class for validation tests
    public class ValidationTestModel
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = "";
    }

    // Helper component to capture the cascaded PrivilegeFormState
    public class TestFormStateCapture : ComponentBase
    {
        [CascadingParameter]
        public PrivilegeFormState? FormState { get; set; }

        [Parameter]
        public EventCallback<PrivilegeFormState?> OnFormStateCapture { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            await OnFormStateCapture.InvokeAsync(FormState);
        }

        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
        {
            // Empty component, just used to capture the cascaded state
        }
    }
}
