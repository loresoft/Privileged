using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Privileged.Authorization.Tests;

public class PrivilegeRequirementHandlerTests
{
    [Fact]
    public async Task HandleRequirement_WhenUserHasRequiredPrivilege_Succeeds()
    {
        // Arrange
        var privilegeContext = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Build();

        var provider = new TestPrivilegeContextProvider(privilegeContext);

        var services = new ServiceCollection();
        services.AddSingleton<IPrivilegeContextProvider>(provider);
        services.AddSingleton<IAuthorizationHandler, PrivilegeRequirementHandler>();
        services.AddAuthorization();
        services.AddLogging();

        var serviceProvider = services.BuildServiceProvider();
        var authorizationService = serviceProvider.GetRequiredService<IAuthorizationService>();

        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "TestUser")], "TestAuthType"));
        var requirement = new PrivilegeRequirement("read", "Post");

        // Act
        var result = await authorizationService.AuthorizeAsync(user, null, requirement);

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task HandleRequirement_WhenUserDoesNotHaveRequiredPrivilege_Fails()
    {
        // Arrange
        var privilegeContext = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Build();

        var provider = new TestPrivilegeContextProvider(privilegeContext);

        var services = new ServiceCollection();
        services.AddSingleton<IPrivilegeContextProvider>(provider);
        services.AddSingleton<IAuthorizationHandler, PrivilegeRequirementHandler>();
        services.AddAuthorization();
        services.AddLogging();

        var serviceProvider = services.BuildServiceProvider();
        var authorizationService = serviceProvider.GetRequiredService<IAuthorizationService>();

        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "TestUser")], "TestAuthType"));
        var requirement = new PrivilegeRequirement("delete", "Post"); // Not allowed

        // Act
        var result = await authorizationService.AuthorizeAsync(user, null, requirement);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Single(result.Failure!.FailureReasons);
        Assert.Equal("User does not have the required privilege", result.Failure.FailureReasons.First().Message);
    }

    [Fact]
    public async Task HandleRequirement_WhenPrivilegeContextIsNull_Fails()
    {
        // Arrange
        var provider = new TestPrivilegeContextProvider(null);

        var services = new ServiceCollection();
        services.AddSingleton<IPrivilegeContextProvider>(provider);
        services.AddSingleton<IAuthorizationHandler, PrivilegeRequirementHandler>();
        services.AddAuthorization();
        services.AddLogging();

        var serviceProvider = services.BuildServiceProvider();
        var authorizationService = serviceProvider.GetRequiredService<IAuthorizationService>();

        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "TestUser")], "TestAuthType"));
        var requirement = new PrivilegeRequirement("read", "Post");

        // Act
        var result = await authorizationService.AuthorizeAsync(user, null, requirement);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Single(result.Failure!.FailureReasons);
        Assert.Equal("User does not have the required privilege", result.Failure.FailureReasons.First().Message);
    }

    [Fact]
    public async Task HandleRequirement_WithQualifier_WhenUserHasRequiredPrivilege_Succeeds()
    {
        // Arrange
        var privilegeContext = new PrivilegeBuilder()
            .Allow("update", "Post", ["title", "content"])
            .Build();

        var provider = new TestPrivilegeContextProvider(privilegeContext);

        var services = new ServiceCollection();
        services.AddSingleton<IPrivilegeContextProvider>(provider);
        services.AddSingleton<IAuthorizationHandler, PrivilegeRequirementHandler>();
        services.AddAuthorization();
        services.AddLogging();

        var serviceProvider = services.BuildServiceProvider();
        var authorizationService = serviceProvider.GetRequiredService<IAuthorizationService>();

        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "TestUser")], "TestAuthType"));
        var requirement = new PrivilegeRequirement("update", "Post", "title");

        // Act
        var result = await authorizationService.AuthorizeAsync(user, null, requirement);

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task HandleRequirement_WithQualifier_WhenUserDoesNotHaveRequiredPrivilege_Fails()
    {
        // Arrange
        var privilegeContext = new PrivilegeBuilder()
            .Allow("update", "Post", ["title", "content"])
            .Build();

        var provider = new TestPrivilegeContextProvider(privilegeContext);

        var services = new ServiceCollection();
        services.AddSingleton<IPrivilegeContextProvider>(provider);
        services.AddSingleton<IAuthorizationHandler, PrivilegeRequirementHandler>();
        services.AddAuthorization();
        services.AddLogging();

        var serviceProvider = services.BuildServiceProvider();
        var authorizationService = serviceProvider.GetRequiredService<IAuthorizationService>();

        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "TestUser")], "TestAuthType"));
        var requirement = new PrivilegeRequirement("update", "Post", "status"); // Not allowed

        // Act
        var result = await authorizationService.AuthorizeAsync(user, null, requirement);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Single(result.Failure!.FailureReasons);
        Assert.Equal("User does not have the required privilege", result.Failure.FailureReasons.First().Message);
    }

    [Fact]
    public async Task HandleRequirement_WithComplexPrivilegeRules_WorksCorrectly()
    {
        // Arrange
        var privilegeContext = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Allow("update", "Post", ["title", "content"])
            .Forbid("delete", "Post")
            .Allow("manage", "User")
            .Build();

        var provider = new TestPrivilegeContextProvider(privilegeContext);

        var services = new ServiceCollection();
        services.AddSingleton<IPrivilegeContextProvider>(provider);
        services.AddSingleton<IAuthorizationHandler, PrivilegeRequirementHandler>();
        services.AddAuthorization();
        services.AddLogging();

        var serviceProvider = services.BuildServiceProvider();
        var authorizationService = serviceProvider.GetRequiredService<IAuthorizationService>();

        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "TestUser")], "TestAuthType"));

        // Test allowed read action
        var readRequirement = new PrivilegeRequirement("read", "Post");
        var readResult = await authorizationService.AuthorizeAsync(user, null, readRequirement);
        Assert.True(readResult.Succeeded);

        // Test forbidden delete action
        var deleteRequirement = new PrivilegeRequirement("delete", "Post");
        var deleteResult = await authorizationService.AuthorizeAsync(user, null, deleteRequirement);
        Assert.False(deleteResult.Succeeded);

        // Test allowed manage action on User
        var manageRequirement = new PrivilegeRequirement("manage", "User");
        var manageResult = await authorizationService.AuthorizeAsync(user, null, manageRequirement);
        Assert.True(manageResult.Succeeded);
    }

    [Fact]
    public async Task HandleRequirement_WithWildcardRules_WorksCorrectly()
    {
        // Arrange
        var privilegeContext = new PrivilegeBuilder()
            .Allow("read", PrivilegeSubjects.All)
            .Allow(PrivilegeActions.All, "Post")
            .Forbid("delete", "Post")
            .Build();

        var provider = new TestPrivilegeContextProvider(privilegeContext);

        var services = new ServiceCollection();
        services.AddSingleton<IPrivilegeContextProvider>(provider);
        services.AddSingleton<IAuthorizationHandler, PrivilegeRequirementHandler>();
        services.AddAuthorization();
        services.AddLogging();

        var serviceProvider = services.BuildServiceProvider();
        var authorizationService = serviceProvider.GetRequiredService<IAuthorizationService>();

        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "TestUser")], "TestAuthType"));

        // Test wildcard subject (read anything)
        var readUserRequirement = new PrivilegeRequirement("read", "User");
        var readUserResult = await authorizationService.AuthorizeAsync(user, null, readUserRequirement);
        Assert.True(readUserResult.Succeeded);

        // Test wildcard action (any action on Post, except delete which is forbidden)
        var updatePostRequirement = new PrivilegeRequirement("update", "Post");
        var updatePostResult = await authorizationService.AuthorizeAsync(user, null, updatePostRequirement);
        Assert.True(updatePostResult.Succeeded);

        // Test forbidden delete action (forbid overrides wildcard allow)
        var deletePostRequirement = new PrivilegeRequirement("delete", "Post");
        var deletePostResult = await authorizationService.AuthorizeAsync(user, null, deletePostRequirement);
        Assert.False(deletePostResult.Succeeded);
    }

    [Fact]
    public async Task HandleRequirement_WithAliases_WorksCorrectly()
    {
        // Arrange
        var privilegeContext = new PrivilegeBuilder()
            .Alias("Crud", ["create", "read", "update", "delete"], PrivilegeMatch.Action)
            .Allow("Crud", "Post")
            .Forbid("delete", "Post")
            .Build();

        var provider = new TestPrivilegeContextProvider(privilegeContext);

        var services = new ServiceCollection();
        services.AddSingleton<IPrivilegeContextProvider>(provider);
        services.AddSingleton<IAuthorizationHandler, PrivilegeRequirementHandler>();
        services.AddAuthorization();
        services.AddLogging();

        var serviceProvider = services.BuildServiceProvider();
        var authorizationService = serviceProvider.GetRequiredService<IAuthorizationService>();

        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "TestUser")], "TestAuthType"));

        // Test alias expansion for create action
        var createRequirement = new PrivilegeRequirement("create", "Post");
        var createResult = await authorizationService.AuthorizeAsync(user, null, createRequirement);
        Assert.True(createResult.Succeeded);

        // Test alias expansion for read action
        var readRequirement = new PrivilegeRequirement("read", "Post");
        var readResult = await authorizationService.AuthorizeAsync(user, null, readRequirement);
        Assert.True(readResult.Succeeded);

        // Test forbid rule overrides alias
        var deleteRequirement = new PrivilegeRequirement("delete", "Post");
        var deleteResult = await authorizationService.AuthorizeAsync(user, null, deleteRequirement);
        Assert.False(deleteResult.Succeeded);
    }

    [Theory]
    [InlineData("read", "Post", null)]
    [InlineData("update", "User", "email")]
    [InlineData("delete", "Comment", "content")]
    public async Task HandleRequirement_WithVariousRequirements_CallsCorrectPrivilegeCheck(
        string action, string subject, string? qualifier)
    {
        // Arrange
        var privilegeContext = new PrivilegeBuilder()
            .Allow(action, subject, qualifier != null ? [qualifier] : null)
            .Build();

        var provider = new TestPrivilegeContextProvider(privilegeContext);

        var services = new ServiceCollection();
        services.AddSingleton<IPrivilegeContextProvider>(provider);
        services.AddSingleton<IAuthorizationHandler, PrivilegeRequirementHandler>();
        services.AddAuthorization();
        services.AddLogging();

        var serviceProvider = services.BuildServiceProvider();
        var authorizationService = serviceProvider.GetRequiredService<IAuthorizationService>();

        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "TestUser")], "TestAuthType"));
        var requirement = new PrivilegeRequirement(action, subject, qualifier);

        // Act
        var result = await authorizationService.AuthorizeAsync(user, null, requirement);

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task HandleRequirement_WithMultipleHandlers_OnlyProcessesOwnRequirement()
    {
        // Arrange
        var privilegeContext = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Build();

        var provider = new TestPrivilegeContextProvider(privilegeContext);

        var services = new ServiceCollection();
        services.AddSingleton<IPrivilegeContextProvider>(provider);
        services.AddSingleton<IAuthorizationHandler, PrivilegeRequirementHandler>();
        services.AddAuthorization();
        services.AddLogging();

        var serviceProvider = services.BuildServiceProvider();
        var authorizationService = serviceProvider.GetRequiredService<IAuthorizationService>();

        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "TestUser")], "TestAuthType"));

        // Test with PrivilegeRequirement (should succeed)
        var privilegeRequirement = new PrivilegeRequirement("read", "Post");
        var privilegeResult = await authorizationService.AuthorizeAsync(user, null, privilegeRequirement);
        Assert.True(privilegeResult.Succeeded);

        // Test with different requirement type (should fail because no handler)
        var otherRequirement = new TestRequirement();
        var otherResult = await authorizationService.AuthorizeAsync(user, null, otherRequirement);
        Assert.False(otherResult.Succeeded);
    }

    [Fact]
    public async Task HandleRequirement_WithAsyncProvider_WorksCorrectly()
    {
        // Arrange
        var privilegeContext = new PrivilegeBuilder()
            .Allow("read", "Document")
            .Build();

        var provider = new AsyncPrivilegeContextProvider(privilegeContext, TimeSpan.FromMilliseconds(10));

        var services = new ServiceCollection();
        services.AddSingleton<IPrivilegeContextProvider>(provider);
        services.AddSingleton<IAuthorizationHandler, PrivilegeRequirementHandler>();
        services.AddAuthorization();
        services.AddLogging();

        var serviceProvider = services.BuildServiceProvider();
        var authorizationService = serviceProvider.GetRequiredService<IAuthorizationService>();

        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "TestUser")], "TestAuthType"));
        var requirement = new PrivilegeRequirement("read", "Document");

        // Act
        var result = await authorizationService.AuthorizeAsync(user, null, requirement);

        // Assert
        Assert.True(result.Succeeded);
    }

    // Test helper classes
    private class TestPrivilegeContextProvider : IPrivilegeContextProvider
    {
        private readonly PrivilegeContext? _context;

        public TestPrivilegeContextProvider(PrivilegeContext? context)
        {
            _context = context;
        }

        public ValueTask<PrivilegeContext> GetContextAsync(ClaimsPrincipal? claimsPrincipal = null)
        {
            return _context != null
                ? ValueTask.FromResult(_context)
                : ValueTask.FromResult<PrivilegeContext>(null!);
        }
    }

    private class AsyncPrivilegeContextProvider : IPrivilegeContextProvider
    {
        private readonly PrivilegeContext _context;
        private readonly TimeSpan _delay;

        public AsyncPrivilegeContextProvider(PrivilegeContext context, TimeSpan delay)
        {
            _context = context;
            _delay = delay;
        }

        public async ValueTask<PrivilegeContext> GetContextAsync(ClaimsPrincipal? claimsPrincipal = null)
        {
            await Task.Delay(_delay);
            return _context;
        }
    }

    private class TestRequirement : IAuthorizationRequirement
    {
    }
}
