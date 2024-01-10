namespace Privileged.Tests;

public class PrivilegeContextTests
{
    [Fact]
    public void AllowByDefault()
    {
        var context = new PrivilegeBuilder()
            .Allow("test", PrivilegeSubjects.All)
            .Allow(PrivilegeActions.All, "Post")
            .Forbid("publish", "Post")
            .Build();

        context.Allowed("read", "Post").Should().BeTrue();
        context.Allowed("update", "Post").Should().BeTrue();
        context.Allowed("archive", "Post").Should().BeTrue();
        context.Allowed(null, "Post").Should().BeFalse();
        context.Allowed("archive", null).Should().BeFalse();
        context.Allowed("read", "User").Should().BeFalse();
        context.Allowed("delete", "Post").Should().BeTrue();
        context.Allowed("publish", "Post").Should().BeFalse();
        context.Allowed("test", "User").Should().BeTrue();
        context.Allowed("test", "Post").Should().BeTrue();
    }

    [Fact]
    public void AllowConstructRules()
    {
        var context = new PrivilegeBuilder()
            .Allow("read", "Article")
            .Allow("update", "Article")
            .Build();

        context.Allowed("read", "Article").Should().BeTrue();
        context.Allowed("update", "Article").Should().BeTrue();
        context.Allowed("delete", "Article").Should().BeFalse();
    }

    [Fact]
    public void AllowSpecifyMultipleActions()
    {
        var context = new PrivilegeBuilder()
            .Allow(["read", "update"], "Post")
            .Build();

        context.Allowed("read", "Post").Should().BeTrue();
        context.Allowed("update", "Post").Should().BeTrue();
        context.Allowed("delete", "Post").Should().BeFalse();
    }

    [Fact]
    public void AllowSpecifyMultipleSubjects()
    {
        var context = new PrivilegeBuilder()
            .Allow("read", ["Post", "User"])
            .Build();

        context.Allowed("read", "Post").Should().BeTrue();
        context.Allowed("read", "User").Should().BeTrue();
        context.Allowed("read", "Article").Should().BeFalse();
    }

    [Fact]
    public void AllowRulesWithFields()
    {
        var context = new PrivilegeBuilder()
            .Allow("read", "Post", ["title", "id"])
            .Allow("read", "User")
            .Build();

        context.Allowed("read", "Post").Should().BeTrue();
        context.Allowed("read", "Post", "id").Should().BeTrue();
        context.Allowed("read", "Post", "title").Should().BeTrue();
        context.Allowed("read", "Post", "ssn").Should().BeFalse();

        context.Allowed("read", "User").Should().BeTrue();
        context.Allowed("read", "User", "id").Should().BeTrue();
    }
}
