namespace Privileged.Components.Tests;

public class PrivilegeViewTests : TestContext
{
    [Fact]
    public void AuthorizedChildContent()
    {
        var context = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Build();

        var cut = RenderComponent<PrivilegeView>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "read")
            .Add(p => p.Subject, "Post")
            .Add(p => p.ChildContent, _ => "<p>Allowed</p>")
        );

        Assert.True(cut.Instance.IsAllowed);

        cut.Find("p").MarkupMatches("<p>Allowed</p>");
    }

    [Fact]
    public void AuthorizedAuthorizedContent()
    {
        var context = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Build();

        var cut = RenderComponent<PrivilegeView>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "read")
            .Add(p => p.Subject, "Post")
            .Add(p => p.Allowed, _ => "<p>Allowed</p>")
            .Add(p => p.Forbidden, _ => "<p>Forbidden</p>")
        );

        Assert.True(cut.Instance.IsAllowed);

        cut.Find("p").MarkupMatches("<p>Allowed</p>");
    }

    [Fact]
    public void AuthorizedForbiddenContent()
    {
        var context = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Build();

        var cut = RenderComponent<PrivilegeView>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "update")
            .Add(p => p.Subject, "Post")
            .Add(p => p.Allowed, _ => "<p>Allowed</p>")
            .Add(p => p.Forbidden, _ => "<p>Forbidden</p>")
        );

        Assert.False(cut.Instance.IsAllowed);

        cut.Find("p").MarkupMatches("<p>Forbidden</p>");
    }

    [Fact]
    public void AuthorizedForbiddenChildContent()
    {
        var context = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Build();

        var cut = RenderComponent<PrivilegeView>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "update")
            .Add(p => p.Subject, "Post")
            .Add(p => p.ChildContent, _ => "<p>Allowed</p>")
        );

        Assert.False(cut.Instance.IsAllowed);

        cut.MarkupMatches(string.Empty);
    }

}
