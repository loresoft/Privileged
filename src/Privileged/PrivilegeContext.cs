namespace Privileged;

/// <summary>
/// The privilege context used to check privileges
/// </summary>
/// <param name="rules">The privilege rules for this context</param>
/// <param name="stringComparer">The <see cref="StringComparer"/> used for matching names</param>
/// <seealso cref="Privileged.IPrivilegeContext" />
public class PrivilegeContext(IReadOnlyCollection<PrivilegeRule> rules, StringComparer? stringComparer = null) : IPrivilegeContext
{
    /// <inheritdoc />
    public IReadOnlyCollection<PrivilegeRule> Rules { get; } = rules ?? throw new ArgumentNullException(nameof(rules));

    /// <summary>
    /// Gets the <see cref="StringComparer"/> used for matching names.
    /// </summary>
    /// <value>
    /// The <see cref="StringComparer"/> used for matching names.
    /// </value>
    public StringComparer StringComparer { get; } = stringComparer ?? StringComparer.InvariantCultureIgnoreCase;


    /// <inheritdoc />
    public bool Allowed(string? action, string? subject, string? field = null)
    {
        if (action is null || subject is null)
            return false;

        var matchedRules = MatchRules(action, subject, field);
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
    public bool Forbidden(string? action, string? subject, string? field = null) => !Allowed(action, subject, field);

    /// <inheritdoc />
    public IEnumerable<PrivilegeRule> MatchRules(string? action, string? subject, string? field = null)
    {
        if (action is null || subject is null)
            return Enumerable.Empty<PrivilegeRule>();

        return Rules.Where(r => RuleMatcher(r, action, subject, field));
    }


    private bool RuleMatcher(PrivilegeRule rule, string action, string subject, string? field = null)
    {
        return SubjectMather(rule, subject)
               && ActionMather(rule, action)
               && FieldMatcher(rule, field);
    }

    private bool SubjectMather(PrivilegeRule rule, string subject)
    {
        // can match global all or requested subject
        return StringComparer.Equals(rule.Subject, subject)
               || StringComparer.Equals(rule.Subject, PrivilegeSubjects.All);
    }

    private bool ActionMather(PrivilegeRule rule, string action)
    {
        // can match global manage action or requested action
        return StringComparer.Equals(rule.Action, action)
               || StringComparer.Equals(rule.Action, PrivilegeActions.All);
    }

    private bool FieldMatcher(PrivilegeRule rule, string? field)
    {
        // if rule doesn't have fields, all allowed
        if (field == null || rule.Fields == null || rule.Fields.Count == 0)
            return true;

        // ensure rule has field
        return rule.Fields.Contains(field, StringComparer);
    }
}
