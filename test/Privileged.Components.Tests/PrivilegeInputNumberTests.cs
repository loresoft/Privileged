using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Privileged.Components.Tests;

public class PrivilegeInputNumberTests : TestContext
{
    [Fact]
    public void Renders_Number_Input_When_Read_And_Update_Allowed()
    {
        var model = new TestModel { Age = 42 };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", nameof(TestModel), [nameof(TestModel.Age)])
            .Allow("update", nameof(TestModel), [nameof(TestModel.Age)])
            .Build();

        var cut = RenderComponent<PrivilegeInputNumber<int>>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Age)
            .Add(p => p.ValueExpression, () => model.Age)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<int>(this, v => model.Age = v))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.Age))
        );

        var input = cut.Find("input");
        input.GetAttribute("type").Should().Be("number");
        input.HasAttribute("readonly").Should().BeFalse();
    }

    [Fact]
    public void Sets_Readonly_When_Read_Allowed_Update_Denied()
    {
        var model = new TestModel { Age = 42 };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", nameof(TestModel), [nameof(TestModel.Age)])
            .Build();

        var cut = RenderComponent<PrivilegeInputNumber<int>>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Age)
            .Add(p => p.ValueExpression, () => model.Age)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<int>(this, v => model.Age = v))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.Age))
        );

        var input = cut.Find("input");
        input.GetAttribute("type").Should().Be("number");
        input.HasAttribute("readonly").Should().BeTrue();
    }

    [Fact]
    public void Masks_When_Read_Denied_Update_Denied()
    {
        var model = new TestModel { Age = 42 };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder().Build();

        var cut = RenderComponent<PrivilegeInputNumber<int>>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Age)
            .Add(p => p.ValueExpression, () => model.Age)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<int>(this, v => model.Age = v))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.Age))
        );

        var input = cut.Find("input");
        input.GetAttribute("type").Should().Be("password");
        input.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void Throws_When_No_PrivilegeContext_Provided()
    {
        var model = new TestModel { Age = 42 };
        var editContext = new EditContext(model);

        var exception = Assert.Throws<System.InvalidOperationException>(() =>
        {
            RenderComponent<PrivilegeInputNumber<int>>(ps => ps
                .AddCascadingValue(editContext)
                .Add(p => p.Value, model.Age)
                .Add(p => p.ValueExpression, () => model.Age)
                .Add(p => p.ValueChanged, EventCallback.Factory.Create<int>(this, v => model.Age = v))
                .Add(p => p.Subject, nameof(TestModel))
                .Add(p => p.Field, nameof(TestModel.Age))
            );
        });

        exception.Message.Should().Contain("PrivilegeContext");
    }
}
