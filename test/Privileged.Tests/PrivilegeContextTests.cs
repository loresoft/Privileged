namespace Privileged.Tests;

public class PrivilegeContextTests
{
    [Fact]
    public void AllowByDefault()
    {
        var context = new PrivilegeBuilder()
            .Allow("test", PrivilegeRule.Any)
            .Allow(PrivilegeRule.Any, "Post")
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

        context.Allowed("read", "Post").Should().BeFalse();
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

        context.Allowed("read", PrivilegeRule.Any).Should().BeTrue();
        context.Allowed(PrivilegeRule.Any, "User").Should().BeTrue();
    }

    [Fact]
    public void AllowAliasQualifiers()
    {
        var context = new PrivilegeBuilder()
            .Alias("fields", ["title", "id"], PrivilegeMatch.Qualifier)
            .Allow("read", "Post", ["fields"])
            .Build();

        context.Allowed("read", "Post").Should().BeFalse();
        context.Allowed("read", "Post", PrivilegeRule.Any).Should().BeTrue();
        context.Allowed("read", "Post", "title").Should().BeTrue();
        context.Allowed("read", "Post", "id").Should().BeTrue();

        context.Allowed("update", "Post", "id").Should().BeFalse();
        context.Allowed("read", "Post", "ssn").Should().BeFalse();
    }

    [Fact]
    public void PreventDuplicateRules()
    {
        var context = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Allow("read", "Post")  // Duplicate rule
            .Allow("update", "User")
            .Allow("update", "User") // Duplicate rule
            .Allow("read", "Post", ["title"])
            .Allow("read", "Post", ["title"]) // Duplicate rule with same qualifiers
            .Allow("read", "Post", ["title", "content"])
            .Allow("read", "Post", ["content", "title"]) // Different order, should be different rule
            .Forbid("delete", "Post")
            .Forbid("delete", "Post") // Duplicate forbid rule
            .Build();

        // Verify the rules work as expected
        context.Allowed("read", "Post").Should().BeTrue();
        context.Allowed("update", "User").Should().BeTrue();
        context.Allowed("read", "Post", "title").Should().BeTrue();
        context.Allowed("read", "Post", "content").Should().BeTrue();
        context.Allowed("delete", "Post").Should().BeFalse();

        // Verify that only unique rules are stored
        // We should have 5 unique rules: 
        // 1. Allow("read", "Post")
        // 2. Allow("update", "User") 
        // 3. Allow("read", "Post", ["title"])
        // 4. Allow("read", "Post", ["title", "content"])
        // 5. Allow("read", "Post", ["content", "title"]) - different order makes it unique
        // 6. Forbid("delete", "Post")
        context.Rules.Should().HaveCount(6);

        // Verify specific rule existence by checking if we can find the expected rules
        var readPostRule = context.Rules.FirstOrDefault(r => 
            r.Action == "read" && 
            r.Subject == "Post" && 
            (r.Qualifiers == null || r.Qualifiers.Count == 0) &&
            r.Denied != true);
        readPostRule.Should().NotBeNull();

        var updateUserRule = context.Rules.FirstOrDefault(r => 
            r.Action == "update" && 
            r.Subject == "User" && 
            (r.Qualifiers == null || r.Qualifiers.Count == 0) &&
            r.Denied != true);
        updateUserRule.Should().NotBeNull();

        var deletePostRule = context.Rules.FirstOrDefault(r => 
            r.Action == "delete" && 
            r.Subject == "Post" && 
            r.Denied == true);
        deletePostRule.Should().NotBeNull();
    }

    [Fact]
    public void PreventDuplicateAliases()
    {
        var context = new PrivilegeBuilder()
            .Alias("CRUD", ["Create", "Read", "Update", "Delete"], PrivilegeMatch.Action)
            .Alias("CRUD", ["Create", "Read", "Update", "Delete"], PrivilegeMatch.Action) // Exact duplicate
            .Alias("ContentTypes", ["Post", "Article", "Comment"], PrivilegeMatch.Subject)
            .Alias("ContentTypes", ["Post", "Article", "Comment"], PrivilegeMatch.Subject) // Exact duplicate
            .Alias("CRUD", ["Create", "Read", "Update"], PrivilegeMatch.Action) // Same alias name but different values - should be different
            .Alias("CRUD", ["Create", "Read", "Update", "Delete"], PrivilegeMatch.Subject) // Same alias name but different type - should be different
            .Allow("CRUD", "ContentTypes")
            .Build();

        // Verify aliases work as expected
        context.Allowed("Create", "Post").Should().BeTrue();
        context.Allowed("Read", "Article").Should().BeTrue();
        context.Allowed("Update", "Comment").Should().BeTrue();
        context.Allowed("Delete", "Post").Should().BeTrue();

        // Verify that only unique aliases are stored
        // We should have 4 unique aliases:
        // 1. Alias("CRUD", ["Create", "Read", "Update", "Delete"], PrivilegeMatch.Action)
        // 2. Alias("ContentTypes", ["Post", "Article", "Comment"], PrivilegeMatch.Subject) 
        // 3. Alias("CRUD", ["Create", "Read", "Update"], PrivilegeMatch.Action) - different values
        // 4. Alias("CRUD", ["Create", "Read", "Update", "Delete"], PrivilegeMatch.Subject) - different type
        context.Aliases.Should().HaveCount(4);

        // Verify specific alias existence
        var crudActionAlias = context.Aliases.FirstOrDefault(a => 
            a.Alias == "CRUD" && 
            a.Type == PrivilegeMatch.Action && 
            a.Values.Count == 4 &&
            a.Values.Contains("Create") && a.Values.Contains("Read") && 
            a.Values.Contains("Update") && a.Values.Contains("Delete"));
        crudActionAlias.Should().NotBeNull();

        var contentTypesAlias = context.Aliases.FirstOrDefault(a => 
            a.Alias == "ContentTypes" && 
            a.Type == PrivilegeMatch.Subject &&
            a.Values.Count == 3);
        contentTypesAlias.Should().NotBeNull();

        var partialCrudAlias = context.Aliases.FirstOrDefault(a => 
            a.Alias == "CRUD" && 
            a.Type == PrivilegeMatch.Action && 
            a.Values.Count == 3);
        partialCrudAlias.Should().NotBeNull();

        var crudSubjectAlias = context.Aliases.FirstOrDefault(a => 
            a.Alias == "CRUD" && 
            a.Type == PrivilegeMatch.Subject);
        crudSubjectAlias.Should().NotBeNull();
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

        bool canCreateProject = context.Allowed("Create", "Project");               // true
        bool canReadUser = context.Allowed("Read", "User");                         // true
        bool canUpdateProfile = context.Allowed("Update", "User", "Profile");       // true
        bool canUpdatePassword = context.Allowed("Update", "User", "Password");     // false
        bool canUpdateAny = context.Allowed("Update", "User", PrivilegeRule.Any);   // true
        bool canDeleteUser = context.Allowed("Delete", "User");                     // false

        canCreateProject.Should().BeTrue();
        canReadUser.Should().BeTrue();
        canUpdateProfile.Should().BeTrue();
        canUpdatePassword.Should().BeFalse();
        canUpdateAny.Should().BeTrue();
        canDeleteUser.Should().BeFalse();
    }
}
