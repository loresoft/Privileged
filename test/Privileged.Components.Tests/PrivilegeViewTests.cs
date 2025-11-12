namespace Privileged.Components.Tests;

public class PrivilegeViewTests : BunitContext
{
    [Fact]
    public void AuthorizedChildContent()
    {
        var context = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Build();

        var cut = Render<PrivilegeView>(parameters => parameters
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

        var cut = Render<PrivilegeView>(parameters => parameters
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

        var cut = Render<PrivilegeView>(parameters => parameters
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

        var cut = Render<PrivilegeView>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "update")
            .Add(p => p.Subject, "Post")
            .Add(p => p.ChildContent, _ => "<p>Allowed</p>")
        );

        Assert.False(cut.Instance.IsAllowed);

        cut.MarkupMatches(string.Empty);
    }

    // Tests for empty subject assuming all privileges
    [Fact]
    public void EmptySubject_AssumeAllPrivileges_RendersChildContent()
    {
        var context = new PrivilegeBuilder()
            .Build(); // No specific permissions

        var cut = Render<PrivilegeView>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "read")
            .Add(p => p.Subject, "") // Empty subject
            .Add(p => p.ChildContent, _ => "<p>Allowed with empty subject</p>")
        );

        Assert.True(cut.Instance.IsAllowed);
        cut.Find("p").MarkupMatches("<p>Allowed with empty subject</p>");
    }

    [Fact]
    public void NullSubject_AssumeAllPrivileges_RendersChildContent()
    {
        var context = new PrivilegeBuilder()
            .Build(); // No specific permissions

        var cut = Render<PrivilegeView>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "read")
            .Add(p => p.Subject, (string?)null) // Null subject
            .Add(p => p.ChildContent, _ => "<p>Allowed with null subject</p>")
        );

        Assert.True(cut.Instance.IsAllowed);
        cut.Find("p").MarkupMatches("<p>Allowed with null subject</p>");
    }

    [Fact]
    public void WhitespaceSubject_AssumeAllPrivileges_RendersChildContent()
    {
        var context = new PrivilegeBuilder()
            .Build(); // No specific permissions

        var cut = Render<PrivilegeView>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "read")
            .Add(p => p.Subject, "   ") // Whitespace subject
            .Add(p => p.ChildContent, _ => "<p>Allowed with whitespace subject</p>")
        );

        Assert.True(cut.Instance.IsAllowed);
        cut.Find("p").MarkupMatches("<p>Allowed with whitespace subject</p>");
    }

    [Fact]
    public void EmptySubject_WithAllowedTemplate_RendersAllowedContent()
    {
        var context = new PrivilegeBuilder()
            .Build(); // No specific permissions

        var cut = Render<PrivilegeView>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "delete")
            .Add(p => p.Subject, "") // Empty subject
            .Add(p => p.Allowed, _ => "<p>Allowed</p>")
            .Add(p => p.Forbidden, _ => "<p>Forbidden</p>")
        );

        Assert.True(cut.Instance.IsAllowed);
        cut.Find("p").MarkupMatches("<p>Allowed</p>");
    }

    [Fact]
    public void EmptySubject_WithQualifier_StillAssumeAllPrivileges()
    {
        var context = new PrivilegeBuilder()
            .Build(); // No specific permissions

        var cut = Render<PrivilegeView>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "update")
            .Add(p => p.Subject, "") // Empty subject
            .Add(p => p.Qualifier, "title") // With qualifier
            .Add(p => p.ChildContent, _ => "<p>Allowed with qualifier</p>")
        );

        Assert.True(cut.Instance.IsAllowed);
        cut.Find("p").MarkupMatches("<p>Allowed with qualifier</p>");
    }

    // Tests for Subjects parameter
    [Fact]
    public void SubjectsParameter_WithAllowedSubject_RendersChildContent()
    {
        var context = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Allow("read", "Comment")
            .Build();

        var cut = Render<PrivilegeView>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "read")
            .Add(p => p.Subjects, new[] { "Post", "User", "Comment" })
            .Add(p => p.ChildContent, _ => "<p>Any allowed</p>")
        );

        Assert.True(cut.Instance.IsAllowed);
        cut.Find("p").MarkupMatches("<p>Any allowed</p>");
    }

    [Fact]
    public void SubjectsParameter_WithNoAllowedSubjects_RendersForbiddenContent()
    {
        var context = new PrivilegeBuilder()
            .Allow("read", "Admin")
            .Build();

        var cut = Render<PrivilegeView>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "read")
            .Add(p => p.Subjects, new[] { "Post", "User", "Comment" })
            .Add(p => p.Allowed, _ => "<p>Allowed</p>")
            .Add(p => p.Forbidden, _ => "<p>Forbidden</p>")
        );

        Assert.False(cut.Instance.IsAllowed);
        cut.Find("p").MarkupMatches("<p>Forbidden</p>");
    }

    [Fact]
    public void SubjectsParameter_TakesPrecedenceOverSubject()
    {
        var context = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Forbid("read", "User")
            .Build();

        var cut = Render<PrivilegeView>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "read")
            .Add(p => p.Subject, "User") // This should be ignored
            .Add(p => p.Subjects, new[] { "Post" }) // This takes precedence
            .Add(p => p.ChildContent, _ => "<p>Post allowed</p>")
        );

        Assert.True(cut.Instance.IsAllowed);
        cut.Find("p").MarkupMatches("<p>Post allowed</p>");
    }

    [Fact]
    public void SubjectsParameter_WithEmptyCollection_UsesSubjectInstead()
    {
        var context = new PrivilegeBuilder()
            .Allow("read", "User")
            .Build();

        var cut = Render<PrivilegeView>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "read")
            .Add(p => p.Subject, "User")
            .Add(p => p.Subjects, new string[0])
            .Add(p => p.ChildContent, _ => "<p>User allowed</p>")
        );

        Assert.True(cut.Instance.IsAllowed);
        cut.Find("p").MarkupMatches("<p>User allowed</p>");
    }

    [Fact]
    public void SubjectsParameter_WithSingleAllowedSubject_RendersContent()
    {
        var context = new PrivilegeBuilder()
            .Forbid("update", "Post")
            .Forbid("update", "User")
            .Allow("update", "Comment")
            .Build();

        var cut = Render<PrivilegeView>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "update")
            .Add(p => p.Subjects, new[] { "Post", "User", "Comment" })
            .Add(p => p.ChildContent, _ => "<p>Comment allowed</p>")
        );

        Assert.True(cut.Instance.IsAllowed);
        cut.Find("p").MarkupMatches("<p>Comment allowed</p>");
    }

    [Fact]
    public void SubjectsParameter_WithWildcardPermissions_RendersContent()
    {
        var context = new PrivilegeBuilder()
            .Allow("read", PrivilegeRule.Any) // Allow read on all subjects
            .Build();

        var cut = Render<PrivilegeView>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "read")
            .Add(p => p.Subjects, new[] { "Post", "User", "Comment", "Admin" })
            .Add(p => p.ChildContent, _ => "<p>All subjects allowed</p>")
        );

        Assert.True(cut.Instance.IsAllowed);
        cut.Find("p").MarkupMatches("<p>All subjects allowed</p>");
    }

    [Fact]
    public void SubjectsParameter_WithEmptySubjectsAndEmptySubject_AssumeAllPrivileges()
    {
        var context = new PrivilegeBuilder()
            .Build(); // No specific permissions

        var cut = Render<PrivilegeView>(parameters => parameters
            .AddCascadingValue(context)
            .Add(p => p.Action, "read")
            .Add(p => p.Subject, "") // Empty subject
            .Add(p => p.Subjects, new string[0]) // Empty subjects collection
            .Add(p => p.ChildContent, _ => "<p>Empty subject fallback</p>")
        );

        Assert.True(cut.Instance.IsAllowed);
        cut.Find("p").MarkupMatches("<p>Empty subject fallback</p>");
    }
}
