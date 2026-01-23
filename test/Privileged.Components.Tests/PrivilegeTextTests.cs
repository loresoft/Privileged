namespace Privileged.Components.Tests;

public class PrivilegeTextTests : BunitContext
{
    [Fact]
    public void AuthorizedWithText_ShowsMaskedText()
    {
        var context = new PrivilegeBuilder()
            .Allow("read", "Secret")
            .Build();

        var cut = Render<PrivilegeText>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "read")
            .Add(p => p.Subject, "Secret")
            .Add(p => p.Text, "Sensitive Data")
        );

        var button = cut.Find("button");
        Assert.NotNull(button);
        
        cut.Find("div div").MarkupMatches("<div>••••••</div>");
    }

    [Fact]
    public void AuthorizedWithText_ToggleShowsRealText()
    {
        var context = new PrivilegeBuilder()
            .Allow("read", "Secret")
            .Build();

        var cut = Render<PrivilegeText>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "read")
            .Add(p => p.Subject, "Secret")
            .Add(p => p.Text, "Sensitive Data")
        );

        var button = cut.Find("button");
        button.Click();

        cut.Find("div div").MarkupMatches("<div>Sensitive Data</div>");
    }

    [Fact]
    public void AuthorizedWithText_ToggleTwiceHidesText()
    {
        var context = new PrivilegeBuilder()
            .Allow("read", "Secret")
            .Build();

        var cut = Render<PrivilegeText>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "read")
            .Add(p => p.Subject, "Secret")
            .Add(p => p.Text, "Sensitive Data")
        );

        var button = cut.Find("button");
        button.Click();
        button.Click();

        cut.Find("div div").MarkupMatches("<div>••••••</div>");
    }

    [Fact]
    public void AuthorizedWithChildContent_ShowsMaskedText()
    {
        var context = new PrivilegeBuilder()
            .Allow("read", "Secret")
            .Build();

        var cut = Render<PrivilegeText>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "read")
            .Add(p => p.Subject, "Secret")
            .Add(p => p.ChildContent, "<span>Sensitive Content</span>")
        );

        var button = cut.Find("button");
        Assert.NotNull(button);

        cut.Find("div div").MarkupMatches("<div>••••••</div>");
    }

    [Fact]
    public void AuthorizedWithChildContent_ToggleShowsContent()
    {
        var context = new PrivilegeBuilder()
            .Allow("read", "Secret")
            .Build();

        var cut = Render<PrivilegeText>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "read")
            .Add(p => p.Subject, "Secret")
            .Add(p => p.ChildContent, "<span>Sensitive Content</span>")
        );

        var button = cut.Find("button");
        button.Click();

        cut.Find("span").MarkupMatches("<span>Sensitive Content</span>");
    }

    [Fact]
    public void CustomMaskedText_ShowsCustomMask()
    {
        var context = new PrivilegeBuilder()
            .Allow("read", "Secret")
            .Build();

        var cut = Render<PrivilegeText>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "read")
            .Add(p => p.Subject, "Secret")
            .Add(p => p.Text, "Sensitive Data")
            .Add(p => p.MaskedText, "***REDACTED***")
        );

        cut.Find("div div").MarkupMatches("<div>***REDACTED***</div>");
    }

    [Fact]
    public void CustomMaskedContent_ShowsCustomMaskedContent()
    {
        var context = new PrivilegeBuilder()
            .Allow("read", "Secret")
            .Build();

        var cut = Render<PrivilegeText>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "read")
            .Add(p => p.Subject, "Secret")
            .Add(p => p.Text, "Sensitive Data")
            .Add(p => p.MaskedContent, "<em>Hidden</em>")
        );

        cut.Find("em").MarkupMatches("<em>Hidden</em>");
    }

    [Fact]
    public void CustomMaskedContent_ToggleShowsRealContent()
    {
        var context = new PrivilegeBuilder()
            .Allow("read", "Secret")
            .Build();

        var cut = Render<PrivilegeText>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "read")
            .Add(p => p.Subject, "Secret")
            .Add(p => p.ChildContent, "<span>Sensitive Content</span>")
            .Add(p => p.MaskedContent, "<em>Hidden</em>")
        );

        cut.Find("button").Click();

        cut.Find("span").MarkupMatches("<span>Sensitive Content</span>");
    }

    [Fact]
    public void UnauthorizedWithText_ShowsMaskedTextWithoutButton()
    {
        var context = new PrivilegeBuilder()
            .Forbid("read", "Secret")
            .Build();

        var cut = Render<PrivilegeText>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "read")
            .Add(p => p.Subject, "Secret")
            .Add(p => p.Text, "Sensitive Data")
        );

        Assert.Throws<Bunit.ElementNotFoundException>(() => cut.Find("button"));
        cut.Find("div div").MarkupMatches("<div>••••••</div>");
    }

    [Fact]
    public void UnauthorizedWithChildContent_ShowsMaskedContent()
    {
        var context = new PrivilegeBuilder()
            .Forbid("read", "Secret")
            .Build();

        var cut = Render<PrivilegeText>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "read")
            .Add(p => p.Subject, "Secret")
            .Add(p => p.ChildContent, "<span>Sensitive Content</span>")
            .Add(p => p.MaskedContent, "<em>Unauthorized</em>")
        );

        cut.Find("em").MarkupMatches("<em>Unauthorized</em>");
    }

    [Fact]
    public void EmptySubject_AssumeAllPrivileges_ShowsToggleButton()
    {
        var context = new PrivilegeBuilder()
            .Build();

        var cut = Render<PrivilegeText>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "read")
            .Add(p => p.Subject, "")
            .Add(p => p.Text, "Public Data")
        );

        var button = cut.Find("button");
        Assert.NotNull(button);
    }

    [Fact]
    public void NullSubject_AssumeAllPrivileges_ShowsToggleButton()
    {
        var context = new PrivilegeBuilder()
            .Build();

        var cut = Render<PrivilegeText>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "read")
            .Add(p => p.Subject, (string?)null)
            .Add(p => p.Text, "Public Data")
        );

        var button = cut.Find("button");
        Assert.NotNull(button);
    }

    [Fact]
    public void WhitespaceSubject_AssumeAllPrivileges_ShowsToggleButton()
    {
        var context = new PrivilegeBuilder()
            .Build();

        var cut = Render<PrivilegeText>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "read")
            .Add(p => p.Subject, "   ")
            .Add(p => p.Text, "Public Data")
        );

        var button = cut.Find("button");
        Assert.NotNull(button);
    }

    [Fact]
    public void WithQualifier_ChecksPermissionWithQualifier()
    {
        var context = new PrivilegeBuilder()
            .Allow("read", "User", ["email"])
            .Build();

        var cut = Render<PrivilegeText>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "read")
            .Add(p => p.Subject, "User")
            .Add(p => p.Qualifier, "email")
            .Add(p => p.Text, "user@example.com")
        );

        var button = cut.Find("button");
        button.Click();

        cut.Find("div div").MarkupMatches("<div>user@example.com</div>");
    }

    [Fact]
    public void WithQualifier_DeniedForDifferentQualifier()
    {
        var context = new PrivilegeBuilder()
            .Allow("read", "User", ["name"])
            .Forbid("read", "User", ["email"])
            .Build();

        var cut = Render<PrivilegeText>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "read")
            .Add(p => p.Subject, "User")
            .Add(p => p.Qualifier, "email")
            .Add(p => p.Text, "user@example.com")
        );

        Assert.Throws<Bunit.ElementNotFoundException>(() => cut.Find("button"));
    }

    [Fact]
    public void ButtonHasCorrectAriaLabel_WhenHidden()
    {
        var context = new PrivilegeBuilder()
            .Allow("read", "Secret")
            .Build();

        var cut = Render<PrivilegeText>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "read")
            .Add(p => p.Subject, "Secret")
            .Add(p => p.Text, "Sensitive Data")
        );

        var button = cut.Find("button");
        Assert.Equal("Show", button.GetAttribute("aria-label"));
    }

    [Fact]
    public void ButtonHasCorrectAriaLabel_WhenShown()
    {
        var context = new PrivilegeBuilder()
            .Allow("read", "Secret")
            .Build();

        var cut = Render<PrivilegeText>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "read")
            .Add(p => p.Subject, "Secret")
            .Add(p => p.Text, "Sensitive Data")
        );

        var button = cut.Find("button");
        button.Click();

        Assert.Equal("Hide", button.GetAttribute("aria-label"));
    }

    [Fact]
    public void DefaultAction_IsRead()
    {
        var context = new PrivilegeBuilder()
            .Allow("read", "Secret")
            .Build();

        var cut = Render<PrivilegeText>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Subject, "Secret")
            .Add(p => p.Text, "Sensitive Data")
        );

        var button = cut.Find("button");
        Assert.NotNull(button);
    }
}
