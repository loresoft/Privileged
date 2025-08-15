using System;
using System.Collections.Generic;

namespace Privileged.Tests;

public class PrivilegeContextExtensionsTests
{
    [Fact]
    public void Any_WithAllowedSubject_ReturnsTrue()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Allow("read", "Comment")
            .Build();

        // Act
        var result = context.Any("read", "Post", "User", "Article");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Any_WithNoAllowedSubjects_ReturnsFalse()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Build();

        // Act
        var result = context.Any("read", "User", "Article", "Admin");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Any_WithMultipleAllowedSubjects_ReturnsTrue()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Allow("read", "Comment")
            .Allow("read", "User")
            .Build();

        // Act
        var result = context.Any("read", "Post", "Comment", "User");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Any_WithEmptySubjects_ReturnsFalse()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Build();

        // Act
        var result = context.Any("read", Array.Empty<string>());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Any_WithSingleSubject_ReturnsCorrectResult()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Build();

        // Act
        var allowedResult = context.Any("read", "Post");
        var forbiddenResult = context.Any("read", "User");

        // Assert
        allowedResult.Should().BeTrue();
        forbiddenResult.Should().BeFalse();
    }

    [Fact]
    public void Any_ShortCircuitEvaluation_StopsOnFirstAllowed()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Forbid("read", "User")
            .Build();

        // Act - Post is first and allowed, so it should return true without checking User
        var result = context.Any("read", "Post", "User");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Any_WithForbiddenAction_ReturnsFalse()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Forbid("delete", "Post")
            .Build();

        // Act
        var result = context.Any("delete", "Post", "Comment", "User");

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Any_WithInvalidAction_ThrowsArgumentException(string action)
    {
        // Arrange
        var context = new PrivilegeBuilder().Build();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => context.Any(action, "Post", "User"));
    }

    [Fact]
    public void Any_WithNullAction_ThrowsArgumentNullException()
    {
        // Arrange
        var context = new PrivilegeBuilder().Build();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => context.Any(null!, "Post", "User"));
    }

    [Fact]
    public void Any_WithNullContext_ThrowsArgumentNullException()
    {
        // Arrange
        PrivilegeContext context = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => context.Any("read", "Post"));
    }

    [Fact]
    public void Any_WithNullSubjects_ThrowsArgumentNullException()
    {
        // Arrange
        var context = new PrivilegeBuilder().Build();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => context.Any("read", (IEnumerable<string>)null!));
    }

    [Fact]
    public void All_WithAllSubjectsAllowed_ReturnsTrue()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Allow("read", "Comment")
            .Allow("read", "User")
            .Build();

        // Act
        var result = context.All("read", "Post", "Comment", "User");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void All_WithSomeSubjectsForbidden_ReturnsFalse()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Allow("read", "Comment")
            .Forbid("read", "User")
            .Build();

        // Act
        var result = context.All("read", "Post", "Comment", "User");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void All_WithNoAllowedSubjects_ReturnsFalse()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("write", "Post")
            .Build();

        // Act
        var result = context.All("read", "Post", "Comment", "User");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void All_WithEmptySubjects_ReturnsTrue()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Build();

        // Act
        var result = context.All("read", Array.Empty<string>());

        // Assert
        result.Should().BeTrue(); // Vacuous truth
    }

    [Fact]
    public void All_WithSingleAllowedSubject_ReturnsTrue()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Build();

        // Act
        var result = context.All("read", "Post");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void All_WithSingleForbiddenSubject_ReturnsFalse()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Forbid("read", "Post")
            .Build();

        // Act
        var result = context.All("read", "Post");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void All_ShortCircuitEvaluation_StopsOnFirstForbidden()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Forbid("read", "User") // First subject is forbidden
            .Allow("read", "Post")
            .Build();

        // Act - User is first and forbidden, so it should return false without checking Post
        var result = context.All("read", "User", "Post");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void All_WithMixedPermissions_ReturnsFalse()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Allow("read", "Comment")
            .Build(); // No explicit rule for "User", so it's forbidden

        // Act
        var result = context.All("read", "Post", "Comment", "User");

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void All_WithInvalidAction_ThrowsArgumentException(string action)
    {
        // Arrange
        var context = new PrivilegeBuilder().Build();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => context.All(action, "Post", "User"));
    }

    [Fact]
    public void All_WithNullAction_ThrowsArgumentNullException()
    {
        // Arrange
        var context = new PrivilegeBuilder().Build();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => context.All(null!, "Post", "User"));
    }

    [Fact]
    public void All_WithNullContext_ThrowsArgumentNullException()
    {
        // Arrange
        PrivilegeContext context = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => context.All("read", "Post"));
    }

    [Fact]
    public void All_WithNullSubjects_ThrowsArgumentNullException()
    {
        // Arrange
        var context = new PrivilegeBuilder().Build();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => context.All("read", (IEnumerable<string>)null!));
    }

    [Fact]
    public void None_WithNoAllowedSubjects_ReturnsTrue()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Build();

        // Act
        var result = context.None("read", "User", "Admin", "Article");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void None_WithAllSubjectsAllowed_ReturnsFalse()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Allow("read", "Comment")
            .Allow("read", "User")
            .Build();

        // Act
        var result = context.None("read", "Post", "Comment", "User");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void None_WithSomeSubjectsAllowed_ReturnsFalse()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Forbid("read", "User")
            .Build();

        // Act
        var result = context.None("read", "Post", "User", "Admin");

        // Assert
        result.Should().BeFalse(); // Because "Post" is allowed
    }

    [Fact]
    public void None_WithEmptySubjects_ReturnsTrue()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Build();

        // Act
        var result = context.None("read", Array.Empty<string>());

        // Assert
        result.Should().BeTrue(); // Vacuous truth
    }

    [Fact]
    public void None_WithSingleAllowedSubject_ReturnsFalse()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Build();

        // Act
        var result = context.None("read", "Post");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void None_WithSingleForbiddenSubject_ReturnsTrue()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Forbid("read", "Post")
            .Build();

        // Act
        var result = context.None("read", "Post");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void None_ShortCircuitEvaluation_StopsOnFirstAllowed()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("read", "Post") // First subject is allowed
            .Forbid("read", "User")
            .Build();

        // Act - Post is first and allowed, so it should return false without checking User
        var result = context.None("read", "Post", "User");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void None_WithAllForbiddenSubjects_ReturnsTrue()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Forbid("read", "Post")
            .Forbid("read", "User")
            .Forbid("read", "Comment")
            .Build();

        // Act
        var result = context.None("read", "Post", "User", "Comment");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void None_IsLogicalInverseOfAny()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Forbid("read", "User")
            .Build();

        var subjects = new[] { "Post", "User", "Admin" };

        // Act
        var anyResult = context.Any("read", subjects);
        var noneResult = context.None("read", subjects);

        // Assert
        anyResult.Should().BeTrue();
        noneResult.Should().BeFalse();
        (anyResult && noneResult).Should().BeFalse(); // They should never both be true
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void None_WithInvalidAction_ThrowsArgumentException(string action)
    {
        // Arrange
        var context = new PrivilegeBuilder().Build();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => context.None(action, "Post", "User"));
    }

    [Fact]
    public void None_WithNullAction_ThrowsArgumentNullException()
    {
        // Arrange
        var context = new PrivilegeBuilder().Build();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => context.None(null!, "Post", "User"));
    }

    [Fact]
    public void None_WithNullContext_ThrowsArgumentNullException()
    {
        // Arrange
        PrivilegeContext context = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => context.None("read", "Post"));
    }

    [Fact]
    public void None_WithNullSubjects_ThrowsArgumentNullException()
    {
        // Arrange
        var context = new PrivilegeBuilder().Build();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => context.None("read", (IEnumerable<string>)null!));
    }

    [Fact]
    public void ExtensionMethods_WithWildcardRules_WorkCorrectly()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow(PrivilegeRule.All, "Post")
            .Allow("read", PrivilegeRule.All)
            .Forbid("delete", "User")
            .Build();

        // Act & Assert
        context.Any("create", "Post", "User").Should().BeTrue(); // Post allows all actions
        context.Any("read", "Article", "Admin").Should().BeTrue(); // Read allowed on all subjects
        
        context.All("read", "Post", "User", "Admin").Should().BeTrue(); // Read allowed on all subjects
        context.All("create", "Post", "User").Should().BeFalse(); // User doesn't allow create
        
        // The "read" wildcard only grants "read" permission, not "delete"
        // "delete" on "User" is explicitly forbidden, and "delete" on "Admin" is not granted by "read" wildcard
        context.None("delete", "User", "Admin").Should().BeTrue(); // Neither User nor Admin allow delete
    }

    [Fact]
    public void ExtensionMethods_WithAliases_WorkCorrectly()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Alias("manage", ["read", "create", "update"], PrivilegeMatch.Action)
            .Alias("content", ["Post", "Article"], PrivilegeMatch.Subject)
            .Allow("manage", "content")
            .Build();

        // Act & Assert
        context.Any("read", "Post", "Article", "User").Should().BeTrue(); // Post and Article are in content alias
        context.All("read", "Post", "Article").Should().BeTrue(); // Both Post and Article allow read via alias
        context.None("delete", "Post", "Article", "User").Should().BeTrue(); // Delete is not in manage alias
    }

    [Fact]
    public void ExtensionMethods_WithComplexPermissionSet_WorkCorrectly()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Allow("read", "Comment")
            .Allow("write", "Post")
            .Forbid("delete", "Post")
            .Allow("admin", PrivilegeRule.All)
            .Forbid("admin", "SecretDocument")
            .Build();

        var readSubjects = new[] { "Post", "Comment" };
        var writeSubjects = new[] { "Post", "Comment", "User" };
        var adminSubjects = new[] { "Post", "Comment", "User" };
        var secretSubjects = new[] { "SecretDocument", "TopSecret" };

        // Act & Assert
        context.Any("read", readSubjects).Should().BeTrue();
        context.All("read", readSubjects).Should().BeTrue();
        context.None("read", "User", "Admin").Should().BeTrue();

        context.Any("write", writeSubjects).Should().BeTrue();
        context.All("write", writeSubjects).Should().BeFalse(); // Comment and User don't allow write
        
        context.Any("admin", adminSubjects).Should().BeTrue();
        context.All("admin", adminSubjects).Should().BeTrue();
        context.None("admin", secretSubjects).Should().BeFalse(); // SecretDocument is forbidden but TopSecret might be allowed
    }

    [Fact]
    public void ExtensionMethods_PerformanceTest_WithManySubjects()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("read", "AllowedSubject")
            .Build();

        var manySubjects = new List<string>();
        for (int i = 0; i < 1000; i++)
        {
            manySubjects.Add($"Subject{i}");
        }
        manySubjects.Add("AllowedSubject"); // Add one allowed subject at the end

        // Act - These should be fast due to short-circuit evaluation
        var anyResult = context.Any("read", manySubjects.ToArray());
        var allResult = context.All("read", manySubjects.ToArray());
        var noneResult = context.None("read", manySubjects.Take(999).ToArray()); // Exclude the allowed subject

        // Assert
        anyResult.Should().BeTrue();
        allResult.Should().BeFalse();
        noneResult.Should().BeTrue();
    }

    [Fact]
    public void ExtensionMethods_WithDuplicateSubjects_WorkCorrectly()
    {
        // Arrange
        var context = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Build();

        var duplicateSubjects = new[] { "Post", "Post", "User", "Post" };

        // Act
        var anyResult = context.Any("read", duplicateSubjects);
        var allResult = context.All("read", duplicateSubjects);
        var noneResult = context.None("read", "User", "User", "Admin");

        // Assert
        anyResult.Should().BeTrue(); // Post is allowed
        allResult.Should().BeFalse(); // User is not allowed (appears multiple times)
        noneResult.Should().BeTrue(); // Neither User nor Admin is allowed
    }
}
