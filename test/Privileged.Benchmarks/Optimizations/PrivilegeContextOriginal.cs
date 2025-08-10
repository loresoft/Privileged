using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Privileged.Benchmarks.Optimizations;

public class PrivilegeContextOriginal
{
    public PrivilegeContextOriginal(
        PrivilegeModel model,
        StringComparer? stringComparer = null)
        : this(model.Rules, model.Aliases, stringComparer)
    {
    }

    public PrivilegeContextOriginal(
        IReadOnlyList<PrivilegeRule> rules,
        IReadOnlyList<PrivilegeAlias>? aliases = null,
        StringComparer? stringComparer = null)
    {
        Rules = rules ?? throw new ArgumentNullException(nameof(rules));
        Aliases = aliases ?? [];
        StringComparer = stringComparer ?? StringComparer.InvariantCultureIgnoreCase;
    }

    public IReadOnlyList<PrivilegeRule> Rules { get; }

    public IReadOnlyList<PrivilegeAlias> Aliases { get; }

    public StringComparer StringComparer { get; }

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

    public bool Forbidden(string? action, string? subject, string? qualifier = null) => !Allowed(action, subject, qualifier);

    public IReadOnlyList<PrivilegeRule> MatchRules(string? action, string? subject, string? qualifier = null)
    {
        if (action is null || subject is null)
            return [];

        return Rules.Where(r => RuleMatcher(r, action, subject, qualifier)).ToList();
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
