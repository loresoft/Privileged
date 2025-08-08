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
        context.Allowed("read", "Article").Should().BeFalse();
    }

    [Fact]
    public void AllowSpecifyMultipleSubjects()
    {
        var context = new PrivilegeBuilder()
            .Allow("read", ["Post", "User"])
            .Build();

        context.Allowed("read", "Post").Should().BeTrue();
        context.Allowed("read", "User").Should().BeTrue();
        context.Allowed("update", "Post").Should().BeFalse();
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

    [Fact]
    public void AllowAliasActions()
    {
        var context = new PrivilegeBuilder()
            .Alias("manage", ["read", "create", "update"], PrivilegeMatch.Action)
            .Allow("manage", "Post")
            .Build();

        context.Allowed("read", "Post").Should().BeTrue();
        context.Allowed("update", "Post").Should().BeTrue();
        context.Allowed("delete", "Post").Should().BeFalse();
        context.Allowed("read", "Article").Should().BeFalse();
    }

    [Fact]
    public void AllowAliasSubjects()
    {
        var context = new PrivilegeBuilder()
            .Alias("common", ["Post", "User"], PrivilegeMatch.Subject)
            .Allow("read", "common")
            .Build();

        context.Allowed("read", "Post").Should().BeTrue();
        context.Allowed("read", "User").Should().BeTrue();
        context.Allowed("read", "Article").Should().BeFalse();
    }

    [Fact]
    public void AllowAliasQualifiers()
    {
        var context = new PrivilegeBuilder()
            .Alias("fields", ["title", "id"], PrivilegeMatch.Qualifier)
            .Allow("read", "Post", ["fields"])
            .Build();

        context.Allowed("read", "Post").Should().BeTrue();
        context.Allowed("read", "Post", "title").Should().BeTrue();
        context.Allowed("read", "Post", "id").Should().BeTrue();

        context.Allowed("update", "Post", "id").Should().BeFalse();
        context.Allowed("read", "Post", "ssn").Should().BeFalse();
    }

    [Fact]
    public void DocumentationExample()
    {
        var context = new PrivilegeBuilder()
            .Alias("Manage", ["Create", "Update", "Delete"], PrivilegeMatch.Action)
            .Allow("Manage", "Project")                         // Allows all actions defined in the "Manage" alias
            .Allow("Read", "User")                              // Allows reading User
            .Allow("Update", "User", ["Profile", "Settings"])   // Allows updating User's Profile and Settings
            .Forbid("Delete", "User")                           // Forbids deleting User
            .Build();

        bool canCreateProject = context.Allowed("Create", "Project");           // true
        bool canReadUser = context.Allowed("Read", "User");                     // true
        bool canUpdateProfile = context.Allowed("Update", "User", "Profile");   // true
        bool canUpdatePassword = context.Allowed("Update", "User", "Password"); // false
        bool canDeleteUser = context.Allowed("Delete", "User");                 // false

        canCreateProject.Should().BeTrue();
        canReadUser.Should().BeTrue();
        canUpdateProfile.Should().BeTrue();
        canUpdatePassword.Should().BeFalse();
        canDeleteUser.Should().BeFalse();
    }
}
