using System;
using System.Collections.Concurrent;
using System.Reflection;

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
/// <item><description>Wildcard matching using <see cref="PrivilegeRule.Any"/> and <see cref="PrivilegeRule.Any"/>.</description></item>
/// <item><description>Field-level permissions through qualifiers.</description></item>
/// <item><description>Customizable string comparison for rule matching.</description></item>
/// </list>
/// <para>
/// Rule evaluation follows a specific precedence order where forbid rules take precedence over allow rules,
/// and more specific rules override general wildcard rules.
/// </para>
/// </remarks>
/// <seealso cref="PrivilegeModel" />
/// <seealso cref="PrivilegeRule" />
/// <seealso cref="PrivilegeAlias" />
public class PrivilegeContext
{
    private readonly ConcurrentDictionary<string, bool> _allowedCache;
    private readonly ConcurrentDictionary<(string Alias, PrivilegeMatch Type), HashSet<string>> _aliasCache;

    /// <summary>
    /// Represents an empty privilege context with no rules or aliases.
    /// </summary>
    public static readonly PrivilegeContext Empty = new([], []);

    /// <summary>
    /// Represents a privilege context with all permissions allowed.
    /// </summary>
    public static readonly PrivilegeContext All = new([PrivilegeRule.AllowAny], []);


    /// <summary>
    /// Initializes a new instance of the <see cref="PrivilegeContext"/> class using a privilege model.
    /// </summary>
    /// <param name="model">The privilege model containing rules and aliases.</param>
    /// <param name="stringComparer">An optional string comparer for rule matching. Defaults to <see cref="StringComparer.InvariantCultureIgnoreCase"/>.</param>
    public PrivilegeContext(
        PrivilegeModel model,
        StringComparer? stringComparer = null)
    {
        ArgumentNullException.ThrowIfNull(model);

        Rules = model.Rules ?? [];
        Aliases = model.Aliases ?? [];
        StringComparer = stringComparer ?? StringComparer.InvariantCultureIgnoreCase;

        _allowedCache = new ConcurrentDictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        _aliasCache = new ConcurrentDictionary<(string Alias, PrivilegeMatch Type), HashSet<string>>();

        Initialize();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PrivilegeContext"/> class using rules and aliases.
    /// </summary>
    /// <param name="rules">The collection of privilege rules.</param>
    /// <param name="aliases">The collection of privilege aliases. Optional.</param>
    /// <param name="stringComparer">An optional string comparer for rule matching. Defaults to <see cref="StringComparer.InvariantCultureIgnoreCase"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rules"/> is <c>null</c>.</exception>
    public PrivilegeContext(
        IReadOnlyList<PrivilegeRule> rules,
        IReadOnlyList<PrivilegeAlias>? aliases = null,
        StringComparer? stringComparer = null)
    {
        ArgumentNullException.ThrowIfNull(rules);

        Rules = rules;
        Aliases = aliases ?? [];
        StringComparer = stringComparer ?? StringComparer.InvariantCultureIgnoreCase;

        _allowedCache = [];
        _aliasCache = [];

        Initialize();
    }

    /// <summary>
    /// Gets the collection of privilege rules defined for this context.
    /// </summary>
    /// <value>
    /// A read-only collection of <see cref="PrivilegeRule"/> instances representing the authorization rules.
    /// </value>
    public IReadOnlyList<PrivilegeRule> Rules { get; }

    /// <summary>
    /// Gets the collection of privilege aliases defined for this context.
    /// </summary>
    /// <value>
    /// A read-only collection of <see cref="PrivilegeAlias"/> instances used for rule aliasing or mapping.
    /// </value>
    public IReadOnlyList<PrivilegeAlias> Aliases { get; }

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

        var cacheKey = $"privilege:{action}:{subject}:{qualifier ?? string.Empty}";
        return _allowedCache.GetOrAdd(cacheKey, _ =>
        {
            // Perform privilege check
            var matchedRules = MatchRules(action, subject, qualifier);
            bool? state = null;

            for (int i = 0; i < matchedRules.Count; i++)
            {
                PrivilegeRule matchedRule = matchedRules[i];
                if (matchedRule.Denied == true)
                    state = state != null && (state.Value && false);
                else
                    state = state == null || (state.Value || true);
            }

            return state ?? false;
        });
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
    public bool Forbidden(string? action, string? subject, string? qualifier = null)
        => !Allowed(action, subject, qualifier);

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
    public IReadOnlyList<PrivilegeRule> MatchRules(string? action, string? subject, string? qualifier = null)
    {
        if (action is null || subject is null)
            return [];

        var matchedRules = new List<PrivilegeRule>();

        for (int r = 0; r < Rules.Count; r++)
        {
            if (!RuleMatcher(Rules[r], action, subject, qualifier))
                continue;

            matchedRules.Add(Rules[r]);
        }

        return matchedRules;
    }


    private void Initialize()
    {
        if (Aliases.Count == 0)
            return;

        // compute alias HashSets for faster lookups
        foreach (var alias in Aliases)
        {
            var key = (alias.Alias, alias.Type);
            if (!_aliasCache.ContainsKey(key))
                _aliasCache[key] = new HashSet<string>(alias.Values, StringComparer);
        }
    }

    private bool RuleMatcher(PrivilegeRule rule, string action, string subject, string? qualifier = null)
    {
        return SubjectMatcher(rule, subject)
               && ActionMatcher(rule, action)
               && QualifierMatcher(rule, qualifier);
    }

    private bool SubjectMatcher(PrivilegeRule rule, string subject)
    {
        // wildcard match optimization
        if (StringComparer.Equals(subject, PrivilegeRule.Any))
            return true;

        // Direct match optimization
        if (StringComparer.Equals(rule.Subject, subject))
            return true;

        // wildcard match optimization
        if (StringComparer.Equals(rule.Subject, PrivilegeRule.Any))
            return true;

        // Alias match
        return AliasMatcher(rule.Subject, subject, PrivilegeMatch.Subject);
    }

    private bool ActionMatcher(PrivilegeRule rule, string action)
    {
        // wildcard match optimization
        if (StringComparer.Equals(action, PrivilegeRule.Any))
            return true;

        // Direct match optimization
        if (StringComparer.Equals(rule.Action, action))
            return true;

        // wildcard match optimization
        if (StringComparer.Equals(rule.Action, PrivilegeRule.Any))
            return true;

        // Alias match
        return AliasMatcher(rule.Action, action, PrivilegeMatch.Action);
    }

    private bool QualifierMatcher(PrivilegeRule rule, string? qualifier)
    {
        var hasValue = !string.IsNullOrWhiteSpace(qualifier);
        var hasQualifiers = rule.Qualifiers?.Count > 0;

        // No qualifier specified in rule or request
        if (!hasValue && !hasQualifiers)
            return true;

        // Qualifier specified in request but not in rule
        if (hasValue && !hasQualifiers)
            return true;

        // wildcard match optimization
        if (StringComparer.Equals(qualifier, PrivilegeRule.Any))
            return true;

        // No qualifier specified in request but required by rule
        if (!hasValue && hasQualifiers)
            return false;

        // Direct match
        if (rule.Qualifiers?.Contains(qualifier, StringComparer) == true)
            return true;

        // Alias match
        return AliasMatcher(rule.Qualifiers, qualifier, PrivilegeMatch.Qualifier);
    }

    private bool AliasMatcher(IEnumerable<string>? names, string? value, PrivilegeMatch privilegeType)
    {
        if (_aliasCache.IsEmpty || names is null || value is null)
            return false;

        foreach (var name in names)
        {
            var key = (name, privilegeType);
            if (_aliasCache.TryGetValue(key, out var valueSet) && valueSet.Contains(value))
                return true;
        }

        return false;
    }

    private bool AliasMatcher(string? name, string? value, PrivilegeMatch privilegeType)
    {
        if (_aliasCache.Count == 0 || name is null || value is null)
            return false;

        var key = (name, privilegeType);
        return _aliasCache.TryGetValue(key, out var valueSet) && valueSet.Contains(value);
    }
}
