using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Privileged.Authorization.Tests;

public class PrivilegePolicyProviderTests
{
    private readonly PrivilegePolicyProvider _policyProvider;

    public PrivilegePolicyProviderTests()
    {
        var options = Options.Create(new AuthorizationOptions());
        _policyProvider = new PrivilegePolicyProvider(options);
    }

    [Fact]
    public async Task GetPolicyAsync_WithValidPrivilegePolicy_ReturnsPolicy()
    {
        // Arrange
        var policyName = "Privilege:read:Post:title";

        // Act
        var policy = await _policyProvider.GetPolicyAsync(policyName);

        // Assert
        Assert.NotNull(policy);
        Assert.Single(policy.Requirements);

        var requirement = Assert.IsType<PrivilegeRequirement>(policy.Requirements.First());

        Assert.Equal("read", requirement.Action);
        Assert.Equal("Post", requirement.Subject);
        Assert.Equal("title", requirement.Qualifier);
    }

    [Fact]
    public async Task GetPolicyAsync_WithValidPrivilegePolicyNoQualifier_ReturnsPolicy()
    {
        // Arrange
        var policyName = "Privilege:write:User:";

        // Act
        var policy = await _policyProvider.GetPolicyAsync(policyName);

        // Assert
        Assert.NotNull(policy);
        Assert.Single(policy.Requirements);

        var requirement = Assert.IsType<PrivilegeRequirement>(policy.Requirements.First());

        Assert.Equal("write", requirement.Action);
        Assert.Equal("User", requirement.Subject);
        Assert.Null(requirement.Qualifier);
    }

    [Fact]
    public async Task GetPolicyAsync_WithNonPrivilegePolicy_ReturnsNull()
    {
        // Arrange
        var policyName = "SomeOtherPolicy";

        // Act
        var policy = await _policyProvider.GetPolicyAsync(policyName);

        // Assert
        Assert.Null(policy);
    }

    [Fact]
    public async Task GetPolicyAsync_WithInvalidFormat_ReturnsNull()
    {
        // Arrange
        var policyName = "Privilege:"; // Invalid - missing action and subject

        // Act
        var policy = await _policyProvider.GetPolicyAsync(policyName);

        // Assert
        Assert.Null(policy);
    }

    [Fact]
    public async Task GetPolicyAsync_WithEmptyAction_ReturnsNull()
    {
        // Arrange
        var policyName = "Privilege::Post:title";

        // Act
        var policy = await _policyProvider.GetPolicyAsync(policyName);

        // Assert
        Assert.Null(policy);
    }

    [Fact]
    public async Task GetPolicyAsync_WithEmptySubject_ReturnsNull()
    {
        // Arrange
        var policyName = "Privilege:read::title";

        // Act
        var policy = await _policyProvider.GetPolicyAsync(policyName);

        // Assert
        Assert.Null(policy);
    }

    [Fact]
    public async Task GetPolicyAsync_WithNullOrEmptyPolicyName_ReturnsNull()
    {
        // Act & Assert
        var policy1 = await _policyProvider.GetPolicyAsync(null!);
        var policy2 = await _policyProvider.GetPolicyAsync("");

        Assert.Null(policy1);
        Assert.Null(policy2);
    }

    [Fact]
    public async Task GetPolicyAsync_CachesSamePolicy_ReturnsSameInstance()
    {
        // Arrange
        var policyName = "Privilege:read:Post:title";

        // Act
        var policy1 = await _policyProvider.GetPolicyAsync(policyName);
        var policy2 = await _policyProvider.GetPolicyAsync(policyName);

        // Assert
        Assert.NotNull(policy1);
        Assert.NotNull(policy2);

        // Should be the same instance from cache
        Assert.Same(policy1, policy2);
    }

    [Fact]
    public async Task GetPolicyAsync_CaseSensitivity_HandlesCorrectly()
    {
        // Arrange
        var policyName1 = "Privilege:read:Post:title";
        var policyName2 = "privilege:read:post:title";

        // Act
        var policy1 = await _policyProvider.GetPolicyAsync(policyName1);
        var policy2 = await _policyProvider.GetPolicyAsync(policyName2);

        // Assert
        Assert.NotNull(policy1);
        Assert.NotNull(policy2);

        // They should be same instances
        Assert.Same(policy1, policy2);
    }

    [Fact]
    public async Task GetPolicyAsync_WithTooManyColons_ReturnsNull()
    {
        // Arrange
        var policyName = "Privilege:read:Post:title:extra";

        // Act
        var policy = await _policyProvider.GetPolicyAsync(policyName);

        // Assert
        Assert.Null(policy);
    }

    [Fact]
    public async Task GetPolicyAsync_WithTooFewColons_ReturnsNull()
    {
        // Arrange
        var policyName = "Privilege:read"; // Only one colon after prefix

        // Act
        var policy = await _policyProvider.GetPolicyAsync(policyName);

        // Assert
        Assert.Null(policy);
    }

    [Fact]
    public async Task GetPolicyAsync_WithValidPrivilegePolicyWithoutQualifier_ReturnsPolicy()
    {
        // Arrange
        var policyName = "Privilege:read:Post";

        // Act
        var policy = await _policyProvider.GetPolicyAsync(policyName);

        // Assert
        Assert.NotNull(policy);
        Assert.Single(policy.Requirements);

        var requirement = Assert.IsType<PrivilegeRequirement>(policy.Requirements.First());

        Assert.Equal("read", requirement.Action);
        Assert.Equal("Post", requirement.Subject);
        Assert.Null(requirement.Qualifier);
    }

    [Fact]
    public async Task GetPolicyAsync_WithPrivilegeAttributeGeneratedPolicy_ReturnsPolicy()
    {
        var attribute = new PrivilegeAttribute("read", "Post");
        var policyName = attribute.Policy;

        Assert.NotNull(policyName);
        Assert.Equal("Privilege:read:Post", policyName);

        // Act
        var policy = await _policyProvider.GetPolicyAsync(policyName);

        // Assert
        Assert.NotNull(policy);
        Assert.Single(policy.Requirements);

        var requirement = Assert.IsType<PrivilegeRequirement>(policy.Requirements.First());

        Assert.Equal("read", requirement.Action);
        Assert.Equal("Post", requirement.Subject);
        Assert.Null(requirement.Qualifier);
    }

    [Fact]
    public async Task GetPolicyAsync_WithPrivilegeAttributeGeneratedPolicyWithQualifier_ReturnsPolicy()
    {
        var attribute = new PrivilegeAttribute("read", "Post", "title");
        var policyName = attribute.Policy;

        Assert.NotNull(policyName);
        Assert.Equal("Privilege:read:Post:title", policyName);

        // Act
        var policy = await _policyProvider.GetPolicyAsync(policyName);

        // Assert
        Assert.NotNull(policy);
        Assert.Single(policy.Requirements);

        var requirement = Assert.IsType<PrivilegeRequirement>(policy.Requirements.First());

        Assert.Equal("read", requirement.Action);
        Assert.Equal("Post", requirement.Subject);
        Assert.Equal("title", requirement.Qualifier);
    }
}
