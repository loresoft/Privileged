namespace Privileged;

/// <summary>
/// Represents a context that encapsulates privilege rules and logic for authorization checks.
/// </summary>
public interface IPrivilegeContext
{
    /// <summary>
    /// Gets the collection of privilege rules defined for this context.
    /// </summary>
    /// <value>
    /// A read-only collection of <see cref="PrivilegeRule"/> instances representing the authorization rules.
    /// </value>
    IReadOnlyList<PrivilegeRule> Rules { get; }

    /// <summary>
    /// Gets the collection of privilege aliases defined for this context.
    /// </summary>
    /// <value>
    /// A read-only collection of <see cref="PrivilegeAlias"/> instances used for rule aliasing or mapping.
    /// </value>
    IReadOnlyList<PrivilegeAlias> Aliases { get; }

    /// <summary>
    /// Determines whether the specified action is allowed for the given subject and qualifier.
    /// </summary>
    /// <param name="action">The action to authorize (e.g., "read", "write").</param>
    /// <param name="subject">The subject to authorize (e.g., a resource or entity).</param>
    /// <param name="qualifier">An optional qualifier that further scopes the privilege (e.g., a field or scope).</param>
    /// <returns>
    /// <c>true</c> if the specified action is explicitly allowed for the subject and qualifier; otherwise, <c>false</c>.
    /// </returns>
    bool Allowed(string? action, string? subject, string? qualifier = null);

    /// <summary>
    /// Determines whether the specified action is forbidden for the given subject and qualifier.
    /// </summary>
    /// <param name="action">The action to authorize (e.g., "delete", "update").</param>
    /// <param name="subject">The subject to authorize (e.g., a resource or entity).</param>
    /// <param name="qualifier">An optional qualifier that further scopes the privilege (e.g., a field or scope).</param>
    /// <returns>
    /// <c>true</c> if the specified action is explicitly denied for the subject and qualifier; otherwise, <c>false</c>.
    /// </returns>
    bool Forbidden(string? action, string? subject, string? qualifier = null);

    /// <summary>
    /// Finds all privilege rules that match the specified action, subject and optional qualifier.
    /// </summary>
    /// <param name="action">The action to match (e.g., "execute", "read").</param>
    /// <param name="subject">The subject to match (e.g., a resource or entity).</param>
    /// <param name="qualifier">An optional qualifier that further scopes the privilege (e.g., a field or scope).</param>
    /// <returns>
    /// A collection of <see cref="PrivilegeRule"/> instances that match the specified criteria.
    /// </returns>
    IReadOnlyList<PrivilegeRule> MatchRules(string? action, string? subject, string? qualifier = null);
}

