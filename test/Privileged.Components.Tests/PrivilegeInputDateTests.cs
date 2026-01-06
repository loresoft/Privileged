using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Privileged.Components.Tests;

public class PrivilegeInputDateTests : BunitContext
{
    [Fact]
    public void Renders_Date_Input_When_Read_And_Update_Allowed()
    {
        var model = new TestModel { DateOfBirth = new DateTime(1990, 5, 15) };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", nameof(TestModel), [nameof(TestModel.DateOfBirth)])
            .Allow("update", nameof(TestModel), [nameof(TestModel.DateOfBirth)])
            .Build();

        var cut = Render<PrivilegeInputDate<DateTime>>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.DateOfBirth)
            .Add(p => p.ValueExpression, () => model.DateOfBirth)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<DateTime>(this, v => model.DateOfBirth = v))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.DateOfBirth))
        );

        var input = cut.Find("input");
        input.GetAttribute("type").Should().Be("date");
        input.HasAttribute("readonly").Should().BeFalse();
    }

    [Fact]
    public void Sets_Readonly_When_Read_Allowed_Update_Denied()
    {
        var model = new TestModel { DateOfBirth = new DateTime(1990, 5, 15) };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", nameof(TestModel), [nameof(TestModel.DateOfBirth)])
            .Build(); // no update rule

        var cut = Render<PrivilegeInputDate<DateTime>>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.DateOfBirth)
            .Add(p => p.ValueExpression, () => model.DateOfBirth)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<DateTime>(this, v => model.DateOfBirth = v))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.DateOfBirth))
        );

        var input = cut.Find("input");
        input.GetAttribute("type").Should().Be("date");
        input.HasAttribute("readonly").Should().BeTrue();
    }

    [Fact]
    public void Masks_When_Read_Denied_Update_Denied()
    {
        var model = new TestModel { DateOfBirth = new DateTime(1990, 5, 15) };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder().Build(); // No permissions

        var cut = Render<PrivilegeInputDate<DateTime>>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.DateOfBirth)
            .Add(p => p.ValueExpression, () => model.DateOfBirth)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<DateTime>(this, v => model.DateOfBirth = v))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.DateOfBirth))
        );

        var input = cut.Find("input[type=password]");
        input.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void EmptySubject_AssumeAllPrivileges_RendersEditableInput()
    {
        var model = new TestModel { DateOfBirth = new DateTime(1990, 5, 15) };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Build(); // No specific permissions

        var cut = Render<PrivilegeInputDate<DateTime>>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.DateOfBirth)
            .Add(p => p.ValueExpression, () => model.DateOfBirth)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<DateTime>(this, v => model.DateOfBirth = v))
            .Add(p => p.Subject, "") // Empty subject
            .Add(p => p.Field, nameof(TestModel.DateOfBirth))
        );

        var input = cut.Find("input");
        input.GetAttribute("type").Should().Be("date");
        input.HasAttribute("readonly").Should().BeFalse();
    }

    [Fact]
    public void NullSubject_AssumeAllPrivileges_RendersEditableInput()
    {
        var model = new TestModel { DateOfBirth = new DateTime(1990, 5, 15) };

        var ctx = new PrivilegeBuilder()
            .Build(); // No specific permissions

        var cut = Render<PrivilegeInputDate<DateTime>>(ps => ps
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.DateOfBirth)
            .Add(p => p.ValueExpression, () => model.DateOfBirth)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<DateTime>(this, v => model.DateOfBirth = v))
            .Add(p => p.Subject, (string?)null) // Null subject
            .Add(p => p.Field, nameof(TestModel.DateOfBirth))
        );

        var input = cut.Find("input");
        input.GetAttribute("type").Should().Be("date");
        input.HasAttribute("readonly").Should().BeFalse();
    }

    [Fact]
    public void CustomActions_UsesCustomActionNames()
    {
        var model = new TestModel { DateOfBirth = new DateTime(1990, 5, 15) };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("view", nameof(TestModel), [nameof(TestModel.DateOfBirth)])
            .Allow("edit", nameof(TestModel), [nameof(TestModel.DateOfBirth)])
            .Build();

        var cut = Render<PrivilegeInputDate<DateTime>>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.DateOfBirth)
            .Add(p => p.ValueExpression, () => model.DateOfBirth)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<DateTime>(this, v => model.DateOfBirth = v))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.DateOfBirth))
            .Add(p => p.ReadAction, "view")
            .Add(p => p.UpdateAction, "edit")
        );

        var input = cut.Find("input");
        input.GetAttribute("type").Should().Be("date");
        input.HasAttribute("readonly").Should().BeFalse();
    }

    [Fact]
    public void CustomActions_ReadDenied_RendersPasswordInput()
    {
        var model = new TestModel { DateOfBirth = new DateTime(1990, 5, 15) };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("edit", nameof(TestModel), [nameof(TestModel.DateOfBirth)])
            .Build(); // view action not allowed

        var cut = Render<PrivilegeInputDate<DateTime>>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.DateOfBirth)
            .Add(p => p.ValueExpression, () => model.DateOfBirth)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<DateTime>(this, v => model.DateOfBirth = v))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.DateOfBirth))
            .Add(p => p.ReadAction, "view")
            .Add(p => p.UpdateAction, "edit")
        );

        var input = cut.Find("input[type=password]");
        input.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void CustomActions_UpdateDenied_RendersReadonlyInput()
    {
        var model = new TestModel { DateOfBirth = new DateTime(1990, 5, 15) };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("view", nameof(TestModel), [nameof(TestModel.DateOfBirth)])
            .Build(); // edit action not allowed

        var cut = Render<PrivilegeInputDate<DateTime>>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.DateOfBirth)
            .Add(p => p.ValueExpression, () => model.DateOfBirth)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<DateTime>(this, v => model.DateOfBirth = v))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.DateOfBirth))
            .Add(p => p.ReadAction, "view")
            .Add(p => p.UpdateAction, "edit")
        );

        var input = cut.Find("input");
        input.GetAttribute("type").Should().Be("date");
        input.HasAttribute("readonly").Should().BeTrue();
    }

    [Fact]
    public void NoPrivilegeContext_AssumeAllPrivileges_RendersEditableInput()
    {
        // When no PrivilegeContext is provided, component should assume all privileges
        var model = new TestModel { DateOfBirth = new DateTime(1990, 5, 15) };
        var editContext = new EditContext(model);

        var cut = Render<PrivilegeInputDate<DateTime>>(ps => ps
            .AddCascadingValue(editContext)
            // No PrivilegeContext provided
            .Add(p => p.Value, model.DateOfBirth)
            .Add(p => p.ValueExpression, () => model.DateOfBirth)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<DateTime>(this, v => model.DateOfBirth = v))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.DateOfBirth))
        );

        var input = cut.Find("input");
        input.GetAttribute("type").Should().Be("date");
        input.HasAttribute("readonly").Should().BeFalse();
        input.HasAttribute("disabled").Should().BeFalse();
    }

    [Fact]
    public void WorksWithDateOnly()
    {
        var dateOnlyValue = new DateOnly(1990, 5, 15);
        var model = new { DateOfBirth = dateOnlyValue };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", model.GetType().Name, [nameof(model.DateOfBirth)])
            .Allow("update", model.GetType().Name, [nameof(model.DateOfBirth)])
            .Build();

        var cut = Render<PrivilegeInputDate<DateOnly>>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, dateOnlyValue)
            .Add(p => p.ValueExpression, () => model.DateOfBirth)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<DateOnly>(this, v => { }))
            .Add(p => p.Subject, model.GetType().Name)
            .Add(p => p.Field, nameof(model.DateOfBirth))
        );

        var input = cut.Find("input");
        input.GetAttribute("type").Should().Be("date");
        input.HasAttribute("readonly").Should().BeFalse();
    }

    [Fact]
    public void WorksWithNullableDateTime()
    {
        DateTime? nullableDate = new DateTime(1990, 5, 15);
        var model = new { DateOfBirth = nullableDate };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", model.GetType().Name, [nameof(model.DateOfBirth)])
            .Allow("update", model.GetType().Name, [nameof(model.DateOfBirth)])
            .Build();

        var cut = Render<PrivilegeInputDate<DateTime?>>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, nullableDate)
            .Add(p => p.ValueExpression, () => model.DateOfBirth)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<DateTime?>(this, v => { }))
            .Add(p => p.Subject, model.GetType().Name)
            .Add(p => p.Field, nameof(model.DateOfBirth))
        );

        var input = cut.Find("input");
        input.GetAttribute("type").Should().Be("date");
        input.HasAttribute("readonly").Should().BeFalse();
    }

    [Fact]
    public void AdditionalAttributes_ArePreserved()
    {
        var model = new TestModel { DateOfBirth = new DateTime(1990, 5, 15) };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", nameof(TestModel), [nameof(TestModel.DateOfBirth)])
            .Allow("update", nameof(TestModel), [nameof(TestModel.DateOfBirth)])
            .Build();

        var additionalAttributes = new Dictionary<string, object>
        {
            ["data-testid"] = "date-input",
            ["placeholder"] = "Select a date"
        };

        var cut = Render<PrivilegeInputDate<DateTime>>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.DateOfBirth)
            .Add(p => p.ValueExpression, () => model.DateOfBirth)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<DateTime>(this, v => model.DateOfBirth = v))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.DateOfBirth))
            .Add(p => p.AdditionalAttributes, additionalAttributes)
        );

        var input = cut.Find("input");
        input.GetAttribute("data-testid").Should().Be("date-input");
        input.GetAttribute("placeholder").Should().Be("Select a date");
    }

    [Fact]
    public void DateTime_MinValue_RendersAsEmptyString()
    {
        var model = new TestModel { DateOfBirth = DateTime.MinValue };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", nameof(TestModel), [nameof(TestModel.DateOfBirth)])
            .Allow("update", nameof(TestModel), [nameof(TestModel.DateOfBirth)])
            .Build();

        var cut = Render<PrivilegeInputDate<DateTime>>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, DateTime.MinValue)
            .Add(p => p.ValueExpression, () => model.DateOfBirth)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<DateTime>(this, v => model.DateOfBirth = v))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.DateOfBirth))
        );

        var input = cut.Find("input");
        input.GetAttribute("value").Should().BeEmpty();
    }

    [Fact]
    public void DateOnly_MinValue_RendersAsEmptyString()
    {
        var dateOnlyValue = DateOnly.MinValue;
        var model = new { DateOfBirth = dateOnlyValue };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", model.GetType().Name, [nameof(model.DateOfBirth)])
            .Allow("update", model.GetType().Name, [nameof(model.DateOfBirth)])
            .Build();

        var cut = Render<PrivilegeInputDate<DateOnly>>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, DateOnly.MinValue)
            .Add(p => p.ValueExpression, () => model.DateOfBirth)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<DateOnly>(this, v => { }))
            .Add(p => p.Subject, model.GetType().Name)
            .Add(p => p.Field, nameof(model.DateOfBirth))
        );

        var input = cut.Find("input");
        input.GetAttribute("value").Should().BeEmpty();
    }

    [Fact]
    public void WorksWithDateTimeOffset()
    {
        var dateTimeOffsetValue = new DateTimeOffset(1990, 5, 15, 0, 0, 0, TimeSpan.Zero);
        var model = new { DateOfBirth = dateTimeOffsetValue };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", model.GetType().Name, [nameof(model.DateOfBirth)])
            .Allow("update", model.GetType().Name, [nameof(model.DateOfBirth)])
            .Build();

        var cut = Render<PrivilegeInputDate<DateTimeOffset>>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, dateTimeOffsetValue)
            .Add(p => p.ValueExpression, () => model.DateOfBirth)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<DateTimeOffset>(this, v => { }))
            .Add(p => p.Subject, model.GetType().Name)
            .Add(p => p.Field, nameof(model.DateOfBirth))
        );

        var input = cut.Find("input");
        input.GetAttribute("type").Should().Be("date");
        input.HasAttribute("readonly").Should().BeFalse();
    }

    [Fact]
    public void DateTimeOffset_MinValue_RendersAsEmptyString()
    {
        var model = new { DateOfBirth = DateTimeOffset.MinValue };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", model.GetType().Name, [nameof(model.DateOfBirth)])
            .Allow("update", model.GetType().Name, [nameof(model.DateOfBirth)])
            .Build();

        var cut = Render<PrivilegeInputDate<DateTimeOffset>>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, DateTimeOffset.MinValue)
            .Add(p => p.ValueExpression, () => model.DateOfBirth)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<DateTimeOffset>(this, v => { }))
            .Add(p => p.Subject, model.GetType().Name)
            .Add(p => p.Field, nameof(model.DateOfBirth))
        );

        var input = cut.Find("input");
        input.GetAttribute("value").Should().BeEmpty();
    }

    [Fact]
    public void WorksWithNullableDateOnly()
    {
        DateOnly? nullableDateOnly = new DateOnly(1990, 5, 15);
        var model = new { DateOfBirth = nullableDateOnly };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", model.GetType().Name, [nameof(model.DateOfBirth)])
            .Allow("update", model.GetType().Name, [nameof(model.DateOfBirth)])
            .Build();

        var cut = Render<PrivilegeInputDate<DateOnly?>>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, nullableDateOnly)
            .Add(p => p.ValueExpression, () => model.DateOfBirth)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<DateOnly?>(this, v => { }))
            .Add(p => p.Subject, model.GetType().Name)
            .Add(p => p.Field, nameof(model.DateOfBirth))
        );

        var input = cut.Find("input");
        input.GetAttribute("type").Should().Be("date");
        input.HasAttribute("readonly").Should().BeFalse();
    }

    [Fact]
    public void WorksWithNullableDateTimeOffset()
    {
        DateTimeOffset? nullableDateTimeOffset = new DateTimeOffset(1990, 5, 15, 0, 0, 0, TimeSpan.Zero);
        var model = new { DateOfBirth = nullableDateTimeOffset };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", model.GetType().Name, [nameof(model.DateOfBirth)])
            .Allow("update", model.GetType().Name, [nameof(model.DateOfBirth)])
            .Build();

        var cut = Render<PrivilegeInputDate<DateTimeOffset?>>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, nullableDateTimeOffset)
            .Add(p => p.ValueExpression, () => model.DateOfBirth)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<DateTimeOffset?>(this, v => { }))
            .Add(p => p.Subject, model.GetType().Name)
            .Add(p => p.Field, nameof(model.DateOfBirth))
        );

        var input = cut.Find("input");
        input.GetAttribute("type").Should().Be("date");
        input.HasAttribute("readonly").Should().BeFalse();
    }

    [Fact]
    public void AdditionalAttributes_PreservedWithReadonly()
    {
        var model = new TestModel { DateOfBirth = new DateTime(1990, 5, 15) };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", nameof(TestModel), [nameof(TestModel.DateOfBirth)])
            .Build(); // no update permission

        var additionalAttributes = new Dictionary<string, object>
        {
            ["data-testid"] = "date-input",
            ["class"] = "custom-class"
        };

        var cut = Render<PrivilegeInputDate<DateTime>>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.DateOfBirth)
            .Add(p => p.ValueExpression, () => model.DateOfBirth)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<DateTime>(this, v => model.DateOfBirth = v))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.DateOfBirth))
            .Add(p => p.AdditionalAttributes, additionalAttributes)
        );

        var input = cut.Find("input");
        input.GetAttribute("data-testid").Should().Be("date-input");
        input.GetAttribute("class").Should().Contain("custom-class");
        input.HasAttribute("readonly").Should().BeTrue();
    }

    [Fact]
    public void AdditionalAttributes_PreservedWhenMasked()
    {
        var model = new TestModel { DateOfBirth = new DateTime(1990, 5, 15) };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder().Build(); // No permissions

        var additionalAttributes = new Dictionary<string, object>
        {
            ["data-testid"] = "date-input",
            ["aria-label"] = "Birth Date"
        };

        var cut = Render<PrivilegeInputDate<DateTime>>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.DateOfBirth)
            .Add(p => p.ValueExpression, () => model.DateOfBirth)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<DateTime>(this, v => model.DateOfBirth = v))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.DateOfBirth))
            .Add(p => p.AdditionalAttributes, additionalAttributes)
        );

        var input = cut.Find("input[type=password]");
        input.GetAttribute("data-testid").Should().Be("date-input");
        input.GetAttribute("aria-label").Should().Be("Birth Date");
    }

    [Fact]
    public void UsesFormStateForDefaultSubject()
    {
        var model = new TestModel { DateOfBirth = new DateTime(1990, 5, 15) };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", "FormSubject", [nameof(TestModel.DateOfBirth)])
            .Allow("update", "FormSubject", [nameof(TestModel.DateOfBirth)])
            .Build();

        var formState = new PrivilegeFormState(
            Subject: "FormSubject",
            ReadAction: "read",
            UpdateAction: "update"
        );

        var cut = Render<PrivilegeInputDate<DateTime>>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .AddCascadingValue(formState)
            .Add(p => p.Value, model.DateOfBirth)
            .Add(p => p.ValueExpression, () => model.DateOfBirth)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<DateTime>(this, v => model.DateOfBirth = v))
            // Not setting Subject - should use FormState.Subject
            .Add(p => p.Field, nameof(TestModel.DateOfBirth))
        );

        var input = cut.Find("input");
        input.GetAttribute("type").Should().Be("date");
        input.HasAttribute("readonly").Should().BeFalse();
    }

    [Fact]
    public void UsesFormStateForDefaultActions()
    {
        var model = new TestModel { DateOfBirth = new DateTime(1990, 5, 15) };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("view", nameof(TestModel), [nameof(TestModel.DateOfBirth)])
            .Allow("modify", nameof(TestModel), [nameof(TestModel.DateOfBirth)])
            .Build();

        var formState = new PrivilegeFormState(
            Subject: nameof(TestModel),
            ReadAction: "view",
            UpdateAction: "modify"
        );

        var cut = Render<PrivilegeInputDate<DateTime>>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .AddCascadingValue(formState)
            .Add(p => p.Value, model.DateOfBirth)
            .Add(p => p.ValueExpression, () => model.DateOfBirth)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<DateTime>(this, v => model.DateOfBirth = v))
            // Not setting ReadAction/UpdateAction - should use FormState values
            .Add(p => p.Field, nameof(TestModel.DateOfBirth))
        );

        var input = cut.Find("input");
        input.GetAttribute("type").Should().Be("date");
        input.HasAttribute("readonly").Should().BeFalse();
    }

    [Fact]
    public void ComponentParametersOverrideFormState()
    {
        var model = new TestModel { DateOfBirth = new DateTime(1990, 5, 15) };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("componentRead", "ComponentSubject", [nameof(TestModel.DateOfBirth)])
            .Allow("componentUpdate", "ComponentSubject", [nameof(TestModel.DateOfBirth)])
            .Build();

        var formState = new PrivilegeFormState(
            Subject: "FormSubject",
            ReadAction: "formRead",
            UpdateAction: "formUpdate"
        );

        var cut = Render<PrivilegeInputDate<DateTime>>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .AddCascadingValue(formState)
            .Add(p => p.Value, model.DateOfBirth)
            .Add(p => p.ValueExpression, () => model.DateOfBirth)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<DateTime>(this, v => model.DateOfBirth = v))
            // Component parameters override FormState
            .Add(p => p.Subject, "ComponentSubject")
            .Add(p => p.ReadAction, "componentRead")
            .Add(p => p.UpdateAction, "componentUpdate")
            .Add(p => p.Field, nameof(TestModel.DateOfBirth))
        );

        var input = cut.Find("input");
        input.GetAttribute("type").Should().Be("date");
        input.HasAttribute("readonly").Should().BeFalse();
    }

    [Fact]
    public void RendersCorrectly_WhenValueChanges()
    {
        var model = new TestModel { DateOfBirth = new DateTime(1990, 5, 15) };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", nameof(TestModel), [nameof(TestModel.DateOfBirth)])
            .Allow("update", nameof(TestModel), [nameof(TestModel.DateOfBirth)])
            .Build();

        var cut = Render<PrivilegeInputDate<DateTime>>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.DateOfBirth)
            .Add(p => p.ValueExpression, () => model.DateOfBirth)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<DateTime>(this, v => model.DateOfBirth = v))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.DateOfBirth))
        );

        var input = cut.Find("input");
        input.GetAttribute("type").Should().Be("date");

        // Update value
        var newDate = new DateTime(2000, 12, 25);
        cut.Render(ps => ps.Add(p => p.Value, newDate));

        input = cut.Find("input");
        input.GetAttribute("type").Should().Be("date");
    }

    [Fact]
    public void NullableDateTime_NullValue_RendersEmpty()
    {
        DateTime? nullableDate = null;
        var model = new { DateOfBirth = nullableDate };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", model.GetType().Name, [nameof(model.DateOfBirth)])
            .Allow("update", model.GetType().Name, [nameof(model.DateOfBirth)])
            .Build();

        var cut = Render<PrivilegeInputDate<DateTime?>>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, nullableDate)
            .Add(p => p.ValueExpression, () => model.DateOfBirth)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<DateTime?>(this, v => { }))
            .Add(p => p.Subject, model.GetType().Name)
            .Add(p => p.Field, nameof(model.DateOfBirth))
        );

        var input = cut.Find("input");
        input.GetAttribute("type").Should().Be("date");
        input.HasAttribute("readonly").Should().BeFalse();
    }
}
