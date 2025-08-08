namespace Privileged;

/// <summary>
/// Represents a context used to evaluate privilege rules for actions, subjects, and optional qualifiers.
/// This class provides the core functionality for authorization checks by evaluating rules and aliases
/// to determine whether specific actions are allowed or forbidden for given subjects and qualifiers.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="PrivilegeContext"/> class serves as the central authorization engine that evaluates
/// privilege rules to determine access permissions. It supports:
/// </para>
/// <list type="bullet">
/// <item><description>Rule-based authorization with allow and forbid permissions.</description></item>
/// <item><description>Alias expansion for actions, subjects, and qualifiers.</description></item>
/// <item><description>Wildcard matching using <see cref="PrivilegeActions.All"/> and <see cref="PrivilegeSubjects.All"/>.</description></item>
/// <item><description>Field-level permissions through qualifiers.</description></item>
/// <item><description>Customizable string comparison for rule matching.</description></item>
/// </list>
/// <para>
/// Rule evaluation follows a specific precedence order where forbid rules take precedence over allow rules,
/// and more specific rules override general wildcard rules.
/// </para>
/// </remarks>
/// <example>
/// The following example demonstrates creating and using a <see cref="PrivilegeContext"/>:
/// <code>
/// var rules = new[]
/// {
///     new PrivilegeRule { Action = "read", Subject = "Post" },
///     new PrivilegeRule { Action = "write", Subject = "Post", Qualifiers = new[] { "title", "content" } },
///     new PrivilegeRule { Action = "delete", Subject = "Post", Denied = true }
/// };
///
/// var model = new PrivilegeModel(rules);
/// var context = new PrivilegeContext(model);
///
/// bool canRead = context.Allowed("read", "Post");             // true
/// bool canWrite = context.Allowed("write", "Post", "title");  // true
/// bool canDelete = context.Allowed("delete", "Post");         // false
/// </code>
/// </example>
/// <seealso cref="IPrivilegeContext" />
/// <seealso cref="PrivilegeModel" />
/// <seealso cref="PrivilegeRule" />
/// <seealso cref="PrivilegeAlias" />
public class PrivilegeContext : IPrivilegeContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PrivilegeContext"/> class using a privilege model.
    /// </summary>
    /// <param name="model">The privilege model containing rules and aliases.</param>
    /// <param name="stringComparer">An optional string comparer for rule matching. Defaults to <see cref="StringComparer.InvariantCultureIgnoreCase"/>.</param>
    public PrivilegeContext(
        PrivilegeModel model,
        StringComparer? stringComparer = null)
        : this(model.Rules, model.Aliases, stringComparer)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PrivilegeContext"/> class using rules and aliases.
    /// </summary>
    /// <param name="rules">The collection of privilege rules.</param>
    /// <param name="aliases">The collection of privilege aliases. Optional.</param>
    /// <param name="stringComparer">An optional string comparer for rule matching. Defaults to <see cref="StringComparer.InvariantCultureIgnoreCase"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rules"/> is <c>null</c>.</exception>
    public PrivilegeContext(
        IReadOnlyCollection<PrivilegeRule> rules,
        IReadOnlyCollection<PrivilegeAlias>? aliases = null,
        StringComparer? stringComparer = null)
    {
        Rules = rules ?? throw new ArgumentNullException(nameof(rules));
        Aliases = aliases ?? [];
        StringComparer = stringComparer ?? StringComparer.InvariantCultureIgnoreCase;
    }

    /// <inheritdoc />
    public IReadOnlyCollection<PrivilegeRule> Rules { get; }

    /// <inheritdoc />
    public IReadOnlyCollection<PrivilegeAlias> Aliases { get; }

    /// <summary>
    /// Gets the <see cref="StringComparer"/> used to match actions, subjects, and qualifiers.
    /// </summary>
    /// <value>
    /// The string comparer instance used for all rule matching operations.
    /// Defaults to <see cref="StringComparer.InvariantCultureIgnoreCase"/> if not specified.
    /// </value>
    public StringComparer StringComparer { get; }

    /// <inheritdoc />
    /// <summary>
    /// Determines whether the specified action is allowed for the given subject and qualifier.
    /// </summary>
    /// <param name="action">The action to authorize (e.g., "read", "write").</param>
    /// <param name="subject">The subject to authorize (e.g., a resource or entity).</param>
    /// <param name="qualifier">An optional qualifier that further scopes the privilege (e.g., a field or scope).</param>
    /// <returns>
    /// <c>true</c> if the specified action is explicitly allowed for the subject and qualifier; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> or <paramref name="subject"/> is <c>null</c>.</exception>
    public bool Allowed(string? action, string? subject, string? qualifier = null)
    {
        if (action is null || subject is null)
            return false;

        var matchedRules = MatchRules(action, subject, qualifier);
        bool? state = null;

        foreach (var matchedRule in matchedRules)
        {
            if (matchedRule.Denied == true)
                state = state != null && (state.Value && false);
            else
                state = state == null || (state.Value || true);
        }

        return state ?? false;
    }

    /// <inheritdoc />
    /// <summary>
    /// Determines whether the specified action is forbidden for the given subject and qualifier.
    /// </summary>
    /// <param name="action">The action to authorize (e.g., "delete", "update").</param>
    /// <param name="subject">The subject to authorize (e.g., a resource or entity).</param>
    /// <param name="qualifier">An optional qualifier that further scopes the privilege (e.g., a field or scope).</param>
    /// <returns>
    /// <c>true</c> if the specified action is explicitly denied for the subject and qualifier; otherwise, <c>false</c>.
    /// </returns>
    public bool Forbidden(string? action, string? subject, string? qualifier = null) => !Allowed(action, subject, qualifier);

    /// <inheritdoc />
    /// <summary>
    /// Finds all privilege rules that match the specified action, subject, and optional qualifier.
    /// </summary>
    /// <param name="action">The action to match (e.g., "execute", "read").</param>
    /// <param name="subject">The subject to match (e.g., a resource or entity).</param>
    /// <param name="qualifier">An optional qualifier that further scopes the privilege (e.g., a field or scope).</param>
    /// <returns>
    /// A collection of <see cref="PrivilegeRule"/> instances that match the specified criteria.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> or <paramref name="subject"/> is <c>null</c>.</exception>
    public IEnumerable<PrivilegeRule> MatchRules(string? action, string? subject, string? qualifier = null)
    {
        if (action is null || subject is null)
            return [];

        return Rules.Where(r => RuleMatcher(r, action, subject, qualifier));
    }

    private bool RuleMatcher(PrivilegeRule rule, string action, string subject, string? qualifier = null)
    {
        return SubjectMatcher(rule, subject)
               && ActionMatcher(rule, action)
               && QualifierMatcher(rule, qualifier);
    }

    private bool SubjectMatcher(PrivilegeRule rule, string subject)
    {
        // can match global all or requested subject
        return StringComparer.Equals(rule.Subject, subject)
               || StringComparer.Equals(rule.Subject, PrivilegeSubjects.All)
               || AliasMatcher(rule.Subject, subject, PrivilegeMatch.Subject);
    }

    private bool ActionMatcher(PrivilegeRule rule, string action)
    {
        // can match global manage action or requested action
        return StringComparer.Equals(rule.Action, action)
               || StringComparer.Equals(rule.Action, PrivilegeActions.All)
               || AliasMatcher(rule.Action, action, PrivilegeMatch.Action);
    }

    private bool QualifierMatcher(PrivilegeRule rule, string? qualifier)
    {
        // if rule doesn't have qualifiers, all allowed
        if (qualifier == null || rule.Qualifiers == null || rule.Qualifiers.Count == 0)
            return true;

        // ensure rule matches qualifier
        return rule.Qualifiers.Contains(qualifier, StringComparer)
               || AliasMatcher(rule.Qualifiers, qualifier, PrivilegeMatch.Qualifier);
    }

    private bool AliasMatcher(string name, string value, PrivilegeMatch privilegeType)
    {
        if (Aliases == null || Aliases.Count == 0)
            return false;

        return Aliases
            .Any(a =>
                StringComparer.Equals(a.Alias, name)
                && a.Type == privilegeType
                && a.Values.Contains(value, StringComparer)
            );
    }

    private bool AliasMatcher(IEnumerable<string> names, string value, PrivilegeMatch privilegeType)
    {
        if (Aliases == null || Aliases.Count == 0)
            return false;

        return Aliases
            .Any(a =>
                names.Contains(a.Alias, StringComparer)
                && a.Type == privilegeType
                && a.Values.Contains(value, StringComparer)
            );
    }
}
