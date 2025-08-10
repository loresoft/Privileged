namespace Privileged.Authorization.Tests;

public class PrivilegeAttributeTests
{
    [Fact]
    public void Constructor_WithActionAndSubject_SetsPropertiesCorrectly()
    {
        // Arrange
        var action = "read";
        var subject = "User";

        // Act
        var attribute = new PrivilegeAttribute(action, subject);

        // Assert
        Assert.Equal(action, attribute.Action);
        Assert.Equal(subject, attribute.Subject);
        Assert.Null(attribute.Qualifier);
    }

    [Fact]
    public void Constructor_WithActionSubjectAndQualifier_SetsPropertiesCorrectly()
    {
        // Arrange
        var action = "write";
        var subject = "Post";
        var qualifier = "title";

        // Act
        var attribute = new PrivilegeAttribute(action, subject, qualifier);

        // Assert
        Assert.Equal(action, attribute.Action);
        Assert.Equal(subject, attribute.Subject);
        Assert.Equal(qualifier, attribute.Qualifier);
    }

    [Fact]
    public void Constructor_WithNullQualifier_SetsQualifierToNull()
    {
        // Arrange
        var action = "delete";
        var subject = "Comment";

        // Act
        var attribute = new PrivilegeAttribute(action, subject, null);

        // Assert
        Assert.Equal(action, attribute.Action);
        Assert.Equal(subject, attribute.Subject);
        Assert.Null(attribute.Qualifier);
    }

    [Fact]
    public void Policy_WithoutQualifier_GeneratesCorrectPolicyName()
    {
        // Arrange
        var action = "read";
        var subject = "User";

        // Act
        var attribute = new PrivilegeAttribute(action, subject);

        // Assert
        Assert.Equal("Privilege:read:User", attribute.Policy);
    }

    [Fact]
    public void Policy_WithQualifier_GeneratesCorrectPolicyName()
    {
        // Arrange
        var action = "write";
        var subject = "Post";
        var qualifier = "title";

        // Act
        var attribute = new PrivilegeAttribute(action, subject, qualifier);

        // Assert
        Assert.Equal("Privilege:write:Post:title", attribute.Policy);
    }

    [Fact]
    public void Policy_WithEmptyQualifier_GeneratesThreePartPolicyName()
    {
        // Arrange
        var action = "update";
        var subject = "Profile";
        var qualifier = "";

        // Act
        var attribute = new PrivilegeAttribute(action, subject, qualifier);

        // Assert
        Assert.Equal("Privilege:update:Profile", attribute.Policy);
    }

    [Fact]
    public void Policy_WithWhitespaceQualifier_GeneratesFourPartPolicyName()
    {
        // Arrange
        var action = "delete";
        var subject = "Document";
        var qualifier = " ";

        // Act
        var attribute = new PrivilegeAttribute(action, subject, qualifier);

        // Assert
        Assert.Equal("Privilege:delete:Document: ", attribute.Policy);
    }

    [Theory]
    [InlineData("read", "User", null, "Privilege:read:User")]
    [InlineData("write", "Post", "title", "Privilege:write:Post:title")]
    [InlineData("delete", "Comment", "", "Privilege:delete:Comment")]
    [InlineData("update", "Profile", "avatar", "Privilege:update:Profile:avatar")]
    [InlineData("create", "Article", "draft", "Privilege:create:Article:draft")]
    public void Policy_WithVariousInputs_GeneratesExpectedPolicyName(string action, string subject, string? qualifier, string expectedPolicy)
    {
        // Act
        var attribute = new PrivilegeAttribute(action, subject, qualifier);

        // Assert
        Assert.Equal(expectedPolicy, attribute.Policy);
    }

    [Fact]
    public void Constructor_WithSpecialCharacters_HandlesCorrectly()
    {
        // Arrange
        var action = "read-write";
        var subject = "User_Profile";
        var qualifier = "field.name";

        // Act
        var attribute = new PrivilegeAttribute(action, subject, qualifier);

        // Assert
        Assert.Equal(action, attribute.Action);
        Assert.Equal(subject, attribute.Subject);
        Assert.Equal(qualifier, attribute.Qualifier);
        Assert.Equal("Privilege:read-write:User_Profile:field.name", attribute.Policy);
    }

    [Fact]
    public void InheritsFromAuthorizeAttribute()
    {
        // Arrange & Act
        var attribute = new PrivilegeAttribute("read", "User");

        // Assert
        Assert.IsAssignableFrom<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>(attribute);
    }

    [Fact]
    public void Policy_UsesStringIsNullOrEmptyForQualifier()
    {
        // Test the exact behavior of string.IsNullOrEmpty() in policy generation

        // Null qualifier
        var attributeWithNull = new PrivilegeAttribute("read", "User", null);
        Assert.Equal("Privilege:read:User", attributeWithNull.Policy);

        // Empty string qualifier
        var attributeWithEmpty = new PrivilegeAttribute("read", "User", "");
        Assert.Equal("Privilege:read:User", attributeWithEmpty.Policy);

        // Whitespace qualifier (not null or empty, so should be included)
        var attributeWithWhitespace = new PrivilegeAttribute("read", "User", " ");
        Assert.Equal("Privilege:read:User: ", attributeWithWhitespace.Policy);
    }
}
