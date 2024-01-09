namespace Privileged;

/// <summary>
/// The authorization context definition used to check privileges
/// </summary>
public interface IAuthorizationContext
{
    /// <summary>
    /// Gets the authorization rules for this context.
    /// </summary>
    /// <value>
    /// The authorization rules for this context.
    /// </value>
    IReadOnlyCollection<AuthorizationRule> Rules { get; }

    /// <summary>
    /// Check if the specified <paramref name="action"/>, <paramref name="subject"/> and <paramref name="field"/> are authorized.
    /// </summary>
    /// <param name="action">The action to authorize.</param>
    /// <param name="subject">The subject to authorize.</param>
    /// <param name="field">The optional field to authorize.</param>
    /// <returns>true if the specified <paramref name="action"/>, <paramref name="subject"/> and <paramref name="field"/> are authorized; otherwise false</returns>
    bool Authorized(string? action, string? subject, string? field = null);

    /// <summary>
    /// Check if the specified <paramref name="action"/>, <paramref name="subject"/> and <paramref name="field"/> are unauthorized.
    /// </summary>
    /// <param name="action">The action to authorize.</param>
    /// <param name="subject">The subject to authorize.</param>
    /// <param name="field">The optional field to authorize.</param>
    /// <returns>true if the specified <paramref name="action"/>, <paramref name="subject"/> and <paramref name="field"/> are unauthorized; otherwise false</returns>
    bool Unauthorized(string? action, string? subject, string? field = null);

    /// <summary>
    /// Find the rules for the specified <paramref name="action"/>, <paramref name="subject"/> and <paramref name="field"/>
    /// </summary>
    /// <param name="action">The action to match.</param>
    /// <param name="subject">The subject to match.</param>
    /// <param name="field">The optional field to match.</param>
    /// <returns>The rules for the specified <paramref name="action"/>, <paramref name="subject"/> and <paramref name="field"/></returns>
    IEnumerable<AuthorizationRule> MatchRules(string? action, string? subject, string? field = null);
}
