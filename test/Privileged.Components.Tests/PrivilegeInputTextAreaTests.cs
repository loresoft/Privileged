using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Privileged.Components.Tests;

public class PrivilegeInputTextAreaTests : TestContext
{
    [Fact]
    public void Renders_TextArea_When_Read_And_Update_Allowed()
    {
        var model = new TestModel { Description = "Desc" };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", nameof(TestModel), [nameof(TestModel.Description)])
            .Allow("update", nameof(TestModel), [nameof(TestModel.Description)])
            .Build();

        var cut = RenderComponent<PrivilegeInputTextArea>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Description)
            .Add(p => p.ValueExpression, () => model.Description)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, v => model.Description = v!))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.Description))
        );

        cut.Find("textarea").HasAttribute("readonly").Should().BeFalse();
    }

    [Fact]
    public void TextArea_Readonly_When_Update_Denied()
    {
        var model = new TestModel { Description = "Desc" };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder()
            .Allow("read", nameof(TestModel), [nameof(TestModel.Description)])
            .Build();

        var cut = RenderComponent<PrivilegeInputTextArea>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Description)
            .Add(p => p.ValueExpression, () => model.Description)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, v => model.Description = v!))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.Description))
        );

        cut.Find("textarea").HasAttribute("readonly").Should().BeTrue();
    }

    [Fact]
    public void TextArea_Masked_When_Read_Denied()
    {
        var model = new TestModel { Description = "Desc" };
        var editContext = new EditContext(model);
        var ctx = new PrivilegeBuilder().Build();

        var cut = RenderComponent<PrivilegeInputTextArea>(ps => ps
            .AddCascadingValue(editContext)
            .AddCascadingValue(ctx)
            .Add(p => p.Value, model.Description)
            .Add(p => p.ValueExpression, () => model.Description)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, v => model.Description = v!))
            .Add(p => p.Subject, nameof(TestModel))
            .Add(p => p.Field, nameof(TestModel.Description))
        );

        var input = cut.Find("input[type=password]");
        input.Should().NotBeNull();
    }

    [Fact]
    public void Throws_When_No_PrivilegeContext_Provided()
    {
        var model = new TestModel { Description = "Desc" };
        var editContext = new EditContext(model);

        var exception = Assert.Throws<System.InvalidOperationException>(() =>
        {
            RenderComponent<PrivilegeInputTextArea>(ps => ps
                .AddCascadingValue(editContext)
                .Add(p => p.Value, model.Description)
                .Add(p => p.ValueExpression, () => model.Description)
                .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, v => model.Description = v!))
                .Add(p => p.Subject, nameof(TestModel))
                .Add(p => p.Field, nameof(TestModel.Description))
            );
        });

        exception.Message.Should().Contain("PrivilegeContext");
    }
}
