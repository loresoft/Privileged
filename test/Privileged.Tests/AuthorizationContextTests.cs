using System.Text.Json;

namespace Privileged.Tests;

public class AuthorizationContextTests
{
    [Fact]
    public void AllowByDefault()
    {
        var context = new AuthorizationBuilder()
            .Allow("test", AuthorizationSubjects.All)
            .Allow(AuthorizationActions.All, "Post")
            .Forbid("publish", "Post")
            .Build();

        context.Authorized("read", "Post").Should().BeTrue();
        context.Authorized("update", "Post").Should().BeTrue();
        context.Authorized("archive", "Post").Should().BeTrue();
        context.Authorized(null, "Post").Should().BeFalse();
        context.Authorized("archive", null).Should().BeFalse();
        context.Authorized("read", "User").Should().BeFalse();
        context.Authorized("delete", "Post").Should().BeTrue();
        context.Authorized("publish", "Post").Should().BeFalse();
        context.Authorized("test", "User").Should().BeTrue();
        context.Authorized("test", "Post").Should().BeTrue();
    }

    [Fact]
    public void AllowConstructRules()
    {
        var context = new AuthorizationBuilder()
            .Allow("read", "Article")
            .Allow("update", "Article")
            .Build();

        context.Authorized("read", "Article").Should().BeTrue();
        context.Authorized("update", "Article").Should().BeTrue();
        context.Authorized("delete", "Article").Should().BeFalse();
    }

    [Fact]
    public void AllowSpecifyMultipleActions()
    {
        var context = new AuthorizationBuilder()
            .Allow(["read", "update"], "Post")
            .Build();

        context.Authorized("read", "Post").Should().BeTrue();
        context.Authorized("update", "Post").Should().BeTrue();
        context.Authorized("delete", "Post").Should().BeFalse();
    }

    [Fact]
    public void AllowSpecifyMultipleSubjects()
    {
        var context = new AuthorizationBuilder()
            .Allow("read", ["Post", "User"])
            .Build();

        context.Authorized("read", "Post").Should().BeTrue();
        context.Authorized("read", "User").Should().BeTrue();
        context.Authorized("read", "Article").Should().BeFalse();
    }

    [Fact]
    public void AllowRulesWithFields()
    {
        var context = new AuthorizationBuilder()
            .Allow("read", "Post", ["title", "id"])
            .Allow("read", "User")
            .Build();

        context.Authorized("read", "Post").Should().BeTrue();
        context.Authorized("read", "Post", "id").Should().BeTrue();
        context.Authorized("read", "Post", "title").Should().BeTrue();
        context.Authorized("read", "Post", "ssn").Should().BeFalse();

        context.Authorized("read", "User").Should().BeTrue();
        context.Authorized("read", "User", "id").Should().BeTrue();
    }
}
