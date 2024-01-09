namespace Privileged.Components.Tests;

public class PrivilegedViewTests : TestContext
{
    [Fact]
    public void AuthorizedChildContent()
    {
        var context = new AuthorizationBuilder()
            .Allow("read", "Post")
            .Build();

        var cut = RenderComponent<PrivilegedView>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "read")
            .Add(p => p.Subject, "Post")
            .Add(p => p.ChildContent, _ => "<p>Authorized</p>")
        );

        Assert.True(cut.Instance.IsAuthorized);

        cut.Find("p").MarkupMatches("<p>Authorized</p>");
    }

    [Fact]
    public void AuthorizedAuthorizedContent()
    {
        var context = new AuthorizationBuilder()
            .Allow("read", "Post")
            .Build();

        var cut = RenderComponent<PrivilegedView>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "read")
            .Add(p => p.Subject, "Post")
            .Add(p => p.Authorized, _ => "<p>Authorized</p>")
            .Add(p => p.Forbidden, _ => "<p>Forbidden</p>")
        );

        Assert.True(cut.Instance.IsAuthorized);

        cut.Find("p").MarkupMatches("<p>Authorized</p>");
    }

    [Fact]
    public void AuthorizedForbiddenContent()
    {
        var context = new AuthorizationBuilder()
            .Allow("read", "Post")
            .Build();

        var cut = RenderComponent<PrivilegedView>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "update")
            .Add(p => p.Subject, "Post")
            .Add(p => p.Authorized, _ => "<p>Authorized</p>")
            .Add(p => p.Forbidden, _ => "<p>Forbidden</p>")
        );

        Assert.False(cut.Instance.IsAuthorized);

        cut.Find("p").MarkupMatches("<p>Forbidden</p>");
    }

    [Fact]
    public void AuthorizedForbiddenChildContent()
    {
        var context = new AuthorizationBuilder()
            .Allow("read", "Post")
            .Build();

        var cut = RenderComponent<PrivilegedView>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "update")
            .Add(p => p.Subject, "Post")
            .Add(p => p.ChildContent, _ => "<p>Authorized</p>")
        );

        Assert.False(cut.Instance.IsAuthorized);

        cut.MarkupMatches(string.Empty);
    }

}
