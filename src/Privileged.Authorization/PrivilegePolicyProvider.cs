using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

using System.Collections.Concurrent;

namespace Privileged.Authorization;

/// <summary>
/// A custom authorization policy provider that dynamically creates policies for privilege-based authorization.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="PrivilegePolicyProvider"/> extends <see cref="DefaultAuthorizationPolicyProvider"/> to provide
/// dynamic policy creation for privilege-based authorization. When the authorization framework requests a policy
/// with a name that starts with "Privilege:", this provider creates an appropriate <see cref="AuthorizationPolicy"/>
/// containing a <see cref="PrivilegeRequirement"/>.
/// </para>
/// <para>
/// This provider works in conjunction with <see cref="PrivilegeAttribute"/> which generates policy names in the format:
/// <list type="bullet">
/// <item><description>"Privilege:action:subject" for basic privilege requirements</description></item>
/// <item><description>"Privilege:action:subject:qualifier" for privilege requirements with qualifiers</description></item>
/// </list>
/// </para>
/// <para>
/// The provider uses caching to improve performance for repeated policy requests and falls back to the default
/// policy provider for non-privilege policies.
/// </para>
/// </remarks>
/// <seealso cref="PrivilegeAttribute"/>
/// <seealso cref="PrivilegeRequirement"/>
/// <seealso cref="PrivilegeRequirementHandler"/>
public class PrivilegePolicyProvider(IOptions<AuthorizationOptions> options)
    : DefaultAuthorizationPolicyProvider(options)
{
    private static readonly ConcurrentDictionary<string, AuthorizationPolicy?> PolicyCache = new(StringComparer.OrdinalIgnoreCase);
    private const string PrivilegePolicyPrefix = "Privilege:";

    /// <summary>
    /// Gets the authorization policy for the specified policy name.
    /// </summary>
    /// <param name="policyName">The name of the policy to retrieve.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the authorization policy
    /// if found, or <c>null</c> if no matching policy exists.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method first checks if the policy name represents a privilege policy (starts with "Privilege:").
    /// If it does, it parses the policy name and creates a dynamic <see cref="AuthorizationPolicy"/> with
    /// the appropriate <see cref="PrivilegeRequirement"/>. If not, it falls back to the default policy provider.
    /// </para>
    /// <para>
    /// The method uses caching to avoid repeatedly parsing the same policy names and improve performance
    /// for frequently accessed policies.
    /// </para>
    /// </remarks>
    public override Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // Handle null/empty policy names before checking if it's a privilege policy
        if (string.IsNullOrEmpty(policyName))
            return Task.FromResult<AuthorizationPolicy?>(null);

        // Check if this is a privilege policy request
        var policy = ParsePrivilegePolicy(policyName);
        if (policy is not null)
            return Task.FromResult<AuthorizationPolicy?>(policy);

        // Fall back to the default provider for other policies
        return base.GetPolicyAsync(policyName);
    }

    /// <summary>
    /// Parses a privilege policy name and creates the corresponding authorization policy.
    /// </summary>
    /// <param name="policyName">The policy name to parse.</param>
    /// <returns>
    /// An <see cref="AuthorizationPolicy"/> if the policy name represents a valid privilege policy,
    /// or <c>null</c> if the policy name is not a privilege policy or is malformed.
    /// </returns>
    private static AuthorizationPolicy? ParsePrivilegePolicy(string policyName)
    {
        if (string.IsNullOrEmpty(policyName))
            return null;

        // Check cache first for better performance
        if (PolicyCache.TryGetValue(policyName, out var cachedPolicy))
            return cachedPolicy;

        // Ensure the policy name starts with "Privilege:" using more efficient string comparison
        if (!policyName.StartsWith(PrivilegePolicyPrefix, StringComparison.OrdinalIgnoreCase))
        {
            // Cache the null result to avoid repeated parsing of non-privilege policies
            PolicyCache.TryAdd(policyName, null);
            return null;
        }

        // Use Span<char> for more efficient string splitting
        ReadOnlySpan<char> policySpan = policyName.AsSpan();

        // Count colons to validate format before splitting
        int colonCount = 0;
        for (int i = 0; i < policySpan.Length; i++)
        {
            if (policySpan[i] == ':')
                colonCount++;
        }

        // Policy format: "Privilege:action:subject" or "Privilege:action:subject:qualifier"
        // Should have exactly 2 or 3 colons
        if (colonCount != 2 && colonCount != 3)
        {
            PolicyCache.TryAdd(policyName, null);
            return null;
        }

        // Parse the policy parts - allow for 3 or 4 parts
        var parts = policyName.Split(':');
        if (parts.Length < 3 || parts.Length > 4)
        {
            PolicyCache.TryAdd(policyName, null);
            return null;
        }

        var action = parts[1];
        var subject = parts[2];

        // Validate that action and subject are not empty
        if (string.IsNullOrEmpty(action) || string.IsNullOrEmpty(subject))
        {
            PolicyCache.TryAdd(policyName, null);
            return null;
        }

        // Handle qualifier: null if not provided, or convert empty string to null
        var qualifier = (parts.Length == 4 && parts[3].Length > 0) ? parts[3] : null;

        var requirement = new PrivilegeRequirement(action, subject, qualifier);

        // Create a new AuthorizationPolicy with the PrivilegeRequirement
        var policy = new AuthorizationPolicy([requirement], []);

        // Cache the result for future requests
        PolicyCache.TryAdd(policyName, policy);

        return policy;
    }
}
