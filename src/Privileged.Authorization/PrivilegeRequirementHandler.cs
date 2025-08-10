using System.Collections.Concurrent;

using Microsoft.AspNetCore.Authorization;

namespace Privileged.Authorization;

/// <summary>
/// An authorization handler that evaluates <see cref="PrivilegeRequirement"/> instances using a privilege context.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="PrivilegeRequirementHandler"/> is responsible for evaluating privilege-based authorization
/// requirements. It works by obtaining a <see cref="PrivilegeContext"/> from the registered
/// <see cref="IPrivilegeContextProvider"/> and using it to determine if the current user has the required privileges.
/// </para>
/// <para>
/// This handler is automatically registered when using the <see cref="ServiceCollectionExtensions.AddPrivilegeAuthorization"/>
/// extension method. The handler evaluates requirements created by <see cref="PrivilegePolicyProvider"/>
/// for policies generated from <see cref="PrivilegeAttribute"/> usage.
/// </para>
/// <para>
/// The evaluation process:
/// <list type="number">
/// <item><description>Obtains the privilege context from the registered provider</description></item>
/// <item><description>Checks if the context allows the required action on the specified subject</description></item>
/// <item><description>Succeeds or fails the authorization requirement based on the privilege check</description></item>
/// </list>
/// </para>
/// </remarks>
/// <seealso cref="PrivilegeRequirement"/>
/// <seealso cref="IPrivilegeContextProvider"/>
/// <seealso cref="PrivilegeAttribute"/>
/// <seealso cref="PrivilegePolicyProvider"/>
public class PrivilegeRequirementHandler : AuthorizationHandler<PrivilegeRequirement>
{
    private readonly IPrivilegeContextProvider _contextProvider;
    private readonly ConcurrentDictionary<string, PrivilegeContext?> _cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="PrivilegeRequirementHandler"/> class.
    /// </summary>
    /// <param name="contextProvider">The privilege context provider used to obtain the current user's privilege context.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextProvider"/> is <c>null</c>.</exception>
    public PrivilegeRequirementHandler(IPrivilegeContextProvider contextProvider)
    {
        _contextProvider = contextProvider ?? throw new ArgumentNullException(nameof(contextProvider));
        _cache = new ConcurrentDictionary<string, PrivilegeContext?>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Evaluates the specified privilege requirement against the current user's privilege context.
    /// </summary>
    /// <param name="context">The authorization handler context containing information about the current authorization request.</param>
    /// <param name="requirement">The privilege requirement that specifies the action, subject, and optional qualifier to authorize.</param>
    /// <returns>A task that represents the asynchronous authorization evaluation.</returns>
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PrivilegeRequirement requirement)
    {
        if (context == null || requirement == null)
            return;

        // If there's no user or the user is not authenticated, do not succeed the requirement
        if (context.User == null || context.User.Identity?.IsAuthenticated != true)
            return;

        var cacheKey = $"privilege:{context.User.Identity.Name}";

        // Try to get the privilege context from the cache
        if (!_cache.TryGetValue(cacheKey, out var privilegeContext))
        {
            // If not found in cache, obtain the context from the provider
            privilegeContext = await _contextProvider.GetContextAsync(context.User);
            _cache[cacheKey] = privilegeContext;
        }

        // Check if the privilege context contains the required privilege
        if (privilegeContext?.Allowed(requirement.Action, requirement.Subject, requirement.Qualifier) == true)
        {
            // User has the required privilege, succeed the requirement
            context.Succeed(requirement);
        }
        else
        {
            // User does not have the required privilege, fail the requirement
            context.Fail(new AuthorizationFailureReason(this, "User does not have the required privilege"));
        }
    }
}
