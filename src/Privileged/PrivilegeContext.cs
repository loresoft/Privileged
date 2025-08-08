namespace Privileged;

/// <summary>
/// Represents a context used to evaluate privilege rules for actions, subjects, and optional qualifiers.
/// This class provides the core functionality for authorization checks by evaluating rules and aliases
/// to determine whether specific actions are allowed or forbidden for given subjects and qualifiers.
/// </summary>
/// <param name="rules">The privilege rules to evaluate within this context.</param>
/// <param name="aliases">Optional alias mappings used to match equivalent privilege names.</param>
/// <param name="stringComparer">The <see cref="StringComparer"/> used to match privilege names; defaults to <see cref="StringComparer.InvariantCultureIgnoreCase"/>.</param>
/// <remarks>
/// <para>
/// The <see cref="PrivilegeContext"/> class serves as the central authorization engine that evaluates
/// privilege rules to determine access permissions. It supports:
/// </para>
/// <list type="bullet">
/// <item><description>Rule-based authorization with allow and forbid permissions</description></item>
/// <item><description>Alias expansion for actions, subjects, and qualifiers</description></item>
/// <item><description>Wildcard matching using <see cref="PrivilegeActions.All"/> and <see cref="PrivilegeSubjects.All"/></description></item>
/// <item><description>Field-level permissions through qualifiers</description></item>
/// <item><description>Customizable string comparison for rule matching</description></item>
/// </list>
/// <para>
/// Rule evaluation follows a specific precedence order where forbid rules take precedence over allow rules,
/// and more specific rules override general wildcard rules.
/// </para>
/// </remarks>
/// <example>
/// The following example demonstrates creating and using a PrivilegeContext:
/// <code>
/// var rules = new[]
/// {
///     new PrivilegeRule("read", "Post"),
///     new PrivilegeRule("write", "Post", new[] { "title", "content" }),
///     new PrivilegeRule("delete", "Post", Denied: true)
/// };
/// 
/// var context = new PrivilegeContext(rules);
/// 
/// bool canRead = context.Allowed("read", "Post");           // true
/// bool canWrite = context.Allowed("write", "Post", "title"); // true
/// bool canDelete = context.Allowed("delete", "Post");       // false
/// </code>
/// </example>
/// <seealso cref="IPrivilegeContext" />
/// <seealso cref="PrivilegeRule" />
/// <seealso cref="PrivilegeAlias" />
public class PrivilegeContext(
    IReadOnlyCollection<PrivilegeRule> rules,
    IReadOnlyCollection<PrivilegeAlias>? aliases = null,
    StringComparer? stringComparer = null)
    : IPrivilegeContext
{
    /// <inheritdoc />
    public IReadOnlyCollection<PrivilegeRule> Rules { get; } = rules ?? throw new ArgumentNullException(nameof(rules));

    /// <inheritdoc />
    public IReadOnlyCollection<PrivilegeAlias> Aliases { get; } = aliases ?? [];

    /// <summary>
    /// Gets the <see cref="StringComparer"/> used to match actions, subjects, and qualifiers.
    /// </summary>
    /// <value>
    /// The string comparer instance used for all rule matching operations.
    /// Defaults to <see cref="StringComparer.InvariantCultureIgnoreCase"/> if not specified.
    /// </value>
    /// <remarks>
    /// This comparer affects how privilege names are matched during rule evaluation.
    /// Using a case-insensitive comparer (the default) allows for more flexible rule definitions
    /// where "Read" and "read" are treated as equivalent.
    /// </remarks>
    public StringComparer StringComparer { get; } = stringComparer ?? StringComparer.InvariantCultureIgnoreCase;


    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> or <paramref name="subject"/> is null.</exception>
    /// <remarks>
    /// <para>
    /// This method evaluates all matching rules and applies the following logic:
    /// </para>
    /// <list type="bullet">
    /// <item><description>If no rules match, returns <c>false</c></description></item>
    /// <item><description>Forbid rules (where <see cref="PrivilegeRule.Denied"/> is <c>true</c>) take precedence over allow rules</description></item>
    /// <item><description>Rules are evaluated in the order they appear in the <see cref="Rules"/> collection</description></item>
    /// <item><description>Wildcard rules using <see cref="PrivilegeActions.All"/> and <see cref="PrivilegeSubjects.All"/> match any value</description></item>
    /// <item><description>Alias expansion is performed automatically for actions, subjects, and qualifiers</description></item>
    /// </list>
    /// </remarks>
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
    /// <remarks>
    /// This method is equivalent to calling <c>!Allowed(action, subject, qualifier)</c>.
    /// It provides a convenient way to check if an action is explicitly denied or not allowed.
    /// </remarks>
    public bool Forbidden(string? action, string? subject, string? qualifier = null) => !Allowed(action, subject, qualifier);

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> or <paramref name="subject"/> is null.</exception>
    /// <remarks>
    /// <para>
    /// This method returns all rules that match the specified criteria, including:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Exact matches for action and subject</description></item>
    /// <item><description>Wildcard matches using <see cref="PrivilegeActions.All"/> and <see cref="PrivilegeSubjects.All"/></description></item>
    /// <item><description>Alias-based matches where aliases expand to include the requested values</description></item>
    /// <item><description>Qualifier matches (if qualifier is provided and rule has qualifiers)</description></item>
    /// </list>
    /// <para>
    /// The returned rules maintain their original order from the <see cref="Rules"/> collection.
    /// This method is useful for debugging rule evaluation or implementing custom authorization logic.
    /// </para>
    /// </remarks>
    public IEnumerable<PrivilegeRule> MatchRules(string? action, string? subject, string? qualifier = null)
    {
        if (action is null || subject is null)
            return [];

        return Rules.Where(r => RuleMatcher(r, action, subject, qualifier));
    }

    /// <summary>
    /// Determines whether the specified rule matches the given action, subject, and qualifier.
    /// </summary>
    /// <param name="rule">The privilege rule to evaluate.</param>
    /// <param name="action">The action to match against the rule.</param>
    /// <param name="subject">The subject to match against the rule.</param>
    /// <param name="qualifier">The optional qualifier to match against the rule.</param>
    /// <returns>
    /// <c>true</c> if the rule matches all specified criteria; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// A rule matches when all three components (subject, action, and qualifier) match their respective
    /// criteria in the rule. Each component is evaluated using dedicated matcher methods.
    /// </remarks>
    private bool RuleMatcher(PrivilegeRule rule, string action, string subject, string? qualifier = null)
    {
        return SubjectMatcher(rule, subject)
               && ActionMatcher(rule, action)
               && QualifierMatcher(rule, qualifier);
    }

    /// <summary>
    /// Determines whether the rule's subject matches the requested subject.
    /// </summary>
    /// <param name="rule">The privilege rule containing the subject to match.</param>
    /// <param name="subject">The requested subject to match against.</param>
    /// <returns>
    /// <c>true</c> if the rule's subject matches the requested subject; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Subject matching supports:
    /// <list type="bullet">
    /// <item><description>Exact string matches using the configured <see cref="StringComparer"/></description></item>
    /// <item><description>Wildcard matching when the rule subject is <see cref="PrivilegeSubjects.All"/></description></item>
    /// <item><description>Alias expansion where subject aliases can match multiple actual subjects</description></item>
    /// </list>
    /// </remarks>
    private bool SubjectMatcher(PrivilegeRule rule, string subject)
    {
        // can match global all or requested subject
        return StringComparer.Equals(rule.Subject, subject)
               || StringComparer.Equals(rule.Subject, PrivilegeSubjects.All)
               || AliasMatcher(rule.Subject, subject, PrivilegeMatch.Subject);
    }

    /// <summary>
    /// Determines whether the rule's action matches the requested action.
    /// </summary>
    /// <param name="rule">The privilege rule containing the action to match.</param>
    /// <param name="action">The requested action to match against.</param>
    /// <returns>
    /// <c>true</c> if the rule's action matches the requested action; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Action matching supports:
    /// <list type="bullet">
    /// <item><description>Exact string matches using the configured <see cref="StringComparer"/></description></item>
    /// <item><description>Wildcard matching when the rule action is <see cref="PrivilegeActions.All"/></description></item>
    /// <item><description>Alias expansion where action aliases can match multiple actual actions</description></item>
    /// </list>
    /// </remarks>
    private bool ActionMatcher(PrivilegeRule rule, string action)
    {
        // can match global manage action or requested action
        return StringComparer.Equals(rule.Action, action)
               || StringComparer.Equals(rule.Action, PrivilegeActions.All)
               || AliasMatcher(rule.Action, action, PrivilegeMatch.Action);
    }

    /// <summary>
    /// Determines whether the rule's qualifiers match the requested qualifier.
    /// </summary>
    /// <param name="rule">The privilege rule containing the qualifiers to match.</param>
    /// <param name="qualifier">The requested qualifier to match against.</param>
    /// <returns>
    /// <c>true</c> if the rule's qualifiers match the requested qualifier; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Qualifier matching follows these rules:
    /// <list type="bullet">
    /// <item><description>If no qualifier is requested or the rule has no qualifiers, returns <c>true</c></description></item>
    /// <item><description>If both qualifier and rule qualifiers exist, checks for exact matches using the configured <see cref="StringComparer"/></description></item>
    /// <item><description>Supports alias expansion where qualifier aliases can match multiple actual qualifiers</description></item>
    /// </list>
    /// </remarks>
    private bool QualifierMatcher(PrivilegeRule rule, string? qualifier)
    {
        // if rule doesn't have qualifiers, all allowed
        if (qualifier == null || rule.Qualifiers == null || rule.Qualifiers.Count == 0)
            return true;

        // ensure rule matches qualifier
        return rule.Qualifiers.Contains(qualifier, StringComparer)
               || AliasMatcher(rule.Qualifiers, qualifier, PrivilegeMatch.Qualifier);
    }

    /// <summary>
    /// Determines whether an alias matches the specified name and value for the given privilege type.
    /// </summary>
    /// <param name="name">The alias name to search for.</param>
    /// <param name="value">The value to check against the alias's expanded values.</param>
    /// <param name="privilegeType">The type of privilege match (Action, Subject, or Qualifier).</param>
    /// <returns>
    /// <c>true</c> if an alias is found that matches the name and contains the value; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method searches through the <see cref="Aliases"/> collection to find an alias that:
    /// <list type="bullet">
    /// <item><description>Has an alias name that matches the specified <paramref name="name"/></description></item>
    /// <item><description>Is of the correct <paramref name="privilegeType"/></description></item>
    /// <item><description>Contains the specified <paramref name="value"/> in its values collection</description></item>
    /// </list>
    /// </remarks>
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

    /// <summary>
    /// Determines whether any alias in the collection of names matches the specified value for the given privilege type.
    /// </summary>
    /// <param name="names">The collection of alias names to search for.</param>
    /// <param name="value">The value to check against the aliases' expanded values.</param>
    /// <param name="privilegeType">The type of privilege match (Action, Subject, or Qualifier).</param>
    /// <returns>
    /// <c>true</c> if any alias is found that matches one of the names and contains the value; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method searches through the <see cref="Aliases"/> collection to find an alias that:
    /// <list type="bullet">
    /// <item><description>Has an alias name that matches any of the specified <paramref name="names"/></description></item>
    /// <item><description>Is of the correct <paramref name="privilegeType"/></description></item>
    /// <item><description>Contains the specified <paramref name="value"/> in its values collection</description></item>
    /// </list>
    /// This overload is primarily used for qualifier matching where a rule may contain multiple qualifier names.
    /// </remarks>
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
