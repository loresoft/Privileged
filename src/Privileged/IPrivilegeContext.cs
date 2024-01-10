namespace Privileged;

/// <summary>
/// The privilege context definition used to check privileges
/// </summary>
public interface IPrivilegeContext
{
    /// <summary>
    /// Gets the privilege rules for this context.
    /// </summary>
    /// <value>
    /// The privilege rules for this context.
    /// </value>
    IReadOnlyCollection<PrivilegeRule> Rules { get; }

    /// <summary>
    /// Check if the specified <paramref name="action"/>, <paramref name="subject"/> and <paramref name="field"/> are allowed.
    /// </summary>
    /// <param name="action">The action to authorize.</param>
    /// <param name="subject">The subject to authorize.</param>
    /// <param name="field">The optional field to authorize.</param>
    /// <returns>true if the specified <paramref name="action"/>, <paramref name="subject"/> and <paramref name="field"/> are allowed; otherwise false</returns>
    bool Allowed(string? action, string? subject, string? field = null);

    /// <summary>
    /// Check if the specified <paramref name="action"/>, <paramref name="subject"/> and <paramref name="field"/> are forbidden.
    /// </summary>
    /// <param name="action">The action to authorize.</param>
    /// <param name="subject">The subject to authorize.</param>
    /// <param name="field">The optional field to authorize.</param>
    /// <returns>true if the specified <paramref name="action"/>, <paramref name="subject"/> and <paramref name="field"/> are forbidden; otherwise false</returns>
    bool Forbidden(string? action, string? subject, string? field = null);

    /// <summary>
    /// Find the rules for the specified <paramref name="action"/>, <paramref name="subject"/> and <paramref name="field"/>
    /// </summary>
    /// <param name="action">The action to match.</param>
    /// <param name="subject">The subject to match.</param>
    /// <param name="field">The optional field to match.</param>
    /// <returns>The rules for the specified <paramref name="action"/>, <paramref name="subject"/> and <paramref name="field"/></returns>
    IEnumerable<PrivilegeRule> MatchRules(string? action, string? subject, string? field = null);
}
