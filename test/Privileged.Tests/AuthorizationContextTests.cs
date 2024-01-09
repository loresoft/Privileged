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

        Assert.True(context.Authorized("read", "Post"));
        Assert.True(context.Authorized("update", "Post"));
        Assert.True(context.Authorized("archive", "Post"));
        Assert.False(context.Authorized(null, "Post"));
        Assert.False(context.Authorized("archive", null));
        Assert.False(context.Authorized("read", "User"));
        Assert.True(context.Authorized("delete", "Post"));
        Assert.False(context.Authorized("publish", "Post"));
        Assert.True(context.Authorized("test", "User"));
        Assert.True(context.Authorized("test", "Post"));
    }

    [Fact]
    public void AllowConstructRules()
    {
        var context = new AuthorizationBuilder()
            .Allow("read", "Article")
            .Allow("update", "Article")
            .Build();

        Assert.True(context.Authorized("read", "Article"));
        Assert.True(context.Authorized("update", "Article"));
        Assert.False(context.Authorized("delete", "Article"));
    }

    [Fact]
    public void AllowSpecifyMultipleActions()
    {
        var context = new AuthorizationBuilder()
            .Allow(["read", "update"], "Post")
            .Build();

        Assert.True(context.Authorized("read", "Post"));
        Assert.True(context.Authorized("update", "Post"));
        Assert.False(context.Authorized("delete", "Post"));
    }

    [Fact]
    public void AllowSpecifyMultipleSubjects()
    {
        var context = new AuthorizationBuilder()
            .Allow("read", ["Post", "User"])
            .Build();

        Assert.True(context.Authorized("read", "Post"));
        Assert.True(context.Authorized("read", "User"));
        Assert.False(context.Authorized("read", "Article"));
    }

    [Fact]
    public void AllowRulesWithFields()
    {
        var context = new AuthorizationBuilder()
            .Allow("read", "Post", ["title", "id"])
            .Allow("read", "User")
            .Build();

        Assert.True(context.Authorized("read", "Post"));
        Assert.True(context.Authorized("read", "Post", "id"));
        Assert.True(context.Authorized("read", "Post", "title"));
        Assert.False(context.Authorized("read", "Post", "ssn"));

        Assert.True(context.Authorized("read", "User"));
        Assert.True(context.Authorized("read", "User", "id"));
    }
  

}
